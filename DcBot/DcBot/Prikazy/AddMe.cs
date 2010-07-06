using DcBot;

namespace DcBot
{
    [PomocAtribut("p�id� do dota rosteru")]
    class AddMe : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            if (DotaRoster.PridejHrace(odesilatel))
                bot.PrivateZprava(odesilatel, "Byl jsi p�id�n do Dota Rosteru (pro zobrazen� -dotaroster, pro odebr�n� -removeme, pro spam na dotu -spamdota)");
        }
    }
}
