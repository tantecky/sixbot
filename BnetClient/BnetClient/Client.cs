using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace BnetClient
{
    #region Delegáti
    public delegate void ClientUdalostCallback();
    public delegate void PacketaUdalostCallback(PacketID packeta);
    public delegate void ChybaUdalostCallback(string chyba);
    #endregion

    public sealed class Client : IDisposable
    {
        #region Události
        public event ClientUdalostCallback OnlineUdalost;
        public event ClientUdalostCallback OfflineUdalost;
        public event ClientUdalostCallback VstupDoChatuUdalost;
        public event PacketaUdalostCallback PrijataPacketaUdalost;
        public event PacketaUdalostCallback OdeslanaPacketaUdalost;
        public event ChybaUdalostCallback ChybaUdalost;

        #endregion

        private static readonly TimeSpan m_Timeout = TimeSpan.FromSeconds(20.0); //čas po kterém dojde k odpojení při neaktivitě

        private Socket m_Socket;
        private DateTime m_PosledniAktivita;
        private Timer m_KontrolaAktivity;
        private TimerCallback m_ZkontrolujAktivitu;

        private byte[] m_ReadBuffer;
        private PacketBuffer m_PacketBuffer;
        private PacketReader m_Reader;
        private AsyncCallback m_PrectiData;

        private PacketWriter m_Writer;
        private AsyncCallback m_OdesliData;
        private AutoResetEvent m_RizeniOdesilani;

        private ushort m_DelkaPakety;
        private int m_PocatecniPozice;
        private int m_PocetPrijatychBytu;

        #region W3 záležitosti
        private Timer m_GameListTimer;
        private TimerCallback m_OdesliZadostGameList;
        private uint m_PingCookie;
        private BnetInfoList m_Hry;
        private StartedGamesList m_CekajiciHry;

        internal uint PingCookie
        {
            get
            {
                return m_PingCookie;
            }
            set
            {
                m_PingCookie = value;
            }
        }

        public BnetInfoList Hry
        {
            get
            {
                return m_Hry;
            }
        }

        public StartedGamesList CekajiciHry
        {
            get
            {
                return m_CekajiciHry;
            }
        }

        #endregion

        public Client()
        {
            m_DelkaPakety = 0;

            //kapacity musí být dělitelné 16 bez zbytku, neboli kapacita % 16 == 0
            m_ReadBuffer = new byte[1024];
            m_PacketBuffer = new PacketBuffer(2048);

            m_Reader = new PacketReader(m_PacketBuffer);
            m_PrectiData = new AsyncCallback(PrectiData);

            m_Writer = new PacketWriter(512);
            m_OdesliData = new AsyncCallback(OdesliData);
            m_RizeniOdesilani = new AutoResetEvent(true);

            Packety.InicializujPackety(m_Reader, m_Writer, this);

            m_ZkontrolujAktivitu = new TimerCallback(ZkontrolujAktivitu);

            m_Hry = new BnetInfoList(2);
            m_CekajiciHry = new StartedGamesList(2);
            m_OdesliZadostGameList = new TimerCallback(OdesliZadostGameList);
        }

        internal void OdesliZadostGameList(object o)
        {
            this.OdesliPacketu(PacketID.SID_GETADVLISTEX);
            this.OdesliPacketu(PacketID.SID_CHATCOMMAND);
        }

        internal void OdesliPacketu(PacketID id)
        {
            Packeta pac = Packety.ZiskejPacketu(id);

            if (pac == null)
            {
                ZalogujChybu("Neznámá packeta k odeslání: " + id.ToString());
                UkonciSpojeni();
                return;
            }

            if (OdeslanaPacketaUdalost != null)
                OdeslanaPacketaUdalost(id);

            m_RizeniOdesilani.WaitOne();

            try
            {
                pac.ZapisPacketu();
            }
            catch (IndexOutOfRangeException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();

                return;
            }

            try
            {
                m_Socket.BeginSend(m_Writer.Packeta, 0, m_Writer.Delka, SocketFlags.None, m_OdesliData, null);
            }
            catch (SocketException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void OdesliData(IAsyncResult ar)
        {
            try
            {
                m_Socket.EndSend(ar);
            }
            catch (SocketException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (ArgumentException)
            {
                return;
            }

            m_Writer.Vyprazdni();
            m_RizeniOdesilani.Set();
        }

        private void ZkontrolujAktivitu(object o)
        {
            if (!JeAktivni())
            {
                //ZalogujChybu("Timeout připojení");
                UkonciSpojeni();
            }
        }

        private void NastavAktvitu()
        {
            lock (this)
                m_PosledniAktivita = DateTime.Now;
        }

        private bool JeAktivni()
        {
            lock (this)
                return (DateTime.Now - m_PosledniAktivita) < m_Timeout;
        }

        private void PrectiData(IAsyncResult ar)
        {
            try
            {
                m_PocetPrijatychBytu = m_Socket.EndReceive(ar);
            }
            catch (SocketException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (ArgumentException)
            {
                return;
            }

            if (m_PocetPrijatychBytu > 0)
            {
                NastavAktvitu();

                if (!m_PacketBuffer.VlozData(m_ReadBuffer, m_PocetPrijatychBytu))
                {
                    ZalogujChybu("Přetekl PacketBuffer");
                    UkonciSpojeni();
                    return;
                }
            }
            else
            {
               // ZalogujChybu("Obdržena data o nulové délce");
               // UkonciSpojeni();
                return;
            }

            while (m_PacketBuffer.ObsahujeData)
            {
                switch (m_PacketBuffer.ZiskejPacketu(ref m_DelkaPakety, ref m_PocatecniPozice))
                {
                    case StavPackety.NeplatnaHlavicka:
                        {
                            ZalogujChybu("Neplatná hlavička (!= 0xFF)");
                            UkonciSpojeni();
                            return;
                        }
                    case StavPackety.PrilisKratkaPacketa:
                        {
                            ZalogujChybu("Příliš krátká packeta");
                            UkonciSpojeni();
                            return;
                        }
                    case StavPackety.NedostatekDat:
                        {
                            try
                            {
                                m_Socket.BeginReceive(m_ReadBuffer, 0, m_ReadBuffer.Length, SocketFlags.None, m_PrectiData, null);
                            }
                            catch (SocketException ex)
                            {
                                ZalogujChybu(ex.ToString());
                                UkonciSpojeni();
                            }

                            return;
                        }
                    case StavPackety.OK:
                        {
                            Packeta pc = Packety.ZiskejPacketu(m_PacketBuffer.Packeta[m_PocatecniPozice + 1]);

                            if (pc == null)
                            {
                                ZalogujChybu("Neznamá packeta ID: " + m_PacketBuffer.Packeta[m_PocatecniPozice + 1].ToString("X2"));
                                UkonciSpojeni();
                                return;
                            }

                            m_Reader.NactiPacketu(m_DelkaPakety, m_PocatecniPozice);

                            try
                            {
                                pc.PrectiPacketu();

                                if (PrijataPacketaUdalost != null)
                                    PrijataPacketaUdalost(pc.ID);

                            }
                            catch (Exception ex)
                            {
                                ZalogujChybu(ex.ToString());
                                UkonciSpojeni();

                                return;
                            }

                            break;
                        }
                }
            }

            try
            {
                m_Socket.BeginReceive(m_ReadBuffer, 0, m_ReadBuffer.Length, SocketFlags.None, m_PrectiData, null);
            }
            catch (SocketException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();
            }
        }

        private void ZalogujChybu(string chyba)
        {
            if (ChybaUdalost != null)
                ChybaUdalost(chyba);

            try
            {
                using (StreamWriter sw = new StreamWriter("BnetErrors.log", true))
                {
                    sw.WriteLine("##################################################################");
                    sw.WriteLine(DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString());
                    sw.WriteLine("Chyba:");
                    sw.WriteLine(chyba);
                    sw.WriteLine();

                    m_PacketBuffer.DumpniBuffer(sw);

                    sw.WriteLine();
                }
            }
            catch (IOException)
            {
            }
        }

        private void VytvorSocket()
        {
            try
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.LingerState.Enabled = false;
                m_Socket.NoDelay = true;
                m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            }
            catch (SocketException ex)
            {
                ZalogujChybu(ex.ToString());
                UkonciSpojeni();
            }
        }

        public void NavazSpojeni(IPEndPoint ip)
        {
            VytvorSocket();

            m_KontrolaAktivity = new Timer(m_ZkontrolujAktivitu, null, TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(9.0));
            
            try
            {
                m_Socket.Connect(ip);

                NastavAktvitu();

                if (OnlineUdalost != null)
                    OnlineUdalost();

                m_Socket.BeginReceive(m_ReadBuffer, 0, m_ReadBuffer.Length, SocketFlags.None, m_PrectiData, null);
                m_Socket.Send(new byte[] { 0x1 }); //game
            }
            catch (SocketException /*ex*/)
            {
               // ZalogujChybu(ex.ToString());
                UkonciSpojeni();

                return;
            }

            OdesliPacketu(PacketID.SID_AUTH_INFO);
        }

        public void UkonciSpojeni()
        {
            if (m_KontrolaAktivity != null)
            {
                m_KontrolaAktivity.Dispose();
                m_KontrolaAktivity = null;
            }

            if (m_GameListTimer != null)
            {
                m_GameListTimer.Dispose();
                m_GameListTimer = null;
            }

            if (m_Socket != null)
            {
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }

                m_Socket.Close();
            }

            m_RizeniOdesilani.Set();
            m_Hry.OdemkniList();

            m_PacketBuffer.Vyprazdni();
            m_Writer.Vyprazdni();

            if (OfflineUdalost != null)
                OfflineUdalost();
        }

        internal void PriVstupuDoChatu()
        {
            if (VstupDoChatuUdalost != null)
                VstupDoChatuUdalost();

            m_GameListTimer = new Timer(m_OdesliZadostGameList, null, TimeSpan.Zero, TimeSpan.FromSeconds(8.0));
        }

        public void Dispose()
        {
            UkonciSpojeni();

            m_RizeniOdesilani.Close();
            m_Hry.Dispose();
        }

    }
}
