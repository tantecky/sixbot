using System;

namespace DcBot
{
    [AttributeUsage( AttributeTargets.Class)]
    internal class PomocAtribut : Attribute
    {
        private string m_Popis;

        public string Popis
        {
            get
            {
                return m_Popis;
            }
        }

        internal PomocAtribut(string popis)
        {
            m_Popis = popis;
        }
    }
}
