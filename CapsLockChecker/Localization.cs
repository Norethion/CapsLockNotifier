using System.Collections.Generic;

namespace CapsLockNotifier
{
    public static class Localization
    {
        private static Dictionary<string, string> _strings;
        private static string _currentLanguage = "tr";

        private static readonly Dictionary<string, string> TurkishStrings = new Dictionary<string, string>
        {
            // Notifications
            { "CapsLockOn", "CapsLock ACIK" },
            { "CapsLockOff", "CapsLock KAPALI" },
            { "NumLockOn", "NumLock ACIK" },
            { "NumLockOff", "NumLock KAPALI" },
            { "ScrollLockOn", "ScrollLock ACIK" },
            { "ScrollLockOff", "ScrollLock KAPALI" },

            // Menu
            { "Settings", "Ayarlar" },
            { "VersionInfo", "Surum Bilgisi" },
            { "StartWithWindows", "Windows Baslangicinda Calistir" },
            { "Exit", "Cikis" },

            // Tray
            { "TrayTooltip", "Kilit Tuslari Izleyicisi" },

            // Settings window
            { "SettingsTitle", "Ayarlar" },
            { "ThemeTab", "Tema" },
            { "NotificationTab", "Bildirim" },
            { "KeysTab", "Tuslar" },
            { "LanguageTab", "Dil" },

            // Theme
            { "DarkTheme", "Koyu Tema" },
            { "LightTheme", "Acik Tema" },
            { "BackgroundColor", "Arka Plan Rengi" },
            { "TextColor", "Yazi Rengi" },
            { "ChooseColor", "Renk Sec..." },

            // Notification
            { "Position", "Pozisyon" },
            { "TopLeft", "Sol Ust" },
            { "TopCenter", "Orta Ust" },
            { "TopRight", "Sag Ust" },
            { "BottomLeft", "Sol Alt" },
            { "BottomCenter", "Orta Alt" },
            { "BottomRight", "Sag Alt" },
            { "Duration", "Sure (ms)" },

            // Keys
            { "MonitorCapsLock", "CapsLock Izle" },
            { "MonitorNumLock", "NumLock Izle" },
            { "MonitorScrollLock", "ScrollLock Izle" },

            // Language
            { "Turkish", "Turkce" },
            { "English", "Ingilizce" },
            { "LanguageChangeNote", "Dil degisikligi uygulama yeniden baslatildiginda aktif olacaktir." },

            // Buttons
            { "Save", "Kaydet" },
            { "Cancel", "Iptal" },
            { "ResetDefaults", "Varsayilanlara Sifirla" },

            // Messages
            { "SettingsSaved", "Ayarlar kaydedildi." },
            { "StartupEnabled", "Uygulama Windows baslangicinda calisacak sekilde ayarlandi." },
            { "StartupDisabled", "Uygulama Windows baslangicindandan kaldirildi." },
            { "AlreadyRunning", "Uygulama zaten calisiyor." },
            { "Info", "Bilgi" },
            { "Error", "Hata" },
            { "Information", "Bilgilendirme" },
            { "RegistryError", "Registry anahtarina erisilemedi." },
            { "SettingsChangeError", "Ayar degistirilemedi: " },

            // Version
            { "Version", "Surum" },
            { "ReleaseDate", "Yayin Tarihi" },
            { "Notes", "Notlar" },
            { "InitialRelease", "Ilk surum" }
        };

        private static readonly Dictionary<string, string> EnglishStrings = new Dictionary<string, string>
        {
            // Notifications
            { "CapsLockOn", "CapsLock ON" },
            { "CapsLockOff", "CapsLock OFF" },
            { "NumLockOn", "NumLock ON" },
            { "NumLockOff", "NumLock OFF" },
            { "ScrollLockOn", "ScrollLock ON" },
            { "ScrollLockOff", "ScrollLock OFF" },

            // Menu
            { "Settings", "Settings" },
            { "VersionInfo", "Version Info" },
            { "StartWithWindows", "Start with Windows" },
            { "Exit", "Exit" },

            // Tray
            { "TrayTooltip", "Lock Keys Monitor" },

            // Settings window
            { "SettingsTitle", "Settings" },
            { "ThemeTab", "Theme" },
            { "NotificationTab", "Notification" },
            { "KeysTab", "Keys" },
            { "LanguageTab", "Language" },

            // Theme
            { "DarkTheme", "Dark Theme" },
            { "LightTheme", "Light Theme" },
            { "BackgroundColor", "Background Color" },
            { "TextColor", "Text Color" },
            { "ChooseColor", "Choose Color..." },

            // Notification
            { "Position", "Position" },
            { "TopLeft", "Top Left" },
            { "TopCenter", "Top Center" },
            { "TopRight", "Top Right" },
            { "BottomLeft", "Bottom Left" },
            { "BottomCenter", "Bottom Center" },
            { "BottomRight", "Bottom Right" },
            { "Duration", "Duration (ms)" },

            // Keys
            { "MonitorCapsLock", "Monitor CapsLock" },
            { "MonitorNumLock", "Monitor NumLock" },
            { "MonitorScrollLock", "Monitor ScrollLock" },

            // Language
            { "Turkish", "Turkish" },
            { "English", "English" },
            { "LanguageChangeNote", "Language change will take effect after restarting the application." },

            // Buttons
            { "Save", "Save" },
            { "Cancel", "Cancel" },
            { "ResetDefaults", "Reset to Defaults" },

            // Messages
            { "SettingsSaved", "Settings saved." },
            { "StartupEnabled", "Application set to start with Windows." },
            { "StartupDisabled", "Application removed from Windows startup." },
            { "AlreadyRunning", "Application is already running." },
            { "Info", "Info" },
            { "Error", "Error" },
            { "Information", "Information" },
            { "RegistryError", "Could not access registry key." },
            { "SettingsChangeError", "Could not change setting: " },

            // Version
            { "Version", "Version" },
            { "ReleaseDate", "Release Date" },
            { "Notes", "Notes" },
            { "InitialRelease", "Initial release" }
        };

        static Localization()
        {
            _strings = TurkishStrings;
        }

        public static string CurrentLanguage => _currentLanguage;

        public static void SetLanguage(string language)
        {
            _currentLanguage = language;
            _strings = language == "en" ? EnglishStrings : TurkishStrings;
        }

        public static string Get(string key)
        {
            if (_strings.TryGetValue(key, out var value))
            {
                return value;
            }
            return key; // Return key if not found
        }

        // Shortcut access
        public static string CapsLockOn => Get("CapsLockOn");
        public static string CapsLockOff => Get("CapsLockOff");
        public static string NumLockOn => Get("NumLockOn");
        public static string NumLockOff => Get("NumLockOff");
        public static string ScrollLockOn => Get("ScrollLockOn");
        public static string ScrollLockOff => Get("ScrollLockOff");
    }
}
