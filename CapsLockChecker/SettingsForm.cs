using System;
using System.Drawing;
using System.Windows.Forms;

namespace CapsLockNotifier
{
    public class SettingsForm : Form
    {
        private AppSettings _settings;
        private TabControl tabControl;

        // Theme controls
        private RadioButton rbLightTheme;
        private RadioButton rbDarkTheme;
        private Button btnBackgroundColor;
        private Button btnTextColor;
        private Panel pnlBackgroundPreview;
        private Panel pnlTextPreview;

        // Notification controls
        private ComboBox cmbPosition;
        private TrackBar trkDuration;
        private Label lblDurationValue;

        // Key controls
        private CheckBox chkCapsLock;
        private CheckBox chkNumLock;
        private CheckBox chkScrollLock;

        // Language controls
        private RadioButton rbTurkish;
        private RadioButton rbEnglish;
        private Label lblLanguageNote;

        // Buttons
        private Button btnSave;
        private Button btnCancel;
        private Button btnReset;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = Localization.Get("SettingsTitle");
            Size = new Size(450, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // TabControl
            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(415, 300)
            };

            // Theme tab
            var tabTheme = new TabPage(Localization.Get("ThemeTab"));
            CreateThemeTab(tabTheme);
            tabControl.TabPages.Add(tabTheme);

            // Notification tab
            var tabNotification = new TabPage(Localization.Get("NotificationTab"));
            CreateNotificationTab(tabNotification);
            tabControl.TabPages.Add(tabNotification);

            // Keys tab
            var tabKeys = new TabPage(Localization.Get("KeysTab"));
            CreateKeysTab(tabKeys);
            tabControl.TabPages.Add(tabKeys);

            // Language tab
            var tabLanguage = new TabPage(Localization.Get("LanguageTab"));
            CreateLanguageTab(tabLanguage);
            tabControl.TabPages.Add(tabLanguage);

            Controls.Add(tabControl);

            // Buttons
            btnSave = new Button
            {
                Text = Localization.Get("Save"),
                Location = new Point(180, 320),
                Size = new Size(80, 30)
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = Localization.Get("Cancel"),
                Location = new Point(270, 320),
                Size = new Size(80, 30)
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            btnReset = new Button
            {
                Text = Localization.Get("ResetDefaults"),
                Location = new Point(10, 320),
                Size = new Size(150, 30)
            };
            btnReset.Click += BtnReset_Click;
            Controls.Add(btnReset);
        }

        private void CreateThemeTab(TabPage tab)
        {
            // Theme selection
            var lblTheme = new Label
            {
                Text = Localization.Get("ThemeTab") + ":",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tab.Controls.Add(lblTheme);

            rbLightTheme = new RadioButton
            {
                Text = Localization.Get("LightTheme"),
                Location = new Point(20, 45),
                AutoSize = true
            };
            rbLightTheme.CheckedChanged += ThemeRadio_CheckedChanged;
            tab.Controls.Add(rbLightTheme);

            rbDarkTheme = new RadioButton
            {
                Text = Localization.Get("DarkTheme"),
                Location = new Point(150, 45),
                AutoSize = true
            };
            rbDarkTheme.CheckedChanged += ThemeRadio_CheckedChanged;
            tab.Controls.Add(rbDarkTheme);

            // Background color
            var lblBackground = new Label
            {
                Text = Localization.Get("BackgroundColor") + ":",
                Location = new Point(20, 90),
                AutoSize = true
            };
            tab.Controls.Add(lblBackground);

            pnlBackgroundPreview = new Panel
            {
                Location = new Point(20, 115),
                Size = new Size(50, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(pnlBackgroundPreview);

            btnBackgroundColor = new Button
            {
                Text = Localization.Get("ChooseColor"),
                Location = new Point(80, 115),
                Size = new Size(100, 25)
            };
            btnBackgroundColor.Click += BtnBackgroundColor_Click;
            tab.Controls.Add(btnBackgroundColor);

            // Text color
            var lblText = new Label
            {
                Text = Localization.Get("TextColor") + ":",
                Location = new Point(20, 160),
                AutoSize = true
            };
            tab.Controls.Add(lblText);

            pnlTextPreview = new Panel
            {
                Location = new Point(20, 185),
                Size = new Size(50, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            tab.Controls.Add(pnlTextPreview);

            btnTextColor = new Button
            {
                Text = Localization.Get("ChooseColor"),
                Location = new Point(80, 185),
                Size = new Size(100, 25)
            };
            btnTextColor.Click += BtnTextColor_Click;
            tab.Controls.Add(btnTextColor);
        }

        private void CreateNotificationTab(TabPage tab)
        {
            // Position
            var lblPosition = new Label
            {
                Text = Localization.Get("Position") + ":",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tab.Controls.Add(lblPosition);

            cmbPosition = new ComboBox
            {
                Location = new Point(20, 45),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPosition.Items.Add(Localization.Get("TopLeft"));
            cmbPosition.Items.Add(Localization.Get("TopCenter"));
            cmbPosition.Items.Add(Localization.Get("TopRight"));
            cmbPosition.Items.Add(Localization.Get("BottomLeft"));
            cmbPosition.Items.Add(Localization.Get("BottomCenter"));
            cmbPosition.Items.Add(Localization.Get("BottomRight"));
            tab.Controls.Add(cmbPosition);

            // Duration
            var lblDuration = new Label
            {
                Text = Localization.Get("Duration") + ":",
                Location = new Point(20, 90),
                AutoSize = true
            };
            tab.Controls.Add(lblDuration);

            trkDuration = new TrackBar
            {
                Location = new Point(20, 115),
                Size = new Size(300, 45),
                Minimum = 400,
                Maximum = 2000,
                TickFrequency = 200,
                LargeChange = 200,
                SmallChange = 100
            };
            trkDuration.ValueChanged += TrkDuration_ValueChanged;
            tab.Controls.Add(trkDuration);

            lblDurationValue = new Label
            {
                Location = new Point(330, 120),
                Size = new Size(60, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tab.Controls.Add(lblDurationValue);
        }

        private void CreateKeysTab(TabPage tab)
        {
            var lblInfo = new Label
            {
                Text = "Izlenecek tuslari secin:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tab.Controls.Add(lblInfo);

            chkCapsLock = new CheckBox
            {
                Text = Localization.Get("MonitorCapsLock"),
                Location = new Point(20, 50),
                AutoSize = true
            };
            tab.Controls.Add(chkCapsLock);

            chkNumLock = new CheckBox
            {
                Text = Localization.Get("MonitorNumLock"),
                Location = new Point(20, 80),
                AutoSize = true
            };
            tab.Controls.Add(chkNumLock);

            chkScrollLock = new CheckBox
            {
                Text = Localization.Get("MonitorScrollLock"),
                Location = new Point(20, 110),
                AutoSize = true
            };
            tab.Controls.Add(chkScrollLock);
        }

        private void CreateLanguageTab(TabPage tab)
        {
            var lblLanguage = new Label
            {
                Text = Localization.Get("LanguageTab") + ":",
                Location = new Point(20, 20),
                AutoSize = true
            };
            tab.Controls.Add(lblLanguage);

            rbTurkish = new RadioButton
            {
                Text = Localization.Get("Turkish"),
                Location = new Point(20, 50),
                AutoSize = true
            };
            tab.Controls.Add(rbTurkish);

            rbEnglish = new RadioButton
            {
                Text = Localization.Get("English"),
                Location = new Point(20, 80),
                AutoSize = true
            };
            tab.Controls.Add(rbEnglish);

            lblLanguageNote = new Label
            {
                Text = Localization.Get("LanguageChangeNote"),
                Location = new Point(20, 120),
                Size = new Size(350, 40),
                ForeColor = Color.Gray
            };
            tab.Controls.Add(lblLanguageNote);
        }

        private void LoadSettings()
        {
            // Theme
            rbLightTheme.Checked = !_settings.IsDarkTheme;
            rbDarkTheme.Checked = _settings.IsDarkTheme;
            pnlBackgroundPreview.BackColor = _settings.BackgroundColor;
            pnlTextPreview.BackColor = _settings.TextColor;

            // Notification
            cmbPosition.SelectedIndex = (int)_settings.Position;
            trkDuration.Value = Math.Max(trkDuration.Minimum, Math.Min(trkDuration.Maximum, _settings.NotificationDurationMs));
            lblDurationValue.Text = trkDuration.Value + " ms";

            // Keys
            chkCapsLock.Checked = _settings.MonitorCapsLock;
            chkNumLock.Checked = _settings.MonitorNumLock;
            chkScrollLock.Checked = _settings.MonitorScrollLock;

            // Language
            rbTurkish.Checked = _settings.Language == "tr";
            rbEnglish.Checked = _settings.Language == "en";
        }

        private void ThemeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDarkTheme.Checked)
            {
                pnlBackgroundPreview.BackColor = Color.FromArgb(45, 45, 48);
                pnlTextPreview.BackColor = Color.White;
            }
            else
            {
                pnlBackgroundPreview.BackColor = Color.FromArgb(240, 240, 240);
                pnlTextPreview.BackColor = Color.Black;
            }
        }

        private void BtnBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = pnlBackgroundPreview.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    pnlBackgroundPreview.BackColor = colorDialog.Color;
                }
            }
        }

        private void BtnTextColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = pnlTextPreview.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    pnlTextPreview.BackColor = colorDialog.Color;
                }
            }
        }

        private void TrkDuration_ValueChanged(object sender, EventArgs e)
        {
            lblDurationValue.Text = trkDuration.Value + " ms";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Save settings
            _settings.IsDarkTheme = rbDarkTheme.Checked;
            _settings.BackgroundColor = pnlBackgroundPreview.BackColor;
            _settings.TextColor = pnlTextPreview.BackColor;
            _settings.Position = (NotificationPosition)cmbPosition.SelectedIndex;
            _settings.NotificationDurationMs = trkDuration.Value;
            _settings.MonitorCapsLock = chkCapsLock.Checked;
            _settings.MonitorNumLock = chkNumLock.Checked;
            _settings.MonitorScrollLock = chkScrollLock.Checked;
            _settings.Language = rbEnglish.Checked ? "en" : "tr";

            _settings.Save();

            MessageBox.Show(Localization.Get("SettingsSaved"), Localization.Get("Information"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _settings.SetDefaults();
            LoadSettings();
        }
    }
}
