using Advantech.Adam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace POP
{
    internal class ADAM6050
    {
        private static string CLASSNAME = "[ADAM6050]";
        private string ip;
        private string port;

        private AdamSocket mAdamSocket;
        public Adam6000Type m_Adam6000Type;

        //private Adam6000Type m_Adam6000Type;

        private int m_iDoTotal, m_iDiTotal;

        private System.Timers.Timer timerKeepAlive;

        internal ADAM6050(string ip, string port, int DI, int DO)
        {
            mAdamSocket = new AdamSocket();
            this.ip = ip;
            this.port = port;
            m_iDiTotal = DI;
            m_iDoTotal = DO;
            timerKeepAlive = new System.Timers.Timer();
            mAdamSocket.SetTimeout(0, 0, 0); // set timeout for TCP
            m_Adam6000Type = Adam6000Type.Adam6050; //아무역할이 없는데 왜 쓰셨을까?
            timerKeepAlive.Interval = 100;
            timerKeepAlive.Elapsed += new ElapsedEventHandler(sendKeepAlive);
            timerKeepAlive.Start();
        }

        internal void Close()
        {

            if (mAdamSocket.Connected)
            {
                mAdamSocket.Disconnect();
            }
            timerKeepAlive.Stop();
            timerKeepAlive.Dispose();
        }

        private bool Connect()
        {
            
            //m_Adam6000Type = Adam6000Type.Adam6050; // the sample is for ADAM-6050
            return mAdamSocket.Connect(ip, ProtocolType.Tcp, Convert.ToInt32(port)); 
        }

        //bData 0~14 AI 1~15;
        internal bool[] RefreshDIO()
        {
            int iDiStart = 1, iDoStart = 17;
            int iChTotal;
            bool[] bDiData, bDoData, bData = new bool[1];//1          //12
            if (mAdamSocket.Modbus().ReadCoilStatus(iDiStart, m_iDiTotal, out bDiData) &&
                //18         //6
                mAdamSocket.Modbus().ReadCoilStatus(iDoStart, m_iDoTotal, out bDoData))
            {            //    12           6
                iChTotal = m_iDiTotal + m_iDoTotal;
                bData = new bool[iChTotal]; //18
                                                   //12
                Array.Copy(bDiData, 0, bData, 0, m_iDiTotal);
                                                //12       //6
                Array.Copy(bDoData, 0, bData, m_iDiTotal, m_iDoTotal);
                return bData;
            }
            System.GC.Collect();
            //System.GC.Collect();
            return bData;
        }

        internal bool SetDO(int i_iCh, int value)
        {
            int iStart = 17 + i_iCh - m_iDiTotal;
            bool result = false;
            //17 + 12 - 11 =  18  1 or 0
            result = mAdamSocket.Modbus().ForceSingleCoil(iStart, value);
            if(result)
            {
                RefreshDIO();
                return result;
            }
            else
            {
                return result;
            }
        }
        
        private void sendKeepAlive(object sender, ElapsedEventArgs e)
        {
            if (mAdamSocket != null || !mAdamSocket.Connected)
            {
                mAdamSocket.Disconnect();
                Thread.Sleep(200);
                Connect();
            }
        }
    }
}
