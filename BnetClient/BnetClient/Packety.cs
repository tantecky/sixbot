using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BnetClient
{
    internal static class Packety
    {
        private static readonly Packeta[] m_Packety = new Packeta[256];

        internal static void InicializujPackety(PacketReader pr, PacketWriter pw, Client client)
        {
            //mám rád reflexi :)
            foreach (Type t in Assembly.GetAssembly(typeof(Packeta)).GetTypes()) 
            {
                if (t.IsSubclassOf(typeof(Packeta)))
                {
                    ConstructorInfo c = t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(PacketReader), typeof(PacketWriter), typeof(Client)}, null);

                    Packeta pac = c.Invoke(new object[]{pr, pw, client}) as Packeta;
                    m_Packety[(int)pac.ID] = pac;
                }
            }
        }

        internal static Packeta ZiskejPacketu(int id)
        {
            return m_Packety[id];
        }

        internal static Packeta ZiskejPacketu(PacketID id)
        {
            return m_Packety[(int)id];
        }

    }

    #region Pakety

    sealed class AuthInfoPacketa : Packeta
    {
        private static readonly byte[] m_Data = new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00, 
                        0x36, 0x38, 0x58, 0x49, 0x50, 0x58, 0x33, 0x57, 
                        0x18, 0x00, 0x00, 0x00, 0x53, 0x55, 0x6e, 0x65, 
                        0x00, 0x00, 0x00, 0x00, 0xc4, 0xff, 0xff, 0xff, 
                        0x05, 0x04, 0x00, 0x00, 0x05, 0x04, 0x00, 0x00, 
                        0x43, 0x5a, 0x45, 0x00, 0x43, 0x7a, 0x65, 0x63, 
                        0x68, 0x20, 0x52, 0x65, 0x70, 0x75, 0x62, 0x6c, 
                        0x69, 0x63, 0x00 
                    };

        internal AuthInfoPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_AUTH_INFO, client)
        {
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisBajtArray(m_Data);

            base.ZapisPacketu();
        }
    }

    sealed class PingPacketa : Packeta
    {
        internal PingPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_PING, client)
        {
        }

        internal override void PrectiPacketu()
        {
            m_Client.PingCookie = m_Reader.PrectiUInt();
            m_Client.OdesliPacketu(PacketID.SID_PING);
            m_Client.OdesliPacketu(PacketID.SID_AUTH_CHECK);
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisUInt(m_Client.PingCookie);

            base.ZapisPacketu();
        }
    }

    sealed class AuthCheckPacketa : Packeta
    {
        private static readonly byte[] m_Data = new byte[]
                    {
                        0x59, 0x8a, 0xf1, 0x00, 
                        0xf0, 0x03, 0x18, 0x01, 0x96, 0x4a, 0x7b, 0x2a, 
                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                        0x1a, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 
                        0x71, 0x70, 0xba, 0x02, 0x00, 0x00, 0x00, 0x00, 
                        0x3f, 0xc9, 0x6a, 0x78, 0x1a, 0x60, 0x48, 0xcb, 
                        0x51, 0x12, 0x52, 0x46, 0x93, 0x12, 0x1a, 0xbd, 
                        0x27, 0x29, 0x03, 0xe0, 0x1a, 0x00, 0x00, 0x00, 
                        0x12, 0x00, 0x00, 0x00, 0x7d, 0xdb, 0x71, 0x00, 
                        0x00, 0x00, 0x00, 0x00, 0x29, 0xdd, 0x5c, 0xbc, 
                        0x3b, 0x79, 0xea, 0x41, 0x53, 0xb7, 0x1c, 0x68, 
                        0x97, 0x17, 0x29, 0x3c, 0x9e, 0x5f, 0xca, 0x82, 
                        0x77, 0x61, 0x72, 0x33, 0x2e, 0x65, 0x78, 0x65, 
                        0x20, 0x30, 0x33, 0x2f, 0x32, 0x30, 0x2f, 0x31, 
                        0x30, 0x20, 0x31, 0x39, 0x3a, 0x35, 0x37, 0x3a, 
                        0x30, 0x34, 0x20, 0x34, 0x37, 0x31, 0x30, 0x34, 
                        0x30, 0x00, 0x6b, 0x61, 0x6c, 0x00 
                    };

        internal AuthCheckPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_AUTH_CHECK, client)
        {
        }

        internal override void PrectiPacketu()
        {
            uint id = m_Reader.PrectiUInt();

            if (id != 0x00)
                throw new W3Exception("SID_AUTH_CHECK neobvyklá odezva: " + id.ToString("X8"));

            m_Client.OdesliPacketu(PacketID.SID_AUTH_ACCOUNTLOGON);
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisBajtArray(m_Data);

            base.ZapisPacketu();
        }
    }

    sealed class AuthLogonPacketa : Packeta
    {
        private static readonly byte[] m_Data = new byte[]
                    {
                        //client key + login
                        0x10, 0xba, 0x14, 0x37, 
                        0x9c, 0xc5, 0x28, 0x2c, 0x6b, 0xb8, 0x90, 0x35, 
                        0x9a, 0xd9, 0x3c, 0xc7, 0xfc, 0x84, 0x46, 0x89, 
                        0x35, 0xe6, 0xf4, 0xf6, 0x7d, 0x85, 0x6d, 0x79, 
                        0xc7, 0xac, 0x9c, 0x88, 0x53, 0x69, 0x78, 0x62, 
                        0x6f, 0x74, 0x00 
                    };

        internal AuthLogonPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_AUTH_ACCOUNTLOGON, client)
        {
        }

        internal override void PrectiPacketu()
        {
            uint id = m_Reader.PrectiUInt();

            if (id != 0x00)
                throw new W3Exception("SID_AUTH_ACCOUNTLOGON neobvyklá odezva: " + id.ToString("X8"));

            m_Client.OdesliPacketu(PacketID.SID_AUTH_ACCOUNTLOGONPROOF);
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisBajtArray(m_Data);

            base.ZapisPacketu();
        }
    }

    sealed class AuthLogonProofPacketa : Packeta
    {
        private static readonly byte[] m_Data = new byte[]
                    {
                        //zahashovane heslo 
                        0xad, 0xfd, 0x6e, 0x61, 
                        0x43, 0x26, 0x61, 0xca, 0x9e, 0x52, 0x48, 0x01, 
                        0xb2, 0x59, 0x9a, 0xe0, 0x77, 0xeb, 0xa6, 0xfe 
                    };

        internal AuthLogonProofPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_AUTH_ACCOUNTLOGONPROOF, client)
        {
        }

        internal override void PrectiPacketu()
        {
            uint id = m_Reader.PrectiUInt();

            if (id != 0x00)
                throw new W3Exception("SID_AUTH_ACCOUNTLOGONPROOF neobvyklá odezva: " + id.ToString("X8"));

            m_Client.OdesliPacketu(PacketID.SID_NETGAMEPORT);
            m_Client.OdesliPacketu(PacketID.SID_ENTERCHAT);
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisBajtArray(m_Data);

            base.ZapisPacketu();
        }
    }

    sealed class NetGamePortPacketa : Packeta
    {
        internal NetGamePortPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_NETGAMEPORT, client)
        {
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisUShort(6112); //klasicky portík pro hru

            base.ZapisPacketu();
        }
    }

    sealed class EnterChatPacketa : Packeta
    {
        internal EnterChatPacketa(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_ENTERCHAT, client)
        {
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisUShort(0x00);

            base.ZapisPacketu();
        }

        internal override void PrectiPacketu()
        {
            m_Client.PriVstupuDoChatu();
        }
    }

    sealed class GetAdcListex : Packeta
    {
        private static readonly byte[] m_Data = new byte[]
                    {
                    0x00, 0xe0, 0x7f, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 
                    };

        private static readonly Regex m_MapaRegex = new Regex(@"[^\\]+\.w3", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        internal GetAdcListex(PacketReader pr, PacketWriter pw, Client client)
            : base(pr, pw, PacketID.SID_GETADVLISTEX, client)
        {
        }

        internal override void ZapisPacketu()
        {
            m_Writer.ZapisBajtArray(m_Data);

            base.ZapisPacketu();
        }

        internal override void PrectiPacketu()
        {
            uint pocetHer = m_Reader.PrectiUInt();

            if (pocetHer > 0)
            {
                m_Client.Hry.ZamkniList();

                if(m_Client.Hry.Pocet > pocetHer)
                    m_Client.Hry.VyprazdniList();

                m_Client.Hry.OdemkniList();

                for (int i = 0; i < pocetHer; i++)
                {
                    m_Reader.SeekniO(24);

                    uint stavHry = m_Reader.PrectiUInt(); //výzkum, 0x04 je podle mě ložnuto a čeká se

                    m_Reader.SeekniO(4); //Elapsed time (in seconds)

                    string nazevHry = m_Reader.PrectiASCIIString();

                    m_Reader.SeekniO(1); //password

                    int celkovaKapacita = m_Reader.PrectiBajt() - 0x30 + 1; //+1 není jistá uvidíme

                    if (celkovaKapacita < 1)
                        throw new W3Exception("Celková kapacita hry je menší než 1: " + celkovaKapacita);

                    /*byte[] pocetHracuRaw = m_Reader.PrectiBajtArray(8);
                    Array.Reverse(pocetHracuRaw);

                    uint pocetHracu = Convert.ToUInt32(Encoding.ASCII.GetString(pocetHracuRaw), 16);*/

                    m_Reader.SeekniO(8); //počet hráčů který je bohužel posraný takže na to musím jinak

                    int novaPozice = m_Reader.DekodujStatString();

                    m_Reader.SeekniO(13); //hovadinky ve StatStringu

                    string mapa = m_MapaRegex.Match(m_Reader.PrectiASCIIString()).Value;
                    mapa = mapa.Remove(mapa.IndexOf(".w3"));

                    if (mapa == String.Empty)
                        throw new W3Exception("Název mapy nenalezen ve StatStringu");

                    string zalozil = m_Reader.PrectiASCIIString();

                    if (zalozil == String.Empty)
                        throw new W3Exception("Kdo založil nenalezeno ve StatStringu");

                    m_Reader.SeekniAbsolutne(novaPozice); //nastavi pozici na konec StatStringu

                    m_Client.Hry.ZamkniList();
                    m_Client.Hry.ZiskejHru(i).NastavVlastnosti(stavHry, nazevHry, celkovaKapacita, /*pocetHracu,*/ mapa, zalozil);
                    m_Client.Hry.OdemkniList();
                }
            }
            else
            {
                m_Client.Hry.ZamkniList();
                m_Client.Hry.VyprazdniList();
                m_Client.Hry.OdemkniList();
            }
        }

        sealed class ChatCommandPacketa : Packeta
        {
            private static readonly byte[] m_Data = new byte[]
                    {
                        //games
                        0x2f, 0x67, 0x61, 0x6d, 
                        0x65, 0x73, 0x00 
                    };
            internal ChatCommandPacketa(PacketReader pr, PacketWriter pw, Client client)
                : base(pr, pw, PacketID.SID_CHATCOMMAND, client)
            {
            }

            internal override void ZapisPacketu()
            {
                m_Writer.ZapisBajtArray(m_Data);

                base.ZapisPacketu();
            }
        }

        sealed class ChatEventPacketa : Packeta
        {
            private static readonly Regex m_MapaRegex = new Regex(@".+\s[nj]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            internal ChatEventPacketa(PacketReader pr, PacketWriter pw, Client client)
                : base(pr, pw, PacketID.SID_CHATEVENT, client)
            {
            }

            internal override void PrectiPacketu()
            {
                uint eventId = m_Reader.PrectiUInt();

                if (eventId != 0x12)
                    throw new W3Exception("SID_CHATEVENT neobvyklá odezva: " + eventId.ToString("X8"));

                m_Reader.SeekniO(16); //bordel
                m_Reader.PrectiASCIIString(); //nevim, 4 bajty + 0x00 takže string

                if(m_Reader.SeekniBajt() == 0x20)
                    m_Reader.SeekniO(1); // 0x20 - mezera

                string infoRaw = m_Reader.PrectiASCIIString();

                if (infoRaw.StartsWith("------"))
                    m_Client.CekajiciHry.SmazHry();
                else if (!infoRaw.StartsWith("Currently"))
                {
                    string stav = infoRaw.Substring(19, 8).Trim();
                    string nazev = infoRaw.Substring(0, 16).Trim(); //nazev
                    uint pocetHracu = uint.Parse(infoRaw.Substring(50, 5).Trim()); //počet

                    if (stav == "open") //stav
                    {
                        m_Client.Hry.NastavPocetHracu(nazev, pocetHracu);
                    }
                    else if (stav == "started")
                    {
                        m_Client.CekajiciHry.PridejHru("Název: " + nazev + " Počet hráčů: " + pocetHracu);
                    }
                    else if (stav == "full")
                        return;
                    else
                        throw new W3Exception("SID_CHATEVENT neznámý stav hry: " + stav);
                }
           }
        }
    }
        
    #endregion
}
