#region defines
static Thread TheadMain = new Thread(ThreadMain);

static Thread ThreadManager0 = new Thread(ThreadMonitor0);
static Thread ThreadManager1 = new Thread(ThreadBarode);
static Thread ThreadManager2 = new Thread(ThreadTool1);


static SoundPlayer wp_OK = new SoundPlayer("SGPOP/OK.wav");
static SoundPlayer wp_NG = new SoundPlayer("SGPOP/NG.wav");
static SoundPlayer wp_NG2 = new SoundPlayer("SGPOP/NG2.wav");
static SoundPlayer wp_ResultOK = new SoundPlayer("SGPOP/RESULT_OK.wav");

static int NOofBARCODE = 0;
static int NOofTORQUE = 4;

static int tool1_OK_Count = 0;
static int barcodeReadCount = 0;


static string barcodeFlag = "0";
static string resultOK = "OK";
//static string lotNum = "";

static string torque1 = "";
static string torque2 = "";


static int sendDB_Count = 0;
static int readDB_Count = 0;

static string LR = "";


static bool checkbarcode= false;
static bool checktool1 = false;

static string[] BarcodeList=new string[10];
static int[] BarcodeResult = new int[10];
static string[] TorqueList = new string[10];
static int[] TorqueResult = new int[10];

static int testCount = 100;
#endregion 

private libNutTcp nut1;

static void Main() 
{
	pop_terminal.frmMain.ScreenEvent += new pop_terminal.frmMain.ScreenEventHandler(Pop_ScreenEvent);	

	PowerTool.PowerTool.Initialize();
	PowerTool.PowerTool.Start();
	
	nut = new libNutTcp(192.168.50.95, 4545);
	nut.sendCb += new libNutTcp.toqueEventCb(getToqueCB1);
	nut.start();
	
	 
	TheadMain.Start();
	ThreadManager0.Start(); 	
	ThreadManager1.Start();  // Barcode Data Monitoring
	ThreadManager2.Start();  // Tool1 Data Monitoring
	
	LogMsg.CMsg.Show("SCRIPT","satarted main","",false,true);
}

#region // thread main	
static void ThreadMain()
{
	int PlcData = 0;
	bool StartTest= false; 
	bool EndTest = false;

	string sql = "";
	
	while(true)
	{
		PlcData =(int)UPAR.PlcInStatus;
		if(PlcData == 0)
		{
			//Reset();
		}
		
		switch(testCount)
		{
			case 100:
				#region // check start (100
				if((PlcData == 1) && (StartTest== false) && (EndTest == false))
				{
					LogMsg.CMsg.Show("SCRIPT ","start test","",false,true);
					StartTest = true;
					EndTest = false;
					//read option
					sql = "sp_JIT_GET_OPTION," +USTR.StationCode+ ","+ USTR.RfidLot;
					pop.SendDB(sql);
					LogMsg.CMsg.Show("SCRIPT","read option",sql,false,true);
						
					tool1_OK_Count = 0;
					barcodeReadCount = 0;

					if(NOofBARCODE > 0) checkbarcode = true;
					if(NOofTORQUE > 0) checktool1 =true;
					testCount =200;
					LogMsg.CMsg.Show("SCRIPT ","test count:",testCount.ToString(),false,true);
				}		
				#endregion
				break;
			case 200:
				#region // check end test(110) --> 101
				if( StartTest && (barcodeReadCount >=NOofBARCODE) && ( tool1_OK_Count >=  NOofTORQUE ) && (EndTest == false) )
				{
					// send to  result
					UPAR.PlcOutStatus =1;		
					
					string temp = "sp_JIT_INS_RESULT,FR," + USTR.StationLeftorRight.ToString().Substring(0,1) + "," + USTR.StationCode.ToString()+","+USTR.RfidLot;
					for(int i=0;i<10;i++) temp +=","+BarcodeList[i];
					for(int i=0;i<10;i++) temp +=","+TorqueList[i];
					temp+=","+"testid" +",OK";
					pop.SendDB(temp);
					
					LogMsg.CMsg.Show("SCRIPT","send result : ",temp,false,true);
					
					StartTest = false;
					EndTest = true;
					checkbarcode= false;
					checktool1 = false;
					barcodeReadCount =0;
					tool1_OK_Count = 0;
					
					pop.OpenWindow(2,true);
					wp_ResultOK.Play();
					Thread.Sleep(1000);
					pop.OpenWindow(2,false);
					testCount = 500;
					LogMsg.CMsg.Show("SCRIPT ","test count:",testCount.ToString(),false,true);
					
					// 실적조회
					USTR.DbReceved="";
					temp = "sp_JIT_GET_AMOUNT_TERMINAL,FR,"  + USTR.StationCode.ToString()+","+USTR.PlanDate.ToString()+","+USTR.PlanSequence.ToString();
					pop.SendDB(temp);
					LogMsg.CMsg.Show("SCRIPT","request count: ",temp,false,true);
				}
				#endregion	
				
				break;
				
			case 300:
				
				break;
			case 400:
				
				break;
			case 500:
				#region // seat out and reset(001)
				if((PlcData == 0)&& (StartTest == false )&&(EndTest == true))
				{
					LogMsg.CMsg.Show("SCRIPT","zig out, reset","",false,true);
					StartTest = false;
					EndTest = false;
					barcodeReadCount =0;
					tool1_OK_Count = 0;
					
					readDB_Count = 0;

					checkbarcode= false;
					checktool1 = false;
					
					for(int i=0;i<10;i++) TorqueList[i] ="";
					for(int i=0;i<10;i++) TorqueResult[i] =0;
					
					for(int i=0;i<10;i++) BarcodeList[i] ="";
					for(int i=0;i<10;i++) BarcodeResult[i] =0;
					
					USTR.TestString0 = "";
					//USTR.TestString11 = "";// barcode 1
					//USTR.TestString12 = "";// barcode 2
					USTR.TestString3 = "";
					USTR.TestString4 = "";
					USTR.TestString5 = "";
					USTR.TestString6 = "";
					USTR.TestString7 = "";
					
					//UPAR.POP_PAR01 = 0;
					//UPAR.POP_PAR02 = 0;
					
					USTR.Barcode="";
					UPAR.PlcOutStatus=0;
					//USTR.LOT_ID ="";
					//USTR.LOT_ID2 ="";
					
					testCount =100;
					LogMsg.CMsg.Show("SCRIPT ","test count:",testCount.ToString(),false,true);
				}
				#endregion
				break;
				
			case 600:
				/*if(USTR.DbReceved !="")
				{
					string[] sa = USTR.DbReceved.ToString().Split(',');  
					if(sa.Length >=3)
					{
						USTR.TestString9 = sa[2];
					}
					testCount =100;
				}
				*/
				break;
		}
		
		
		
		//LogMsg.CMsg.Show("SCRIPT","status",PlcData.ToString()+StartTest.ToString() + EndTest.ToString(),false,true);
		Thread.Sleep(200);
	}
}
#endregion

#region // thread update display
static void ThreadMonitor0()
{
	string preRfid="";
	string sql ="";
	
	
	while(true)
	{
		if(USTR.RfidLot !=preRfid)
		{
			USTR.DbReceved ="";
			sql = "sp_JIT_GET_OPTION," +USTR.StationCode+ ","+ USTR.RfidLot;
			pop.SendDB(sql);
			LogMsg.CMsg.Show("SCRIPT","read option",sql,false,true);
			preRfid = USTR.RfidLot;
		}
		
		USTR.STR_BARCODE00 = BarcodeList[0];
		USTR.STR_BARCODE01 = BarcodeList[1];
		USTR.STR_BARCODE02 = BarcodeList[2];
		USTR.STR_BARCODE03 = BarcodeList[3];
		USTR.STR_BARCODE04 = BarcodeList[4];
		USTR.STR_BARCODE05 = BarcodeList[5];
		USTR.STR_BARCODE06 = BarcodeList[6];
		USTR.STR_BARCODE07 = BarcodeList[7];
		USTR.STR_BARCODE08 = BarcodeList[8];
		USTR.STR_BARCODE09 = BarcodeList[9];
		
		UPAR.PAR_BCOD_RST00 = BarcodeResult[0];
		UPAR.PAR_BCOD_RST01 = BarcodeResult[1];
		UPAR.PAR_BCOD_RST02 = BarcodeResult[2];
		UPAR.PAR_BCOD_RST03 = BarcodeResult[3];
		UPAR.PAR_BCOD_RST04 = BarcodeResult[4];
		UPAR.PAR_BCOD_RST05 = BarcodeResult[5];
		UPAR.PAR_BCOD_RST06 = BarcodeResult[6];
		UPAR.PAR_BCOD_RST07 = BarcodeResult[7];
		UPAR.PAR_BCOD_RST08 = BarcodeResult[8];
		UPAR.PAR_BCOD_RST09 = BarcodeResult[9];
		
		USTR.STR_TORQUE00 = TorqueList[0];
		USTR.STR_TORQUE01 = TorqueList[1];
		USTR.STR_TORQUE02 = TorqueList[2];
		USTR.STR_TORQUE03 = TorqueList[3];
		USTR.STR_TORQUE04 = TorqueList[4];
		USTR.STR_TORQUE05 = TorqueList[5];
		USTR.STR_TORQUE06 = TorqueList[6];
		USTR.STR_TORQUE07 = TorqueList[7];
		USTR.STR_TORQUE08 = TorqueList[8];
		USTR.STR_TORQUE09 = TorqueList[9];
		
		UPAR.PAR_TOQU_RST00 = TorqueResult[0];
		UPAR.PAR_TOQU_RST01 = TorqueResult[1];
		UPAR.PAR_TOQU_RST02 = TorqueResult[2];
		UPAR.PAR_TOQU_RST03 = TorqueResult[3];
		UPAR.PAR_TOQU_RST04 = TorqueResult[4];
		UPAR.PAR_TOQU_RST05 = TorqueResult[5];
		UPAR.PAR_TOQU_RST06 = TorqueResult[6];
		UPAR.PAR_TOQU_RST07 = TorqueResult[7];
		UPAR.PAR_TOQU_RST08 = TorqueResult[8];
		UPAR.PAR_TOQU_RST09 = TorqueResult[9];
		
		
		Thread.Sleep(100);
	}
}
#endregion

#region // waitng new seat
static string preBarcode = "";
static bool WaitingBarode()
{
	string barcode = USTR.Barcode;
	//LogMsg.CMsg.Show("SCRIPT ","waiting barcode","",false,true);
	
	if(preBarcode !=barcode)
	{
		//check barcode to db
		
		//pop.SendDB("sp_SUB_GET_LOTNO," + barcode + "," + USTR.PLAN_DATE + "," + USTR.PLAN_SEQUENCE);
		LogMsg.CMsg.Show("SCRIPT","check barcode lot",barcode,false,true);
	
		//Thread.Sleep(1000);
		//string s = USTR.DbReceved.ToString();
		
		//LogMsg.CMsg.Show("SCRIPT","check response:","",false,true);
		if(true)
		{
			int  ss= barcode.IndexOf("TL1");
			USTR.LOT_ID  = barcode.Substring(ss+1,barcode.Length  - ss -2);
			LogMsg.CMsg.Show("SCRIPT","received "+USTR.LOT_ID,"-- ok",false,true);
			if(USTR.LOT_ID.ToString().Length >13)
			{
				USTR.TestString11 = USTR.LOT_ID.ToString().Substring(11,1);
			}
			if(USTR.LOT_ID.ToString().Length >13)
			{
				USTR.TestString10 = USTR.LOT_ID.ToString().Substring(7,4);
			}
			barcode ="";
			USTR.Barcode ="";
			preBarcode="";
			return true;
		}
		preBarcode =barcode;
	}
	return false;
}
#endregion

#region // waitng new seat 2
static bool WaitingBarode2()
{
	string barcode = USTR.Barcode;
	//LogMsg.CMsg.Show("SCRIPT ","waiting barcode","",false,true);
	
	if(preBarcode !=barcode)
	{
		//check barcode to db
		
		//pop.SendDB("sp_SUB_GET_LOTNO," + barcode + "," + USTR.PLAN_DATE + "," + USTR.PLAN_SEQUENCE);
		LogMsg.CMsg.Show("SCRIPT","check barcode lot",barcode,false,true);
	
		//Thread.Sleep(1000);
		//string s = USTR.DbReceved.ToString();
		
		//LogMsg.CMsg.Show("SCRIPT","check response:","",false,true);
		if(true)
		{
			int  ss= barcode.IndexOf("TL1");
			USTR.LOT_ID2  = barcode.Substring(ss+1,barcode.Length  - ss -2);
			LogMsg.CMsg.Show("SCRIPT","received "+USTR.LOT_ID2,"-- ok",false,true);

			if(USTR.LOT_ID.ToString().Length >13)
			{
				USTR.TestString11 = USTR.LOT_ID.ToString().Substring(11,1);
			}
			
			barcode ="";
			USTR.Barcode ="";
			preBarcode="";
			return true;
		}
		preBarcode =barcode;
	}
	return false;
}
#endregion

#region // thread barcode
static void ThreadBarode()
{
	barcodeReadCount=0;
	string barcode = "";
	string preBarcode = "";
	LogMsg.CMsg.Show("SCRIPT ","barcode thread start","",false,true);
	while(true)
	{
		if(checkbarcode)
		{
			barcode = USTR.Barcode;
	        	// LogMsg.CMsg.Show("ThreadBarode - ",barcode,"",false,true);
			if(preBarcode !=barcode)
			{
				//check barcode to db
				
				//pop.SendDB("sp_SUB_GET_LOTNO," + barcode + "," + USTR.PLAN_DATE + "," + USTR.PLAN_SEQUENCE);
				LogMsg.CMsg.Show("SCRIPT","check barcode",barcode,false,true);
				
				//Thread.Sleep(1000);
				string s = USTR.DbReceved.ToString();
				
				LogMsg.CMsg.Show("SCRIPT","check response:","",false,true);
				
				#region true
				if(true)//lotNum != "")
				{
					BarcodeList[barcodeReadCount] =barcode;
					LogMsg.CMsg.Show("SCRIPT","barcode check "+barcodeReadCount.ToString(),"-- ok",false,true);
					BarcodeResult[barcodeReadCount]  = 1;
					barcodeReadCount++;
					barcode ="";
					USTR.Barcode ="";
					preBarcode="";
					
				}
				#endregion
				#region false
				else
				{
					BarcodeList[barcodeReadCount] =barcode;
					LogMsg.CMsg.Show("SCRIPT","barcode check "+barcodeReadCount.ToString(),"-- ng",false,true);
					BarcodeResult[barcodeReadCount]  = 2;
					
				}
				#endregion
				preBarcode = barcode;
			}
		}
		else
		{
			barcode="";
			preBarcode="";
		}
		Thread.Sleep(100);
	}
	LogMsg.CMsg.Show("SCRIPT","barcode thread stopped ","",false,true);

}
#endregion

#region // thread tool1
static void ThreadTool1()
{
	double torque =0;
	double angle = 0;
	double tightStaus = 0;	
	double pretorque =0;
	double preAngle = 0;
	float[,] _fData = new float[3, 2]; // tool 이 2개이면 6,1로 변경
	LogMsg.CMsg.Show("SCRIPT","tool1 thread start","",false,true);
	
	while(true)
	{
		if(checktool1)
		{
			// Read Torque
			PowerTool.PowerTool.ReadOneAI(ref _fData);
			torque = _fData[0, 0];
			tightStaus = _fData[1,0];
			angle = _fData[2, 0];
			
			UPAR.POP_PAR00= _fData[0, 1];
			
			if(torque > 0)
			{	
				if(pretorque != torque || preAngle != angle)
				{
					LogMsg.CMsg.Show("SCRIPT","tool1 receive tool data ", tool1_OK_Count.ToString(), false, true);
					//check check
					checkTool1Data(torque);
					if(tool1_OK_Count == NOofTORQUE) 
					{
						checktool1 = false;
						LogMsg.CMsg.Show("SCRIPT","checktool1 end","",false,true);
					}
					pretorque = torque;
					preAngle = angle;
 				}
			}	
		}
		else
		{
			PowerTool.PowerTool.ReadOneAI(ref _fData);
			
			torque = _fData[0, 0];
			tightStaus = _fData[1,0];
			angle = _fData[2, 0];
			UPAR.POP_PAR00= _fData[0, 1];
			
			pretorque = torque;
			preAngle = angle;
		}

		Thread.Sleep(100);
	}
	LogMsg.CMsg.Show("SCRIPT","tool1 thread stopped ","",false,true);

}
#endregion 

#region // check tool value
static bool isBetweenSpec(double value, double min, double max)
{
	return value >= min && value <= max;
}

static void checkTool1Data(double Tool1Data)
{
	// spec
	double min = 0.0;
	double max = 150.0;
	string strTemp = "";
	
	LogMsg.CMsg.Show("SCRIPT","checktool1 data ",Tool1Data.ToString(),false,true);
		
	// spec check
	if(isBetweenSpec(Tool1Data, min,  max))
	{
		strTemp = String.Format("{0:f2}", Tool1Data);
		TorqueList[tool1_OK_Count] = strTemp;
		TorqueResult[tool1_OK_Count] = 1;
		// ok spec Count increase + 1
		tool1_OK_Count = tool1_OK_Count + 1;
		
		LogMsg.CMsg.Show("SCRIPT","checktool1 ok:",tool1_OK_Count.ToString(),false,true);
	}
	else
	{
		strTemp = String.Format("{0:f2}", Tool1Data);
		//USTR.TestString3 = strTemp;
		torque1  = strTemp;
		
		// 불합격 알림
		wp_NG.PlaySync();

	}
}
#endregion

static void Reset()
{
	pop.OpenWindow(2,false);
					
	LogMsg.CMsg.Show("SCRIPT","reset","",false,true);


	
	for(int i=0;i<10;i++) TorqueList[i] ="";
	for(int i=0;i<10;i++) TorqueResult[i] =0;
	
	for(int i=0;i<10;i++) BarcodeList[i] ="";
	for(int i=0;i<10;i++) BarcodeResult[i] =0;
	USTR.Barcode="";
	UPAR.PlcOutStatus=0;
	
	testCount =100;
	
}

public void getToqueCB1(double torque, string tightStaus, double angle)
{
	LogMsg.CMsg.Show("SCRIPT","getToqueCB1 ",torque.ToString(),false,true);
	
	if (torque != -1)
	{
		//check check
		nut.dataClear();
	}
}



#region // screen event
static void Pop_ScreenEvent(string str)
{
	switch(str)
	{
		case "EVT_EVBVIEWER":	        // SCR_MEAS Convert
			break;
		
		case "EVT_BUTTON_MAIN":  	// SCR_MAIN Convert
			break;
		case "EVT_BUTTON_MEAS":	// SCR_MEAS Convert
			break;
		case "EVT_EXIT":
			nut.end();
            nut.sendCb -= new libNutTcp.toqueEventCb(getToqueCB1);
			Application.Exit();
			break;
		case "EVT_LR":                          // Change LR
			if(LR == "L")
			{
				LR = "R";
				USTR.TestString8 = LR;
				USTR.TestString9 = "L";
				
			}
			else
			{
				LR = "L";
				USTR.TestString8 = LR;
				USTR.TestString9 = "R";
			}
			
		
			
			break;
	}
}
#endregion