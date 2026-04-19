using Markdig;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using IniParser;


namespace TestApp
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
        }
        private async void Form2_Load(object sender, EventArgs e)
        {
            stop = true;
            comboBox1.SelectedIndex = Form1.IsAlreadyLocked[0];
            comboBox2.SelectedIndex = Form1.IsAlreadyLocked[3];
            comboBox3.SelectedIndex = Form1.IsAlreadyLocked[2];
            comboBox4.SelectedIndex = Form1.IsAlreadyLocked[1];
            if (checkBox2.Checked)
            {
                label8.Text = "DebugCode = " + Form1.DebugCode.ToString();
                tabPage3.Update();
            }
            var text = File.ReadAllText(@"readme.md");
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdig.Markdown.ToHtml(text, pipeline);
            richTextBox1.Text = result;
            comboBox5.SelectedIndex = Form1.StartPage;
            tabPage4.Update();
            stop = false;
        }

        public AudioFileReader[] push = new AudioFileReader[3];
        public bool stop = true;

        //private IniParser.Model.IniData data = new IniParser.Model.IniData();
        private void StartAudio(int number, bool stop)
        {
            if (!stop) 
            {
                if (!stop)
                {
                    push[0] = new AudioFileReader("push0.mp3");
                    push[1] = new AudioFileReader("push1.mp3");
                    push[2] = new AudioFileReader("push2.mp3");
                    var outputDevice = new WaveOutEvent();
                    outputDevice.Init(push[number]);
                    outputDevice.Play();
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            Form1.IsAlreadyLocked[0] = comboBox1.SelectedIndex;
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 1 && comboBox3.SelectedIndex == 1 && comboBox4.SelectedIndex == 1)
            {
                checkBox1.Checked = true;

            }
            else
            {
                checkBox1.Checked = false;
            }
            if (comboBox1.SelectedIndex == 0)
            {
                checkBox3.Checked = false;
            }
            else
            {
                checkBox3.Checked = true;
            }

            if (stop) 
            {
                stop = false;
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            Form1.IsAlreadyLocked[1] = comboBox4.SelectedIndex;
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 1 && comboBox3.SelectedIndex == 1 && comboBox4.SelectedIndex == 1)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            if (comboBox4.SelectedIndex == 0)
            {
                checkBox4.Checked = false;
            }
            else
            {
                checkBox4.Checked = true;
            }
            if (stop)
            {
                stop = false;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            Form1.IsAlreadyLocked[2] = comboBox3.SelectedIndex;
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 1 && comboBox3.SelectedIndex == 1 && comboBox4.SelectedIndex == 1)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            if (comboBox3.SelectedIndex == 0)
            {
                checkBox5.Checked = false;
            }
            else
            {
                checkBox5.Checked = true;
            }
            if (stop)
            {
                stop = false;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            Form1.IsAlreadyLocked[3] = comboBox2.SelectedIndex;
            if (comboBox1.SelectedIndex == 1 && comboBox2.SelectedIndex == 1 && comboBox3.SelectedIndex == 1 && comboBox4.SelectedIndex == 1)
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
            if (comboBox2.SelectedIndex == 0)
            {
                checkBox6.Checked = false;
            }
            else
            {
                checkBox6.Checked = true;
            }
            if (stop)
            {
                stop = false;
            }
        }
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(2, false);
            stop = true;
            var page = tabControl1.SelectedIndex;
            string text = "";
            switch (page)
            {
                case 0:
                    text = "遅延時に使うツールです。\n各方向ごとに設定できます。";
                    break;
                case 1:
                    text = "手動で表示を変えるツールです。\n臨時ダイヤ等で使います。\n(未完成)";
                    break;
                case 2:
                    text = "デバッグ用ツールです。\nコードが6以外だとエラーが起こってます。";
                    break;
                case 5:
                    text = "シミュレーションモードの設定。\n(未実装)";
                    break;
                default:
                    text = "";
                    break;
            }
            label7.Text = text;

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            int Checked = checkBox1.Checked ? 1 : 0;
            for (int i = 0; i < 4; i++)
            {
                if (Checked == 1)
                {
                    Form1.IsAlreadyLocked[i] = Checked;
                }
            }
            if (Checked == 1)
            {
                comboBox1.SelectedIndex = Checked;
                comboBox2.SelectedIndex = Checked;
                comboBox3.SelectedIndex = Checked;
                comboBox4.SelectedIndex = Checked;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartAudio(1, stop);
            stop = true;
            int Checked = 0;
            for (int i = 0; i < 4; i++)
            {

                Form1.IsAlreadyLocked[i] = Checked;

            }

            comboBox1.SelectedIndex = Checked;
            comboBox2.SelectedIndex = Checked;
            comboBox3.SelectedIndex = Checked;
            comboBox4.SelectedIndex = Checked;
            stop = false;

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            int Checked = checkBox2.Checked ? 1 : 0;
            if (Checked == 1)
            {
                label8.Text = "DebugCode = " + Form1.DebugCode.ToString();
                tabPage3.Update();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            int data = checkBox3.Checked ? 1 : 0;
            comboBox1.SelectedIndex = data;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            int data = checkBox4.Checked ? 1 : 0;
            comboBox4.SelectedIndex = data;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            int data = checkBox5.Checked ? 1 : 0;
            comboBox3.SelectedIndex = data;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            StartAudio(0, stop);
            stop = true;
            int data = checkBox6.Checked ? 1 : 0;
            comboBox2.SelectedIndex = data;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(2, false);
            int SelectedIndex = comboBox5.SelectedIndex;
            Form1.StartPage = SelectedIndex;
            Form1.Setinidata();
           
        }
    }
}
