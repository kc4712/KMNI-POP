using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POP
{
    /// <summary>
    /// 프로그램 시작시 LOG디렉토리 내 파일 용량이 50MB를 넘어갈시 로그 삭제
    /// 공정과 조립에 따라 UI를 호출, 각 UI FORM클래스가 로드되고 해당 방향을 다시 지정
    /// </summary>
    /// <param name="Program"></param>
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            long length = Directory.GetFiles(".\\Log", "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
            string[] filename = Directory.GetFiles(".\\Log", "*", SearchOption.AllDirectories);
            Console.WriteLine("length" + length);
            if (length > 50000000)
            {
                foreach (string str in filename)
                {
                    File.Delete(str);
                    length = Directory.GetFiles(".\\Log", "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
                    if (length < 50000000)
                    {
                        break;
                    }
                }
            }
            if (INI.WORKNAME == "SIDE") { 
                if(INI.SIDE.Type == "GA")
                {
                    Application.Run(new Side_GA());
                }
                if(INI.SIDE.Type == "JIN")
                {
                    Application.Run(new Side_JIN());
                }
            }
            if (INI.WORKNAME == "FRT")
            {
                if (INI.FRT.Type == "GA")
                {
                    Application.Run(new FRT_GA());
                }
                if (INI.FRT.Type == "JIN")
                {
                    Application.Run(new FRT_JIN());
                }
            }

        }
    }
}
