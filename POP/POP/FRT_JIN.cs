using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Media;
using System.Net.NetworkInformation;
using System.Collections;
using System.Timers;
using System.Text.RegularExpressions;

namespace POP
{
    /// <summary>
    /// FRT는 양산중이 아니기도 해서, 최대한 이벤트 드리븐으로 구현
    /// </summary>
    /// <param name="FRT_JIN"></param>
    public partial class FRT_JIN : Form
    {
        private static System.Timers.Timer mUiTimer;
        private static SoundPlayer wp_OK = new SoundPlayer("SGPOP/OK.wav");
        private static SoundPlayer wp_NG = new SoundPlayer("SGPOP/NG.wav");
        private static SoundPlayer wp_NG2 = new SoundPlayer("SGPOP/NG2.wav");
        private static SoundPlayer wp_ResultOK = new SoundPlayer("SGPOP/RESULT_OK.wav");
        private static SoundPlayer wp_BarcodeOK = new SoundPlayer("SGPOP/BarcodeOK.wav");
        private static SoundPlayer wp_BarcodeNG2 = new SoundPlayer("SGPOP/BarcodeNG2.wav");
        private PeripheralControlCenter mPCC;

        /// <summary>
        /// 작업완료 처리를 위한 기준 변수
        /// MAIN바코드 내의 활성화 되는 자재에 따라 변동 있음
        /// </summary>
        /// <param name="GETOKCNT"></param>
        private static int GETOKCNT = 0;

        /// <summary>
        /// 활성화된 UI에 따라 바코드 찍고, 툴값 다 넣으면 증감해서 GETOKCNT랑 비교해서 작업완료 처리
        /// </summary>
        /// <param name="NOWOKCNT"></param>
        private static int NOWOKCNT = 0;

        private static int UIFLICKERTIME = 500;

        private static int TOOL1OKCNT = 0;

        private int PingTestCnt = 0;
        private int LoginoutCnt = 0;

        private UTIL mUTIL = new UTIL();
        private Frm_WorkStandard mFrm_WorkStandard = new Frm_WorkStandard();

        private string USER_ID = "";
        private string USER_NAME = "";


        /// <summary>
        /// 플래그 진짜 싫어하는데.. 같은 자재 찍어서 NOWOKCNT 막 올라가는 버그 보여주더라...
        /// FRT는 진짜 플래그 안쓸랬는데 바로 고쳐야해서 어쩔수 없이..
        /// </summary>
        /// <param name="HARNESSOK"></param>
        private bool HARNESSOK = false;
        private bool SABOK = false;
        private bool FRAMEOK = false;
        private bool LSUPTOK = false;
        private bool MOTOROK = false;
        private bool MATOK = false;

        private string m_strHarnessBarcode = "";
        private string m_strSabBarcode = "";
        private string m_strFrameBarcode = "";
        private string m_strLsuptBarcode = "";
        private string m_strMotorBarcode = "";
        private string m_strMatBarcode = "";
        private bool SCANINTERLOCK = false;
        private bool TOOLINTERLOCK = false;


        public FRT_JIN()
        {
            InitializeComponent();
        }

        private void FRT_JIN_Load(object sender, EventArgs e)
        {
            //mADAM6050 = new ADAM6050(Program.ADAMIP,Program.ADAMPort, Program.ADAMDI,Program.ADAMDO);

            //READ DATA 
            //data 0-14   AI 1-15 
            //true == 0, false = 1
            //bool[] data = mADAM6050.RefreshDIO();

            //WRITE DATA
            //mADAM6050.SetDO(12, 1);
            //Thread.Sleep(500);
            //mADAM6050.SetDO(12, 0);
            
            //Close ADAM
            //mADAM6050.Close();

            Screen[] screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                Screen scr = (screens[0].WorkingArea.Contains(this.Location)) ? screens[1] : screens[0];
                mFrm_WorkStandard.Show();
                mFrm_WorkStandard.Location = new System.Drawing.Point(scr.Bounds.Left, 0);
                mFrm_WorkStandard.WindowState = FormWindowState.Maximized;
            }
            
            if (INI.FRT.Direction == "L")
            {
                lbllrstat.Text = "LH";
                label33.Text = "SAB 진조립 (LH)";
            }
            if (INI.FRT.Direction == "R")
            {
                lbllrstat.Text = "RH";
                label33.Text = "SAB 진조립 (RH)";
            }
            lblinfo.Text = "SAB, 하네스, L/SUPT 이름 확인 할 것";
            mUiTimer = new System.Timers.Timer();
            mUiTimer.Interval = 100;
            mUiTimer.Elapsed += new ElapsedEventHandler(Refresh);

            mPCC = new PeripheralControlCenter();
            mPCC.rs232barcodeCb += new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb += new PeripheralControlCenter.Tool1CB(Tool1Result);
            
            mUiTimer.Start();
            dataAllReset();
            //CheckBarcode("06Y4219800000000RP4261625612V557819358TL119074B06AR9001");
            //logboxAdd("Tool:" + String.Format("{0}", 1.1));
            //logboxAdd("Barcode:" + "06Y4219800000000RP4261625612V557819358TL119074B06AR9001" + "," + " Length:" + "55");
            lblworksequence.Text = NowWorkCount(INI.FRT.Direction);
            LastWorkListUpdate();
            pictureBox2.Load("Pictures/user image.png");
            pictureBox2.Visible = true;
            listBox1.Visible = false;
            lblWarning.Text = "";
            lblWarning.Visible = false;
        }
        
        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(Refresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb -= new PeripheralControlCenter.Tool1CB(Tool1Result);
            this.Dispose();
            this.Close();
            System.Diagnostics.Process[] baseProcess;
            baseProcess = System.Diagnostics.Process.GetProcesses();
            baseProcess = System.Diagnostics.Process.GetProcessesByName("POP");
            for (int i = 0; i < baseProcess.Length; i++) baseProcess[i].Kill();

            Application.Exit();
        }

        private string NowWorkCount(string LR)
        {
            string strReceiveData = "";
            Console.WriteLine("cnt" + INI.FRT.Direction);
            strReceiveData = mUTIL.SendDB("sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB" + "," + LR);
            //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
            string[] strArr = { "", "", "0" ,"0" };
            strArr = strReceiveData.Split(',');
            int i = 0;
            foreach (string str in strArr)
            {
                Console.WriteLine(i + " db parsing" + str);
                i++;
            }
            //if(strArr.Length < 1)
            //{
            //    return "";
            //}
            //else
            //{
            return strArr[2];
            //}
            
        }

        private void LastWorkListUpdate()
        {
            this.Invoke(new Action(delegate ()
            {
                string strReceiveData = "";
                strReceiveData = mUTIL.SendDB("sp_JIT_SEL_SAB_SUB" + "," + lbllrstat.Text.Substring(0, 1));
                //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
                //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
                string[] strArr;
                strArr = strReceiveData.Split(',');
                int i = 0;
                foreach (string str in strArr)
                {
                    //Console.WriteLine(i + " db parsing" + str);
                    i++;
                }
                int startIndex = 2;
                string[] strData = new string[10];
                dataGridView1.ScrollBars = ScrollBars.Vertical;
                if(dataGridView1.Rows.Count > 0) { 
                    dataGridView1.Rows.Clear();
                    dataGridView1.DataSource = null;
                }
                for (int z = 0; z < 10; z++)
                {
                    //Console.WriteLine("startIndex: "+ startIndex+" strArr.Length:" + strArr.Length);
                    if (startIndex > strArr.Length)
                    {
                        break;
                    }
                    strData[0] = strArr[startIndex];
                    strData[1] = strArr[startIndex + 2];
                    strData[2] = strArr[startIndex + 4];
                    strData[3] = strArr[startIndex + 6];
                    strData[4] = strArr[startIndex + 8].Substring(0, 1);
                    strData[5] = strArr[startIndex + 10];
                    strData[6] = strArr[startIndex + 12];
                    strData[7] = strArr[startIndex + 14];
                    strData[8] = strArr[startIndex + 30];
                    strData[9] = strArr[startIndex + 32];
                    dataGridView1.Rows.Insert(int.Parse(strArr[startIndex])-1,strData);
                    dataGridView1.Focus();
                    startIndex += 50;
                }
                dataGridView1.ClearSelection();
                int nRowIndex = dataGridView1.Rows.Count - 1;
                dataGridView1.Rows[nRowIndex].Selected = true;
                dataGridView1.Rows[nRowIndex].Cells[0].Selected = true;
            }));
        }

        private bool CheckBarcodeOverlap(string Barcode, int BarcodeLength)
        {
            bool OVERLAP = false;
            string strReceiveData = "";
            strReceiveData = mUTIL.SendDB("sp_JIT_CHECK_BARCODE_SAB_SUB" + "," + Barcode.Trim() + "," + BarcodeLength);
            //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
            string[] strArr;
            strArr = strReceiveData.Split(',');
            int i = 0;
            foreach (string str in strArr)
            {
                Console.WriteLine(i + " db parsing" + str);
                i++;
            }
            if (int.Parse(strArr[2]) > 0)
            {
                lblresult.Text = "NG";
                lblWarning.Text = "중복";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                wp_BarcodeNG2.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    //formJobResult_Close();
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                OVERLAP = true;
            }
            OVERLAP = false;
            return OVERLAP;
        }


        private void Refresh(object sender, ElapsedEventArgs e)
        {
            TimerStat_Test();
        }

        /// <summary>
        /// 작업완료시에 옵션 정보는 남아있어야해서 작업 내용만 리셋
        /// </summary>
        /// <param name="dataReset"></param>
        public void dataReset()
        {
            Console.WriteLine("dataReset Called");
            lblharnessBar.Text = "";
            lblsabBar.Text = "";
            lbllsuptBar.Text = "";
            lblbframeBar.Text = "";
            lblmatbar.Text = "";
            lblmotorbar.Text = "";
            lbltool1result1.Text = "";
            lbltool1result2.Text = "";
            lbltool2result1.Text = "";
            lbltool2result2.Text = "";
            if (lbltool2result1.BackColor == Color.LightPink || lbltool2result1.BackColor == Color.PaleGreen) { 
                lbltool2result1.BackColor = Color.LightPink;
            }
            if (lbltool2result2.BackColor == Color.LightPink || lbltool2result2.BackColor == Color.PaleGreen)
            {
                lbltool2result2.BackColor = Color.LightPink;
            }
            TOOL1OKCNT = 0;
            NOWOKCNT = 0;
            HARNESSOK = false;
            SABOK = false;
            FRAMEOK = false;
            LSUPTOK = false;
            MOTOROK = false;
            MATOK = false;
            m_strHarnessBarcode = "";
            m_strSabBarcode = "";
            m_strFrameBarcode = "";
            m_strLsuptBarcode = "";
            m_strMotorBarcode = "";
            m_strMatBarcode = "";
        }

        /// <summary>
        /// UI전체 초기화
        /// </summary>
        /// <param name="dataAllReset"></param>
        public void dataAllReset()
        {
            Console.WriteLine("dataAllReset Called");
            lblframedbopt.Text = "";
            lblharnessBar.Text = "";
            lblharnessDBbar.Text = "";
            lblsabBar.Text = "";
            lblsabDBbar.Text = "";
            lbllsuptBar.Text = "";
            lbllsuptdbbar.Text = "";
            lblbframeBar.Text = "";
            lblframeDBbar.Text = "";
            lblmotorbar.Text = "";
            lblmotorDBbar.Text = "";
            lblmatbar.Text = "";
            lblmatDBbar.Text = "";
            lbltool1result1.Text = "";
            lbltool1result2.Text = "";
            lbltool2result1.Text = "";
            lbltool2result2.Text = "";
            TOOL1OKCNT = 0;
            GETOKCNT = 0;
            NOWOKCNT = 0;

            HARNESSOK = false;
            SABOK = false;
            FRAMEOK = false;
            LSUPTOK = false;
            MOTOROK = false;
            MATOK = false;
            m_strHarnessBarcode = "";
            m_strSabBarcode = "";
            m_strFrameBarcode = "";
            m_strLsuptBarcode = "";
            m_strMotorBarcode = "";
            m_strMatBarcode = "";



            lblharnessDBbar.BackColor = Color.Gray;
            lblsabDBbar.BackColor = Color.Gray;
            lblframeDBbar.BackColor = Color.Gray;
            lblframedbopt.BackColor = Color.Gray;
            lbllsuptdbbar.BackColor = Color.Gray;
            lblmotorDBbar.BackColor = Color.Gray;
            lblmatDBbar.BackColor = Color.Gray;
            lblharnessBar.BackColor = Color.Gray;
            lblsabBar.BackColor = Color.Gray;
            lblbframeBar.BackColor = Color.Gray;
            lbllsuptBar.BackColor = Color.Gray;
            lblmotorbar.BackColor = Color.Gray;
            lblmatbar.BackColor = Color.Gray;

            lbltool1result1.BackColor = Color.Gray;
            lbltool1result2.BackColor = Color.Gray;
            lbltool2result1.BackColor = Color.Gray;
            lbltool2result2.BackColor = Color.Gray;
            //lblWarning.Text = "";
            //lblWarning.Visible = false;
        }
        public void CheckMAIN_Barcode(string strBarcode)
        {
            dataAllReset();
            //string mainex = "06Y4219800000000RP4261625612V557819358TL119074B06AR9001";
            Console.WriteLine("length: " + strBarcode.Length + "barcode-> " + strBarcode);            // first barcode DB 체크
            Console.WriteLine(" strBarcode.Substring(46, 4) " + strBarcode.Substring(46, 4) + " strBarcode.Substring(50)" + strBarcode.Substring(50, 1));
            string strReceiveData = "";
            strReceiveData = mUTIL.SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + strBarcode.Substring(46, 4).Trim()+ "," + strBarcode.Substring(50, 1).Trim());
            //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
            string[] strArr;
            strArr = strReceiveData.Split(',');
            //m_strLotNO = strArr[2];
            int i = 0;
            foreach (string str in strArr)
            {
                Console.WriteLine(i+" db parsing" + str);
                i++;
            }

            if (strBarcode.Substring(46, 4) == strArr[2])
            {
                lblcartype.Text = strArr[4];

                string direction = "";
                if (strBarcode.Substring(50, 1) == "L")
                {
                    direction = "LH";
                }
                if (strBarcode.Substring(50, 1) == "R")
                {
                    direction = "RH";
                }

                lbllrstat.Text = direction;
                lbllocal.Text = strArr[8];
                lblcode.Text = strBarcode.Substring(46, 4);
                //lblworksequence.Text = "";// 작업수량 미정

                
                if (strBarcode.Substring(50, 1) == "L")
                {
                    if(strArr[46] != "" || strArr[46] != "X")
                    {
                        lblharnessDBbar.BackColor = Color.LightPink;
                        lblharnessDBbar.Text = strArr[46];
                        lblharnessBar.BackColor = Color.WhiteSmoke;
                        GETOKCNT++;
                    }
                    else
                    {
                        lblharnessDBbar.BackColor = Color.Gray;
                        lblharnessDBbar.Text = "";
                        lblharnessBar.BackColor = Color.Gray;
                    }
                }

                if (strBarcode.Substring(50, 1) == "R")
                {
                    if (strArr[46] != "" || strArr[46] != "X")
                    {
                        lblharnessDBbar.BackColor = Color.LightPink;
                        lblharnessDBbar.Text = strArr[48];
                        lblharnessBar.BackColor = Color.WhiteSmoke;
                        GETOKCNT++;
                    }
                    else
                    {
                        lblharnessDBbar.BackColor = Color.Gray;
                        lblharnessDBbar.Text = "";
                        lblharnessBar.BackColor = Color.Gray;
                    }
                }

                if (strBarcode.Substring(50, 1) == "L")
                {
                    if(strArr[64] != "" || strArr[46] != "X")
                    {
                        lblsabDBbar.BackColor = Color.PaleGreen;
                        lblsabDBbar.Text = strArr[64].Substring(4);
                        lblsabBar.BackColor = Color.WhiteSmoke;
                        lbltool1result1.BackColor = Color.PaleGreen;
                        lbltool1result2.BackColor = Color.PaleGreen;
                        GETOKCNT++;
                        GETOKCNT++;
                        GETOKCNT++;
                    }
                    else
                    {
                        lblsabDBbar.BackColor = Color.Gray;
                        lblsabDBbar.Text = "";
                        lblsabBar.BackColor = Color.Gray;
                    }
                }

                if (strBarcode.Substring(50, 1) == "R")
                {
                    if (strArr[66] != "" || strArr[66] != "X")
                    {
                        lblsabDBbar.BackColor = Color.PaleGreen;
                        lblsabDBbar.Text = strArr[66].Substring(4);
                        lblsabBar.BackColor = Color.WhiteSmoke;
                        lbltool1result1.BackColor = Color.PaleGreen;
                        lbltool1result2.BackColor = Color.PaleGreen;
                        GETOKCNT++;
                        GETOKCNT++;
                        GETOKCNT++;
                    }
                    else
                    {
                        lblsabDBbar.BackColor = Color.Gray;
                        lblsabDBbar.Text = "";
                        lblsabBar.BackColor = Color.Gray;
                    }
                }
                
                if (lbllrstat.Text == "LH")
                {

                    lblframeDBbar.BackColor = Color.LightPink;
                    lblframeDBbar.Text = strArr[74].Substring(4);
                    lblframedbopt.BackColor = Color.LightPink;
                    lblframedbopt.Text = strArr[22];
                    lblbframeBar.BackColor = Color.WhiteSmoke;
                    GETOKCNT++;
                }
                if (lbllrstat.Text == "RH")
                {
                    /*if (strArr[26] == ""){
                        lblframeDBbar.BackColor = Color.PaleGreen;
                        lblframeDBbar.Text = "STD";
                        lblbframeBar.BackColor = Color.WhiteSmoke;
                    }
                    else
                    {*/
                        lblframeDBbar.BackColor = Color.LightPink;
                        lblframeDBbar.Text = strArr[76].Substring(4);
                    lblframedbopt.BackColor = Color.LightPink;
                    lblframedbopt.Text = strArr[24];
                    lblbframeBar.BackColor = Color.WhiteSmoke;
                    GETOKCNT++;
                    //}
                }
                if (strBarcode.Substring(50, 1) == "L")
                {
                    lbllsuptdbbar.BackColor = Color.Gray;
                    lbllsuptdbbar.Text = "";
                    lbllsuptBar.BackColor = Color.Gray;
                    if (strArr[82] == "" || strArr[82] == "X")
                    {
                        lbllsuptdbbar.BackColor = Color.Gray;
                        lbllsuptdbbar.Text = "";
                        lbllsuptBar.BackColor = Color.Gray;
                    }
                    else
                    {
                        lbllsuptdbbar.BackColor = Color.PaleGreen;
                        lbllsuptdbbar.Text = strArr[82];
                        lbllsuptBar.BackColor = Color.WhiteSmoke;
                        GETOKCNT++;
                    }
                }

                if (strBarcode.Substring(50, 1) == "R")
                {
                    if(strArr[84] == "" || strArr[84] == "X")
                    {
                        lbllsuptdbbar.BackColor = Color.Gray;
                        lbllsuptdbbar.Text = "";
                        lbllsuptBar.BackColor = Color.Gray;
                    }
                    else
                    {
                        lbllsuptdbbar.BackColor = Color.PaleGreen;
                        lbllsuptdbbar.Text = strArr[84];
                        lbllsuptBar.BackColor = Color.WhiteSmoke;
                        GETOKCNT++;
                    }
                }
                Console.WriteLine("???=>  " + strArr[118] + " " + strArr[118].Length);
                
                if (strArr[118] == "" || strArr[118] == "X")
                {
                    lblmotorDBbar.BackColor = Color.Gray;
                    lblmotorDBbar.Text = "";
                    lblmotorbar.BackColor = Color.Gray;
                    lbltool2result1.BackColor = Color.Gray;
                    lbltool2result2.BackColor = Color.Gray;
                }
                else
                {
                    lblmotorDBbar.BackColor = Color.LightPink;
                    lblmotorDBbar.Text = strArr[118];
                    lblmotorbar.BackColor = Color.WhiteSmoke;
                    lbltool2result1.BackColor = Color.LightPink;
                    lbltool2result2.BackColor = Color.LightPink;
                    GETOKCNT++;
                    GETOKCNT++;
                    GETOKCNT++;
                }
                Console.WriteLine("???=>  " + strArr[114]+ " " + strArr[114].Length);
                
                if (strArr[114] == "" || strArr[114] == "X")
                {
                    lblmatDBbar.BackColor = Color.Gray;
                    lblmatDBbar.Text = "";
                    lblmatbar.BackColor = Color.Gray;
                    
                }
                else
                {
                    lblmatDBbar.BackColor = Color.PaleGreen;
                    lblmatDBbar.Text = strArr[114];
                    lblmatbar.BackColor = Color.WhiteSmoke;
                    GETOKCNT++;

                }
                lblresult.Text = "OK";
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblWarning.Text = "";
                    lblresult.Text = "";
                    if (lblWarning.Visible)
                    {
                        lblWarning.Visible = false;
                    }
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                lblworksequence.Text = NowWorkCount(lbllrstat.Text.Substring(0,1));
                LastWorkListUpdate();
            }
            else
            {
                lblresult.Text = "NG";
                lblWarning.Text = "NG";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    wp_NG2.Play();
                }));
                thread.Start();

                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));                      
            }
        }


        public void CheckHarness_Barcode(string strBarcode)
        {
            //84369253
            //string mainex = "8436925301152018064319";
            Console.WriteLine("CheckHarness_Barcode(strBarcode) " + strBarcode);
            
            if(CheckBarcodeOverlap(strBarcode, strBarcode.Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            if (strBarcode.Substring(4, 4) == lblharnessDBbar.Text)
            {
                lblharnessBar.Text = strBarcode;
                lblharnessDBbar.BackColor = Color.LightPink;
                m_strHarnessBarcode = strBarcode;
                lblresult.Text = "OK";
                lblWarning.Text = "OK";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                wp_OK.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));

                if (!HARNESSOK) {
                    HARNESSOK = true;
                    NOWOKCNT++;
                }
                if (JobCompleteUIChecker())
                {
                    JobComplete();
                    dataReset();
                }
            }
            else
            {
                lblharnessBar.Text = "";
                lblresult.Text = "NG";
                lblharnessDBbar.BackColor = Color.LightPink;
                lblWarning.Text = "NG";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                wp_NG2.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            }
        }


        public void CheckSAB_Barcode(string strBarcode)
        {
            //string mainex = "06Y4730800000000RP4247997812V544663743T11182729978W0057";
            // barcode 체크
            Console.WriteLine("strBarcode.Substring(22, 4)  " + strBarcode.Substring(22, 4));

            if (CheckBarcodeOverlap(strBarcode.Substring(38), strBarcode.Substring(38).Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            
            if (strBarcode.Substring(22, 4) == lblsabDBbar.Text)
            {
                //eBARCODESABRESULTSTAT = BARCODERESULTSTAT.OK;
                lblsabBar.Text = strBarcode.Substring(38);
                m_strSabBarcode = strBarcode;
                lblresult.Text = "OK";
                lblWarning.Text = "OK";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                lblsabDBbar.BackColor = Color.PaleGreen;
                lbltool1result1.BackColor = Color.PaleGreen;
                lbltool1result2.BackColor = Color.PaleGreen;
                wp_OK.Play();
                   
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    //formJobResult_Close();
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                
                if (!SABOK)
                {
                    SABOK = true;
                    NOWOKCNT++;
                }
                if (JobCompleteUIChecker())
                {
                    JobComplete();
                    dataReset();
                }
            }
            else
            {
                lblsabBar.Text = "";
                lblresult.Text = "NG";
                lblsabDBbar.BackColor = Color.LightPink;
                lblWarning.Text = "NG";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                wp_NG2.Play();
                    
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                
            }
        }

        public void CheckFrame_Barcode(string strBarcode, int length)
        {
            //string mainex = "30AE0318122900123";
            //string mainex = "KR400393060016";
            if (CheckBarcodeOverlap(strBarcode, strBarcode.Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            if(length == 17 ) //&& strBarcode.Substring(8,4) == lblframeDBbar.Text)
            {
                lblbframeBar.Text = strBarcode;
                lblframeDBbar.BackColor = Color.LightPink;
                lblframedbopt.BackColor = Color.LightPink;
                m_strFrameBarcode = strBarcode;
                lblresult.Text = "OK";
                lblWarning.Text = "OK";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    wp_OK.Play();
                }));
                thread.Start();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    //formJobResult_Close();
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    lblresult.Text = "";
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                if (!FRAMEOK)
                {
                    FRAMEOK = true;
                    NOWOKCNT++;
                }
            }
            
            else if(length == 14 )//&& strBarcode.Substring(2, 4) == lblframeDBbar.Text)
            {
                lblbframeBar.Text = strBarcode;
                m_strFrameBarcode = strBarcode;
                lblframeDBbar.BackColor = Color.LightPink;
                lblframedbopt.BackColor = Color.LightPink;
                lblresult.Text = "OK";
                lblWarning.Text = "OK";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    wp_OK.Play();
                }));
                thread.Start();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    //formJobResult_Close();
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    lblresult.Text = "";
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                if (!FRAMEOK)
                {
                    FRAMEOK = true;
                    NOWOKCNT++;
                }
            }
            else
            {
                lblbframeBar.Text = strBarcode;
                lblframeDBbar.BackColor = Color.LightPink;
                lblframedbopt.BackColor = Color.LightPink;
                lblresult.Text = "NG";
                lblWarning.Text = "NG";
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Visible = true;
                }));
                wp_NG2.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            }
            if (JobCompleteUIChecker())
            {
                JobComplete();
                dataReset();
            }
        }



        public void CheckLSUPT_Barcode(string strBarcode)
        {
            //84369253
            //string mainex = "5052806BAA1210510182568";
            Console.WriteLine("CheckLSUPT_Barcode(strBarcode) " + strBarcode);
            if (CheckBarcodeOverlap(strBarcode, strBarcode.Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            lbllsuptBar.Text = strBarcode;
            m_strLsuptBarcode = strBarcode;
            lblresult.Text = "OK";
            lblWarning.Text = "OK";
            this.Invoke(new Action(delegate ()
            {
                lblWarning.Visible = true;
            }));
            lbllsuptdbbar.BackColor = Color.PaleGreen;
                wp_OK.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    //formJobResult_Close();
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    lblresult.Text = "";
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            if (!LSUPTOK)
            {
                LSUPTOK = true;
                NOWOKCNT++;
            }
            if (JobCompleteUIChecker())
            {
                JobComplete();
                dataReset();
            }
        }

        public void CheckMotor_Barcode(string strBarcode)
        {
            //91자리...
            //string mainex = "00002706Y4211100000000XP1353064812V588156823T38183100000001851P12023081B1TX3081B06Y18380185";
            Console.WriteLine("CheckHarness_Barcode(strBarcode) " + strBarcode);
            if (CheckBarcodeOverlap(strBarcode, strBarcode.Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            lblmotorbar.Text = strBarcode;
            m_strMotorBarcode = strBarcode;
            lblresult.Text = "OK";
            lblWarning.Text = "OK";
            this.Invoke(new Action(delegate ()
            {
                lblWarning.Visible = true;
            }));
            lblmotorDBbar.BackColor = Color.LightPink;
                wp_OK.Play();
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            if (!MOTOROK)
            {
                MOTOROK = true;
                NOWOKCNT++;
            }
            if (JobCompleteUIChecker())
            {
                JobComplete();
                dataReset();
            }
        }

        public void CheckMat_Barcode(string strBarcode)
        {
            //string mainex = "13527743L4218A241180237";
            Console.WriteLine("CheckMat_Barcode(strBarcode) " + strBarcode);
            if (CheckBarcodeOverlap(strBarcode, strBarcode.Length))
            {
                Console.WriteLine("바코드 중복");
                return;
            }
            //if (strBarcode.Substring(4, 4) == lblharnessDBbar.Text)
            //{
            //eBARCODEHARNESSRESULTSTAT = BARCODERESULTSTAT.OK;
            lblmatbar.Text = strBarcode;
            m_strMatBarcode = strBarcode;
            lblresult.Text = "OK";
            lblWarning.Text = "OK";
            this.Invoke(new Action(delegate ()
            {
                lblWarning.Visible = true;
            }));
            lblmatDBbar.BackColor = Color.PaleGreen;
            wp_OK.Play();
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((object state) =>
            {
                //formJobResult_Close();
                lblresult.Text = "";
                lblWarning.Text = "";
                lblWarning.Visible = false;
                timer.Dispose();
            }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            if (!MATOK)
            {
                MATOK = true;
                NOWOKCNT++;
            }
            if (JobCompleteUIChecker())
            {
                JobComplete();
                dataReset();
            }
        }


        public void CheckInterlock()
        {
            string strReceiveData = "";
            strReceiveData = mUTIL.SendDB("sp_JIT_GET_INTERLOCK" + "," + "SAB_SUB");
            //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
            string[] strArr;
            strArr = strReceiveData.Split(',');
            //m_strLotNO = strArr[2];
            int i = 0;
            foreach (string str in strArr)
            {
                Console.WriteLine(i + " db parsing" + str);
                i++;
            }
            if (strArr[0] == "S" && strArr[1] == "O")
            {
                SCANINTERLOCK = true;
            }
            if (strArr[0] == "S" && strArr[1] == "X")
            {
                SCANINTERLOCK = false;
            }
            if (strArr[0] == "T" && strArr[1] == "O")
            {
                TOOLINTERLOCK = true;
            }
            if (strArr[0] == "T" && strArr[1] == "X")
            {
                TOOLINTERLOCK = false;
            }
        }


        public void CheckBarcode(string strBarcode)
        {
            Console.WriteLine("CheckBarcode callback " + strBarcode + " " + strBarcode.Length);
            logboxAdd("Barcode:" + strBarcode +","+" Length:" + strBarcode.Length);
            LoginoutCnt = 0;
            if (pictureBox2.Visible)
            {
                switch (strBarcode.Length)
                {
                    case (int)BARCODE_LENGTH_ID.ID:
                        string strReceiveData = "";
                        strReceiveData = mUTIL.SendDB("sp_JIT_SEL_USER" + "," + strBarcode.Trim());
                        //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
                        //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
                        string[] strArr;
                        strArr = strReceiveData.Split(',');
                        //m_strLotNO = strArr[2];
                        int i = 0;
                        foreach (string str in strArr)
                        {
                            Console.WriteLine(i + " db parsing" + str);
                            i++;
                        }
                        if(strArr[2] != "")
                        {
                            pictureBox2.Visible = false;

                            lblname.Text = strArr[2];
                            USER_ID = strBarcode;
                            USER_NAME = strArr[2];
                            CheckInterlock();
                        }
                        break;
                }
            }
            else
            {
                if (strBarcode == "9999")
                {
                    USER_ID = "";
                    USER_NAME = "";
                    lblname.Text = "";
                    pictureBox2.Visible = true;
                }
                switch (strBarcode.Length)
                {
                    case (int)BARCODE_LENGTH_ID.END:
                        if(strBarcode == "END") { 
                            USER_ID = "";
                            USER_NAME = "";
                            lblname.Text = "";
                            pictureBox2.Visible = true;
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.MAIN:
                        int judgeMain_SAB = 0;
                        bool maincheck = int.TryParse(strBarcode.Substring(46, 4), out judgeMain_SAB);
                        if (!maincheck)
                        {
                            CheckMAIN_Barcode(strBarcode);
                            //Console.WriteLine("eBARCODEMAINRESULTSTAT" + eBARCODEMAINRESULTSTAT.ToString());
                        }
                        else
                        {
                            if (lblsabBar.BackColor == Color.WhiteSmoke)
                            {
                                CheckSAB_Barcode(strBarcode);
                            }
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.FRAME:
                        if (lblbframeBar.BackColor == Color.WhiteSmoke) //&& lblframeDBbar.Text == "F.FLAT")
                        {
                            CheckFrame_Barcode(strBarcode, (int)BARCODE_LENGTH_ID.FRAME);
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.FRAME2:
                        if (lblbframeBar.BackColor == Color.WhiteSmoke)
                        {
                            CheckFrame_Barcode(strBarcode, (int)BARCODE_LENGTH_ID.FRAME2);
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.HARNESS:
                        //8449719302162019075941 HAR [21]
                        if (lblharnessBar.BackColor == Color.WhiteSmoke)
                        {
                            CheckHarness_Barcode(strBarcode);
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.LSUPT:
                        //5052806BAA1210510182568 LSUPT [22]  <-- 7자리 b
                        //13527743L4218A241180237 MAT [22]    <-- 7자리 3
                        int judgeMain_LSUPT = 0;
                        bool lsuptcheck = int.TryParse(strBarcode.Substring(7, 0), out judgeMain_LSUPT);
                        if (!lsuptcheck)
                        {
                            if (lbllsuptBar.BackColor == Color.WhiteSmoke)
                            {
                                CheckLSUPT_Barcode(strBarcode);
                            }
                        }
                        else
                        {
                            if (lblmatbar.BackColor == Color.WhiteSmoke)
                            {
                                CheckMat_Barcode(strBarcode);
                            }
                        }
                        break;
                }
                if (strBarcode.Length > 65)
                {
                    if (lblmotorbar.BackColor == Color.WhiteSmoke)
                    {
                        CheckMotor_Barcode(strBarcode);
                    }
                }
            }
        }

        public bool isBetweenSpec(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public void Tool1Result(double torque)
        {
            logboxAdd("Tool:" + String.Format("{0}", torque));
            LoginoutCnt = 0;
            Console.WriteLine("torque nut1 " + torque);
            if (torque != -1)
            {
                //check check
                Console.WriteLine("torque nut1 " + torque);
                checkTool1Data(torque);
            }
        }

        public void checkTool1Data(double Tool1Data)
        {
            double min = double.Parse(INI.FRT.JINTOOL1MIN);
            double max = double.Parse(INI.FRT.JINTOOL1MAX);
            switch (TOOL1OKCNT)
            {
                case 0:
                    if (isBetweenSpec(Tool1Data, min, max))
                    {
                        lbltool1result1.Text = String.Format("{0:f2}", Tool1Data);
                        TOOL1OKCNT = TOOL1OKCNT + 1;
                        NOWOKCNT++;
                    }
                    break;
                case 1:
                    if (isBetweenSpec(Tool1Data, min, max))
                    {
                        if (lblmotorDBbar.BackColor == Color.Gray && lblmatDBbar.BackColor == Color.Gray)
                        {
                            if(lbltool1result2.Text == "") { 
                                NOWOKCNT++;
                                lbltool1result2.Text = String.Format("{0:f2}", Tool1Data);
                                lblWarning.Text = "OK";
                                this.Invoke(new Action(delegate ()
                                {
                                    lblWarning.Visible = true;
                                }));
                                wp_OK.Play();
                                System.Threading.Timer timer = null;
                                timer = new System.Threading.Timer((object state) =>
                                {
                                    lblresult.Text = "";
                                    lblWarning.Text = "";
                                    lblWarning.Visible = false;
                                    timer.Dispose();
                                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                                if (JobCompleteUIChecker())
                                {
                                    JobComplete();
                                    dataReset();
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    if (Tool1Data > 0 && lblmotorDBbar.BackColor == Color.LightPink)
                    {
                        lbltool2result1.BackColor = Color.PaleGreen;
                        TOOL1OKCNT = TOOL1OKCNT + 1;
                        NOWOKCNT++;
                    }
                    break;
                case 3:
                    if (Tool1Data > 0 && lblmotorDBbar.BackColor == Color.LightPink)
                    {
                        lbltool2result2.BackColor = Color.PaleGreen;
                        lblWarning.Text = "OK";
                        NOWOKCNT++;
                        this.Invoke(new Action(delegate ()
                        {
                            lblWarning.Visible = true;
                        }));
                        wp_OK.Play();
                        System.Threading.Timer timer = null;
                        timer = new System.Threading.Timer((object state) =>
                        {
                            lblresult.Text = "";
                            lblWarning.Text = "";
                            lblWarning.Visible = false;
                            timer.Dispose();
                        }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                        if (JobCompleteUIChecker())
                        {
                            JobComplete();
                            dataReset();
                        }
                    }
                    break;
            }
        }

        private bool JobCompleteUIChecker()
        {
            bool CHECKER = false;

            if(NOWOKCNT >= GETOKCNT)
            {
                CHECKER = true;
            }
            Console.WriteLine("CHECKER " + NOWOKCNT + " " + GETOKCNT);
            return CHECKER;
        }
        
        private void JobComplete()
        {
            Console.WriteLine("JobComple");
            lblresult.Text = "OK";
            lblWarning.Text = "SAB 조립\n\n작업 완료";
            this.Invoke(new Action(delegate ()
            {
                wp_ResultOK.Play();
                lblWarning.Visible = true;
            }));
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((object state) =>
            {
                this.Invoke(new Action(delegate ()
                {
                    lblWarning.Text = "";
                    lblresult.Text = "";
                
                    if (lblWarning.Visible)
                    {
                        lblWarning.Visible = false;
                    }
                }));
                timer.Dispose();
            }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
            mUTIL.SendDB("sp_JIT_INS_SUB_SAB" + "," + lblcode.Text + "," + lbllrstat.Text + "," + m_strHarnessBarcode + "," + m_strSabBarcode + "," + m_strFrameBarcode
            + "," + m_strLsuptBarcode + "," + m_strMotorBarcode + "," + m_strMatBarcode + "," + " " + "," + " " + "," + " " + "," + " " + "," + lbltool1result1.Text + "," + lbltool1result2.Text + "," + lbltool2result1.Text + "," + lbltool2result2.Text + "," +
            " " + "," + " " + "," + " " + "," + " " + "," + " " + "," + " " + "," + "OK" + "," + USER_ID + "," + USER_NAME);
            LastWorkListUpdate();
            lblworksequence.Text = NowWorkCount(lbllrstat.Text.Substring(0, 1));
        }

        public void TimerStat_Test()
        {
            Ping pingSender = new Ping();
            PingReply reply;

            //DB
            if (PingTestCnt > 49)
            {
                PingTestCnt = 0;
                //db
                reply = pingSender.Send(INI.FRT.DB_IP);
                if (reply.Status == IPStatus.Success)
                {
                    lbldbstat.BackColor = Color.LimeGreen;
                    lbldbstat.ForeColor = Color.White;
                }
                else
                {
                    lbldbstat.BackColor = Color.Red;
                    lbldbstat.ForeColor = Color.White;
                }

                //tool
                reply = pingSender.Send(INI.FRT.NUT1IP);

                if (reply.Status == IPStatus.Success)
                {
                    lbltoolstat.BackColor = Color.LimeGreen;
                    lbltoolstat.ForeColor = Color.White;
                }
                else 
                {
                    lbltoolstat.BackColor = Color.Red;
                    lbltoolstat.ForeColor = Color.White;
                }

                /*
                //Clamp adam
                reply = pingSender.Send(Program.ADAMIP);

                if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
                {
                    lblclampstat.BackColor = Color.LimeGreen;
                    lblclampstat.ForeColor = Color.Black;
                }
                else //핑이 제대로 들어가지 않고 있을 경우 
                {
                    lblclampstat.BackColor = Color.Red;
                    lblclampstat.ForeColor = Color.White;
                }
                */
            }

            PingTestCnt++;

            if(LoginoutCnt > 6000)
            {
                if (!pictureBox2.Visible)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        pictureBox2.Visible = true;
                    }));
                }
                //변수 오버 플로우 방지
                LoginoutCnt = 7000;
            }
            
            LoginoutCnt++;
            
        }


        private void timer_time_Tick(object sender, EventArgs e)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lblTime.Text = today;
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(Refresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb -= new PeripheralControlCenter.Tool1CB(Tool1Result);
            this.Dispose();
            this.Close();
            System.Diagnostics.Process[] baseProcess;
            baseProcess = System.Diagnostics.Process.GetProcesses();
            baseProcess = System.Diagnostics.Process.GetProcessesByName("POP");
            for (int i = 0; i < baseProcess.Length; i++) baseProcess[i].Kill();

            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!listBox1.Visible)
            {
                listBox1.Visible = true;
            }
            else
            {
                listBox1.Visible = false;
            }
        }
        private void logboxAdd(string msg)
        {
            string today = DateTime.Now.ToString("yyyyMMddHHmmss");
            listBox1.Items.Add("[" + today + "] " + msg);
        }
    }
}
