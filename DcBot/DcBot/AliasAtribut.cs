using System;

namespace DcBot
{
    [AttributeUsage( AttributeTargets.Class)]
    internal class AliasAtribut : Attribute
    {
        private string m_Alias;

        public string Alias
        {
            get
            {
                return m_Alias;
            }
        }

        internal AliasAtribut(string alias)
        {
            m_Alias = alias;
        }
    }
}
