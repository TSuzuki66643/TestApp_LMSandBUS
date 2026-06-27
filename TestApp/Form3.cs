using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        public static string Msg;
        public static int Progress;

        private async void Form3_Load(object sender, EventArgs e)
        {
            Msg = "";
            Progress = 0;
            label1.Text = Msg;
            progressBar1.Value = Progress;
            label1.Update();
            progressBar1.Update();

            await Wait();
           
            this.Close();
            Form1.Form3_closed = true;
        }
        public async Task Wait()
        { 
            while (progressBar1.Value < 100)
            {
                label1.Text = Msg;
                progressBar1.Value = Progress;
                label1.Update();
                progressBar1.Update(); 
                await Task.Delay(100); 
            }
            await Task.Delay(100);

        }
    }
}
