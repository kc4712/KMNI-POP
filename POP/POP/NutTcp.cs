using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace POP
{
    internal class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
    
    internal class NutTcp
    {
        private static string CLASSNAME = "[NutTcp]";
        private Socket mainSock;
        private IPEndPoint ipep;
        private AsyncObject ao;
        private System.Timers.Timer timerKeepAlive;
        private string ip;
        private int port;

        internal delegate void toqueEventCb(double toque, string stat, double angle);
        internal event toqueEventCb sendCb;

        internal NutTcp(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            
            timerKeepAlive = new System.Timers.Timer();
            timerKeepAlive.Interval = 100;
            timerKeepAlive.Elapsed += new ElapsedEventHandler(sendKeepAlive);
            timerKeepAlive.Start();
        }

        internal void Close()
        {
            timerKeepAlive.Elapsed -= new ElapsedEventHandler(sendKeepAlive);
            timerKeepAlive.Stop();
            timerKeepAlive.Close();
            timerKeepAlive.Dispose();
            if (mainSock.Connected)
            {
                Thread.Sleep(200);
                mainSock.Close();
            }
        }
        private void DataReceived(IAsyncResult ar)
        {
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            int received = obj.WorkingSocket.EndReceive(ar);
            string data = Encoding.Default.GetString(obj.Buffer);
            switch(int.Parse(data.Substring(4, 4))){
                case 0002:
                    Console.WriteLine(CLASSNAME + "StartACK:" + data.Substring(4, 4));
                    OnSendData(MakeStr(2));
                    break;
                case 0005:
                    Console.WriteLine(CLASSNAME + "CMD Accepted:" + data.Substring(4, 4));
                    break;
                case 0061:
                    string strTorque = data.Substring(142, 2) + "." + data.Substring(144, 2);
                    string tightState = data.Substring(107, 1);
                    string strangle = data.Substring(169, 5);
                    Console.WriteLine(CLASSNAME + "Last Toque Result:" + strTorque);
                    sendCb?.Invoke(double.Parse(strTorque), tightState, double.Parse(strangle));
                    OnSendData(MakeStr(3));
                    break;
                case 9999:
                    //Console.WriteLine(CLASSNAME + "9999:" + data.Substring(4, 4));
                    break;
                default:
                    Console.WriteLine(CLASSNAME + "DataReceiveError:" + data.Substring(4, 4));
                    break;

            }
            obj.ClearBuffer();
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
        }

        private bool OnSendData(string msg)
        {
            // 서버가 대기중인지 확인한다.
            if (!mainSock.IsBound || !mainSock.Connected)
            {
                return Connect();
            }
            
            // 문자열을 utf8 형식의 바이트로 변환한다.
            byte[] bDts = Encoding.Default.GetBytes(msg);

            // 서버에 전송한다.
            int result = mainSock.Send(bDts);
            if(result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private void sendKeepAlive(object sender, ElapsedEventArgs e)
        {
            if (mainSock != null || mainSock.Connected)
            {
                //Console.WriteLine(CLASSNAME + "KEEP ALIVE");
                OnSendData(MakeStr(4));
            }
            else
            {
                Connect();
            }
        }

        internal bool Connect()
        {
            try {
                bool CONN = false;
                if (mainSock == null || !mainSock.Connected)
                {
                    mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    ao = new AsyncObject(4096);
                    ipep = new IPEndPoint(IPAddress.Parse(ip), port);
                    mainSock.Connect(ipep);
                    //SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    //args.RemoteEndPoint = ipep;
                    //mainSock.ConnectAsync(args);
                    ao.WorkingSocket = mainSock;
                    mainSock.BeginReceive(ao.Buffer, 0, ao.BufferSize, 0, DataReceived, ao);
                    OnSendData(MakeStr(0));
                    CONN = true; 
                }
                return CONN;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(CLASSNAME + "ConnectError:" + ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(CLASSNAME + "ConnectError:" + ex.ToString());
                return false;
            }
        }


        private string MakeStr(int Num_Cmd)
        {

            string sMsg;
            string strLength = "0020";
            string strMid = "0001";
            string strRevision = "000";
            string strAckFlag = "0";
            string strStatinID = "00";
            string strPindleID = "00";
            string strSpare = "0000";
            string strMsgEnd = "\0";

            switch (Num_Cmd)
            {
                case 0: // Start 0020 0001 000 0 00 00 0000 \0
                    strLength = "0020";
                    strMid = "0001";
                    strRevision = "003";
                    break;
                case 1: // Stop 0020 0003 000 0 00 00 0000 \0
                    strLength = "0020";
                    strMid = "0003";
                    strRevision = "001";
                    break;
                case 2: // Torque "0020 0060 000 0 00 00 0000 \0"
                    strLength = "0020";
                    strMid = "0060";
                    strRevision = "001";
                    break;
                case 3: // Torque Ack 0020 0062 001 0 00 00 0000 \0
                    strLength = "0020";
                    strMid = "0062";
                    strRevision = "001";
                    break;
                case 4: // Keep Alive "0020 9999 000 0 00 00 0000 \0"
                    strLength = "0020";
                    strMid = "9999";
                    strRevision = "001";
                    break;
                default:
                    break;
            }

            sMsg = strLength + strMid + strRevision + strAckFlag + strStatinID + strPindleID + strSpare
                    + strMsgEnd;
            return sMsg;
        }
    }
}
