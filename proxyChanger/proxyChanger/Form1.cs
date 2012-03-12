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
        }
        private void addList(string server)
        {
            if (server != "")
            {
                if (listBox1.Items.Contains(server) == false)
                {
                    listBox1.Items.Add(server);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string server;
            try
            {
                RegistryKey rkApp = getRegKey();
                int enable = 1;
                server = textBox1.Text;
                rkApp.SetValue("ProxyEnable", enable, RegistryValueKind.DWord);
                rkApp.SetValue("ProxyServer", server, RegistryValueKind.String);
                refreshIeSetting();
                timer2_Tick(null, null);
                button2.Text = "Complete..";
            }
            catch (Exception)
            {
                server = "";
                button2.Text = "Error..";
            }
            timer1.Start();
            addList(server);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rkApp = getRegKey();
                int enable = 0;
                rkApp.SetValue("ProxyEnable", enable, RegistryValueKind.DWord);
                refreshIeSetting();
                timer2_Tick(null, null);
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
            textBox1.Text = (string)listBox1.SelectedItem;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.Icon1;

            timer1.Interval = 2000;
            timer2.Interval = 1000;
            timer2.Start();
            timer2_Tick(null, null);

            getServerFromUie();
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
                for (int i = 0; i < arr.Length; i += 1)
                {
                    addList(arr[i].Trim());
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
                label2.Text = (enable != 0 ? "[On] " : "[Off] ") + server;
            }
            catch (Exception)
            {
                label2.Text = "Error..";
            }
        }

    }
}
