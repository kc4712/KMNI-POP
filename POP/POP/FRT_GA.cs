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
    public partial class FRT_GA : Form
    {
        private static System.Timers.Timer mUiTimer;

        private static SoundPlayer wp_OK = new SoundPlayer("SGPOP/OK.wav");
        private static SoundPlayer wp_NG = new SoundPlayer("SGPOP/NG.wav");
        private static SoundPlayer wp_NG2 = new SoundPlayer("SGPOP/NG2.wav");
        private static SoundPlayer wp_ResultOK = new SoundPlayer("SGPOP/RESULT_OK.wav");
        private static SoundPlayer wp_BarcodeOK = new SoundPlayer("SGPOP/BarcodeOK.wav");
        private static SoundPlayer wp_BarcodeNG2 = new SoundPlayer("SGPOP/BarcodeNG2.wav");

        private static int UIFLICKERTIME = 2000;

        private string USER_ID = "";
        private string USER_NAME = "";
        private int LoginoutCnt = 0;
        private UTIL mUTIL = new UTIL();
        private Frm_WorkStandard mFrm_WorkStandard = new Frm_WorkStandard();
        private int PingTestCnt = 0;

        private PeripheralControlCenter mPCC;

        public FRT_GA()
        {
            InitializeComponent();
        }

        private void FRT_GA_Load(object sender, EventArgs e)
        {
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
                label33.Text = "SAB 가조립 (LH)";
            }
            if (INI.FRT.Direction == "R")
            {
                lbllrstat.Text = "RH";
                label33.Text = "SAB 가조립 (RH)";
            }
            lblinfo.Text = "SAB, 하네스, L/SUPT 이름 확인 할 것";
            mUiTimer = new System.Timers.Timer();
            mUiTimer.Interval = 100;
            mUiTimer.Elapsed += new ElapsedEventHandler(Refresh);
            mPCC = new PeripheralControlCenter();
            mPCC.rs232barcodeCb += new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            
            mUiTimer.Start();
            pictureBox2.Load("Pictures/user image.png");
            pictureBox2.Visible = true;
            listBox1.Visible = false;
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(Refresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.deInit();
            this.Dispose();
            this.Close();
            System.Diagnostics.Process[] baseProcess;
            baseProcess = System.Diagnostics.Process.GetProcesses();
            baseProcess = System.Diagnostics.Process.GetProcessesByName("POP");
            for (int i = 0; i < baseProcess.Length; i++) baseProcess[i].Kill();

            Application.Exit();
        }
        
        public void Refresh(object sender, ElapsedEventArgs e)
        {
            TimerStat_Test();
        }
        
        
        public void dataReset()
        {
            Console.WriteLine("dataReset Called");
            
            lblharnessDBbar.Text = "";
            lblsabDBbar.Text = "";
            lbllsuptDBbar.Text = "";
            lblframeDBbar.Text = "";
            lblventDBbar.Text = "";
            lblbackrecDBbar.Text = "";
            //eBARCODEMAINRESULTSTAT = BARCODERESULTSTAT.NONE;   
        }
        public void CheckMAIN_Barcode(string strBarcode)
        {
            //ex : 06Y4219800000000RP4261625612V557819358TL119074B06AR9001
            //string mainex = "06Y4219800000000RP4261625612V557819358TL119074B06AR9001";
            Console.WriteLine("length: "+ strBarcode.Length+ "barcode-> "+ strBarcode);            // first barcode DB 체크
            Console.WriteLine(" strBarcode.Substring(54, 4) " + strBarcode.Substring(46, 4) + " strBarcode.Substring(50)" + strBarcode.Substring(50,1));
            string strReceiveData = "";
            strReceiveData = mUTIL.SendDB("sp_JIT_GET_OPTION_SAB_SUB_GA" + "," + strBarcode.Substring(46, 4) +","+ strBarcode.Substring(50, 1));
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);

            string[] strArr = strReceiveData.Split(',');
            //m_strLotNO = strArr[2];
            foreach (string str in strArr)
            {
                Console.WriteLine("db parsing" + str);
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
                lblworkdate.Text = "";// 작업수량 미정
                if (strBarcode.Substring(50, 1) == "L") lblharnessDBbar.Text = strArr[46];
                if (strBarcode.Substring(50, 1) == "R") lblharnessDBbar.Text = strArr[48];
                if (strBarcode.Substring(50, 1) == "L") lblsabDBbar.Text = strArr[64].Substring(4);
                if (strBarcode.Substring(50, 1) == "R") lblsabDBbar.Text = strArr[66].Substring(4);
                if (strBarcode.Substring(50, 1) == "L")  lblframeDBbar.Text = strArr[74].Substring(4);
                if (strBarcode.Substring(50, 1) == "R")  lblframeDBbar.Text = strArr[76].Substring(4);
                lbllsuptDBbar.Text = strArr[34];
                if (strBarcode.Substring(50, 1) == "L") lblbackrecDBbar.Text = strArr[22];
                if (strBarcode.Substring(50, 1) == "R") lblbackrecDBbar.Text = strArr[24];
                lblventDBbar.Text = strArr[36];

                lblresult.Text = "OK";
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object state) =>
                {
                    lblresult.Text = "";
                    timer.Dispose();
                }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));

            }
            else
            {
                lblresult.Text = "NG";
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    wp_NG2.Play();
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((object state) =>
                    {
                        lblresult.Text = "";
                        timer.Dispose();
                    }, null, TimeSpan.FromMilliseconds(UIFLICKERTIME), TimeSpan.FromMilliseconds(-1));
                }));
                thread.Start();
            }
        }

        public void CheckBarcode(string strBarcode)
        {
            Console.WriteLine("CheckBarcode callback " + strBarcode);
            logboxAdd("Barcode:" + strBarcode + "," + " Length:" + strBarcode.Length);
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
                        int i = 0;
                        foreach (string str in strArr)
                        {
                            Console.WriteLine(i + " db parsing" + str);
                            i++;
                        }
                        if (strArr[2] != "")
                        {
                            pictureBox2.Visible = false;

                            lblname.Text = strArr[2];
                            USER_ID = strBarcode;
                            USER_NAME = strArr[2];
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
                        if (strBarcode == "END")
                        {
                            USER_ID = "";
                            USER_NAME = "";
                            lblname.Text = "";
                            pictureBox2.Visible = true;
                        }
                        break;
                    case (int)BARCODE_LENGTH_ID.MAIN:
                        int judgeMain_SAB = 0;
                        int.TryParse(strBarcode.Substring(46, 4), out judgeMain_SAB);
                        if (judgeMain_SAB == 0)// && eBARCODEMAINRESULTSTAT != BARCODERESULTSTAT.OK)
                        {
                            CheckMAIN_Barcode(strBarcode);
                        }
                        break;
                }
                
            }
        }

        public void TimerStat_Test()
        {
            Ping pingSender = new Ping();
            PingReply reply;

            //DB
            PingTestCnt++;
            if (PingTestCnt > 49)
            {
                PingTestCnt = 0;
                //db
                reply = pingSender.Send(INI.FRT.DB_IP);
                if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
                {
                    lbldbstat.BackColor = Color.LimeGreen;
                    lbldbstat.ForeColor = Color.White;
                }
                else
                {
                    lbldbstat.BackColor = Color.Red;
                    lbldbstat.ForeColor = Color.White;
                }
                if(SerialComm.SerialComm.PortList.Length > 0)
                {
                    lblscanstat.BackColor = Color.LimeGreen;
                    lblscanstat.ForeColor = Color.White;
                }
                else
                {
                    lblscanstat.BackColor = Color.Red;
                    lblscanstat.ForeColor = Color.White;
                }
            }
            if (LoginoutCnt > 6000)
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
            lblworkdate.Text = DateTime.Now.ToString("yyyyMMdd");
        }
        
        private void btnexit_Click(object sender, EventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(Refresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
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
