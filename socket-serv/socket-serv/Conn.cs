using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;


namespace socket_server
{
    class Conn
    {
        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        public bool isUse = false;
        public byte[] readBuff;
        public int buffCount = 0;

        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }
        /// <summary>
        /// 初始化socket
        /// </summary>
        /// <param name="socket"></param>
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }

        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }
        /// <summary>
        /// 获取连接点的IP地址
        /// </summary>
        /// <returns></returns>
        public string GetAdress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }
        /// <summary>
        /// 断开链接
        /// </summary>
        public void Close()
        {
            if (!isUse)
                return;
            Console.WriteLine("[断开链接]" + GetAdress());
            socket.Close();
            isUse = false;
        }
    }
}
