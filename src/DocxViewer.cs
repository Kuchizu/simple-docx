using System;
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
        var form = new Form { Text = "DOCX Viewer", StartPosition = FormStartPosition.CenterScreen,
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
            if (args.Length > 0)
            {
                var path = Path.GetFullPath(args[0]);
                core.SetVirtualHostNameToFolderMapping("doc.local", Path.GetDirectoryName(path), CoreWebView2HostResourceAccessKind.Allow);
                url += "?f=" + Uri.EscapeDataString(Path.GetFileName(path));
            }
            core.DocumentTitleChanged += (s2, e2) => form.Text = core.DocumentTitle;
            core.WebMessageReceived += (s2, e2) => form.Close();
            core.Navigate(url);
        };
        Application.Run(form);
    }
}
