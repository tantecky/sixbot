using DcBot;
using System.Threading;
using System.Collections.Generic;
using System;

namespace DcBot
{
    [PomocAtribut("po�le PMko lidem v dota rosteru, voliteln� argument n�zev hry nap�.: -spamdota hra 1")]
    [AliasAtribut("sd")]
    class SpamDota : BasePrikaz
    {
        private static readonly Random m_Nahoda = new Random();

        private static bool m_Rozesila = false;

        public static bool Rozesila
        {
            get
            {
                return m_Rozesila;
            }
        }

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Nepovinny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel, string[] argumenty)
        {
            if(m_Rozesila)
            {
                bot.PrivateZprava(odesilatel, "Dota spam se ji� rozes�l� n�k�m jin�m!");
                return;
            }

            m_Rozesila = true;

            List<string> list;

            lock (DotaRoster.Hraci)
            {
                if (DotaRoster.Hraci.Count < 2)
                {
                    bot.PrivateZprava(odesilatel, "Dotaroster obsahuje p��li� m�lo hr���");

                    m_Rozesila = false;
                    return;
                }
                else if (!DotaRoster.Hraci.Contains(odesilatel))
                {
                    bot.PrivateZprava(odesilatel, "Pouze hr��i kte�� jsou v dotarosteru m��ou pou��vat tento p��kaz. Pro p�id�n� napi� -addme");

                    m_Rozesila = false;
                    return;
                }

                list = new List<string>(DotaRoster.Hraci.Count);

                foreach (string hrac in DotaRoster.Hraci)
                    list.Add(hrac);
            }

            bot.PrivateZprava(odesilatel, "Rozes�l�m...");

            ushort online = 0;

            foreach (string hrac in list)
            {
                if (hrac != odesilatel && bot.Hub.Userlist.ContainsKey(hrac))
                {

                    switch (m_Nahoda.Next(3))
                    {
                        case 0:
                            {
                                if (argumenty == null)
                                    bot.PrivateZprava(hrac, string.Format("= DotA zalalo�ena come on! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= DotA zalalo�ena come on! N�zev hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                        case 1:
                            {
                                if (argumenty == null)
                                    bot.PrivateZprava(hrac, string.Format("= Byla zalo�ena DotA! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= Byla zalo�ena DotA! N�zev hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                        case 2:
                            {
                                if (argumenty == null)
                                    bot.PrivateZprava(hrac, string.Format("= Zalo�ena DotA! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= Zalo�ena DotA! N�zev hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                    }

                    online++;

                    Thread.Sleep(4000);
                }
            }

            bot.PrivateZprava(odesilatel, string.Format("Pozv�nka na Dotu odesl�na {0} online hr���m", online));

            online++; //i s t�m co odes�l�

            switch (m_Nahoda.Next(3))
            {
                case 0:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= DotA zalalo�ena come on! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= DotA zalalo�ena come on! N�zev hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
                case 1:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= Byla zalo�ena DotA! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= Byla zalo�ena DotA! N�zev hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
                case 2:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= Zalo�ena DotA! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= Zalo�ena DotA! N�zev hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
            }

            m_Rozesila = false;
        }
    }
}
