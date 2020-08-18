using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace POP
{
    /// <summary>
    /// Setting.ini 파싱 클래스
    /// </summary>
    /// <param name="INI"></param>
    static class INI
    {
        /// <summary>
        ///  프로그램을 실행하기 위한 공정 이름 - "SIDE", "FRT"
        /// </summary>
        /// <param name="WORKNAME"></param>
        public static string WORKNAME = Setting_INI.ReadValue("WORKNAME", "WORKNAME");
        // INI(프로그램 타입, 방향, 툴, 기준값 ,DB) 관련 변수
        public static class SIDE
        {
            
            public static string Server_IP = Setting_INI.ReadValue("SIDE", "SERVERIP");
            /// <summary>
            /// 작업PC의 조립명= "JIN", "GA"
            /// </summary>
            /// <param name="Type"></param>
            public static string Type = Setting_INI.ReadValue("SIDE", "TYPE");
            /// <summary>
            /// 작업PC의 작업방향 = 'L', 'R'
            /// </summary>
            /// <param name="Direction"></param>
            public static string Direction = Setting_INI.ReadValue("SIDE", "DIR");
            /// <summary>
            /// SIDE 공정 한정구분 변수
            /// 1번툴: RS232, 2번툴:RS232 = '1' 
            /// 1번툴: ETHERNET, 2번툴:RS232 = '2'
            /// 1번툴: ETHERNET, 2번툴:ETHERNET = '3'
            /// </summary>
            /// <param name="DeviceType"></param>
            public static string DeviceType = Setting_INI.ReadValue("SIDE", "DEVICETYPE");
            /// <summary>
            /// 가조립 1번툴 최소 툴값
            /// </summary>
            /// <param name="GATOOL1MIN"></param>
            public static string GATOOL1MIN = Setting_INI.ReadValue("SIDE", "GATOOL1MIN");
            /// <summary>
            /// 가조립 1번툴 최대 툴값
            /// </summary>
            /// <param name="GATOOL1MAX"></param>
            public static string GATOOL1MAX = Setting_INI.ReadValue("SIDE", "GATOOL1MAX");
            /// <summary>
            /// 가조립 2번툴 최소 툴값
            /// </summary>
            /// <param name="GATOOL2MIN"></param>
            public static string GATOOL2MIN = Setting_INI.ReadValue("SIDE", "GATOOL2MIN");
            /// <summary>
            /// 가조립 2번툴 최대 툴값
            /// </summary>
            /// <param name="GATOOL2MAX"></param>
            public static string GATOOL2MAX = Setting_INI.ReadValue("SIDE", "GATOOL2MAX");
            /// <summary>
            /// 진조립 1번툴 최소 툴값
            /// </summary>
            /// <param name="JINTOOL1MIN"></param>
            public static string JINTOOL1MIN = Setting_INI.ReadValue("SIDE", "JINTOOL1MIN");
            /// <summary>
            /// 진조립 1번툴 최대 툴값
            /// </summary>
            /// <param name="JINTOOL1MAX"></param>
            public static string JINTOOL1MAX = Setting_INI.ReadValue("SIDE", "JINTOOL1MAX");
            /// <summary>
            /// 진조립 2번툴 최소 툴값
            /// </summary>
            /// <param name="JINTOOL2MIN"></param>
            public static string JINTOOL2MIN = Setting_INI.ReadValue("SIDE", "JINTOOL2MIN");
            /// <summary>
            /// 진조립 2번툴 최대 툴값
            /// </summary>
            /// <param name="JINTOOL2MAX"></param>
            public static string JINTOOL2MAX = Setting_INI.ReadValue("SIDE", "JINTOOL2MAX");
            public static string NUT1IP = Setting_INI.ReadValue("SIDE", "NUT1IP");
            public static string NUT2IP = Setting_INI.ReadValue("SIDE", "NUT2IP");
            public static string ADAMIP = Setting_INI.ReadValue("SIDE", "ADAMIP");
            public static string ADAMPORT = Setting_INI.ReadValue("SIDE", "ADAMPORT");
            public static string ADAMDI = Setting_INI.ReadValue("SIDE", "ADAMDI");
            public static string ADAMDO = Setting_INI.ReadValue("SIDE", "ADAMDO");
            public static string PRINTIPGSUV = Setting_INI.ReadValue("SIDE", "PRINTIPGSUV");
            public static string PRINTIP9BUX = Setting_INI.ReadValue("SIDE", "PRINTIP9BUX");
            public static string DB_IP = Setting_INI.ReadValue("SIDE", "DBIP");
            public static string DB_NAME = Setting_INI.ReadValue("SIDE", "DBNAME");
            public static string DB_ID = Setting_INI.ReadValue("SIDE", "DBID");
            public static string DB_Password = Setting_INI.ReadValue("SIDE", "DBPass");
        }
        public static class FRT
        {
            public static string Server_IP = Setting_INI.ReadValue("FRT", "SERVERIP");
            public static string Type = Setting_INI.ReadValue("FRT", "TYPE");
            public static string Direction = Setting_INI.ReadValue("FRT", "DIR");
            public static string JINTOOL1MIN = Setting_INI.ReadValue("FRT", "JINTOOL1MIN");
            public static string JINTOOL1MAX = Setting_INI.ReadValue("FRT", "JINTOOL1MAX");
            public static string NUT1IP = Setting_INI.ReadValue("FRT", "NUT1IP");
            public static string ADAMIP = Setting_INI.ReadValue("FRT", "ADAMIP");
            public static string DB_IP = Setting_INI.ReadValue("FRT", "DBIP");
            public static string DB_NAME = Setting_INI.ReadValue("FRT", "DBNAME");
            public static string DB_ID = Setting_INI.ReadValue("FRT", "DBID");
            public static string DB_Password = Setting_INI.ReadValue("FRT", "DBPass");
        }

    }



    class Setting_INI
    {
        /*string strSection :[section]
         *string strKey : 값의 키
         *string val : 키의 값
         *filePath : 쓸 ini 파일경로
         */
        [DllImport("Kernel32")]
        private static extern long WritePrivateProfileString(String section, String key, String val, String filePath);


        /*
         * String section : 가져올 값의 키가 속해있는 섹션이름
         * String key : 가져올 값의 키이름
         * String def : 키의 값이 없을 경우 기본값은 Default
         * StringBuilder retVal : 가져올 값
         * int size : 가져올 값의 길이
         * string filePath : 읽어올 ini 파일경로
         */

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);


        public static string iPath = System.Windows.Forms.Application.StartupPath + @"\Setting.ini";

        //파일 쓰기
        public static void WriteValue(String strSection, String strKey, String strValue)
        {
            WritePrivateProfileString(strSection, strKey, strValue, iPath);
        }

        //파일 읽기

        public static string ReadValue(String strSection, String Key)
        {
            StringBuilder strValue = new StringBuilder(500);
            int i = GetPrivateProfileString(strSection, Key, "", strValue, 500, iPath);
            return strValue.ToString();
        }

    }
}