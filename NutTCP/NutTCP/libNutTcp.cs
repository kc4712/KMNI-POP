using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Timers;

    public class libNutTcp
    {
        string m_strIP;
        int m_nPort;
        bool toolTorqueFlag = false;
        bool keepAliveFlag = false;
        bool serverStart = true;
        bool keepAliveStart = true;
        Socket server;
        IPEndPoint ipep;

        System.Timers.Timer timerKeepAlive;
        Thread thTorqueMonitor;

        double torque = -1;
        double angle = -1;
        string status = "";


        public delegate void toqueEventCb(double toque, string stat, double angle);
        public event toqueEventCb sendCb;

        public libNutTcp(string ip, int port)
        {
            m_strIP = ip;
            m_nPort = port;
            timerKeepAlive = new System.Timers.Timer();
            timerKeepAlive.Interval = 3000;
            timerKeepAlive.Elapsed += new ElapsedEventHandler(sendKeepAlive);
            thTorqueMonitor = new Thread(connect);
        }

        public void start()
        {
            timerKeepAlive.Start();
            thTorqueMonitor.Start();
        }

        public void end()
        {
            serverStart = false;

            keepAliveStart = false;
            keepAliveFlag = false;

            //Thread.Sleep(2000);

            thTorqueMonitor.Abort();
            timerKeepAlive.Stop();

            if (server.Connected)
            {
                server.Close();
            }


        }

        public void connect()
        {
            try
            {
                while (serverStart)
                {
                    // 접속 상태를 유지
                    if (server == null || server.Connected == false)
                    {
                        try
                        {

                            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                            ipep = new IPEndPoint(IPAddress.Parse(m_strIP), m_nPort);

                            // connect
                            server.Connect(ipep);


                            String szData = "00200001000000000000\0";

                            // Convert String Command to Byte Array
                            byte[] byData = byData = Encoding.Default.GetBytes(szData);

                            int sent = server.Send(byData, SocketFlags.None);

                            byte[] bytesFrom = new byte[255];
                            //  string bytesSent = server.Send(
                            int iRx = server.Receive(bytesFrom);

                            string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                            String resultchk = dataFromClient.Substring(4, 4).ToString();

                            // Response Communication start Ack
                            if (resultchk == "0002")
                            {
                                toolTorqueFlag = false;

                                //LogMsg.CMsg.Show("Tool State : 0002 -", m_strIP, "Communication Start", false, true);

                            }

                        }
                        catch (Exception ex)
                        {
                            //LogMsg.CMsg.Show("Exception", "", ex.ToString(), false, true);
                        }
                    }
                    else
                    {
                        // 접속이 끊어질 때 다시 요청
                        if (toolTorqueFlag == false)
                        {
                            sendTorqueResult();
                            toolTorqueFlag = true;
                            keepAliveFlag = true;
                        }
                        else
                        {
                            receiveTorque();

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //LogMsg.CMsg.Show("Exception", "", ex.ToString(), false, true);
                keepAliveFlag = false;
            }
        }

        public void sendTorqueResult()
        {
            String szData = "00200060000000000000\0";//Request Torque data 
            byte[] byData = Encoding.Default.GetBytes(szData);
            int sent = 0;

            try
            {

                sent = server.Send(byData, SocketFlags.None);

                //LogMsg.CMsg.Show(" State : 0060 -", "Send Torque-", "", false, true);
            }
            catch (Exception ex)
            {
                //LogMsg.CMsg.Show("Exception", ex.ToString(), "", false, true);
            }
        }

        public void receiveTorque()
        {
            try
            {
                String szData = "";
                byte[] bytesFrom = new byte[255];
                byte[] byData;
                int iRx = server.Receive(bytesFrom);

                string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                String resultchk = dataFromClient.Substring(4, 4).ToString();

                // Response Last Torque Data
                if (resultchk == "0061")
                {

                    // send Torque 수신 확인 메시지 전송
                    szData = "00200062001000000000\0";//Request Torque data 
                    byData = Encoding.Default.GetBytes(szData);
                    server.Send(byData, SocketFlags.None);

                    // Torque, 체결 여부, 각도
                    string strTorque = dataFromClient.Substring(142, 2) + "." + dataFromClient.Substring(144, 2);
                    string tightState = dataFromClient.Substring(107, 1).ToString();
                    string strangle = dataFromClient.Substring(169, 5);

                    torque = double.Parse(strTorque);
                    status = tightState;
                    angle = double.Parse(strangle);

                    sendCb?.Invoke(torque, status, angle);

                    //LogMsg.CMsg.Show("Tool:" + m_strIP + ",State : 0061 -", "Receive Torque-", torque.ToString(), false, true);
                    //LogMsg.CMsg.Show("Tool:" + m_strIP + ",State : 0061 -", "Receive Tight State-", tightState, false, true);
                    //LogMsg.CMsg.Show("Tool:" + m_strIP + ",State : 0061 -", "Receive Angle-", angle.ToString(), false, true);

                    toolTorqueFlag = false;


                }
                else if (resultchk == "9999")
                {
                   // LogMsg.CMsg.Show("Tool State : 9999 -", "Receive keep Alive-", m_strIP, false, true);
                }

            }
            catch (Exception ex)
            {
                //LogMsg.CMsg.Show("Exception", ex.ToString(), "", false, true);
            }
        }

        public void sendKeepAlive(object sender, ElapsedEventArgs e)
        {

            if (keepAliveStart && keepAliveFlag)
            {
                String szData = "00209999000000000000\0";//Request Torque data 

                byte[] byData = Encoding.Default.GetBytes(szData);

                // send alive
                int sent = server.Send(byData, SocketFlags.None);

                //LogMsg.CMsg.Show("Tool State : 9999 -", "Aliv-", "Send", false, true);
            }
        }

        public bool getAlive()
        {
            return keepAliveFlag;
        }

        public double getTorque()
        {
            return torque;
        }

        public double getAngle()
        {
            return angle;
        }

        public string getTightState()
        {
            return status;
        }

        public void dataClear()
        {
            torque = -1;
            angle = -1;
            status = "";
        }
    }
