using System.Collections.Generic;
using DcBot;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using FindCS.Protocols;
using FindCS.Networking;
using FindCS;

namespace DcBot
{
    [PomocAtribut("detekuje zda nebìží herní server (CS, CSS, DoD, L4D, COD:UO, UT2004) na ip adresách daných v seznamu")]
    [AliasAtribut("cs")]
    class FindCs : BasePrikaz
    {
        private const byte m_Oddelovac = 0x00;
        private static readonly byte[] m_Dotaz = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54 };
        private static readonly byte[] m_Odpoved = { 0xFF, 0xFF, 0xFF, 0xFF, 0x6D };

        private static List<IPAddress> m_CsServery;

        internal static List<IPAddress> CsServery
        {
            get
            {
                return m_CsServery;
            }
        }

        private StringBuilder m_Hlaska;
        private int m_OtestovanoIP;
        private AutoResetEvent m_Stopka;
        private AsyncCallback m_AsCallback;
        private SixBot m_Bot;
        private TimerCallback m_TimerekCallback;
        private Stopwatch m_MereniCasu;
        private Timer m_Aktualizace;
        private bool m_Ulozit;

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal FindCs(SixBot bot)
        {
            m_Bot = bot;

            m_CsServery = new List<IPAddress>();
            NactiCsServery();

            m_Hlaska = new StringBuilder(2);
            m_AsCallback = new AsyncCallback(ReceiveCallback);
            m_TimerekCallback = new TimerCallback(PriUplynutiDoby);
            m_MereniCasu = new Stopwatch();
            m_Stopka = new AutoResetEvent(false);
            m_Ulozit = false;

            m_Aktualizace = new Timer(m_Aktualizace_Tick, null, TimeSpan.FromSeconds(5.0), TimeSpan.FromHours(12.0));
        }

        private void m_Aktualizace_Tick(object sender)
        {
#if !DEBUG
            lock (CsServery)
            {
                VyaktualizujSeznam();
            }
#endif
        }

        private void VyaktualizujSeznam()
        {
            WebResponse odezva = null;

            try
            {
                WebRequest pozadavek = WebRequest.Create("http://www.findcs.wz.cz/ServerList.xml");

                odezva = pozadavek.GetResponse();

                XmlDocument doc = new XmlDocument();

                using (Stream proud = odezva.GetResponseStream())
                {
                    doc.Load(proud);
                }

                XmlNodeList xml = doc["ServerList"].SelectNodes("server");

                IPAddress[] webovyList = new IPAddress[xml.Count];

                for (int i = 0; i < webovyList.Length; i++)
                {
                    webovyList[i] = IPAddress.Parse(xml[i].Attributes["IP"].Value);
                }

                //odebere servery které již nejsou ve webovem listu 
                for (int i = 0; i < m_CsServery.Count; i++)
                {
                    if (!ObsahujeIP(m_CsServery[i], webovyList))
                    {
                        m_CsServery.RemoveAt(i);
                        i--;

                        m_Ulozit = true;
                    }
                }

                //pøidá nové servery
                foreach (IPAddress ip in webovyList)
                {
                    if (!ObsahujeIP(ip, m_CsServery.ToArray()))
                    {
                        m_CsServery.Add(ip);

                        m_Ulozit = true;
                    }
                }

                UlozCsServery();
            }
            catch (WebException)
            {
                m_Hlaska.AppendLine("Nepodaøilo se stáhnout ServerList");
            }
            finally
            {
                if (odezva != null)
                    odezva.Close();
            }
        }

        private static bool ObsahujeIP(IPAddress ipko, IPAddress[] list)
        {
            foreach (IPAddress ip in list)
            {
                if (ip.Equals(ipko))
                    return true;
            }

            return false;
        }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            lock (CsServery)
            {
                if (m_CsServery.Count == 0)
                {
                    return;
                }

                m_MereniCasu.Reset();
                m_MereniCasu.Start();

                m_OtestovanoIP = 0;

                if (m_Hlaska.Length != 0)
                    m_Hlaska.Remove(0, m_Hlaska.Length);

                foreach (IPAddress ip in m_CsServery)
                {
                    foreach (StatProtocol protokol in StatProtocol.Protokoly)
                    {
                        UdpClient client = new UdpClient();
                        UdpObjekt obj = new UdpObjekt(client, ip, odesilatel, protokol);
                        client.Connect(ip, protokol.Port);
                        client.Send(protokol.Dotaz, protokol.Dotaz.Length);

                        try
                        {
                            client.BeginReceive(m_AsCallback, obj);
                            obj.Timerek = new Timer(m_TimerekCallback, obj, 100, Timeout.Infinite);
                        }
                        catch (SocketException /*ex*/)
                        {
                            // Stávající pøipojení bylo vynucenì ukonèeno vzdáleným hostitelem
                           // m_Bot.Gui.VypisRadek("CATCH: " + ex.ToString());

                            ZkontrolujStopku();

                            client.Close();

                            continue;
                        }
                    }
                }

                m_Stopka.WaitOne();

                m_MereniCasu.Stop();

                if (m_Hlaska.Length > 0)
                {
                    m_Hlaska.AppendLine(string.Format("Doba testu: {0} ms Otestováno: {1} IP adres Odesilatel: {2} (pohodlnìjší verze http://www.findcs.wz.cz/ )", (Math.Round((1000.0 * 1000.0 * 1000.0 * m_MereniCasu.ElapsedTicks / Stopwatch.Frequency) / 1000000.0, 3)).ToString(), (m_OtestovanoIP / StatProtocol.Protokoly.Length).ToString(), odesilatel));
                    m_Bot.ChatZprava(m_Hlaska.ToString());
                }
                else
                {
                    m_Hlaska.AppendLine(string.Format("Pøikaz -findcs nenašel žádný bìžící server Odesilatel: {0} (pohodlnìjší verze http://www.findcs.wz.cz/ )", odesilatel));
                    m_Hlaska.AppendLine(string.Format("Doba testu: {0} ms Otestováno: {1} IP adres", (Math.Round((1000.0 * 1000.0 * 1000.0 * m_MereniCasu.ElapsedTicks / Stopwatch.Frequency) / 1000000.0, 3)), (m_OtestovanoIP / StatProtocol.Protokoly.Length).ToString()));
                    m_Bot.ChatZprava(m_Hlaska.ToString());
                }
            }
        }

        private void PriUplynutiDoby(object o)
        {
            UdpObjekt obj = o as UdpObjekt;

            obj.Klient.Close();
            obj.Timerek.Dispose();

            //m_Bot.Gui.VypisRadek("TIMEOUT");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            UdpObjekt obj = ar.AsyncState as UdpObjekt;

            IPEndPoint ip = new IPEndPoint(obj.IP, obj.Protokol.Port);
            byte[] paketa;

            try
            {
                paketa = obj.Klient.EndReceive(ar, ref ip);
                obj.Timerek.Dispose();

                PacketReader reader = new PacketReader();
                reader.NactiPaketu(paketa);

                ServerInfo info = obj.Protokol.ZiskejInfo(reader);

                if (info != null)
                {
                    lock (m_Hlaska)
                        m_Hlaska.AppendLine(string.Format("Hra: {0} IP: {1} Mapa: {2} Poèet hráèù: {3} Poèet botù: {4}", info.Hra.ToString(), ip.Address.ToString(), info.Mapa, info.PocetHracu.ToString(), info.PocetBotu.ToString()));
                }

                reader.UvolniPaketu();
                obj.Klient.Close();
            }
            catch (NullReferenceException)
            {
                //ar mùže mùže být zøídka null nevím proè a je mi to jedno :)
            }
            catch (SocketException)
            {
                //m_Bot.Gui.VypisRadek("CATCH2");
            }
            catch (ObjectDisposedException) //vznikají když to zruším pøi timeoutu
            {
            }

            ZkontrolujStopku();
        }

        private void ZkontrolujStopku()
        {
            lock (m_Stopka)
            {
                m_OtestovanoIP++;

                if (m_OtestovanoIP == m_CsServery.Count * StatProtocol.Protokoly.Length)
                    m_Stopka.Set();
            }
        }

        private bool CsPaketa(byte[] paketa)
        {
            if (paketa.Length < 6)
                return false;

            for (byte i = 0; i < m_Odpoved.Length; i++)
            {
                if (paketa[i] != m_Odpoved[i])
                    return false;
            }

            return true;
        }

        private void ZapisPaketu(byte[] paketa)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream("packet.bin", FileMode.Append, FileAccess.Write)))
            {
                bw.Write(paketa);
            }
        }

        private void UlozCsServery()
        {
            if (!m_Ulozit)
                return;

            using (XmlTextWriter writer = new XmlTextWriter("ServerList.xml", Encoding.UTF8))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ServerList");

                foreach (IPAddress ip in m_CsServery)
                {
                    writer.WriteStartElement("server");
                    writer.WriteAttributeString("IP", ip.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            m_Ulozit = false;
        }

        private void NactiCsServery()
        {
            if (!File.Exists("ServerList.xml"))
            {
                m_Bot.Gui.VypisRadek("Nepodaøilo se najít ServerList.xml"); 
                return;
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load("ServerList.xml");
            }
            catch (XmlException)
            {
                m_Bot.Gui.VypisRadek("Nepodaøilo se naèíst ServerList.xml"); 
                return;
            }

            foreach (XmlNode uzel in doc["ServerList"].SelectNodes("server"))
            {
                m_CsServery.Add(IPAddress.Parse(uzel.Attributes["IP"].Value));
            }
        }

        public override void Dispose()
        {
            if (Disposed)
                return;

            base.Dispose();

            m_Stopka.Close();
            m_Aktualizace.Dispose();
        }
    }
}