using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POP
{
    /// <summary>
    /// 작업 표준서 클래스
    /// </summary>
    /// <param name="Frm_WorkStandard"></param>
    public partial class Frm_WorkStandard : Form
    {
        public Frm_WorkStandard()
        {
            InitializeComponent();
        }
        private void Frm_WorkStandard_Load(object sender, EventArgs e)
        {
            bool DIREXISTS = System.IO.Directory.Exists("..\\작업표준서");

            if (!DIREXISTS)
            {
                System.IO.Directory.CreateDirectory("..\\작업표준서");
            }
            else
            {
                if (INI.WORKNAME == "SIDE")
                {
                    if (INI.SIDE.Type == "GA")
                    {
                        bool FILEEXISTS = System.IO.File.Exists("..\\작업표준서\\9B_RBSB020.PNG");
                        if (FILEEXISTS)
                        {
                            pictureBox1.Load("..\\작업표준서\\9B_RBSB020.PNG");
                        }
                    }
                    if (INI.SIDE.Type == "JIN")
                    {
                        bool FILEEXISTS = System.IO.File.Exists("..\\작업표준서\\9B_RBSB030.PNG");
                        if (FILEEXISTS)
                        {
                            pictureBox1.Load("..\\작업표준서\\9B_RBSB030.PNG");
                        }
                    }
                }
                if (INI.WORKNAME == "FRT")
                {
                    if (INI.FRT.Type == "GA")
                    {
                        bool FILEEXISTS = System.IO.File.Exists("..\\작업표준서\\9B-FBS2.PNG");
                        if (FILEEXISTS)
                        {
                            pictureBox1.Load("..\\작업표준서\\9B-FBS2.PNG");
                        }
                    }
                    if (INI.FRT.Type == "JIN")
                    {
                        bool FILEEXISTS = System.IO.File.Exists("..\\작업표준서\\9B-FBS1.PNG");
                        if (FILEEXISTS)
                        {
                            pictureBox1.Load("..\\작업표준서\\9B-FBS1.PNG");
                        }
                    }
                }
            }
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}
