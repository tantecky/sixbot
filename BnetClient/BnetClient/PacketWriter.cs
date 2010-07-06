using System;
using System.Text;

namespace BnetClient
{
    sealed class PacketWriter
    {
        private byte[] m_Packeta;
        private ushort m_Delka;
        private int m_Index;

        internal byte[] Packeta
        {
            get
            {
                return m_Packeta;
            }
        }

        internal ushort Delka
        {
            get
            {
                return m_Delka;
            }
        }

        internal PacketWriter(int kapacita)
        {
            m_Packeta = new byte[kapacita];
            Vyprazdni();
        }

        internal void Vyprazdni()
        {
            m_Delka = 4;
            m_Index = 4;
        }

        private void OverDelku(int pocet)
        {
            if (m_Delka + pocet >= m_Packeta.Length)
                throw new IndexOutOfRangeException("Přetečení PacketWriteru, Delka=" + m_Delka + " Index=" + m_Index);
        }

        internal void ZapisByte(byte data)
        {
            OverDelku(1);
            m_Packeta[m_Index++] = data;

            m_Delka++;
        }

        internal void ZapisUShort(ushort data)
        {
            OverDelku(2);
            m_Packeta[m_Index++] = (byte)((data << 8) >> 8);
            m_Packeta[m_Index++] = (byte)(data >> 8);

            m_Delka += 2;
        }

        internal void ZapisUInt(uint data)
        {
            OverDelku(4);
            m_Packeta[m_Index++] = (byte)((data << 24) >> 24);
            m_Packeta[m_Index++] = (byte)((data << 16) >> 24);
            m_Packeta[m_Index++] = (byte)((data << 8) >> 24);
            m_Packeta[m_Index++] = (byte)(data >> 24);

            m_Delka += 4;
        }

        internal void ZapisBajtArray(byte[] data)
        {
            OverDelku(data.Length);

            Buffer.BlockCopy(data, 0, m_Packeta, m_Index, data.Length);

            m_Delka += (ushort)(data.Length);
        }

        internal void ZapisString(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            OverDelku(data.Length + 1);

            for (int i = 0; i < data.Length; i++)
                m_Packeta[m_Index++] = data[i];

            m_Packeta[m_Index++] = 0x00;

            m_Delka += (ushort)(data.Length + 1);
        }

        internal void ZapisHlavicku(PacketID id)
        {
            m_Packeta[0] = 0xFF;
            m_Packeta[1] = (byte)id;
            m_Packeta[2] = (byte)((m_Delka << 8) >> 8);
            m_Packeta[3] = (byte)(m_Delka >> 8);
        }
    }
}
