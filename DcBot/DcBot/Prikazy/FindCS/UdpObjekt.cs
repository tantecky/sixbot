using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FindCS.Protocols;

internal class UdpObjekt
{
    private UdpClient m_Klient;
    private IPAddress m_IP;
    private Timer m_Timerek;
    private string m_Odesilatel;
    private StatProtocol m_Protokol;
    
    internal UdpClient Klient
    {
        get
        {
            return m_Klient;
        }
    }

    internal IPAddress IP
    {
        get
        {
            return m_IP;
        }
    }

    internal Timer Timerek
    {
        get
        {
            return m_Timerek;
        }
        set
        {
            m_Timerek = value;
        }
    }

    internal string Odesilatel
    {
        get
        {
            return m_Odesilatel;
        }
    }

    internal StatProtocol Protokol
    {
        get
        {
            return m_Protokol;
        }
    }

    internal UdpObjekt(UdpClient klient, IPAddress ip, string odesilatel, StatProtocol protokol)
    {
        m_Klient = klient;
        m_IP = ip;
        m_Odesilatel = odesilatel;
        m_Protokol= protokol;
    }
}
