/*using System;
using System.Net;

namespace BnetClient
{
    class Program
    {
        static Client cl = new Client();

        static void OnOnline()
        {
            Console.WriteLine("Online");
        }

        static void OnOffline()
        {
            Console.WriteLine("Offline");
        }

        static void OnEnterChat()
        {
            Console.WriteLine("Vstoupil do chatu");
        }

        static void OnReceive(PacketID id)
        {
            Console.WriteLine("Přijato << " + id.ToString());

            if (id == PacketID.SID_GETADVLISTEX)
            {
                cl.Hry.ZamkniList();

                BnetInfo[] hry = cl.Hry.ZiskejHry();

                if (hry == null)
                {
                    cl.Hry.OdemkniList();
                    return;
                }

                foreach (BnetInfo info in hry)
                {
                    Console.WriteLine(info.ToString());
                }

                cl.Hry.OdemkniList();
            }
        }

        static void OnSend(PacketID id)
        {
            Console.WriteLine("Odesláno >> " + id.ToString());
        }

        static void OnError(string chyba)
        {
            Console.WriteLine("Chyba: " + chyba);
        }


        static void Main(string[] args)
        {
            cl.OnlineUdalost += new ClientUdalostCallback(OnOnline);
            cl.OfflineUdalost += new ClientUdalostCallback(OnOffline);
            cl.VstupDoChatuUdalost += new ClientUdalostCallback(OnEnterChat);

            cl.PrijataPacketaUdalost += new PacketaUdalostCallback(OnReceive);
            cl.OdeslanaPacketaUdalost += new PacketaUdalostCallback(OnSend);

            cl.ChybaUdalost += new ChybaUdalostCallback(OnError);

            cl.NavazSpojeni(new IPEndPoint(IPAddress.Parse("147.33.227.122"), 6112));

            Console.ReadLine();
        }
    }
}*/
