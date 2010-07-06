using FindCS.Networking;
using FindCS.Hry;
using System.Text.RegularExpressions;
using System.Text;

namespace FindCS.Protocols
{
    sealed class UTStatProtocol : StatProtocol
    {
        internal override TypProtokolu Protocol
        {
            get { return TypProtokolu.Steam; }
        }

        private static readonly byte[] m_Dotaz = new byte[] //payload :)
        { 
            //5C 69 6E 66 6F 5C
            0x5C, 0x69, 0x6E, 0x66, 0x6F, 0x5C              
        };

        private const short m_Port = 7787;

        private const byte m_Oddelovac = 0x5C;

        private readonly Regex m_MapaRegexp = new Regex(@"mapname\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex m_PocetHracuRegexp = new Regex(@"numplayers\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal UTStatProtocol()
            : this(m_Port)
        {
        }

        internal UTStatProtocol(short port)
            : base(m_Dotaz, port)
        {
        }

        internal override ServerInfo ZiskejInfo(PacketReader reader)
        {
            if (reader.Delka < 10 || reader.PrectiByte() != 0x5C) //hlavièka
                return null;

           /* using (System.IO.BinaryWriter bin = new System.IO.BinaryWriter(new System.IO.FileStream("packet.bin", System.IO.FileMode.Create)))
            {
                bin.Write(reader.Paketa);
            }*/


            string data = Encoding.ASCII.GetString(reader.Paketa);

            ServerInfo info = new ServerInfo();

            info.Hra = TypHry.UT2004;

            reader.Pozice = m_MapaRegexp.Match(data, reader.Pozice, reader.Delka - reader.Pozice).Index + 7;

            info.Mapa = reader.PrectiASCIIString(m_Oddelovac);

            reader.Pozice = m_PocetHracuRegexp.Match(data, reader.Pozice, reader.Delka - reader.Pozice).Index + 10;

            info.PocetHracu = byte.Parse(reader.PrectiASCIIString(m_Oddelovac));

            reader.Pozice = reader.PoziceBajtu(m_Oddelovac); 

            info.MaxHracu = byte.Parse(reader.PrectiASCIIString(m_Oddelovac));

            return info;
        }
    }
}
