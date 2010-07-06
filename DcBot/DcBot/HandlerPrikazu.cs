using System;
using System.Collections.Generic;

namespace DcBot
{
    internal static class HandlerPrikazu
    {
#if DEBUG
        internal const string PrikazPrefix = "+";
#else

        internal const string PrikazPrefix = "-";
#endif

        internal static readonly string[] Oddelovac = new string[] { " " };

        private static SortedDictionary<string, BasePrikaz> m_Prikazy = new SortedDictionary<string, BasePrikaz>();

        internal static SortedDictionary<string, BasePrikaz> Prikazy
        {
            get
            {
                return m_Prikazy;
            }
        }

        internal static void Inicialiazce(SixBot bot)
        {
            m_Prikazy.Add("echo", new EchoPrikaz());
            m_Prikazy.Add("dotaroster", new DotaRoster());
            m_Prikazy.Add("addme", new AddMe());
            m_Prikazy.Add("removeme", new RemoveMe());
            m_Prikazy.Add("spamdota", new SpamDota());
            m_Prikazy.Add("help", new Help());
            m_Prikazy.Add("google", new Google());
            m_Prikazy.Add("findcs", new FindCs(bot));
            m_Prikazy.Add("say", new Say());
            m_Prikazy.Add("about", new AboutPrikaz());
            m_Prikazy.Add("dotaonline", new DotaOnline(bot));
            m_Prikazy.Add("bus", new Bus(bot));

            //tohle vždy na konec
            PridejAliasy();
        }

        private static void PridejAliasy()
        {
            Dictionary<string, BasePrikaz> m_Aliasy = new Dictionary<string, BasePrikaz>(m_Prikazy.Count);

            string alias;

            foreach (BasePrikaz polozka in m_Prikazy.Values)
            {
                alias = polozka.ZiskejAlias();

                if (alias == null)
                    continue;

                m_Aliasy.Add(alias, polozka);
            }

            foreach(KeyValuePair<string, BasePrikaz> polozka in m_Aliasy)
            {
                m_Prikazy.Add(polozka.Key, polozka.Value);
            }
        }

        internal static void PouzijPrikaz(object obj)
        {
            ArgumentyHandleru data = (ArgumentyHandleru)obj;

            try
            {
                int index = data.Text.IndexOf(Oddelovac[0]);

                BasePrikaz prikaz;

                if (index == -1)
                    prikaz = m_Prikazy[data.Text.ToLower()];
                else
                    prikaz = m_Prikazy[data.Text.Substring(0, index).ToLower()];

                switch (prikaz.PodporovaneArgumenty)
                {
                    case TypArgumentu.Zadny:
                        {
                            prikaz.PriPouziti(data.Bot, data.Odesilatel);

                            break;
                        }
                    case TypArgumentu.Nepovinny:
                        {
                            if (index == -1)
                                prikaz.PriPouziti(data.Bot, data.Odesilatel, null);
                            else
                                prikaz.PriPouziti(data.Bot, data.Odesilatel, data.Text.Substring(index).Split(Oddelovac, StringSplitOptions.RemoveEmptyEntries));

                            break;
                        }
                    case TypArgumentu.Povinny:
                        {
                            if (index == -1)
                            {
                                data.Bot.PrivateZprava(data.Odesilatel, prikaz.ZiskejHelp());
                            }
                            else
                            {
                                string[] argumenty = data.Text.Substring(index).Split(Oddelovac, StringSplitOptions.RemoveEmptyEntries);

                                if(prikaz.MinimalniPocetArgumentu > argumenty.Length)
                                    data.Bot.PrivateZprava(data.Odesilatel, prikaz.ZiskejHelp());
                                else
                                    prikaz.PriPouziti(data.Bot, data.Odesilatel, argumenty);
                            }

                            break;
                        }
                }

                data.Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} {3}>> {4}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, data.Odesilatel, data.Text));

            }
            catch (KeyNotFoundException)
            {
                data.Bot.Gui.VypisRadek(string.Format("{0:00}:{1:00}:{2:00} Neznámý pøíkaz {3}>> {4}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, data.Odesilatel, data.Text));
                m_Prikazy["help"].PriPouziti(data.Bot, data.Odesilatel);
            }

        }
    }
}
