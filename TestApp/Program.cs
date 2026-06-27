using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;

namespace TestApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Trace.TraceInformation("Initialize操作完了");
            try
            {
                string targetFontName = "Mgen+ 2p regular";
                string SystemFontName = SystemFonts.MenuFont.Name;
                // インストールされているフォントを取得
                InstalledFontCollection ifc = new InstalledFontCollection();

                // フォント名が存在するか判定（大文字・小文字を区別しない）
                bool isExist = ifc.Families.Any(f => f.Name.ToLower() == targetFontName.ToLower());
                bool isExist2 = ifc.Families.Any(f => f.Name.ToLower() == SystemFontName.ToLower());
                if (isExist) 
                {
                    Application.SetDefaultFont(new Font("Mgen+ 2p regular", 9F));
                }
                else if (isExist2) 
                {
                    Trace.TraceInformation("フォントが存在しません。現在のシステムフォントを使用します。");
                    Application.SetDefaultFont(new Font(SystemFontName, 9F));
                }
                else if (System.Environment.OSVersion.Version.Major == 10)
                {
                    Trace.TraceInformation("フォントが存在しません。Windows10/11のデフォルトフォントを使用します。");
                    Application.SetDefaultFont(new Font("Yu Gothic UI", 9F));
                }
                else 
                {
                    Trace.TraceInformation("フォントが存在しません。代替フォントを使用します。");
                    Application.SetDefaultFont(new Font("MS UI Gothic", 9F));
                }

                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
           
            
        }

        

        
    }
}