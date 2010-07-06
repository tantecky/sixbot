using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

namespace DcBot
{
    internal class Program
    {
        internal static readonly string Verze = UrciVerzi();

        [STAThread]
        internal static void Main()
        {
#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
          
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Okno());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Došlo k neošetřené výjimce v aplikace. Aplikace bude vypnuta a chyba zalogováno do txt souboru v adresáři aplikace", "Chyba - Sixbot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ZalogujVyjimku(((Exception)e.ExceptionObject).ToString());
        }

        private static string UrciVerzi()
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            return string.Concat(info.FileMajorPart, ".", info.FileMinorPart).ToString(CultureInfo.InvariantCulture);
        }

        private static void ZalogujVyjimku(string chyba)
        {
            using (StreamWriter writer = new StreamWriter("Crashlog.txt", true))
            {
                writer.WriteLine(String.Empty);
                writer.WriteLine("##########################");
                writer.WriteLine(Environment.OSVersion.ToString());
                writer.WriteLine(Environment.Version.ToString());

                writer.WriteLine(DateTime.Now.ToString("g", DateTimeFormatInfo.InvariantInfo));
                writer.WriteLine(String.Empty);
                writer.WriteLine(chyba);
            }
        }
    }
}