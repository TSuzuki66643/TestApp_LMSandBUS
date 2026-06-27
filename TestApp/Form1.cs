using CsvHelper;
using HolidayJp; 
using IniParser;
using IniParser.Model;
using Json.Net;
using log4net;
using Markdig;
using Markdig.Wpf;
using Microsoft.VisualBasic;
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
using System.Windows.Media.Animation;
using Windows.ApplicationModel.UserDataTasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Enumeration;
using Windows.Devices.PointOfService;
using Windows.Devices.Power;
using Windows.Media.AppBroadcasting;
using Windows.UI.Notifications;
using Windows.UI.Notifications;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Timer = System.Threading.Timer;

namespace TestApp
{
    public partial class Form1 : Form
    {
        Form2 form2 = null;
        Form4 form4 = null;

        public Form1()
        {
            Trace.TraceInformation("Component初期化開始");
            logger.Info("コンポーネントの初期化を開始します。");
            InitializeComponent();
            Trace.TraceInformation("Component初期化完了");
            logger.Info("コンポーネントの初期化が完了しました。");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            logger.Info("残りの起動処理を開始します。");
            Trace.TraceInformation("その他の起動時処理開始");
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
                logger.Error("ファイルが見つかりませんでしたが続行します。");
            }
            logger.Info("処理完了: 設定ファイル読み込み");


            StartPage = int.Parse(data["General"]["StartTab"]);
            Settings = data;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if (attendstate[i, j, k] == null)
                        {
                            attendstate[i, j, k] = "0";
                        }
                    }
                }
            }
            logger.Info("処理完了: 出席データ処理");
            for (int i = 0; i < 100000; i++)
            {
                if (data["UserLink"]["Link" + (i + 1).ToString()] != null && data["UserLink"]["Label" + (i + 1).ToString()] != null)
                {
                    UserLink.Add(data["UserLink"]["Link" + (i + 1).ToString()]);
                    UserLinkLabel.Add(data["UserLink"]["Label" + (i + 1).ToString()]);
                    listBox2.Items.Add(UserLinkLabel[i]); //リストボックスに最初から加えておく
                }
                else
                {
                    break; //項目が見つからなったら、項目終了とみなしてループ解除
                }
            }
            for (int i = 0; i < 100000; i++)
            {
                if (data["Homework"]["Title" + (i + 1).ToString()] != null
                    && data["Homework"]["URL" + (i + 1).ToString()] != null
                    && data["Homework"]["Description" + (i + 1).ToString()] != null
                    && data["Homework"]["Deadline" + (i + 1).ToString()] != null
                    && data["Homework"]["subject" + (i + 1).ToString()] != null)
                {
                    HomeworkList.Add(new HomeworkData(
                        data["Homework"]["Title" + (i + 1).ToString()],
                        data["Homework"]["URL" + (i + 1).ToString()],
                        data["Homework"]["Description" + (i + 1).ToString()],
                        DateTime.Parse(data["Homework"]["Deadline" + (i + 1).ToString()]),
                        data["Homework"]["subject" + (i + 1).ToString()]
                    ));
                    listBox3.Items.Add(data["Homework"]["Title" + (i + 1).ToString()] + " | " + DateTime.Parse(data["Homework"]["Deadline" + (i + 1).ToString()]).ToString("yyyy-MM-dd HH:mm"));
                }
                else
                {
                    break; //項目が見つからなったら、項目終了とみなしてループ解除
                }
            }
            logger.Info("処理完了: ユーザーリンク、課題リスト");

            //バグ回避用に最初の項目を選択しておく（項目がある場合。なかったらそれはそれでエラー起こすから。）
            if (listBox2.Items.Count > 0)
            {
                listBox2.SelectedIndex = 0;
            }
            if (listBox3.Items.Count > 0)
            {
                listBox3.SelectedIndex = 0;
            }


            // コンピュータ名 = "." はローカルコンピュータを表す。コンピュータ名は省略可能（省略時は"."）
            // Memory/Available MBytesのようにインスタンスを指定できない項目は、インスタンスを空文字にする

            // 取りたい情報を並べる

            /*
            var counterList = new List<(string machine, string category, string counter, string instance)>();
            var pcList = new List<PerformanceCounter>();

            counterList.Add((".", "Processor", "% Processor Time", "_Total"));
            counterList.Add((".", "Network Interface", "Bytes Total/Sec", "Intel[R] Wi-Fi 6 AX201 160MHz")); // インスタンス名は機種によって変わる。
            counterList.Add((".", "PhysicalDisk", "% Disk Time", "_Total"));
            counterList.Add((".", "Memory", "Available MBytes", ""));
            counterList.Add((".", "Process", "Working Set", "_Total"));
            counterList.Add((".", "Process", "IO Data Bytes/Sec", "_Total"));
            

            // エラーチェック後、PerformanceCounterオブジェクトの作成を作成
            counterList.ForEach((x) =>
            {
                if (!PerformanceCounterCategory.Exists(x.category, x.machine))
                {
                    //カテゴリが存在するか確かめる
                    Console.WriteLine("登録されていないカテゴリです：" + x.category);
                }
                else if (!PerformanceCounterCategory.CounterExists(x.counter, x.category, x.machine))
                {
                    //カウンタが存在するか確かめる
                    Console.WriteLine("登録されていないカウンタです：" + x.counter);
                }
                else
                {
                    //PerformanceCounterオブジェクトの作成
                    pcList.Add(new PerformanceCounter(x.category, x.counter, x.instance, x.machine));
                }
            });

            */
            tabControl1.SelectedIndex = StartPage;
            comboBox3.SelectedIndex = 0;

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
            comboBox2.SelectedIndex = 3;
            Ccounter = 0;

            InternetInstanceName = data["Browser"]["InternetInstanceName"] != null ? data["Browser"]["InternetInstanceName"] : "Intel[R] Wi-Fi 6 AX201 160MHz";

            await webView21.EnsureCoreWebView2Async();
            logger.Info("処理完了: その他の設定");


            PowerStatus status = SystemInformation.PowerStatus;
            isCharging = (status.BatteryChargeStatus & BatteryChargeStatus.Charging) != 0;
            isAcOnline = status.PowerLineStatus == PowerLineStatus.Online;


            previousChargeRate = 1;

            if (!isCharging && !isAcOnline)
            {
                previousChargeRate = -1;
            }



            Action act = BackgroundTask;
            Task task = new Task(act);
            try
            {
                Trace.TraceInformation("BackgroundTask最初の起動");
                logger.Info("BackgroundTask最初の処理を行います");
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
            CreateReadme();



            Trace.TraceInformation("ループ処理起動");
            logger.Info("ループ処理を開始しました");
            // 非同期でループを開始
            await Task.Run(() =>
            {
                while (true) // 無限ループ
                {
                    // 時間のかかる計算やデータ取得を想定


                    // UIスレッドでコントロールを安全に更新
                    this.Invoke((MethodInvoker)delegate
                    {
                        GetBatteryData();
                        if (DateTime.Now.Second == 0)
                            this.BackgroundTask();
                        WriteLog();
                    });

                    // 更新間隔（1秒）
                    Thread.Sleep(1000);
                }
            });

        }
        public async void WriteLog()
        {

        }

        public struct Progress
        {
            public int Process1;
            public int Process2;
            public int Process3;
            public int Total;
        }

        private static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static log4net.ILog logger2 = LogManager.GetLogger("PowerStatusLogger");
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
        public int isCanceled = 0; //運転中止
        public void BackgroundTask()
        {
            AntiThreat(BrowserPath);
            SetLMSData();
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
                                || (Buslist[data].Details[i].isSkip == true && (nowMonth == 2 && nowDay >= 13 || nowMonth == 3)))
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
                logger.Warn("BusUtility: シミュレーションモードが有効になっています。");
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
            logger.Info("BusUtility: バス時刻表データの反映を完了しました。");

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
            Buslist[93].Details.Add(new Details(930503, 08, 35, false, 93, 0, 0, 0, 52));// 2026年7月5日以降で最後の変数をtrueに
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

            #region 2026年7月ダイヤ改正対応関数(改正後に削除)
            //2026年7月5日以降、平日93系統8:35発の便は学期休み運休となる
            if (( DateTime.Now.Year == 2026 && (DateTime.Now.Month == 7 && DateTime.Now.Day >= 5) || DateTime.Now.Month > 7) || DateTime.Now.Year > 2026)
            {
                Buslist[93].Details[2] = IsSkipModify(Buslist[93].Details[2], true);
            }

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

            logger.Info("BusUtility: 時刻表データを読み込みました。");

        }
        public Details IsSkipModify(Details d, bool isSkip)
        {
            d.isSkip = isSkip;
            return d;
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

        public static string? attendList;

        public static string[,,] syllabusURL = new string[7, 6, 4];
        public static string controlname;

        Stopwatch sw;

        public static bool Form3_closed = false;

        public static string[,,] attendstate = new string[7, 6, 3];
        //public static double[,] attendstatus = new double[7, 6];
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
                    logger.Info("LMSJunper: Jsonファイルからの読み込みのため、定義がスキップされます。");
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
                        logger.Error("LMSJumper: LMSデータ設定に失敗しました。");
                        MessageBox.Show("LMSデータ設定に失敗しました。\n" + e.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                        //補足: 内部のデータは削除済みのため、現在これは用いない
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


            }
            else
            {
                Trace.TraceInformation("定義処理はスキップします。");
                logger.Info("LMSJumper: 定義処理はスキップされます。");
            }
            //Config.iniに書き込む準備
            IniParser.Model.IniData data = new IniParser.Model.IniData();
            if (Settings != null)
            {
                //設定がすでにある場合
                data = Settings;
            }

            SetToolTip();
            logger.Info("LMSJumper: ツールチップ設定が完了しました。");

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

            data["attend"]["mon1"] = attendstate[0, 0, 1] + "/" + attendstate[0, 0, 0];
            data["attend"]["mon2"] = attendstate[0, 1, 1] + "/" + attendstate[0, 1, 0];
            data["attend"]["mon3"] = attendstate[0, 2, 1] + "/" + attendstate[0, 2, 0];
            data["attend"]["mon4"] = attendstate[0, 3, 1] + "/" + attendstate[0, 3, 0];
            data["attend"]["mon5"] = attendstate[0, 4, 1] + "/" + attendstate[0, 4, 0];
            data["attend"]["mon6"] = attendstate[0, 5, 1] + "/" + attendstate[0, 5, 0];

            data["attend"]["tue1"] = attendstate[1, 0, 1] + "/" + attendstate[1, 0, 0];
            data["attend"]["tue2"] = attendstate[1, 1, 1] + "/" + attendstate[1, 1, 0];
            data["attend"]["tue3"] = attendstate[1, 2, 1] + "/" + attendstate[1, 2, 0];
            data["attend"]["tue4"] = attendstate[1, 3, 1] + "/" + attendstate[1, 3, 0];
            data["attend"]["tue5"] = attendstate[1, 4, 1] + "/" + attendstate[1, 4, 0];
            data["attend"]["tue6"] = attendstate[1, 5, 1] + "/" + attendstate[1, 5, 0];

            data["attend"]["wed1"] = attendstate[2, 0, 1] + "/" + attendstate[2, 0, 0];
            data["attend"]["wed2"] = attendstate[2, 1, 1] + "/" + attendstate[2, 1, 0];
            data["attend"]["wed3"] = attendstate[2, 2, 1] + "/" + attendstate[2, 2, 0];
            data["attend"]["wed4"] = attendstate[2, 3, 1] + "/" + attendstate[2, 3, 0];
            data["attend"]["wed5"] = attendstate[2, 4, 1] + "/" + attendstate[2, 4, 0];
            data["attend"]["wed6"] = attendstate[2, 5, 1] + "/" + attendstate[2, 5, 0];

            data["attend"]["thu1"] = attendstate[3, 0, 1] + "/" + attendstate[3, 0, 0];
            data["attend"]["thu2"] = attendstate[3, 1, 1] + "/" + attendstate[3, 1, 0];
            data["attend"]["thu3"] = attendstate[3, 2, 1] + "/" + attendstate[3, 2, 0];
            data["attend"]["thu4"] = attendstate[3, 3, 1] + "/" + attendstate[3, 3, 0];
            data["attend"]["thu5"] = attendstate[3, 4, 1] + "/" + attendstate[3, 4, 0];
            data["attend"]["thu6"] = attendstate[3, 5, 1] + "/" + attendstate[3, 5, 0];

            data["attend"]["fri1"] = attendstate[4, 0, 1] + "/" + attendstate[4, 0, 0];
            data["attend"]["fri2"] = attendstate[4, 1, 1] + "/" + attendstate[4, 1, 0];
            data["attend"]["fri3"] = attendstate[4, 2, 1] + "/" + attendstate[4, 2, 0];
            data["attend"]["fri4"] = attendstate[4, 3, 1] + "/" + attendstate[4, 3, 0];
            data["attend"]["fri5"] = attendstate[4, 4, 1] + "/" + attendstate[4, 4, 0];
            data["attend"]["fri6"] = attendstate[4, 5, 1] + "/" + attendstate[4, 5, 0];

            data["attend"]["sat1"] = attendstate[5, 0, 1] + "/" + attendstate[5, 0, 0];
            data["attend"]["sat2"] = attendstate[5, 1, 1] + "/" + attendstate[5, 1, 0];
            data["attend"]["sat3"] = attendstate[5, 2, 1] + "/" + attendstate[5, 2, 0];
            data["attend"]["sat4"] = attendstate[5, 3, 1] + "/" + attendstate[5, 3, 0];
            data["attend"]["sat5"] = attendstate[5, 4, 1] + "/" + attendstate[5, 4, 0];
            data["attend"]["sat6"] = attendstate[5, 5, 1] + "/" + attendstate[5, 5, 0];

            data["attend"]["oth1"] = attendstate[6, 0, 1] + "/" + attendstate[6, 0, 0];
            data["attend"]["oth2"] = attendstate[6, 1, 1] + "/" + attendstate[6, 1, 0];
            data["attend"]["oth3"] = attendstate[6, 2, 1] + "/" + attendstate[6, 2, 0];
            data["attend"]["oth4"] = attendstate[6, 3, 1] + "/" + attendstate[6, 3, 0];
            data["attend"]["oth5"] = attendstate[6, 4, 1] + "/" + attendstate[6, 4, 0];
            data["attend"]["oth6"] = attendstate[6, 5, 1] + "/" + attendstate[6, 5, 0];

            data["syllabus"]["mon1"] = syllabusURL[0, 0, 0];
            data["syllabus"]["mon2"] = syllabusURL[0, 1, 0];
            data["syllabus"]["mon3"] = syllabusURL[0, 2, 0];
            data["syllabus"]["mon4"] = syllabusURL[0, 3, 0];
            data["syllabus"]["mon5"] = syllabusURL[0, 4, 0];
            data["syllabus"]["mon6"] = syllabusURL[0, 5, 0];

            data["syllabus"]["tue1"] = syllabusURL[1, 0, 0];
            data["syllabus"]["tue2"] = syllabusURL[1, 1, 0];
            data["syllabus"]["tue3"] = syllabusURL[1, 2, 0];
            data["syllabus"]["tue4"] = syllabusURL[1, 3, 0];
            data["syllabus"]["tue5"] = syllabusURL[1, 4, 0];
            data["syllabus"]["tue6"] = syllabusURL[1, 5, 0];

            data["syllabus"]["wed1"] = syllabusURL[2, 0, 0];
            data["syllabus"]["wed2"] = syllabusURL[2, 1, 0];
            data["syllabus"]["wed3"] = syllabusURL[2, 2, 0];
            data["syllabus"]["wed4"] = syllabusURL[2, 3, 0];
            data["syllabus"]["wed5"] = syllabusURL[2, 4, 0];
            data["syllabus"]["wed6"] = syllabusURL[2, 5, 0];

            data["syllabus"]["thu1"] = syllabusURL[3, 0, 0];
            data["syllabus"]["thu2"] = syllabusURL[3, 1, 0];
            data["syllabus"]["thu3"] = syllabusURL[3, 2, 0];
            data["syllabus"]["thu4"] = syllabusURL[3, 3, 0];
            data["syllabus"]["thu5"] = syllabusURL[3, 4, 0];
            data["syllabus"]["thu6"] = syllabusURL[3, 5, 0];

            data["syllabus"]["fri1"] = syllabusURL[4, 0, 0];
            data["syllabus"]["fri2"] = syllabusURL[4, 1, 0];
            data["syllabus"]["fri3"] = syllabusURL[4, 2, 0];
            data["syllabus"]["fri4"] = syllabusURL[4, 3, 0];
            data["syllabus"]["fri5"] = syllabusURL[4, 4, 0];
            data["syllabus"]["fri6"] = syllabusURL[4, 5, 0];

            data["syllabus"]["sat1"] = syllabusURL[5, 0, 0];
            data["syllabus"]["sat2"] = syllabusURL[5, 1, 0];
            data["syllabus"]["sat3"] = syllabusURL[5, 2, 0];
            data["syllabus"]["sat4"] = syllabusURL[5, 3, 0];
            data["syllabus"]["sat5"] = syllabusURL[5, 4, 0];
            data["syllabus"]["sat6"] = syllabusURL[5, 5, 0];

            data["syllabus"]["oth1"] = syllabusURL[6, 0, 0];
            data["syllabus"]["oth2"] = syllabusURL[6, 1, 0];
            data["syllabus"]["oth3"] = syllabusURL[6, 2, 0];
            data["syllabus"]["oth4"] = syllabusURL[6, 3, 0];
            data["syllabus"]["oth5"] = syllabusURL[6, 4, 0];
            data["syllabus"]["oth6"] = syllabusURL[6, 5, 0];
            data["Status"]["WiFiInstanceName"] = InternetInstanceName;

            if (listBox2.Items.Count > 0)
            {
                int index = 0;
                if (UserLink.Count > 0 && UserLinkLabel.Count > 0)
                {
                    for (int i = 0; i < UserLink.Count; i++)
                    {
                        data["UserLink"]["Link" + (i + 1).ToString()] = UserLink[i];
                    }
                    for (int i = 0; i < UserLinkLabel.Count; i++)
                    {
                        data["UserLink"]["Label" + (i + 1).ToString()] = UserLinkLabel[i];
                        index = i + 1;
                    }
                }
                for (int i = index; i < 100000; i++)
                {
                    if (data.Sections["UserLink"].ContainsKey("Link" + (i + 1).ToString()) || data.Sections["UserLink"].ContainsKey("Label" + (i + 1).ToString()))
                    {
                        data["UserLink"].RemoveKey("Link" + (i + 1).ToString());
                        data["UserLink"].RemoveKey("Label" + (i + 1).ToString());
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (listBox3.Items.Count > 0)
            {
                int index = 0;
                if (HomeworkList.Count > 0)
                {
                    for (int i = 0; i < HomeworkList.Count; i++)
                    {
                        data["Homework"]["Title" + (i + 1).ToString()] = HomeworkList[i].title;
                        data["Homework"]["URL" + (i + 1).ToString()] = HomeworkList[i].URL;
                        data["Homework"]["Description" + (i + 1).ToString()] = HomeworkList[i].Description;
                        data["Homework"]["Deadline" + (i + 1).ToString()] = HomeworkList[i].Deadline.ToString("yyyy/MM/dd HH:mm");
                        data["Homework"]["subject" + (i + 1).ToString()] = HomeworkList[i].subject;
                        index++;
                    }
                }
                for (int i = index; i < 100000; i++)
                {
                    if (data.Sections["Homework"].ContainsKey("Title" + (i + 1).ToString()))
                    {
                        data["Homework"].RemoveKey("Title" + (i + 1).ToString());
                        data["Homework"].RemoveKey("URL" + (i + 1).ToString());
                        data["Homework"].RemoveKey("Description" + (i + 1).ToString());
                        data["Homework"].RemoveKey("Deadline" + (i + 1).ToString());
                        data["Homework"].RemoveKey("subject" + (i + 1).ToString());
                    }
                    else
                    {
                        break;
                    }
                }
            }



            Settings = data;


            var parser = new FileIniDataParser();
            parser.WriteFile("Config.ini", Settings);
            logger.Info("LMSJumper: 設定ファイルへの書き込みが完了しました。");

            tabPage1.Update();
        }

        public void SetLMSDataManually(LoadingFailureIndicator ExceptionCode)
        {

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
                string weekday2 = "";
                switch (i)
                {
                    case 0:
                        Weekday = "Monday";
                        weekday2 = "mon";
                        break;
                    case 1:
                        Weekday = "Tuesday";
                        weekday2 = "tue";
                        break;
                    case 2:
                        Weekday = "Wednesday";
                        weekday2 = "wed";
                        break;
                    case 3:
                        Weekday = "Thursday";
                        weekday2 = "thu";
                        break;
                    case 4:
                        Weekday = "Friday";
                        weekday2 = "fri";
                        break;
                    case 5:
                        Weekday = "Saturday";
                        weekday2 = "sat";
                        break;
                    case 6:
                        Weekday = "Other";
                        weekday2 = "oth";
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
                    attendstate[i, j, 0] = data["attend"][weekday2 + (j + 1).ToString()].Split("/")[1];
                    attendstate[i, j, 1] = data["attend"][weekday2 + (j + 1).ToString()].Split("/")[0];
                    syllabusURL[i, j, 0] = data["syllabus"][weekday2 + (j + 1).ToString()];
                }
            }
            logger.Info("LMSJumper: 設定からLMSをデータを読み込みました。");
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


        public async void SetLMSDataFromjson()
        {
            await SetLMSDataFromjson(false);
        }

        /// <summary>
        /// #バイバイ金大LMS からエクスポートしたjsonからLMSデータの抽出・設定を行う。
        /// </summary>
        public async Task SetLMSDataFromjson(bool b演出)
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

                        var form3 = new Form3();
                        //開き処理
                        if (b演出)
                        {



                            // フォームを生成して、表示します
                            form3 = new Form3();
                            form3.Show();
                            logger.Info("LMSJumper: フォームが表示されました。演出モードが有効です。");

                        }
                        StreamReader sr = new StreamReader(stream);
                        logger.Info("LMSJumper: ファイルを開いています...");
                        if (b演出)
                        {
                            Form3.Msg = "ファイルを開いています...";
                            
                            Form3.Progress = 0;
                            await Task.Delay(800);

                        }
                        logger.Info("LMSJumper: ファイルの情報を取得しています...");
                        string str = sr.ReadToEnd();
                        if (b演出)
                        {
                            Form3.Msg = "ファイルを読み込んでいます...";
                            
                            Form3.Progress = 15;
                            await Task.Delay(1200);

                        }
                        //Trace.TraceInformation(str);
                        logger.Info("LMSJumper: ファイルの情報を取得しています...");
                        JToken entries = JToken.Parse(str);
                        for (int i = 0; i < 7; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                DataList[i, j] = new LMSData(i, j, "(なし)", "-1");
                            }
                        }
                        if (b演出)
                        {
                            Form3.Msg = "ファイルの情報を取得しています...";
                            
                            Form3.Progress = 30;
                            await Task.Delay(1500);

                        }
                        logger.Info("LMSJumper: データを読み込んでいます...");
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

                            syllabusURL[dayInt32, Period, 0] = entries[0]["entries"][i]["syllabusUrl"].ToString();

                        }
                        if (b演出)
                        {
                            Form3.Msg = "データを読み込んでいます...";
                            Form3.Progress = 55;
                            await Task.Delay(3500);

                        }






                        /*
                        foreach (JArray entry in entries["entries"]) 
                        {
                            Trace.TraceInformation(entry.ToString());
                        }
                        */

                        //終了処理
                        sr.Close();
                        stream.Close();
                        if (b演出)
                        {
                            Form3.Msg = "ファイルを閉じています...";
                            Form3.Progress = 98;
                            await Task.Delay(500);
                        }


                        SetLMSData(true, false);
                        if (b演出)
                        {
                            Form3.Msg = "最後の処理をしています...";
                            Form3.Progress = 99;
                            await Task.Delay(1500);
                            Form3.Progress = 100;
                            while (!Form3_closed)
                            {
                                await Task.Delay(100);
                            }
                        }

                        logger.Info("LMSJumper: Jsonファイルからのデータを読み込みました。");
                        MessageBox.Show("データのインポートに成功しました。", "インポート完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception e)
                    {
                        logger.Error("LMSJumper: Jsonファイルの読み込み中にエラーが発生しました。\n" + e.ToString());
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
                logger.Info("LMSJumper: リンクが実行されました。");
            }
            else
            {
                returns = 0;
                logger.Info("LMSJumper: リンクの実行がキャンセルされました。");
            }

        }
        public void LinkExecute(string URL)
        {
            if (BrowserPath.Contains("Chrome"))
                System.Diagnostics.Process.Start(BrowserPath,
                "--disable-features=ExtensionManifestV2Unsupported,ExtensionManifestV2Disabled " + URL);
            else
                System.Diagnostics.Process.Start(BrowserPath, URL);

            logger.Info("main: リンクが実行されました。");
        }

        /// <summary>
        /// これを組み込むと、ブラウザーかどうかの判定ができる。
        /// InvalidDataExceptionを投げて抑止もできる。
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
                logger.Warn("LMSJumper: このソフトは読み込まれたブラウザーリストと一致しませんでした。");
                isAllowed = false;
                checkBox1.Checked = false;

                bool stop = false;
                if (stop)
                {
                    //throw new InvalidDataException("ブラウザー以外の実行ファイルが検出されました。");
                }

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
            toolTip1.SetToolTip(linkLabel1, "教科名: " + DataList[0, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 1" + "\n出席: " + attendstate[0, 0, 1] + "/" + attendstate[0, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel2, "教科名: " + DataList[0, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 2" + "\n出席: " + attendstate[0, 1, 1] + "/" + attendstate[0, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel3, "教科名: " + DataList[0, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 3" + "\n出席: " + attendstate[0, 2, 1] + "/" + attendstate[0, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel4, "教科名: " + DataList[0, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 4" + "\n出席: " + attendstate[0, 3, 1] + "/" + attendstate[0, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel5, "教科名: " + DataList[0, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 5" + "\n出席: " + attendstate[0, 4, 1] + "/" + attendstate[0, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel6, "教科名: " + DataList[0, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(0) + "\n時限: 6" + "\n出席: " + attendstate[0, 5, 1] + "/" + attendstate[0, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel7, "教科名: " + DataList[1, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 1" + "\n出席: " + attendstate[1, 0, 1] + "/" + attendstate[1, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel8, "教科名: " + DataList[1, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 2" + "\n出席: " + attendstate[1, 1, 1] + "/" + attendstate[1, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel9, "教科名: " + DataList[1, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 3" + "\n出席: " + attendstate[1, 2, 1] + "/" + attendstate[1, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel10, "教科名: " + DataList[1, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 4" + "\n出席: " + attendstate[1, 3, 1] + "/" + attendstate[1, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel11, "教科名: " + DataList[1, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 5" + "\n出席: " + attendstate[1, 4, 1] + "/" + attendstate[1, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel12, "教科名: " + DataList[1, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(1) + "\n時限: 6" + "\n出席: " + attendstate[1, 5, 1] + "/" + attendstate[1, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel13, "教科名: " + DataList[2, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 1" + "\n出席: " + attendstate[2, 0, 1] + "/" + attendstate[2, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel14, "教科名: " + DataList[2, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 2" + "\n出席: " + attendstate[2, 1, 1] + "/" + attendstate[2, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel15, "教科名: " + DataList[2, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 3" + "\n出席: " + attendstate[2, 2, 1] + "/" + attendstate[2, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel16, "教科名: " + DataList[2, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 4" + "\n出席: " + attendstate[2, 3, 1] + "/" + attendstate[2, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel17, "教科名: " + DataList[2, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 5" + "\n出席: " + attendstate[2, 4, 1] + "/" + attendstate[2, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel18, "教科名: " + DataList[2, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(2) + "\n時限: 6" + "\n出席: " + attendstate[2, 5, 1] + "/" + attendstate[2, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel19, "教科名: " + DataList[3, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 1" + "\n出席: " + attendstate[3, 0, 1] + "/" + attendstate[3, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel20, "教科名: " + DataList[3, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 2" + "\n出席: " + attendstate[3, 1, 1] + "/" + attendstate[3, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel21, "教科名: " + DataList[3, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 3" + "\n出席: " + attendstate[3, 2, 1] + "/" + attendstate[3, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel22, "教科名: " + DataList[3, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 4" + "\n出席: " + attendstate[3, 3, 1] + "/" + attendstate[3, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel23, "教科名: " + DataList[3, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 5" + "\n出席: " + attendstate[3, 4, 1] + "/" + attendstate[3, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel24, "教科名: " + DataList[3, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(3) + "\n時限: 6" + "\n出席: " + attendstate[3, 5, 1] + "/" + attendstate[3, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel25, "教科名: " + DataList[4, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 1" + "\n出席: " + attendstate[4, 0, 1] + "/" + attendstate[4, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel26, "教科名: " + DataList[4, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 2" + "\n出席: " + attendstate[4, 1, 1] + "/" + attendstate[4, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel27, "教科名: " + DataList[4, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 3" + "\n出席: " + attendstate[4, 2, 1] + "/" + attendstate[4, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel28, "教科名: " + DataList[4, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 4" + "\n出席: " + attendstate[4, 3, 1] + "/" + attendstate[4, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel29, "教科名: " + DataList[4, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 5" + "\n出席: " + attendstate[4, 4, 1] + "/" + attendstate[4, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel30, "教科名: " + DataList[4, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(4) + "\n時限: 6" + "\n出席: " + attendstate[4, 5, 1] + "/" + attendstate[4, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel31, "教科名: " + DataList[5, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 1" + "\n出席: " + attendstate[5, 0, 1] + "/" + attendstate[5, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel32, "教科名: " + DataList[5, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 2" + "\n出席: " + attendstate[5, 1, 1] + "/" + attendstate[5, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel33, "教科名: " + DataList[5, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 3" + "\n出席: " + attendstate[5, 2, 1] + "/" + attendstate[5, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel34, "教科名: " + DataList[5, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 4" + "\n出席: " + attendstate[5, 3, 1] + "/" + attendstate[5, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel35, "教科名: " + DataList[5, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 5" + "\n出席: " + attendstate[5, 4, 1] + "/" + attendstate[5, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel36, "教科名: " + DataList[5, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(5) + "\n時限: 6" + "\n出席: " + attendstate[5, 5, 1] + "/" + attendstate[5, 5, 0] + "\n右クリックでオプションを表示");

            toolTip1.SetToolTip(linkLabel37, "教科名: " + DataList[6, 0].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 1" + "\n出席: " + attendstate[6, 0, 1] + "/" + attendstate[6, 0, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel38, "教科名: " + DataList[6, 1].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 2" + "\n出席: " + attendstate[6, 1, 1] + "/" + attendstate[6, 1, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel39, "教科名: " + DataList[6, 2].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 3" + "\n出席: " + attendstate[6, 2, 1] + "/" + attendstate[6, 2, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel40, "教科名: " + DataList[6, 3].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 4" + "\n出席: " + attendstate[6, 3, 1] + "/" + attendstate[6, 3, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel41, "教科名: " + DataList[6, 4].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 5" + "\n出席: " + attendstate[6, 4, 1] + "/" + attendstate[6, 4, 0] + "\n右クリックでオプションを表示");
            toolTip1.SetToolTip(linkLabel42, "教科名: " + DataList[6, 5].Title.Replace("\n", "") + "\n曜日: " + IntToWeekDay(6) + "\n時限: 6" + "\n出席: " + attendstate[6, 5, 1] + "/" + attendstate[6, 5, 0] + "\n右クリックでオプションを表示");

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

        //以下はシラバスに関するコード
        public static void GETsyllabusURL()
        {

        }


        #endregion

        #region [FileClipper]

        public void OpenFile(string FilePath)
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{FilePath}\"");
        }


        #endregion

        #region [Readme]

        public static bool Nav;
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

                webView21.ExecuteScriptAsync("document.getElementsByName('j_username').item(0).value = '" + "';");
                webView21.ExecuteScriptAsync("document.getElementsByName('j_password').item(0).value = '" + "';");
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
            logger.Info("BusUtility: 表示バス停を変更しました。");
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
            logger.Info("main: 設定を表示します。Form2が展開されました。");
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
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\\Documents\\Export.txt", str);
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ファイルを実行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(listBox1.SelectedItems.ToString()))
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
            if (System.IO.File.Exists(listBox1.Items[listBox1.SelectedIndex].ToString()))
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
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\\Documents\\" + textBox2.Text + @".txt", str);
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

        private async void button15_Click(object sender, EventArgs e)
        {

            //試験運用
            if (webView21.CoreWebView2.Source.Contains("SSO"))
            {
                Nav = false;
                string ID = textBox3.Text;
                string Password = textBox4.Text;
                await webView21.ExecuteScriptAsync("document.getElementsByName('j_username').item(0).value = '" + ID + "';");
                await webView21.ExecuteScriptAsync("document.getElementsByName('j_password').item(0).value = '" + Password + "';");
                await webView21.ExecuteScriptAsync("document.getElementsByName('_eventId_proceed').item(0).click();");
                sw = Stopwatch.StartNew();

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

        public string script(int row, int cells)
        {
            var script = "function getTableData() {const table = document.getElementById('ctl00_phContents_ucAttendLctList_grvAttendList');\r\n\r\n" +
                "// 1. 全ての行をループしてデータを取得\r\n" +
                "for (let i = 0; i < table.rows.length; i++) {" +
                "\r\n    let row = table.rows[i];\r\n" +
                "    for (let j = 0; j < row.cells.length; j++) {\r\n" +
                "        console.log(row.cells[j].innerText); // セルのテキストを表示\r\n" +
                "    }\r\n" +
                "}\r\n\r\n" +
                "// 2. 特定の行（例: 2行目）の特定の列（例: 1列目）を取得\r\n" +
                "const cellValue = table.rows[" + row + "].cells[" + cells + "].innerText;" +
                "return  JSON.stringify(cellValue); }" +
                "\r\n \r\n getTableData();";
            return script;
        }


        private async void button16_Click(object sender, EventArgs e)
        {
            attendstate = new string[7, 6, 3];
            try
            {
                int counter = 0;

                for (int i = 1; i < 50; i++)
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 5)));
                    var quater = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 2))).Replace("\"", "");

                    int day = 0;
                    int period = 0;
                    if (data == null)
                    {
                        break;
                    }
                    if (data.Contains("月"))
                    {
                        day = 0;
                    }
                    else if (data.Contains("火"))
                    {
                        day = 1;
                    }
                    else if (data.Contains("水"))
                    {
                        day = 2;
                    }
                    else if (data.Contains("木"))
                    {
                        day = 3;
                    }
                    else if (data.Contains("金"))
                    {
                        day = 4;
                    }
                    else if (data.Contains("土"))
                    {
                        day = 5;
                    }
                    else
                    {
                        day = 6;
                    }
                    if (data.Contains("1"))
                    {
                        period = 0;
                    }
                    else if (data.Contains("2"))
                    {
                        period = 1;
                    }
                    else if (data.Contains("3"))
                    {
                        period = 2;
                    }
                    else if (data.Contains("4"))
                    {
                        period = 3;
                    }
                    else if (data.Contains("5"))
                    {
                        period = 4;
                    }
                    else if (data.Contains("6"))
                    {
                        period = 5;
                    }

                    string mode = "";
                    switch (comboBox3.SelectedIndex)
                    {
                        case 0:
                            mode = "Q1";
                            break;
                        case 1:
                            mode = "Q2";
                            break;
                        case 2:
                            mode = "Q3";
                            break;
                        case 3:
                            mode = "Q4";
                            break;
                    }

                    if (data.Contains("〜"))
                    {
                        int at = 0;
                        if (data.Contains("1"))
                        {
                            at = 0;
                        }
                        if (data.Contains("2"))
                        {
                            at = 1;
                        }
                        if (data.Contains("3"))
                        {
                            at = 2;
                        }
                        if (data.Contains("4"))
                        {
                            at = 3;
                        }
                        if (data.Contains("5"))
                        {
                            at = 4;
                        }
                        if (data.Contains("6"))
                        {
                            at = 5;
                        }





                        for (int j = period; j < at + 1; j++)
                        {
                            if (quater.Contains(mode))
                            {
                                Trace.TraceInformation(i + "," + quater);
                                attendstate[day, j, 0] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 6))).Replace("\"", "");
                                attendstate[day, j, 1] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 7))).Replace("\"", "");
                            }
                        }
                    }


                    else if (day != 6)
                    {
                        if (quater.Contains(mode))
                        {
                            Trace.TraceInformation(i + "," + quater);
                            attendstate[day, period, 0] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 6))).Replace("\"", "");
                            attendstate[day, period, 1] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 7))).Replace("\"", "");
                        }
                    }
                    else
                    {
                        if (quater.Contains(mode))
                        {
                            Trace.TraceInformation(i + "," + quater);
                            attendstate[day, counter, 0] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 6))).Replace("\"", "");
                            attendstate[day, counter, 1] = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script(i, 7))).Replace("\"", "");
                        }
                    }


                    if (attendstate[day, period, 0] == null)
                    {
                        Trace.TraceError("null返ってきた");
                    }
                    if (day == 6)
                    {
                        counter++;
                    }

                }

                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            Trace.TraceInformation(attendstate[i, j, k]);
                            if (attendstate[i, j, k] == null || attendstate[i, j, k] == " ")
                            {
                                attendstate[i, j, k] = "0";
                            }
                        }
                    }
                }

            }
            catch
            {
                Trace.TraceError("");
            }

            SetLMSData(true);


        }

        private void シラバスを開く未実装ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var name = int.Parse(controlname.Replace("linkLabel", "")) - 1;
                int day = (int)Math.Floor(name / 6.0);
                int period = name - day * 6;
                LinkExecute(syllabusURL[day, period, 0]);
            }
            catch (Exception w)
            {
                Trace.TraceError(controlname.Replace("linkLabel", "") + "をintに変換できません。\n" + w.ToString());
                MessageBox.Show("エラーが発生し、処理できませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void t開かれたときの操作(object sender, CancelEventArgs e)
        {
            // コンテキストメニューからソースコントロールを取得
            Control sourceControl = contextMenuStrip1.SourceControl;
            if (sourceControl != null)
            {
                controlname = sourceControl.Name;
            }

        }

        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView21.CoreWebView2.Source.ToString() == "https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/Top.aspx")
            {
                sw.Stop();
                label23.Text = sw.ElapsedMilliseconds.ToString() + "ms";
                tabPage2.Update();
            }
        }

        public static int? previousChargeRate = 1;
        public static bool notified = false;
        public static int Ccounter;
        public static string InternetInstanceName = "Intel[R] Wi-Fi 6 AX201 160MHz";
        public static bool isExecuted = false;
        public static bool isCharging;
        public static bool isAcOnline;
        public static int NotifyModeSelector = 0;

        public static async Task GetBatteryData()
        {
            var rep = await DeviceInformation.FindAllAsync(Battery.GetDeviceSelector());

            int? chargeRate = 0;
            int? MaxBatteryD = 1;
            int? MaxBatteryC = 0;
            int? RemainingBattery = 0;
            decimal BatteryPercentage = 0;
            decimal charge = 0;
            decimal chargepercentage = 0;

            var p = SystemInformation.PowerStatus;


            foreach (DeviceInformation device in rep)
            {
                try
                {
                    // Create battery object
                    var battery = await Battery.FromIdAsync(device.Id);

                    // Get report
                    var report = battery.GetReport();

                    chargeRate = report.ChargeRateInMilliwatts;
                    MaxBatteryD = report.DesignCapacityInMilliwattHours;
                    MaxBatteryC = report.FullChargeCapacityInMilliwattHours;
                    RemainingBattery = report.RemainingCapacityInMilliwattHours;


                    BatteryPercentage = Math.Round((decimal)RemainingBattery / (decimal)MaxBatteryC * 10000, MidpointRounding.AwayFromZero) / 100m;
                    chargepercentage = Math.Round((decimal)chargeRate / (decimal)MaxBatteryC * 10000, MidpointRounding.AwayFromZero) / 100m;
                    if (chargeRate > 0)
                    {
                        charge = Math.Round((decimal)(MaxBatteryC - RemainingBattery) / (decimal)chargeRate * 100m, MidpointRounding.AwayFromZero) / 100m;
                    }
                    else if (chargeRate < 0)
                    {
                        charge = Math.Round((decimal)RemainingBattery / (decimal)chargeRate * 100m, MidpointRounding.AwayFromZero) / 100m;
                    }



                    // Update UI

                }
                catch (Exception e)
                {
                    /* Add error handling, as applicable */
                    Trace.TraceError(e.ToString());
                }
            }

            #region [PCパフォーマンス]



            /*
            pcList.ForEach((x) =>
            {
                //計算された値を取得し、表示する
                Console.WriteLine(x.CategoryName + " / " + x.CounterName + "：" + x.NextValue());
            });*/

            /*
            try
            {
                label83.Text = "CPU使用率 ：" + pcList[0].NextValue().ToString("##0.00") + "%";
                label84.Text = "ネットワーク通信量 (" + pcList[1].InstanceName + ")：" + pcList[1].NextValue().ToString("##0.00") + "B/s";
                label85.Text = "ディスク使用率：" + pcList[2].NextValue().ToString("##0.00") + "%";
                label86.Text = "残りメモリ残量：" + pcList[3].NextValue().ToString("##0.00") + "MB";
                label87.Text = "ワーキングセット：" + (pcList[4].NextValue() / 1024 / 1024).ToString("####0.0000") + "MB";
                label88.Text = "ディスクR/W速度：" + (pcList[5].NextValue() / 1000 / 1000).ToString("####0.0000") + "MB/s";
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            */
            #endregion

            var batteryHealth = Math.Round((decimal)MaxBatteryC / (decimal)MaxBatteryD * 10000, MidpointRounding.AwayFromZero) / 100m;
            label24.Text = "バッテリー残量: " + BatteryPercentage.ToString("##0.00") + "%";
            label25.Text = "充電レート: " + chargeRate.ToString() + "mW" + " | " + charge.ToString("##0.00") + "h | " + chargepercentage.ToString("##0.00") + "%/h";
            label26.Text = "設計上最大充電容量: " + MaxBatteryD.ToString() + "mWh";
            label27.Text = "現在の最大充電容量: " + MaxBatteryC.ToString() + "mWh" + " | 設計容量の: " + batteryHealth.ToString("##0.00") + "%";
            label28.Text = "現在の残り充電容量: " + RemainingBattery.ToString() + "mWh";
            progressBar1.Value = (int)(BatteryPercentage * 100m);
            toolStripStatusLabel1.Text = "バッテリー残量: " + BatteryPercentage.ToString("##0.00") + "%";

            label24.Update();
            label25.Update();
            label26.Update();
            label27.Update();
            label28.Update();
            progressBar1.Update();

            logger2.Debug("BatteryMonitor: PowerLineStatus: " + SystemInformation.PowerStatus.PowerLineStatus + ", BatteryChargeStatus: " + SystemInformation.PowerStatus.BatteryChargeStatus + ", Percentage:" + BatteryPercentage.ToString("##0.00") + "%, CurrentMaxCapacity: " + MaxBatteryC.ToString() + "mWh");
            //toolStripStatusLabel1.Update();


            int NotifyModeSelector = 0;

            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    NotifyModeSelector = 999999;
                    break;
                case 1:
                    NotifyModeSelector = 1;
                    break;
                case 2:
                    NotifyModeSelector = 0;
                    break;
                case 3:
                    NotifyModeSelector = -100;
                    break;
                case 4:
                    NotifyModeSelector = -250;
                    break;
                case 5:
                    NotifyModeSelector = -500;
                    break;
                case 6:
                    NotifyModeSelector = -1000;
                    break;
                case 7:
                    NotifyModeSelector = -3000;
                    break;
                case 8:
                    NotifyModeSelector = -999999;
                    break;

            }
            try
            {
                var check = (chargeRate / previousChargeRate);
                if (!isExecuted)
                {
                    isExecuted = true;
                }
                else if ((check < 0 && chargeRate < NotifyModeSelector))
                {
                    previousChargeRate = chargeRate;
                    if (chargeRate > 0)
                    {
                        MessageBox.Show("充電が開始されました。", "Notify", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        previousChargeRate = 1;
                    }
                    else if (previousChargeRate == 0)
                    {
                        previousChargeRate = 1;
                    }
                    else
                    {
                        MessageBox.Show("充電器が抜かれたか、バッテリー保護が作動したか、あるいは何らかの理由でバッテリー充電が中止されました。", "Notify", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        previousChargeRate = -1;
                    }

                }

                previousChargeRate = chargeRate;
            }
            catch (Exception e)
            {
                Trace.TraceError("なんかのエラー" + e.ToString());
            }

        }

        private void button17_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Navigate("https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/Top.aspx");
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    NotifyModeSelector = 999999;
                    break;
                case 1:
                    NotifyModeSelector = 1;
                    break;
                case 2:
                    NotifyModeSelector = 0;
                    break;
                case 3:
                    NotifyModeSelector = -100;
                    break;
                case 4:
                    NotifyModeSelector = -250;
                    break;
                case 5:
                    NotifyModeSelector = -500;
                    break;
                case 6:
                    NotifyModeSelector = -1000;
                    break;
                case 7:
                    NotifyModeSelector = -3000;
                    break;
                case 8:
                    NotifyModeSelector = -999999;
                    break;

            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            LinkExecute("https://acanthus.cis.kanazawa-u.ac.jp/base/message/index");
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (form4 == null)
            {
                // フォームを生成して、表示します
                form4 = new Form4();
                form4.Show();
            }

            if (form4.IsDisposed == true)
            {
                form4 = new Form4();
                form4.Show();
            }

            else
            {
                form4.WindowState = FormWindowState.Normal;
                form4.Activate();
            }
        }

        private void label83_Click(object sender, EventArgs e)
        {

        }

        public static List<string> UserLink = new List<string>();
        public static List<string> UserLinkLabel = new List<string>();
        private void button24_Click(object sender, EventArgs e)
        {
            if (textBox5.Text != "" && textBox6.Text != "")
            {
                UserLink.Add(textBox6.Text);
                UserLinkLabel.Add(textBox5.Text);
                listBox2.Items.Add(textBox5.Text);
                SaveLink();
                textBox5.Text = "";
                textBox6.Text = "";
                label88.Text = "";
            }
            else
            {
                label88.Text = "エラー: URLとラベルの両方を入力してください。";
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {

        }
        public void SaveLink()
        {
            SetLMSData(false, true);
        }

        private void listBox2_Action(object sender, EventArgs e)
        {
            LinkExecute(UserLink[listBox2.SelectedIndex]);
            label88.Text = "";
        }

        private void button26_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("リストの全データを削除します。この操作はもとに戻せません。\n \n続行しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                listBox2.Items.Clear();
                UserLink.Clear();
                UserLinkLabel.Clear();
                SaveLink();
            }
            label88.Text = "";
        }

        private void button27_Click(object sender, EventArgs e)
        {
            //項目を上に移動する処理。

            //一時的に保存する変数を作る
            string padding = "";

            //選択された項目を代入
            padding = UserLink[listBox2.SelectedIndex];

            if (listBox2.SelectedIndex != 0) //バグ回避用。一番上の項目で処理する際のIndexOutOfRangeExceptionを回避するための処理。
            {
                UserLink[listBox2.SelectedIndex] = UserLink[listBox2.SelectedIndex - 1];
                UserLink[listBox2.SelectedIndex - 1] = padding;

                padding = UserLinkLabel[listBox2.SelectedIndex];
                UserLinkLabel[listBox2.SelectedIndex] = UserLinkLabel[listBox2.SelectedIndex - 1];
                UserLinkLabel[listBox2.SelectedIndex - 1] = padding;

                // 元の場所から削除して1つ上の位置に挿入
                padding = listBox2.Items[listBox2.SelectedIndex].ToString();

                int x = listBox2.SelectedIndex;
                listBox2.Items.RemoveAt(x);
                listBox2.Items.Insert(x - 1, padding);

                // 移動後も項目が選択された状態を維持
                listBox2.SelectedIndex = x - 1;
            }
            SetLMSData(false, true);
            label88.Text = "";

        }

        private void button28_Click(object sender, EventArgs e)
        {
            //項目を下に移動する処理。

            //一時的に保存する変数を作る
            string padding = "";

            //選択された項目を代入
            padding = UserLink[listBox2.SelectedIndex];

            if (listBox2.SelectedIndex != listBox2.Items.Count - 1) //バグ回避用。一番下の項目で処理する際のIndexOutOfRangeExceptionを回避するための処理。
            {
                UserLink[listBox2.SelectedIndex] = UserLink[listBox2.SelectedIndex + 1];
                UserLink[listBox2.SelectedIndex + 1] = padding;

                padding = UserLinkLabel[listBox2.SelectedIndex];
                UserLinkLabel[listBox2.SelectedIndex] = UserLinkLabel[listBox2.SelectedIndex + 1];
                UserLinkLabel[listBox2.SelectedIndex + 1] = padding;

                // 元の場所から削除して1つ下の位置に挿入
                padding = listBox2.Items[listBox2.SelectedIndex].ToString();

                int x = listBox2.SelectedIndex;
                listBox2.Items.RemoveAt(x);
                listBox2.Items.Insert(x + 1, padding);

                // 移動後も項目が選択された状態を維持
                listBox2.SelectedIndex = x + 1;
            }
            SetLMSData(false, true);
            label88.Text = "";
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                textBox5.Text = UserLinkLabel[listBox2.SelectedIndex];
                textBox6.Text = UserLink[listBox2.SelectedIndex];
                label88.Text = "";
            }
            else
            {
                label88.Text = "エラー: 先に編集する項目を選択してください。";
            }

        }

        private void button30_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count > 0 && listBox2.SelectedIndex != -1 && textBox5.Text != "" && textBox6.Text != "")
            {
                UserLink[listBox2.SelectedIndex] = textBox6.Text;
                UserLinkLabel[listBox2.SelectedIndex] = textBox5.Text;
                listBox2.Items[listBox2.SelectedIndex] = textBox5.Text;
                SaveLink();
                label88.Text = "";
            }
            else
            {
                label88.Text = "エラー: URLとラベルの両方を入力してください。";
            }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            listBox2_Action(sender, e);
            label88.Text = "";
        }

        private void button32_Click(object sender, EventArgs e)
        {
            textBox5.Text = "";
            label88.Text = "";
        }

        private void button33_Click(object sender, EventArgs e)
        {
            textBox6.Text = "";
            label88.Text = "";
        }

        private void button34_Click(object sender, EventArgs e)
        {
            textBox5.Text = "";
            textBox6.Text = "";
            label88.Text = "";
        }

        private void button25_Click_1(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                UserLink.RemoveAt(listBox2.SelectedIndex);
                UserLinkLabel.RemoveAt(listBox2.SelectedIndex);
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            }
        }

        private void button35_Click(object sender, EventArgs e)
        {
            SaveLink();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            LinkExecute("https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/ReferResults/Menu.aspx");
        }

        //変数系
        public static decimal GPA;
        public static decimal[] scores = new decimal[7]; //S,A,B,C,不可,トータル値,除外値。除外値は「合格」「認定」のものが含まれる。
        public static int DebugCode4;

        public string script2(string tableId, int row, int cells)
        {
            var script = "function getTableData() {const table = document.getElementById('ctl00_phContents_Results_Gpa1_ctlCreditsGetYear_gv');\r\n\r\n" +
    "// 1. 全ての行をループしてデータを取得\r\n" +
    "for (let i = 0; i < table.rows.length; i++) {" +
    "\r\n    let row = table.rows[i];\r\n" +
    "    for (let j = 0; j < row.cells.length; j++) {\r\n" +
    "        console.log(row.cells[j].innerText); // セルのテキストを表示\r\n" +
    "    }\r\n" +
    "}\r\n\r\n" +
    "// 2. 特定の行（例: 2行目）の特定の列（例: 1列目）を取得\r\n" +
    "const cellValue = table.rows[" + row.ToString() + "].cells[" + cells.ToString() + "].innerText;" +
    "return  JSON.stringify(cellValue); }" +
    "\r\n \r\n getTableData();";
            return script;
        }
        public string script3(int row, int cells)
        {
            var script = "function getTableData() {const table = document.getElementById('ctl00_phContents_Results1_gridResultsRec_rdlGrid_gridList');\r\n\r\n" +
    "// 1. 全ての行をループしてデータを取得\r\n" +
    "for (let i = 0; i < table.rows.length; i++) {" +
    "\r\n    let row = table.rows[i];\r\n" +
    "    for (let j = 0; j < row.cells.length; j++) {\r\n" +
    "        console.log(row.cells[j].innerText); // セルのテキストを表示\r\n" +
    "    }\r\n" +
    "}\r\n\r\n" +
    "// 2. 特定の行（例: 2行目）の特定の列（例: 1列目）を取得\r\n" +
    "const cellValue = table.rows[" + row.ToString() + "].cells[" + cells.ToString() + "].innerText;" +
    "return  JSON.stringify(cellValue); }" +
    "\r\n \r\n getTableData();";
            return script;
        }



        private async void button36_Click(object sender, EventArgs e)
        {
            scores = new decimal[7];
            string[] Linkstr = new string[100];
            //GPAお手軽計算器 v1.0.0
            if (webView21.CoreWebView2.Source.ToString() == "https://eduweb.sta.kanazawa-u.ac.jp/Portal/StudentApp/ReferResults/Results.aspx")
            {/*
                try 
                {
                    for (int i = 0;i < 13744;i++) 
                    {
                        if (System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", i, 0))).Replace("\"", "") != null ) 
                        {
                            if (System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", i, 6))).Replace("\"", "") != null)
                            {
                                string str56 = System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", i, 6))).Replace("\"", "");
                                int data = int.Parse(System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", i, 5))).Replace("\"", ""));
                                switch (str56) 
                                {
                                    case "S":
                                        scores[0] += data;
                                        break;
                                    case "A":
                                        scores[1] += data;
                                        break;
                                    case "B":
                                        scores[2] += data;
                                        break;
                                    case "C":
                                        scores[3] += data;
                                        break;
                                    case "不可":
                                        scores[4] += data;
                                        break;

                                }
                            }
                        }
                        else 
                        {
                            break;
                        }
                    }
                }
                catch { }
                //計算・描画
                scores[5] = scores[0] + scores[1] + scores[2] + scores[3] + scores[4];
                GPA = Math.Round((scores[0] * 4 + scores[1] * 3 + scores[2] * 2 + scores[3] * 1) / (decimal)scores[5], 3, MidpointRounding.AwayFromZero);

                string str = "GPA: " + GPA.ToString("##0.000") + "\n" +
                    "S: " + scores[0] + "単位\n" +
                    "A: " + scores[1] + "単位\n" +
                    "B: " + scores[2] + "単位\n" +
                    "C: " + scores[3] + "単位\n";

                描画(str);
                */
            }
            else
            {
                //テーブル情報の取得
                scores = new decimal[7];

                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                scores[i] += System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", j + 2, i + 1))).Replace("\"", "") != " " ? decimal.Parse(System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", j + 2, i + 1))).Replace("\"", "")) : 0;
                                Trace.TraceInformation(System.Text.Json.JsonSerializer.Deserialize<string>(await webView21.ExecuteScriptAsync(script2("ctl00_phContents_ucResultList_grvResultList", j + 2, i + 1))).Replace("\"", ""));
                            }
                        }
                        catch
                        { }
                    }
                }
                catch
                {
                    //行終わりのため、例外処理を用いてループを抜ける。
                    Trace.TraceInformation("テーブルの末尾に達しました。");
                }


                scores[4] = numericUpDown2.Value;

                //計算・描画
                scores[5] = scores[0] + scores[1] + scores[2] + scores[3] + scores[4];
                GPA = Math.Round((scores[0] * 4 + scores[1] * 3 + scores[2] * 2 + scores[3] * 1) / (decimal)scores[5], 3, MidpointRounding.AwayFromZero);

                string str = "GPA: " + GPA.ToString("##0.000") + "\n" +
                    "S: " + scores[0] + "単位\n" +
                    "A: " + scores[1] + "単位\n" +
                    "B: " + scores[2] + "単位\n" +
                    "C: " + scores[3] + "単位\n";

                描画(str);
                logger.Info("WebBeta: GPAを計算しました。");

            }
        }

        public void 描画(string str)
        {
            label89.Text = str;
            label89.Update();
        }

        //Homework Manager ver 0.5.0
        private void 設定ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            button7_Click(sender, e);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        public struct HomeworkData(string title, string URL, string Description, DateTime Deadline, string subject)
        {
            public string title = title; //課題名。(必須項目)
            public string URL = URL;     //LMSリンクでもここに貼っとけ。参考リンクでも良い。(任意。入力されない場合はstr = "") (教科名によっては自動入力できる)
            public string Description = Description; //課題の説明をここに入れておく。(任意。入力されない場合はstr = "")
            public DateTime Deadline = Deadline;　　　//期日を入れておく。(必須項目)
            public string subject = subject; //教科名か、あるいはなんかの団体のタスクであるならそういうラベルでも良い。(任意。入力されない場合はstr = "")
        }

        public static List<HomeworkData> HomeworkList = new List<HomeworkData>();

        private void button47_Click(object sender, EventArgs e)
        {
            //課題を追加する処理
            if (textBox8.Text != "")
            {
                HomeworkList.Add(new HomeworkData(textBox8.Text, textBox7.Text, textBox9.Text, dateTimePicker1.Value, textBox10.Text));
                listBox3.Items.Add(textBox8.Text + " | " + dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm"));
            }
            SetLMSData(false, true);
            button37_Click(sender, e);
            logger.Info("HomeworkManager: 課題は正常に追加されました。");
        }

        private void button46_Click(object sender, EventArgs e)
        {
            textBox8.Text = HomeworkList[listBox3.SelectedIndex].title;
            textBox7.Text = HomeworkList[listBox3.SelectedIndex].URL;
            textBox9.Text = HomeworkList[listBox3.SelectedIndex].Description;
            dateTimePicker1.Value = HomeworkList[listBox3.SelectedIndex].Deadline;
            textBox10.Text = HomeworkList[listBox3.SelectedIndex].subject;
        }

        private void button45_Click(object sender, EventArgs e)
        {
            HomeworkList[listBox3.SelectedIndex] = new HomeworkData(textBox8.Text, textBox7.Text, textBox9.Text, dateTimePicker1.Value, textBox10.Text);
            listBox3.Items[listBox3.SelectedIndex] = textBox8.Text + " | " + dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm");
            SetLMSData(false, true);
            button37_Click(sender, e);
            logger.Info("HomeworkManager: 課題データは正常に修正されました。");
        }

        private void button38_Click(object sender, EventArgs e)
        {
            string padding = "";
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    string str = textBox10.Text;

                    if (str == DataList[i, j].Title)
                    {
                        textBox7.Text = DataList[i, j].URL;
                        textBox10.Text = str;
                        padding = "found";
                    }
                    else
                    {
                        //正規表現を使用して、英数字を全角に変換する
                        string str2 = Regex.Replace(str, @"[a-zA-Z0-9０-９ａ-ｚＡ－Ｚ]", match =>
                        {
                            // マッチした英数字を全角に変換
                            return Strings.StrConv(match.Value, VbStrConv.Wide, 0x411);
                        });
                        if (str2 == DataList[i, j].Title)
                        {
                            textBox7.Text = DataList[i, j].URL;
                            textBox10.Text = str2;
                            padding = "found";
                        }
                        else
                        {
                            string str3 = str.Replace("1", "Ⅰ");
                            str3 = str3.Replace("2", "Ⅱ");
                            str3 = str3.Replace("3", "Ⅲ");
                            str3 = str3.Replace("4", "Ⅳ");
                            str3 = str3.Replace("5", "Ⅴ");
                            str3 = str3.Replace("6", "Ⅵ");
                            if (str3 == DataList[i, j].Title)
                            {
                                textBox7.Text = DataList[i, j].URL;
                                textBox10.Text = str3;
                                padding = "found";
                            }

                        }
                    }


                }
            }
            if (padding != "found")
            {
                MessageBox.Show("教科名で一致するものが見つかりませんでした。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.Warn("HomeworkManager: 教科名で一致するものが見つかりませんでした。");
            }

        }

        public void HomeworkListSetup(IniData data)
        {

        }

        private void button37_Click(object sender, EventArgs e)
        {
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
        }

        private void button43_Click(object sender, EventArgs e)
        {
            HomeworkList.RemoveAt(listBox3.SelectedIndex);
            listBox3.Items.RemoveAt(listBox3.SelectedIndex);
            SetLMSData(false, true);
            logger.Info("HomeworkManager: 課題が削除されました。");
        }

        private void 設定ファイルを開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists("Config.ini"))
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo("Config.ini") { UseShellExecute = true });

            }
        }

        private void button44_Click(object sender, EventArgs e)
        {
            //課題を全て削除する処理
            listBox3.Items.Clear();
            HomeworkList.Clear();
            SetLMSData(false, true);
            logger.Warn("HomeworkManager: すべての課題が削除されました。");
        }

        private void button40_Click(object sender, EventArgs e)
        {
            SetLMSData(false, true);
        }

        private void button49_Click(object sender, EventArgs e)
        {
            LinkExecute(HomeworkList[listBox3.SelectedIndex].URL);
        }

        private void button41_Click(object sender, EventArgs e)
        {
            //課題を完了する処理。完了フラグを立てたものは一覧から削除され、CompTask.txtに追記される。
            if (MessageBox.Show("この課題を完了します。\n完了するとデータが一覧から削除されます。\n続行しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //CompTask.txtの存在確認
                if (!System.IO.File.Exists("CompTask.txt"))
                {
                    System.IO.File.WriteAllText("CompTask.txt", "Homework Manager - Completed Tasks Here: \n");
                }
                //文字列の書き込み
                string str = "\n <Completed> \n Title = " + HomeworkList[listBox3.SelectedIndex].title + "\nLink = " + HomeworkList[listBox3.SelectedIndex].URL + "\nDescription = " + HomeworkList[listBox3.SelectedIndex].Description + "\nDeadline = " + HomeworkList[listBox3.SelectedIndex].Deadline.ToString("yyyy-MM-dd HH:mm") + "\nSubject = " + HomeworkList[listBox3.SelectedIndex].subject;
                System.IO.File.AppendAllText("CompTask.txt", str);
                //一覧から削除
                button43_Click(sender, e);
                logger.Info("HomeworkManager: 課題完了措置が取られました。詳細はComptask.txtの末尾を参照してください。");
            }
        }

        private void button42_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex > 0) //IndexOutOfRangeExceptionを回避するための処理。
            {
                //課題を上に移動する処理

                //データを吸い出す
                string str = listBox3.Items[listBox3.SelectedIndex].ToString();
                HomeworkData data = HomeworkList[listBox3.SelectedIndex];
                int x = listBox3.SelectedIndex;

                //一旦一覧から消去
                HomeworkList.RemoveAt(x);
                listBox3.Items.RemoveAt(x);

                //上に挿入
                listBox3.Items.Insert(x - 1, str);
                HomeworkList.Insert(x - 1, data);

                //選択箇所も移行
                listBox3.SelectedIndex = x - 1;
            }
        }

        private void button48_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex < listBox3.Items.Count - 1) //IndexOutOfRangeExceptionを回避するための処理。
            {
                //課題を下に移動する処理

                //データを吸い出す
                string str = listBox3.Items[listBox3.SelectedIndex].ToString();
                HomeworkData data = HomeworkList[listBox3.SelectedIndex];
                int x = listBox3.SelectedIndex;

                //一旦一覧から消去
                HomeworkList.RemoveAt(x);
                listBox3.Items.RemoveAt(x);

                //下に挿入
                listBox3.Items.Insert(x + 1, str);
                HomeworkList.Insert(x + 1, data);

                //選択箇所も移行
                listBox3.SelectedIndex = x + 1;
            }
        }

        private void button50_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex >= 0)
            {
                MessageBox.Show(HomeworkList[listBox3.SelectedIndex].Description, HomeworkList[listBox3.SelectedIndex].title + "の詳細", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
