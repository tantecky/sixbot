using System;
using System.Text;

namespace BnetClient
{
    public class StartedGamesList
    {
        private StringBuilder m_Hry;

        internal StartedGamesList(int kapacita)
        {
            m_Hry = new StringBuilder(kapacita);
        }

        internal void PridejHru(string hra)
        {
            lock (m_Hry)
                m_Hry.AppendLine(hra);
        }

        internal void SmazHry()
        {
            lock (m_Hry)
                m_Hry.Remove(0, m_Hry.Length);
        }

        public string ZiskejHry()
        {
            lock (m_Hry)
            {
                if (m_Hry.Length > 0)
                    return m_Hry.ToString();
                else
                    return null;
            }
        }
    }
}
