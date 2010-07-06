using System;
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;
using FlowLib.Events;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using FlowLib.Enums;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;
using System.Windows.Forms;

namespace DcBot
{
    internal class SixBot : IBaseUpdater, IDisposable
    {
        private static readonly WaitCallback m_ThreadPoolCallback = new WaitCallback(HandlerPrikazu.PouzijPrikaz);

        private Hub m_Hub;
        private Okno m_Gui;
        private TransferManager m_TransferManager;
        private DownloadManager m_DownloadManager;
        private Share m_Share;
        private string m_Hostname;

        internal Hub Hub
        {
            get
            {
                return m_Hub;
            }
        }

        internal Okno Gui
        {
            get
            {
                return m_Gui;
            }
        }


        #region IBaseUpdater Members
        public event FmdcEventHandler UpdateBase;
        #endregion

        private SixBot(Okno gui) 
        {
            gui.Bot = this;

            m_Gui = gui;
            m_Hostname = "sazavahub.vscht.cz";

            CekejNaPripojeni();

            m_TransferManager = new TransferManager();
            m_DownloadManager = new DownloadManager();

            m_Share = new Share("Share");
            //m_Share.ErrorOccured += new FmdcEventHandler(m_Share_ErrorOccured);
            //m_Share.HashAllowDuplicate = false;
            //m_Share.HashAutoSaveCount = 1;
            //m_Share.AddVirtualDir("Test", AppDomain.CurrentDomain.BaseDirectory+"\\Test");
            //m_Share.AddVirtualDir("Test2", @"F:\Hry\EasyUO");
            //General.AddCommonFilelistsToShare(m_Share, AppDomain.CurrentDomain.BaseDirectory);

           /* m_Share.HashContent();

            m_Gui.VypisRadek(m_Share.TotalCount.ToString());
            m_Gui.VypisRadek(m_Share.IsHashing.ToString());
            m_Gui.VypisRadek(m_Share.HashedCount.ToString());*/

            HubSetting nastaveni = new HubSetting();
            nastaveni.Address = m_Hostname;
            nastaveni.Port = 411;

#if DEBUG
            nastaveni.DisplayName = "Sixbot_Test";
#else
            nastaveni.DisplayName = "Sixbot";
#endif

            nastaveni.Protocol = "Nmdc";
            nastaveni.Password = "botka";

            m_Hub = new Hub(nastaveni, this);
            m_Hub.Share = m_Share;
            m_Hub.Me.TagInfo.Version = "SixBot";
            m_Hub.Me.Description = "http://sixbot.googlecode.com";
            m_Hub.Me.Mode = ConnectionTypes.Direct;

            m_Hub.ConnectionStatusChange += new FmdcEventHandler(m_Hub_ConnectionStatusChange);

            HandlerPrikazu.Inicialiazce(this);

            m_Hub.Connect();

            m_Hub.Protocol.Encoding = Encoding.Default; //je to nutné od verze 4
        }

        internal static void VytvorInstanciBota(object gui) //tohle se volá pro spuštìní bota volá se z jiného vlákna aby to nic neblokovalo
        {
            new SixBot((Okno)gui);
        }

        private void CekejNaPripojeni() //fakt celkem praso styl ale nic lepšího mì nenpadlo :(
        {
            m_Gui.VypisText(string.Format("Zjišuji dostupnost {0}...", m_Hostname));

            while (true)
            {
                try
                {
                    Dns.GetHostEntry(m_Hostname);
                    break;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            }

            m_Gui.VypisRadek("hotovo!");
        }

        internal void PrivateZprava(string uzivatel, string text)
        {
            try
            {
                User uz = m_Hub.Userlist[uzivatel];
                UpdateBase(this, new FmdcEventArgs(Actions.PrivateMessage, new PrivateMessage(uz.ID, m_Hub.Me.ID, text)));
            }
            catch (KeyNotFoundException)
            {
            }
        }

        internal void ChatZprava(string text)
        {
#if !DEBUG
            UpdateBase(this, new FmdcEventArgs(Actions.MainMessage, new MainMessage(m_Hub.Me.ID, text)));
#else
            MessageBox.Show(text);
#endif
        }

        private void m_Hub_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case 0: //connecting
                    {
                        m_Gui.VypisText(string.Format("Pøipojuji se k {0}...", m_Hub.HubSetting.Address));
                        break;
                    }
                case 1: //connected
                    {
                        m_Hub.Protocol.Update += new FmdcEventHandler(Protocol_Update);
                        m_Gui.VypisRadek("hotovo!");
                        break;
                    }
                case 2: //disconnected
                    {
                        m_Hub.Protocol.Update -= new FmdcEventHandler(Protocol_Update);
                        m_Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "Odpojen!"));

                        break;
                    }
            }
        }

        private void Protocol_Update(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.MainMessage:
                    {
                        MainMessage zprava = e.Data as MainMessage;

                        if (zprava.Content.StartsWith(HandlerPrikazu.PrikazPrefix) && zprava.Content.Length > 1)
                        {
                            ThreadPool.QueueUserWorkItem(m_ThreadPoolCallback, new ArgumentyHandleru(zprava.Content.Remove(0, 1).Trim(), zprava.From, this)); 
                        }

#if DEBUG
                        if (zprava.From == "AdminChar" || zprava.From == "VerliHub")
                            m_Gui.VypisRadek(zprava.Content);
#endif

                        break;
                    }
                case Actions.PrivateMessage:
                    {
                        PrivateMessage zprava = e.Data as PrivateMessage;

                        if (zprava.From == m_Hub.HubSetting.DisplayName) //tohle je od nové verze 4 FlowLibu nutné protože on si posílá jakoby vlastní PMka
                            break;

                        string text = zprava.Content.Replace(string.Format("<{0}> ", zprava.From), "");

                        if (text.StartsWith(HandlerPrikazu.PrikazPrefix) && text.Length > 1)
                        {
                            ThreadPool.QueueUserWorkItem(m_ThreadPoolCallback, new ArgumentyHandleru(text.Remove(0, 1).Trim(), zprava.From, this)); 
                        }
                        else if(zprava.From != "AdminChar" && zprava.From != "VerliHub")
                            PrivateZprava(zprava.From, string.Concat("Jsem jen tupý bot. Pro seznam pøíkazù napiš " , HandlerPrikazu.PrikazPrefix, "help"));

#if DEBUG
                        if(zprava.From == "AdminChar" || zprava.From == "VerliHub")
                            m_Gui.VypisRadek(text);
#endif

                        break;
                    }
                case Actions.SearchResult:
                case Actions.Search:
                    {
                        m_Gui.VypisRadek("SEARCH");
                        if (e.Data is SearchInfo)
                        {
                            SearchInfo info = e.Data as SearchInfo;
                        }

                        if (e.Data is SearchResultInfo)
                        {
                            SearchResultInfo info = e.Data as SearchResultInfo;
                        }

                        break;
                    }
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        //m_Gui.VypisRadek(trans.RemoteAddress.Address.ToString());
                        m_TransferManager.StartTransfer(trans);
                    }
                    break;

            }
        }

        public void Dispose()
        {
            foreach (BasePrikaz prikaz in HandlerPrikazu.Prikazy.Values)
                prikaz.Dispose();

            if(m_Hub != null)
                m_Hub.Dispose();
        }
    }
}
