using System;
using System.Reflection;

namespace DcBot
{
    enum Opraveni : byte
    {
        Guest,
        Admin
    }

    internal abstract class BasePrikaz : IDisposable
    {
        protected bool Disposed = false;

        internal abstract TypArgumentu PodporovaneArgumenty { get; }

        internal virtual Opraveni Opraveni { get { return Opraveni.Guest; } }

        internal virtual byte MinimalniPocetArgumentu { get { return 0; } }

        internal virtual void PriPouziti(SixBot bot, string odesilatel)
        {
        }

        internal virtual void PriPouziti(SixBot bot, string odesilatel, string[] argumenty)
        {
        }

        internal string ZiskejHelp()
        {
            object[] atributy = this.GetType().GetCustomAttributes(typeof(PomocAtribut), true);

            if (atributy == null || atributy.Length == 0)
                return "<nedefinováno>";

            return ((PomocAtribut)atributy[0]).Popis;
        }

        internal string ZiskejAlias()
        {
            object[] atributy = this.GetType().GetCustomAttributes(typeof(AliasAtribut), true);

            if (atributy == null || atributy.Length == 0)
                return null;

            return ((AliasAtribut)atributy[0]).Alias;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            Disposed = true;
        }

        #endregion
    }
}
