using System;
using System.IO;
using System.Text;

namespace BnetClient
{
    enum StavPackety : byte
    {
        NeplatnaHlavicka,
        NedostatekDat,
        PrilisKratkaPacketa,
        OK
    }

    sealed class PacketBuffer
    {
        private byte[] m_Buffer;
        private int m_Delka;
        private int m_Zacatek;

        internal bool ObsahujeData
        {
            get
            {
                return m_Delka != 0;
            }
        }

        internal byte[] Packeta
        {
            get
            {
                return m_Buffer;
            }
        }

        internal PacketBuffer(int kapacita)
        {
            m_Buffer = new byte[kapacita];
            Vyprazdni();
        }

        internal void Vyprazdni()
        {
            m_Delka = 0;
            m_Zacatek = 0;
        }

        internal void DumpniBuffer(StreamWriter sw)
        {
            sw.WriteLine("---Dump PacketBufferu---");
            sw.WriteLine("Delka: " + m_Delka);
            sw.WriteLine("Zacatek: " + m_Zacatek);
            sw.WriteLine();

            sw.WriteLine("     01 02 03 04 05 06 07 08 09 10 AA BB CC DD EE FF ASCII");
            sw.WriteLine("------------------------------------------------------------------");

            for (int i = 0; i < m_Buffer.Length; i += 16)
            {
                sw.Write((i + 16).ToString("X4") + " ");
                sw.Write(BitConverter.ToString(m_Buffer, i, 16).Replace("-", " ") + " ");

                sw.WriteLine(Encoding.ASCII.GetString(m_Buffer, i, 16));
            }
        }

        internal bool VlozData(byte[] data, int pocetBajtu)
        {
            if (m_Delka + pocetBajtu > m_Buffer.Length)
                return false;

            Buffer.BlockCopy(data, 0, m_Buffer, m_Delka, pocetBajtu);
            m_Delka += pocetBajtu;

            return true;
        }

        internal StavPackety ZiskejPacketu(ref ushort delka, ref int zacatek)
        {
            if (m_Buffer[m_Zacatek] != 0xFF)
                return StavPackety.NeplatnaHlavicka;

            delka = (ushort)(m_Buffer[m_Zacatek + 3] << 8 | m_Buffer[m_Zacatek + 2]);

            if (delka < 5)
                return StavPackety.PrilisKratkaPacketa;

            if (m_Zacatek + delka > m_Delka)
                return StavPackety.NedostatekDat;

            zacatek = m_Zacatek;

            m_Zacatek += delka;

            if (m_Zacatek == m_Delka)
            {
                m_Delka = 0;
                m_Zacatek = 0;
            }

            return StavPackety.OK;
        }
    }
}
