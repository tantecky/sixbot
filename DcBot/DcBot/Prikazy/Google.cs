using DcBot;
using Gapi.Search;
using System.Net;

namespace DcBot
{
    [PomocAtribut("vygoogluje daný textový øetìzec napø.: -google dota")]
    [AliasAtribut("g")]
    class Google : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Povinny; } }

        internal override byte MinimalniPocetArgumentu { get { return 1; } }

        internal override void PriPouziti(SixBot bot, string odesilatel, string[] argumenty)
        {
            try
            {
                SearchResults vysledky = Searcher.Search(SearchType.Web, string.Join(" ", argumenty));

                if (vysledky != null && vysledky.Items != null && vysledky.Items.Length > 0)
                {
                    bot.ChatZprava(string.Format(@"{0} *** {1} *** {2} *** Dotaz položil: {3}", vysledky.Items[0].Title, vysledky.Items[0].Content, vysledky.Items[0].Url, odesilatel).Replace("<b>", "").Replace("</b>", ""));
                }
            }
            catch (WebException)
            {
            }
        }
    }
}
