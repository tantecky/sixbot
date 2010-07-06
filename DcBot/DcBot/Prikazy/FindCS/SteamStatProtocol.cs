using FindCS.Networking;
using FindCS.Hry;

namespace FindCS.Protocols
{
    sealed class SteamStatProtocol : StatProtocol
    {
        internal override TypProtokolu Protocol
        {
            get { return TypProtokolu.Steam; }
        }

        private static readonly byte[] m_Dotaz = new byte[] //payload :)
        { 
            0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 
            0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 
            0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 
            0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 
            0x00 
        };

        private const short m_Port = 27015;

        private const byte m_Oddelovac = 0x00;


        internal SteamStatProtocol()
            : this(m_Port)
        {
        }

        internal SteamStatProtocol(short port)
            : base(m_Dotaz, port)
        {
        }

        private static TypHry UrciTypHry(string hra)
        {

            if(hra == "Counter-Strike" || hra == "Condition Zero") //kvùli kolejním fakùm
                return TypHry.CS;
            else if (hra == "Counter-Strike: Source")
                return TypHry.CSS;
            else if (hra == "Day of Defeat")
                return TypHry.DoD;
            else if(hra.Contains("L4D"))
                return TypHry.L4D;
            else
                return TypHry.Zadna;
        }

        internal override ServerInfo ZiskejInfo(PacketReader reader)
        {
            if (reader.Delka < 6 || reader.PrectiInt() != -1) //hlavièka
                return null;

           // System.Console.WriteLine(reader.SeekniByte().ToString("x"));

            bool source = reader.PrectiByte() == 0x49 ? true : false;
            //0x6D je klasicky half protokol

            /*using(System.IO.BinaryWriter bin = new System.IO.BinaryWriter(new System.IO.FileStream("packet.bin", System.IO.FileMode.Create)))
            {
                bin.Write(reader.Paketa);
            }*/

            if (!source)
                reader.Pozice = reader.PoziceBajtu(m_Oddelovac);

            reader.Pozice = reader.PoziceBajtu(m_Oddelovac); 
            
            ServerInfo info = new ServerInfo();

            info.Mapa = reader.PrectiASCIIString(m_Oddelovac); //mapa
            //System.Console.WriteLine(info.Mapa);
            reader.Pozice = reader.PoziceBajtu(m_Oddelovac);

            //System.Console.WriteLine(reader.PrectiASCIIString(0x00)); //typ hry
            info.Hra = UrciTypHry(reader.PrectiASCIIString(m_Oddelovac));

            if (info.Hra == TypHry.Zadna)
                return null;

            reader.Pozice += source ? 2 : 0;

            info.PocetHracu = reader.PrectiByte(); //poèet hráèù
            //System.Console.WriteLine(info.PocetHracu);
            info.MaxHracu = reader.PrectiByte(); //maximální poèet hráèù

            if (source)
                info.PocetBotu = reader.PrectiSbyte(); //poèet botù
            else
            {
                reader.Pozice = reader.Paketa.Length - 2;
                info.PocetBotu = reader.PrectiSbyte(); //poèet botù je to úplnì poslední bajt
            }

            return info;
        }
    }
}
