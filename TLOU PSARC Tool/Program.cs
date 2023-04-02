using System;
using System.Windows.Forms;
using TLOU_PSARC_Tool.Forms;

namespace TLOU_PSARC_Tool
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
