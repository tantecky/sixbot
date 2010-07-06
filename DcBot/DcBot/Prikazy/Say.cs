using DcBot;

namespace DcBot
{
    [PomocAtribut("bot odešle daný textový øetìzec do main chatu: -say Ahoj!")]
    [AliasAtribut("s")]
    class Say : BasePrikaz
    {
        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Nepovinny; } }

        internal override void PriPouziti(SixBot bot, string odesilatel, string[] argumenty)
        {
            bot.ChatZprava(string.Join(HandlerPrikazu.Oddelovac[0], argumenty));
        }
    }
}
