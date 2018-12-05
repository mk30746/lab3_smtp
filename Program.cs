using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;

namespace smtp
{
    class Program
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        static void Main(string[] args)
        {
            List<string> message = new List<string>();
            List<string> configs = new List<string>();
            
            string[] SpaceStr = new string[] { " " };
            string[] Separator = new string[] { ":" };
            string[] SplitStr;

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"smtp.config");
            while ((line = file.ReadLine()) != null)
            {
                if (line == "<MESSAGE>")
                {
                    line = file.ReadLine();
                    while (line != "</MESSAGE>")
                    {
                        message.Add(line);
                        line = file.ReadLine();
                    }
                    break;
                }
                SplitStr = line.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                configs.Add(SplitStr[1]);
            }
            file.Close();

            string username = configs[0];
            string password = configs[1];
            string adres = configs[2];
            int port = Convert.ToInt32(configs[3]);
            string target = configs[4];
            string subject = configs[5];

            TcpClient NetStream = new TcpClient();
            NetStream.Connect(adres, port);
            SslStream ssl = new SslStream(NetStream.GetStream());
            ssl.AuthenticateAsClient(adres);
            
            string str = string.Empty;
            string strTemp = string.Empty;
                using (StreamReader StrmRead = new StreamReader(ssl))
                {
                    using (StreamWriter StrmWriter = new StreamWriter(ssl))
                    {
                        StrmWriter.WriteLine("EHLO eltchar");
                        StrmWriter.Flush();

                        do
                        {
                            strTemp = StrmRead.ReadLine();
                            if (strTemp == "." || strTemp.IndexOf("-ERR") != -1)
                            {
                                break;
                            }
                            
                            str = str + strTemp + "\n";
                            Console.WriteLine(strTemp);
                        }
                        while (!strTemp.StartsWith("250 "));



                        StrmWriter.WriteLine("AUTH PLAIN");
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine(Base64Encode(string.Format("\0{0}\0{1}",username,password)));
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine("MAIL FROM:<" + username + ">");
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine("RCPT TO:<" + target + ">");
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine("DATA");
                        StrmWriter.Flush();
                        StrmWriter.WriteLine("DATE: " + DateTime.Today.ToString());
                        StrmWriter.Flush();
                        StrmWriter.WriteLine("FROM: " + username);
                        StrmWriter.Flush();
                        StrmWriter.WriteLine("TO: " + target);
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine("SUBJECT: " + subject);
                        StrmWriter.Flush();
                        for (int i = 0; i < message.Count; i++)
                        {
                            StrmWriter.WriteLine(message[i]);
                            StrmWriter.Flush();
                        }
                        StrmWriter.WriteLine(".");
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                        StrmWriter.WriteLine("QUIT");
                        StrmWriter.Flush();
                        Console.WriteLine(StrmRead.ReadLine());
                    }
                }
            Console.WriteLine("END, press any key");
            Console.ReadLine();
            return;
        }
    }
}
