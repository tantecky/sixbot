using System;
using System.IO;
using System.Xml;
using FindCS.Protocols;
using System.Diagnostics;
using System.Globalization;

namespace FindCS.Hry
{
    internal enum TypHry : byte
    {
        Zadna = 0xFF,
        CS = 0,
        CSS = 1,
        L4D = 2,
        DoD = 3,
        UT2004 = 4,
        CODUO = 5
    }

    /*internal abstract class Hra
    {
        #region statické èleny

        internal static readonly Hra[] Hry = new Hra[]{new CS(), new CSS(), new L4D(), new DoD(), new UT2004(), new CODUO()};
        internal static bool Ulozit;

        internal static void NactiNastaveni(XmlDocument doc)
        {
            foreach (XmlNode uzel in doc["Nastaveni"].SelectNodes("hra"))
            {
                Hra hra = Hry[int.Parse(uzel.Attributes["typ"].Value, CultureInfo.InvariantCulture)];

                XmlNode uzlik = uzel.SelectSingleNode("cesta");

                if (uzlik != null && File.Exists(uzlik.InnerText))
                    hra.Cesta = uzlik.InnerText;

                uzlik = uzel.SelectSingleNode("argumenty");

                if (uzlik != null)
                    hra.Argumenty = uzlik.InnerText;

                uzlik = uzel.SelectSingleNode("priorita");

                if (uzlik != null)
                    hra.Priorita = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), uzlik.InnerText);
            }
        }

        internal static void UlozNastaveni(XmlTextWriter writer)
        {
            foreach (Hra hra in Hry)
            {
                if (hra.Cesta != null)
                {
                    writer.WriteStartElement("hra");

                    writer.WriteAttributeString("typ", ((int)hra.Typ).ToString(CultureInfo.InvariantCulture));

                    writer.WriteStartElement("cesta");
                    writer.WriteValue(hra.Cesta);
                    writer.WriteEndElement();

                    if (hra.Argumenty != null)
                    {
                        writer.WriteStartElement("argumenty");
                        writer.WriteValue(hra.Argumenty);
                        writer.WriteEndElement();
                    }

                    if (hra.Priorita != ProcessPriorityClass.Normal)
                    {
                        writer.WriteStartElement("priorita");
                        writer.WriteValue(hra.Priorita.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
        }

        #endregion

        protected string m_Cesta;
        protected string m_Argumenty;
        protected ProcessPriorityClass m_Priorita;

        internal string Cesta
        {
            get
            {
                return m_Cesta;
            }
            set
            {
                m_Cesta = value;
            }
        }

        internal string Argumenty
        {
            get
            {
                return m_Argumenty;
            }
            set
            {
                m_Argumenty = value;
            }
        }

        internal ProcessPriorityClass Priorita
        {
            get
            {
                return m_Priorita;
            }
            set
            {
                m_Priorita = value;
            }
        }

        internal Hra()
        {
            m_Priorita = ProcessPriorityClass.Normal;
        }

        internal void NastavPrioritu(ProcessPriorityClass priorita)
        {
            m_Priorita = priorita;
            Ulozit = true;
        }

        internal void NastavArgumenty(string argumenty)
        {
            m_Argumenty = argumenty;
            Ulozit = true;
        }

        internal bool NastavCestu(string adresar)
        {
            string cesta = string.Concat(adresar, NazevExace);

            if(File.Exists(cesta))
            {
                m_Cesta = cesta;
                Ulozit = true;

                return true;
            }

            return false;
        }

        internal abstract TypHry Typ { get; }
        internal abstract string NazevExace { get; }
        internal abstract TypProtokolu Protocol { get; }
        internal virtual string PovinnyArgument
        {
            get
            {
                return null;
            }
        }
    }*/
}
