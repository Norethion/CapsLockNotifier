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
        public static bool LastCapsLockState;
        public static bool LastNumLockState;
        public static bool LastScrollLockState;
        private static NotifyIcon trayIcon;
        private static bool isStartupEnabled = false;

        [STAThread]
        static void Main()
        {
            // Check if already running
            if (System.Diagnostics.Process.GetProcessesByName(
                System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show("Uygulama zaten çalışıyor.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // İlk kilit tuşları durumunu kontrol ediliyor
            LastCapsLockState = Control.IsKeyLocked(Keys.CapsLock);
            LastNumLockState = Control.IsKeyLocked(Keys.NumLock);
            LastScrollLockState = Control.IsKeyLocked(Keys.Scroll);

            // Load startup setting
            isStartupEnabled = CheckStartupSetting();

            // VersionInfo sınıfını kullanarak sürüm bilgisini oluşturuyoruz
            VersionInfo versionInfo = new VersionInfo("1.0.0", "12/12/2024", "İlk sürüm");

            using (trayIcon = new NotifyIcon())
            {
                // Use a custom icon for better visibility
                trayIcon.Icon = new Icon(SystemIcons.Information, 40, 40);
                trayIcon.Visible = true;
                trayIcon.Text = "Kilit Tuşları İzleyicisi";

                // Create context menu with startup option
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                // Sürüm bilgisi menü maddesi
                ToolStripMenuItem versionMenuItem = new ToolStripMenuItem("Sürüm Bilgisi");
                versionMenuItem.Click += (s, e) => versionInfo.DisplayVersionInfo();
                contextMenu.Items.Add(versionMenuItem);

                // Startup at Windows startup checkbox menu item
                ToolStripMenuItem startupMenuItem = new ToolStripMenuItem("Windows Başlangıcında Çalıştır")
                {
                    CheckOnClick = true,
                    Checked = isStartupEnabled
                };
                startupMenuItem.Click += ToggleStartup;
                contextMenu.Items.Add(startupMenuItem);

                // Exit menu item
                contextMenu.Items.Add("Çıkış", null, (s, e) => Application.Exit());

                trayIcon.ContextMenuStrip = contextMenu;

                // Initial notifications for lock states
                ShowInitialNotifications();

                Thread monitorThread = new Thread(MonitorLockKeys)
                {
                    IsBackground = true
                };
                monitorThread.Start();

                Application.Run();
            }
        }

        private static bool CheckStartupSetting()
        {
            try
            {
                RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                return startupKey.GetValue("CapsLockNotifier") != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ToggleStartup(object sender, EventArgs e)
        {
            try
            {
                RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

                if (menuItem.Checked)
                {
                    // Add to startup
                    startupKey.SetValue("CapsLockNotifier",
                        Application.ExecutablePath.ToString());
                    MessageBox.Show("Uygulama Windows başlangıcında çalışacak şekilde ayarlandı.",
                        "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Remove from startup
                    startupKey.DeleteValue("CapsLockNotifier", false);
                    MessageBox.Show("Uygulama Windows başlangıcından kaldırıldı.",
                        "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ayar değiştirilemedi: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetInitialLockStatus()
        {
            string status = "CapsLock: " + (LastCapsLockState ? "AÇIK" : "KAPALI") +
                            "\nNumLock: " + (LastNumLockState ? "AÇIK" : "KAPALI") +
                            "\nScrollLock: " + (LastScrollLockState ? "AÇIK" : "KAPALI");
            return status;
        }

        private static void ShowInitialNotifications()
        {
            if (LastCapsLockState)
                ShowNotification("CapsLock AÇIK");

            if (LastNumLockState)
                ShowNotification("NumLock AÇIK");

            if (LastScrollLockState)
                ShowNotification("ScrollLock AÇIK");
        }

        private static void MonitorLockKeys()
        {
            while (true)
            {
                // CapsLock kontrolü
                bool currentCapsLockState = Control.IsKeyLocked(Keys.CapsLock);
                if (currentCapsLockState != LastCapsLockState)
                {
                    string message = currentCapsLockState ? "CapsLock AÇIK" : "CapsLock KAPALI";
                    ShowNotification(message);
                    LastCapsLockState = currentCapsLockState;
                }

                // NumLock kontrolü
                bool currentNumLockState = Control.IsKeyLocked(Keys.NumLock);
                if (currentNumLockState != LastNumLockState)
                {
                    string message = currentNumLockState ? "NumLock AÇIK" : "NumLock KAPALI";
                    ShowNotification(message);
                    LastNumLockState = currentNumLockState;
                }

                // ScrollLock kontrolü
                bool currentScrollLockState = Control.IsKeyLocked(Keys.Scroll);
                if (currentScrollLockState != LastScrollLockState)
                {
                    string message = currentScrollLockState ? "ScrollLock AÇIK" : "ScrollLock KAPALI";
                    ShowNotification(message);
                    LastScrollLockState = currentScrollLockState;
                }

                Thread.Sleep(200);
            }
        }

        private static void ShowNotification(string message)
        {
            new Thread(() =>
            {
                AnimatedNotificationForm notificationForm = new AnimatedNotificationForm(message);
                Application.Run(notificationForm);
            }).Start();
        }

        // Custom form with animation and always-on-top behavior
        private class AnimatedNotificationForm : Form
        {
            private Label messageLabel;
            private System.Windows.Forms.Timer animationTimer;
            private int animationStep = 0;
            private const int AnimationDuration = 800; // Total animation time in milliseconds
            private const int AnimationSteps = 40;

            public AnimatedNotificationForm(string message)
            {
                InitializeForm(message);
                SetupAnimationTimer();
            }

            private void InitializeForm(string message)
            {
                // Form properties
                Size = new Size(350, 70);
                FormBorderStyle = FormBorderStyle.None;
                BackColor = Color.FromArgb(255, 240, 240, 240); // Soft gray background
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                Opacity = 0; // Start fully transparent
                Region = CreateRoundedRegion();

                // Position the form at the bottom of the screen
                var screenBounds = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(screenBounds.Width / 2 - Width / 2, screenBounds.Height - Height - 20);

                // Create and configure label
                messageLabel = new Label
                {
                    Text = message,
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                Controls.Add(messageLabel);

                // Ensure the form appears on top of other windows
                TopMost = true;
            }

            // Create rounded corners
            private Region CreateRoundedRegion()
            {
                GraphicsPath path = new GraphicsPath();
                int cornerRadius = 15;
                path.AddArc(0, 0, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(Width - cornerRadius * 2, 0, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(Width - cornerRadius * 2, Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(0, Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseAllFigures();
                return new Region(path);
            }

            private void SetupAnimationTimer()
            {
                animationTimer = new System.Windows.Forms.Timer();
                animationTimer.Interval = AnimationDuration / AnimationSteps;
                animationTimer.Tick += AnimateNotification;
                animationTimer.Start();
            }

            private void AnimateNotification(object sender, EventArgs e)
            {
                animationStep++;

                // Fade in
                if (animationStep <= AnimationSteps / 2)
                {
                    Opacity = (double)animationStep / (AnimationSteps / 2);
                }
                // Display
                else if (animationStep <= AnimationSteps * 3 / 4)
                {
                    Opacity = 1.0;
                }
                // Fade out
                else if (animationStep <= AnimationSteps)
                {
                    Opacity = 1.0 - (double)(animationStep - AnimationSteps * 3 / 4) / (AnimationSteps / 4);
                }
                // Close
                else
                {
                    animationTimer.Stop();
                    Close();
                }
            }

            [DllImport("user32.dll")]
            static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            private const uint SWP_NOMOVE = 0x0002;
            private const uint SWP_NOSIZE = 0x0001;

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                // Ensure the window is truly topmost and cannot be activated
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }
    }
}