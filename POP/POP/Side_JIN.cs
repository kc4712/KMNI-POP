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

namespace POP
{
    public partial class Side_JIN : Form
    {
        private System.Timers.Timer mUiTimer;

        private static bool barcodeRestult = false;

        private string torque1 = "";
        private string torque2 = "";
        private string torque3 = "";
        private string torque4 = "";
        private string torque5 = "";
        private string torque6 = "";
        private string torque7 = "";

        private static SoundPlayer wp_OK = new SoundPlayer("SGPOP/OK.wav");
        private static SoundPlayer wp_NG = new SoundPlayer("SGPOP/NG.wav");
        private static SoundPlayer wp_NG2 = new SoundPlayer("SGPOP/NG2.wav");
        private static SoundPlayer wp_ResultOK = new SoundPlayer("SGPOP/RESULT_OK.wav");
        private static SoundPlayer wp_BarcodeOK = new SoundPlayer("SGPOP/BarcodeOK.wav");
        private static SoundPlayer wp_BarcodeNG2 = new SoundPlayer("SGPOP/BarcodeNG2.wav");

        private static int Tool1_OK_Count = 0;
        private static int Tool2_OK_Count = 0;
        
        //private string m_strSerialData = "";

        private string m_strPLAN_DATE = "";
        private string m_strPLAN_SEQUENCE = "";
        //private string m_strLR = "";
        private string m_strLotNO = "";
        private string m_strSAB_Barcode = "";

        private int PingTestCnt = 0;
        private static int UIREFRESH_CNT = 1000;
        private int LoginoutCnt = 0;

        private UTIL mUTIL = new UTIL();
        private Frm_WorkStandard mFrm_WorkStandard = new Frm_WorkStandard();
        string USER_ID = "";
        string USER_NAME = "";
        //bool[] ADAMDATA = { };

        private PeripheralControlCenter mPCC;

        public Side_JIN()
        {
            InitializeComponent();
        }

        #region FormCtrl
        private void Frm_Main_Load(object sender, EventArgs e)
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                Screen scr = (screens[0].WorkingArea.Contains(this.Location)) ? screens[1] : screens[0];
                mFrm_WorkStandard.Show();
                mFrm_WorkStandard.Location = new System.Drawing.Point(scr.Bounds.Left, 0);
                mFrm_WorkStandard.WindowState = FormWindowState.Maximized;
            }
            pictureBox3.Load("Pictures/user image.png");
            pictureBox3.Visible = true;
            lblClampWarning.Visible = false;
            listBox1.Visible = false;
            /*string test = "[)>06P4269061212V557819358TL119066CMKL0005";
            string testt = ")> 06P4269061212V557819358TL119066CMKL0005JBLR157361502020128[)> 06P4269061212V557819358TL119066CMKL0005JBLR157361502020128
            [)> 06P4269061212V557819358TL119066CMKL0005
            [)>06P4269061212V557819358TL119066CMKL0005
            [)>06P4269061212V557819358TL119066CMKL0005
            [)>06P4269061212V557819358TL119066CMKL0005JBLR157361502020128JBLR157361502020128
            [)>06P4269061212V557819358TL119066CMKL0005[)>06P4269061212V557819358TL119066CMKL0005JBLR157361502020128
            [)>06P4269061212V557819358TL119066CMKL0005JBLR15736150202012
            8[)>06P4269061212V557819358TL119066CMKL0005JBLR157361502020128
            [)>06P4269061212V557819358TL119066CMKL0005JBLR157361502020128
            [)>06P4269061212V557819358TL119066CMKL0005[)>06P4269061212V557819358TL119066CMKL0005";

            Console.WriteLine("test.Substring(47) " + test.Length);
            Console.WriteLine("test.Substring(47) " + test.Substring(30));*/
            if (INI.SIDE.Direction == "L")
            {
                lblLR.Text = "L";
                button2.Text = "RH";
            }
            if (INI.SIDE.Direction == "R")
            {
                lblLR.Text = "R";
                button2.Text = "LH";
            }


            mPCC = new PeripheralControlCenter();
            mPCC.rs232barcodeCb += new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb += new PeripheralControlCenter.Tool1CB(Tool1Result);
            mPCC.Tool2Cb += new PeripheralControlCenter.Tool2CB(Tool2Result);

            lblDisplayDay.Text = "당일\n작업수량";

            bool bResult;
            bResult = check9BUX_Barcode_workselect();
            if (bResult)
            {
                LoadAmount();
                LoadProductionData();
                //CheckBarcode("06Y4220300000000LP4269291312V557819358TL119079BBKL0001");
            }
            else
            {

                lblMessage.Text = "생산계획이 없습니다.";
                lblBARCHECK.Text = "";
                // SETCODE
                lblSETCODE.Text = "";
                // PJTCD
                lblPJTCD.Text = "";
                // CLOTH
                lblCLOTH.Text = "";
                // COLOR
                lblCOLOR.Text = "";
                //lblBarcode.Text = strArr[2];
                m_strPLAN_DATE = "";
                m_strPLAN_SEQUENCE = "";
                lblITEMNO.Text = "";
                lblDAY_JOB.Text = "";
                lblCUR_JOB.Text = "";
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }

            mUiTimer = new System.Timers.Timer();
            mUiTimer.Interval = 100;
            mUiTimer.Elapsed += new ElapsedEventHandler(UiRefresh);


            mUiTimer.Start();

        }
        
        private void timer_time_Tick(object sender, EventArgs e)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lblTime.Text = today;
            //Console.WriteLine(today);
        }
        
        private void button1_Click(object sender, EventArgs e) // 닫기
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(UiRefresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            
            this.Dispose();
            this.Close();
            mFrm_WorkStandard.Close();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb -= new PeripheralControlCenter.Tool1CB(Tool1Result);
            mPCC.Tool2Cb -= new PeripheralControlCenter.Tool2CB(Tool2Result);
            mPCC.deInit();
            
            System.Diagnostics.Process[] baseProcess;
            baseProcess = System.Diagnostics.Process.GetProcesses();
            baseProcess = System.Diagnostics.Process.GetProcessesByName("POP");
            for (int i = 0; i < baseProcess.Length; i++) baseProcess[i].Kill();
            Application.Exit();

        }
        
        private void Frm_Main_Closing(object sender, FormClosingEventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(UiRefresh);
            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            
            this.Dispose();
            this.Close();
            mFrm_WorkStandard.Close();
            mPCC.rs232barcodeCb -= new PeripheralControlCenter.rs232barcodeEventCb(CheckBarcode);
            mPCC.Tool1Cb -= new PeripheralControlCenter.Tool1CB(Tool1Result);
            mPCC.Tool2Cb -= new PeripheralControlCenter.Tool2CB(Tool2Result);
            mPCC.deInit();
            
            System.Diagnostics.Process[] baseProcess;
            baseProcess = System.Diagnostics.Process.GetProcesses();
            baseProcess = System.Diagnostics.Process.GetProcessesByName("POP");
            for (int i = 0; i < baseProcess.Length; i++) baseProcess[i].Kill();
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e) //좌우 표기 교체
        {
            if (lblLR.Text == "R")
            {
                lblLR.Text = "L";
                button2.Text = "RH";
            }
            else
            {
                lblLR.Text = "R";
                button2.Text = "LH";
            }
            bool bResult;
            bResult = check9BUX_Barcode_workselect();
            if (bResult)
            {
                LoadAmount();
                LoadProductionData();
                //CheckBarcode("06Y4220300000000LP4269291312V557819358TL119079BBKL0001");
            }
            else
            {

                lblMessage.Text = "생산계획이 없습니다.";
                lblBARCHECK.Text = "";
                // SETCODE
                lblSETCODE.Text = "";
                // PJTCD
                lblPJTCD.Text = "";
                // CLOTH
                lblCLOTH.Text = "";
                // COLOR
                lblCOLOR.Text = "";
                //lblBarcode.Text = strArr[2];
                m_strPLAN_DATE = "";
                m_strPLAN_SEQUENCE = "";
                lblITEMNO.Text = "";
                lblDAY_JOB.Text = "";
                lblCUR_JOB.Text = "";
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }
        }

        private void btnJobCancel_Click(object sender, EventArgs e)
        {
            Tool1_OK_Count = 0;
            Tool2_OK_Count = 0;

            barcodeRestult = false;
            dataReset("a");
        }

        private void btnJobComplete_Click(object sender, EventArgs e)
        {
            // 현재 작업 완료
            /*
            frmJobComplete frm = new frmJobComplete();

            frm.setLR(lblLR.Text);
            frm.Show();*/
        }
#endregion


        public void ResultLabelOK(string str)
        {
            this.Invoke(new Action(delegate ()
            {
                pictureBox2.Load("Pictures/OK.bmp");
                lblMessage.Text = "신규 바코드 입니다.";
                lblMessage.BackColor = Color.YellowGreen;

            }));
        }

        public void ResultLabelResultOK(string str)
        {
            this.Invoke(new Action(delegate ()
            {
                pictureBox2.Load("Pictures/OK.bmp");

                lblMessage.Text = "합격 입니다.";
                lblMessage.BackColor = Color.YellowGreen;

            }));
        }

        public void ResultLabelNG(string str)
        {
            this.Invoke(new Action(delegate ()
            {
                pictureBox2.Load("Pictures/NG.bmp");
                lblMessage.Text = "바코드 매칭 불량 입니다.";

            }));

        }

        public void ResultLabelOver(string str)
        {
            this.Invoke(new Action(delegate ()
            {
                pictureBox2.Load("Pictures/OverLap.bmp");
                lblMessage.Text = "중복 바코드 입니다.";

            }));
        }

        public void ResultLabelClear(string str)
        {
            this.Invoke(new Action(delegate ()
            {
                pictureBox2.Load("Pictures/Normal.bmp");
                lblMessage.Text = "";
                lblMessage.BackColor = Color.Black;
            }));
        }


        public void dataReset(string str)
        {
            lblBarcode.Text = "";
            lbl_TQ1.Text = "";
            lbl_TQ2.Text = "";
            lbl_TQ3.Text = "";
            lbl_TQ4.Text = "";
            lbl_TQ5.Text = "";
            lbl_TQ6.Text = "";
            lbl_TQ7.Text = "";

            m_strSAB_Barcode = "";

            Tool1_OK_Count = 0;
            Tool2_OK_Count = 0;

            lbl_TQ1_Check.ForeColor = Color.White;
            lbl_TQ1_Check.BackColor = Color.Red;

            lbl_TQ2_Check.ForeColor = Color.White;
            lbl_TQ2_Check.BackColor = Color.Red;

            lbl_TQ3_Check.ForeColor = Color.White;
            lbl_TQ3_Check.BackColor = Color.Red;

            lbl_TQ4_Check.ForeColor = Color.White;
            lbl_TQ4_Check.BackColor = Color.Red;

            lbl_TQ5_Check.ForeColor = Color.White;
            lbl_TQ5_Check.BackColor = Color.Red;

            lbl_TQ6_Check.ForeColor = Color.White;
            lbl_TQ6_Check.BackColor = Color.Red;

            lbl_TQ7_Check.ForeColor = Color.White;
            lbl_TQ7_Check.BackColor = Color.Red;
        }

        bool clamp = false;
        public void UiRefresh(object sender, ElapsedEventArgs e)
        {
            //ADAMDATA = mPCC.AdamRead();
            //Thread.Sleep(200);
            //Console.WriteLine(ADAMDATA.Length);
            //foreach( bool test in ADAMDATA)
            //{
            //    Console.WriteLine(test);
            //}
            //Console.WriteLine("ADAMDATA[0]" + ADAMDATA[0]);
            checkToolPosition();
            //Console.WriteLine("ADAM_TCP.ADAM.sBuffer_AI_01[0] " + ADAM_TCP.ADAM.sBuffer_AI_01[0]);
            // 클램프 ON
            if (ADAM_TCP.ADAM.sBuffer_AI_01[0] == 1)
            //if (mPCC.AdamRead()[0] == 1)
            {
                lblSTATUS_CLAMP.BackColor = Color.LimeGreen;
                lblSTATUS_CLAMP.ForeColor = Color.Black;
                if (clamp)
                {
                    logboxAdd("Clamp: ON");
                    bool bResult;
                    bResult = check9BUX_Barcode_workselect();
                    if (bResult)
                    {
                        ADAM_TCP.ADAM.WriteData(0, 12, 0);
                        //mPCC.AdamWrite(0);
                        LoadAmount();
                        //LoadProductionData();
                        //CheckBarcode("06Y4220300000000LP4269291312V557819358TL119079BBKL0001");
                    }
                    else
                    {
                        lblMessage.Text = "생산계획이 없습니다.";
                        lblBARCHECK.Text = "";
                        // SETCODE
                        lblSETCODE.Text = "";
                        // PJTCD
                        lblPJTCD.Text = "";
                        // CLOTH
                        lblCLOTH.Text = "";
                        // COLOR
                        lblCOLOR.Text = "";
                        //lblBarcode.Text = strArr[2];
                        m_strPLAN_DATE = "";
                        m_strPLAN_SEQUENCE = "";
                        lblITEMNO.Text = "";
                        lblDAY_JOB.Text = "";
                        lblCUR_JOB.Text = "";
                        dataGridView1.Rows.Clear();
                        dataGridView1.Refresh();
                    }
                    clamp = false;
                }

                //작업완료 루틴
                if (barcodeRestult && (Tool1_OK_Count == 2) &&
                    (Tool2_OK_Count == 5))
                {
                    barcodeRestult = false;
                    Thread thread = new Thread(new ThreadStart(delegate ()
                    {
                        ResultLabelResultOK("a");
                        wp_ResultOK.Play();
                        System.Threading.Timer timer = null;
                        timer = new System.Threading.Timer((object state) =>
                        {
                            ResultLabelClear("a");
                            timer.Dispose();
                        }, null, TimeSpan.FromMilliseconds(UIREFRESH_CNT), TimeSpan.FromMilliseconds(-1));
                    }));
                    thread.Start();

                    // DO =  1
                    //ADAM_TCP.ADAM.WriteData(0, 12, 1);
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "-WRITE 1" + "", false, true);

                    lblSTATUS_CLAMP.BackColor = Color.Red;
                    lblSTATUS_CLAMP.ForeColor = Color.White;
                    logboxAdd("Clamp: OFF");
                    // send to db
                    mUTIL.SendDB("sp_SUB_OUTPUT_UPDATE," + m_strLotNO + "," + torque1 + "," + torque2 + "," + torque3 + "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7 + "," + USER_ID + "," + USER_NAME);

                    //string msg = "sp_SUB_OUTPUT_UPDATE," + m_strLotNO + "," + torque1 + "," + torque2 + "," + torque3 + "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7;
                    //LogMsg.CMsg.Show("SendDB", "sp_SUB_UPDATE", msg, false, true);

                    // release clamp
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "", false, true);

                    dataReset("a");

                    bool bResult;
                    bResult = check9BUX_Barcode_workselect();
                    if (bResult)
                    {
                        LoadAmount();
                        LoadProductionData();
                        //바코드 임의 테스트
                        //CheckBarcode("06Y4220300000000LP4269291312V557819358TL119079BBKL0001");
                    }
                    else
                    {
                        //LoadAmount();
                        //LoadProductionData();
                        lblMessage.Text = "생산계획이 없습니다.";
                        lblBARCHECK.Text = "";
                        // SETCODE
                        lblSETCODE.Text = "";
                        // PJTCD
                        lblPJTCD.Text = "";
                        // CLOTH
                        lblCLOTH.Text = "";
                        // COLOR
                        lblCOLOR.Text = "";
                        //lblBarcode.Text = strArr[2];
                        m_strPLAN_DATE = "";
                        m_strPLAN_SEQUENCE = "";
                        lblITEMNO.Text = "";
                        lblDAY_JOB.Text = "";
                        lblCUR_JOB.Text = "";
                        dataGridView1.Rows.Clear();
                        dataGridView1.Refresh();
                    }
                    //ADAM_TCP.ADAM.WriteData(0, 12, 1);
                    //Thread.Sleep(500);
                    //// DIO  = 0 Reset
                    //ADAM_TCP.ADAM.WriteData(0, 12, 0);
                    mPCC.ClampRelease();
                    Tool1_OK_Count = 0;
                    Tool2_OK_Count = 0;

                }

            }
            else
            {
                lblSTATUS_CLAMP.BackColor = Color.Red;
                lblSTATUS_CLAMP.ForeColor = Color.White;
                clamp = true;
                Tool1_OK_Count = 0;
                Tool2_OK_Count = 0;

                barcodeRestult = false;
                dataReset("a");

            }

            PingTestCnt++;
            if (PingTestCnt > 49)
            {
                PingTestCnt = 0;
                TimerPing_Test();
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

        public void CheckBarcode(string strBarcode)
        {
            logboxAdd("Barcode:" + strBarcode + "," + " Length:" + strBarcode.Length);
            LoginoutCnt = 0;
            Console.WriteLine(strBarcode + "," + strBarcode.Length);
            if (pictureBox3.Visible)
            {
                if (strBarcode.Length == 4 && strBarcode != "9999")
                {
                    string strReceiveData = "";
                    strReceiveData = mUTIL.SendDB("sp_JIT_SEL_USER" + "," + strBarcode.Trim());
                    //strReceiveData = SendDB("sp_JIT_GET_OPTION_SAB_SUB_JIN" + "," + "B01A" + "," + "L");
                    //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);

                    string[] strArr = strReceiveData.Split(',');
                    //m_strLotNO = strArr[2];
                    int i = 0;
                    foreach (string str in strArr)
                    {
                        Console.WriteLine(i + " db parsing" + str);
                        i++;
                    }
                    if (strArr[2] != "")
                    {
                        pictureBox3.Visible = false;
                        //lblUser.Text = strArr[2];
                        lbl_user_nm.Text = strArr[2];
                        USER_ID = strBarcode;
                        USER_NAME = strArr[2];
                    }
                }
            }
            else
            {
                if (strBarcode.Length == 4 && strBarcode == "9999" || strBarcode.Length == 3 && strBarcode == "END")
                {
                    this.Invoke(new Action(delegate ()
                    {
                        pictureBox3.Visible = true;
                    }));
                    lbl_user_nm.Text = "";
                    USER_ID = "";
                    USER_NAME = "";
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_01[0] != 1)
                //if (mPCC.AdamRead()[0] != 1)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        lblClampWarning.Visible = true;
                    }));
                    System.Threading.Timer timer = null;
                    timer = new System.Threading.Timer((object state) =>
                    {
                        lblClampWarning.Visible = false;
                        timer.Dispose();
                    }, null, TimeSpan.FromMilliseconds(UIREFRESH_CNT), TimeSpan.FromMilliseconds(-1));
                    Console.WriteLine("lblSTATUS_CLAMP.BackColor");
                    return;
                }

                string strResult = "";
                /// <summary>
                /// 바코드 중복 체크용 플래그... 두번의 DB질의가 통과되어야 해서 사용중
                /// </summary>
                /// <param name="TAG_CHECK"></param>
                bool TAG_CHECK = false;
                Console.WriteLine("CheckBarcode callback " + strBarcode);
                if (strBarcode != "")
                {
                    Console.WriteLine("check9BUX_Barcode(strBarcode) " + strResult + " " + strBarcode.Substring(54, 3) + " " + strBarcode.Substring(47, 15));
                    // ex : 06Y4220300000000LP4269291312V557819358TL119079BBKL002
                    if (strBarcode.Length > 20 && strBarcode.Substring(54, 3) == lblSETCODE.Text)
                    {

                        // barcode 체크
                        //strResult = check_db_barcode(strBarcode.Substring(38,15));
                        string strReceiveData = "";

                        //strResult = check_db_barcode("L119079BBKL002");
                        m_strLotNO = strBarcode.Substring(47, 15);
                        strReceiveData = mUTIL.SendDB("sp_SUB_CHECK_BARCODE" + "," + strBarcode.Substring(47, 15));
                        //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);

                        string[] strArr = strReceiveData.Split(',');
                        foreach (string str in strArr)
                        {
                            Console.WriteLine("check_db_barcode(strBarcode) " + str);
                        }
                        Console.WriteLine("strArr[0] " + strArr[0]);
                        Console.WriteLine("strArr[1] " + strArr[1]);
                        if (strArr[2] == "1")
                        {
                            TAG_CHECK = true;
                        }
                        else
                        {
                            TAG_CHECK = false;
                        }

                        strReceiveData = mUTIL.SendDB("sp_SUB_CHECK_BARCODE2" + "," + strBarcode.Substring(47, 15));
                        string[] strArr1 = strReceiveData.Split(',');
                        foreach (string str in strArr1)
                        {
                            Console.WriteLine("sp_SUB_CHECK_BARCODE2(strBarcode) " + str);
                        }
                        Console.WriteLine("strArr1[0] " + strArr1[0]);
                        Console.WriteLine("strArr1[1] " + strArr1[1]);
                        if (strArr1[2] == "0")
                        {
                            TAG_CHECK = true;
                        }
                        else
                        {
                            TAG_CHECK = false;
                        }

                        /*if(strBarcode.Substring(58, 1) != Program.Direction)
                        {
                            Console.WriteLine("좌우 반전 " + strBarcode);
                            lblBarcode.Text = "";

                            barcodeRestult = false;

                            Thread thread = new Thread(new ThreadStart(delegate ()
                            {
                                ResultLabelNG("a");
                                wp_NG2.Play();
                                System.Threading.Timer timer = null;
                                timer = new System.Threading.Timer((object state) =>
                                {
                                    ResultLabelClear("a");
                                    timer.Dispose();
                                }, null, TimeSpan.FromMilliseconds(UIREFRESH_CNT), TimeSpan.FromMilliseconds(-1));

                            }));
                            thread.Start();

                            LogMsg.CMsg.Show("Barcode", "매칭 불량", " -- NG", false, true);
                            barcodeRestult = false;
                            TAG_CHECK = false;
                            return;
                        }*/


                    }
                    else
                    {
                        TAG_CHECK = false;
                    }
                    if (TAG_CHECK)
                    {
                        Console.WriteLine("strResult == OK " + strBarcode);
                        Console.WriteLine("strBarcode.Substring(47) " + strBarcode.Substring(30));
                        m_strSAB_Barcode = strBarcode;
                        lblBarcode.Text = strBarcode.Substring(47, 15);
                        //this.Invoke(new Action(delegate ()
                        //{
                        //    lblBarcode.Text = strBarcode.Substring(47);
                        //}));

                        barcodeRestult = true;
                        Thread thread = new Thread(new ThreadStart(delegate ()
                        {
                            ResultLabelOK("a");
                            wp_OK.Play();
                            System.Threading.Timer timer = null;
                            timer = new System.Threading.Timer((object state) =>
                            {
                                ResultLabelClear("a");
                                timer.Dispose();
                            }, null, TimeSpan.FromMilliseconds(UIREFRESH_CNT), TimeSpan.FromMilliseconds(-1));
                        }));
                        thread.Start();
                    }
                    else //if (strResult == "불량")
                    {
                        Console.WriteLine("strResult == 불량 " + strBarcode);
                        lblBarcode.Text = "";

                        barcodeRestult = false;

                        Thread thread = new Thread(new ThreadStart(delegate ()
                        {
                            ResultLabelOver("a");
                            wp_BarcodeNG2.Play();
                            System.Threading.Timer timer = null;
                            timer = new System.Threading.Timer((object state) =>
                            {
                                ResultLabelClear("a");
                                timer.Dispose();
                            }, null, TimeSpan.FromMilliseconds(UIREFRESH_CNT), TimeSpan.FromMilliseconds(-1));

                        }));
                        thread.Start();

                        //LogMsg.CMsg.Show("Barcode", "매칭 불량", " -- NG", false, true);
                    }
                }
            }
        }


#region TOOLPART

        public void Tool1Result(double torque)
        {
            logboxAdd("Tool1:" + String.Format("{0}", torque));
            LoginoutCnt = 0;
            if (!barcodeRestult || lblSTATUS_CLAMP.BackColor == Color.Red)
            {
                Console.WriteLine("바코드 혹은 클램프 통과 못함.");
                return;
            }
            Console.WriteLine("torque nut1 " + torque);
            if (torque != -1)
            {
                //check check
                Console.WriteLine("torque nut1 " + torque);
                checkTool1Data(torque);
                //Thread.Sleep(100);
                if (Tool1_OK_Count == 2)
                {
                    //LogMsg.CMsg.Show("ThreadTool1", "- STOP ", "", false, true);

                    if (Tool2_OK_Count != 3)
                    {
                        wp_OK.Play();
                    }

                }
                //Console.WriteLine("ThreadTool1, nut1.dataClear();");
            }
        }

        public void Tool2Result(double torque)
        {
            logboxAdd("Tool2:" + String.Format("{0}", torque));
            LoginoutCnt = 0;
            if (!barcodeRestult || lblSTATUS_CLAMP.BackColor == Color.Red)
            {
                Console.WriteLine("바코드 혹은 클램프 통과 못함.");
                return;
            }
            //if (torque != -1 && tightStaus == "1")
            Console.WriteLine("torque nut2 " + torque);
            if (torque != -1)
            {
                Console.WriteLine("torque nut2 " + torque);
                //check check
                checkTool2Data(torque);
                if (Tool2_OK_Count == 5)
                {
                    //LogMsg.CMsg.Show("ThreadTool2", "- STOP ", "", false, true);

                    if (Tool1_OK_Count != 2)
                    {
                        //wp_OK.Play();
                    }
                }
                Console.WriteLine("ThreadTool2, nut2.dataClear();");
            }
        }

        public void checkTool1Data(double Tool1Data)
        {
            double min = Double.Parse(INI.SIDE.JINTOOL1MIN);
            double max = Double.Parse(INI.SIDE.JINTOOL1MAX);
            //Console.WriteLine("Tool1_OK_Count " + ADAM_TCP.ADAM.sBuffer_AI_04[0]);
            if (Tool1_OK_Count == 0)
            {
                // spec check and 위치 센서 1일 때만 값을 넣음
                //Console.WriteLine("ADAM_TCP.ADAM.sBuffer_AI_04[0] " + ADAM_TCP.ADAM.sBuffer_AI_04[0]);
                if (mUTIL.isBetweenSpec(Tool1Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_04[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool1Data, min, max) && mPCC.AdamRead()[3] == 1)
                {

                    lblStatus_Pos1.BackColor = Color.LimeGreen;
                    lbl_TQ1_Check.ForeColor = Color.Black;

                    lbl_TQ1_Check.BackColor = Color.LimeGreen;

                    lbl_TQ1.Text = String.Format("{0:f2}", Tool1Data);
                    //Console.WriteLine("lbl_TQ1.Text " + lbl_TQ1.Text + " Tool1Data " + Tool1Data);    
                    torque1 = String.Format("{0:f2}", Tool1Data);

                    // ok spec Count increase + 1
                    Tool1_OK_Count = Tool1_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool1Data", "checktool1 - ", Tool1_OK_Count.ToString(), false, true);
                }

            }
            else if (Tool1_OK_Count == 1)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool1Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_05[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool1Data, min, max) && mPCC.AdamRead()[4] == 1)
                {
                    lblStatus_Pos2.BackColor = Color.LimeGreen;
                    lbl_TQ2_Check.ForeColor = Color.Black;

                    lbl_TQ2_Check.BackColor = Color.LimeGreen;

                    lbl_TQ2.Text = String.Format("{0:f2}", Tool1Data);
                    torque2 = String.Format("{0:f2}", Tool1Data);

                    // ok spec Count increase + 1
                    Tool1_OK_Count = Tool1_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool1Data", "checktool1 ", " - 2 ok", false, true);

                }
            }
        }

        public void checkTool2Data(double Tool2Data)
        {
            double min = Double.Parse(INI.SIDE.JINTOOL2MIN);
            double max = Double.Parse(INI.SIDE.JINTOOL2MAX);
            if (Tool2_OK_Count == 0)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_06[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool2Data, min, max) && mPCC.AdamRead()[5] == 1)
                {
                    lblStatus_Pos3.BackColor = Color.LimeGreen;
                    lbl_TQ3_Check.ForeColor = Color.Black;

                    lbl_TQ3_Check.BackColor = Color.LimeGreen;

                    lbl_TQ3.Text = String.Format("{0:f2}", Tool2Data);

                    torque3 = String.Format("{0:f2}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 1)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_07[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool2Data, min, max) && mPCC.AdamRead()[6] == 1)
                {
                    lblStatus_Pos4.BackColor = Color.LimeGreen;
                    lbl_TQ4_Check.ForeColor = Color.Black;

                    lbl_TQ4_Check.BackColor = Color.LimeGreen;

                    lbl_TQ4.Text = String.Format("{0:f2}", Tool2Data);

                    torque4 = String.Format("{0:f2}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 2)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_08[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool2Data, min, max) && mPCC.AdamRead()[7] == 1)
                {
                    lblStatus_Pos5.BackColor = Color.LimeGreen;
                    lbl_TQ5_Check.ForeColor = Color.Black;

                    lbl_TQ5_Check.BackColor = Color.LimeGreen;

                    lbl_TQ5.Text = String.Format("{0:f2}", Tool2Data);

                    torque5 = String.Format("{0:f2}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 3)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_09[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool2Data, min, max) && mPCC.AdamRead()[8] == 1)
                {
                    lblStatus_Pos6.BackColor = Color.LimeGreen;
                    lbl_TQ6_Check.ForeColor = Color.Black;

                    lbl_TQ6_Check.BackColor = Color.LimeGreen;

                    lbl_TQ6.Text = String.Format("{0:f2}", Tool2Data);

                    torque6 = String.Format("{0:f2}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 4)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max) && ADAM_TCP.ADAM.sBuffer_AI_10[0] == 1)
                //if (mUTIL.isBetweenSpec(Tool2Data, min, max) && mPCC.AdamRead()[9] == 1)
                {
                    lblStatus_Pos7.BackColor = Color.LimeGreen;
                    lbl_TQ7_Check.ForeColor = Color.Black;

                    lbl_TQ7_Check.BackColor = Color.LimeGreen;

                    lbl_TQ7.Text = String.Format("{0:f2}", Tool2Data);

                    torque7 = String.Format("{0:f2}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
        }
#endregion

        public void checkToolPosition()
        {
            //ADAMDATA = mPCC.AdamRead();
            //Console.WriteLine(ADAMDATA);
            if (ADAM_TCP.ADAM.sBuffer_AI_04[0] == 1)
            //this.Invoke(new Action(delegate ()
            //{
                //if (mPCC.AdamRead()[3] == 1)
                {
                    lblStatus_Pos1.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos1.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_05[0] == 1)
                //if (mPCC.AdamRead()[4] == 1)
                {
                    lblStatus_Pos2.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos2.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_06[0] == 1)
                //if (mPCC.AdamRead()[5] == 1)
                {
                    lblStatus_Pos3.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos3.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_07[0] == 1)
                //if (mPCC.AdamRead()[6] == 1)
                {
                    lblStatus_Pos4.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos4.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_08[0] == 1)
                //if (mPCC.AdamRead()[7] == 1)
                {
                    lblStatus_Pos5.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos5.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_09[0] == 1)
                //if (mPCC.AdamRead()[8] == 1)
                {
                    lblStatus_Pos6.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos6.BackColor = Color.Gray;
                }
                if (ADAM_TCP.ADAM.sBuffer_AI_10[0] == 1)
                //if (mPCC.AdamRead()[9] == 1)
                {
                    lblStatus_Pos7.BackColor = Color.LimeGreen;
                }
                else
                {
                    lblStatus_Pos7.BackColor = Color.Gray;
                }
            //}));
        }

        public void TimerPing_Test()
        {
            Ping pingSender = new Ping();
            //try
            //{
            Console.WriteLine(INI.SIDE.ADAMIP);
            Console.WriteLine(INI.SIDE.NUT1IP);
            // ADAM
            PingReply reply;
            reply = pingSender.Send(INI.SIDE.ADAMIP);
            if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
            {
                lblSTATUS_ADAM.BackColor = Color.LimeGreen;
                lblSTATUS_ADAM.ForeColor = Color.Black;
            }
            else //핑이 제대로 들어가지 않고 있을 경우 
            {
                lblSTATUS_ADAM.BackColor = Color.Red;
                lblSTATUS_ADAM.ForeColor = Color.White;
            }

            reply = pingSender.Send("192.168.40.41");

            if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
            {
                lblSTATUS_DB.BackColor = Color.LimeGreen;
                lblSTATUS_DB.ForeColor = Color.Black;
            }
            else //핑이 제대로 들어가지 않고 있을 경우 
            {
                lblSTATUS_DB.BackColor = Color.Red;
                lblSTATUS_DB.ForeColor = Color.White;
            }

            reply = pingSender.Send(INI.SIDE.NUT1IP);

            if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
            {
                lblSTATUS_TOOL1.BackColor = Color.LimeGreen;
                lblSTATUS_TOOL1.ForeColor = Color.Black;
            }
            else //핑이 제대로 들어가지 않고 있을 경우 
            {
                lblSTATUS_TOOL1.BackColor = Color.Red;
                lblSTATUS_TOOL1.ForeColor = Color.White;
            }

            reply = pingSender.Send(INI.SIDE.NUT2IP);

            if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
            {
                lblSTATUS_TOOL2.BackColor = Color.LimeGreen;
                lblSTATUS_TOOL2.ForeColor = Color.Black;
            }
            else //핑이 제대로 들어가지 않고 있을 경우 
            {
                lblSTATUS_TOOL2.BackColor = Color.Red;
                lblSTATUS_TOOL2.ForeColor = Color.White;
            }
        }

#region DBPART
        public bool check9BUX_Barcode_workselect()
        {
            string strReceiveData = "";
            strReceiveData = mUTIL.SendDB("sp_SUB_GET_OPTION_9BUX" + "," + lblLR.Text);
            //LogMsg.CMsg.Show("Send DB", "Receive Data-", strReceiveData, false, true);
            //Console.WriteLine("strReceiveData: "+ strReceiveData);
            if (strReceiveData.Contains("A_ERROR") || strReceiveData.Contains("LOTNO"))
            {
                return false;
            }
            else// if (strReceiveData != "")
            {
                parsingDB(strReceiveData);

                return true;
            }
        }

        public bool LoadOption()
        {
            string command = "sp_SUB_GET_OPTION,";
            string today = DateTime.Now.ToString("yyyyMMdd");
            string sql = command + "GSUV," + lblLR.Text + "," + today;

            bool bResult = false;

            //LogMsg.CMsg.Show("SendDB", "sp_Option", sql, false, true);

            string receiveData = "";

            // Get Option
            receiveData = mUTIL.SendDB(sql);
            Console.WriteLine("LoadOption:" + receiveData);
            if (receiveData.Contains("A_ERROR"))
            {
                return false;
            }
            else if (receiveData != "")
            {
                parsingDB(receiveData);
                bResult = true;
            }

            return bResult;
        }

        public void LoadAmount()
        {
            string command = "sp_SUB_GET_AMOUNT3";
            string sql = command + "," + lblPJTCD.Text + "," + lblLR.Text + "," +
                lblSETCODE.Text + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE;
            Console.WriteLine("LoadAmount()" + sql);
            //LogMsg.CMsg.Show("SendDB", "sp_Amount", sql, false, true);

            string receiveData = "";

            // Get Option
            receiveData = mUTIL.SendDB(sql);

            if (receiveData != "")
            {
                parsingDB(receiveData);
            }

        }

        public void LoadProductionData()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");

            string command = "sp_SUB_GET_RESULT2";
            string sql = command + "," + "9BUX" + "," + lblLR.Text + "," + today;

            //LogMsg.CMsg.Show("SendDB", "sp_GET_RESULT2", sql, false, true);

            string receiveData = "";

            // Get Option
            receiveData = mUTIL.SendDB(sql);

            if (receiveData != "")
            {
                parsingDB(receiveData);

            }
        }
/*
0 A_sp_SUB_GET_RESULT Parsing -  A_sp_SUB_GET_RESULT
1 A_sp_SUB_GET_RESULT Parsing -  SEQ1
2 A_sp_SUB_GET_RESULT Parsing -  1
3 A_sp_SUB_GET_RESULT Parsing -  Column1
4 A_sp_SUB_GET_RESULT Parsing -  2019-03-22 13:49:29
5 A_sp_SUB_GET_RESULT Parsing -  LOTNO
6 A_sp_SUB_GET_RESULT Parsing -  L119081BBKL0001
7 A_sp_SUB_GET_RESULT Parsing -  BAR1
8 A_sp_SUB_GET_RESULT Parsing -  [)>06Y4730800000000LP6000302012V544663743T11190093020W0005
9 A_sp_SUB_GET_RESULT Parsing -  BAR2
10 A_sp_SUB_GET_RESULT Parsing -  SG181106L0030
11 A_sp_SUB_GET_RESULT Parsing -  TOR1
12 A_sp_SUB_GET_RESULT Parsing -
13 A_sp_SUB_GET_RESULT Parsing -  TOR2
14 A_sp_SUB_GET_RESULT Parsing -
15 A_sp_SUB_GET_RESULT Parsing -  TOR3
16 A_sp_SUB_GET_RESULT Parsing -
17 A_sp_SUB_GET_RESULT Parsing -  TOR4
18 A_sp_SUB_GET_RESULT Parsing -
19 A_sp_SUB_GET_RESULT Parsing -  TOR5
20 A_sp_SUB_GET_RESULT Parsing -
21 A_sp_SUB_GET_RESULT Parsing -  CLOTH
22 A_sp_SUB_GET_RESULT Parsing -  JET BLACK
23 A_sp_SUB_GET_RESULT Parsing -  COLOR
24 A_sp_SUB_GET_RESULT Parsing -  HERA
*/

        public void parsingDB(string str)
        {
            string[] strTemp = str.Split(',');

            // getAmount3 (작업수량)
            if (strTemp[0] == "A_sp_SUB_GET_AMOUNT3")
            {

                lblCUR_JOB.Text = strTemp[1] + "/" + strTemp[2];
                lblDAY_JOB.Text = strTemp[3];
            }
            // getResult (실적)
            else if (strTemp[0] == "A_sp_SUB_GET_RESULT2")
            {
                string[] strArr = str.Split(',');
                string[] strData = new string[9];
                int startIndex = 2;
                foreach (string s in strArr)
                {
                    Console.WriteLine("A_sp_SUB_GET_RESULT2 Parsing -  " + s);
                }

/*
0 A_sp_SUB_GET_RESULT2 Parsing -  A_sp_SUB_GET_RESULT2
1 A_sp_SUB_GET_RESULT2 Parsing -  SEQ1
2 A_sp_SUB_GET_RESULT2 Parsing -  1
3 A_sp_SUB_GET_RESULT2 Parsing -  Column1
4 A_sp_SUB_GET_RESULT2 Parsing -  2019-03-25 14:27:46
5 A_sp_SUB_GET_RESULT2 Parsing -  LOTNO
6 A_sp_SUB_GET_RESULT2 Parsing -  L119084BKBL0002
7 A_sp_SUB_GET_RESULT2 Parsing -  BAR1
8 A_sp_SUB_GET_RESULT2 Parsing -  [)>06Y4730800000000LP6000302012V544663743T11190093020W0005
9 A_sp_SUB_GET_RESULT2 Parsing -  BAR2
10 A_sp_SUB_GET_RESULT2 Parsing -  SG181106L0030
11 A_sp_SUB_GET_RESULT2 Parsing -  TOR1
12 A_sp_SUB_GET_RESULT2 Parsing -
13 A_sp_SUB_GET_RESULT2 Parsing -  TOR2
14 A_sp_SUB_GET_RESULT2 Parsing -
15 A_sp_SUB_GET_RESULT2 Parsing -  TOR3
16 A_sp_SUB_GET_RESULT2 Parsing -
17 A_sp_SUB_GET_RESULT2 Parsing -  TOR4
18 A_sp_SUB_GET_RESULT2 Parsing -
19 A_sp_SUB_GET_RESULT2 Parsing -  TOR5
20 A_sp_SUB_GET_RESULT2 Parsing -
21 A_sp_SUB_GET_RESULT2 Parsing -  TOR6
22 A_sp_SUB_GET_RESULT2 Parsing -
23 A_sp_SUB_GET_RESULT2 Parsing -  TOR7
24 A_sp_SUB_GET_RESULT2 Parsing -
25 A_sp_SUB_GET_RESULT2 Parsing -  CLOTH
26 A_sp_SUB_GET_RESULT2 Parsing -  JET BLACK
27 A_sp_SUB_GET_RESULT2 Parsing -  COLOR
28 A_sp_SUB_GET_RESULT2 Parsing -  HERA

29 A_sp_SUB_GET_RESULT2 Parsing -  SEQ2
30 A_sp_SUB_GET_RESULT2 Parsing -  2
A_sp_SUB_GET_RESULT2 Parsing -  Column1
A_sp_SUB_GET_RESULT2 Parsing -  2019-03-25 14:26:26
A_sp_SUB_GET_RESULT2 Parsing -  LOTNO
A_sp_SUB_GET_RESULT2 Parsing -  L119084BKBL0001
A_sp_SUB_GET_RESULT2 Parsing -  BAR1
A_sp_SUB_GET_RESULT2 Parsing -  [)>06Y4730800000000LP6000302012V544663743T11190093020W0025
A_sp_SUB_GET_RESULT2 Parsing -  BAR2
A_sp_SUB_GET_RESULT2 Parsing -  SG181106L0036
A_sp_SUB_GET_RESULT2 Parsing -  TOR1
A_sp_SUB_GET_RESULT2 Parsing -  7.15
A_sp_SUB_GET_RESULT2 Parsing -  TOR2
A_sp_SUB_GET_RESULT2 Parsing -  7.04
A_sp_SUB_GET_RESULT2 Parsing -  TOR3
A_sp_SUB_GET_RESULT2 Parsing -  9.37
A_sp_SUB_GET_RESULT2 Parsing -  TOR4
A_sp_SUB_GET_RESULT2 Parsing -  9.81
A_sp_SUB_GET_RESULT2 Parsing -  TOR5
A_sp_SUB_GET_RESULT2 Parsing -  9.70
A_sp_SUB_GET_RESULT2 Parsing -  TOR6
A_sp_SUB_GET_RESULT2 Parsing -  9.40
A_sp_SUB_GET_RESULT2 Parsing -  TOR7
A_sp_SUB_GET_RESULT2 Parsing -  9.57
A_sp_SUB_GET_RESULT2 Parsing -  CLOTH
A_sp_SUB_GET_RESULT2 Parsing -  JET BLACK
A_sp_SUB_GET_RESULT2 Parsing -  COLOR
A_sp_SUB_GET_RESULT2 Parsing -  HERA
*/
                dataGridView1.Rows.Clear();
                //dataGridView1.Refresh();
                for (int i = 0; i < 6; i++)
                {
                    if (startIndex > strArr.Length)
                    {
                        break;
                    }

                    strData[0] = strArr[startIndex];
                    strData[1] = strArr[startIndex + 2];
                    strData[2] = strArr[startIndex + 4];
                    if (strArr[startIndex + 10] != "" && strArr[startIndex + 12] != "")
                    {
                        strData[3] = "OK";// strArr[startIndex + 12];
                    }
                    else
                    {
                        strData[3] = "NG";
                    }
                    if (strArr[startIndex + 14] != "" && strArr[startIndex + 16] != "" && strArr[startIndex + 18] != "")
                    {
                        strData[4] = "OK";// strArr[startIndex + 12];
                    }
                    else
                    {
                        strData[4] = "NG";
                    }
                    /*if (barcodeRestult)
                    { 
                        strData[5] = "OK";//strArr[startIndex + 22];
                    }
                    else
                    {
                        strData[5] = "NG";
                    }*/
                    dataGridView1.Rows.Add(strData);

                    dataGridView1.Focus();

                    startIndex += 28;
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;

            }
            else if (strTemp[0] == "A_sp_SUB_GET_OPTION_9BUX")
            {
                string[] strArr = str.Split(',');
                //m_strLotNO = strArr[2];
                foreach (string st in strArr)
                {
                    Console.WriteLine("db parsing" + st);
                }

                /*
                0 db parsingA_sp_SUB_GET_OPTION_9BUX
                1 db parsingSETCODE
                2 db parsingBBK  1
                3 db parsingCOLOR
                4 db parsingJET BLACK 2
                5 db parsingVEH
                6 db parsing4  3
                7 db parsingSER
                8 db parsingJV  4
                9 db parsingCLOTH 
                10 db parsingHERA  5
                11 db parsingSTITCH 
                12 db parsingShadow Gary  6
                13 db parsingBARCHECK
                14 db parsing3020  7
                15 db parsingLR
                16 db parsingL  8
                17 db parsingPLAN_DATE
                18 db parsing20190320  9
                19 db parsingPLAN_SEQUENCE
                20 db parsing9   10
                21 db parsingPJTCD
                22 db parsing9BUX  11
                23 db parsingITEMNO
                24 db parsing42692913  12
                 */

                // BARCHECK
                lblBARCHECK.Text = strArr[7];
                // SETCODE
                lblSETCODE.Text = strArr[1];
                // PJTCD
                lblPJTCD.Text = strArr[11];
                // CLOTH
                lblCLOTH.Text = strArr[5];
                // COLOR
                lblCOLOR.Text = strArr[2];
                //lblBarcode.Text = strArr[2];
                lblLR.Text = strArr[8];
                m_strPLAN_DATE = strArr[9];
                m_strPLAN_SEQUENCE = strArr[10];
                lblITEMNO.Text = strArr[12];
            }

        }
#endregion

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
