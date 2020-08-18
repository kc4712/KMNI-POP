﻿using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace POP
{
    class DBcontrol
    {
        #region 서버 관련 변수

        private string strConnect;				//연결문, dbConnect()전에 연결문 입력할 것

        public SqlConnection dbCon;
        public SqlConnection dbCon_S = new SqlConnection();

        public SqlCommand sCmd;

        public DataTable tbTemp = new DataTable();
        public DataTable dtResult = new DataTable();
        // public static SqlDataAdapter sAdapter;

        public String strQuery = string.Empty;

        #endregion


        public DBcontrol()
        {
            try
            {
                // 데이터베이스 모드와 연결 문자열을 전역 변수에 저장
                //this.iDBmode = iDBmode;
                //this.sConnStr = sConnStr;
            }
            catch
            {
                throw;
            }
        }

        ~DBcontrol()
        {
            try
            {
                strConnect = string.Empty;

                //OLCon = null;
                dbCon = null;
                //OCon = null;

                GC.Collect();
            }
            catch
            {
                throw;
            }
        }


        #region CheckDBConnection
        // static object lockobj = new object();
        public bool CheckDBConnection()						//strConnect에 할당된 조건으로 서버 연결
        {
            //lock(lockobj)
            //{

            dbCon = new SqlConnection();
            //SqlConnection dbCon = new SqlConnection(strConnect);
            if (INI.WORKNAME == "SIDE") { 
                strConnect = "Data Source=" + INI.SIDE.DB_IP + ";";
                strConnect += "Initial Catalog =" + INI.SIDE.DB_NAME + ";";
                strConnect += "User id = " + INI.SIDE.DB_ID + ";";
                strConnect += "pwd =" + INI.SIDE.DB_Password + ";";
            }
            if (INI.WORKNAME == "FRT")
            {
                strConnect = "Data Source=" + INI.FRT.DB_IP + ";";
                strConnect += "Initial Catalog =" + INI.FRT.DB_NAME + ";";
                strConnect += "User id = " + INI.FRT.DB_ID + ";";
                strConnect += "pwd =" + INI.FRT.DB_Password + ";";
            }
            // }

            //log.logWriteLine(strConnect, 0);  
            try
            {
                if (dbCon.State.ToString() == "Open")
                {
                    return true;
                }
                else
                {
                 
                    dbCon.ConnectionString = strConnect;
                    dbCon.Open();   //DB접속


                    return true;
                }
            }
            catch (Exception er)
            {
                //LogMsg.CMsg.Show("CheckDBConnection", "CheckDBConnection error", er.ToString(), false, true);
                return false;
            }
        }


        public  bool CheckDBConnection_Setting()						//strConnect에 할당된 조건으로 서버 연결
        {
            if(INI.WORKNAME == "SIDE") { 
                strConnect = "Data Source=" + INI.SIDE.DB_IP + ";";
                strConnect += "Initial Catalog =" + INI.SIDE.DB_NAME + ";";
                strConnect += "User id = " + INI.SIDE.DB_ID + ";";
                strConnect += "pwd =" + INI.SIDE.DB_Password + ";";
            }
            if(INI.WORKNAME == "FRT")
            {
                strConnect = "Data Source=" + INI.FRT.DB_IP + ";";
                strConnect += "Initial Catalog =" + INI.FRT.DB_NAME + ";";
                strConnect += "User id = " + INI.FRT.DB_ID + ";";
                strConnect += "pwd =" + INI.FRT.DB_Password + ";";
            }


            //log.logWriteLine(strConnect, 0);  
            try
            {
                if (dbCon_S.State.ToString() == "Open")
                {
                    return true;
                }
                else
                {
                    dbCon_S.ConnectionString = strConnect;
                    dbCon_S.Open();	//DB접속

                    return true;
                }
            }
            catch (Exception er)
            {
                //                MessageBox.Show(er.ToString());  
                return false;
            }
        }

        #endregion


        #region adaptRead
        public  bool adaptRead(string strQuery, string strTableName)
        {
            SqlCommandBuilder builder;
            bool bReturn = false;
            try
            {
                
                SqlDataAdapter sAdapter;
                sAdapter = new SqlDataAdapter(strQuery, dbCon);
                builder = new SqlCommandBuilder(sAdapter);
                sAdapter.SelectCommand.CommandTimeout = 5;

                tbTemp = null;
                tbTemp = new DataTable(strTableName);
                sAdapter.Fill(tbTemp);
                sAdapter.Dispose();
                bReturn = true;
            }

            catch (Exception er)
            {
                bReturn = false;
                //LogMsg.CMsg.Show("adaptRead error", strQuery, er.ToString(), false, true);
            }
            finally
            {
              CloseDBConnection();

            }
            return bReturn;
        }
        #endregion


        #region ExecSql
        public  int ExecSql(string strQuery)		//insert, update, delete 처리
        {
            int iResult = 0;
            try
            {

                sCmd = null;
                sCmd = new SqlCommand(strQuery, dbCon);
                sCmd.CommandTimeout = 5;
                iResult = (int)sCmd.ExecuteNonQuery();
            }
            catch (Exception er)
            {
                //LogMsg.CMsg.Show("ExecSql error", strQuery, er.ToString(), false, true);
            }
            finally
            {
               CloseDBConnection();
            }
            return iResult;
        }
        #endregion

        #region ExecuteScalar
        public  string ExecuteScalar(string strQuery)		//insert 처리
        {
            string iResult = "";
            try
            {
                sCmd = null;
                sCmd = new SqlCommand(strQuery, dbCon);
                sCmd.CommandTimeout = 5;

                iResult = Convert.ToString(sCmd.ExecuteScalar());
                //iResult = (int)sCmd.ExecuteNonQuery();
            }
            catch (Exception er)
            {
                //LogMsg.CMsg.Show("ExecuteScalar error", strQuery , er.ToString(), false, true);
            }
            finally
            {
               CloseDBConnection();
            }
            return iResult;
        }

        #endregion


        #region ExecSP
        public int ExecSPUpdate(string strStoredProcedure, params DictionaryEntry[] ParamName)
        {
            int iResult = 0;
            sCmd = null;
            sCmd = new SqlCommand(strStoredProcedure, dbCon);
            sCmd.CommandType = CommandType.StoredProcedure;

            foreach (DictionaryEntry paramV in ParamName)
            {
                sCmd.Parameters.AddWithValue(paramV.Key.ToString(), paramV.Value);
            }

            try
            {
                sCmd.CommandTimeout = 5;
                iResult = sCmd.ExecuteNonQuery();
            }
            catch (Exception er)
            {
                iResult = 0;
                //LogMsg.CMsg.Show("ExecSPUpdate", "ExecSPUpdate error - ", strStoredProcedure + "," + er.ToString(), false, true);
            }
            finally
            {
                CloseDBConnection();
            }
            return iResult;
        }

        #endregion

        #region ExecSPReader
        public bool ExecSPReader(string strStoredProcedure, string strTableName, params DictionaryEntry[] ParamName)
        {
            bool bReturn = false;
            sCmd = null;
            sCmd = new SqlCommand();
            sCmd.CommandText = strStoredProcedure;
            sCmd.CommandType = CommandType.StoredProcedure;
            sCmd.CommandTimeout = 5; //무한

            foreach (DictionaryEntry paramV in ParamName)
            {
                sCmd.Parameters.AddWithValue(paramV.Key.ToString(), paramV.Value);
            }
            SqlDataAdapter sAdapter;
            sAdapter = null;
            sAdapter = new SqlDataAdapter();
            sAdapter.SelectCommand = sCmd;
            sAdapter.SelectCommand.Connection = dbCon;
            sAdapter.SelectCommand.CommandTimeout = 5;
            dtResult = null;
            dtResult = new DataTable(strTableName);
            try
            {
                int iRecord = sAdapter.Fill(dtResult);
                //string strResult = "";
                //strResult = sCmd.Parameters["@LocateGubun"].Value.ToString();
                //MessageBox.Show(strResult);   
                bReturn = true;
            }
            catch (Exception er)
            {
                bReturn = false;

                //LogMsg.CMsg.Show("ExecSPReader", "ExecSPReader error", er.ToString(), false, true);
            }
            finally
            {
               CloseDBConnection();
            }
            return bReturn;
        }

        #endregion


        public bool CloseDBConnection()
        {
            try
            {

                // 연결 종료
                if (dbCon.State == System.Data.ConnectionState.Open)
                    dbCon.Close();

                // 연결 함수 반환
                dbCon.Dispose();

                // 연결 함수 초기화
                dbCon = null;

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                // 메모리 반환
                GC.Collect();
            }
        }

        #region [[ LOG ]]

        public static string LogFile = Application.StartupPath + "\\FileLog";
        public static string NGFile = Application.StartupPath + "\\Log";

        public static string LogFile_File = Application.StartupPath + "\\FileLog\\File_LOG" + ".txt";
        public static string LogFile_NG = Application.StartupPath + "\\Log\\NG_LOG(" + DateTime.Now.ToString("yyyy-MM") + ").txt";


        public static void FN_Log(int num, string strErrorLoc, string strErrorCode)
        {
            try
            {
                DirectoryInfo di;
                di = new DirectoryInfo(LogFile);
                if (di.Exists == false) di.Create();

                di = new DirectoryInfo(NGFile);
                if (di.Exists == false) di.Create();
                //di = new DirectoryInfo(recSystem.LogFile_NG.Substring(0, recSystem.LogFile_NG.Length - 4));
                //if (di.Exists == false) di.Create();

                switch (num)
                {
                    case 1:
                        //File.AppendAllText(Program.LOG_PATH + "FILELOG.log", "NAME [" + Item_Name + "] " + "SORT [" + Sort + "] " + "LV [" + Lv + "] " + "PATH [" + FilePath + "]" + "\r\n");
                        using (StreamWriter sw = File.AppendText(LogFile_File))
                        {
                            sw.WriteLine(strErrorCode);
                        }
                        break;

                    case 2:

                        using (StreamWriter sw = File.AppendText(LogFile_NG))
                        {
                            sw.WriteLine("[" + DateTime.Now.ToString("yyyyMMddHHmmss") + "]" + strErrorLoc + "=>" + strErrorCode);
                        }

                        break;
                }


            }
            catch
            { } //MessageBox.Show(ex.ToString()); }
        }
        #endregion
        
    }
}
