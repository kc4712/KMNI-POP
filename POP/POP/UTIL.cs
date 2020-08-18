using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;

namespace POP
{
    /*//Main TAG
    if (strBarcode.Length == 65)
    //harness TAG
    if (strBarcode.Length == 22) //22?            
    //SAB TAG
    if (strBarcode.Length == 55) //55?
    //FRAME TAG
    if (strBarcode.Length == 17) //?*/
    //lsupt
    //5052806BAA1210510182568



    ///vent mat lsupt 자리수 같음 숫자 변환 가능 불가능으로 확인
    ///MAIN과 SAB 자리수 같음...        
    ///VENT MOTOR 자리수 제일 긴상태
    /// <summary>
    /// FRT공정용 바코드 자리수를 이용한 구분자
    /// </summary>
    /// <param name="BARCODE_LENGTH_ID"></param>
    public enum BARCODE_LENGTH_ID : int
    {
        MAIN = 55,
        HARNESS = 22,
        FRAME = 14,
        FRAME2 = 17,
        LSUPT = 23,
        MOTOR = 65, /*65자리 이상 일 경우*/
        ID = 4,
        END = 3,
        NONE = 0
    };

    /// <summary>
    /// DB내용 송수신, 툴값 비교, 툴값 컨버트등 잡다한 메서드 모음 클래스
    /// </summary>
    /// <param name="UTIL"></param>
    public class UTIL
    {
        public double doubleConvert(string str)
        {
            double dbTemp = 0;
            try
            {

                dbTemp = double.Parse(str);

            }
            catch (Exception ex)
            {
                dbTemp = 0;
            }

            return dbTemp;
        }

        public bool isBetweenSpec(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public string SendDB(string szData)
        {
            String Sendstr = null;
            #region 변수 선언부

            DBcontrol dbcontrol = new DBcontrol();
            string QueryStr = string.Empty;

            // 받은 메시지 저장용
            string[] rData = null;
            string[] sData = null;

            int result = 0;

            string resultData = "";

            #endregion

            if (!dbcontrol.CheckDBConnection())
            {
                Console.WriteLine("Error");
                return "A_ERROR,";
            }

            try
            {
                szData = szData.Replace("\0", "");
                rData = szData.Split(',');

                if (rData.Length > 0)
                {
                    switch (rData[0].Trim())
                    {
                        case "sp_JIT_CHECK_BARCODE_SAB_SUB":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@BAR_LEN";
                                SP_Info[1].Value = rData[2].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_CHECK_BARCODE_SAB_SUB]", "sp_JIT_CHECK_BARCODE_SAB_SUB", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_CHECK_BARCODE_SAB_SUB";
                                    Console.WriteLine("dbcontrol.dtResult.Columns.Count:" + dbcontrol.dtResult.Columns.Count + " dbcontrol.dtResult.Rows:" + dbcontrol.dtResult.Rows.Count);

                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB, ERROR, 0";
                                }
                            }
                            break;
                        case "sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@LR";
                                SP_Info[0].Value = rData[1].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB]", "sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_GET_AMOUNT_TERMINAL_SAB_SUB";
                                    Console.WriteLine("dbcontrol.dtResult.Columns.Count:" + dbcontrol.dtResult.Columns.Count + " dbcontrol.dtResult.Rows:" + dbcontrol.dtResult.Rows.Count);

                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }
                            }
                            break;
                        case "sp_JIT_GET_INTERLOCK":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@STATIONCODE";  // SAB_SUB
                                SP_Info[0].Value = rData[1].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_GET_INTERLOCK]", "sp_JIT_GET_INTERLOCK", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_GET_INTERLOCK";
                                    Console.WriteLine("dbcontrol.dtResult.Columns.Count:" + dbcontrol.dtResult.Columns.Count + " dbcontrol.dtResult.Rows:" + dbcontrol.dtResult.Rows.Count);

                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }
                            }
                            break;
                        case "sp_JIT_SEL_SAB_SUB":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@LR";
                                SP_Info[0].Value = rData[1].Trim();
                                /*foreach (DataRow row in dbcontrol.dtResult.Rows)
                                {
                                    foreach (var item in row.ItemArray)
                                    {
                                        Console.WriteLine("dbctrl " + item);
                                        Sendstr += "," + item.ToString();
                                    }
                                }*/
                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_SEL_SAB_SUB]", "sp_JIT_SEL_SAB_SUB", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_SEL_SAB_SUB";
                                    if (dbcontrol.dtResult.Rows.Count > 0)
                                    {
                                        for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                        {
                                            Sendstr += ",SEQ" + (r + 1).ToString();
                                            Sendstr += "," + (r + 1).ToString();
                                            for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                            {
                                                Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                                Sendstr += "," + dbcontrol.dtResult.Rows[r][c].ToString();
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }
                            }
                            resultData = Sendstr;
                            break;

                        case "sp_JIT_SEL_USER":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@USER_ID";
                                SP_Info[0].Value = rData[1].Trim();
                                Console.WriteLine("rData[1].Trim():" + rData[1].Trim());
                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_SEL_USER]", "sp_JIT_SEL_USER", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_SEL_USER";
                                    Console.WriteLine("dbcontrol.dtResult.Columns.Count:" + dbcontrol.dtResult.Columns.Count + " dbcontrol.dtResult.Rows:" + dbcontrol.dtResult.Rows.Count);
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Console.WriteLine(dbcontrol.dtResult.Columns[c].ColumnName  + " " + dbcontrol.dtResult.Rows[0][c].ToString());
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }

                                //SendData(Sendstr);

                            }
                            break;

                        /*
                        try
                        {
                            if (dbcontrol.CheckDBConnection())
                            { //사용자 인증 쿼리
                                lb_dbCon.BackColor = System.Drawing.Color.LimeGreen;

                                //String Sendstr = null;
                                if (dbcontrol.CheckDBConnection())
                                {
                                    lb_dbCon.BackColor = Color.LimeGreen;

                                    dbcontrol.strQuery = "";

                                    DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                    SP_Info[0].Key = "@USER_ID";
                                    SP_Info[0].Value = UserID;

                                    if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_SEL_USER]", "sp_JIT_SEL_USER", SP_Info))
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {
                                            UserName = dbcontrol.dtResult.Rows[0][c].ToString();
                                        }
                                    }
                                    //dbcontrol.CloseDBConnection();
                                    LogMsg.CMsg.Show("SEL_USER", "-", UserID + " , " + UserName, false, true);
                                }
                            }
                            else
                            {
                                lb_dbCon.BackColor = Color.Red;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMsg.CMsg.Show("SEL_USER", "-", ex.ToString(), false, true);
                        }*/

                        case "sp_JIT_GET_OPTION_SAB_SUB_GA":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@SEATCODE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                Console.WriteLine("rData[1].Trim():" + rData[1].Trim());
                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_GET_OPTION_SAB_SUB_GA]", "sp_JIT_GET_OPTION_SAB_SUB_GA", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_GET_OPTION_SAB_SUB_GA";
                                    Console.WriteLine("dbcontrol.dtResult.Columns.Count:" + dbcontrol.dtResult.Columns.Count + " dbcontrol.dtResult.Rows:" + dbcontrol.dtResult.Rows.Count);
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }

                                //SendData(Sendstr);

                            }

                            break;
                        case "sp_JIT_GET_OPTION_SAB_SUB_JIN":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@SEATCODE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_JIT_GET_OPTION_SAB_SUB_JIN]", "sp_JIT_GET_OPTION_SAB_SUB_JIN", SP_Info))
                                {
                                    Sendstr += "A_sp_JIT_GET_OPTION_SAB_SUB_JIN";
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }
                                    resultData = Sendstr;
                                }
                                else
                                {
                                    resultData = "A_ERROR,";
                                }

                                //SendData(Sendstr);

                            }

                            break;
                        case "sp_JIT_INS_SUB_SAB":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[25];

                                SP_Info[0].Key = "@SEATCODE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();

                                SP_Info[2].Key = "@BARCODE1";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@BARCODE2";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@BARCODE3";
                                SP_Info[4].Value = rData[5].Trim();
                                SP_Info[5].Key = "@BARCODE4";
                                SP_Info[5].Value = rData[6].Trim();
                                SP_Info[6].Key = "@BARCODE5";
                                SP_Info[6].Value = rData[7].Trim();
                                SP_Info[7].Key = "@BARCODE6";
                                SP_Info[7].Value = rData[8].Trim();
                                SP_Info[8].Key = "@BARCODE7";
                                SP_Info[8].Value = rData[9].Trim();
                                SP_Info[9].Key = "@BARCODE8";
                                SP_Info[9].Value = rData[10].Trim();
                                SP_Info[10].Key = "@BARCODE9";
                                SP_Info[10].Value = rData[11].Trim();
                                SP_Info[11].Key = "@BARCODE10";
                                SP_Info[11].Value = rData[12].Trim();

                                SP_Info[12].Key = "@TORQUE1";
                                SP_Info[12].Value = rData[13].Trim();
                                SP_Info[13].Key = "@TORQUE2";
                                SP_Info[13].Value = rData[14].Trim();
                                SP_Info[14].Key = "@TORQUE3";
                                SP_Info[14].Value = rData[15].Trim();
                                SP_Info[15].Key = "@TORQUE4";
                                SP_Info[15].Value = rData[16].Trim();
                                SP_Info[16].Key = "@TORQUE5";
                                SP_Info[16].Value = rData[17].Trim();
                                SP_Info[17].Key = "@TORQUE6";
                                SP_Info[17].Value = rData[18].Trim();
                                SP_Info[18].Key = "@TORQUE7";
                                SP_Info[18].Value = rData[19].Trim();
                                SP_Info[19].Key = "@TORQUE8";
                                SP_Info[19].Value = rData[20].Trim();
                                SP_Info[20].Key = "@TORQUE9";
                                SP_Info[20].Value = rData[21].Trim();
                                SP_Info[21].Key = "@TORQUE10";
                                SP_Info[21].Value = rData[22].Trim();

                                SP_Info[22].Key = "@RESULT";
                                SP_Info[22].Value = rData[23].Trim();
                                SP_Info[23].Key = "@USERID";
                                SP_Info[23].Value = rData[24].Trim();
                                SP_Info[24].Key = "@USERNAME";
                                SP_Info[24].Value = rData[25].Trim();

                                result = dbcontrol.ExecSPUpdate("[dbo].[sp_JIT_INS_SUB_SAB]", SP_Info);
                                Console.WriteLine("sp_JIT_INS_SUB_SAB? = " + result);
                                //if(result > 0)
                                //{
                                // 막음
                                //SendData("A_sp_SUB_OUTPUT,True");
                                resultData = "A_sp_JIT_INS_SUB_SAB,True";
                                //}

                                //else
                                //{
                                // SendData("A_sp_SUB_OUTPUT,False");
                                //}
                            }
                            break;
                        case "sp_SUB_GET_OPTION":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[3];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@PLAN_DATE";
                                SP_Info[2].Value = rData[3].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_OPTION]", "sp_SUB_GET_OPTION", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_OPTION";
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }

                                }

                                //SendData(Sendstr);
                                resultData = Sendstr;
                            }

                            break;
                        case "sp_SUB_CHECK_BARCODE":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = rData[1].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_CHECK_BARCODE]", "sp_SUB_CHECK_BARCODE", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_CHECK_BARCODE";
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }

                                }

                                //SendData(Sendstr);
                                resultData = Sendstr;
                            }

                            break;
                        case "sp_SUB_CHECK_BARCODE2":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = rData[1].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_CHECK_BARCODE2]", "sp_SUB_CHECK_BARCODE2", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_CHECK_BARCODE2";
                                    for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][c].ToString();
                                    }

                                }

                                //SendData(Sendstr);
                                resultData = Sendstr;
                            }

                            break;
                        case "sp_SUB_OUTPUT":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[16];

                                SP_Info[0].Key = "@SETCODE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@BARCODE1";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@BARCODE2";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@TORQUE1";
                                SP_Info[4].Value = rData[5].Trim();
                                SP_Info[5].Key = "@TORQUE2";
                                SP_Info[5].Value = rData[6].Trim();
                                SP_Info[6].Key = "@TORQUE3";
                                SP_Info[6].Value = rData[7].Trim();
                                SP_Info[7].Key = "@TORQUE4";
                                SP_Info[7].Value = rData[8].Trim();
                                SP_Info[8].Key = "@TORQUE5";
                                SP_Info[8].Value = rData[9].Trim();
                                SP_Info[9].Key = "@TORQUE6";
                                SP_Info[9].Value = rData[10].Trim();
                                SP_Info[10].Key = "@TORQUE7";
                                SP_Info[10].Value = rData[11].Trim();

                                SP_Info[11].Key = "@RESULT";
                                SP_Info[11].Value = rData[12].Trim();
                                SP_Info[12].Key = "@USERID";
                                SP_Info[12].Value = rData[13].Trim();
                                SP_Info[13].Key = "@PLAN_DATE";
                                SP_Info[13].Value = rData[14].Trim();
                                SP_Info[14].Key = "@PLAN_SEQ";
                                SP_Info[14].Value = rData[15].Trim();
                                SP_Info[15].Key = "@USERNAME";
                                SP_Info[15].Value = rData[16].Trim();

                                result = dbcontrol.ExecSPUpdate("[dbo].[sp_SUB_OUTPUT]", SP_Info);

                                //if(result > 0)
                                //{
                                // 막음
                                //SendData("A_sp_SUB_OUTPUT,True");
                                resultData = "A_sp_SUB_OUTPUT,True";
                                //}

                                //else
                                //{
                                // SendData("A_sp_SUB_OUTPUT,False");
                                //}
                            }

                            break;

                        case "sp_SUB_GET_PLAN":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = rData[1].Trim();
                                SP_Info[1].Key = "@PLAN_DATE";
                                SP_Info[1].Value = rData[2].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_AMOUNT]", "sp_SUB_GET_AMOUNT", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_AMOUNT";


                                    for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {
                                            Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                            Sendstr += "," + dbcontrol.dtResult.Rows[r][c].ToString();
                                        }

                                    }
                                }

                                string[] arrTemp_SUB = new string[9];

                                //4*2 = 8
                                for (int i = 0; i < arrTemp_SUB.Length; i++)
                                {
                                    arrTemp_SUB[i] = ",PLAN_SEQUENCE" + (i + 1).ToString() + "," + (i + 1).ToString();
                                }

                                string arrItem_str = "";

                                if (dbcontrol.CheckDBConnection())
                                {
                                    if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_PLAN]", "sp_SUB_GET_PLAN", SP_Info))
                                    {
                                        for (int i = 0; i < arrTemp_SUB.Length; i++)
                                        {
                                            for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                            {
                                                arrTemp_SUB[i] += "," + dbcontrol.dtResult.Columns[c].ColumnName + (i + 1).ToString() + ",";
                                            }
                                        }

                                        for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                        {
                                            arrItem_str = ",PLAN_SEQUENCE" + (r + 1).ToString() + "," + (r + 1).ToString();

                                            for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                            {

                                                arrItem_str += "," + dbcontrol.dtResult.Columns[c].ColumnName + (r + 1).ToString();
                                                arrItem_str += "," + dbcontrol.dtResult.Rows[r][c].ToString();
                                            }

                                            arrTemp_SUB[r] = arrItem_str;
                                        }
                                    }
                                }

                                for (int i = 0; i < arrTemp_SUB.Length; i++)
                                {

                                    Sendstr += arrTemp_SUB[i];
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;
                            break;

                        case "sp_SUB_GET_BARCODE":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[4];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@SETCODE";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@LR";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@BARCDOE_TOTAL_LEN";
                                SP_Info[3].Value = rData[1].Trim().Length;

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_BARCODE]", "sp_SUB_GET_BARCODE", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_BARCODE";

                                    if (dbcontrol.dtResult.Rows[0][0].ToString() == "1")
                                        Sendstr += ",True";

                                    else
                                        Sendstr += ",False";
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;

                        case "sp_SUB_CHECK_TESTRESULT"://검사기 검사이력 체크
                                                       //LogMsg.CMsg.Show("DataProtocol - ", "sp_SUB_CHECK_TESTRESULT", rData[1].Trim() + "/" + @rData[1].Length.ToString(), false, true);
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = @rData[2].Trim();
                                SP_Info[1].Key = "@BAR_Length";
                                SP_Info[1].Value = @rData[2].Length;


                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_CHECK_TESTRESULT]", "sp_SUB_CHECK_TESTRESULT", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_CHECK_TESTRESULT";

                                    if (dbcontrol.dtResult.Rows[0][0].ToString() == "1")
                                    {
                                        Sendstr += "," + rData[1];
                                        Sendstr += ",X";
                                    }


                                    else
                                    {
                                        Sendstr += "," + rData[1];
                                        Sendstr += ",O";
                                    }
                                }
                            }

                            //SendData(Sendstr);

                            resultData = Sendstr;

                            break;


                        case "sp_SUB_OUTPUT_TESTRESULT": //검사기 저장
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";
                                DictionaryEntry[] SP_Info = new DictionaryEntry[6];

                                //rData[1].Trim();//장비 채널
                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = rData[2].Trim();//BARCODE
                                SP_Info[1].Key = "@DATE";
                                SP_Info[1].Value = rData[3].Trim().Replace("-", "");//DATE
                                SP_Info[2].Key = "@TIME";
                                SP_Info[2].Value = rData[4].Trim().Replace(":", "");//TIME
                                                                                    //rData[5].Trim();//CAR NAME
                                SP_Info[3].Key = "@LR";
                                SP_Info[3].Value = rData[6].Trim();//LH / RH
                                SP_Info[4].Key = "@RESULT";
                                SP_Info[4].Value = rData[7].Trim();//TEST RESULT
                                SP_Info[5].Key = "@RESULT_JUDGE";
                                SP_Info[5].Value = rData[8].Trim();//JUDG
                                                                   //rData[9].Trim(); SPARE1
                                                                   //rData[10].Trim(); SPARE2
                                                                   //rData[11].Trim(); SPARE3
                                                                   //rData[12].Trim(); SPARE4
                                                                   //rData[13].Trim(); SPARE5
                                                                   //rData[14].Trim(); SPARE6
                                                                   //rData[15].Trim(); SPARE7
                                                                   //rData[16].Trim(); SPARE8
                                                                   //rData[17].Trim(); SPARE9
                                                                   //rData[18].Trim(); SPARE10
                                                                   //rData[19].Trim(); SPARE10

                                result = dbcontrol.ExecSPUpdate("[dbo].[sp_SUB_OUTPUT_TESTRESULT]", SP_Info);

                                //if (result > 0)
                                //{
                                //SendData("A_sp_SUB_OUTPUT_TESTRESULT,True");
                                resultData = Sendstr;
                                //}

                                //else
                                //{
                                //    SendData("A_sp_SUB_OUTPUT,False");
                                //}
                            }


                            break;

                        case "sp_SUB_DEL_TESTRESULT"://검사기 삭제
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";
                                dbcontrol.strQuery = "DELETE FROM [dbo].[RESULT_REARSAB] WHERE SNO = '" + rData[1].Trim() + "'";

                                result = dbcontrol.ExecSql(dbcontrol.strQuery);

                                if (result > 0)
                                {
                                    //SendData("A_sp_SUB_DEL_TESTRESULT,True");
                                    resultData = "A_sp_SUB_DEL_TESTRESULT,True";
                                }

                                else
                                {
                                    //SendData("A_sp_SUB_DEL_TESTRESULT,False");
                                    resultData = "A_sp_SUB_DEL_TESTRESULT,False";
                                }
                            }


                            break;

                        case "sp_SUB_GET_BARCODEPRINT":
                            if (dbcontrol.CheckDBConnection())
                            {

                                DictionaryEntry[] SP_Info = new DictionaryEntry[5];

                                //SIDEBORSTER_GSUV
                                if (rData[1] == "1")
                                {

                                    SP_Info[0].Key = "@GUBUN";
                                    SP_Info[0].Value = "1";
                                }

                                else
                                {
                                    SP_Info[0].Key = "@GUBUN";
                                    SP_Info[0].Value = "0";
                                }

                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@SETCODE";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@PLANDATE";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@PLANSEQ";
                                SP_Info[4].Value = rData[5].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_BARCODEPRINT]", "sp_SUB_GET_BARCODEPRINT", SP_Info))
                                {

                                    for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {

                                            Sendstr += dbcontrol.dtResult.Rows[r][c].ToString() + ",";

                                        }
                                    }
                                }
                            }

                            //SendData(Sendstr.Remove(Sendstr.Length - 1));
                            resultData = Sendstr.Remove(Sendstr.Length - 1);
                            //SendData(Sendstr + ",");

                            break;



                        case "sp_SUB_GET_BARCODEPRINT_temp":
                            if (dbcontrol.CheckDBConnection())
                            {

                                DictionaryEntry[] SP_Info = new DictionaryEntry[5];

                                //SIDEBORSTER_GSUV
                                if (rData[1] == "1")
                                {

                                    SP_Info[0].Key = "@GUBUN";
                                    SP_Info[0].Value = "1";
                                }

                                else
                                {
                                    SP_Info[0].Key = "@GUBUN";
                                    SP_Info[0].Value = "0";
                                }

                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@SETCODE";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@PLANDATE";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@PLANSEQ";
                                SP_Info[4].Value = rData[5].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_BARCODEPRINT_temp]", "sp_SUB_GET_BARCODEPRINT_temp", SP_Info))
                                {

                                    for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {

                                            Sendstr += dbcontrol.dtResult.Rows[r][c].ToString() + ",";

                                        }
                                    }
                                }
                            }

                            //SendData(Sendstr.Remove(Sendstr.Length - 1));
                            resultData = Sendstr.Remove(Sendstr.Length - 1);
                            //SendData(Sendstr + ",");

                            break;


                        case "sp_SUB_GET_LOTNO":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[4];

                                SP_Info[0].Key = "@BARCODE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@PLAN_DATE";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@PLAN_SEQ";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@BARCODE_LEN";
                                SP_Info[3].Value = (rData[1].Length).ToString();

                                //LogMsg.CMsg.Show("sp_SUB_GET_LOTNO - ", rData[1].Trim() + "/", (rData[1].Length - 2).ToString(), false, true);

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_LOTNO]", "sp_SUB_GET_LOTNO", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_LOTNO";

                                    if (dbcontrol.dtResult.Rows.Count > 0)
                                    {
                                        Sendstr += "," + dbcontrol.dtResult.Rows[0][0].ToString();
                                    }

                                    else
                                    {
                                        Sendstr += ",X";
                                    }
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;

                        case "sp_SUB_OUTPUT_UPDATE":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[11];

                                SP_Info[0].Key = "@LOTNO";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@LOTNO_LEN";
                                SP_Info[1].Value = rData[1].Trim().Length;
                                SP_Info[2].Key = "@TORQUE1";
                                SP_Info[2].Value = rData[2].Trim();
                                SP_Info[3].Key = "@TORQUE2";
                                SP_Info[3].Value = rData[3].Trim();
                                SP_Info[4].Key = "@TORQUE3";
                                SP_Info[4].Value = rData[4].Trim();
                                SP_Info[5].Key = "@TORQUE4";
                                SP_Info[5].Value = rData[5].Trim();
                                SP_Info[6].Key = "@TORQUE5";
                                SP_Info[6].Value = rData[6].Trim();
                                SP_Info[7].Key = "@TORQUE6";
                                SP_Info[7].Value = rData[7].Trim();
                                SP_Info[8].Key = "@TORQUE7";
                                SP_Info[8].Value = rData[8].Trim();
                                SP_Info[9].Key = "@USERID";
                                SP_Info[9].Value = rData[9].Trim();
                                SP_Info[10].Key = "@USERNAME";
                                SP_Info[10].Value = rData[10].Trim();

                                result = dbcontrol.ExecSPUpdate("[dbo].[sp_SUB_OUTPUT_UPDATE]", SP_Info);

                                //if (result > 0)
                                //{
                                //SendData("A_sp_SUB_OUTPUT_UPDATE,True");
                                resultData = "A_sp_SUB_OUTPUT_UPDATE,True";
                                //}

                                //else
                                //{
                                //    SendData("A_sp_SUB_OUTPUT_UPDATE,False");
                                //}
                            }

                            //SendData(Sendstr);

                            break;

                        case "sp_SUB_GET_AMOUNT2":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[5];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@SETCODE";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@PLAN_DATE";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@PLAN_SEQUENCE";
                                SP_Info[4].Value = rData[5].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_AMOUNT2]", "sp_SUB_GET_AMOUNT2", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_AMOUNT2,";

                                    for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {

                                            Sendstr += dbcontrol.dtResult.Rows[r][c].ToString() + ",";

                                        }
                                    }
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;
                        case "sp_SUB_GET_AMOUNT3":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";

                                DictionaryEntry[] SP_Info = new DictionaryEntry[5];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = rData[2].Trim();
                                SP_Info[2].Key = "@SETCODE";
                                SP_Info[2].Value = rData[3].Trim();
                                SP_Info[3].Key = "@PLAN_DATE";
                                SP_Info[3].Value = rData[4].Trim();
                                SP_Info[4].Key = "@PLAN_SEQUENCE";
                                SP_Info[4].Value = rData[5].Trim();

                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_AMOUNT3]", "sp_SUB_GET_AMOUNT3", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_AMOUNT3,";

                                    for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                    {
                                        for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {

                                            Sendstr += dbcontrol.dtResult.Rows[r][c].ToString() + ",";

                                        }
                                    }
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;

                        case "sp_SUB_GET_OPTION_9BUX":
                            if (dbcontrol.CheckDBConnection())
                            {
                                DictionaryEntry[] SP_Info = new DictionaryEntry[1];

                                SP_Info[0].Key = "@LR";
                                SP_Info[0].Value = @rData[1].Trim();


                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_OPTION_9BUX]", "sp_SUB_GET_OPTION_9BUX", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_OPTION_9BUX";
                                    Console.WriteLine(dbcontrol.dtResult.Rows.Count + "   " + dbcontrol.dtResult.Columns.Count);



                                    if (dbcontrol.dtResult.Rows.Count > 0)
                                    {
                                        /*for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                        {
                                            //Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                            //Sendstr += "," + dbcontrol.dtResult.Rows[1][c].ToString();
                                            //Console.WriteLine("dbcontrol.dtResult.Rows[1][c].ToString() " + dbcontrol.dtResult.Rows[1][c].ToString());
                                        }*/
                                        foreach (DataRow row in dbcontrol.dtResult.Rows)
                                        {
                                            foreach (var item in row.ItemArray)
                                            {
                                                Console.WriteLine("dbctrl " + item);
                                                Sendstr += "," + item.ToString();
                                            }
                                        }

                                        Console.WriteLine("Sendstr " + Sendstr);

                                    }

                                    else
                                    {
                                        Sendstr += ",LOTNO,X";
                                    }



                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;

                        case "sp_SUB_GET_RESULT":
                            if (dbcontrol.CheckDBConnection())
                            {
                                DictionaryEntry[] SP_Info = new DictionaryEntry[3];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = @rData[2].Trim();
                                SP_Info[2].Key = "@PLAN_DATE";
                                SP_Info[2].Value = @rData[3].Trim();
                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_RESULT]", "sp_SUB_GET_RESULT", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_RESULT";

                                    if (dbcontrol.dtResult.Rows.Count > 0)
                                    {
                                        for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                        {
                                            Sendstr += ",SEQ" + (r + 1).ToString();
                                            Sendstr += "," + (r + 1).ToString();

                                            for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                            {

                                                Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                                Sendstr += "," + dbcontrol.dtResult.Rows[r][c].ToString();
                                            }
                                        }
                                    }
                                }
                            }

                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;
                        case "sp_SUB_GET_RESULT2":
                            if (dbcontrol.CheckDBConnection())
                            {
                                DictionaryEntry[] SP_Info = new DictionaryEntry[3];

                                SP_Info[0].Key = "@LINE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@LR";
                                SP_Info[1].Value = @rData[2].Trim();
                                SP_Info[2].Key = "@PLAN_DATE";
                                SP_Info[2].Value = @rData[3].Trim();
                                if (dbcontrol.ExecSPReader("[dbo].[sp_SUB_GET_RESULT2]", "sp_SUB_GET_RESULT2", SP_Info))
                                {
                                    Sendstr += "A_sp_SUB_GET_RESULT2";

                                    if (dbcontrol.dtResult.Rows.Count > 0)
                                    {
                                        for (int r = 0; r < dbcontrol.dtResult.Rows.Count; r++)
                                        {
                                            Sendstr += ",SEQ" + (r + 1).ToString();
                                            Sendstr += "," + (r + 1).ToString();

                                            for (int c = 0; c < dbcontrol.dtResult.Columns.Count; c++)
                                            {

                                                Sendstr += "," + dbcontrol.dtResult.Columns[c].ColumnName;
                                                Sendstr += "," + dbcontrol.dtResult.Rows[r][c].ToString();
                                            }
                                        }
                                    }


                                }
                            }
                            //SendData(Sendstr);
                            resultData = Sendstr;

                            break;

                        case "sp_SUB_JOB_COMPLETE":
                            if (dbcontrol.CheckDBConnection())
                            {
                                dbcontrol.strQuery = "";
                                DictionaryEntry[] SP_Info = new DictionaryEntry[2];

                                SP_Info[0].Key = "@PLANDATE";
                                SP_Info[0].Value = @rData[1].Trim();
                                SP_Info[1].Key = "@PLANSEQ";
                                SP_Info[1].Value = @rData[2].Trim();
                                result = dbcontrol.ExecSPUpdate("[dbo].[sp_SUB_JOB_COMPLETE]", SP_Info);

                                //if(result > 0)
                                //{
                                // 막음
                                //SendData("A_sp_SUB_OUTPUT,True");
                                resultData = "A_sp_SUB_JOB_COMPLETE,True";
                                //}

                                //else
                                //{
                                // SendData("A_sp_SUB_OUTPUT,False");
                                //}
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //LogMsg.CMsg.Show("DataProtocol - Error", "", ex.ToString(), false, true);
                return "A_ERROR,";
            }
            finally
            {

                #region 선언한 변수 초기화 및 메모리반환

                QueryStr = string.Empty;
                rData = null;
                // 쿼리 실행 결과 저장용 함수
                result = 0;
                // 메모리 정리
                GC.Collect();
                #endregion
            }

            return resultData;

        }

    }
}
