using System;
using System.Text;
using System.Collections.Generic;

namespace BnetClient
{
    sealed class PacketReader
    {
        private byte[] m_Packeta;
        private ushort m_Delka;
        private int m_Index;
        private int m_Preceteno;

        internal PacketReader(PacketBuffer bf)
        {
            m_Packeta = bf.Packeta;
        }

        internal void NactiPacketu(ushort delka, int index)
        {
            m_Delka = delka;
            m_Index = index;

            //přeskočím 0xFF + packetID + ushort delku
            m_Index += 4;
            m_Preceteno = 3;
        }

        internal void SeekniO(ushort skok)
        {
            OverDelku(skok);

            m_Index += skok;
            m_Preceteno += skok;
        }

        internal void SeekniAbsolutne(int novyIndex)
        {
            if(novyIndex < 0)
                throw new IndexOutOfRangeException("SeekniAbsolutne novyIndex menší než 0, Delka=" + m_Delka + " Precteno=" + m_Preceteno + " Index=" + m_Index);

            m_Preceteno += novyIndex - m_Index;
            m_Index = novyIndex;

            if (m_Preceteno < 0)
                throw new IndexOutOfRangeException("SeekniAbsolutne m_Preceteno menší než 0, Delka=" + m_Delka + " Precteno=" + m_Preceteno + " Index=" + m_Index);

            if (m_Preceteno >= m_Delka)
                throw new IndexOutOfRangeException("SeekniAbsolutne m_Preceteno větší nebo rovno než m_Delka, Delka=" + m_Delka + " Precteno=" + m_Preceteno + " Index=" + m_Index);
        }

        private void OverDelku(int pocet)
        {
            if (m_Preceteno + pocet >= m_Delka)
                throw new IndexOutOfRangeException("Přetečení PacketReaderu, Delka=" + m_Delka + " Precteno=" + m_Preceteno + " Index=" + m_Index);
        }

        internal byte PrectiBajt()
        {
            OverDelku(1);
            m_Preceteno++;

            byte data = m_Packeta[m_Index];
            m_Index++;

            return data;
        }

        internal byte SeekniBajt()
        {
            OverDelku(1);

            return m_Packeta[m_Index];
        }

        internal ushort PrectiUShort()
        {
            OverDelku(2);
            m_Preceteno += 2;

            ushort data = (ushort)(m_Packeta[m_Index] | m_Packeta[++m_Index] << 8);
            m_Index++;

            return data;
        }

        internal uint PrectiUInt()
        {
            OverDelku(4);
            m_Preceteno += 4;

            uint data = (uint)(m_Packeta[m_Index] | m_Packeta[++m_Index] << 8 | m_Packeta[++m_Index] << 16 | m_Packeta[++m_Index] << 24);
            m_Index++;

            return data;
        }

        internal byte[] PrectiBajtArray(ushort delka)
        {
            OverDelku(delka);

            byte[] data = new byte[delka];

            Buffer.BlockCopy(m_Packeta, m_Index, data, 0, delka);

            m_Preceteno += delka;
            m_Index += delka;

            return data;

        }

        internal string PrectiASCIIString()
        {
            StringBuilder sb = new StringBuilder();

            while (this.SeekniBajt() != 0x00)
            {
                sb.Append((char)m_Packeta[m_Index++]);

                m_Preceteno++;
            }

            //posunutí za null bajt
            m_Index++;
            m_Preceteno++;

            return sb.ToString();
        }

        internal int DekodujStatString()
        {
            //checknoout null bajt
            if (m_Packeta[m_Delka - 1] != 0x00)
                throw new W3Exception("Neplatné ukončení StatStringu");

            int precteno = m_Preceteno;
            int index = m_Index;

            int delkaCrypt = 0;
            while (this.PrectiBajt() != 0x00)
                delkaCrypt++;

            m_Preceteno = precteno;
            m_Index = index;

            List<byte> temp = new List<byte>(delkaCrypt); //o pár bajtů větší
            byte bitovaMaska = 0;

            for (int i = m_Index, j = 0; i < delkaCrypt + m_Index; i++, j++)
            {
                if ((j % 8) == 0)
                    bitovaMaska = m_Packeta[i];
                else
                {
                    if ((bitovaMaska & (1 << (j % 8))) == 0)
                        temp.Add((byte)(m_Packeta[i] - 1));
                    else
                        temp.Add(m_Packeta[i]);
                }
            }

            byte[] dekodovano = temp.ToArray();

            Buffer.BlockCopy(dekodovano, 0, m_Packeta, m_Index, dekodovano.Length);

            return delkaCrypt + m_Index + 1;
            //--------old
            /*int delkaCrypt = m_Delka - m_Index;

            List<byte> temp = new List<byte>(delkaCrypt); //o pár bajtů větší
            byte bitovaMaska = 0;

            for (int i = m_Index, j = 0; i < m_Delka - 1; i++, j++) //-1 protože je tam null byte
            {
                if ((j % 8) == 0)
                    bitovaMaska = m_Packeta[i];
                else
                {
                    if ((bitovaMaska & (1 << (j % 8))) == 0)
                        temp.Add((byte)(m_Packeta[i] - 1));
                    else
                        temp.Add(m_Packeta[i]);
                }
            }

            byte[] dekodovano = temp.ToArray();

            Buffer.BlockCopy(dekodovano, 0, m_Packeta, m_Index, dekodovano.Length);*/

            //m_Delka -= (ushort)(delkaCrypt - dekodovano.Length);
        }
    }
}
