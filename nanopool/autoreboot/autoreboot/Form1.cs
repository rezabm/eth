using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using Utilities;

namespace autoreboot
{
    public partial class Form1 : Form
    {
        Arguments commandLine;

        int secondsInFailure = 0;
        int timeout = 900;

        string worker = string.Empty;

        public Form1(string[] args)
        {
            commandLine = new Arguments(args);
            if (commandLine["worker"] != null)
            {
                worker = commandLine["worker"];
            }
            else 
            {
                MessageBox.Show("Wrong argument.\nUsage: " + Path.GetFileName(Application.ExecutablePath).ToUpper() + " /worker=<workerid>");
                Load += (s, e) => Close();
            }

            if (commandLine["timeout"] != null)
            {
                try {
                    int i = int.Parse(commandLine["timeout"]);
                    timeout = i;
                } catch
                {
                }
            }



            InitializeComponent();
            this.Text = worker;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            WebClient client = new WebClient();

            string json = client.DownloadString("http://eth.nanopool.org/api/hashrate/0xc8fd764ad39e69ecb857ba7466a0e0057c611c32/" + worker);
            NanopoolApiWorker data = JsonConvert.DeserializeObject<NanopoolApiWorker>(json);

            try
            {
                bool s = bool.Parse(data.Status);
                double d = double.Parse(data.Data, NumberStyles.Any, CultureInfo.InvariantCulture);
                if (!s || (d == 0))
                    secondsInFailure++;
                else
                    secondsInFailure = 0;

                labelStatus.Text = "Status: " + (s?"Running":"Stopped");
                labelData.Text = "Hashrate: " + d.ToString() + "MH/s";

                label1.Text = "Restart counter: " + secondsInFailure.ToString() + " seconds (will restart when reaches" + timeout.ToString() + " seconds)";

                if (secondsInFailure > timeout)
                    Restart();


            }
            catch
            {
            }

        }

        private void Restart()
        {
            System.Diagnostics.Process.Start("cmd.exe",  "/c shutdown /r /f /t 0");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Restart();
        }
    }
    public class NanopoolApiWorker
    {

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }


    }
