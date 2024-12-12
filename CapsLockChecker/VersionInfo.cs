using System;
using System.Windows.Forms;

namespace CapsLockNotifier
{
    public class VersionInfo
    {
        public string Version { get; private set; }
        public string ReleaseDate { get; private set; }
        public string Notes { get; private set; }

        // Constructor to initialize version details
        public VersionInfo(string version, string releaseDate, string notes = "")
        {
            Version = version;
            ReleaseDate = releaseDate;
            Notes = notes;
        }

        // Display version information in a message box
        public void DisplayVersionInfo()
        {
            string info = $"Sürüm: {Version}\nYayın Tarihi: {ReleaseDate}\nNotlar: {Notes}";
            MessageBox.Show(info, "Sürüm Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
