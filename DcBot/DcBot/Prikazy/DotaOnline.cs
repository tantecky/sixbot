using System.Text;
using System;
using BnetClient;
using System.Net;
using System.Threading;

namespace DcBot
{
    [PomocAtribut("zobrazí hráče doty aktuálně přítomné na DC, čekající a probíhající hry Doty")]
    [AliasAtribut("do")]
    class DotaOnline : BasePrikaz
    {
        private readonly Client m_Client;
        private SixBot m_Bot;
        private readonly IPEndPoint m_IP = new IPEndPoint(IPAddress.Parse("147.33.227.122"), 6112);

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal DotaOnline(SixBot bot)
        {
            m_Bot = bot;

            m_Client = new Client();
            //m_Client.OnlineUdalost += new ClientUdalostCallback(m_Client_OnlineUdalost);
            m_Client.OfflineUdalost += new ClientUdalostCallback(m_Client_OfflineUdalost);
            m_Client.VstupDoChatuUdalost += new ClientUdalostCallback(m_Client_VstupDoChatuUdalost);
            m_Client.ChybaUdalost += new ChybaUdalostCallback(m_Client_ChybaUdalost);

            m_Client.NavazSpojeni(m_IP);
        }

        void m_Client_ChybaUdalost(string chyba)
        {
            m_Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "BnetClient chyba: " + chyba));
        }

        void m_Client_VstupDoChatuUdalost()
        {
            m_Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "BnetClient vstoupil do bnet chatu " + m_IP.Address.ToString()));
        }

        void m_Client_OfflineUdalost()
        {
            m_Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "BnetClient offline"));

            if (!Disposed)
            {
                Thread.Sleep(60000);
                m_Client.NavazSpojeni(m_IP);
            }
        }

        private void m_Client_OnlineUdalost()
        {
            m_Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "Bnet klient online na bnetu " + m_IP.Address.ToString()));
        }

        public override void Dispose()
        {
            if (Disposed)
                return;

            base.Dispose();

            m_Client.UkonciSpojeni();
        }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            if(SpamDota.Rozesila)
                bot.PrivateZprava(odesilatel, "Právě se rozesílá Spam dota, chvilku strpení...");

            StringBuilder sb = new StringBuilder(DotaRoster.Hraci.Count / 2);
            ushort pocet = 0;

            lock (DotaRoster.Hraci)
            {
                if (DotaRoster.Hraci.Count == 0)
                {
                    bot.PrivateZprava(odesilatel, "Dotaroster obsahuje příliš málo hráčů");
                    return;
                }

                sb.AppendLine("Hráči:");

                foreach (string hrac in DotaRoster.Hraci)
                {
                    if (bot.Hub.Userlist.ContainsKey(hrac))
                    {
                        pocet++;
                        sb.AppendLine(hrac);
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("Online: " + pocet);
            sb.AppendLine();

            m_Client.Hry.ZamkniList();

            BnetInfo[] hry = m_Client.Hry.ZiskejHry();

            if (hry != null)
            {
                sb.AppendLine("Čekající hry:");

                foreach (BnetInfo hra in hry)
                    sb.AppendLine(hra.ToString());

                sb.AppendLine();
            }

            m_Client.Hry.OdemkniList();

            string cekajiciHry = m_Client.CekajiciHry.ZiskejHry();

            if (cekajiciHry != null)
            {
                sb.AppendLine("Probíhající hry:");
                sb.AppendLine(cekajiciHry);
            }

            bot.PrivateZprava(odesilatel, sb.ToString());
        }
    }
}