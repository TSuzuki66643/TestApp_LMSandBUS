using CsvHelper;
using HolidayJp; 
using IniParser;
using Json.Net;
using Markdig;
using Markdig.Wpf;
using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Python.Runtime;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using Windows.ApplicationModel.UserDataTasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.PointOfService;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace TestApp
{
    public partial class Form1 : Form
    {
        Form2 form2 = null;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //設定ファイル読み込み
            var parser = new FileIniDataParser();
            IniParser.Model.IniData data = new IniParser.Model.IniData();
            IniParser.Model.IniData data2 = new IniParser.Model.IniData();
            try
            {
                data = parser.ReadFile("Config.ini");
                data2 = parser.ReadFile("idData.ini");
            }
            catch (Exception f)
            {
                Trace.TraceError("ファイルが見つかりませんでしたが続行します。");
            }

            StartPage = int.Parse(data["General"]["StartTab"]);
            Settings = data;

            tabControl1.SelectedIndex = StartPage;

            //Background操作起動処理

            BrowserPath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
            if (data["Browser"]["BrowserPath"] != null)
            {
                BrowserPath = data["Browser"]["BrowserPath"];
            }
            textBox1.Text = BrowserPath;
            if (data2["Main"]["ID"] != null)
            {
                textBox3.Text = data2["Main"]["ID"];
            }
            if (data2["Main"]["Pass"] != null)
            {
                textBox4.Text = data2["Main"]["Pass"];
            }


            await webView21.EnsureCoreWebView2Async();
            Action act = BackgroundTask;
            Task task = new Task(act);
            try
            {
                task.Start();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.SelectedIndex = 0; // 先頭の項目を選択

                data1_enable = 0;
                data2_enable = 0;
                data3_enable = 0;
                data4_enable = 0;


            }
            catch (ArgumentOutOfRangeException exp)
            {
                throw new ArgumentOutOfRangeException(nameof(exp), exp, null);
            }





            // 非同期でループを開始
            await Task.Run(() =>
            {
                while (true) // 無限ループ
                {
                    // 時間のかかる計算やデータ取得を想定


                    // UIスレッドでコントロールを安全に更新
                    this.Invoke((MethodInvoker)delegate
                    {

                        if (DateTime.Now.Second == 0)
                            this.BackgroundTask();
                    });

                    // 更新間隔（1秒）
                    Thread.Sleep(1000);
                }
            });

        }
        public struct Progress
        {
            public int Process1;
            public int Process2;
            public int Process3;
            public int Total;
        }

        public static int data1_enable;
        public static int data2_enable;
        public static int data3_enable;
        public static int data4_enable;

        public static int StartPage;
        public static IniParser.Model.IniData Settings;

        public AudioFileReader[] push = new AudioFileReader[3];
        public bool stop = true;

        public static string PinnedFile = "file:///C:/Users/琢真/Downloads/2026_教職化学2.pdf";

        public static bool isAllowed;


        public string BrowserPath;

        public void UpdateProgressbar()
        {
            //progressBar1.Update();
        }


        public int nowvalue;
        public int a;
        public int b;
        public int c;
        public double LastResult;
        public int progressPoint;

        //public void Set(Setting data)

        public string? path;


        public enum TabPageIndex
        {
            None = 0,
            Mode1 = 1,
            Mode2 = 2,
            Multi = 3
        }

        public int Placeindex;

        //private double DegreeToRadian(double angle)
        //{
        //    return Math.PI * angle / 180.0;
        //}

        //private double RadianToDegree(double angle)
        //{
        //    return angle * (180.0 / Math.PI);
        //}

        #region 時刻表
        public static Lists[] Buslist = new Lists[400];
        /// <summary>
        /// バスダイヤの定義用関数
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Hour"></param>
        /// <param name="Minutes"></param>
        /// <param name="IsHoliday"></param>
        /// <param name="routeID"></param>
        /// <param name="PlaceID"></param>
        /// <param name="終電情報"></param>
        /// <param name="unknown"></param>
        /// <param name="TO"></param>
        /// <param name="isSkippedDuringInterval"></param>
        public struct Details(int ID, int Hour, int Minutes, bool IsHoliday, int routeID, int PlaceID, int 終電情報, int unknown, int TO, bool isSkippedDuringInterval = false)
        {
            public int ID = ID;　//<系統(2-3桁)><土休日か(1桁)><index(3桁)>で指定
            public int Hour = Hour;
            public int Minutes = Minutes;
            public bool isHoliday = IsHoliday;
            public int RouteID = routeID; //93,94,96/53,97,99が主な対象。ナンバリングなしは別に定義する
            public int PlaceID = PlaceID; //0: 鈴見台2丁目、1: 鈴見町 以降適宜定義する
            public int last = 終電情報;
            public int unknown = unknown;
            public int TO = TO; //0=金大 1=朝霧台(92用) 2=若松
            public int checksum = Hour * 60 + Minutes;
            public bool isSkip = isSkippedDuringInterval; //学期休み運休かどうかを定義(デフォルトはfalse)
            //public bool isforStation = isforStation;
        }

        /// <summary>
        /// バス時刻表リスト
        /// </summary>
        /// <param name="a"></param>
        /// <param name="ID"></param>
        public struct Lists(List<Details> a, int ID)
        {
            public List<Details> Details = a;
            public int ID = ID;
        }
        /// <summary>
        /// 行き先の定義部分　返り値: (string)text
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string Boundfor(int index)
        {
            string str = "";
            switch (index)
            {
                case 0:
                    str = "金沢大学(角間)";
                    break;
                case 1:
                    str = "朝霧台";
                    break;
                case 2:
                    str = "若松";
                    break;
                //ここから金大用
                case 52:
                    str = "金沢駅";
                    break;
                case 53:
                    str = "西金沢";
                    break;
                case 54:
                    str = "東金沢駅";
                    break;
                case 55:
                    str = "田井町";
                    break;
                case 56:
                    str = "香林坊";
                    break;
                default:
                    str = "";
                    break;
            }

            return str;
        }
        /// <summary>
        /// 終バス情報。返り値: (string)text
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string LastInfo(int index)
        {
            string str = "";
            switch (index)
            {
                case 0:
                    str = "なし";
                    break;
                case 1: //16:22 - 99
                    str = "鈴見町と、鈴見台1丁目より先の各停留所";
                    break;
                case 2: //19:25 - 53
                    str = "丸の内、合同庁舎前と、香林坊より先の各停留所";
                    break;
                case 3: //20:27 - 93
                    str = "若谷、鈴見台2丁目、鈴見台1丁目";
                    break;
                case 4: //真の終バス
                    str = "全停留所";
                    break;
                case 5: //終了
                    str = "本日の運行はすべて終了しました。";
                    break;
                default://日中
                    str = "なし";
                    break;
            }

            return str;
        }
        public int[,] sHour = new int[3, 4];
        public int[,] sMinute = new int[3, 4];
        public int[,] sLabel2 = new int[3, 4];
        public int[] nHour = new int[4];
        public int[] nMinute = new int[4];
        public static int DebugCode = 0;
        public static int IsReaded = 0;
        public static int[] IsAlreadyLocked = new int[4];
        public int DebugMode = 0; //シミュモード
        public int[] DefaultTime = new int[2];
        public int isCanceled = 0; //運転中止指令
        public void BackgroundTask()
        {
            AntiThreat(BrowserPath);
            SetLMSData();
            CreateReadme();
            SetBuslist();
            int place = 0;
            bool isHoliday = (GetNowDayofWeek() == "Saturday" || GetNowDayofWeek() == "Sunday" || GetHoliday_jp() == true) ? true : false;
            int[] Hours = new int[3];
            int[] Minutes = new int[3];
            int NowHour = DateTime.Now.Hour;
            int NowMinutes = DateTime.Now.Minute;
            NowHour = 12;
            NowMinutes = 40; //デバッグ用
            int[] Label = new int[3];
            int place2 = Placeindex;
            int[,] Hour = new int[3, 4];
            int[,] Minute = new int[3, 4];
            int[,] Label2 = new int[3, 4];


            int nowMonth = DateTime.Now.Month;
            int nowDay = DateTime.Now.Day;

            int[,] SpringInterval = new int[2, 2] { { 2, 13 }, { 3, 31 } }; //なんでか使わなかった

            var IsDebugMode = DebugMode;
            var Canceled = isCanceled;

            for (int i = 0; i < 4; i++)
            {
                DefaultTime[0] = DateTime.Now.Hour;
                DefaultTime[1] = DateTime.Now.Minute;



                IsDebugMode = 0;

                if (IsReaded == 0 || IsAlreadyLocked[i] == 0)
                {
                    nHour[i] = DateTime.Now.Hour;
                    nMinute[i] = DateTime.Now.Minute;
                }
                if (IsDebugMode == 1)
                {
                    DefaultTime[0] = 24;
                    DefaultTime[1] = 37;

                    Canceled = 0;
                    isHoliday = false;

                    nHour[0] = 20;
                    nMinute[0] = 32;
                    nHour[1] = 21;
                    nMinute[1] = 35;
                    nHour[2] = 20;
                    nMinute[2] = 00;
                    nHour[3] = 16;
                    nMinute[3] = 30;
                }
            }
            int padding = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Hour[i, j] = -1;
                    Minute[i, j] = -1;
                    Label2[i, j] = -1;
                }
            }
            if (NowHour >= 2)
            {

                int counter = 0;
                DebugCode = 1;
                //ここから
                bool isIgnored = false;
                switch (place2)
                {

                    case 0: //上
                        padding = 0;
                        break;
                    case 1: //中央
                        padding = 1;
                        break;
                    case 2: //自然研前
                        padding = 4;
                        isIgnored = true; //10時以前は通過
                        break;
                }
                for (int j = 0; j < 4; j++)
                {
                    NowHour = nHour[j];
                    NowMinutes = nMinute[j];

                    int counter2 = 0;
                    int data = 0;
                    //系統番号(暫定)
                    switch (j)
                    {
                        case 0:
                            data = 93;
                            break;
                        case 1:
                            data = 94;
                            break;
                        case 2:
                            data = 53;
                            break;
                        case 3:
                            data = 99;
                            break;
                    }
                    if (isHoliday) data += 200;
                    //判定
                    if (data != 299)
                    {
                        for (int i = 0; i < Buslist[data].Details.Count; i++)
                        {
                            int 判定中の時 = Buslist[data].Details[i].Hour;
                            int 判定中の分 = Buslist[data].Details[i].Minutes + padding; //調整

                            DebugCode = 2;
                            //繰り上げ処理
                            if (判定中の分 >= 60)
                            {
                                判定中の時 -= 60;
                                判定中の時++;
                            }
                            int ignoredminutes = isHoliday ? 5 : 0;
                            if ((isIgnored && (Buslist[data].Details[i].Hour < 10 || (Buslist[data].Details[i].Hour == 10 && Buslist[data].Details[i].Minutes <= ignoredminutes)))
                                || (Buslist[data].Details[i].isSkip == true && (nowMonth == 2 && nowDay >= 13 || nowMonth == 3))
                                                                                                                                                                                        /*|| (j == 0 && data1_enable == 1)
                                                                                                                                                                                          || (j == 1 && data2_enable == 1)
                                                                                                                                                                                          || (j == 2 && data3_enable == 1)
                                                                                                                                                                                          || (j == 3 && data4_enable == 1)*/)
                            {
                                //判定対象外。何もしない
                                DebugCode = 4;
                            }
                            //同時間帯
                            else if (判定中の時 == NowHour)
                            {
                                if (判定中の分 >= NowMinutes)
                                {
                                    //溜まるまで表示用リストに代入
                                    if (counter2 < 3)
                                    {
                                        Hour[counter2, j] = 判定中の時;
                                        Minute[counter2, j] = 判定中の分;
                                        Label2[counter2, j] = Buslist[data].Details[i].TO;
                                        sHour[counter2, j] = Hour[counter2, j];
                                        sMinute[counter2, j] = Minute[counter2, j];
                                        sLabel2[counter2, j] = Label2[counter2, j];
                                        DebugCode = 3;
                                        counter2++;


                                    }

                                }
                            }
                            //異なる時間帯
                            else if (判定中の時 > NowHour)
                            {
                                if (counter2 < 3)
                                {
                                    Hour[counter2, j] = 判定中の時;
                                    Minute[counter2, j] = 判定中の分;
                                    Label2[counter2, j] = Buslist[data].Details[i].TO;
                                    sHour[counter2, j] = Hour[counter2, j];
                                    sMinute[counter2, j] = Minute[counter2, j];
                                    sLabel2[counter2, j] = Label2[counter2, j];
                                    counter2++;


                                }
                            }

                        }
                    }
                }


            }



            //表示初期化
            label32.Text = "";
            label33.Text = "";
            label34.Text = "";
            label35.Text = "";
            label36.Text = "";
            label37.Text = "";
            label38.Text = "";
            label39.Text = "";
            label40.Text = "";
            label41.Text = "";
            label42.Text = "";
            label43.Text = "";
            label44.Text = "";
            label45.Text = "";
            label46.Text = "";
            label47.Text = "";
            label48.Text = "";
            label49.Text = "";
            label54.Text = "";
            label55.Text = "";
            label56.Text = "";
            label57.Text = "";
            label58.Text = "";
            label59.Text = "";
            label61.Text = "";

            int dt = DefaultTime[0];
            if (DefaultTime[0] >= 24)
            {
                dt -= 24;
            }
            label74.Text = DefaultTime[1].ToString("00");
            label75.Text = dt.ToString("00");
            //代入

            if (Hour[0, 0] != -1 && Minute[0, 0] != -1)
            {
                label32.Text = sHour[0, 0].ToString("00") + ":" + sMinute[0, 0].ToString("00");
                label35.Text = Boundfor(sLabel2[0, 0]);
                label32.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[0, 0], sMinute[0, 0]));
                if (Delay >= 5)
                {
                    label32.Text = DelayProceedure(Delay);
                    label32.ForeColor = Color.Tomato;
                }
            }
            if (Hour[1, 0] != -1 && Minute[1, 0] != -1)
            {
                label33.Text = sHour[1, 0].ToString("00") + ":" + sMinute[1, 0].ToString("00");
                label36.Text = Boundfor(sLabel2[1, 0]);
                label33.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[1, 0], sMinute[1, 0]));
                if (Delay >= 5)
                {
                    label33.Text = DelayProceedure(Delay);
                    label33.ForeColor = Color.Tomato;
                }
            }
            if (Hour[2, 0] != -1 && Minute[2, 0] != -1)
            {
                label34.Text = sHour[2, 0].ToString("00") + ":" + sMinute[2, 0].ToString("00");
                label37.Text = Boundfor(sLabel2[2, 0]);
                label34.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[2, 0], sMinute[2, 0]));
                if (Delay >= 5)
                {
                    label34.Text = DelayProceedure(Delay);
                    label34.ForeColor = Color.Tomato;
                }
            }
            if (Hour[0, 1] != -1 && Minute[0, 1] != -1)
            {
                label38.Text = sHour[0, 1].ToString("00") + ":" + sMinute[0, 1].ToString("00");
                label41.Text = Boundfor(sLabel2[0, 1]);
                label38.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[0, 1], sMinute[0, 1]));
                if (Delay >= 5)
                {
                    label38.Text = DelayProceedure(Delay);
                    label38.ForeColor = Color.Tomato;
                }
            }
            if (Hour[1, 1] != -1 && Minute[1, 1] != -1)
            {
                label39.Text = sHour[1, 1].ToString("00") + ":" + sMinute[1, 1].ToString("00");
                label42.Text = Boundfor(sLabel2[1, 1]);
                label39.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[1, 1], sMinute[1, 1]));
                if (Delay >= 5)
                {
                    label39.Text = DelayProceedure(Delay);
                    label39.ForeColor = Color.Tomato;
                }
            }
            if (Hour[2, 1] != -1 && Minute[2, 1] != -1)
            {
                label40.Text = sHour[2, 1].ToString("00") + ":" + sMinute[2, 1].ToString("00");
                label43.Text = Boundfor(sLabel2[2, 1]);
                label40.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[2, 1], sMinute[2, 1]));
                if (Delay >= 5)
                {
                    label40.Text = DelayProceedure(Delay);
                    label40.ForeColor = Color.Tomato;
                }
            }
            if (Hour[0, 2] != -1 && Minute[0, 2] != -1)
            {
                label44.Text = sHour[0, 2].ToString("00") + ":" + sMinute[0, 2].ToString("00");
                label47.Text = Boundfor(sLabel2[0, 2]);
                label44.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[0, 2], sMinute[0, 2]));
                if (Delay >= 5)
                {
                    label44.Text = DelayProceedure(Delay);
                    label44.ForeColor = Color.Tomato;
                }
            }
            if (Hour[1, 2] != -1 && Minute[1, 2] != -1)
            {
                label45.Text = sHour[1, 2].ToString("00") + ":" + sMinute[1, 2].ToString("00");
                label48.Text = Boundfor(sLabel2[1, 2]);
                label45.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[1, 2], sMinute[1, 2]));
                if (Delay >= 5)
                {
                    label45.Text = DelayProceedure(Delay);
                    label45.ForeColor = Color.Tomato;
                }
            }
            if (Hour[2, 2] != -1 && Minute[2, 2] != -1)
            {
                label46.Text = sHour[2, 2].ToString("00") + ":" + sMinute[2, 2].ToString("00");
                label49.Text = Boundfor(sLabel2[2, 2]);
                label46.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[2, 2], sMinute[2, 2]));
                if (Delay >= 5)
                {
                    label46.Text = DelayProceedure(Delay);
                    label46.ForeColor = Color.Tomato;
                }
            }
            if (Hour[0, 3] != -1 && Minute[0, 3] != -1)
            {
                label54.Text = sHour[0, 3].ToString("00") + ":" + sMinute[0, 3].ToString("00");
                label57.Text = Boundfor(sLabel2[0, 3]);
                label54.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[0, 3], sMinute[0, 3]));
                if (Delay >= 5)
                {
                    label54.Text = DelayProceedure(Delay);
                    label54.ForeColor = Color.Tomato;
                }
            }
            if (Hour[1, 3] != -1 && Minute[1, 3] != -1)
            {
                label55.Text = sHour[1, 3].ToString("00") + ":" + sMinute[1, 3].ToString("00");
                label58.Text = Boundfor(sLabel2[1, 3]);
                label55.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[1, 3], sMinute[2, 3]));
                if (Delay >= 5)
                {
                    label55.Text = DelayProceedure(Delay);
                    label55.ForeColor = Color.Tomato;
                }
            }
            if (Hour[2, 3] != -1 && Minute[2, 3] != -1)
            {
                label56.Text = sHour[2, 3].ToString("00") + ":" + sMinute[2, 3].ToString("00");
                label59.Text = Boundfor(sLabel2[2, 3]);
                label56.ForeColor = Color.Orange;
                int Delay = (TimeUnify(DefaultTime[0], DefaultTime[1]) - TimeUnify(sHour[2, 3], sMinute[2, 3]));
                if (Delay >= 5)
                {
                    label56.Text = DelayProceedure(Delay);
                    label56.ForeColor = Color.Tomato;
                }
                DebugCode = 5;
            }
            int data2 = 0;
            //終電情報判定
            if (!isHoliday)
            {
                if (NowHour * 60 + NowMinutes > TimeUnify(15, 52 + padding))
                {
                    if (NowHour * 60 + NowMinutes > TimeUnify(18, 55 + padding))
                    {
                        if (NowHour * 60 + NowMinutes > TimeUnify(19, 57 + padding))
                        {
                            if (NowHour * 60 + NowMinutes > TimeUnify(21, 00 + padding))
                            {
                                if (NowHour * 60 + NowMinutes > TimeUnify(21, 30 + padding))
                                {
                                    data2 = 5;
                                }
                                else
                                {
                                    data2 = 4;
                                }

                            }
                            else if (NowHour * 60 + NowMinutes < TimeUnify(19, 57 + padding) + 30)
                            {
                                data2 = 3;
                            }
                        }
                        else if (NowHour * 60 + NowMinutes < TimeUnify(18, 55 + padding) + 30)
                        {
                            data2 = 2;
                        }
                    }
                    else if (NowHour * 60 + NowMinutes < TimeUnify(15, 52 + padding) + 30)
                    {
                        data2 = 1;
                    }


                }
            }

            //label61.Text = LastInfo(data2);
            if (Canceled == 0)
            {
                //最終ランプ判定・初期化
                if (label32.Text == "")
                    label77.Text = "終了";
                else if ((label33.Text != "" || label34.Text != "") || label32.Text == "")
                    label77.Text = "";
                else
                    label77.Text = "最終";
                if (label38.Text == "")
                    label78.Text = "終了";
                else if ((label39.Text != "" || label40.Text != "") || label38.Text == "")
                    label78.Text = "";
                else
                    label78.Text = "最終";
                if (label44.Text == "")
                    label79.Text = "終了";
                else if ((label45.Text != "" || label46.Text != "") || label44.Text == "")
                    label79.Text = "";
                else
                    label79.Text = "最終";
                if (label54.Text == "")
                    label80.Text = "終了";
                else if ((label55.Text != "" || label56.Text != "") || label54.Text == "")
                    label80.Text = "";
                else
                    label80.Text = "最終";
            }
            DebugCode = 6;
            IsReaded = 1;

            if (IsDebugMode == 1)
            {
                label61.Text = "シミュレーションモードが有効です。";
            }
            if (Canceled == 1)
            {
                label35.Text = "";
                label36.Text = "";
                label37.Text = "";
                label41.Text = "";
                label42.Text = "";
                label43.Text = "";
                label47.Text = "";
                label48.Text = "";
                label49.Text = "";
                label57.Text = "";
                label58.Text = "";
                label59.Text = "";
                label77.Text = "";
                label78.Text = "";
                label79.Text = "";
                label80.Text = "";
            }
            tabPage6.Update();

            //出力

            //終了
        }
        /// <summary>
        /// バス時刻表の定義部分。ダイヤ改正の際はここを変更。
        /// </summary>
        public void SetBuslist()
        {


            #region 上り
            //上りダイヤ・平日
            //93系統
            Buslist[93].Details = new List<Details>();
            Buslist[93].Details.Add(new Details(930501, 08, 25, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930502, 08, 32, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930503, 08, 35, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930504, 08, 51, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930505, 10, 00, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930506, 10, 42, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930507, 14, 13, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930508, 14, 53, false, 93, 0, 0, 0, 52, true));
            Buslist[93].Details.Add(new Details(930509, 15, 43, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930510, 16, 19, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930511, 16, 35, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930512, 17, 33, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930513, 18, 20, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930514, 18, 39, false, 93, 0, 0, 0, 52));
            Buslist[93].Details.Add(new Details(930515, 20, 27, false, 93, 0, 0, 0, 52));

            //94系統・田井町止め・若松止め
            Buslist[94].Details = new List<Details>();
            Buslist[94].Details.Add(new Details(940501, 08, 43, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940502, 09, 35, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940503, 10, 24, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940504, 11, 40, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940505, 12, 05, false, 94, 0, 0, 0, 55, true));
            Buslist[94].Details.Add(new Details(940506, 12, 15, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940507, 12, 38, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940528, 12, 50, false, 94, 0, 0, 0, 2, true));
            Buslist[94].Details.Add(new Details(940509, 13, 00, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940510, 13, 10, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940511, 13, 57, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940512, 14, 40, false, 94, 0, 0, 0, 55, true));
            Buslist[94].Details.Add(new Details(940513, 14, 43, false, 94, 0, 0, 0, 52, true));
            Buslist[94].Details.Add(new Details(940514, 15, 00, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940515, 16, 25, false, 94, 0, 0, 0, 55, true));
            Buslist[94].Details.Add(new Details(940516, 16, 43, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940517, 17, 00, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940518, 17, 15, false, 94, 0, 0, 0, 55, true));
            Buslist[94].Details.Add(new Details(940519, 18, 07, false, 94, 0, 0, 0, 2, true));
            Buslist[94].Details.Add(new Details(940520, 18, 15, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940521, 18, 25, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940522, 18, 35, false, 94, 0, 0, 0, 2, true));
            Buslist[94].Details.Add(new Details(940523, 19, 30, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940524, 19, 40, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940525, 19, 50, false, 94, 0, 0, 0, 55, true));
            Buslist[94].Details.Add(new Details(940526, 20, 15, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940527, 20, 20, false, 94, 0, 0, 0, 2, true));
            Buslist[94].Details.Add(new Details(940528, 20, 35, false, 94, 0, 0, 0, 52));
            Buslist[94].Details.Add(new Details(940529, 20, 49, false, 94, 0, 0, 0, 52));
            //Buslist[94].Details.Add(new Details(940528, 21, 05, false, 94, 0, 0, 0, 2, true));
            Buslist[94].Details.Add(new Details(940530, 21, 20, false, 94, 0, 0, 0, 55));
            Buslist[94].Details.Add(new Details(940531, 21, 30, false, 94, 0, 0, 0, 52));

            //53系統・96系統香林坊止め
            Buslist[53].Details = new List<Details>();
            Buslist[53].Details.Add(new Details(530501, 08, 00, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530502, 08, 40, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530503, 09, 05, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530504, 09, 45, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530505, 10, 10, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530506, 10, 40, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530507, 11, 00, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530508, 11, 30, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530509, 12, 20, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530510, 13, 30, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530511, 13, 45, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530512, 14, 48, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530513, 15, 20, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530514, 15, 48, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530515, 16, 30, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530516, 17, 40, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530517, 18, 10, false, 53, 0, 0, 0, 53));
            Buslist[53].Details.Add(new Details(530518, 18, 55, false, 96, 0, 0, 0, 56));
            Buslist[53].Details.Add(new Details(530519, 19, 20, false, 53, 0, 0, 0, 53));

            //99系統
            Buslist[99].Details = new List<Details>();
            Buslist[99].Details.Add(new Details(990501, 12, 10, false, 99, 0, 0, 0, 54));
            Buslist[99].Details.Add(new Details(990502, 16, 22, false, 99, 0, 0, 0, 54));
            #endregion

            #region 上り休日

            //上りダイヤ・休日
            //93系統
            Buslist[293].Details = new List<Details>();
            Buslist[293].Details.Add(new Details(931501, 08, 40, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 10, 40, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931503, 11, 25, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931504, 12, 39, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931505, 13, 50, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 14, 40, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 15, 25, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 16, 12, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 17, 45, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 18, 43, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 19, 40, false, 93, 0, 0, 0, 52));
            Buslist[293].Details.Add(new Details(931502, 20, 40, false, 93, 0, 0, 0, 52));

            //94系統
            Buslist[294].Details = new List<Details>();
            Buslist[294].Details.Add(new Details(941501, 10, 05, false, 94, 0, 0, 0, 52));
            Buslist[294].Details.Add(new Details(941502, 11, 55, false, 94, 0, 0, 0, 52));
            Buslist[294].Details.Add(new Details(941503, 17, 10, false, 94, 0, 0, 0, 52));
            Buslist[294].Details.Add(new Details(941501, 17, 30, false, 94, 0, 0, 0, 52));
            Buslist[294].Details.Add(new Details(941501, 18, 53, false, 94, 0, 0, 0, 52));

            //96系統
            Buslist[253].Details = new List<Details>();
            Buslist[253].Details.Add(new Details(531501, 08, 30, false, 53, 0, 0, 0, 53));
            Buslist[253].Details.Add(new Details(531502, 10, 30, false, 53, 0, 0, 0, 53));
            Buslist[253].Details.Add(new Details(531503, 12, 40, false, 53, 0, 0, 0, 53));
            Buslist[253].Details.Add(new Details(531504, 14, 30, false, 53, 0, 0, 0, 53));
            Buslist[253].Details.Add(new Details(531505, 16, 30, false, 53, 0, 0, 0, 53));
            Buslist[253].Details.Add(new Details(531506, 19, 00, false, 53, 0, 0, 0, 53));

            //99系統はない
            //Buslist[299].Details = new List<Details>();

            #endregion

        }
        /// <summary>
        /// 時間判定用。返り値: (int)(Hours * 60 + Minutes)
        /// </summary>
        /// <param name="Hour">ここに時間を入力。</param>
        /// <param name="Minutes">ここに分を入力。</param>
        /// <returns></returns>
        public static int TimeUnify(int Hour, int Minutes)
        {
            int data = Hour * 60 + Minutes;
            return data;
        }

        public static string GetNowDayofWeek()
        {
            var theDay = DateTime.Now; //現在の日付
            var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US"); //英語 - アメリカ
            string str = theDay.ToString("dddd", culture);//曜日取得
            return str;
        }

        public static bool GetHoliday_jp()
        {
            bool b = HolidayJp.HolidayJp.IsHoliday(DateTime.Now); //HolidayJpより祝日かどうかを判定
            return b;
        }

        public string DelayProceedure(int Delay)
        {
            string str = "";
            if (Delay < 10)
                str = "+  " + (Delay).ToString() + "m";
            else if (Delay < 100)
                str = "+ " + (Delay).ToString() + "m";
            else
            {
                str = "+" + (Delay).ToString() + "m";
            }
            return str;
        }

        //ここから開発中の機能
        public void SetBusDataFromTXT(string txt)
        {
            //分割処理
            string[] rawLists;
            rawLists = txt.Split("\n");
            int[] counter = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < txt.Length; i++)
            {
                try
                {
                    string[] rawList;
                    rawList = rawLists[i].Split(" ");
                    int newID = 939000 + counter[0];
                    //内部代入
                    if (i == 0)
                    {
                        //初めての処理
                        Buslist[101].Details = new List<Details>();
                        Buslist[102].Details = new List<Details>();
                        Buslist[103].Details = new List<Details>();
                        Buslist[104].Details = new List<Details>();
                    }
                    int[] ListHour = new int[4];
                    int[] ListMinute = new int[4];
                    int[] ListLine = new int[4];
                    int[] ListBoundFor = new int[4];
                    for (int j = 0; j < 4; j++)
                    {
                        //値を-1にすると処理をスキップできるようif文を用意
                        if (Convert.ToInt32(rawList[j * 4]) != -1)
                            ListHour[j] = Convert.ToInt32(rawList[j * 4]);
                        if (Convert.ToInt32(rawList[j * 4 + 1]) != -1)
                            ListMinute[j] = Convert.ToInt32(rawList[j * 4 + 1]);
                        if (Convert.ToInt32(rawList[j * 4 + 2]) == -1)
                            ListLine[j] = Convert.ToInt32(rawList[j * 4 + 2]);
                        if (Convert.ToInt32(rawList[j * 4 + 3]) == -1)
                            ListBoundFor[j] = Convert.ToInt32(rawList[j * 4 + 3]);
                    }

                }
                catch (Exception e)
                {
                    //throw new Exception();
                }

            }
        }
        /*
        超簡易的時刻表 - 金沢大学 Ed. 現バージョン: ver. 3.0.d4
        使用した技術など
        ・await task.Run(() => { while (true) { (コード) } });
        　→無限ループコード。(コード内で)秒数==0で更新関数を呼び出し。
        ・HolidayJp
        　→日本の祝日をインポートするNuGetパッケージ。HolidayJp.HolidayJp.IsHoliday(日時)で判定。
        
        バージョン履歴
        v1.0.00 実装
        v1.0.01 元のラベルテキストが残る不具合を修正。
        v1.0.02 金大にある3つの停留所に対応。
        v1.1.00 朝4時にならないとその日の時刻表が出ないようになったのと、終電情報が然るべきときに出るようにした。
        v1.2.00 1分ごとに更新するようになった。
        v2.0.β1 デザイン更新、学期休みダイヤ対応、最終表示仮実装。
        v2.0.00 休日ダイヤ対応（判定にはHolidayJpを採用）、その日の時刻表を出す時間を2時~に緩和。
        v2.0.01 UI改善、分ごとの更新に問題があったため修正。
        v3.0.d1 制御フォームの生成、Form1の変数とのリンク完了。
        v3.0.d2 制御システム仮完成
        v3.0.d3 仮置きアイコンを用意した。
        v3.0.d4 終電情報廃止、UI改善を実施。
                ダイヤ改正に合わせて、時刻表定義部分を変更:
            平日93: 増発:  8:32(52), 16:35(52)
            平日94: 増発: 12:50(05), 14:40(55)
            　　    変更: 16:55→17:00(52)
            　　    廃止: 21:05(05)
            平日53: 変更:  8:30→ 8:40(53)
            　　    　　  13:05→13:30(53)
            　　    　　  14:22→14:20(53)
            　　    　　  19:25→19:20(53)
            平日99: (変更なし)

            休日93: 変更: 13:45→13:50(52)
            　　    　　  14:35→14:40(52)
            休日94: (変更なし)
            休日53: 変更: 16:55→16:30(53)
            　　    　　  18:55→19:00(53)

        Yahoo!乗換案内がダイヤ改正に対応済みであることを確認し、照らし合わせて実装。


        今後の実装機能
        ・UI変更機能実装（余裕があれば）
        ・英語版実装（余裕があれば）
        ・遅延時制御の実装。これは別フォームで制御する

         */

        #endregion

        #region [LMSジャンパー]

        public struct LMSData(int WeekDay, int time, string title, string URL)
        {
            public int Weekday = WeekDay;
            public int Time = time;
            public string Title = title;
            public string URL = URL;
        }

        public static int[,,] attendstate = new int[7, 6, 2];
        public static LMSData[,] DataList = new LMSData[7, 6];
        public int returns = 0;
        public enum LoadingFailureIndicator
        {
            //LMSデータのロード状況
            False = 0,
            AutoTrue = 1,
            //手動操作用
            ManualTrue = 2,

        };
        public bool Notified = false;
        public LoadingFailureIndicator IsLoadFailure = LoadingFailureIndicator.False;

        public void SetLMSData()
        {
            SetLMSData(false, false);
        }
        public void SetLMSData(bool ExportIniOnly)
        {
            SetLMSData(false, ExportIniOnly);
        }

        public void SetLMSData(bool setSkip, bool ExportIniOnly)
        {
            if (!ExportIniOnly)
            {
                if (setSkip)
                {
                    Trace.TraceInformation("Jsonファイルから読み込みのため、定義はスキップします。");
                }
                else if (IsLoadFailure == LoadingFailureIndicator.False)
                {
                    //エラーで動作が止まる恐れがあるためtry-catchに
                    try
                    {
                        //データ読み込み

                        SetLMSDataFromFile();
                    }
                    catch (Exception e)
                    {
                        //エラータイプ: ロード時のエラー
                        IsLoadFailure = LoadingFailureIndicator.AutoTrue;
                        Notified = true;
                        MessageBox.Show("LMSデータ設定に失敗しました。内蔵データで置き換えます。\n" + e.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        SetLMSDataManually(IsLoadFailure);
                    }
                    if (IsLoadFailure == LoadingFailureIndicator.False && !Notified)
                    {
                        //成功時
                        //Notified = true;
                        //MessageBox.Show("LMSデータ設定を読み込みました。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                else
                {
                    if (!Notified)
                    {
                        //エラータイプ: 設定により意図的に作用させている
                        Notified = true;
                        MessageBox.Show("LMSデータ設定を内蔵データで行います。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    SetLMSDataManually(IsLoadFailure);

                }

                Setinidata();

                //反映部分
                linkLabel1.Text = DataList[0, 0].Title;
                linkLabel2.Text = DataList[0, 1].Title;
                linkLabel3.Text = DataList[0, 2].Title;
                linkLabel4.Text = DataList[0, 3].Title;
                linkLabel5.Text = DataList[0, 4].Title;
                linkLabel6.Text = DataList[0, 5].Title;
                linkLabel7.Text = DataList[1, 0].Title;
                linkLabel8.Text = DataList[1, 1].Title;
                linkLabel9.Text = DataList[1, 2].Title;
                linkLabel10.Text = DataList[1, 3].Title;
                linkLabel11.Text = DataList[1, 4].Title;
                linkLabel12.Text = DataList[1, 5].Title;
                linkLabel13.Text = DataList[2, 0].Title;
                linkLabel14.Text = DataList[2, 1].Title;
                linkLabel15.Text = DataList[2, 2].Title;
                linkLabel16.Text = DataList[2, 3].Title;
                linkLabel17.Text = DataList[2, 4].Title;
                linkLabel18.Text = DataList[2, 5].Title;
                linkLabel19.Text = DataList[3, 0].Title;
                linkLabel20.Text = DataList[3, 1].Title;
                linkLabel21.Text = DataList[3, 2].Title;
                linkLabel22.Text = DataList[3, 3].Title;
                linkLabel23.Text = DataList[3, 4].Title;
                linkLabel24.Text = DataList[3, 5].Title;
                linkLabel25.Text = DataList[4, 0].Title;
                linkLabel26.Text = DataList[4, 1].Title;
                linkLabel27.Text = DataList[4, 2].Title;
                linkLabel28.Text = DataList[4, 3].Title;
                linkLabel29.Text = DataList[4, 4].Title;
                linkLabel30.Text = DataList[4, 5].Title;
                linkLabel31.Text = DataList[5, 0].Title;
                linkLabel32.Text = DataList[5, 1].Title;
                linkLabel33.Text = DataList[5, 2].Title;
                linkLabel34.Text = DataList[5, 3].Title;
                linkLabel35.Text = DataList[5, 4].Title;
                linkLabel36.Text = DataList[5, 5].Title;
                linkLabel37.Text = DataList[6, 0].Title;
                linkLabel38.Text = DataList[6, 1].Title;
                linkLabel39.Text = DataList[6, 2].Title;
                linkLabel40.Text = DataList[6, 3].Title;
                linkLabel41.Text = DataList[6, 4].Title;
                linkLabel42.Text = DataList[6, 5].Title;

                SetToolTip();
            }
            else
            {
                Trace.TraceInformation("定義処理はスキップします。");
            }
            //Config.iniに書き込む準備
            IniParser.Model.IniData data = new IniParser.Model.IniData();
            if (Settings != null)
            {
                //設定がすでにある場合
                data = Settings;
            }



            data["General"]["StartTab"] = StartPage.ToString();
            //ラベル登録
            data["Monday"]["Label1"] = DataList[0, 0].Title.Replace("\n", "");
            data["Monday"]["Label2"] = DataList[0, 1].Title.Replace("\n", "");
            data["Monday"]["Label3"] = DataList[0, 2].Title.Replace("\n", "");
            data["Monday"]["Label4"] = DataList[0, 3].Title.Replace("\n", "");
            data["Monday"]["Label5"] = DataList[0, 4].Title.Replace("\n", "");
            data["Monday"]["Label6"] = DataList[0, 5].Title.Replace("\n", "");

            data["Tuesday"]["Label1"] = DataList[1, 0].Title.Replace("\n", "");
            data["Tuesday"]["Label2"] = DataList[1, 1].Title.Replace("\n", "");
            data["Tuesday"]["Label3"] = DataList[1, 2].Title.Replace("\n", "");
            data["Tuesday"]["Label4"] = DataList[1, 3].Title.Replace("\n", "");
            data["Tuesday"]["Label5"] = DataList[1, 4].Title.Replace("\n", "");
            data["Tuesday"]["Label6"] = DataList[1, 5].Title.Replace("\n", "");

            data["Wednesday"]["Label1"] = DataList[2, 0].Title.Replace("\n", "");
            data["Wednesday"]["Label2"] = DataList[2, 1].Title.Replace("\n", "");
            data["Wednesday"]["Label3"] = DataList[2, 2].Title.Replace("\n", "");
            data["Wednesday"]["Label4"] = DataList[2, 3].Title.Replace("\n", "");
            data["Wednesday"]["Label5"] = DataList[2, 4].Title.Replace("\n", "");
            data["Wednesday"]["Label6"] = DataList[2, 5].Title.Replace("\n", "");

            data["Thursday"]["Label1"] = DataList[3, 0].Title.Replace("\n", "");
            data["Thursday"]["Label2"] = DataList[3, 1].Title.Replace("\n", "");
            data["Thursday"]["Label3"] = DataList[3, 2].Title.Replace("\n", "");
            data["Thursday"]["Label4"] = DataList[3, 3].Title.Replace("\n", "");
            data["Thursday"]["Label5"] = DataList[3, 4].Title.Replace("\n", "");
            data["Thursday"]["Label6"] = DataList[3, 5].Title.Replace("\n", "");

            data["Friday"]["Label1"] = DataList[4, 0].Title.Replace("\n", "");
            data["Friday"]["Label2"] = DataList[4, 1].Title.Replace("\n", "");
            data["Friday"]["Label3"] = DataList[4, 2].Title.Replace("\n", "");
            data["Friday"]["Label4"] = DataList[4, 3].Title.Replace("\n", "");
            data["Friday"]["Label5"] = DataList[4, 4].Title.Replace("\n", "");
            data["Friday"]["Label6"] = DataList[4, 5].Title.Replace("\n", "");

            data["Saturday"]["Label1"] = DataList[5, 0].Title.Replace("\n", "");
            data["Saturday"]["Label2"] = DataList[5, 1].Title.Replace("\n", "");
            data["Saturday"]["Label3"] = DataList[5, 2].Title.Replace("\n", "");
            data["Saturday"]["Label4"] = DataList[5, 3].Title.Replace("\n", "");
            data["Saturday"]["Label5"] = DataList[5, 4].Title.Replace("\n", "");
            data["Saturday"]["Label6"] = DataList[5, 5].Title.Replace("\n", "");

            data["Other"]["Label1"] = DataList[6, 0].Title.Replace("\n", "");
            data["Other"]["Label2"] = DataList[6, 1].Title.Replace("\n", "");
            data["Other"]["Label3"] = DataList[6, 2].Title.Replace("\n", "");
            data["Other"]["Label4"] = DataList[6, 3].Title.Replace("\n", "");
            data["Other"]["Label5"] = DataList[6, 4].Title.Replace("\n", "");
            data["Other"]["Label6"] = DataList[6, 5].Title.Replace("\n", "");

            //リンク登録
            data["Monday"]["Link1"] = DataList[0, 0].URL;
            data["Monday"]["Link2"] = DataList[0, 1].URL;
            data["Monday"]["Link3"] = DataList[0, 2].URL;
            data["Monday"]["Link4"] = DataList[0, 3].URL;
            data["Monday"]["Link5"] = DataList[0, 4].URL;
            data["Monday"]["Link6"] = DataList[0, 5].URL;

            data["Tuesday"]["Link1"] = DataList[1, 0].URL;
            data["Tuesday"]["Link2"] = DataList[1, 1].URL;
            data["Tuesday"]["Link3"] = DataList[1, 2].URL;
            data["Tuesday"]["Link4"] = DataList[1, 3].URL;
            data["Tuesday"]["Link5"] = DataList[1, 4].URL;
            data["Tuesday"]["Link6"] = DataList[1, 5].URL;

            data["Wednesday"]["Link1"] = DataList[2, 0].URL;
            data["Wednesday"]["Link2"] = DataList[2, 1].URL;
            data["Wednesday"]["Link3"] = DataList[2, 2].URL;
            data["Wednesday"]["Link4"] = DataList[2, 3].URL;
            data["Wednesday"]["Link5"] = DataList[2, 4].URL;
            data["Wednesday"]["Link6"] = DataList[2, 5].URL;

            data["Thursday"]["Link1"] = DataList[3, 0].URL;
            data["Thursday"]["Link2"] = DataList[3, 1].URL;
            data["Thursday"]["Link3"] = DataList[3, 2].URL;
            data["Thursday"]["Link4"] = DataList[3, 3].URL;
            data["Thursday"]["Link5"] = DataList[3, 4].URL;
            data["Thursday"]["Link6"] = DataList[3, 5].URL;

            data["Friday"]["Link1"] = DataList[4, 0].URL;
            data["Friday"]["Link2"] = DataList[4, 1].URL;
            data["Friday"]["Link3"] = DataList[4, 2].URL;
            data["Friday"]["Link4"] = DataList[4, 3].URL;
            data["Friday"]["Link5"] = DataList[4, 4].URL;
            data["Friday"]["Link6"] = DataList[4, 5].URL;

            data["Saturday"]["Link1"] = DataList[5, 0].URL;
            data["Saturday"]["Link2"] = DataList[5, 1].URL;
            data["Saturday"]["Link3"] = DataList[5, 2].URL;
            data["Saturday"]["Link4"] = DataList[5, 3].URL;
            data["Saturday"]["Link5"] = DataList[5, 4].URL;
            data["Saturday"]["Link6"] = DataList[5, 5].URL;

            data["Other"]["Link1"] = DataList[6, 0].URL;
            data["Other"]["Link2"] = DataList[6, 1].URL;
            data["Other"]["Link3"] = DataList[6, 2].URL;
            data["Other"]["Link4"] = DataList[6, 3].URL;
            data["Other"]["Link5"] = DataList[6, 4].URL;
            data["Other"]["Link6"] = DataList[6, 5].URL;

            data["Browser"]["BrowserPath"] = BrowserPath;

            Settings = data;


            var parser = new FileIniDataParser();
            parser.WriteFile("Config.ini", Settings);


            tabPage1.Update();
        }

        public void SetLMSDataManually(LoadingFailureIndicator ExceptionCode)
        {
            //定義部分
            //月曜
            DataList[0, 0] = (new LMSData(0, 0, "AI入門", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010074G00110&systemType=1"));
            DataList[0, 1] = (new LMSData(0, 1, "教育の理念と歴史A", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26035291020000&systemType=1"));
            DataList[0, 2] = (new LMSData(0, 2, "物理学実験", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010075213010&systemType=1"));
            DataList[0, 3] = (new LMSData(0, 3, "物理学実験", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010075213010&systemType=1"));
            DataList[0, 4] = (new LMSData(0, 4, "物理学実験", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010075213010&systemType=1"));
            DataList[0, 5] = (new LMSData(0, 5, "教職地学", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015290007300&systemType=1"));
            //火曜
            DataList[1, 0] = (new LMSData(1, 0, "学域GS言語科目Ⅰ", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26035220101101&systemType=1"));
            DataList[1, 1] = (new LMSData(1, 1, "物理数学1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226019000&systemType=1"));
            DataList[1, 2] = (new LMSData(1, 2, "力学1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226011000&systemType=1"));
            DataList[1, 3] = (new LMSData(1, 3, "力学演習1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226013000&systemType=1"));
            DataList[1, 4] = (new LMSData(1, 4, "力学演習1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226013000&systemType=1"));
            DataList[1, 5] = (new LMSData(1, 5, "教育の制度と経営", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015290203000&systemType=1"));
            //水曜
            DataList[2, 0] = (new LMSData(2, 0, "電磁気学演習1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226017000&systemType=1"));
            DataList[2, 1] = (new LMSData(2, 1, "電磁気学演習1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226017000&systemType=1"));
            DataList[2, 2] = (new LMSData(2, 2, "計算科学序論1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226009000&systemType=1"));
            DataList[2, 3] = (new LMSData(2, 3, "(なし)", "-1"));
            DataList[2, 4] = (new LMSData(2, 4, "計算物理学a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226023000&systemType=1"));
            DataList[2, 5] = (new LMSData(2, 5, "(なし)", "-1"));
            //木曜
            DataList[3, 0] = (new LMSData(3, 0, "(なし)", "-1"));
            DataList[3, 1] = (new LMSData(3, 1, "エクササイズ＆\nスポーツ テニス\n(基礎)", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010072F0za12&systemType=1"));
            DataList[3, 2] = (new LMSData(3, 2, "(なし)", "-1"));
            DataList[3, 3] = (new LMSData(3, 3, "グローバル時代の\n社会学", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26010071C10a12&systemType=1"));
            DataList[3, 4] = (new LMSData(3, 4, "(なし)", "-1"));
            DataList[3, 5] = (new LMSData(3, 5, "(なし)", "-1"));
            //金曜
            DataList[4, 0] = (new LMSData(4, 0, "熱統計力学序論a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226021000&systemType=1"));
            DataList[4, 1] = (new LMSData(4, 1, "電磁気学1a", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015226015000&systemType=1"));
            DataList[4, 2] = (new LMSData(4, 2, "(なし)", "-1"));
            DataList[4, 3] = (new LMSData(4, 3, "(なし)", "-1"));
            DataList[4, 4] = (new LMSData(4, 4, "(なし)", "-1"));
            DataList[4, 5] = (new LMSData(4, 5, "教職化学", "https://acanthus.cis.kanazawa-u.ac.jp/base/lms-course/sso-link/?courseId=26015290005100&systemType=1"));
            //土曜
            DataList[5, 0] = (new LMSData(5, 0, "(なし)", "-1"));
            DataList[5, 1] = (new LMSData(5, 1, "(なし)", "-1"));
            DataList[5, 2] = (new LMSData(5, 2, "(なし)", "-1"));
            DataList[5, 3] = (new LMSData(5, 3, "(なし)", "-1"));
            DataList[5, 4] = (new LMSData(5, 4, "(なし)", "-1"));
            DataList[5, 5] = (new LMSData(5, 5, "(なし)", "-1"));
            //集中講義とか
            DataList[6, 0] = (new LMSData(6, 0, "(なし)", "-1"));
            DataList[6, 1] = (new LMSData(6, 1, "(なし)", "-1"));
            DataList[6, 2] = (new LMSData(6, 2, "(なし)", "-1"));
            DataList[6, 3] = (new LMSData(6, 3, "(なし)", "-1"));
            DataList[6, 4] = (new LMSData(6, 4, "(なし)", "-1"));
            DataList[6, 5] = (new LMSData(6, 5, "(なし)", "-1"));
        }
        /// <summary>
        /// 設定ファイルからLMSデータを読み込んでセットする関数。
        /// </summary>
        /// <param name="path"></param>
        public void SetLMSDataFromFile()
        {

            var data = Settings;

            for (int i = 0; i < 7; i++)
            {
                string Weekday = "";
                switch (i)
                {
                    case 0:
                        Weekday = "Monday";
                        break;
                    case 1:
                        Weekday = "Tuesday";
                        break;
                    case 2:
                        Weekday = "Wednesday";
                        break;
                    case 3:
                        Weekday = "Thursday";
                        break;
                    case 4:
                        Weekday = "Friday";
                        break;
                    case 5:
                        Weekday = "Saturday";
                        break;
                    case 6:
                        Weekday = "Other";
                        break;
                }
                for (int j = 0; j < 6; j++)
                {
                    string str1 = "";
                    string str2 = "";
                    switch (j)
                    {
                        case 0:
                            str1 = "Label1";
                            str2 = "Link1";
                            break;
                        case 1:
                            str1 = "Label2";
                            str2 = "Link2";
                            break;
                        case 2:
                            str1 = "Label3";
                            str2 = "Link3";
                            break;
                        case 3:
                            str1 = "Label4";
                            str2 = "Link4";
                            break;
                        case 4:
                            str1 = "Label5";
                            str2 = "Link5";
                            break;
                        case 5:
                            str1 = "Label6";
                            str2 = "Link6";
                            break;

                    }
                    DataList[i, j].Title = data[Weekday][str1];
                    DataList[i, j].Title = t改行する(DataList[i, j].Title);
                    DataList[i, j].URL = data[Weekday][str2];
                    DataList[i, j].Weekday = i;
                    DataList[i, j].Time = j;
                }
            }

        }

        public string t改行する(string str)
        {
            if (str.Length >= 10)
            {
                //整数型(int, long)どうしの計算をMath.Floor()中ですることができないので先に計算
                double Len = (str.Length) / 8;
                //操作位置をオーバーしてエラーになるので修正
                for (int k = 1; k < (int)Math.Floor(Len) + 1; k++)
                {
                    str = str.Insert(8 * k, "\n");
                }
            }
            return str;
        }
        public static void Setinidata()
        {

            var data = new IniParser.Model.IniData();

            data["General"]["StartTab"] = StartPage.ToString();
            //ラベル登録
            data["Monday"]["Label1"] = DataList[0, 0].Title;
            data["Monday"]["Label2"] = DataList[0, 1].Title;
            data["Monday"]["Label3"] = DataList[0, 2].Title;
            data["Monday"]["Label4"] = DataList[0, 3].Title;
            data["Monday"]["Label5"] = DataList[0, 4].Title;
            data["Monday"]["Label6"] = DataList[0, 5].Title;

            data["Tuesday"]["Label1"] = DataList[1, 0].Title;
            data["Tuesday"]["Label2"] = DataList[1, 1].Title;
            data["Tuesday"]["Label3"] = DataList[1, 2].Title;
            data["Tuesday"]["Label4"] = DataList[1, 3].Title;
            data["Tuesday"]["Label5"] = DataList[1, 4].Title;
            data["Tuesday"]["Label6"] = DataList[1, 5].Title;

            data["Wednesday"]["Label1"] = DataList[2, 0].Title;
            data["Wednesday"]["Label2"] = DataList[2, 1].Title;
            data["Wednesday"]["Label3"] = DataList[2, 2].Title;
            data["Wednesday"]["Label4"] = DataList[2, 3].Title;
            data["Wednesday"]["Label5"] = DataList[2, 4].Title;
            data["Wednesday"]["Label6"] = DataList[2, 5].Title;

            data["Thursday"]["Label1"] = DataList[3, 0].Title;
            data["Thursday"]["Label2"] = DataList[3, 1].Title;
            data["Thursday"]["Label3"] = DataList[3, 2].Title;
            data["Thursday"]["Label4"] = DataList[3, 3].Title;
            data["Thursday"]["Label5"] = DataList[3, 4].Title;
            data["Thursday"]["Label6"] = DataList[3, 5].Title;

            data["Friday"]["Label1"] = DataList[4, 0].Title;
            data["Friday"]["Label2"] = DataList[4, 1].Title;
            data["Friday"]["Label3"] = DataList[4, 2].Title;
            data["Friday"]["Label4"] = DataList[4, 3].Title;
            data["Friday"]["Label5"] = DataList[4, 4].Title;
            data["Friday"]["Label6"] = DataList[4, 5].Title;

            data["Saturday"]["Label1"] = DataList[5, 0].Title;
            data["Saturday"]["Label2"] = DataList[5, 1].Title;
            data["Saturday"]["Label3"] = DataList[5, 2].Title;
            data["Saturday"]["Label4"] = DataList[5, 3].Title;
            data["Saturday"]["Label5"] = DataList[5, 4].Title;
            data["Saturday"]["Label6"] = DataList[5, 5].Title;

            data["Other"]["Label1"] = DataList[6, 0].Title;
            data["Other"]["Label2"] = DataList[6, 1].Title;
            data["Other"]["Label3"] = DataList[6, 2].Title;
            data["Other"]["Label4"] = DataList[6, 3].Title;
            data["Other"]["Label5"] = DataList[6, 4].Title;
            data["Other"]["Label6"] = DataList[6, 5].Title;

            //リンク登録
            data["Monday"]["Link1"] = DataList[0, 0].URL;
            data["Monday"]["Link2"] = DataList[0, 1].URL;
            data["Monday"]["Link3"] = DataList[0, 2].URL;
            data["Monday"]["Link4"] = DataList[0, 3].URL;
            data["Monday"]["Link5"] = DataList[0, 4].URL;
            data["Monday"]["Link6"] = DataList[0, 5].URL;

            data["Tuesday"]["Link1"] = DataList[1, 0].URL;
            data["Tuesday"]["Link2"] = DataList[1, 1].URL;
            data["Tuesday"]["Link3"] = DataList[1, 2].URL;
            data["Tuesday"]["Link4"] = DataList[1, 3].URL;
            data["Tuesday"]["Link5"] = DataList[1, 4].URL;
            data["Tuesday"]["Link6"] = DataList[1, 5].URL;

            data["Wednesday"]["Link1"] = DataList[2, 0].URL;
            data["Wednesday"]["Link2"] = DataList[2, 1].URL;
            data["Wednesday"]["Link3"] = DataList[2, 2].URL;
            data["Wednesday"]["Link4"] = DataList[2, 3].URL;
            data["Wednesday"]["Link5"] = DataList[2, 4].URL;
            data["Wednesday"]["Link6"] = DataList[2, 5].URL;

            data["Thursday"]["Link1"] = DataList[3, 0].URL;
            data["Thursday"]["Link2"] = DataList[3, 1].URL;
            data["Thursday"]["Link3"] = DataList[3, 2].URL;
            data["Thursday"]["Link4"] = DataList[3, 3].URL;
            data["Thursday"]["Link5"] = DataList[3, 4].URL;
            data["Thursday"]["Link6"] = DataList[3, 5].URL;

            data["Friday"]["Link1"] = DataList[4, 0].URL;
            data["Friday"]["Link2"] = DataList[4, 1].URL;
            data["Friday"]["Link3"] = DataList[4, 2].URL;
            data["Friday"]["Link4"] = DataList[4, 3].URL;
            data["Friday"]["Link5"] = DataList[4, 4].URL;
            data["Friday"]["Link6"] = DataList[4, 5].URL;

            data["Saturday"]["Link1"] = DataList[5, 0].URL;
            data["Saturday"]["Link2"] = DataList[5, 1].URL;
            data["Saturday"]["Link3"] = DataList[5, 2].URL;
            data["Saturday"]["Link4"] = DataList[5, 3].URL;
            data["Saturday"]["Link5"] = DataList[5, 4].URL;
            data["Saturday"]["Link6"] = DataList[5, 5].URL;

            data["Other"]["Link1"] = DataList[6, 0].URL;
            data["Other"]["Link2"] = DataList[6, 1].URL;
            data["Other"]["Link3"] = DataList[6, 2].URL;
            data["Other"]["Link4"] = DataList[6, 3].URL;
            data["Other"]["Link5"] = DataList[6, 4].URL;
            data["Other"]["Link6"] = DataList[6, 5].URL;
        }
        public static int debug;
        public struct LMSDataJson(string day, int period, string title, string URL)
        {
            public string WeekDay = day;
            public int time = period;
            public string title = title;
            public string URL = URL;
        }
        /// <summary>
        /// #バイバイ金大LMS からエクスポートしたjsonからLMSデータの抽出・設定を行う。
        /// </summary>
        public void SetLMSDataFromjson()
        {
            //ファイル選択ダイアログ
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "goodByeLMS_allLectureData.json";
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ofd.InitialDirectory = System.IO.Path.Combine(userPath + "\\", @"Downloads");
            ofd.Filter = "JSONファイル|*.json|すべて|*.*";
            ofd.Title = "ファイルを選択";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Stream stream = ofd.OpenFile();
                if (stream != null)
                {
                    try
                    {
                        //開き処理
                        StreamReader sr = new StreamReader(stream);
                        string str = sr.ReadToEnd();
                        //Trace.TraceInformation(str);
                        JToken entries = JToken.Parse(str);
                        for (int i = 0; i < 7; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                DataList[i, j] = new LMSData(i, j, "(なし)", "-1");
                            }
                        }
                        for (int i = 0; i < entries[0]["entries"].Count(); i++)
                        {
                            debug = entries[0]["entries"].Count();
                            Trace.TraceInformation("処理開始: " + entries[0]["entries"][i]["subjectName"].ToString());
                            string day = entries[0]["entries"][i]["day"].ToString();
                            int dayInt32 = 0;
                            switch (day)
                            {
                                case "月":
                                    dayInt32 = 0;
                                    break;
                                case "火":
                                    dayInt32 = 1;
                                    break;
                                case "水":
                                    dayInt32 = 2;
                                    break;
                                case "木":
                                    dayInt32 = 3;
                                    break;
                                case "金":
                                    dayInt32 = 4;
                                    break;
                                case "土":
                                    dayInt32 = 5;
                                    break;
                                default:
                                    dayInt32 = 6;
                                    break;

                            }
                            int Period = Int32.Parse(entries[0]["entries"][i]["period"].ToString()) - 1;
                            string Label = entries[0]["entries"][i]["subjectName"].ToString();
                            Label = Label.Replace("★", "");
                            Label = t改行する(Label);
                            string URL = entries[0]["entries"][i]["lmsUrl"].ToString();
                            DataList[dayInt32, Period] = new LMSData(dayInt32, Period, Label, URL);


                        }
                        MessageBox.Show("データのインポートに成功しました。", "インポート完了", MessageBoxButtons.OK, MessageBoxIcon.Information);



                        /*
                        foreach (JArray entry in entries["entries"]) 
                        {
                            Trace.TraceInformation(entry.ToString());
                        }
                        */

                        //終了処理
                        sr.Close();
                        stream.Close();

                        SetLMSData(true, false);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }





                }
            }


        }
        /// <summary>
        /// リンクを開く。
        /// </summary>
        /// <param name="Weekday"></param>
        /// <param name="Time"></param>
        public void LinkExecute(int Weekday, int Time, MouseButtons Mouse)
        {
            StartAudio(2, false);
            if (DataList[Weekday, Time].URL != "-1" && Mouse == MouseButtons.Left && isAllowed)
            {
                if (BrowserPath.Contains("Chrome"))
                    System.Diagnostics.Process.Start(BrowserPath,
                    "--disable-features=ExtensionManifestV2Unsupported,ExtensionManifestV2Disabled " + DataList[Weekday, Time].URL);
                else
                    System.Diagnostics.Process.Start(BrowserPath,
                                        DataList[Weekday, Time].URL);
                returns = 1;
            }
            else
            {
                returns = 0;

            }

        }
        public void LinkExecute(string URL)
        {
            if (BrowserPath.Contains("Chrome"))
                System.Diagnostics.Process.Start(BrowserPath,
                "--disable-features=ExtensionManifestV2Unsupported,ExtensionManifestV2Disabled " + URL);
            else
                System.Diagnostics.Process.Start(BrowserPath, URL);
        }

        /// <summary>
        /// これを組み込むと、指定ファイル以外での実行ファイル実行を抑止できる。
        /// 判定に引っかからない場合、InvalidOperationExceptionを投げる。
        /// </summary>
        public void AntiThreat(string Path)
        {
            //エラー防止もしくは脅威からの保護のため、実行ファイル名を定義。
            string[] str = new string[7] { "msedge.exe", "chrome.exe", "firefox.exe", "Brave.exe", "vivaldi.exe", "floorp.exe", "opera.exe" };
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                if (!Path.Contains(str[i]))
                {
                    count++;
                }
            }
            if (count == 7)
            {
                Trace.TraceError("リストに存在しない実行ファイルです。");
                isAllowed = false;
                checkBox1.Checked = false;
            }
            else
            {
                isAllowed = true;
                checkBox1.Checked = true;
            }
        }

        public string SetBrowser()
        {
            return SetBrowser(false, false);
        }
        public string SetBrowser(bool x86)
        {
            return SetBrowser(x86, false);
        }
        public string SetBrowser(bool x86, bool AppData)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            string path = "";
            if (x86)
                path = "C:\\Program Files (x86)";
            else if (AppData)
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData";
            else
                path = "C:\\Program Files";
            ofd.InitialDirectory = System.IO.Path.Combine(path);
            ofd.Filter = "実行ファイル|*.exe|すべて|*.*";
            ofd.Title = "ファイルを選択";
            string str = "";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (str != null)
                {
                    str = ofd.FileName;

                }
                return str;

            }
            else
            {
                return textBox1.Text;
            }

        }

        public void SetToolTip()
        {
            toolTip1.SetToolTip(linkLabel1, "教科名: " + DataList[0, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel2, "教科名: " + DataList[0, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel3, "教科名: " + DataList[0, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel4, "教科名: " + DataList[0, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel5, "教科名: " + DataList[0, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel6, "教科名: " + DataList[0, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel7, "教科名: " + DataList[1, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel8, "教科名: " + DataList[1, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel9, "教科名: " + DataList[1, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel10, "教科名: " + DataList[1, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel11, "教科名: " + DataList[1, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel12, "教科名: " + DataList[1, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel13, "教科名: " + DataList[2, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel14, "教科名: " + DataList[2, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel15, "教科名: " + DataList[2, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel16, "教科名: " + DataList[2, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel17, "教科名: " + DataList[2, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel18, "教科名: " + DataList[2, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel19, "教科名: " + DataList[3, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel20, "教科名: " + DataList[3, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel21, "教科名: " + DataList[3, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel22, "教科名: " + DataList[3, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel23, "教科名: " + DataList[3, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel24, "教科名: " + DataList[3, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel25, "教科名: " + DataList[4, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel26, "教科名: " + DataList[4, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel27, "教科名: " + DataList[4, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel28, "教科名: " + DataList[4, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel29, "教科名: " + DataList[4, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel30, "教科名: " + DataList[4, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel31, "教科名: " + DataList[5, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel32, "教科名: " + DataList[5, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel33, "教科名: " + DataList[5, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel34, "教科名: " + DataList[5, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel35, "教科名: " + DataList[5, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel36, "教科名: " + DataList[5, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 6");

            toolTip1.SetToolTip(linkLabel37, "教科名: " + DataList[6, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 1");
            toolTip1.SetToolTip(linkLabel38, "教科名: " + DataList[6, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 2");
            toolTip1.SetToolTip(linkLabel39, "教科名: " + DataList[6, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 3");
            toolTip1.SetToolTip(linkLabel40, "教科名: " + DataList[6, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 4");
            toolTip1.SetToolTip(linkLabel41, "教科名: " + DataList[6, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 5");
            toolTip1.SetToolTip(linkLabel42, "教科名: " + DataList[6, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 6");

        }

        public string IntToWeekDay(int i)
        {
            string str = "";
            switch (i)
            {
                case 0:
                    str = "月";
                    break;
                case 1:
                    str = "火";
                    break;
                case 2:
                    str = "水";
                    break;
                case 3:
                    str = "木";
                    break;
                case 4:
                    str = "金";
                    break;
                case 5:
                    str = "土";
                    break;
                case 6:
                    str = "集中等";
                    break;
            }
            return str;
        }

        #endregion

        #region [FileClipper]

        public void OpenFile(string FilePath)
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{FilePath}\"");
        }


        #endregion

        #region [Readme]

        public void CreateReadme()
        {
            /*
            var text = File.ReadAllText(@"readme.md");
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdig.Markdown.ToHtml(text, pipeline);
            webView21.BackColor = Color.White;
            webView21.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
            webView21.CoreWebView2.NavigateToString(result);
            */

            //実験: 学務情報サービスに入れるか？ →ログイン画面まで実装
            webView21.BackColor = Color.White;
            webView21.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Light;
            webView21.CoreWebView2.Navigate("https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/Top.aspx");

            if (webView21.CoreWebView2.Source.Contains("SSO"))
            {
                string ID = "sui69150";
                string Password = "%4uY7J=f";
                webView21.ExecuteScriptAsync("document.getElementsByName('j_username').item(0).value = '" + ID + "';");
                webView21.ExecuteScriptAsync("document.getElementsByName('j_password').item(0).value = '" + ID + "';");
            }

        }
        #endregion




        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(2, false);
        }




        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 画面要素の更新をします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartAudio(2, false);
            Placeindex = comboBox1.SelectedIndex;
            BackgroundTask();

        }


        private void tabPage5_Click(object sender, EventArgs e)
        {

        }

        private void label74_Click(object sender, EventArgs e)
        {

        }

        private void label77_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            if (form2 == null)
            {
                // フォームを生成して、表示します
                form2 = new Form2();
                form2.Show();
            }

            if (form2.IsDisposed == true)
            {
                form2 = new Form2();
                form2.Show();
            }

            else
            {
                form2.WindowState = FormWindowState.Normal;
                form2.Activate();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 0;
            linkLabel1.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 1;
            linkLabel2.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 2;
            linkLabel3.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 3;
            linkLabel4.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 4;
            linkLabel5.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 0;
            int Time = 5;
            linkLabel6.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 0;
            linkLabel7.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 1;
            linkLabel8.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 2;
            linkLabel9.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 3;
            linkLabel10.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 4;
            linkLabel11.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 1;
            int Time = 5;
            linkLabel12.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel13_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 0;
            linkLabel13.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel14_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 1;
            linkLabel14.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel15_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 2;
            linkLabel15.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel16_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 3;
            linkLabel16.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel17_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 4;
            linkLabel17.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel18_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 2;
            int Time = 5;
            linkLabel18.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel19_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 0;
            linkLabel19.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel20_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 1;
            linkLabel20.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel21_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 2;
            linkLabel21.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel22_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 3;
            linkLabel22.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel23_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 4;
            linkLabel23.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel24_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 3;
            int Time = 5;
            linkLabel24.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel25_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 0;
            linkLabel25.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel26_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 1;
            linkLabel26.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel27_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 2;
            linkLabel27.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel28_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 3;
            linkLabel28.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel29_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 4;
            linkLabel29.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel30_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 4;
            int Time = 5;
            linkLabel30.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel31_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 0;
            linkLabel31.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel32_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 1;
            linkLabel32.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel33_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 2;
            linkLabel33.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel34_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 3;
            linkLabel34.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel35_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 4;
            linkLabel35.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel36_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 5;
            int Time = 5;
            linkLabel36.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel37_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 0;
            linkLabel37.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel38_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 1;
            linkLabel38.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel39_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 2;
            linkLabel39.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel40_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 3;
            linkLabel40.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel41_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 4;
            linkLabel41.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void linkLabel42_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int Weekday = 6;
            int Time = 5;
            linkLabel42.LinkVisited = true;
            LinkExecute(Weekday, Time, e.Button);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            SetLMSDataFromjson();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            LinkExecute("https://acanthus.cis.kanazawa-u.ac.jp/base/top/");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            LinkExecute("https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/Top.aspx");
        }

        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (form2 == null)
            {
                // フォームを生成して、表示します
                form2 = new Form2();
                form2.Show();
            }

            if (form2.IsDisposed == true)
            {
                form2 = new Form2();
                form2.Show();
            }

            else
            {
                form2.WindowState = FormWindowState.Normal;
                form2.Activate();
            }
        }

        public void StartAudio(int number, bool stop)
        {
            if (!stop)
            {
                push[0] = new AudioFileReader("push0.mp3");
                push[1] = new AudioFileReader("push1.mp3");
                push[2] = new AudioFileReader("push2.mp3");
                var outputDevice = new WaveOutEvent();
                outputDevice.Init(push[number]);
                switch (number)
                {
                    case 1:
                        outputDevice.Volume = (float)0.15;
                        break;
                    default:
                        outputDevice.Volume = (float)1;
                        break;

                }
                outputDevice.Play();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            AntiThreat(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            BrowserPath = textBox1.Text;
            SetLMSData(true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            string str = SetBrowser();
            if (str != null || str == "")
            {
                textBox1.Text = str;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            string str = SetBrowser(true);
            if (str != null || str == "")
            {
                textBox1.Text = str;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            string str = SetBrowser(false, true);
            if (str != null || str == "")
            {
                textBox1.Text = str;
            }
        }

        private void LinkClicked(object sender, LinkClickedEventArgs e)
        {

        }

        private void doubleclick(object sender, EventArgs e)
        {
            OpenFile(listBox1.SelectedItem.ToString());
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            // 渡されたファイルに対して処理を行う
            foreach (var filePath in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                listBox1.Items.Add(filePath);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string str = "";
            foreach (var Items in listBox1.Items)
            {
                str += Items.ToString();
                str += "\n";
            }
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\\Documents\\Export.txt", str);
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ファイルを実行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(listBox1.SelectedItems.ToString()))
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(listBox1.SelectedItems.ToString()));
            }

        }

        private void 項目を削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItems);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (File.Exists(listBox1.Items[listBox1.SelectedIndex].ToString()))
            {
                using System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = listBox1.Items[listBox1.SelectedIndex].ToString();
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string str = richTextBox1.Text;
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\\Documents\\" + textBox2.Text + @".txt", str);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            richTextBox1.Font = new Font(label1.Font.FontFamily, (int)numericUpDown1.Value, label1.Font.Style);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isAllowed = checkBox1.Checked;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            textBox1.Text = BrowserPath;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {
            StartAudio(2, false);
            LinkExecute("https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/Attendance/AttendList.aspx");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //試験運用
            if (webView21.CoreWebView2.Source.Contains("SSO"))
            {
                string ID = textBox3.Text;
                string Password = textBox4.Text;
                webView21.ExecuteScriptAsync("document.getElementsByName('j_username').item(0).value = '" + ID + "';");
                webView21.ExecuteScriptAsync("document.getElementsByName('j_password').item(0).value = '" + Password + "';");
                webView21.ExecuteScriptAsync("document.getElementsByName('_eventId_proceed').item(0).click();");
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            IDtoINI(textBox3.Text, textBox4.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            IDtoINI(textBox3.Text, textBox4.Text);
        }

        public void IDtoINI(string ID, string Pass)
        {
            IniParser.Model.IniData data = new IniParser.Model.IniData();
            data["Main"]["ID"] = ID;
            data["Main"]["Pass"] = Pass;

            var parser = new FileIniDataParser();
            parser.WriteFile("idData.ini", data);
        }

       
    }
}
