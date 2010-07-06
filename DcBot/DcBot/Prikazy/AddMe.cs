using DcBot;

namespace DcBot
{
    [PomocAtribut("pøidá do dota rosteru")]
    class AddMe : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            if (DotaRoster.PridejHrace(odesilatel))
                bot.PrivateZprava(odesilatel, "Byl jsi pøidán do Dota Rosteru (pro zobrazení -dotaroster, pro odebrání -removeme, pro spam na dotu -spamdota)");
        }
    }
}
