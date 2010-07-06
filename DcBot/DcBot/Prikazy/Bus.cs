using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using System.Web;

namespace DcBot
{
    [PomocAtribut("vypíše čas odjezdu několika nejbližších autobusů - chyby v řádu mi prosím nahlašte (Sixkillers)")]
    [AliasAtribut("b")]
    internal class Bus : BasePrikaz
    {
        private static readonly List<Autobus> m_Autobusy = new List<Autobus>(500);
        private const int m_VypisAutobusu = 5;

        private static readonly Regex m_DatumReg = new Regex(@"\<th\>\d{1,2}\.\d{1,2}\.\d+", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_HodinaReg = new Regex(@"\<td\>\d{1,2}", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_MinutyReg = new Regex(@"\<span\>\d{2,2}", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_ChybaReg = new Regex(@"linku a datum nelze", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_ViewStateReg = new Regex("__VIEWSTATE\" value=\".+\"", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex m_EventValidationReg = new Regex("__EVENTVALIDATION\" value=\".+\"", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Comparison<Autobus> m_Porovnavac = new Comparison<Autobus>(PorovnejAutobusy);


        private static readonly Linka[] m_Linky = 
        {
            new Linka("http://idos.dpp.cz/IDOS/ZjrForm.aspx?tt=pid", 177, SmerAutobusu.Opatov),
            new Linka("http://idos.dpp.cz/IDOS/ZjrForm.aspx?tt=pid", 177, SmerAutobusu.Chodov),
            new Linka("http://idos.dpp.cz/IDOS/ZjrForm.aspx?tt=pid", 197, SmerAutobusu.Chodov)
        };

        private Timer m_Aktualizace;
        private SixBot m_Bot;

        internal override TypArgumentu PodporovaneArgumenty { get { return TypArgumentu.Zadny; } }

        internal Bus(SixBot bot)
        {
            m_Bot = bot;

            foreach (Linka linka in m_Linky)
            {
                NactiJizdniRadLinky(linka, DateTime.Now);
            }

            m_Aktualizace = new Timer(ProvedAktualizace, null, TimeSpan.FromSeconds(15.0), TimeSpan.FromHours(3.0));
        }

        internal override void PriPouziti(SixBot bot, string odesilatel)
        {
            StringBuilder sb = new StringBuilder(m_VypisAutobusu + 3);

            sb.AppendLine();

            lock (m_Autobusy)
            {
                for (int i = 0; i < m_Autobusy.Count; i++)
                {
                    if (m_Autobusy[i].Odjezd > DateTime.Now)
                    {
                        for (int j = 0; j < m_VypisAutobusu; j++)
                        {
                            sb.AppendLine(m_Autobusy[i+j].ToString());
                        }

                        break;
                    }
                }

                sb.AppendLine();
                sb.AppendLine("Počet spojů v databázi: " + m_Autobusy.Count);
            }

            bot.PrivateZprava(odesilatel, sb.ToString());
        }

        private void ProvedAktualizace(object sender)
        {
            SmazStareRady();

            DateTime zitrek = DateTime.Now.AddDays(1.0);

            foreach (Linka linka in m_Linky)
            {
                if (!MamRadNaDen(zitrek, linka))
                    NactiJizdniRadLinky(linka, zitrek);
            }

            lock (m_Autobusy)
                m_Autobusy.Sort(m_Porovnavac);
        }

        private void SmazStareRady()
        {
            lock (m_Autobusy)
            {
                for (int i = 0; i < m_Autobusy.Count; i++)
                {
                    if (m_Autobusy[i].Odjezd < DateTime.Now)
                    {
                        m_Autobusy.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private bool MamRadNaDen(DateTime den, Linka linka)
        {
            lock (m_Autobusy)
            {
                foreach (Autobus bus in m_Autobusy)
                {
                    if (StejneDatum(den, bus.Odjezd) && bus.Cislo == linka.Cislo && bus.Smer == linka.Smer) 
                        return true;
                }

                return false;
            }
        }

        private static int PorovnejAutobusy(Autobus x, Autobus y)
        {
            if (x.Odjezd < y.Odjezd)
                return -1;
            else
                return 1;
        }

        private bool StejneDatum(DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day;
        }

        private void NactiJizdniRadLinky(Linka linka, DateTime den)
        {
            WebResponse odezva = null;
            HttpWebRequest pozadavek;
            string html;

            try
            {
                pozadavek = WebRequest.Create(linka.Url) as HttpWebRequest;
                odezva = pozadavek.GetResponse();

                using (StreamReader sr = new StreamReader(odezva.GetResponseStream()))
                {
                    html = sr.ReadToEnd();
                }

                odezva.Close();

                string viewState = m_ViewStateReg.Match(html).Value.Replace("__VIEWSTATE\" value=\"", String.Empty).Replace("\"", String.Empty);

                string eventValidation = m_EventValidationReg.Match(html).Value.Replace("__EVENTVALIDATION\" value=\"", String.Empty).Replace("\"", String.Empty);

                pozadavek = WebRequest.Create(linka.Url) as HttpWebRequest;
                pozadavek.Method = "POST";
                pozadavek.ContentType = "application/x-www-form-urlencoded";
                pozadavek.Referer = linka.Url;

                //__EVENTTARGET=&
                //__EVENTARGUMENT=&
                //__VIEWSTATE=%2FwEPDwUKLTM0MjczMjkyOA8WBh4CdHQFA3BpZB4CY2wFAUMeB1RUSW5kZXgCARYCAgIPZBYWZg8PFgIeBFRleHQFBUxpbmthZGQCAg8QDxYCHgdWaXNpYmxlaGQQFQAVABQrAwAWAGQCBA8PFgIfAwUFT2RrdWRkZAIGDxAPFgIfBGhkEBUAFQAUKwMAFgBkAggPDxYCHwMFA0thbWRkAgoPEA8WAh8EaGQQFQAVABQrAwAWAGQCDQ8QDxYCHwMFDGNlbMO9IHTDvWRlbmRkZGQCDg8WAh8EaBYCZg9kFgICAQ9kFgICAg9kFgZmD2QWCGYPZBYCZg8QDxYCHgdDaGVja2VkZ2RkZGQCAg9kFgJmDxAPFgIfBWdkZGRkAgQPZBYCZg8QDxYCHwVnZGRkZAIGD2QWAmYPEA8WAh8FZ2RkZGQCAg9kFghmD2QWAmYPEA8WAh8FZ2RkZGQCAg9kFgJmDxAPFgIfBWdkZGRkAgQPZBYCZg8QDxYCHwVnZGRkZAIGD2QWAmYPEA8WAh8FZ2RkZGQCBA9kFghmD2QWAmYPEA8WAh8FZ2RkZGQCAg9kFgJmDxAPFgIfBWdkZGRkAgQPZBYCZg8QDxYCHwVnZGRkZAIGD2QWAmYPEA8WAh8FZ2RkZGQCDw8WAh4FdmFsdWUFCHZ5aGxlZGF0ZAIQDw8WAh8DBRV6dm9saXQgb3BhxI1uw70gc23Em3JkZAIRDw8WAh8DBRhwb2Ryb2Juw6kgdnlobGVkw6F2w6Fuw61kZBgBBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WAQUMY2hrV2hvbGVXZWVr%2FG0%2BseXxiF9jK0XCJwF%2BcNOjtt8%3D&
                //txtLine=177&
                //txtFrom=Volha&
                //txtTo=Opatov&
                //txtDate=23.3.2010&
                //cmdSearch=vyhledat&
                //__EVENTVALIDATION=%2FwEWCQKwp%2FmUBgK8hKCFCgKvzsotAoX7%2BrELAsSEqIMOAqXYht4HAtvG9%2FUHAv6N82gCwbKFrgci%2BI2kXhcY%2FYzgGXJ1POBCPQWvbg%3D%3D
                StringBuilder sb = new StringBuilder(9);
                sb.Append("__EVENTTARGET=&");
                sb.Append("__EVENTARGUMENT=&");
                sb.Append("__VIEWSTATE=" + HttpUtility.UrlEncode(viewState) + "&");
                sb.Append("txtLine=" + linka.Cislo + "&");
                sb.Append("txtFrom=Volha&");
                sb.Append("txtTo=" + linka.Smer.ToString() + "&");
                sb.Append("txtDate=" + den.ToShortDateString() + "&");
                sb.Append("cmdSearch=vyhledat&");
                sb.Append("__EVENTVALIDATION=" + HttpUtility.UrlEncode(eventValidation) + "&");

                string postData = sb.ToString();
                pozadavek.ContentLength = postData.Length;

                using (StreamWriter sw = new StreamWriter(pozadavek.GetRequestStream()))
                {
                    sw.Write(postData);
                } 

                odezva = pozadavek.GetResponse();

                using (StreamReader sr = new StreamReader(odezva.GetResponseStream()))
                {
                    html = sr.ReadToEnd();
                }

                if (m_ChybaReg.Match(html).Success)
                {
                    //linka nejezdí o víkendu
                    return;
                }

                Match datum = m_DatumReg.Match(html);

                if (!datum.Success)
                {
                    m_Bot.Gui.VypisRadek("Datum nenalezen");
                    return;
                }

                if (!StejneDatum(DateTime.Parse(datum.Value.Replace("<th>", String.Empty)), den))
                {
                    m_Bot.Gui.VypisRadek("Bus nesedí datumy");
                    return;
                }

                MatchCollection hodiny = m_HodinaReg.Matches(html, datum.Index + datum.Length);

                MatchCollection minuty = m_MinutyReg.Matches(html, datum.Index + datum.Length);

                lock (m_Autobusy)
                {
                    for (int i = 0; i < hodiny.Count; i++)
                    {
                        foreach (Match minuta in minuty)
                        {
                            if (i != hodiny.Count - 1 && minuta.Index > hodiny[i].Index && minuta.Index < hodiny[i + 1].Index)
                            {
                                m_Autobusy.Add(new Autobus(linka.Cislo, new DateTime(den.Year, den.Month, den.Day, int.Parse(hodiny[i].Value.Replace("<td>", String.Empty)), int.Parse(minuta.Value.Replace("<span>", String.Empty)), 0), linka.Smer));
                            }
                            else if (i == hodiny.Count - 1 && minuta.Index > hodiny[i].Index)
                            {
                                m_Autobusy.Add(new Autobus(linka.Cislo, new DateTime(den.Year, den.Month, den.Day, int.Parse(hodiny[i].Value.Replace("<td>", String.Empty)), int.Parse(minuta.Value.Replace("<span>", String.Empty)), 0), linka.Smer));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_Bot.Gui.VypisRadek(ex.ToString());
            }
            finally
            {
                if (odezva != null)
                    odezva.Close();
            }
        }

    }
}
