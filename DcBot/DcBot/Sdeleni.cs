using System;

namespace DcBot
{
    internal class Sdeleni
    {
        private DateTime m_Naposledy;
        private TimeSpan m_Interval;
        private string m_Zprava;

        public string Zprava
        {
            get
            {
                return m_Zprava;
            }
        }

        public DateTime Naposledy
        {
            get
            {
                return m_Naposledy;
            }
        }

        internal Sdeleni(TimeSpan interval, string zprava)
        {
            m_Naposledy = DateTime.MinValue;
            m_Interval = interval;
            m_Zprava = zprava;
        }

        internal bool MamPoslat()
        {
            if((DateTime.Now - m_Naposledy) >= m_Interval)
                return true;

            return false;
        }
    }
}
