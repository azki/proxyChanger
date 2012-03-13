using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.IO;

namespace proxyChanger
{
    public partial class Form1 : Form
    {
        private Color defaultColor;
        private string[] serverList = new string[1024];
        private string[] expsList = new string[1024];
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        bool settingsReturn, refreshReturn;

        private RegistryKey getRegKey()
        {
            return Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
        }
        private void refreshIeSetting()
        {
            settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            timer2_Tick(null, null);
        }
        private void addList(string title, string server, string exps)
        {
            if (server != "")
            {
                for (int i = 0; i < listBox1.Items.Count; i += 1)
                {
                    if (this.serverList[i] == server && this.expsList[i] == exps)
                    {
                        return;
                    }
                }
                listBox1.Items.Add(title);
                this.serverList[listBox1.Items.Count - 1] = server;
                this.expsList[listBox1.Items.Count - 1] = exps;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string server;
            string exps;
            try
            {
                RegistryKey rkApp = getRegKey();
                int enable = 1;
                server = textBox1.Text;
                exps = textBox2.Text;
                rkApp.SetValue("ProxyEnable", enable, RegistryValueKind.DWord);
                rkApp.SetValue("ProxyServer", server, RegistryValueKind.String);
                rkApp.SetValue("ProxyOverride", exps, RegistryValueKind.String);
                refreshIeSetting();
                button2.Text = "Complete..";
            }
            catch (Exception)
            {
                server = "";
                exps = "";
                button2.Text = "Error..";
            }
            timer1.Start();
            DateTime now = DateTime.Now;
            addList(now.Hour + ":" + now.Minute + ":" + now.Second, server, exps);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rkApp = getRegKey();
                int enable = 0;
                rkApp.SetValue("ProxyEnable", enable, RegistryValueKind.DWord);
                refreshIeSetting();
                button3.Text = "Complete..";
            }
            catch (Exception)
            {
                button3.Text = "Error..";
            }
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            button2.Text = "On";
            button3.Text = "Off";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = this.serverList[listBox1.SelectedIndex];
            textBox2.Text = this.expsList[listBox1.SelectedIndex];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.Icon1;
            this.defaultColor = label2.ForeColor;

            timer1.Interval = 2000;
            timer2.Interval = 1000;
            timer2.Start();
            timer2_Tick(null, null);

            getServerFromUie();
            button2.Focus();
        }
        private void getServerFromUie()
        {
            try
            {
                WebClient wc = new WebClient();
                byte[] data = wc.DownloadData("http://uie.daum.net/azki/proxyChanger.txt");
                System.Text.UTF8Encoding encoder = new UTF8Encoding();
                string serverText = encoder.GetString(data);
                string[] arr = serverText.Split('\n');
                for (int i = 2; i < arr.Length; i += 3)
                {
                    addList(arr[i - 2].Trim(), arr[i - 1].Trim(), arr[i].Trim());
                }
            }
            catch (Exception) { }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rkApp = getRegKey();
                int enable = (int)rkApp.GetValue("ProxyEnable", 0);
                string server = (string)rkApp.GetValue("ProxyServer", "");
                string exps = (string)rkApp.GetValue("ProxyOverride", "");
                label2.Text = (enable != 0 ? "[On] " : "[Off] ") + server;
                label7.Text = exps;
                if (enable != 0)
                {
                    label2.ForeColor = Color.Red;
                    label7.ForeColor = Color.Red;
                }
                else
                {
                    label2.ForeColor = this.defaultColor;
                    label7.ForeColor = this.defaultColor;
                }
            }
            catch (Exception)
            {
                label2.Text = "Error..";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
