using DcBot;

namespace DcBot
{
    [PomocAtribut("odebere z dota rosteru")]
    class RemoveMe : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            if (DotaRoster.OdeberHrace(odesilatel))
                bot.PrivateZprava(odesilatel, "Byl jsi odebrán z Dota Rosteru (pro pøidání -addme)");
        }
    }
}
