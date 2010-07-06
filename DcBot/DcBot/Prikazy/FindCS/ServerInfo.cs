using System.Net;
using System.Windows.Forms;
using FindCS.Hry;

namespace FindCS
{
    internal class ServerInfo
    {
        private IPAddress m_IP;
        private TypHry m_Hra;
        private string m_Mapa;
        private byte m_PocetHracu;
        private byte m_MaxHracu;
        private sbyte m_PocetBotu;

        internal IPAddress IP
        {
            get
            {
                return m_IP;
            }
            set
            {
                m_IP = value;
            }
        }

        internal TypHry Hra
        {
            get
            {
                return m_Hra;
            }
            set
            {
                m_Hra = value;
            }
        }

        internal string Mapa
        {
            get
            {
                return m_Mapa;
            }
            set
            {
                m_Mapa = value;
            }
        }

        internal byte PocetHracu
        {
            get
            {
                return m_PocetHracu;
            }
            set
            {
                m_PocetHracu = value;
            }
        }

        internal byte MaxHracu
        {
            get
            {
                return m_MaxHracu;
            }
            set
            {
                m_MaxHracu = value;
            }
        }

        internal sbyte PocetBotu
        {
            get
            {
                return m_PocetBotu;
            }
            set
            {
                m_PocetBotu = value;
            }
        }

        internal ServerInfo()
        {
            m_PocetBotu = -1;
        }
    }
}
