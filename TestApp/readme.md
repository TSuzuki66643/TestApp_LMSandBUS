# TestApp 

趣味のためのツール。Created by T_Suzuki
<br>２つのツールが現状用意されている。

# 1. 簡易式バス発車表示版 - 金沢大学Edition / Simple Bus Departure Panel for Kanazawa University | ver. 3.0.d4
趣味とばかりに勝手に作ったツール。<span style="color:#ffa300">~~あまり役に立たない~~</span>

## 使い方 / How to use
現状「TestApp.exe」となっているファイルを起動するだけ。方面別に次はどの便が出るかが表示される。
なお、何も出なければ(少なくとも直通では)その日運転される便がないことを表す。

なお、3つあるバス停にそれぞれ対応し、プルダウンメニューから変更できる。
>[!NOTE]
>現状の対応バス停は次の通り。
>* 金沢大学
>* 金沢大学中央
>* 金沢大学自然研前
>
>これ以外は非対応。

## 制御パネル / Control systems
制御パネルでは次のことができる。ただし開発中で未実装の機能が含まれる。

### 遅延対応パネル
![figure 1](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.167.png)

トグルスイッチもしくはプルダウンメニューを操作し、遅延時に未到着のバス情報が消滅することを防ぐ。仕様としては、その方向の現在時刻の更新を止める形で実装している。

### 手動設定パネル
![figure 2](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.168.png)
未実装だが、ここに手動で情報を入れることで表示板に反映させるようにする予定。

### デバッグ用ツール
![figure 3](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.169.png)
デバッグ用ツールを設定する。
* デバッグコード表示 (未完成)
* ~~バックグラウンド動作の情報表示~~ (未実装)
* ~~表示変化情報の表示~~ (未実装)

>[!IMPORTANT]
>デバッグコードは問題が発生していると判断したため、近日修正予定である。


## 技術仕様 / Internal systems
バス時刻表は手打ちで入力しており、例えば
```C#
Buslist[94].Details.Add(new Details(940501, 08, 43, false, 94, 0, 0, 0, 52));
```
のように記述する。前から順に
ID、時、分、(特に意味はない)、系統番号、(廃止)、(廃止)、(廃止)、方向コード、(学期休み期間運休の場合ここにtrue)を格納する。

このコードを前から検索し、現在時刻より後で最も近いものから3つを抽出する。

休日判定は曜日からの他、HolidayJpを用いて祝日を判定し、休日とした。

## 更新記録 / Developping Logs
v1.0.00 実装<br>
v1.0.01 元のラベルテキストが残る不具合を修正。<br>
v1.0.02 金大にある3つの停留所に対応。<br>
v1.1.00 朝4時にならないとその日の時刻表が出ないようになったのと、終電情報が然るべきときに出るようにした。<br>
v1.2.00 1分ごとに更新するようになった。<br>
v2.0.β1 デザイン更新、学期休みダイヤ対応、最終表示仮実装。<br>
v2.0.00 休日ダイヤ対応（判定にはHolidayJpを採用）、その日の時刻表を出す時間を2時~に緩和。<br>
v2.0.01 UI改善、分ごとの更新に問題があったため修正。<br>
v3.0.d1 制御フォームの生成、Form1の変数とのリンク完了。<br>
v3.0.d2 制御システム仮完成<br>
v3.0.d3 仮置きアイコンを用意した。<br>
v3.0.d4 終電情報廃止、UI改善を実施。<br>
        ダイヤ改正に合わせて、時刻表定義部分を変更:<br>
    平日93: 増発:  8:32(52), 16:35(52)<br>
    平日94: 増発: 12:50(05), 14:40(55)<br>
    　　    　 変更: 16:55→17:00(52)<br>
    　　    　 廃止: 21:05(05)<br>
    平日53: 変更:  8:30→ 8:40(53)
, 13:05→13:30(53)
, 14:22→14:20(53)
, 19:25→19:20(53)<br>
    平日99: (変更なし)<br>
<br>
    休日93: 変更:13:45→13:50(52)
, 14:35→14:40(52)<br>
    休日94: (変更なし)<br>
    休日53: 変更: 16:55→16:30(53)
, 18:55→19:00(53)<br>
<br>
Yahoo!乗換案内がダイヤ改正に対応済みであることを確認し、照らし合わせて実装。<br>
v3.0.5 遅延時の表示を仮実装。出発時刻の代わりに遅延時間(単位:分)が、時間tに対しt>=5のとき表示される。<span style=color:#ffaaaa>例えば下の画像のような地獄の状況も作れる。</span>
![figure 5](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.172.png)
なんなら運転取りやめの処理まで作ってしまった。現在はコンパイル前に指定しなければならないが。
![figure 6](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.173.png)

# 2. LMSジャンパー / LMS Jumper
<span style=color:#44ccff>#LMS改悪やめろ</span><br>
2026年3月に行われたメンテナンスにより、LMS
コースページから各教科のLMSへのアクセスが不可能になり、アクセス手段が次のとおりとなった。
```
学務情報サービス→履修・成績情報→履修時間割表→各教科を選択→LMSへジャンプ
```
これは非常に<u>手間がかかる</u>上、時期によっては<span style=color:#aaaaaa>(~~ただでさえ履修登録で混み合うため~~)<span style=color:#ff7777>学務情報サービスへのアクセス集中によってアクセス<u>**そのもの**</u>が困難な状況に陥った<span style=color:#ffffff>。<br>そこで、LMSへジャンプするURLを抽出し、そこからリンクを踏むと直接LMSへアクセスできることから、このシステムを用意した。先駆者様のツール(LMS jumper | https://univ.ichihai.dev/LMS-jumper/)を参考にC#を用いたWindows用ソフトの開発を目標とした。

## 内部仕様 / Internal systems

時間割を次のコードで定義する。
```C#
DataList[3, 0] = (new LMSData(3, 0, "(なし)", "-1"));
```
ここでDataListのindexは[曜日, 時限]であり、内部データは(曜日,時限,講義名,リンクとなるURL)である。
<br>そのうちGUIで設定し、.iniにデータを保管するよう実装する予定。

>[!NOTE]
>URLは次の通り取得する。
>* 各教科で「選択代行機能」にアクセス。図は「教職化学」。
>![figure 4](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.170.png)
>* LMSの「ジャンプ」を右クリックし、URLをコピーする。画像はchromeだが、「リンクのアドレスをコピー」でコピーできる。
>![figure 5](C:\Users\琢真\source\repos\TestApp\TestApp\screenshot.171.png)




定義したデータを用いて、リンク付きのUIを構築し、デフォルトでは引数としてURLを渡してChromeを実行する仕組みである。FireFox等でも同様にできると思われるが、変更する場合はブラウザーごとにこの辺の仕様を確認し、実行ファイルのパスを特定して、ブラウザのパス部分を置き換えること。
```C#
 System.Diagnostics.Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "--disable-features=ExtensionManifestV2Unsupported,ExtensionManifestV2Disabled" + DataList[Weekday, Time].URL);
```
上のコードの
```C#
"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
```
を書き換える。
```C#
"--disable-features=ExtensionManifestV2Unsupported,ExtensionManifestV2Disabled"
```
はChrome以外の場合消すこと。
<span style=color:#aaaaaa>~~もしくはChromeを使おう~~<span>

## 更新記録 / Developping Logs
v1.0.0 実装<br>
v1.1.0 Chromeでv2拡張機能を使うために必要なコマンドライン引数を追加。

# 動作環境 / Running Environment
OS: Windows<br>
Framework: .NET 8.0 (ランタイムパッケージを導入しましょう)<br>
TargetOSVersion: 10.0.22600.0<br>
SupportOSVersion: 10.0.17763.0 (Windows 10 初期バージョン)
