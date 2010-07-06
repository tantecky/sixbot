using System;

namespace DcBot
{
    class Linka
    {
        private string m_Url;
        private ushort m_Cislo;
        private SmerAutobusu m_Smer;

        internal string Url
        {
            get
            {
                return m_Url;
            }
        }

        internal ushort Cislo
        {
            get
            {
                return m_Cislo;
            }
        }

        internal SmerAutobusu Smer
        {
            get
            {
                return m_Smer;
            }
        }

        internal Linka(string url, ushort cislo, SmerAutobusu smer)
        {
            m_Url = url;
            m_Cislo = cislo;
            m_Smer = smer;
        }

    }
}
