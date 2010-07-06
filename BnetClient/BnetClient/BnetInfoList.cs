using System;
using System.Collections.Generic;
using System.Threading;

namespace BnetClient
{
    public sealed class BnetInfoList : IDisposable
    {
        private List<BnetInfo> m_Hry;
        private AutoResetEvent m_Synchronizace;

        internal int Pocet
        {
            get
            {
                return m_Hry.Count;
            }
        }

        internal BnetInfoList(int vychoziAlokace)
        {
            m_Hry = new List<BnetInfo>(vychoziAlokace);
            m_Synchronizace = new AutoResetEvent(true);
        }

        public void ZamkniList()
        {
            m_Synchronizace.WaitOne();
        }

        public void OdemkniList()
        {
            m_Synchronizace.Set();
        }

        internal void VyprazdniList()
        {
            m_Hry.Clear();
        }

        internal void NastavPocetHracu(string nazev, uint pocet)
        {
            ZamkniList();

            foreach (BnetInfo info in m_Hry)
            {
                if (info.NazevHry == nazev)
                {
                    info.NastavPocetHracu(pocet);
                    break;
                }
            }

            OdemkniList();
        }

        internal BnetInfo ZiskejHru(int index)
        {
            if (index < m_Hry.Count)
            {
                return m_Hry[index];
            }
            else
            {
                m_Hry.Add(new BnetInfo());

                return m_Hry[m_Hry.Count - 1];
            }
        }

        public BnetInfo[] ZiskejHry()
        {
            if (m_Hry.Count > 0)
                return m_Hry.ToArray();
            else
                return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (m_Synchronizace != null)
                m_Synchronizace.Close();
        }

        #endregion
    }
}
