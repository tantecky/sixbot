using DcBot;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DcBot
{
    [PomocAtribut("zobrazí tuto nápovìdu")]
    internal class Help : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            StringBuilder sb = new StringBuilder(HandlerPrikazu.Prikazy.Count + 1);
            sb.AppendLine("Seznam dostupných pøíkazù:");

            string alias;

            foreach (KeyValuePair<string, BasePrikaz> polozka in HandlerPrikazu.Prikazy)
            {
                alias = polozka.Value.ZiskejAlias();

                if (alias != null)
                {
                    if (alias != polozka.Key)
                        sb.AppendLine(string.Format("{0}{1}, {2}{3} = {4}", HandlerPrikazu.PrikazPrefix, polozka.Key, HandlerPrikazu.PrikazPrefix, alias, polozka.Value.ZiskejHelp()));
                    else
                        continue;
                }
                else
                    sb.AppendLine(string.Format("{0}{1} = {2}", HandlerPrikazu.PrikazPrefix, polozka.Key, polozka.Value.ZiskejHelp()));
            }

            bot.PrivateZprava(odesilatel, sb.ToString());
        }
    }
}
