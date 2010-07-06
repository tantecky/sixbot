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
    [PomocAtribut("zobrazí informace o programu")]
    class AboutPrikaz : BasePrikaz
    {
        internal readonly DateTime m_Start;

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal AboutPrikaz()
        {
            m_Start = DateTime.Now;
        }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            TimeSpan t = DateTime.Now - m_Start;

            bot.PrivateZprava(odesilatel, "Informace o programu:\nVerze: " + Program.Verze.ToString() + string.Format("\nUptime: {0} dnù {1} hodin", t.Days, t.Hours) + "\n\nVyužívá projekty:\n" + "FlowLib: http://code.google.com/p/flowlib\n" + "Gapi.NET: http://www.codeplex.com/GAPIdotNET");

        }
    }
}
