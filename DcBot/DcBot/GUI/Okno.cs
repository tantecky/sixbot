using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace DcBot
{
    public partial class Okno : Form
    {
        private delegate void VypisCallback(string text);

        private SixBot m_Bot;

        internal SixBot Bot
        {
            get
            {
                return m_Bot;
            }
            set
            {
                m_Bot = value;
            }
        }

        internal Okno()
        {
            InitializeComponent();

            Bounds = new Rectangle(100, 100, 350, 300);

            this.Text = string.Concat("Sixbot", " ", Program.Verze);

            notifyIcon.Text = Text;
        }

        internal void VypisText(string text)
        {
            if (infoBox.InvokeRequired)
            {
                Invoke(new VypisCallback(VypisText), new object[] { text });
            }
            else
            {
                infoBox.AppendText(text);
            }
        }

        internal void VypisRadek(string text)
        {
            if (infoBox.InvokeRequired)
            {
                Invoke(new VypisCallback(VypisRadek), new object[] { string.Concat(text, Environment.NewLine) });
            }
            else
            {
                infoBox.AppendText(text);
            }
        }

        private void Okno_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
        }

        private void Okno_Load(object sender, EventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(SixBot.VytvorInstanciBota), this); //jedem
            Thread vlaknoBota = new Thread(new ParameterizedThreadStart(SixBot.VytvorInstanciBota));
            vlaknoBota.Name = "Sixbot_Thread";
            vlaknoBota.IsBackground = true;
            vlaknoBota.Priority = ThreadPriority.AboveNormal;
            vlaknoBota.Start(this);
        }

        private void Okno_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_Bot != null)
                m_Bot.Dispose(); //uklídíme bordel a ukonèíme Tcp spojení s hubem slušným zpùsobem

            notifyIcon.Dispose();
        }
    }
}