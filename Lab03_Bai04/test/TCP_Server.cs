using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace test
{
    public partial class TCP_Server : Form
    {
        TcpClient server = new TcpClient();
        public class Client
        {
            public string UserName;
            public TcpClient tcpClient;
            public Client()
            {
                UserName = "";
                tcpClient = new TcpClient();
            }
        }
        List<Client> MembersList = new List<Client>();

        public TCP_Server()
        {
            InitializeComponent();
        }
                private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn thật sự muốn tắt chương trình ?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
            }
        }
        private void btnListen_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread _ServerThread = new Thread(new ThreadStart(Connect));
            _ServerThread.Start();
            btnListen.Enabled = false;
        }

        //**Thiet Lap Ket Noi**
        public void Connect()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 4040);
            server.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Client.Bind(ip);
            server.Client.Listen(10);
            while (true)
            {
                Client client = new Client();
                client.tcpClient.Client = server.Client.Accept();

                StreamReader Read = new StreamReader(client.tcpClient.GetStream());
                client.UserName = Read.ReadLine();

                MembersList.Add(client);
                lsbMembers.Items.Insert(0, $"New Connect from {client.tcpClient.Client.RemoteEndPoint} || {client.UserName}");

                //Send Thread 
                Thread _thread = new Thread(Send);
                _thread.Start(client);
            }
        }

        //**Gui thong tin**
        public void Send(object o)
        {
            Client client = (Client)o;
            while (client.tcpClient.Connected)
            {
                byte[] sendBytes = new byte[50000];

                try
                {
                    StreamReader sr = new StreamReader(client.tcpClient.GetStream());
                    //string message;
                    //do
                    //{ message = sr.ReadLine(); } while (message == null);
                    Receive(ref sendBytes, client);
                    //nap sendBytes vao 1 ham xu ly du lieu
                    sendBytes = FixData(ref sendBytes);

                    //**gui tra du lieu cho Client**
                    NetworkStream writeStream = client.tcpClient.GetStream();
                    writeStream.Write(sendBytes, 0, sendBytes.Length);
                    writeStream.Flush();
                }
                catch
                {
                    Remove_Client(client);
                    break;
                }

            }
        }
        public void Remove_Client(Client client)
        {
            client.tcpClient.Close();

            //**Cập nhật lại MemberList**
            MembersList.Remove(client);
            lsbMembers.Items.Clear();
            foreach (var Member in MembersList)
            {
                lsbMembers.Items.Add(Member.tcpClient.Client.RemoteEndPoint + " || " + Member.UserName);
            }
        }

        //**Nhan thong tin**
        public void Receive(ref Byte[] ReceiveBytes, Client client)
        {
            NetworkStream readStream = client.tcpClient.GetStream();
            //if (message[0] == '1')
            //{
                readStream.Read(ReceiveBytes, 0, ReceiveBytes.Length);
            //}
            //else if (message[0] == '0')
            //{
            //    Remove_Client(client);
            //}

        }

        //**Ham xu ly du lieu**
        public byte[] FixData(ref byte[] sendBytes)
        {
            string Data = Encoding.UTF8.GetString(sendBytes);
            string temp = RightPlace(ref Data);
            Data = CapitalizeFirst(ref temp);
            return Encoding.UTF8.GetBytes(Data);
        }

        //Dat dung cho
        public string RightPlace(ref string Data)
        {
            StringBuilder sb = new StringBuilder(Data = Data.Trim());
            if (!(Data[Data.Length - 1] == '.' || Data[Data.Length - 1] == '\0' || Data[Data.Length - 1] == '?' || Data[Data.Length - 1] == '!') || Data[Data.Length - 1] == ',' || Data[Data.Length - 1] == ':')
            {
                sb.Append(".");
            }

            sb.Replace(",", ",$$$").Replace(".", ".$$$").Replace("!", "!$$$").Replace("?", "?$$$").Replace(":", ":$$$").Replace(";", ";$$$").Replace("…", "…$$$").Replace("\r\n", "\r\n$$$").Replace("\r", "\r$$$").Replace("\n", "\n$$$").Replace("\0","");
            string[] sentences = sb.ToString().Split("$$$", StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb1 = new StringBuilder();
            for (int i = 0; i < sentences.Length; i++)
            {
                string temp = sentences[i];

                char punc = temp[temp.Length - 1];
                temp = temp.TrimEnd(punc);

                sb1.Append(temp.Trim());
                if (punc == '\n' || punc == '\r')
                {
                    sb1.Append(punc);
                }
                else
                    sb1.AppendFormat("{0} ", punc);
            }
            return sb1.ToString().Trim();
        }
        //Viet hoa
        public string CapitalizeFirst(ref string s)
        {
            bool IsNewSentense = true;
            var result = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (IsNewSentense && char.IsLetter(s[i]))
                {
                    result.Append(char.ToUpper(s[i]));
                    IsNewSentense = false;
                }
                else
                    result.Append(s[i]);

                if (s[i] == '!' || s[i] == '?' || s[i] == '.' || s[i] == '\n')
                {
                    IsNewSentense = true;
                }
            }
            return result.ToString();
        }
    }
}
