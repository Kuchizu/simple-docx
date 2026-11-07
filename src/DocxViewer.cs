using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

static class DocxViewer
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        var form = new Form { Text = "DOCX Viewer", StartPosition = FormStartPosition.CenterScreen, WindowState = FormWindowState.Maximized,
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) };
        float k;
        using (var g = form.CreateGraphics()) k = g.DpiX / 96f;
        form.Width = (int)(1000 * k);
        form.Height = (int)Math.Min(900 * k, Screen.PrimaryScreen.WorkingArea.Height - 40 * k);
        var web = new WebView2 { Dock = DockStyle.Fill };
        form.Controls.Add(web);
        form.Load += async (s, e) =>
        {
            try
            {
                await web.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нужен Microsoft Edge WebView2 Runtime:\n" + ex.Message, "DOCX Viewer");
                form.Close();
                return;
            }
            var core = web.CoreWebView2;
            core.SetVirtualHostNameToFolderMapping("app.local", AppDomain.CurrentDomain.BaseDirectory, CoreWebView2HostResourceAccessKind.Allow);
            var url = "https://app.local/index.html";
            var docDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (args.Length > 0)
            {
                var path = Path.GetFullPath(args[0]);
                docDir = Path.GetDirectoryName(path);
                core.SetVirtualHostNameToFolderMapping("doc.local", docDir, CoreWebView2HostResourceAccessKind.Allow);
                url += "?f=" + Uri.EscapeDataString(Path.GetFileName(path));
            }
            core.DocumentTitleChanged += (s2, e2) => form.Text = core.DocumentTitle;
            core.WebMessageReceived += async (s2, e2) =>
            {
                var msg = e2.TryGetWebMessageAsString().Split('\n');
                if (msg[0] != "pdf")
                {
                    form.Close();
                    return;
                }
                var dialog = new SaveFileDialog { Filter = "PDF|*.pdf", InitialDirectory = docDir,
                    FileName = Path.GetFileNameWithoutExtension(msg[1]) + ".pdf" };
                if (dialog.ShowDialog(form) != DialogResult.OK)
                {
                    core.PostWebMessageAsString("cancel");
                    return;
                }
                var ok = false;
                try
                {
                    var settings = core.Environment.CreatePrintSettings();
                    double width, height;
                    settings.PageWidth = double.TryParse(msg[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width) ? width / 72 : 8.27;
                    settings.PageHeight = double.TryParse(msg[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height) ? height / 72 : 11.69;
                    settings.MarginTop = settings.MarginBottom = settings.MarginLeft = settings.MarginRight = 0;
                    settings.ShouldPrintBackgrounds = true;
                    settings.ShouldPrintHeaderAndFooter = false;
                    ok = await core.PrintToPdfAsync(dialog.FileName, settings);
                }
                catch (Exception)
                {
                }
                core.PostWebMessageAsString(ok ? "saved" : "failed");
            };
            core.Navigate(url);
        };
        Application.Run(form);
    }
}
