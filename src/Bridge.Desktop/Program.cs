using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bridge.Desktop;

static class Program
{
    private static NotifyIcon? _notifyIcon;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(2) };
    private static System.Windows.Forms.Timer? _pollTimer;
    private static string _bridgeBaseUrl = "http://127.0.0.1:8080";

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        _notifyIcon = new NotifyIcon
        {
            Text = "Universal Local Signing Bridge (Checking...)",
            Visible = true,
            Icon = CreateColoredIcon(Color.Orange)
        };

        var contextMenu = new ContextMenuStrip();
        var titleItem = new ToolStripMenuItem("Universal Signing Bridge") { Enabled = false, Font = new Font(Control.DefaultFont, FontStyle.Bold) };
        var statusItem = new ToolStripMenuItem("Status: Checking...") { Enabled = false };
        var sep1 = new ToolStripSeparator();

        var openDashboardItem = new ToolStripMenuItem("Open Dashboard", null, (_, _) => OpenBrowser(_bridgeBaseUrl));
        var diagnosticsItem = new ToolStripMenuItem("Browser Diagnostics", null, (_, _) => OpenBrowser($"{_bridgeBaseUrl}/diagnostics"));
        var certificatesItem = new ToolStripMenuItem("Certificates", null, (_, _) => OpenBrowser($"{_bridgeBaseUrl}/certificates"));
        var restartItem = new ToolStripMenuItem("Restart Service", null, (_, _) => RestartService());
        var logsItem = new ToolStripMenuItem("Logs", null, (_, _) => OpenBrowser($"{_bridgeBaseUrl}/audit"));
        var settingsItem = new ToolStripMenuItem("Settings", null, (_, _) => OpenBrowser($"{_bridgeBaseUrl}/settings"));
        var sep2 = new ToolStripSeparator();
        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) =>
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        });

        contextMenu.Items.AddRange(new ToolStripItem[]
        {
            titleItem,
            statusItem,
            sep1,
            openDashboardItem,
            diagnosticsItem,
            certificatesItem,
            restartItem,
            logsItem,
            settingsItem,
            sep2,
            exitItem
        });

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => OpenBrowser(_bridgeBaseUrl);

        _pollTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        _pollTimer.Tick += async (_, _) =>
        {
            await CheckHealthAsync(statusItem);
        };
        _pollTimer.Start();

        _ = CheckHealthAsync(statusItem);

        Application.Run();
    }

    private static async Task CheckHealthAsync(ToolStripMenuItem statusItem)
    {
        if (_notifyIcon == null) return;

        try
        {
            var res = await _http.GetAsync($"{_bridgeBaseUrl}/health");
            if (res.IsSuccessStatusCode)
            {
                _notifyIcon.Icon = CreateColoredIcon(Color.LimeGreen);
                _notifyIcon.Text = "Universal Signing Bridge: Ready";
                statusItem.Text = "Status: Ready (127.0.0.1:8080)";
            }
            else
            {
                _notifyIcon.Icon = CreateColoredIcon(Color.Gold);
                _notifyIcon.Text = "Universal Signing Bridge: Degraded";
                statusItem.Text = "Status: Degraded (HTTP " + (int)res.StatusCode + ")";
            }
        }
        catch
        {
            _notifyIcon.Icon = CreateColoredIcon(Color.Crimson);
            _notifyIcon.Text = "Universal Signing Bridge: Stopped";
            statusItem.Text = "Status: Stopped / Unreachable";
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private static void RestartService()
    {
        MessageBox.Show("Restarting Local Signing Bridge...", "Universal Bridge", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static Icon CreateColoredIcon(Color color)
    {
        using var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1, 1, 14, 14);
            using var borderPen = new Pen(Color.FromArgb(180, 255, 255, 255), 1.5f);
            g.DrawEllipse(borderPen, 1, 1, 14, 14);
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
