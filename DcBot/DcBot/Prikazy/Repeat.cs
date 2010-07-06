using DcBot;
using System.Collections.Generic;
using System.Threading;

namespace DcBot
{
    [PomocAtribut("lll")]
    class Repeat : BasePrikaz
    {
        private static List<Sdeleni> m_Sdeleni;

        private SixBot m_Bot;

        public static List<Sdeleni> Sdeleni
        {
            get
            {
                return m_Sdeleni;
            }
        }

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Povinny; } }

        internal override byte MinimalniPocetArgumentu { get { return 3; } }

        internal Repeat(SixBot bot)
        {
            m_Bot = bot;

            m_Sdeleni = new List<Sdeleni>();
        }

        internal override void PriPouziti(SixBot bot, string odesilatel, string[] argumenty)
        {
            bot.PrivateZprava(odesilatel, "omg");
        }

        private void SpustVlakno()
        {
            Thread vlakno = new Thread(new ThreadStart(CyklickyCheck));
            vlakno.Name = "RepeatThread";
            vlakno.Priority = ThreadPriority.BelowNormal;
            vlakno.IsBackground = true;
            vlakno.Start();
        }

        private void CyklickyCheck()
        {
            lock (Sdeleni)
            {
                foreach (Sdeleni sdeleni in m_Sdeleni)
                {
                }
            }

            Thread.Sleep(10000);
        }
    }
}
