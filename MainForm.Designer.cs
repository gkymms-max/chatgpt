using System.Drawing;
using System.Windows.Forms;

namespace MultiSocialWeb
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel topBar;
        private PictureBox logoBox;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnOpenPriceList;
        private Button btnOpenQuote;

        private Panel controlsPanel;
        private FlowLayoutPanel flpActions;
        private FlowLayoutPanel flpInputs;
        private Button btnNewWhatsApp;
        private Button btnNewInstagram;
        private Button btnNewFacebook;

        private Label labelUrl;
        private TextBox txtCustomUrl;
        private Label labelName;
        private TextBox txtCustomName;
        private Button btnNewCustom;

        private TabControl tabs;

        private Panel rightPanel;
        private ListView listInstances;
        private ColumnHeader colName;
        private ColumnHeader colService;
        private ColumnHeader colUrl;
        private ColumnHeader colPath;
        private FlowLayoutPanel flpRightActions;
        private Button btnOpenSelected;
        private Button btnDeleteSelected;
        private Button btnRename;
        private Button btnOpenAll;
        private Label lblCount;

        private Label labelPrimary;
        private Button btnPickColor;
        private Label labelLogo;
        private Button btnPickLogo;
        private Label labelDark;
        private CheckBox chkDark;
        private Label labelHighContrast;
        private CheckBox chkHighContrast;
        private Button btnResetTheme;
        private Button btnSafePreset;
        private Button btnToggleControls;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Top bar
            topBar = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(16, 8, 16, 8) };
            logoBox = new PictureBox { Width = 32, Height = 32, Left = 16, Top = 18, SizeMode = PictureBoxSizeMode.CenterImage };
            lblTitle = new Label { AutoSize = true, Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold), Text = "MultiSocial Web", Left = 60, Top = 8 };
            lblSubtitle = new Label { AutoSize = true, Font = new Font("Segoe UI", 9F), Text = "Erişilebilir, kurumsal çoklu oturum tarayıcısı", Left = 60, Top = 40 };
            topBar.Controls.Add(logoBox);
            topBar.Controls.Add(lblTitle);
            
            // Toggle Controls button (hide/show control panel)
            btnToggleControls = new Button { Text = "⮜ Gizle", AutoSize = true, Height = 28 };
            btnToggleControls.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            // 
            // Quick access buttons
            if (btnOpenPriceList == null) btnOpenPriceList = new Button { Text = "📄  Fiyat Listesi", AutoSize = true, Height = 28 };
            if (btnOpenQuote == null)     btnOpenQuote     = new Button { Text = "🧮  Fiyat Teklifi Ver", AutoSize = true, Height = 28 };
            btnOpenPriceList.Top = btnToggleControls.Top; 
            btnOpenQuote.Top = btnToggleControls.Top;
            topBar.Controls.Add(btnOpenPriceList);
            topBar.Controls.Add(btnOpenQuote);
            // place near right edge of topBar
            btnToggleControls.Top = 20;
            btnToggleControls.Left = topBar.Width - (btnToggleControls.Width + 16);
            btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
            btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12);
            topBar.Resize += (s, e) => { btnToggleControls.Left = topBar.Width - (btnToggleControls.Width + 16);
            btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
            btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12); btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12); btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12); };
            btnToggleControls.Click += (s, e) => this.BtnToggleControls_Click(s, e);
            topBar.Controls.Add(btnToggleControls);

            // Kısayol butonları (moved above)
            topBar.Resize += (s, e) => {
                btnToggleControls.Left = topBar.Width - (btnToggleControls.Width + 16);
            btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
            btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12);
                btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
                btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12);
            };
            // ilk konumlandırma
            btnToggleControls.Left = topBar.Width - (btnToggleControls.Width + 16);
            btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
            btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12);
            btnOpenQuote.Left = btnToggleControls.Left - (btnOpenQuote.Width + 12);
            btnOpenPriceList.Left = btnOpenQuote.Left - (btnOpenPriceList.Width + 12);

            // Wire events
            btnOpenPriceList.Click += new System.EventHandler(this.btnOpenPriceList_Click);
            btnOpenQuote.Click += new System.EventHandler(this.btnOpenQuote_Click);

            topBar.Controls.Add(lblSubtitle);

            // Controls panel
            controlsPanel = new Panel { Dock = DockStyle.Top, Padding = new Padding(12), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            flpActions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Padding = new Padding(0,4,0,4) };
            flpInputs  = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(0, 8, 0, 0), Padding = new Padding(0,4,0,4) };

            btnNewWhatsApp = new Button { Text = "Yeni WhatsApp", Width = 140, Height = 32, Margin = new Padding(6) };
            btnNewInstagram = new Button { Text = "Yeni Instagram", Width = 140, Height = 32, Margin = new Padding(6) };
            btnNewFacebook = new Button { Text = "Yeni Facebook", Width = 140, Height = 32, Margin = new Padding(6) };
            flpActions.Controls.AddRange(new Control[]{ btnNewWhatsApp, btnNewInstagram, btnNewFacebook });

            labelUrl = new Label { Text = "URL", AutoSize = true, Padding = new Padding(0, 8, 0, 0), Margin = new Padding(6, 6, 0, 0) };
            txtCustomUrl = new TextBox { PlaceholderText = "https://app.ornek.com", Width = 360, Margin = new Padding(6) };
            labelName = new Label { Text = "Ad", AutoSize = true, Padding = new Padding(0, 8, 0, 0), Margin = new Padding(12, 6, 0, 0) };
            txtCustomName = new TextBox { PlaceholderText = "Özel sekme adı (opsiyonel)", Width = 260, Margin = new Padding(6) };
            btnNewCustom  = new Button { Text = "Yeni Özel Sekme", Width = 150, Height = 32, Margin = new Padding(12, 6, 6, 6) };

            flpInputs.Controls.AddRange(new Control[]{ labelUrl, txtCustomUrl, labelName, txtCustomName, btnNewCustom });
            controlsPanel.Controls.Add(flpInputs);
            controlsPanel.Controls.Add(flpActions);

            // Tabs center
            tabs = new TabControl { Dock = DockStyle.Fill, SizeMode = TabSizeMode.Fixed, ItemSize = new Size(180, 36), Multiline = true };

            // Right panel
            rightPanel = new Panel { Dock = DockStyle.Right, Width = 520, Padding = new Padding(12), AutoScroll = true };

            listInstances = new ListView { Dock = DockStyle.Top, Height = 360, FullRowSelect = true, GridLines = true, View = View.Details };
            colName = new ColumnHeader(){ Text = "Ad", Width = 170 };
            colService = new ColumnHeader(){ Text = "Servis", Width = 90 };
            colUrl = new ColumnHeader(){ Text = "URL", Width = 140 };
            colPath = new ColumnHeader(){ Text = "Profil Klasörü", Width = 200 };
            listInstances.Columns.AddRange(new ColumnHeader[]{ colName, colService, colUrl, colPath });
            listInstances.Font = new Font("Segoe UI", 9F);

            // Right action buttons - Flow
            flpRightActions = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
            btnOpenSelected = new Button { Text = "Seçileni Aç" };
            btnRename = new Button { Text = "Yeniden Adlandır" };
            btnDeleteSelected = new Button { Text = "Sil" };
            btnOpenAll = new Button { Text = "Hepsini Aç" };
            flpRightActions.Controls.AddRange(new Control[]{ btnOpenSelected, btnRename, btnDeleteSelected, btnOpenAll });

            lblCount = new Label { Text = "Toplam: 0 oturum", AutoSize = true };

            labelPrimary = new Label { Text = "Kurumsal Renk", AutoSize = true };
            btnPickColor = new Button { Text = "Renk Seç" };

            labelLogo = new Label { Text = "Logo", AutoSize = true };
            btnPickLogo = new Button { Text = "Logo Seç" };

            labelDark = new Label { Text = "Koyu Tema", AutoSize = true };
            chkDark = new CheckBox { Checked = false };

            labelHighContrast = new Label { Text = "Yüksek Kontrast", AutoSize = true };
            chkHighContrast = new CheckBox { Checked = true };

            btnResetTheme = new Button { Text = "Temayı Sıfırla" };
            btnSafePreset = new Button { Text = "Güvenli Tema" };

            // stack controls
            var stack = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            var lineCount = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            lineCount.Controls.Add(lblCount);
            stack.Controls.Add(lineCount);
            stack.Controls.Add(flpRightActions);

            var line2 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            line2.Controls.Add(labelPrimary);
            line2.Controls.Add(btnPickColor);
            stack.Controls.Add(line2);

            var lineLogo = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            lineLogo.Controls.Add(labelLogo);
            lineLogo.Controls.Add(btnPickLogo);
            stack.Controls.Add(lineLogo);

            var line3 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            line3.Controls.Add(labelDark);
            line3.Controls.Add(chkDark);
            stack.Controls.Add(line3);

            var line4 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            line4.Controls.Add(labelHighContrast);
            line4.Controls.Add(chkHighContrast);
            stack.Controls.Add(line4);

            var line5 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            line5.Controls.Add(btnResetTheme);
            line5.Controls.Add(btnSafePreset);
            stack.Controls.Add(line5);

            rightPanel.Controls.Add(stack);
            rightPanel.Controls.Add(listInstances);

            // Wire events
            btnNewWhatsApp.Click += new System.EventHandler(this.btnNewWhatsApp_Click);
            btnNewInstagram.Click += new System.EventHandler(this.btnNewInstagram_Click);
            btnNewFacebook.Click += new System.EventHandler(this.btnNewFacebook_Click);
            btnNewCustom.Click += new System.EventHandler(this.btnNewCustom_Click);
            btnOpenSelected.Click += new System.EventHandler(this.btnOpenSelected_Click);
            btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
            btnRename.Click += new System.EventHandler(this.btnRename_Click);
            btnOpenAll.Click += new System.EventHandler(this.btnOpenAll_Click);
            btnPickColor.Click += new System.EventHandler(this.btnPickColor_Click);
            btnPickLogo.Click += new System.EventHandler(this.btnPickLogo_Click);
            chkDark.CheckedChanged += new System.EventHandler(this.chkDark_CheckedChanged);
            chkHighContrast.CheckedChanged += new System.EventHandler(this.chkHighContrast_CheckedChanged);
            btnResetTheme.Click += new System.EventHandler(this.btnResetTheme_Click);
            btnSafePreset.Click += new System.EventHandler(this.btnSafePreset_Click);

            // Form
            Text = "MultiSocial Web (Kurumsal)";
            ClientSize = new Size(1500, 900);
            Controls.Add(tabs);
            Controls.Add(rightPanel);
            Controls.Add(controlsPanel);
            Controls.Add(topBar);
        }
    }
}