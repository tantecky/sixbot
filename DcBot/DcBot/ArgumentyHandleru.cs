namespace DcBot
{
    internal class ArgumentyHandleru
    {
        private string m_Text;
        private string m_Odesilatel;
        private SixBot m_Bot;

        internal string Text
        {
            get
            {
                return m_Text;
            }
        }

        internal string Odesilatel
        {
            get
            {
                return m_Odesilatel;
            }
        }

        internal SixBot Bot
        {
            get
            {
                return m_Bot;
            }
        }

        internal ArgumentyHandleru(string text, string odesilatel, SixBot bot)
        {
            m_Text = text;
            m_Odesilatel = odesilatel;
            m_Bot = bot;
        }
    }
}
