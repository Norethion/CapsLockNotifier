using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CapsLockNotifier
{
    static class Program
    {
        // Constants
        private const int FormWidth = 350;
        private const int FormHeight = 70;
        private const int CornerRadius = 15;
        private const int FormMargin = 20;
        private const string MutexName = "CapsLockNotifier_SingleInstance";
        private const string RegistryKeyName = "CapsLockNotifier";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        // Thread-safe state variables
        private static volatile bool lastCapsLockState;
        private static volatile bool lastNumLockState;
        private static volatile bool lastScrollLockState;

        private static NotifyIcon trayIcon;
        private static bool isStartupEnabled;
        private static CancellationTokenSource cancellationTokenSource;
        private static Mutex singleInstanceMutex;
        private static AppSettings settings;
        private static ToolStripMenuItem startupMenuItem;

        [STAThread]
        static void Main()
        {
            // Single instance check with Named Mutex
            bool createdNew;
            singleInstanceMutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(Localization.Get("AlreadyRunning"), Localization.Get("Info"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Load settings
                settings = AppSettings.Load();

                // Set language
                Localization.SetLanguage(settings.Language);

                // Check initial lock keys state
                lastCapsLockState = Control.IsKeyLocked(Keys.CapsLock);
                lastNumLockState = Control.IsKeyLocked(Keys.NumLock);
                lastScrollLockState = Control.IsKeyLocked(Keys.Scroll);

                // Load startup setting
                isStartupEnabled = CheckStartupSetting();

                // Create CancellationToken
                cancellationTokenSource = new CancellationTokenSource();

                using (trayIcon = new NotifyIcon())
                {
                    UpdateTrayIcon();
                    trayIcon.Visible = true;
                    trayIcon.Text = Localization.Get("TrayTooltip");

                    // Create context menu
                    var contextMenu = new ContextMenuStrip();

                    // Settings menu item
                    var settingsMenuItem = new ToolStripMenuItem(Localization.Get("Settings"));
                    settingsMenuItem.Click += (s, e) => ShowSettingsForm();
                    contextMenu.Items.Add(settingsMenuItem);

                    contextMenu.Items.Add(new ToolStripSeparator());

                    // Start with Windows option
                    startupMenuItem = new ToolStripMenuItem(Localization.Get("StartWithWindows"))
                    {
                        CheckOnClick = true,
                        Checked = isStartupEnabled
                    };
                    startupMenuItem.Click += ToggleStartup;
                    contextMenu.Items.Add(startupMenuItem);

                    contextMenu.Items.Add(new ToolStripSeparator());

                    // Exit menu item
                    contextMenu.Items.Add(Localization.Get("Exit"), null, (s, e) =>
                    {
                        cancellationTokenSource.Cancel();
                        Application.Exit();
                    });

                    trayIcon.ContextMenuStrip = contextMenu;

                    // Left click to show status summary
                    trayIcon.MouseClick += TrayIcon_MouseClick;

                    // Initial notifications
                    ShowInitialNotifications();

                    // Start monitoring thread
                    var monitorThread = new Thread(() => MonitorLockKeys(cancellationTokenSource.Token))
                    {
                        IsBackground = true
                    };
                    monitorThread.Start();

                    Application.Run();
                }
            }
            finally
            {
                cancellationTokenSource?.Dispose();
                singleInstanceMutex?.ReleaseMutex();
                singleInstanceMutex?.Dispose();
            }
        }

        private static void UpdateTrayIcon()
        {
            // Change icon based on CapsLock state
            if (lastCapsLockState)
            {
                // CapsLock on - orange/red icon
                trayIcon.Icon = CreateColoredIcon(Color.OrangeRed);
            }
            else
            {
                // CapsLock off - default icon
                trayIcon.Icon = new Icon(SystemIcons.Information, 40, 40);
            }
        }

        private static Icon CreateColoredIcon(Color color)
        {
            // Create a simple colored icon
            using (var bitmap = new Bitmap(16, 16))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, 1, 1, 14, 14);
                }
                using (var pen = new Pen(Color.White, 1))
                {
                    graphics.DrawEllipse(pen, 1, 1, 14, 14);
                }

                // Draw "A" letter (for CapsLock)
                using (var font = new Font("Arial", 8, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    graphics.DrawString("A", font, brush, new RectangleF(0, 0, 16, 16), sf);
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }

        private static void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Show current status
                var status = GetCurrentLockStatus();
                trayIcon.ShowBalloonTip(2000, Localization.Get("TrayTooltip"), status, ToolTipIcon.Info);
            }
        }

        private static string GetCurrentLockStatus()
        {
            var capsStatus = lastCapsLockState ? Localization.CapsLockOn : Localization.CapsLockOff;
            var numStatus = lastNumLockState ? Localization.NumLockOn : Localization.NumLockOff;
            var scrollStatus = lastScrollLockState ? Localization.ScrollLockOn : Localization.ScrollLockOff;

            return $"{capsStatus}\n{numStatus}\n{scrollStatus}";
        }

        private static void ShowSettingsForm()
        {
            using (var form = new SettingsForm(settings))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Settings saved, update localization
                    Localization.SetLanguage(settings.Language);
                    trayIcon.Text = Localization.Get("TrayTooltip");
                }
            }
        }

        private static bool CheckStartupSetting()
        {
            try
            {
                using (var startupKey = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false))
                {
                    if (startupKey == null)
                        return false;

                    return startupKey.GetValue(RegistryKeyName) != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void ToggleStartup(object sender, EventArgs e)
        {
            try
            {
                using (var startupKey = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    if (startupKey == null)
                    {
                        MessageBox.Show(Localization.Get("RegistryError"),
                            Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var menuItem = sender as ToolStripMenuItem;
                    if (menuItem == null) return;

                    if (menuItem.Checked)
                    {
                        startupKey.SetValue(RegistryKeyName, Application.ExecutablePath);
                        MessageBox.Show(Localization.Get("StartupEnabled"),
                            Localization.Get("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        startupKey.DeleteValue(RegistryKeyName, false);
                        MessageBox.Show(Localization.Get("StartupDisabled"),
                            Localization.Get("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localization.Get("SettingsChangeError") + ex.Message,
                    Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowInitialNotifications()
        {
            if (settings.MonitorCapsLock && lastCapsLockState)
                ShowNotification(Localization.CapsLockOn);

            if (settings.MonitorNumLock && lastNumLockState)
                ShowNotification(Localization.NumLockOn);

            if (settings.MonitorScrollLock && lastScrollLockState)
                ShowNotification(Localization.ScrollLockOn);
        }

        private static void MonitorLockKeys(CancellationToken cancellationToken)
        {
            int pollingInterval = 200;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // CapsLock check
                    if (settings.MonitorCapsLock)
                    {
                        bool currentCapsLockState = Control.IsKeyLocked(Keys.CapsLock);
                        if (currentCapsLockState != lastCapsLockState)
                        {
                            string message = currentCapsLockState ? Localization.CapsLockOn : Localization.CapsLockOff;
                            ShowNotification(message);
                            lastCapsLockState = currentCapsLockState;

                            // Update tray icon
                            if (trayIcon != null)
                            {
                                trayIcon.Invoke(new Action(UpdateTrayIcon));
                            }
                        }
                    }

                    // NumLock check
                    if (settings.MonitorNumLock)
                    {
                        bool currentNumLockState = Control.IsKeyLocked(Keys.NumLock);
                        if (currentNumLockState != lastNumLockState)
                        {
                            string message = currentNumLockState ? Localization.NumLockOn : Localization.NumLockOff;
                            ShowNotification(message);
                            lastNumLockState = currentNumLockState;
                        }
                    }

                    // ScrollLock check
                    if (settings.MonitorScrollLock)
                    {
                        bool currentScrollLockState = Control.IsKeyLocked(Keys.Scroll);
                        if (currentScrollLockState != lastScrollLockState)
                        {
                            string message = currentScrollLockState ? Localization.ScrollLockOn : Localization.ScrollLockOff;
                            ShowNotification(message);
                            lastScrollLockState = currentScrollLockState;
                        }
                    }

                    Thread.Sleep(pollingInterval);
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        Thread.Sleep(pollingInterval);
                }
            }
        }

        private static void ShowNotification(string message)
        {
            new Thread(() =>
            {
                try
                {
                    var notificationForm = new AnimatedNotificationForm(message, settings);
                    Application.Run(notificationForm);
                }
                catch (Exception)
                {
                    // Error during notification display
                }
            }).Start();
        }

        private class AnimatedNotificationForm : Form
        {
            private readonly Label messageLabel;
            private readonly System.Windows.Forms.Timer animationTimer;
            private readonly AppSettings formSettings;
            private int animationStep;
            private readonly int animationSteps;

            public AnimatedNotificationForm(string message, AppSettings settings)
            {
                formSettings = settings;
                animationSteps = Math.Max(20, formSettings.NotificationDurationMs / 20);

                InitializeForm();

                messageLabel = new Label
                {
                    Text = message,
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = formSettings.TextColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                Controls.Add(messageLabel);

                animationTimer = new System.Windows.Forms.Timer
                {
                    Interval = formSettings.NotificationDurationMs / animationSteps
                };
                animationTimer.Tick += AnimateNotification;
                animationTimer.Start();
            }

            private void InitializeForm()
            {
                Size = new Size(FormWidth, FormHeight);
                FormBorderStyle = FormBorderStyle.None;
                BackColor = formSettings.BackgroundColor;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                Opacity = 0;
                TopMost = true;

                // Create rounded corners
                using (var path = CreateRoundedPath())
                {
                    Region = new Region(path);
                }

                // Set position
                SetFormPosition();
            }

            private void SetFormPosition()
            {
                var currentScreen = Screen.FromPoint(Cursor.Position);
                var screenBounds = currentScreen.WorkingArea;

                int x, y;

                switch (formSettings.Position)
                {
                    case NotificationPosition.TopLeft:
                        x = screenBounds.Left + FormMargin;
                        y = screenBounds.Top + FormMargin;
                        break;
                    case NotificationPosition.TopCenter:
                        x = screenBounds.Left + (screenBounds.Width / 2) - (Width / 2);
                        y = screenBounds.Top + FormMargin;
                        break;
                    case NotificationPosition.TopRight:
                        x = screenBounds.Right - Width - FormMargin;
                        y = screenBounds.Top + FormMargin;
                        break;
                    case NotificationPosition.BottomLeft:
                        x = screenBounds.Left + FormMargin;
                        y = screenBounds.Bottom - Height - FormMargin;
                        break;
                    case NotificationPosition.BottomRight:
                        x = screenBounds.Right - Width - FormMargin;
                        y = screenBounds.Bottom - Height - FormMargin;
                        break;
                    case NotificationPosition.BottomCenter:
                    default:
                        x = screenBounds.Left + (screenBounds.Width / 2) - (Width / 2);
                        y = screenBounds.Bottom - Height - FormMargin;
                        break;
                }

                Location = new Point(x, y);
            }

            private GraphicsPath CreateRoundedPath()
            {
                var path = new GraphicsPath();
                int diameter = CornerRadius * 2;

                path.AddArc(0, 0, diameter, diameter, 180, 90);
                path.AddArc(Width - diameter, 0, diameter, diameter, 270, 90);
                path.AddArc(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
                path.AddArc(0, Height - diameter, diameter, diameter, 90, 90);
                path.CloseAllFigures();

                return path;
            }

            private void AnimateNotification(object sender, EventArgs e)
            {
                animationStep++;

                if (animationStep <= animationSteps / 2)
                {
                    // Fade in
                    Opacity = (double)animationStep / (animationSteps / 2);
                }
                else if (animationStep <= animationSteps * 3 / 4)
                {
                    // Full visibility
                    Opacity = 1.0;
                }
                else if (animationStep <= animationSteps)
                {
                    // Fade out
                    Opacity = 1.0 - (double)(animationStep - animationSteps * 3 / 4) / (animationSteps / 4);
                }
                else
                {
                    // Close
                    animationTimer.Stop();
                    animationTimer.Dispose();
                    Close();
                }
            }

            [DllImport("user32.dll")]
            private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            private const uint SWP_NOMOVE = 0x0002;
            private const uint SWP_NOSIZE = 0x0001;

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    animationTimer?.Dispose();
                    messageLabel?.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }

    // Extension method for NotifyIcon
    static class ControlExtensions
    {
        public static void Invoke(this NotifyIcon notifyIcon, Action action)
        {
            // Invoke through the form that NotifyIcon is attached to
            if (Application.OpenForms.Count > 0)
            {
                var form = Application.OpenForms[0];
                if (form.InvokeRequired)
                {
                    form.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }
    }
}
