using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace socket_server
{
    class Serv
    {
        public Socket listenfd;
        public Conn[] conns;//新建连接池数组
        public int maxConn = 50;//最大连接数

        public int NewIndex()
        {
            if (conns == null)
                return -1;//连接池数组为空时，返回负1
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();//连接池数组当前位置为空时，新建一个连接池对象
                    return i;//返回值为conns数组的index
                }
                else if (conns[i].isUse == false)
                {
                    return i;//如果当前连接池位置没有被使用时，且不为空时，返回conns数组的index
                }
            }
            return -1;
        }

        public void Start(string host, int port)
        {
            conns = new Conn[maxConn];//新建连接池数组，最大连接数为maxConn
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }//新建50个连接池
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("服务器启动成功");
        }

        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.Write("警告链接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("客户端链接[" + adr + "]conn池ID:" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                int count = conn.socket.EndReceive(ar);
                if (count <= 0)
                {
                    Console.WriteLine("收到[" + conn.GetAdress() + "]断开链接");
                    conn.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                panduan(str);
                Console.WriteLine("收到[" + conn.GetAdress() + "]数据：" + str);
                str = conn.GetAdress() + ":" + str;
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);

                for (int i = 0; i < conns.Length; i++)
                {
                    if (conns[i] == null)
                        continue;
                    if (!conns[i].isUse)
                        continue;
                    Console.WriteLine("将消息传播给" + conns[i].GetAdress());
                    conns[i].socket.Send(bytes);
                }
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb,
                    conn);
            }
            catch (Exception e)
            {
                Console.WriteLine("收到[" + conn.GetAdress() + "]断开链接");
                conn.Close();
            }
        }
        //以下为可修改片段--------------------------------------------------------------------------------------------------------------
        private void panduan(string str)
        {
            String strmy = str;
            str = "";
            if (strmy == "S" || strmy == "s")
            {
                Console.WriteLine("关机");
                GuanjiTime();
            }
            if (strmy == "R" || strmy == "r")
            {
                Console.WriteLine("重启");
                Chongqi();
            }
            if (strmy == "V" || strmy == "v")
            {
                Console.WriteLine("关机");
                BackGuanji();
            }
            if (strmy == "A" || strmy == "a")
            {
                Console.WriteLine("音量+");
                VolumeUp();
            }
            if (strmy == "L" || strmy == "l")
            {
                Console.WriteLine("音量-");
                VolumeDown();
            }
            if (strmy == "M" || strmy == "m")
            {
                Console.WriteLine("静音");
                Mute();
            }
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, UInt32 dwExtraInfo);
        [DllImport("user32.dll")]
        static extern Byte MapVirtualKey(UInt32 uCode, UInt32 uMapType);
        private const byte VK_VOLUME_MUTE = 0xAD;
        private const byte VK_VOLUME_DOWN = 0xAE;
        private const byte VK_VOLUME_UP = 0xAF;
        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const UInt32 KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        const int SW_SHOWMINIMIZED = 2; //{最小化, 激活}  
        const int SW_SHOWMAXIMIZED = 3;//最大化   
        const int SW_SHOWRESTORE = 1;//还原
        public static void VolumeUp()
        {
            //strmy = "";
            keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_VOLUME_UP, MapVirtualKey(VK_VOLUME_UP, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static void VolumeDown()
        {
            //strmy = "";
            keybd_event(VK_VOLUME_DOWN, MapVirtualKey(VK_VOLUME_DOWN, 0), KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_VOLUME_DOWN, MapVirtualKey(VK_VOLUME_DOWN, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static void Mute()
        {
            //strmy = "";
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }


        public static void Zuixiaohua()
        {
            ShowWindow(GetForegroundWindow(), 2);//最小化  
        }

        

        public static void GuanjiTime()
        {
            //strmy = "";
            System.Diagnostics.Process.Start("cmd.exe", "/cshutdown -s -t 2");
        }

        public static void Chongqi()
        {
            //strmy = "";
            System.Diagnostics.Process.Start("cmd.exe", "/cshutdown -r");
        }


        public static void BackGuanji()
        {
            //strmy = "";
            System.Diagnostics.Process.Start("cmd.exe", "/cshutdown -a");
        }
    }
}


