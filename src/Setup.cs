using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

static class Setup
{
    const string App = "DocxViewer", ProgId = "DocxViewer.docx", Title = "DOCX Viewer", Version = "1.3.0";
    const string Docx = @"Software\Classes\.docx", Own = @"Software\DocxViewer";
    const string Uninst = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\DocxViewer";
    static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", App);
    static readonly string Exe = Path.Combine(Dir, App + ".exe");
    static readonly string Uninstaller = Path.Combine(Dir, "uninstall.exe");

    [DllImport("shell32.dll")]
    static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    static bool silent;

    static string T(string ru, string en)
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? ru : en;
    }

    static void Say(string text, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (!silent) MessageBox.Show(text, Title, MessageBoxButtons.OK, icon);
    }

    [STAThread]
    static int Main(string[] args)
    {
        silent = Array.IndexOf(args, "/silent") >= 0;
        try
        {
            if (Array.IndexOf(args, "/uninstall") >= 0)
            {
                Uninstall();
                Say(T("DOCX Viewer удалён.", "DOCX Viewer has been removed."));
                Process.Start(new ProcessStartInfo("cmd.exe", "/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"" + Dir + "\"")
                    { CreateNoWindow = true, UseShellExecute = false });
            }
            else if (silent || MessageBox.Show(T("Установить DOCX Viewer и открывать им файлы .docx?", "Install DOCX Viewer and open .docx files with it?"),
                Title, MessageBoxButtons.OKCancel) == DialogResult.OK)
                Install();
            return 0;
        }
        catch (Exception e)
        {
            Say(T("Ошибка: ", "Error: ") + e.Message, MessageBoxIcon.Error);
            return 1;
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
        Set(@"Software\Classes\" + ProgId, "", T("Документ Word", "Word document"));
        Set(@"Software\Classes\" + ProgId + @"\DefaultIcon", "", Exe + ",0");
        Set(@"Software\Classes\" + ProgId + @"\shell\open\command", "", "\"" + Exe + "\" \"%1\"");
        Set(Docx, "", ProgId);
        Set(Docx + @"\OpenWithProgids", ProgId, "");
        Set(Own + @"\Capabilities", "ApplicationName", Title);
        Set(Own + @"\Capabilities", "ApplicationDescription", T("Быстрый просмотр документов Word", "Quick viewer for Word documents"));
        Set(Own + @"\Capabilities\FileAssociations", ".docx", ProgId);
        Set(@"Software\RegisteredApplications", App, Own + @"\Capabilities");
        Set(Uninst, "DisplayName", Title);
        Set(Uninst, "DisplayIcon", Exe);
        Set(Uninst, "Publisher", "Kuchizu");
        Set(Uninst, "InstallLocation", Dir);
        Set(Uninst, "DisplayVersion", Version);
        Set(Uninst, "UninstallString", "\"" + Uninstaller + "\" /uninstall");
        Set(Uninst, "QuietUninstallString", "\"" + Uninstaller + "\" /uninstall /silent");
        Set(Uninst, "NoModify", 1);
        Set(Uninst, "NoRepair", 1);
        SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);

        var choice = Get(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.docx\UserChoice", "ProgId") as string;
        if (choice == null || choice == ProgId)
        {
            Say(T("Готово. Файлы .docx теперь открываются в DOCX Viewer.", "Done. .docx files now open in DOCX Viewer."));
            return;
        }
        if (silent) return;
        Say(T("Готово. Windows не разрешает программам самим менять приложение по умолчанию: " +
              "в открывшихся настройках выберите DOCX Viewer для .docx.",
              "Done. Windows does not let apps change the default app themselves: " +
              "choose DOCX Viewer for .docx in the Settings page that opens."));
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
