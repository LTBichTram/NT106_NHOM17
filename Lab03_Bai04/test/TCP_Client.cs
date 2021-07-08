using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Xceed.Words.NET;

namespace test
{
    public partial class TCP_Client : Form
    {
        TcpClient tcpClient = new TcpClient();
        NetworkStream readStream;
        NetworkStream writeStream;
        Thread _Thread;

        //UserName mặc định
        string UserName = "Anonymous";
        public TCP_Client()
        {
            InitializeComponent();
        }
        //**Hàm xử lý khi tắt Client**

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
                Send();
            else
            {
                MessageBox.Show("Không có kết nối!", "WARNING", MessageBoxButtons.OK);
                return;
            }
            rtxbDataSend.Clear();
        }
        //**Thực thi nút Disconnect**
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }
        //**Thực thi nút Start**
        private void btnStart_Click(object sender, EventArgs e)
        {
            rtxbDataSend.Enabled = true;
            CheckForIllegalCrossThreadCalls = false;

            _Thread = new Thread(new ThreadStart(ClientThread));
            _Thread.Start();

            //Giau txbUsername
            txbUserName.Enabled = false;

            //Giau nut Start
            btnStart.Enabled = false;
            btnStart.Visible = false;

            //Hien thi nut Disconnect
            btnDisconnect.Enabled = true;
            btnDisconnect.Visible = true;
        }

        private void TCP_Client_Load(object sender, EventArgs e)
        {
            rtxbDataSend.Enabled = false;
            btnDisconnect.Visible = false;
            btnDisconnect.Enabled = false;
        }

        //**Gui khi nhan phim Enter**
        private void rtxbMessage_KeyUp(object sender, KeyEventArgs e)
        { }
        public void ClientThread()
        {
            Connect();
            while (tcpClient.Connected)
            {
                Receive();
            }
        }

        //**Nhan Du Lieu Tu Server**
        public void Receive()
        {
            byte[] ReceiveBytes = new byte[50000];
            NetworkStream networkStream = tcpClient.GetStream();
            NetworkStream readStream = networkStream;
            string Message = null;

            readStream.Read(ReceiveBytes, 0, ReceiveBytes.Length);
            Message = Encoding.UTF8.GetString(ReceiveBytes);

            rtxbDataReceive.Text = Message;
        }

        //**Gui Du Lieu**
        public void Send()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            FileStream fs;

            if (sfd.FileName != "")
            {
                fs = new FileStream(sfd.FileName, FileMode.Create);
            }
            else
            {
                MessageBox.Show("Chon File!", "WARNING", MessageBoxButtons.OK);
                return;
            }
            using (StreamWriter sw = new StreamWriter(fs))
            {
                string output = rtxbDataSend.Text;
                sw.Flush();
                sw.Write(output);
            }
            if (fs != null)
            {
                MessageBox.Show("Da Ghi Thanh Cong", "Thanh Cong", MessageBoxButtons.OK);
            }
            fs.Close();
            //writeStream = tcpClient.GetStream();
            //string Send_Message = "1@" + UserName;
            //StreamWriter sw_send = new StreamWriter(tcpClient.GetStream());
            //sw_send.WriteLine(Send_Message);
            tcpClient.Client.SendFile(sfd.FileName);
        }

        //**Thiet Lap Ket Noi**
        public void Connect()
        {
            if (txbUserName.Text.Length > 0)
            {
                UserName = txbUserName.Text;
            }
            else
            {
                DialogResult m = MessageBox.Show("Gửi với định danh là Anonymous?", "WARNING", MessageBoxButtons.YesNo);
                if(m == DialogResult.Yes)
                {
                    txbUserName.Text = UserName;
                }
                else if(m == DialogResult.No)
                {
                    return;
                }
            }
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 4040);
            readStream = tcpClient.GetStream();
            writeStream = tcpClient.GetStream();
            StreamWriter sw = new StreamWriter(tcpClient.GetStream());
            sw.WriteLine(UserName);
            sw.Flush();
        }

        //**Dong Ket Noi**
        public void Disconnect()
        {
            //thong bao dong ket noi cho Server
            string Disconnect_Message = "0@" + UserName;
            StreamWriter sw = new StreamWriter(tcpClient.GetStream());
            sw.WriteLine(Disconnect_Message);

            readStream.Close();
            writeStream.Close();
            tcpClient.Close();
            btnDisconnect.Enabled = false;
        }

        //**Luu tru du lieu da duoc chinh sua tu Server**
        //Dua thong tin file da luu qua lsbSaveFfile
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            FileStream fs;
            if (sfd.FileName == "")
            {
                MessageBox.Show("Chon file", "WARNING", MessageBoxButtons.OK);
                return;
            }
            else
            {
                fs = new FileStream(sfd.FileName, FileMode.Create);
            }

            DialogResult m = MessageBox.Show("Bạn có muốn thực hiện lưu file theo định dạng Word \n Nếu chọn không mặc định sẽ là file text", "WARNING", MessageBoxButtons.YesNo);
            if (m == DialogResult.Yes)
            {
                var doc = DocX.Create(fs);
                doc.InsertParagraph(rtxbDataReceive.Text);
                doc.Save();
            }
            else if (m == DialogResult.No)
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string output = rtxbDataReceive.Text;
                    sw.Flush();
                    sw.Write(output);
                }
            }
            fs.Close();
            lsbSaveFile.Items.Add(sfd.FileName);
        }

        //**Xoa noi dung tren textbox**
        private void btnClear_Click(object sender, EventArgs e)
        {
            switch (MessageBox.Show("Xoa tat ca", "WARNING", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    rtxbDataReceive.Clear();
                    rtxbDataSend.Clear();
                    break;
                case DialogResult.No:
                    break;
            }
        }

        //**Mo file da luu**
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (lsbSaveFile.SelectedItem == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();
                try
                {
                    FileStream fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate);
                    var doc = DocX.Load(fs);
                    rtxbDataSend.Text = doc.Text;
                    fs.Close();
                }
                catch (Exception)
                {
                    try
                    {
                        FileStream fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate);
                        StreamReader sr = new StreamReader(fs);
                        rtxbDataSend.Text = sr.ReadToEnd();
                        fs.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Không thể mở thư mục!", "Thông báo", MessageBoxButtons.OK);
                    }
                }     

            }
            else
            {
                string curItem = lsbSaveFile.SelectedItem.ToString();
                try
                {
                    FileStream fs = new FileStream(curItem, FileMode.Open);
                    var doc = DocX.Load(fs);
                    rtxbDataSend.Text = doc.Text;
                    fs.Close();
                }
                catch (Exception)
                {
                    try
                    {
                        FileStream fs = new FileStream(curItem, FileMode.Open);
                        StreamReader sr = new StreamReader(fs);
                        string content = sr.ReadToEnd();
                        rtxbDataSend.Text = content;
                        fs.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Không thể mở thư mục!", "Thông báo", MessageBoxButtons.OK);
                    }
                } //đù cách này cũng ko đc rồi, thôi không sao đâu ông, mà mình vẫn đọc file word được đúng không ông, Ừm đọc đc ghi đc á
                // chỉ là lưu có vấn đề đúng hem ông, lưu đc luôn mà
                //vậy ổn rùi, Thôi Trâm Nghỉ đi, tui check lại code sáng tui gửi bản hoàn chỉnh nhất nha, hay ông nghỉ luôn đi
                // xong mai dạy sáng làm trên máy tui cho dễ check nè, Ừm giờ Trâm push nó lên github giúp tui nha, oki khoa, tui out luôn á
                //sao khoa không cop á, để nó có version control cho dễ á, mai mình sửa gì còn biết đường quay về á, bản này cũng gần hoàn chỉnh rồi
                //oki ông
            }

        }
        //**Xoa file da luu**
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lsbSaveFile.SelectedItem == null)
            {
                MessageBox.Show("Chọn file để xoá", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                string filename = lsbSaveFile.SelectedItem.ToString();
                File.Delete(filename);
                lsbSaveFile.Items.Remove(filename);
            }
        }

        private void lsbSaveFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y > lsbSaveFile.ItemHeight * lsbSaveFile.Items.Count)
                lsbSaveFile.SelectedItems.Clear();
        }

        private void TCP_Client_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            if (tcpClient.Connected)
            {
                readStream.Close();
                writeStream.Close();
                tcpClient.Close();
            }
            _Thread.Abort();
        }
    }
}

