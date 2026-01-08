using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace CapsLockNotifier
{
    public enum NotificationPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [Serializable]
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CapsLockNotifier",
            "settings.xml"
        );

        // Theme
        public bool IsDarkTheme { get; set; }
        public int BackgroundColorArgb { get; set; }
        public int TextColorArgb { get; set; }

        // Position
        public NotificationPosition Position { get; set; }

        // Key selection
        public bool MonitorCapsLock { get; set; }
        public bool MonitorNumLock { get; set; }
        public bool MonitorScrollLock { get; set; }

        // Notification duration
        public int NotificationDurationMs { get; set; }

        // Language
        public string Language { get; set; }

        // Color properties (using ARGB for XML serialization)
        [XmlIgnore]
        public Color BackgroundColor
        {
            get => Color.FromArgb(BackgroundColorArgb);
            set => BackgroundColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color TextColor
        {
            get => Color.FromArgb(TextColorArgb);
            set => TextColorArgb = value.ToArgb();
        }

        public AppSettings()
        {
            // Default values
            SetDefaults();
        }

        public void SetDefaults()
        {
            IsDarkTheme = false;
            BackgroundColor = Color.FromArgb(240, 240, 240);
            TextColor = Color.Black;
            Position = NotificationPosition.BottomCenter;
            MonitorCapsLock = true;
            MonitorNumLock = true;
            MonitorScrollLock = true;
            NotificationDurationMs = 800;
            Language = "tr";
        }

        public void ApplyDarkTheme()
        {
            IsDarkTheme = true;
            BackgroundColor = Color.FromArgb(45, 45, 48);
            TextColor = Color.White;
        }

        public void ApplyLightTheme()
        {
            IsDarkTheme = false;
            BackgroundColor = Color.FromArgb(240, 240, 240);
            TextColor = Color.Black;
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var serializer = new XmlSerializer(typeof(AppSettings));
                using (var writer = new StreamWriter(SettingsPath))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception)
            {
                // Failed to save settings, continue silently
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    using (var reader = new StreamReader(SettingsPath))
                    {
                        var settings = (AppSettings)serializer.Deserialize(reader);
                        return settings;
                    }
                }
            }
            catch (Exception)
            {
                // Failed to load settings, use defaults
            }

            return new AppSettings();
        }
    }
}
