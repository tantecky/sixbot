using System;

namespace BnetClient
{
    abstract class Packeta
    {
        protected PacketReader m_Reader;
        protected PacketWriter m_Writer;
        protected PacketID m_Id;
        protected Client m_Client;

        internal PacketID ID
        {
            get
            {
                return m_Id;
            }
        }

        internal Packeta(PacketReader pr, PacketWriter pw, PacketID id, Client client)
        {
            m_Reader = pr;
            m_Writer = pw;
            m_Id = id;
            m_Client = client;
        }

        internal virtual void ZapisPacketu()
        {
            m_Writer.ZapisHlavicku(m_Id);
        }


        internal virtual void PrectiPacketu()
        {
        }
    }
}
