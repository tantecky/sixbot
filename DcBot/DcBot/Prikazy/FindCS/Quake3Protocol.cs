using FindCS.Networking;
using FindCS.Hry;
using System.Text.RegularExpressions;
using System.Text;

namespace FindCS.Protocols
{
    sealed class Quake3Protocol : StatProtocol
    {
        internal override TypProtokolu Protocol
        {
            get { return TypProtokolu.Quake3; }
        }

        private static readonly byte[] m_Dotaz = new byte[] //payload :)
        { 
            //FFFFFFFF6765747374617475730A  
            0xFF, 0xFF, 0xFF, 0xFF, 0x67, 0x65, 0x74, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 0x0A             
        };

        private const short m_Port = 28960;

        private const byte m_Oddelovac = 0x5C;

        private readonly Regex m_MapaRegexp = new Regex(@"mapname\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex m_MaxHracuRegexp = new Regex(@"sv_maxclients\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex m_PocetHracuRegexp = new Regex(@"\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal Quake3Protocol()
            : this(m_Port)
        {
        }

        internal Quake3Protocol(short port)
            : base(m_Dotaz, port)
        {
        }

        internal override ServerInfo ZiskejInfo(PacketReader reader)
        {
            if (reader.Delka < 13 || reader.PrectiInt() != -1) //hlavièka
                return null;

            string data = Encoding.ASCII.GetString(reader.Paketa);

            ServerInfo info = new ServerInfo();

            info.Hra = TypHry.CODUO;

            reader.Pozice = m_MapaRegexp.Match(data, reader.Pozice, reader.Delka - reader.Pozice).Index + 7;

            info.Mapa = reader.PrectiASCIIString(m_Oddelovac);

            reader.Pozice = m_MaxHracuRegexp.Match(data, reader.Pozice, reader.Delka - reader.Pozice).Index + 13;

            info.MaxHracu = byte.Parse(reader.PrectiASCIIString(m_Oddelovac));

            info.PocetHracu = (byte)(m_PocetHracuRegexp.Matches(data, reader.Pozice).Count - 1);

            return info;
        }
    }
}
