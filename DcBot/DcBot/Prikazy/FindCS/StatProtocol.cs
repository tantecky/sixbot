using FindCS.Networking;
using System;

namespace FindCS.Protocols
{
    internal enum TypProtokolu : byte
    {
        Steam = 0,
        UT,
        Quake3
        /*AoE,
        AoE2*/
    }

    internal abstract class StatProtocol
    {
        internal static readonly StatProtocol[] Protokoly = new StatProtocol[] {new SteamStatProtocol(), new UTStatProtocol(), new Quake3Protocol() };

        private byte[] m_Dotaz;
        private short m_Port;

        internal byte[] Dotaz
        {
            get
            {
                return m_Dotaz;
            }
        }

        internal short Port
        {
            get
            {
                return m_Port;
            }
        }

        internal abstract TypProtokolu Protocol { get; }

        internal StatProtocol(byte[] dotaz, short port)
        {
            m_Dotaz = dotaz;
            m_Port = port;
        }

        internal abstract ServerInfo ZiskejInfo(PacketReader reader);
    }
}
