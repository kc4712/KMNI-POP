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
using System.Collections;
using System.Net.NetworkInformation;
using System.Timers;

namespace POP
{
    public partial class Side_GA : Form
    {
        private System.Timers.Timer mUiTimer;
        /// <summary>
        /// 바코드 인식 완료용 플래그...
        /// </summary>
        /// <param name="barcodeRestult"></param>
        private static bool barcodeRestult = false;
        /// <summary>
        /// 9BUX용 자재 바코드 확인 변수
        /// </summary>
        /// <param name="m_nSAB_OK"></param>
        private int m_nSAB_OK = 0;
        /// <summary>
        /// 9BUX용 자재 바코드 확인 변수
        /// </summary>
        /// <param name="m_nTC_OK"></param>
        private int m_nTC_OK = 0;

        private frmJobComplete frm;
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
        private string m_strLR = "";
        private string m_strPrintBarcode = "";
        private string m_strLotNO = "";
        private string m_strSAB_Barcode = "";
        private string m_strTC_Barcode = "";

        /// <summary>
        /// old_~~ 변수들은 재발행시 DB의 이전값 저장용도
        /// </summary>
        /// <param name="old_lblLR"></param>
        string old_lblLR;
        string old_lblSETCODE;
        string old_m_strPLAN_DATE;
        string old_m_strPLAN_SEQUENCE;

        /// <summary>
        /// ADAM 통신 "github" 소스를 보는데 콜백이 안되서 타이머에서 계속 읽어야 하는 판국에 클램프 들어가면 db에서 작업수량 작업리스트 올려달래서 만든 플래그...
        /// </summary>
        /// <param name="clamp"></param>
        bool clamp = false;

        /// <summary>
        /// 핑 날리는 주기용 변수
        /// </summary>
        /// <param name="PingTestCnt"></param>
        private int PingTestCnt = 0;

        /// <summary>
        /// 합격, 불합격등 알림 팝업 사라지는 시간 1초
        /// </summary>
        /// <param name="UIREFRESH_CNT"></param>
        private static int UIREFRESH_CNT = 1000;
        /// <summary>
        /// 로그아웃시 리더기, 툴값 콜백이 10분이 넘을 때 까지 없으면 로그아웃 팝업 띄울 용도의 변수
        /// </summary>
        /// <param name="LoginoutCnt"></param>
        int LoginoutCnt = 0;
        private UTIL mUTIL = new UTIL();
        /// <summary>
        /// 작업표준서용 UI
        /// </summary>
        /// <param name="mFrm_WorkStandard"></param>
        private Frm_WorkStandard mFrm_WorkStandard = new Frm_WorkStandard();

        /// <summary>
        /// sp_JIT_SEL_USER 반환값 및 사용자카드 id  저장용
        /// </summary>
        /// <param name="USER_ID"></param>
        string USER_ID = "";
        string USER_NAME = "";
        
        PeripheralControlCenter mPCC;

        public Side_GA()
        {
            InitializeComponent();
        }

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
            bool bResult = false;
            bResult = LoadOption();
            if (bResult)
            {
                LoadAmount();
                LoadProductionData();

                old_lblLR = lblLR.Text;
                old_lblSETCODE = lblSETCODE.Text;
                old_m_strPLAN_DATE = m_strPLAN_DATE;
                old_m_strPLAN_SEQUENCE = m_strPLAN_SEQUENCE;
            }
            else
            {
                lblMessage.Text = "생산계획이 없습니다.";
                lblCUR_JOB.Text = "";
                lblPJTCD.Text = "";
                lblSETCODE.Text = "";
                lblITEMNO.Text = "";
                lblCLOTH.Text = "";
                lblCOLOR.Text = "";
                lblDAY_JOB.Text = "";
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }

            if (lblPJTCD.Text == "GSUV") { 
                lblClampWarning.Visible = false;
            }
            else { 
                lblClampWarning.Visible = false;
                lblSTATUS_ADAM.Visible = false;
                lblSTATUS_CLAMP.Visible = false;
            }
            listBox1.Visible = false;
            lblDisplayDay.Text = "당일\n작업수량";
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
            
            mUiTimer = new System.Timers.Timer();
            mUiTimer.Interval = 100;
            mUiTimer.Elapsed += new ElapsedEventHandler(UiRefresh);
            
            mUiTimer.Start();
        }


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

        /// <summary>
        /// Barcode에서 콜백이 올라오면서 barcodeRestult플래그와 툴값 카운트 기점으로 작업완료 처리하는 타이머
        /// </summary>
        /// <param name="UiRefresh"></param>
        public void UiRefresh(object sender, ElapsedEventArgs e)
        {
            if(lblPJTCD.Text == "GSUV") {
                lblSTATUS_ADAM.Visible = true;
                lblSTATUS_CLAMP.Visible = true;
                //LogMsg.CMsg.Show("clamp - ", "release clamp", "-WRITE 1" + "", false, true);
                // 클램프 ON
                if (ADAM_TCP.ADAM.sBuffer_AI_01[0] == 1)
                //if (mPCC.AdamRead()[0] == 1)
                {
                    lblSTATUS_CLAMP.BackColor = Color.LimeGreen;
                    lblSTATUS_CLAMP.ForeColor = Color.Black;

                    if (clamp)
                    {
                        logboxAdd("Clamp: On");
                        bool bResult = LoadOption();

                        if (bResult)
                        {
                            // DIO  = 0 Reset
                            mPCC.AdamWrite(0);
                            LoadAmount();
                            GetPrintBarcode();
                            old_lblLR = lblLR.Text;
                            old_lblSETCODE = lblSETCODE.Text;
                            old_m_strPLAN_DATE = m_strPLAN_DATE;
                            old_m_strPLAN_SEQUENCE = m_strPLAN_SEQUENCE;
                        }
                        else
                        {
                            lblMessage.Text = "생산계획이 없습니다.";
                            lblCUR_JOB.Text = "";
                            lblPJTCD.Text = "";
                            lblSETCODE.Text = "";
                            lblITEMNO.Text = "";
                            lblCLOTH.Text = "";
                            lblCOLOR.Text = "";
                            lblDAY_JOB.Text = "";
                            dataGridView1.Rows.Clear();
                            dataGridView1.Refresh();
                        }
                        clamp = false;
                    }

                    if (barcodeRestult && (Tool1_OK_Count == 2) && (Tool2_OK_Count == 3))
                    {
                        barcodeRestult = false;
                        mPCC.ClampRelease();
                        // Print Initialize
                        mPCC.initializPrint("SB_" + lblPJTCD.Text);
                        logboxAdd("Print: " + "SB_" + lblPJTCD.Text);
                        //LogMsg.CMsg.Show("clamp - ", "release clamp", "SB_" + lblPJTCD.Text, false, true);

                        //if (!zPrint.Connect())
                        //{
                        //    //MessageBox.Show("error -print");
                        //}
                        //LogMsg.CMsg.Show("print - ", "print initialize", "Connect" + "", false, true);

                        // print
                        mPCC.barcodePrint(m_strPrintBarcode);
                        logboxAdd("Print: " + m_strPrintBarcode);
                        //LogMsg.CMsg.Show("PRINT", "OK-", m_strPrintBarcode, false, true);


                        //LogMsg.CMsg.Show("clamp - ", "release clamp", "-WRITE 1" + "", false, true);

                        lblSTATUS_CLAMP.BackColor = Color.Red;
                        lblSTATUS_CLAMP.ForeColor = Color.White;

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

                        mUTIL.SendDB("sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                            m_strSAB_Barcode + ",," + torque1 + "," + torque2 + "," + torque3 +
                            "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7 +
                            ",OK," + USER_ID + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE + "," + USER_NAME);

                        //LogMsg.CMsg.Show("SendDB", "sp_SUB_OUTPUT", "sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                        //    m_strSAB_Barcode + ",," + torque1 + "," + torque2 + "," + torque3 +
                        //    "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7 +
                        //    ",OK,9999," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE, false, true);

                        // release clamp
                        //LogMsg.CMsg.Show("clamp - ", "release clamp", "", false, true);
                        //ADAM_TCP.ADAM.WriteData(0, 12, 1);
                        
                        lblSAB_BARCODE.Text = "";
                        lblTC_BARCODE.Text = "";
                        lbl_TQ1.Text = "";
                        lbl_TQ2.Text = "";
                        lbl_TQ3.Text = "";
                        lbl_TQ4.Text = "";
                        lbl_TQ5.Text = "";
                        LoadOption();
                        LoadAmount();
                        dataGridView1.Rows.Clear();
                        LoadProductionData();
                        
                        // DIO  = 0 Reset
                        //Thread.Sleep(1000);
                        //ADAM_TCP.ADAM.WriteData(0, 12, 0);

                        Tool1_OK_Count = 0;
                        Tool2_OK_Count = 0;
                        barcodeRestult = false;
                    }
                }
                else
                {
                    lblSTATUS_CLAMP.BackColor = Color.Red;
                    lblSTATUS_CLAMP.ForeColor = Color.White;

                    clamp = true;

                    lblSAB_BARCODE.Text = "";
                    lblTC_BARCODE.Text = "";
                    lbl_TQ1.Text = "";
                    lbl_TQ2.Text = "";
                    lbl_TQ3.Text = "";
                    lbl_TQ4.Text = "";
                    lbl_TQ5.Text = "";

                    Tool1_OK_Count = 0;
                    Tool2_OK_Count = 0;
                }
            }
            else { 
                if (barcodeRestult && (Tool1_OK_Count == 2) && (Tool2_OK_Count == 3))
                {
                    barcodeRestult = false;

                    // Print Initialize
                    mPCC.initializPrint("SB_" + lblPJTCD.Text);
                    logboxAdd("Print: " + "SB_" + lblPJTCD.Text);
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "SB_" + lblPJTCD.Text, false, true);

                    //if (!zPrint.Connect())
                    //{
                    //    //MessageBox.Show("error -print");
                    //}
                    //LogMsg.CMsg.Show("print - ", "print initialize", "Connect" + "", false, true);

                    // print
                    mPCC.barcodePrint(m_strPrintBarcode);
                    logboxAdd("Print: " + m_strPrintBarcode);
                    //LogMsg.CMsg.Show("PRINT", "OK-", m_strPrintBarcode, false, true);


                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "-WRITE 1" + "", false, true);

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

                    mUTIL.SendDB("sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                        m_strSAB_Barcode + ",," + torque1 + "," + torque2 + "," + torque3 +
                        "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7 +
                        ",OK," + USER_ID + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE + "," + USER_NAME);

                    //LogMsg.CMsg.Show("SendDB", "sp_SUB_OUTPUT", "sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                    //    m_strSAB_Barcode + ",," + torque1 + "," + torque2 + "," + torque3 +
                    //    "," + torque4 + "," + torque5 + "," + torque6 + "," + torque7 +
                    //    ",OK,9999," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE, false, true);

                    // release clamp
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "", false, true);
                    //ADAM_TCP.ADAM.WriteData(0, 12, 1);
                    lblSAB_BARCODE.Text = "";
                    lblTC_BARCODE.Text = "";
                    lbl_TQ1.Text = "";
                    lbl_TQ2.Text = "";
                    lbl_TQ3.Text = "";
                    lbl_TQ4.Text = "";
                    lbl_TQ5.Text = "";
                    LoadOption();
                    LoadAmount();
                    dataGridView1.Rows.Clear();
                    LoadProductionData();
                    // DIO  = 0 Reset
                    //Thread.Sleep(1000);
                    //ADAM_TCP.ADAM.WriteData(0, 12, 0);

                    Tool1_OK_Count = 0;
                    Tool2_OK_Count = 0;
                    barcodeRestult = false;
                }
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
        

        public string CheckSAB_Barcode(string strBarcode)
        {
            string strResult = "";
            string strReceiveData = "";
            string strReceiveData2 = "";

            strReceiveData = mUTIL.SendDB("sp_SUB_GET_BARCODE, " + strBarcode + "," + lblSETCODE.Text + "," + lblLR.Text);

            //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODE", strReceiveData, false, true);

            if (strReceiveData != "")
            {
                //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODE_SAB", strReceiveData, false, true);

                if (strReceiveData.Contains("True"))
                {
                    strReceiveData2 = mUTIL.SendDB("sp_SUB_GET_LOTNO," + strBarcode + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE);

                    if (strReceiveData2 != "")
                    {
                        string[] s = strReceiveData2.Split(',');
                        m_strLotNO = s[1];
                        //LogMsg.CMsg.Show("sp_SUB_GET_LOTNO_SAB", "", m_strLotNO, false, true);

                        if (m_strLotNO.Contains("X"))
                        {
                            strResult = "OK";
                        }
                        else
                        {
                            strResult = "중복";
                        }
                    }
                }
                else // 매칭 불량
                {
                    strResult = "불량";
                }
            }
            return strResult;
        }

        public string CheckTC_Barcode(string strBarcode)
        {
            string strResult = "";
            string strReceiveData = "";

            strReceiveData = mUTIL.SendDB("sp_SUB_GET_LOTNO," + strBarcode + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE);

            //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_LOTNO_TC", strReceiveData, false, true);

            if (strReceiveData != "")
            {
                string[] s = strReceiveData.Split(',');
                m_strLotNO = s[1];
                //LogMsg.CMsg.Show("sp_SUB_GET_LOTNO_TC", "", m_strLotNO, false, true);

                if (m_strLotNO.Contains("X"))
                {
                    strResult = "OK";
                }
                else
                {
                    strResult = "중복";
                }
            }
            return strResult;
        }

        /// <summary>
        /// Barcode에서 콜백 메서드 작업자 로그인 처리, 공정에 따른 각 부품 바코드 처리
        /// 양산으로 인해 엎을 시간이 모자라 원작자의 로직 다수 채용... 
        /// </summary>
        /// <param name="CheckBarcode"></param>
        public void CheckBarcode(string strBarcode)
        {
            logboxAdd("Barcode:" + strBarcode + "," + " Length:" + strBarcode.Length);
            LoginoutCnt = 0;
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
                        lbl_user_nm.Text = strArr[2];
                        USER_ID = strBarcode;
                        USER_NAME = strArr[2];

                        bool bResult = LoadOption();

                        if (bResult)
                        {
                            LoadAmount();
                            GetPrintBarcode();
                            old_lblLR = lblLR.Text;
                            old_lblSETCODE = lblSETCODE.Text;
                            old_m_strPLAN_DATE = m_strPLAN_DATE;
                            old_m_strPLAN_SEQUENCE = m_strPLAN_SEQUENCE;
                        }
                        else
                        {
                            lblMessage.Text = "생산계획이 없습니다.";
                            lblCUR_JOB.Text = "";
                            lblPJTCD.Text = "";
                            lblSETCODE.Text = "";
                            lblITEMNO.Text = "";
                            lblCLOTH.Text = "";
                            lblCOLOR.Text = "";
                            lblDAY_JOB.Text = "";
                            dataGridView1.Rows.Clear();
                            dataGridView1.Refresh();
                        }

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
                    //lblUser.Text = "";
                    lbl_user_nm.Text = "";
                    USER_ID = "";
                    USER_NAME = "";
                }
                if (lblPJTCD.Text == "GSUV")
                {
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
                }
                string strResult = "";

                if (lblPJTCD.Text == "9BUX")
                {
                    Console.WriteLine("strBarcode:" + strBarcode + " length:" + strBarcode.Length);
                    // SAB
                    //lblSAB_BARCODE.Text = "";
                    if (strBarcode.Length > 14)
                    {
                        // SAB barcode 중복 체크
                        strResult = CheckSAB_Barcode(strBarcode);
                        Console.WriteLine("SAB strBarcode:" + strBarcode + " length:" + strBarcode.Length);

                        if (strResult == "OK")
                        {
                            m_strSAB_Barcode = strBarcode;
                            lblSAB_BARCODE.Text = strBarcode.Substring(47);

                            //Thread.Sleep(1000);
                            m_nSAB_OK = 1;

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
                        else if (strResult == "중복")
                        {
                            m_nSAB_OK = 0;
                            lblSAB_BARCODE.Text = "";

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
                        }
                        else if (strResult == "불량")
                        {
                            m_nSAB_OK = 0;
                            lblSAB_BARCODE.Text = "";

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
                        }
                    }
                    // TC
                    else
                    //if(strBarcode.Length < 14)
                    {
                        Console.WriteLine("strBarcode[1].ToString()  " + strBarcode[1].ToString());
                        // T/C barcode 좌우 오사양 체크
                        if (strBarcode[8].ToString() != lblLR.Text)
                        {
                            m_nTC_OK = 0;
                            lblTC_BARCODE.Text = "";

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
                        }
                        // T/C barcode 중복체크
                        strResult = CheckTC_Barcode(strBarcode);
                        //lblTC_BARCODE.Text = "";
                        Console.WriteLine("TC strBarcode:" + strBarcode + " length:" + strBarcode.Length);
                        if (strResult == "OK")
                        {
                            m_nTC_OK = 1;

                            m_strTC_Barcode = strBarcode;
                            lblTC_BARCODE.Text = strBarcode;
                            //Thread.Sleep(1000);
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
                        else if (strResult == "중복")
                        {
                            m_nTC_OK = 0;
                            lblTC_BARCODE.Text = "";

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
                        }
                        else// T/C barcode DB에서 반환된 기타 불량체크
                        {
                            m_nTC_OK = 0;
                            lblTC_BARCODE.Text = "";

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
                        }
                    }
                }
                else // GSUV
                {
                    if (strBarcode.Length > 30)
                    {
                        // SAB barcode 중복 체크
                        strResult = CheckSAB_Barcode(strBarcode);

                        if (strResult == "OK")
                        {
                            m_strSAB_Barcode = strBarcode;
                            lblSAB_BARCODE.Text = strBarcode.Substring(47);

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
                        else if (strResult == "중복")
                        {
                            //lblSAB_BARCODE.Text = "";
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
                        }
                        else if (strResult == "불량")
                        {
                            //lblSAB_BARCODE.Text = "";

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
                            //LogMsg.CMsg.Show("Barcode", "매칭 불량", " -- NG", false, true);
                        }
                    }
                }
                // 가조립 9bux는 바코드만 찍기에 여기서 결과 처리
                if (m_nSAB_OK == 1 && m_nTC_OK == 1 && lblPJTCD.Text == "9BUX")
                {
                    Console.WriteLine(" m_strSAB_Barcode " + m_strSAB_Barcode);
                    Console.WriteLine(" m_strTC_Barcode " + m_strTC_Barcode);
                    lblTC_BARCODE.Text = m_strTC_Barcode;
                    lblSAB_BARCODE.Text = m_strSAB_Barcode.Substring(47);
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

                    //LogMsg.CMsg.Show("Barcode Result", "9BUX", " -- OK", false, true);

                    

                    //ADAM_TCP.ADAM.WriteData(0, 12, 1);
                    //Thread.Sleep(1000);
                    //ADAM_TCP.ADAM.WriteData(0, 12, 0);
                    if(lblPJTCD.Text == "GSUV") { 
                            mPCC.ClampRelease();
                            //LogMsg.CMsg.Show("clamp - ", "release clamp", "-WRITE 1" + "", false, true);

                            lblSTATUS_CLAMP.BackColor = Color.Red;
                            lblSTATUS_CLAMP.ForeColor = Color.White;
                            logboxAdd("Clamp: OFF");
                                // release clamp
                    }

                    //barcodePrint
                    mPCC.initializPrint("SB_" + lblPJTCD.Text);
                    logboxAdd("Print: " + "SB_" + lblPJTCD.Text);
                    GetPrintBarcode();
                    Console.WriteLine("pRINT------" + m_strPrintBarcode);
                    mPCC.barcodePrint(m_strPrintBarcode);
                    logboxAdd("Print: " + m_strPrintBarcode);

                    mUTIL.SendDB("sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                        m_strSAB_Barcode + "," + m_strTC_Barcode + "," + "," + "," + "," + "," + "," + "," + ",OK," + USER_ID + "," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE + "," + USER_NAME);
                    //LogMsg.CMsg.Show("SendDB", "sp_SUB_OUTPUT", "", false, true);
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "", false, true);
                    //LogMsg.CMsg.Show("clamp - ", "release clamp", "sp_SUB_OUTPUT," + lblSETCODE.Text + "," + lblLR.Text + "," +
                    //    m_strSAB_Barcode + "," + m_strTC_Barcode + "," + "," + "," + "," + "," + "," + "," + ",OK,9999," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE, false, true);
                    //barcodePrint send to db

                    //LogMsg.CMsg.Show("PRINT", "OK-", m_strPrintBarcode, false, true);
                    bool bResult = LoadOption();

                    if (bResult)
                    {
                        LoadAmount();
                        LoadProductionData();
                    }
                    else
                    {
                        lblMessage.Text = "생산계획이 없습니다.";
                        lblCUR_JOB.Text = "";
                        lblPJTCD.Text = "";
                        lblSETCODE.Text = "";
                        lblITEMNO.Text = "";
                        lblCLOTH.Text = "";
                        lblCOLOR.Text = "";
                        lblDAY_JOB.Text = "";
                        dataGridView1.Rows.Clear();
                        dataGridView1.Refresh();
                    }
                    m_nSAB_OK = 0;
                    m_nTC_OK = 0;
                    barcodeRestult = false;

                    lblSAB_BARCODE.Text = "";
                    lblTC_BARCODE.Text = "";
                }
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
            string command = "sp_SUB_GET_AMOUNT2";
            string sql = command + "," + lblPJTCD.Text + "," + lblLR.Text + "," +
                lblSETCODE.Text + "," + m_strPLAN_DATE + ", " + m_strPLAN_SEQUENCE;

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

            string command = "sp_SUB_GET_RESULT";
            string sql = command + "," + "GSUV" + "," + lblLR.Text + "," + today;

            //LogMsg.CMsg.Show("SendDB", "sp_GET_RESULT", sql, false, true);

            string receiveData = "";

            // Get Option
            receiveData = mUTIL.SendDB(sql);

            if (receiveData != "")
            {
                parsingDB(receiveData);

            }

        }

        public void GetPrintBarcode()
        {
            string receiveData = "";

            //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_PJTCD", lblPJTCD.Text, false, true);
            if (lblPJTCD.Text == "GSUV")
            {
                receiveData = mUTIL.SendDB("sp_SUB_GET_BARCODEPRINT" + ",1," + lblLR.Text + "," + lblSETCODE.Text + ","
                    + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE);//Print 종류 설정 할수 있도록 수정예정
                //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODEPRINT", "1", false, true);
            }
            else
            {
                receiveData = mUTIL.SendDB("sp_SUB_GET_BARCODEPRINT" + ",0," + lblLR.Text + "," + lblSETCODE.Text + ","
                    + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE);//Print 종류 설정 할수 있도록 수정예정
                //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODEPRINT", "0", false, true);
            }

            if (receiveData != "")
            {
                m_strPrintBarcode = receiveData;
            }
        }

        public void GetPrintBarcodeTemp()
        {
            string receiveData = "";

            //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_PJTCD", lblPJTCD.Text, false, true);
            if (lblPJTCD.Text == "GSUV")
            {
                receiveData = mUTIL.SendDB("sp_SUB_GET_BARCODEPRINT_temp" + ",1," + old_lblLR + "," + old_lblSETCODE + ","
                    + old_m_strPLAN_DATE + "," + old_m_strPLAN_DATE);//Print 종류 설정 할수 있도록 수정예정
                //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODEPRINT_temp", "1", false, true);
            }
            else
            {
                receiveData = mUTIL.SendDB("sp_SUB_GET_BARCODEPRINT_temp" + ",0," + old_lblLR + "," + old_lblSETCODE + ","
                    + old_m_strPLAN_DATE + "," + old_m_strPLAN_SEQUENCE);//Print 종류 설정 할수 있도록 수정예정
                //LogMsg.CMsg.Show("SendDB", "sp_SUB_GET_BARCODEPRINT_temp", "0", false, true);
            }
            if (receiveData != "")
            {
                m_strPrintBarcode = receiveData;
            }
        }

        public void parsingDB(string str)
        {
            string[] strTemp = str.Split(',');
            // getOption
            if (strTemp[0] == "A_sp_SUB_GET_OPTION")
            {
                //Option
                //2 : COlor
                //8 : Cloth
                //12 : Barcheck
                //16 : setcode
                //18 : plandate
                //20 : plansequene
                //22 : pjtcd
                //24 : itemno

                lblCOLOR.Text = strTemp[2];
                lblCLOTH.Text = strTemp[8];
                lblBARCHECK.Text = strTemp[12];
                lblSETCODE.Text = strTemp[16];
                m_strPLAN_DATE = strTemp[18];
                m_strPLAN_SEQUENCE = strTemp[20];
                lblPJTCD.Text = strTemp[22];
                lblITEMNO.Text = strTemp[24];
                m_strLR = lblLR.Text;

            }
            // getAmount2 (작업수량)
            else if (strTemp[0] == "A_sp_SUB_GET_AMOUNT2")
            {

                lblCUR_JOB.Text = strTemp[1] + "/" + strTemp[2];
                lblDAY_JOB.Text = strTemp[3];
            }
            // getResult (실적)
            else if (strTemp[0] == "A_sp_SUB_GET_RESULT")
            {
                string[] strArr = str.Split(',');
                string[] strData = new string[9];
                int startIndex = 2;

                /*for(int i = 0; i < 9; i++)
                {
                    if (startIndex > strArr.Length)
                    {
                        break;
                    }

                    strData[0] = strArr[startIndex];
                    strData[1] = strArr[startIndex + 2];
                    strData[2] = strArr[startIndex + 4];
                    strData[3] = strArr[startIndex + 6].Substring(47);
                    strData[4] = strArr[startIndex + 10];
                    strData[5] = strArr[startIndex + 12];
                    strData[6] = strArr[startIndex + 20];
                    strData[7] = strArr[startIndex + 22];
                    strData[8] = "OK";

                    dataGridView1.Rows.Add(strData);

                    dataGridView1.Focus();

                    startIndex += 24;
                }*/
                dataGridView1.Rows.Clear();
                //dataGridView1.Refresh();
                for (int i = 0; i < 9; i++)
                {
                    Console.WriteLine("startIndex " + startIndex + "strArr.Length " + strArr.Length);
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
                    //strData[5] = "OK";//strArr[startIndex + 22];

                    dataGridView1.Rows.Add(strData);

                    dataGridView1.Focus();

                    startIndex += 24;
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            }
        }

        /// <summary>
        /// PeripheralControlCenter클래스의 툴 1번값 콜백 메서드 
        /// </summary>
        /// <param name="Tool1Result"></param>
        public void Tool1Result(double torque)
        {
            logboxAdd("Tool1:" + String.Format("{0}", torque));
            LoginoutCnt = 0;
            Console.WriteLine("getTCPToqueCB1", "- data ", torque.ToString());
            Console.WriteLine("torque nut1 " + torque);
            if (lblPJTCD.Text == "GSUV") { 
                if (!barcodeRestult || lblSTATUS_CLAMP.BackColor == Color.Red)
                {
                    Console.WriteLine("바코드 혹은 클램프 통과 못함.");
                    return;
                }
            }
            else
            { 
                if (!barcodeRestult)
                {
                    Console.WriteLine("바코드 통과 못함.");
                    return;
                }
            }
            // Read Torque
            Console.WriteLine("ToqueCB1: "+ torque.ToString());
            if (torque > 0)
            {
                //check check
                checkTool1Data(torque);
                if (Tool1_OK_Count == 2)
                {
                    //LogMsg.CMsg.Show("ThreadTool1", "- STOP ", "", false, true);

                    if (Tool2_OK_Count != 3)
                    {
                        wp_OK.Play();
                    }
                }
            }
        }
        /// <summary>
        /// PeripheralControlCenter클래스의 툴 2번값 콜백 메서드 
        /// </summary>
        /// <param name="Tool2Result"></param>
        public void Tool2Result(double torque)
        {
            logboxAdd("Tool2:" + String.Format("{0}", torque));
            LoginoutCnt = 0;
            if(lblPJTCD.Text == "GSUV") { 
                if (!barcodeRestult || lblSTATUS_CLAMP.BackColor == Color.Red)
                {
                    Console.WriteLine("바코드 혹은 클램프 통과 못함.");
                    return;
                }
            }
            else { 
                if (!barcodeRestult)
                {
                    Console.WriteLine("바코드 통과 못함.");
                    return;
                }
            }

            // Read Torque
            Console.WriteLine("ToqueCB2: " + torque.ToString());
            //LogMsg.CMsg.Show("ThreadTool2", "- data ", torque.ToString(), false, true);

            if (torque > 0)
            {
                //check check
                checkTool2Data(torque);
                if (Tool2_OK_Count == 3)
                {
                    //LogMsg.CMsg.Show("ThreadTool2", "- STOP ", "", false, true);

                    if (Tool1_OK_Count != 2)
                    {
                        wp_OK.Play();
                    }
                }
                torque = 0;
            }
        }

        public void checkTool1Data(double Tool1Data)
        {
            double min = Double.Parse(INI.SIDE.GATOOL1MIN);
            double max = Double.Parse(INI.SIDE.GATOOL1MAX);

            if (Tool1_OK_Count == 0)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool1Data, min, max))
                {
                    lbl_TQ1.Text = String.Format("{0:f1}", Tool1Data);

                    torque1 = String.Format("{0:f1}", Tool1Data);

                    // ok spec Count increase + 1
                    Tool1_OK_Count = Tool1_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool1Data", "checktool1 - ", Tool1_OK_Count.ToString(), false, true);
                }

            }
            else if (Tool1_OK_Count == 1)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool1Data, min, max))
                {
                    lbl_TQ2.Text = String.Format("{0:f1}", Tool1Data);
                    torque2 = String.Format("{0:f1}", Tool1Data);

                    // ok spec Count increase + 1
                    Tool1_OK_Count = Tool1_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool1Data", "checktool1 ", " - 2 ok", false, true);

                }
            }
        }

        public void checkTool2Data(double Tool2Data)
        {
            double min = Double.Parse(INI.SIDE.GATOOL2MIN);
            double max = Double.Parse(INI.SIDE.GATOOL2MAX);
            if (Tool2_OK_Count == 0)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max))
                {
                    lbl_TQ3.Text = String.Format("{0:f1}", Tool2Data);

                    torque3 = String.Format("{0:f1}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 1)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max))
                {
                    lbl_TQ4.Text = String.Format("{0:f1}", Tool2Data);

                    torque4 = String.Format("{0:f1}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
            else if (Tool2_OK_Count == 2)
            {
                // spec check
                if (mUTIL.isBetweenSpec(Tool2Data, min, max))
                {
                    lbl_TQ5.Text = String.Format("{0:f1}", Tool2Data);

                    torque5 = String.Format("{0:f1}", Tool2Data);

                    // ok spec Count increase + 1
                    Tool2_OK_Count = Tool2_OK_Count + 1;

                    //LogMsg.CMsg.Show("checkTool2Data", "checktool2 - ", Tool2_OK_Count.ToString(), false, true);
                }
            }
        }

        public void TimerPing_Test()
        {
            Ping pingSender = new Ping();
            // ADAM
            PingReply reply;
            if(lblPJTCD.Text == "GSUV")
            { 
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

            if (lblPJTCD.Text == "9BUX")
            {
                reply = pingSender.Send(INI.SIDE.PRINTIP9BUX);
                if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
                {
                    lblSTATUS_PRINT.BackColor = Color.LimeGreen;
                    lblSTATUS_PRINT.ForeColor = Color.Black;
                }
                else //핑이 제대로 들어가지 않고 있을 경우 
                {
                    lblSTATUS_PRINT.BackColor = Color.Red;
                    lblSTATUS_PRINT.ForeColor = Color.White;
                }
            }
            if (lblPJTCD.Text == "GSUV")
            {
                reply = pingSender.Send(INI.SIDE.PRINTIPGSUV);
                if (reply.Status == IPStatus.Success) //핑이 제대로 들어가고 있을 경우
                {
                    lblSTATUS_PRINT.BackColor = Color.LimeGreen;
                    lblSTATUS_PRINT.ForeColor = Color.Black;
                }
                else //핑이 제대로 들어가지 않고 있을 경우 
                {
                    lblSTATUS_PRINT.BackColor = Color.Red;
                    lblSTATUS_PRINT.ForeColor = Color.White;
                }
            }
        }
        
        private void timer_time_Tick(object sender, EventArgs e)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lblTime.Text = today;
        }

        /// <summary>
        /// 종료 버튼 클릭 
        /// </summary>
        /// <param name="button1_Click"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            mUiTimer.Elapsed -= new ElapsedEventHandler(UiRefresh);

            mUiTimer.Stop();
            mUiTimer.Close();
            mUiTimer.Dispose();
            mFrm_WorkStandard.Close();
            this.Dispose();
            this.Close();
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
            mFrm_WorkStandard.Close();
            this.Dispose();
            this.Close();
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

        /// <summary>
        /// 방향 전환 버튼 클릭 
        /// </summary>
        /// <param name="button2_Click"></param>
        private void button2_Click(object sender, EventArgs e)
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
            bool bResult = false;
            bResult = LoadOption();
            if (bResult)
            {
                LoadAmount();
                LoadProductionData();
                old_lblLR = lblLR.Text;
                old_lblSETCODE = lblSETCODE.Text;
                old_m_strPLAN_DATE = m_strPLAN_DATE;
                old_m_strPLAN_SEQUENCE = m_strPLAN_SEQUENCE;
            }
            else
            {
                lblMessage.Text = "생산계획이 없습니다.";
                lblCUR_JOB.Text = "";
                lblPJTCD.Text = "";
                lblSETCODE.Text = "";
                lblITEMNO.Text = "";
                lblCLOTH.Text = "";
                lblCOLOR.Text = "";
                lblDAY_JOB.Text = "";
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }
        }

        /// <summary>
        /// 작업 완료 버튼 클릭 
        /// </summary>
        /// <param name="btn_Job_Complete_Click"></param>
        private void btn_Job_Complete_Click(object sender, EventArgs e)
        {
            frm = new frmJobComplete();
            frm.cbComplete += new frmJobComplete.JobCompleteCB(cbgetComplete);
            frm.Show();
        }

        /// <summary>
        /// 작업완료시 뜨는 frmJobComplete 폼클래스의 확인, 취소 콜백 클래스
        /// </summary>
        /// <param name="cbgetComplete"></param>
        private void cbgetComplete(bool ok_cancel)
        {
            Console.WriteLine("cb getComplete");
            if (ok_cancel)
            {
                mUTIL.SendDB("sp_SUB_JOB_COMPLETE," + m_strPLAN_DATE + "," + m_strPLAN_SEQUENCE);
                bool bResult = LoadOption();
                if (bResult)
                {
                    LoadAmount();
                    LoadProductionData();
                    GetPrintBarcode();
                    old_lblLR = lblLR.Text;
                    old_lblSETCODE = lblSETCODE.Text;
                    old_m_strPLAN_DATE = m_strPLAN_DATE;
                    old_m_strPLAN_SEQUENCE = m_strPLAN_SEQUENCE;
                }
                else
                {
                    lblMessage.Text = "생산계획이 없습니다.";
                    lblCUR_JOB.Text = "";
                    lblPJTCD.Text = "";
                    lblSETCODE.Text = "";
                    lblITEMNO.Text = "";
                    lblCLOTH.Text = "";
                    lblCOLOR.Text = "";
                    lblDAY_JOB.Text = "";
                    dataGridView1.Rows.Clear();
                    dataGridView1.Refresh();
                }
            }
            frm.cbComplete -= new frmJobComplete.JobCompleteCB(cbgetComplete);

            frm.Close();
        }

        /// <summary>
        /// 회사 CI클릭시 로그뷰 온 오프
        /// </summary>
        /// <param name="pictureBox1_Click"></param>
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

        /// <summary>
        /// 작업취소 버튼 클릭
        /// </summary>
        /// <param name="btn_Job_Cancel_Click_1"></param>
        private void btn_Job_Cancel_Click_1(object sender, EventArgs e)
        {
            lblSAB_BARCODE.Text = "";
            lblTC_BARCODE.Text = "";
            lbl_TQ1.Text = "";
            lbl_TQ2.Text = "";
            lbl_TQ3.Text = "";
            lbl_TQ4.Text = "";
            lbl_TQ5.Text = "";

            Tool1_OK_Count = 0;
            Tool2_OK_Count = 0;
            m_nSAB_OK = 0;
            m_nTC_OK = 0;
            barcodeRestult = false;
        }

        /// <summary>
        /// UI상단 체크박스 변경시 프린터 재발행
        /// </summary>
        /// <param name="checkBox1_CheckedChanged"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                //barcodePrint
                mPCC.initializPrint("SB_" + lblPJTCD.Text);
                logboxAdd("Print: " + "SB_" + lblPJTCD.Text);
                GetPrintBarcodeTemp();
                mPCC.barcodePrint(m_strPrintBarcode);
                logboxAdd("Print: " + m_strPrintBarcode);
            }
        }

    }
}
