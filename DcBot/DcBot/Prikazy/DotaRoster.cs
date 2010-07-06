using System.Collections.Generic;
using DcBot;
using System.IO;
using System.Text;

namespace DcBot
{
    [PomocAtribut("zobrazí seznam hráèù v dota rosteru")]
    [AliasAtribut("dr")]
    class DotaRoster : BasePrikaz
    {
        private static List<string> m_Hraci;

        internal static List<string> Hraci
        {
            get
            {
                return m_Hraci;
            }
        }

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal DotaRoster()
        {
           m_Hraci = new List<string>();
           NactiList();
        }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            lock (Hraci)
            {
                if (m_Hraci.Count == 0)
                    return;

                StringBuilder sb = new StringBuilder(m_Hraci.Count + 1);
                sb.AppendLine("Hráèi Doty, kteøí obdrží PMka pøi napsání -dotaspam. Pro pøidání -addme, pro odebrání -removeme");

                foreach (string hrac in m_Hraci)
                {
                    sb.AppendLine(hrac);
                }

                bot.PrivateZprava(odesilatel, sb.ToString());
            }
        }

        internal static bool PridejHrace(string jmeno)
        {
            lock (Hraci)
            {
                if (!m_Hraci.Contains(jmeno))
                {
                    m_Hraci.Add(jmeno);

                    using (StreamWriter writer = new StreamWriter("DotaRoster.cfg", true))
                    {
                        writer.WriteLine(jmeno);
                    }

                    return true;
                }
                else
                    return false;
            }
        }

        internal static bool OdeberHrace(string jmeno)
        {
            lock (Hraci)
            {

                if (m_Hraci.Contains(jmeno))
                {
                    m_Hraci.Remove(jmeno);

                    using (StreamWriter writer = new StreamWriter("DotaRoster.cfg", false))
                    {
                        foreach (string hrac in m_Hraci)
                            writer.WriteLine(hrac);
                    }

                    return true;
                }
                else
                    return false;
            }
        }

        private void NactiList()
        {
            if (!File.Exists("DotaRoster.cfg"))
                return;

            using(StreamReader reader = new StreamReader("DotaRoster.cfg"))
            {
                while (!reader.EndOfStream)
                {
                    m_Hraci.Add(reader.ReadLine());
                }
            }
        }
    }
}