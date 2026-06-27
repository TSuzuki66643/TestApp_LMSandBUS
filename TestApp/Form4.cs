using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            Option();
        }

        private void Option()
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ProcessStartInfo copy = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c echo F | xcopy " + appPath + "\\Config.ini " + userFolder + "\\Documents\\LMS_Backup\\" + textBox1.Text + ".ini /Y", // /c は実行後にcmdを閉じる
                UseShellExecute = false,
                CreateNoWindow = true // コマンドプロンプト画面を表示しない
            };

            Process.Start(copy);
            this.Close();
        }
    }
}
