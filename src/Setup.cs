using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

static class Setup
{
    const string App = "DocxViewer", ProgId = "DocxViewer.docx", Title = "DOCX Viewer";
    const string Docx = @"Software\Classes\.docx", Own = @"Software\DocxViewer";
    const string Uninst = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\DocxViewer";
    static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", App);
    static readonly string Exe = Path.Combine(Dir, App + ".exe");
    static readonly string Uninstaller = Path.Combine(Dir, "uninstall.exe");

    [DllImport("shell32.dll")]
    static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "/uninstall")
            {
                Uninstall();
                MessageBox.Show("DOCX Viewer удалён.", Title);
                Process.Start(new ProcessStartInfo("cmd.exe", "/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"" + Dir + "\"")
                    { CreateNoWindow = true, UseShellExecute = false });
            }
            else if (MessageBox.Show("Установить DOCX Viewer и открывать им файлы .docx?", Title, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Install();
        }
        catch (Exception e)
        {
            MessageBox.Show("Ошибка: " + e.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static object Get(string key, string name)
    {
        using (var k = Registry.CurrentUser.OpenSubKey(key))
            return k == null ? null : k.GetValue(name);
    }

    static void Set(string key, string name, object value)
    {
        using (var k = Registry.CurrentUser.CreateSubKey(key))
            k.SetValue(name, value);
    }

    static void DeleteValue(string key, string name)
    {
        using (var k = Registry.CurrentUser.OpenSubKey(key, true))
            if (k != null) k.DeleteValue(name, false);
    }

    static void Install()
    {
        Directory.CreateDirectory(Dir);
        var asm = Assembly.GetExecutingAssembly();
        foreach (var name in asm.GetManifestResourceNames())
            using (var src = asm.GetManifestResourceStream(name))
            using (var dst = File.Create(Path.Combine(Dir, name)))
                src.CopyTo(dst);
        File.Copy(asm.Location, Uninstaller, true);

        var prev = Get(Docx, "") as string;
        if (prev != null && prev != ProgId) Set(Own, "PrevDefault", prev);
        Set(@"Software\Classes\" + ProgId, "", "Документ Word");
        Set(@"Software\Classes\" + ProgId + @"\DefaultIcon", "", Exe + ",0");
        Set(@"Software\Classes\" + ProgId + @"\shell\open\command", "", "\"" + Exe + "\" \"%1\"");
        Set(Docx, "", ProgId);
        Set(Docx + @"\OpenWithProgids", ProgId, "");
        Set(Own + @"\Capabilities", "ApplicationName", Title);
        Set(Own + @"\Capabilities", "ApplicationDescription", "Быстрый просмотр документов Word");
        Set(Own + @"\Capabilities\FileAssociations", ".docx", ProgId);
        Set(@"Software\RegisteredApplications", App, Own + @"\Capabilities");
        Set(Uninst, "DisplayName", Title);
        Set(Uninst, "DisplayIcon", Exe);
        Set(Uninst, "Publisher", "Kuchizu");
        Set(Uninst, "InstallLocation", Dir);
        Set(Uninst, "UninstallString", "\"" + Uninstaller + "\" /uninstall");
        Set(Uninst, "NoModify", 1);
        Set(Uninst, "NoRepair", 1);
        SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);

        var choice = Get(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.docx\UserChoice", "ProgId") as string;
        if (choice == null || choice == ProgId)
        {
            MessageBox.Show("Готово. Файлы .docx теперь открываются в DOCX Viewer.", Title);
            return;
        }
        MessageBox.Show("Готово. Windows не разрешает программам самим менять приложение по умолчанию: " +
                        "в открывшихся настройках выберите DOCX Viewer для .docx.", Title);
        Process.Start("ms-settings:defaultapps?registeredAppUser=" + App);
    }

    static void Uninstall()
    {
        if ((Get(Docx, "") as string) == ProgId)
        {
            var prev = Get(Own, "PrevDefault") as string;
            if (prev != null) Set(Docx, "", prev);
            else DeleteValue(Docx, "");
        }
        DeleteValue(Docx + @"\OpenWithProgids", ProgId);
        DeleteValue(@"Software\RegisteredApplications", App);
        foreach (var key in new[] { @"Software\Classes\" + ProgId, Own, Uninst })
            Registry.CurrentUser.DeleteSubKeyTree(key, false);
        SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
    }
}
