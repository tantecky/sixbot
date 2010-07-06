using System;
using System.Text;

namespace FindCS.Networking
{
    sealed class PacketReader
    {
        private byte[] m_Paketa;
        private int m_Delka;
        private int m_Pozice;

        internal byte[] Paketa
        {
            get
            {
                return m_Paketa;
            }
        }

        internal int Delka
        {
            get
            {
                return m_Delka;
            }
        }

        internal int Pozice
        {
            get
            {
                return m_Pozice;
            }
            set
            {
                if (value >= m_Delka)
                    m_Pozice = m_Delka - 1;
                else if (value < 0)
                    m_Pozice = -1;
                else
                    m_Pozice = value;
            }
        }

        internal void NactiPaketu(byte[] paketa)
        {
            m_Paketa = paketa;
            m_Delka = paketa.Length;
            m_Pozice = -1;
        }

        internal void UvolniPaketu()
        {
            m_Paketa = null;
        }

        private void OverDelku()
        {
            OverDelku(1);
        }

        private void OverDelku(int pocet)
        {
            if ((m_Pozice + pocet) >= m_Delka)
                throw new ArgumentOutOfRangeException(BitConverter.ToString(m_Paketa));
        }

        internal byte SeekniByte()
        {
            OverDelku();
            return m_Paketa[m_Pozice + 1];
        }

        internal byte PrectiByte()
        {
            OverDelku();
            return m_Paketa[++m_Pozice];
        }

        internal sbyte PrectiSbyte()
        {
            OverDelku();
            return (sbyte)m_Paketa[++m_Pozice];
        }

        internal bool PrectiBool()
        {
            OverDelku();
            return m_Paketa[++m_Pozice] != 0;
        }

        internal int PrectiInt()
        {
            OverDelku(4);
            return m_Paketa[++m_Pozice] << 24 | m_Paketa[++m_Pozice] << 16 | m_Paketa[++m_Pozice] << 8 | m_Paketa[++m_Pozice];
        }

        internal string PrectiASCIIString(byte oddelovac)
        {
            StringBuilder sb = new StringBuilder();

            while (this.SeekniByte() != oddelovac)
                sb.Append((char)m_Paketa[++m_Pozice]);

            m_Pozice++;

            return sb.ToString();
        }

        internal int PoziceBajtu(byte bajt)
        {
            return Array.IndexOf(m_Paketa, bajt, m_Pozice + 1);
        }
    }
}
