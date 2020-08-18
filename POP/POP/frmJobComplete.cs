using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections;

namespace POP
{
    public partial class frmJobComplete : Form
    {
        public delegate void JobCompleteCB(bool ok_cancel);
        public event JobCompleteCB cbComplete;

        public frmJobComplete()
        {
            InitializeComponent();
        }

        private void frmJobComplete_Load(object sender, EventArgs e)
        {
            lblMessage.Text = "작업 완료 처리\n 하시겠습니까?";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cbComplete?.Invoke(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cbComplete?.Invoke(true);
        }
        
        
        
    }
}
