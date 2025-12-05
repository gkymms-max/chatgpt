using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace MultiSocialWebPlus.Forms
{
    public class SocialsForm : Form
    {
        private TabControl tabControl;
        private Button btnAddWhatsApp, btnAddInstagram, btnAddCustom;
        private int sessionCounter = 0;

        public SocialsForm()
        {
            Text = "Sosyal Bağlantılar";
            
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            btnAddWhatsApp = new Button { Text = "+ WhatsApp", Width = 100 };
            btnAddWhatsApp.Click += (s, e) => AddTab("WhatsApp", "https://web.whatsapp.com");
            btnAddInstagram = new Button { Text = "+ Instagram", Width = 100 };
            btnAddInstagram.Click += (s, e) => AddTab("Instagram", "https://www.instagram.com");
            btnAddCustom = new Button { Text = "+ Özel URL", Width = 100 };
            btnAddCustom.Click += BtnAddCustom_Click;
            topPanel.Controls.AddRange(new Control[] { btnAddWhatsApp, btnAddInstagram, btnAddCustom });

            tabControl = new TabControl { Dock = DockStyle.Fill };
            
            Controls.Add(tabControl);
            Controls.Add(topPanel);
        }

        private void BtnAddCustom_Click(object? sender, EventArgs e)
        {
            using var dlg = new CustomUrlDialog();
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.Url))
            {
                AddTab(dlg.TabName, dlg.Url);
            }
        }

        private async void AddTab(string name, string url)
        {
            sessionCounter++;
            var tabPage = new TabPage($"{name} #{sessionCounter}");
            
            var webView = new WebView2 { Dock = DockStyle.Fill };
            tabPage.Controls.Add(webView);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            // Per-profile userDataFolder to allow multiple sessions
            var dataDir = Environment.GetEnvironmentVariable("MSWPLUS_DATA_DIR")
                       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiSocialWebPlus");
            var profileDir = Path.Combine(dataDir, "WebView2Profiles", $"session_{sessionCounter}");
            Directory.CreateDirectory(profileDir);

            try
            {
                var env = await CoreWebView2Environment.CreateAsync(null, profileDir);
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 başlatılamadı: {ex.Message}\n\nWebView2 Runtime yüklü olduğundan emin olun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class CustomUrlDialog : Form
    {
        public string TabName { get; private set; } = "";
        public string Url { get; private set; } = "";
        
        private TextBox txtName, txtUrl;

        public CustomUrlDialog()
        {
            Text = "Özel URL Ekle";
            Size = new Size(400, 180);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            
            panel.Controls.Add(new Label { Text = "Sekme Adı:", AutoSize = true });
            txtName = new TextBox { Width = 350 };
            panel.Controls.Add(txtName);

            panel.Controls.Add(new Label { Text = "URL:", AutoSize = true });
            txtUrl = new TextBox { Width = 350, Text = "https://" };
            panel.Controls.Add(txtUrl);

            var btnPanel = new FlowLayoutPanel { Width = 350, Height = 40 };
            var btnOK = new Button { Text = "Ekle", Width = 75, DialogResult = DialogResult.OK };
            btnOK.Click += (s, e) => {
                TabName = txtName.Text;
                Url = txtUrl.Text;
            };
            var btnCancel = new Button { Text = "İptal", Width = 75, DialogResult = DialogResult.Cancel };
            btnPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });
            panel.Controls.Add(btnPanel);

            Controls.Add(panel);
            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }
    }
}
