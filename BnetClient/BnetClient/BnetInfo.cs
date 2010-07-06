using System.Text;
namespace BnetClient
{
    public sealed class BnetInfo
    {
        private uint m_StavHry;
        private string m_NazevHry;
        private int m_CelkovaKapacita;
        private uint m_PocetHracu;
        private string m_Mapa;
        private string m_Zalozil;

        public uint StavHry
        {
            get
            {
                return m_StavHry;
            }
        }

        public string NazevHry
        {
            get
            {
                return m_NazevHry;
            }
        }

        public int CelkovaKapacita
        {
            get
            {
                return m_CelkovaKapacita;
            }
        }

        public uint PocetHracu
        {
            get
            {
                return m_PocetHracu;
            }
        }

        public string Mapa
        {
            get
            {
                return m_Mapa;
            }
        }

        public string Zalozil
        {
            get
            {
                return m_Zalozil;
            }
        }

        internal BnetInfo()
        {
        }

        internal void NastavPocetHracu(uint pocet)
        {
            m_PocetHracu = pocet;
        }

        internal void NastavVlastnosti(uint stav, string nazev, int kapacita, /*uint pocetHracu,*/ string mapa, string zalozil)
        {
            m_StavHry = stav;

            if (m_NazevHry == null)
                m_NazevHry = nazev;
            else if (m_NazevHry != null && m_NazevHry != nazev)
                m_NazevHry = nazev;

            m_CelkovaKapacita = kapacita;
           // m_PocetHracu = pocetHracu;

            if (m_Mapa == null)
                m_Mapa = mapa;
            else if (m_Mapa != null && m_Mapa != mapa)
                m_Mapa = mapa;

            if (m_Zalozil == null)
                m_Zalozil = zalozil;
            else if (m_Zalozil != null && m_Zalozil != zalozil)
                m_Zalozil = zalozil;

        }

        public override string ToString()
        {
            return "Název: " + m_NazevHry + " Počet hráčů: " + m_PocetHracu + "/" + m_CelkovaKapacita + " Mapa: " + m_Mapa + " Založil: " + m_Zalozil;
        }
    }
}
