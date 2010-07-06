using DcBot;
using System.Threading;
using System;
using Gapi.Search;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using FlowLib.Containers;

namespace DcBot
{
    [PomocAtribut("testovací pøíkaz")]
    [AliasAtribut("e")]
    internal class EchoPrikaz : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            bot.ChatZprava("OoOoOoOo ooOoOooO");
        }
    }
}
