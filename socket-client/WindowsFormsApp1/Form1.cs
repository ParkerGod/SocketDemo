using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Socket clientsocket;
        public String ip= "192.168.1.105";//这里设置socket服务器端IP地址
        public String recvStr;
        Socket socket;
        const int BUFFER_SIZE = 1024;
        public byte[] readBuff = new byte[BUFFER_SIZE];
        //链接
        public void Connetion(String ip)
        {

            //Socket
            socket = new Socket(AddressFamily.InterNetwork,
                             SocketType.Stream, ProtocolType.Tcp);
            //Connect
            string host = ip;
            int port = 88;
            socket.Connect(host, port);
            //clientText.text = "客户端地址 " + socket.LocalEndPoint.ToString();
            //Recv
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        //接收回调
        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                //count是接收数据的大小
                int count = socket.EndReceive(ar);
                //数据处理
                string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
                if (recvStr.Length > 300) recvStr = "";
                recvStr += str + "\n";
                //继续接收  
                socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
            }
            catch (Exception e)
            {
                //recvText.text += "链接已断开";
                socket.Close();
            }
        }

        //发送数据
        public void Send(String a)
        {
            string str = a;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            try
            {
                socket.Send(bytes);
            }
            catch { }
        }

        //音量+
        private void button1_Click(object sender, EventArgs e)
        {
            Connetion(ip);
            Send("a");
        }
        //音量-
        private void button2_Click(object sender, EventArgs e)
        {
            Connetion(ip);
            Send("l");
        }
        //静音
        private void button3_Click(object sender, EventArgs e)
        {
            Connetion(ip);
            Send("m");
        }
    }
}
