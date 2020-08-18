using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace POP
{
    /// <summary>
    /// 툴과 프린터, 아담을 관제하는 주변기기 클래스
    /// 공정을 기준으로 조립라인을 구분해 툴값을 콜백해줌
    /// Side공정에 한해 pc별로 RS232와 ETHERNET가 혼용되어 있어 초기화와 SerialComm_DeviceEvent 내용이 더러운편
    /// SerialComm_DeviceEvent에서 FRT공정은 문자와 숫자만을 선별해 콜백, SIDE공정은 양산으로 인해 테스트가 어려워 그냥 콜백
    /// </summary>
    /// <param name="PeripheralControlCenter"></param>
    class PeripheralControlCenter
    {
        public delegate void rs232barcodeEventCb(string barcode);
        public event rs232barcodeEventCb rs232barcodeCb;

        public delegate void Tool1CB(double tooldata);
        public event Tool1CB Tool1Cb;

        public delegate void Tool2CB(double tooldata);
        public event Tool2CB Tool2Cb;

        private NutTcp nut1;
        private NutTcp nut2;
        private UTIL mUTIL = new UTIL();
        //private ADAM6050 mADAM6050;

        private string m_strSerialData;
        private string savedBarcode;

        private static ZebraPrint_TCP.ZebraPrint zPrint;

        public PeripheralControlCenter()
        {
            SerialComm.SerialComm.Initialize();
            string[] seriallist = SerialComm.SerialComm.DescriptionofString;
            SerialComm.SerialComm.DeviceEvent += SerialComm_DeviceEvent;
            SerialComm.SerialComm.Start();
            
            //if (Program.DeviceType == "1") //SERIAL TOOL 2
            //{
            //    rs232barcodeCb += new rs232barcodeEventCb(BarcodeResult);
            //    rs232Tool1Cb += new rs232Tool1EventCb(getRS232ToqueCB1);
            //    rs232Tool2Cb += new rs232Tool2EventCb(getRS232ToqueCB2);
            //}
            if(INI.WORKNAME == "SIDE")
            {
                Console.WriteLine(INI.SIDE.DeviceType); 
                if(INI.SIDE.DeviceType == "2")  //ETHERNET TOOL 1, SERIAL TOOL 1
                {
                    nut1 = new NutTcp(INI.SIDE.NUT1IP, 4545);
                    nut1.sendCb += new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Connect();
                }
                if(INI.SIDE.DeviceType == "3") //ETHERNET TOOL2
                {
                    nut1 = new NutTcp(INI.SIDE.NUT1IP, 4545);
                    nut1.sendCb += new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Connect();
                    nut2 = new NutTcp(INI.SIDE.NUT2IP, 4545);

                    nut2.sendCb += new NutTcp.toqueEventCb(getTCPToqueCB2);
                    nut2.Connect();
                }
            
                zPrint = new ZebraPrint_TCP.ZebraPrint();
                //mADAM6050 = new ADAM6050(INI.SIDE.ADAMIP, INI.SIDE.ADAMPORT, int.Parse(INI.SIDE.ADAMDI), int.Parse(INI.SIDE.ADAMDO));

                ADAM_TCP.ADAM.Initialize();
                ADAM_TCP.ADAM.Start();

                if (ADAM_TCP.ADAM.ADAM_List[0].m_adamModbusConnected)
                {


                }
            }
            if(INI.WORKNAME == "FRT")
            {
                if(INI.FRT.Type == "JIN")
                {
                    nut1 = new NutTcp(INI.FRT.NUT1IP, 4545);
                    nut1.sendCb += new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Connect();
                }
            }

        }


        private void SerialComm_DeviceEvent(string port, string str)
        {
            if(INI.WORKNAME == "SIDE") { 
                if (INI.SIDE.DeviceType == "1")
                {
                    if (port == "COM1")
                    {
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        if (str[str.Length - 1] != 0x03)
                        {
                            m_strSerialData = m_strSerialData + str;
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            string barcode = m_strSerialData.Substring(1, m_strSerialData.Length - 2);
                            rs232barcodeCb?.Invoke(barcode);
                            m_strSerialData = "";
                        }
                    }
                    else if (port == "COM2")
                    {
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        if (str[str.Length - 1] != 0x0A)
                        {
                            m_strSerialData = m_strSerialData + str;
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            m_strSerialData.Substring(1, m_strSerialData.Length - 2);

                            string[] s = m_strSerialData.Split(',');

                            double m_dbTool1_Toruqe = double.Parse(s[8]) / 100;
                            if (m_dbTool1_Toruqe > 0 && s[5] == "1" && s[6] == "1")
                            {
                                Tool1Cb?.Invoke(m_dbTool1_Toruqe);
                            }
                        
                            //LogMsg.CMsg.Show("receive Tool1", "-ok", m_dbTool1_Toruqe.ToString(), false, true);

                            m_strSerialData = "";
                        }
                    }
                    else if (port == "COM3")
                    {
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        if (str[str.Length - 1] != 0x0A)
                        {
                            m_strSerialData = m_strSerialData + str;
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            m_strSerialData = m_strSerialData.Substring(1, m_strSerialData.Length - 2);
                            string m_strTool2_Torque = m_strSerialData.Substring(8, 7);
                            if (mUTIL.doubleConvert(m_strTool2_Torque) != 0)
                            {
                                Tool2Cb?.Invoke(mUTIL.doubleConvert(m_strTool2_Torque));
                            }
                            //LogMsg.CMsg.Show("receive Tool2", "-ok", m_strTool2_Torque, false, true);

                            m_strSerialData = "";
                        }
                    }
                }

                if (INI.SIDE.DeviceType == "2")
                {
                    if (port == "COM1")
                    {
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        if (str[str.Length - 1] != 0x03)
                        {
                            m_strSerialData = m_strSerialData + str;
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            string barcode = m_strSerialData.Substring(1, m_strSerialData.Length - 2);
                            rs232barcodeCb?.Invoke(barcode);
                            m_strSerialData = "";
                        }
                    }
                    else if (port == "COM3")
                    {
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        if (str[str.Length - 1] != 0x0A)
                        {
                            m_strSerialData = m_strSerialData + str;
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            m_strSerialData = m_strSerialData.Substring(1, m_strSerialData.Length - 2);
                            string m_strTool2_Torque = m_strSerialData.Substring(8, 7);
                            if (mUTIL.doubleConvert(m_strTool2_Torque) != 0)
                            {
                                Tool2Cb?.Invoke(mUTIL.doubleConvert(m_strTool2_Torque));
                            }
                            //LogMsg.CMsg.Show("receive Tool2", "-ok", m_strTool2_Torque, false, true);

                            m_strSerialData = "";
                        }
                    }
                }
                if (INI.SIDE.DeviceType == "3")
                {
                    if (port == "COM2")
                    {
                        //Console.WriteLine("CheckBarcode callback " + strBarcode);
                        // etx가 들어오지 않으면 계속 데이터를 저장
                        Console.WriteLine("??    " + str[str.Length - 1]);
                        if (str[str.Length - 1] != 0x03)
                        {
                            m_strSerialData = m_strSerialData + str;
                            //Console.WriteLine("??    " + str[str.Length - 3]);
                            //Console.WriteLine("m_strSerialData ? =>" + m_strSerialData);
                        }
                        else
                        {
                            // etx가 들어옴
                            m_strSerialData = m_strSerialData + str;
                            //Console.WriteLine("m_strSerialData else=>" + m_strSerialData);
                            string barcode = m_strSerialData.Substring(1, m_strSerialData.Length - 2);
                            //Console.WriteLine("barcode =>" + barcode);
                            //LogMsg.CMsg.Show("receive Barcode", "-ok", barcode, false, true);

                            rs232barcodeCb?.Invoke(barcode);

                            m_strSerialData = "";
                        }
                    }
                }
            }
            if(INI.WORKNAME == "FRT")
            {
                if (port == "COM2")
                {
                    Console.WriteLine(str);
                    //Console.WriteLine("(char)0x03 ???==    " + str.Trim());
                    Regex regex = new Regex(@"[a-zA-Z0-9]");
                    // 입력받은 문자열을 인자에 넣어줍니다.
                    for (int i = 0; i < str.Length; i++)
                    {
                        bool isMatch = regex.IsMatch(String.Format("{0}", str[i]));
                        //Console.WriteLine("str[i] =>" + String.Format("{0}", (byte)str[i]));
                        if (isMatch)
                        {
                            savedBarcode = savedBarcode + String.Format("{0}", str[i]);
                            //Console.WriteLine("savedBarcode =>" + savedBarcode);
                        }
                        else
                        {
                            if ((byte)str[i] == 0x03)
                            {
                                string tmpstr = savedBarcode;//.Substring(1, savedBarcode.Length - 2);
                                Console.WriteLine("LAST barcode =>" + tmpstr);
                                //LogMsg.CMsg.Show("receive Barcode", "-ok", savedBarcode, false, true);

                                this.savedBarcode = "";
                                rs232barcodeCb?.Invoke(tmpstr);
                                tmpstr = "";
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void getTCPToqueCB1(double torque, string tightStaus, double angle)
        {
            if (torque != -1)
            {
                Tool1Cb?.Invoke(torque);
            }
        }

        private void getTCPToqueCB2(double torque, string tightStaus, double angle)
        {
            if (torque != -1)
            {
                Tool2Cb?.Invoke(torque);
            }
        }


        public bool initializPrint(string initStr)
        {
            zPrint.InitializPrint(initStr);
            if (!zPrint.Connect())
            {
                //MessageBox.Show("error -print");
            }
            return true;
        }
        
        public bool barcodePrint(string barcode)
        {
            return zPrint.FN_Print(barcode);
        }

        //public float[] AdamRead()
        //{
        //    float[] sBufferAI = { };
        //    sBufferAI[0] = ADAM_TCP.ADAM.sBuffer_AI_01[0];
        //    sBufferAI[1] = ADAM_TCP.ADAM.sBuffer_AI_02[0];
        //    sBufferAI[2] = ADAM_TCP.ADAM.sBuffer_AI_03[0];
        //    sBufferAI[3] = ADAM_TCP.ADAM.sBuffer_AI_04[0];
        //    sBufferAI[4] = ADAM_TCP.ADAM.sBuffer_AI_06[0];
        //    sBufferAI[5] = ADAM_TCP.ADAM.sBuffer_AI_07[0];
        //    sBufferAI[6] = ADAM_TCP.ADAM.sBuffer_AI_08[0];
        //    sBufferAI[7] = ADAM_TCP.ADAM.sBuffer_AI_09[0];
        //    sBufferAI[8] = ADAM_TCP.ADAM.sBuffer_AI_10[0];
        //    //sBufferAI[9] = ADAM_TCP.ADAM.sBuffer_AI_11[0];
        //    //sBufferAI[10] = ADAM_TCP.ADAM.sBuffer_AI_11[0];
        //    //sBufferAI[11] = ADAM_TCP.ADAM.sBuffer_AI_12[0];
        //    //sBufferAI[12] = ADAM_TCP.ADAM.sBuffer_AI_13[0];
        //    //sBufferAI[13] = ADAM_TCP.ADAM.sBuffer_AI_14[0];
        //    //sBufferAI[14] = ADAM_TCP.ADAM.sBuffer_AI_15[0];
        //    //sBufferAI[15] = ADAM_TCP.ADAM.sBuffer_AI_16[0];
        //    //sBufferAI[16] = ADAM_TCP.ADAM.sBuffer_AI_17[0];
        //    //sBufferAI[17] = ADAM_TCP.ADAM.sBuffer_AI_18[0];


        //    return sBufferAI;
        //}
        public void ClampRelease()
        {
            Thread thread = new Thread(new ThreadStart(delegate ()
            {
                ADAM_TCP.ADAM.WriteData(0, 12, 1);
                Thread.Sleep(500);
                // DIO  = 0 Reset
                ADAM_TCP.ADAM.WriteData(0, 12, 0);
            }));
            thread.Start();
        }
        public void AdamWrite(int stat)
        {
            ADAM_TCP.ADAM.WriteData(0, 12, stat);
        }
        public void deInit()
        {
            //mADAM6050.Close();
            SerialComm.SerialComm.DeviceEvent -= SerialComm_DeviceEvent;
            SerialComm.SerialComm.Stop();

            //if (Program.DeviceType == "1") //SERIAL TOOL 2
            //{
            //    rs232barcodeCb += new rs232barcodeEventCb(BarcodeResult);
            //    rs232Tool1Cb += new rs232Tool1EventCb(getRS232ToqueCB1);
            //    rs232Tool2Cb += new rs232Tool2EventCb(getRS232ToqueCB2);
            //}
            if (INI.WORKNAME == "SIDE")
            {
                if (INI.SIDE.DeviceType == "2")  //ETHERNET TOOL 1, SERIAL TOOL 1
                {
                    nut1 = new NutTcp(INI.SIDE.NUT1IP, 4545);
                    nut1.sendCb -= new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Close();
                }
                if (INI.SIDE.DeviceType == "3") //ETHERNET TOOL2
                {
                    nut1 = new NutTcp(INI.SIDE.NUT1IP, 4545);
                    nut1.sendCb -= new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Close();
                    nut2 = new NutTcp(INI.SIDE.NUT2IP, 4545);
                    nut2.sendCb -= new NutTcp.toqueEventCb(getTCPToqueCB2);
                    nut2.Close();
                }
            }
            if(INI.WORKNAME == "FRT")
            {
                if(INI.FRT.Type == "JIN")
                {
                    nut1 = new NutTcp(INI.FRT.NUT1IP, 4545);
                    nut1.sendCb -= new NutTcp.toqueEventCb(getTCPToqueCB1);
                    nut1.Close();
                }
            }
        }
    }

}
