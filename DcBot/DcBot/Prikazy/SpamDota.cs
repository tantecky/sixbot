using DcBot;
using System.Threading;
using System.Collections.Generic;
using System;

namespace DcBot
{
    [PomocAtribut("pošle PMko lidem v dota rosteru, volitelný argument název hry napø.: -spamdota hra 1")]
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
                bot.PrivateZprava(odesilatel, "Dota spam se již rozesílá nìkým jiným!");
                return;
            }

            m_Rozesila = true;

            List<string> list;

            lock (DotaRoster.Hraci)
            {
                if (DotaRoster.Hraci.Count < 2)
                {
                    bot.PrivateZprava(odesilatel, "Dotaroster obsahuje pøíliš málo hráèù");

                    m_Rozesila = false;
                    return;
                }
                else if (!DotaRoster.Hraci.Contains(odesilatel))
                {
                    bot.PrivateZprava(odesilatel, "Pouze hráèi kteøí jsou v dotarosteru mùžou používat tento pøíkaz. Pro pøidání napiš -addme");

                    m_Rozesila = false;
                    return;
                }

                list = new List<string>(DotaRoster.Hraci.Count);

                foreach (string hrac in DotaRoster.Hraci)
                    list.Add(hrac);
            }

            bot.PrivateZprava(odesilatel, "Rozesílám...");

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
                                    bot.PrivateZprava(hrac, string.Format("= DotA zalaložena come on! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= DotA zalaložena come on! Název hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                        case 1:
                            {
                                if (argumenty == null)
                                    bot.PrivateZprava(hrac, string.Format("= Byla založena DotA! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= Byla založena DotA! Název hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                        case 2:
                            {
                                if (argumenty == null)
                                    bot.PrivateZprava(hrac, string.Format("= Založena DotA! Odesilatel: {0} =", odesilatel));
                                else
                                    bot.PrivateZprava(hrac, string.Format("= Založena DotA! Název hry: {0} Odesilatel: {1} =", string.Join(" ", argumenty), odesilatel));

                                break;
                            }
                    }

                    online++;

                    Thread.Sleep(4000);
                }
            }

            bot.PrivateZprava(odesilatel, string.Format("Pozvánka na Dotu odeslána {0} online hráèùm", online));

            online++; //i s tím co odesílá

            switch (m_Nahoda.Next(3))
            {
                case 0:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= DotA zalaložena come on! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= DotA zalaložena come on! Název hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
                case 1:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= Byla založena DotA! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= Byla založena DotA! Název hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
                case 2:
                    {
                        if (argumenty == null)
                            bot.ChatZprava(string.Format("= Založena DotA! Odesilatel: {0} Online: {1} =", odesilatel, online));
                        else
                            bot.ChatZprava(string.Format("= Založena DotA! Název hry: {0} Odesilatel: {1} Online: {2} =", string.Join(" ", argumenty), odesilatel, online));

                        break;
                    }
            }

            m_Rozesila = false;
        }
    }
}
