using System;

namespace DcBot
{
    enum SmerAutobusu : byte
    {
        Opatov,
        Chodov
    }

    struct Autobus
    {
        private ushort m_Cislo;
        private DateTime m_Odjezd;
        private SmerAutobusu m_Smer;

        internal ushort Cislo
        {
            get
            {
                return m_Cislo;
            }
        }

        internal DateTime Odjezd
        {
            get
            {
                return m_Odjezd;
            }
        }

        internal SmerAutobusu Smer
        {
            get
            {
                return m_Smer;
            }
        }

        internal Autobus(ushort cislo, DateTime odjezd, SmerAutobusu smer)
        {
            m_Cislo = cislo;
            m_Odjezd = odjezd;
            m_Smer = smer;
        }

        public override string ToString()
        {
            return m_Odjezd.ToShortTimeString() + " " + m_Cislo + " Volha -> " + m_Smer.ToString();
        }
    }
}
