using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace MultiSocialWeb
{
    public partial class MainForm : Form
    {
        private readonly string _dataRoot;
        private AppState _state = new AppState();
        private ThemeManager _theme = new ThemeManager(new BrandConfig());
        private ImageList _icons = new ImageList();
        private ContextMenuStrip _tabMenu = new ContextMenuStrip();
        private int _lastTabIndex = -1;

        // Split view state
        private TabPage _splitTab = null;
        private SplitContainer _splitContainer = null;
        private SplitSlot _splitLeft = null, _splitRight = null;

        private class SplitSlot
        {
            public int OriginalTabIndex;
            public TabPage OriginalPage;
            public InstanceInfo Inst;
            public WebView2 Web;
            public Control Placeholder;
        }

        public MainForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Font;
            MinimumSize = new Size(1000, 650);

            _dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiSocialWeb");
            Directory.CreateDirectory(_dataRoot);
            LoadState();
            _theme = new ThemeManager(_state.Brand);
            InitIcons();
            InitTabOwnerDraw();
            InitTabMenu();
            ApplyTheme();
            RefreshInstancesList();

            DoResponsiveLayout();
        }

        // ---------- Responsive layout core ----------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            DoResponsiveLayout();
        }

        private void DoResponsiveLayout()
        {
            if (rightPanel == null || tabs == null) return;
            int target = (int)Math.Round(ClientSize.Width * 0.32);
            target = Math.Max(360, Math.Min(560, target));
            rightPanel.Width = target;

            if (controlsPanel != null && flpActions != null && flpInputs != null)
            {
                var maxW = Math.Max(300, controlsPanel.ClientSize.Width - 24);
                flpActions.WrapContents = true;
                flpInputs.WrapContents  = true;
                flpActions.MaximumSize = new Size(maxW, 0);
                flpInputs.MaximumSize  = new Size(maxW, 0);
            }

            tabs.Multiline = true;
            int tabCount = Math.Max(1, tabs.TabCount);
            int perTab = Math.Max(130, Math.Min(220, (tabs.ClientSize.Width - 40) / Math.Min(tabCount, 6)));
            if (tabs.ItemSize.Width != perTab)
                tabs.ItemSize = new Size(perTab, 36);

            rightPanel.AutoScroll = true;
        }

        
        private void BtnToggleControls_Click(object sender, EventArgs e)
        {
            bool anyVisible = false;
            if (controlsPanel != null) anyVisible |= controlsPanel.Visible;
            if (rightPanel   != null) anyVisible |= rightPanel.Visible;
            bool visible = !anyVisible;

            if (controlsPanel != null) controlsPanel.Visible = visible;
            if (rightPanel   != null) rightPanel.Visible   = visible;

            if (btnToggleControls != null)
                btnToggleControls.Text = visible ? "⮜ Gizle" : "☰ Göster";

            _state.ControlsCollapsed = !visible;
            SaveState();
            DoResponsiveLayout();
        }
    

private void LoadState()
        {
            try
            {
                var path = Path.Combine(_dataRoot, "state.json");
                if (File.Exists(path))
                {
                    _state = JsonSerializer.Deserialize<AppState>(File.ReadAllText(path)) ?? new AppState();
                
            // Apply controls collapsed state
            try
            {
                controlsPanel.Visible = !_state.ControlsCollapsed;
                if (btnToggleControls != null)
                    btnToggleControls.Text = controlsPanel.Visible ? "⮜ Gizle" : "☰ Göster";
            }
            catch { }
    
        }
            }
            catch { _state = new AppState(); }
        }

        private void SaveState()
        {
            try
            {
                var path = Path.Combine(_dataRoot, "state.json");
                File.WriteAllText(path, JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private void InitIcons()
        {
            _icons = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            ServiceIcons.LoadInto(_icons);
            tabs.ImageList = _icons;
            listInstances.SmallImageList = _icons;
        }

        private void ApplyTheme()
        {
            var c = _theme.Colors;

            BackColor = c.AppBg;
            tabs.BackColor = c.PanelBg;
            rightPanel.BackColor = c.PanelBg;
            controlsPanel.BackColor = c.PanelBg;
            flpActions.BackColor = c.PanelBg;
            flpInputs.BackColor = c.PanelBg;

            topBar.BackColor = c.Primary;
            lblTitle.ForeColor = _theme.TextOn(c.Primary);
            lblSubtitle.ForeColor = _theme.TextOn(c.Primary);
            if (!string.IsNullOrWhiteSpace(_state.Brand.LogoPath) && File.Exists(_state.Brand.LogoPath))
            {
                try
                {
                    using (var img = Image.FromFile(_state.Brand.LogoPath))
                    {
                        logoBox.Image = new Bitmap(img, new Size(28, 28));
                    }
                }
                catch { logoBox.Image = null; }
            }
            else
            {
                logoBox.Image = ServiceIcons.DrawAppLogo(c);
            }

            foreach (var lab in new[] { lblCount, labelUrl, labelName, labelPrimary, labelDark, labelHighContrast, labelLogo })
            {
                lab.ForeColor = _theme.TextForBg(c.PanelBg);
                lab.BackColor = c.PanelBg;
            }

            foreach (var tb in new[] { txtCustomUrl, txtCustomName })
            {
                tb.BackColor = c.InputBg;
                tb.ForeColor = _theme.TextForBg(c.InputBg);
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Font = new Font(new FontFamily("Segoe UI"), 10f);
            }

            listInstances.BackColor = c.PanelBg;
            listInstances.ForeColor = _theme.TextForBg(c.PanelBg);
            listInstances.Font = new Font(new FontFamily("Segoe UI"), 9f);

            StylePrimary(btnNewWhatsApp);
            StylePrimary(btnNewInstagram);
            StylePrimary(btnNewFacebook);
            StylePrimary(btnNewCustom);

            StyleActionArea();

            tabs.ForeColor = _theme.TextForBg(c.PanelBg);
            tabs.Invalidate();
        }

        private void StyleActionArea()
        {
            var c = _theme.Colors;
            Color altBg = _theme.AltBg();

            foreach (var b in new[] { btnOpenSelected, btnRename, btnOpenAll, btnPickColor, btnResetTheme, btnPickLogo })
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = c.Border;
                b.UseVisualStyleBackColor = false;
                b.BackColor = altBg;
                b.ForeColor = _theme.TextForBg(altBg);
                b.Font = new Font(new FontFamily("Segoe UI"), 10f, FontStyle.Bold);
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.Padding = new Padding(12, 8, 12, 8);
                b.MinimumSize = new Size(110, 40);
                b.AutoSize = true;
                b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                b.Cursor = Cursors.Hand;
                b.Margin = new Padding(6);
            }

            StyleDanger(btnDeleteSelected);

            StylePrimary(btnSafePreset);
            btnSafePreset.AutoSize = true;
            btnSafePreset.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void StylePrimary(Button b)
        {
            var c = _theme.Colors;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = c.Primary;
            b.ForeColor = _theme.TextOn(c.Primary);
            b.Font = new Font(new FontFamily("Segoe UI"), 10f, FontStyle.Bold);
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.Padding = new Padding(12, 8, 12, 8);
            b.MinimumSize = new Size(110, 40);
            b.Cursor = Cursors.Hand;
            b.Margin = new Padding(6, 6, 6, 6);
        }

        private void StyleDanger(Button b)
        {
            var danger = Color.FromArgb(220, 38, 38);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 30, 30);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 25, 25);
            b.BackColor = danger;
            b.ForeColor = _theme.TextOn(danger);
            b.Font = new Font(new FontFamily("Segoe UI"), 10f, FontStyle.Bold);
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.Padding = new Padding(14, 8, 14, 8);
            b.MinimumSize = new Size(110, 40);
            b.AutoSize = true;
            b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            b.Cursor = Cursors.Hand;
            b.Margin = new Padding(6, 6, 6, 6);
        }

        private void InitTabOwnerDraw()
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(180, 36);
            tabs.DrawItem += (s, e) =>
            {
                var c = _theme.Colors;
                var page = tabs.TabPages[e.Index];
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var bg = selected ? Color.FromArgb(
                        Math.Max(0, c.PanelBg.R - 8),
                        Math.Max(0, c.PanelBg.G - 8),
                        Math.Max(0, c.PanelBg.B - 8)) : c.PanelBg;
                using (var bgBr = new SolidBrush(bg))
                    e.Graphics.FillRectangle(bgBr, e.Bounds);

                int x = e.Bounds.Left + 8;
                var inst = _state.Instances.FirstOrDefault(i => i.Name == page.Text);
                bool pinned = inst != null && inst.IsPinned;
                if (pinned)
                {
                    ServiceIcons.DrawPin(e.Graphics, new Rectangle(x, e.Bounds.Top + (e.Bounds.Height-12)/2, 12, 12), _theme.TextForBg(bg));
                    x += 16;
                }

                if (_icons != null && page.ImageKey != null && _icons.Images.ContainsKey(page.ImageKey))
                {
                    e.Graphics.DrawImage(_icons.Images[page.ImageKey], new Rectangle(x, e.Bounds.Top + (e.Bounds.Height-16)/2, 16, 16));
                    x += 20;
                }

                if (inst != null && inst.IsMuted)
                {
                    ServiceIcons.DrawSpeakerMuted(e.Graphics, new Rectangle(x, e.Bounds.Top + (e.Bounds.Height-12)/2, 12, 12), _theme.TextForBg(bg));
                    x += 16;
                }

                var txtColor = _theme.TextForBg(bg);
                var textRect = new Rectangle(x, e.Bounds.Top, e.Bounds.Width - x - 28, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, page.Text, e.Font, textRect,
                    txtColor, bg, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                if (!pinned)
                {
                    var crossRect = GetCloseRect(e.Bounds);
                    using (var pen = new Pen(txtColor, 1.6f))
                    {
                        e.Graphics.DrawLine(pen, crossRect.Left, crossRect.Top, crossRect.Right, crossRect.Bottom);
                        e.Graphics.DrawLine(pen, crossRect.Left, crossRect.Bottom, crossRect.Right, crossRect.Top);
                    }
                }

                e.DrawFocusRectangle();
            };

            tabs.MouseDown += Tabs_MouseDown;
            tabs.MouseUp += Tabs_MouseUp;
        }

        private Rectangle GetCloseRect(Rectangle tabBounds)
        {
            return new Rectangle(tabBounds.Right - 18, tabBounds.Top + (tabBounds.Height - 12) / 2, 12, 12);
        }

        private void InitTabMenu()
        {
            _tabMenu = new ContextMenuStrip();
            _tabMenu.Items.Add("Yenile", null, (s, e) => RefreshTab(_lastTabIndex));
            _tabMenu.Items.Add("Yeniden Adlandır", null, (s, e) => RenameTab(_lastTabIndex));
            _tabMenu.Items.Add("Sabitle / Sabiti Kaldır", null, (s, e) => TogglePin(_lastTabIndex));
            _tabMenu.Items.Add(new ToolStripSeparator());
            _tabMenu.Items.Add("Sessize Al / Sesi Aç", null, (s, e) => ToggleMute(_lastTabIndex));
            _tabMenu.Items.Add(new ToolStripSeparator());
            _tabMenu.Items.Add("Split: Solda Göster", null, (s, e) => MoveToSplit(_lastTabIndex, true));
            _tabMenu.Items.Add("Split: Sağda Göster", null, (s, e) => MoveToSplit(_lastTabIndex, false));
            _tabMenu.Items.Add("Split: Bu Sekmeyi Çıkar", null, (s, e) => RemoveFromSplit(_lastTabIndex));
            _tabMenu.Items.Add("Split'i Kapat", null, (s, e) => CloseSplit());
        }

        private void Tabs_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabs.TabCount; i++)
            {
                var r = tabs.GetTabRect(i);
                if (r.Contains(e.Location))
                {
                    _lastTabIndex = i;
                    var inst = GetInstanceByTabIndex(i);
                    if (e.Button == MouseButtons.Left)
                    {
                        if (inst != null && !inst.IsPinned && GetCloseRect(r).Contains(e.Location))
                        {
                            CloseTab(i);
                            return;
                        }
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        _tabMenu.Show(tabs, e.Location);
                    }
                }
            }
        }
        private void Tabs_MouseUp(object sender, MouseEventArgs e) { }

        private InstanceInfo GetInstanceByTabIndex(int index)
        {
            if (index < 0 || index >= tabs.TabCount) return null;
            var page = tabs.TabPages[index];
            return _state.Instances.FirstOrDefault(i => i.Name == page.Text);
        }

        private WebView2 GetWebViewOfTab(int index)
        {
            if (index < 0 || index >= tabs.TabCount) return null;
            var page = tabs.TabPages[index];
            return page.Controls.OfType<WebView2>().FirstOrDefault();
        }

        private WebView2 GetWebViewOfInstance(InstanceInfo inst, int tabIndex)
        {
            var wv = GetWebViewOfTab(tabIndex);
            if (wv != null) return wv;
            if (_splitLeft != null && _splitLeft.Inst == inst) return _splitLeft.Web;
            if (_splitRight != null && _splitRight.Inst == inst) return _splitRight.Web;
            return null;
        }

        private void RefreshTab(int index)
        {
            var wv = GetWebViewOfTab(index);
            if (wv != null) wv.Reload();
        }

        private void RenameTab(int index)
        {
            if (index < 0 || index >= tabs.TabCount) return;
            var page = tabs.TabPages[index];
            var inst = _state.Instances.FirstOrDefault(i => i.Name == page.Text);
            if (inst == null) return;
            var s = Microsoft.VisualBasic.Interaction.InputBox("Yeni ad:", "Yeniden Adlandır", inst.Name);
            if (!string.IsNullOrWhiteSpace(s))
            {
                inst.Name = s.Trim();
                page.Text = inst.Name;
                SaveState();
                tabs.Invalidate();
            }
        }

        private void TogglePin(int index)
        {
            var inst = GetInstanceByTabIndex(index);
            if (inst == null) return;
            inst.IsPinned = !inst.IsPinned;
            SaveState();
            ReorderTabs();
            tabs.Invalidate();
        }

        private void ToggleMute(int index)
        {
            var inst = GetInstanceByTabIndex(index);
            if (inst == null) return;
            inst.IsMuted = !inst.IsMuted;
            SaveState();
            tabs.Invalidate();

            var wv = GetWebViewOfInstance(inst, index);
            if (wv != null && wv.CoreWebView2 != null)
            {
                try { wv.CoreWebView2.IsMuted = inst.IsMuted; } catch { }
                try 
                { 
                    string js = "(function(){try{document.querySelectorAll('video,audio').forEach(e=>e.muted=" 
                                + (inst.IsMuted ? "true" : "false") 
                                + ");}catch(e){}})();";
                    wv.CoreWebView2.ExecuteScriptAsync(js);
                } 
                catch { }
            }
        }

        private void CloseTab(int index)
        {
            var inst = GetInstanceByTabIndex(index);
            if (inst != null && inst.IsPinned) return;
            if (index < 0 || index >= tabs.TabCount) return;
            var page = tabs.TabPages[index];
            foreach (var wv in page.Controls.OfType<WebView2>().ToList()) wv.Dispose();
            tabs.TabPages.RemoveAt(index);
        }

        private void ReorderTabs()
        {
            var pages = tabs.TabPages.Cast<TabPage>().ToList();
            tabs.TabPages.Clear();
            foreach (var p in pages.OrderByDescending(p => _state.Instances.Any(i => i.Name == p.Text && i.IsPinned)))
                tabs.TabPages.Add(p);
        }

        // -------- Split View --------
        private void EnsureSplitTab()
        {
            if (_splitTab != null && tabs.TabPages.Contains(_splitTab) && _splitContainer != null) return;

            if (_splitTab == null || !tabs.TabPages.Contains(_splitTab))
            {
                _splitTab = new TabPage("Yan Yana");
                _splitTab.ImageKey = "custom";
                tabs.TabPages.Add(_splitTab);
            }

            if (_splitContainer == null || _splitContainer.IsDisposed)
            {
                _splitContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterDistance = Math.Max(200, tabs.ClientSize.Width / 2),
                    Panel1MinSize = 200,
                    Panel2MinSize = 200
                };
                _splitTab.Controls.Clear();
                _splitTab.Controls.Add(_splitContainer);
            }
        }

        private void MoveToSplit(int index, bool left)
        {
            try
            {
                if (index < 0 || index >= tabs.TabCount) return;
                var page = tabs.TabPages[index];

                if (_splitTab != null && page == _splitTab) { tabs.SelectedTab = _splitTab; return; }

                var inst = _state.Instances.FirstOrDefault(i => i.Name == page.Text);

                if (inst == null)
                {
                    if ((_splitLeft != null && _splitLeft.OriginalPage == page) || (_splitRight != null && _splitRight.OriginalPage == page))
                    {
                        EnsureSplitTab();
                        tabs.SelectedTab = _splitTab;
                    }
                    return;
                }

                if ((_splitLeft != null && _splitLeft.Inst == inst) || (_splitRight != null && _splitRight.Inst == inst))
                {
                    EnsureSplitTab();
                    tabs.SelectedTab = _splitTab;
                    return;
                }

                var wv = page.Controls.OfType<WebView2>().FirstOrDefault();
                if (wv == null) return;

                EnsureSplitTab();
                tabs.SelectedTab = _splitTab;

                if (left && _splitLeft != null) ReturnFromSplit(_splitLeft);
                if (!left && _splitRight != null) ReturnFromSplit(_splitRight);

                var placeholder = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "Bu sekme 'Yan Yana' görünümünde gösteriliyor.",
                    ForeColor = Color.Gray
                };

                page.Controls.Remove(wv);
                page.Controls.Add(placeholder);

                var slot = new SplitSlot { OriginalTabIndex = index, OriginalPage = page, Inst = inst, Web = wv, Placeholder = placeholder };
                if (left)
                {
                    _splitContainer.Panel1.Controls.Clear();
                    _splitContainer.Panel1.Controls.Add(wv);
                    wv.Dock = DockStyle.Fill;
                    _splitLeft = slot;
                }
                else
                {
                    _splitContainer.Panel2.Controls.Clear();
                    _splitContainer.Panel2.Controls.Add(wv);
                    wv.Dock = DockStyle.Fill;
                    _splitRight = slot;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Split işleminde bir sorun oluştu:\n" + ex.Message, "Split", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveFromSplit(int index)
        {
            if (index < 0 || index >= tabs.TabCount) return;
            var page = tabs.TabPages[index];
            var inst = _state.Instances.FirstOrDefault(i => i.Name == page.Text);
            if (inst == null) return;
            if (_splitLeft != null && _splitLeft.Inst == inst) { ReturnFromSplit(_splitLeft); _splitLeft = null; }
            if (_splitRight != null && _splitRight.Inst == inst) { ReturnFromSplit(_splitRight); _splitRight = null; }
            MaybeCloseSplitTab();
        }

        private void ReturnFromSplit(SplitSlot slot)
        {
            if (slot.Web.Parent != null) slot.Web.Parent.Controls.Remove(slot.Web);
            if (slot.Placeholder != null) slot.OriginalPage.Controls.Remove(slot.Placeholder);
            slot.OriginalPage.Controls.Add(slot.Web);
            slot.Web.Dock = DockStyle.Fill;
        }

        private void CloseSplit()
        {
            if (_splitLeft != null) { ReturnFromSplit(_splitLeft); _splitLeft = null; }
            if (_splitRight != null) { ReturnFromSplit(_splitRight); _splitRight = null; }
            MaybeCloseSplitTab();
        }

        private void MaybeCloseSplitTab()
        {
            if (_splitLeft == null && _splitRight == null && _splitTab != null)
            {
                if (_splitContainer != null)
                {
                    _splitContainer.Panel1.Controls.Clear();
                    _splitContainer.Panel2.Controls.Clear();
                }
                tabs.TabPages.Remove(_splitTab);
                _splitTab.Dispose();
                _splitTab = null;
                _splitContainer = null;
            }
        }

        private void RefreshInstancesList()
        {
            listInstances.Items.Clear();
            foreach (var inst in _state.Instances.OrderBy(i => i.CreatedAt))
            {
                var item = new ListViewItem(new[] { inst.Name, inst.Service, inst.Url, inst.UserDataFolder });
                item.Tag = inst;
                item.ImageKey = ServiceIcons.KeyFor(inst);
                listInstances.Items.Add(item);
            }
            lblCount.Text = $"Toplam: {_state.Instances.Count} oturum";
        }

        private async void btnNewWhatsApp_Click(object sender, EventArgs e) => await CreateInstance("WhatsApp", "https://web.whatsapp.com");
        private async void btnNewInstagram_Click(object sender, EventArgs e) => await CreateInstance("Instagram", "https://www.instagram.com");
        private async void btnNewFacebook_Click(object sender, EventArgs e) => await CreateInstance("Facebook", "https://www.facebook.com");

        private async void btnNewCustom_Click(object sender, EventArgs e)
        {
            var url = txtCustomUrl.Text.Trim();
            var name = txtCustomName.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Lütfen bir URL girin (https://...)", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Regex.IsMatch(url, @"^https?://", RegexOptions.IgnoreCase))
            {
                url = "https://" + url;
            }
            if (string.IsNullOrWhiteSpace(name)) name = url;
            await CreateInstance("Custom", url, name);
        }

        private async Task CreateInstance(string service, string url, string customName = null)
        {
            var id = Guid.NewGuid().ToString("N");
            var folder = Path.Combine(_dataRoot, "Profiles", service, id);
            Directory.CreateDirectory(folder);
            var name = !string.IsNullOrWhiteSpace(customName) ? customName : $"{service} #{_state.Instances.Count(i => i.Service == service) + 1}";
            var inst = new InstanceInfo
            {
                Id = id,
                Name = name,
                Service = service,
                Url = url,
                UserDataFolder = folder,
                CreatedAt = DateTime.UtcNow,
                IsPinned = false,
                IsMuted = false
            };
            _state.Instances.Add(inst);
            SaveState();
            RefreshInstancesList();
            await OpenInTab(inst);
        }

        private async Task OpenInTab(InstanceInfo inst)
        {
            var page = new TabPage(inst.Name);
            page.ImageKey = ServiceIcons.KeyFor(inst);
            var webView = new WebView2 { Dock = DockStyle.Fill };
            page.Controls.Add(webView);
            tabs.TabPages.Add(page);
            tabs.SelectedTab = page;

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: inst.UserDataFolder);
            await webView.EnsureCoreWebView2Async(env);
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView.CoreWebView2.Settings.IsStatusBarEnabled = true;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            webView.Source = new Uri(inst.Url);

            webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                await TrySetFaviconAsync(inst, webView, page);
            };

            DoResponsiveLayout();
        }

        private async Task TrySetFaviconAsync(InstanceInfo inst, WebView2 webView, TabPage page)
        {
            try
            {
                string hrefJson = await webView.CoreWebView2.ExecuteScriptAsync(@"(function(){try{var m=document.querySelector('link[rel*=icon]');return m? new URL(m.href, document.baseURI).href : ''; }catch(e){ return '';}})();");
                var href = (hrefJson ?? "").Trim('"').Trim();
                if (string.IsNullOrEmpty(href))
                {
                    var baseUri = webView.Source ?? new Uri(inst.Url);
                    href = new Uri(baseUri, "/favicon.ico").ToString();
                }
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(5);
                    var bytes = await http.GetByteArrayAsync(href);
                    using (var ms = new MemoryStream(bytes))
                    {
                        Image img;
                        try
                        {
                            using (var ico = new Icon(ms, new Size(16, 16)))
                                img = ico.ToBitmap();
                        }
                        catch
                        {
                            ms.Position = 0;
                            img = Image.FromStream(ms);
                            img = new Bitmap(img, new Size(16, 16));
                        }
                        var key = $"fav_{inst.Id}";
                        if (_icons.Images.ContainsKey(key)) _icons.Images.RemoveByKey(key);
                        _icons.Images.Add(key, img);
                        page.ImageKey = key;
                        tabs.Invalidate();
                    }
                }
            }
            catch { /* ignore */ }
        }

        private async void btnOpenSelected_Click(object sender, EventArgs e)
        {
            if (listInstances.SelectedItems.Count == 0) return;
            foreach (ListViewItem item in listInstances.SelectedItems)
            {
                var inst = item.Tag as InstanceInfo;
                if (inst != null) await OpenInTab(inst);
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (listInstances.SelectedItems.Count == 0) return;
            var confirm = MessageBox.Show("Seçili oturum(lar) silinecek. İlgili profil klasörleri de silinebilir. Devam?",
                "Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            foreach (ListViewItem item in listInstances.SelectedItems)
            {
                var inst = item.Tag as InstanceInfo;
                if (inst != null)
                {
                    try
                    {
                        if (Directory.Exists(inst.UserDataFolder))
                            Directory.Delete(inst.UserDataFolder, true);
                    }
                    catch { }
                    _state.Instances.Remove(inst);
                }
            }
            SaveState();
            RefreshInstancesList();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            if (listInstances.SelectedItems.Count != 1) return;
            var inst = (InstanceInfo)listInstances.SelectedItems[0].Tag;
            var input = Microsoft.VisualBasic.Interaction.InputBox("Yeni ad:", "Yeniden Adlandır", inst.Name);
            if (!string.IsNullOrWhiteSpace(input))
            {
                inst.Name = input.Trim();
                SaveState();
                RefreshInstancesList();
                foreach (TabPage p in tabs.TabPages)
                {
                    if (p.Text == inst.Name) { p.Text = inst.Name; break; }
                }
            }
        }

        private void btnOpenAll_Click(object sender, EventArgs e) { _ = OpenAllAsync(); }
        private async Task OpenAllAsync()
        {
            foreach (var inst in _state.Instances)
            {
                await OpenInTab(inst);
                await Task.Delay(100);
            }
        }

        private void btnPickColor_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _state.Brand.PrimaryHex = ColorTranslator.ToHtml(dlg.Color);
                    _theme = new ThemeManager(_state.Brand);
                    SaveState();
                    ApplyTheme();
                }
            }
        }

        private void btnPickLogo_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Görüntüler|*.png;*.jpg;*.jpeg;*.ico|Tüm Dosyalar|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _state.Brand.LogoPath = dlg.FileName;
                    SaveState();
                    ApplyTheme();
                }
            }
        }

        private void chkDark_CheckedChanged(object sender, EventArgs e)
        {
            _state.Brand.DarkMode = chkDark.Checked;
            _theme = new ThemeManager(_state.Brand);
            SaveState();
            ApplyTheme();
        }

        private void chkHighContrast_CheckedChanged(object sender, EventArgs e)
        {
            _state.Brand.HighContrast = chkHighContrast.Checked;
            _theme = new ThemeManager(_state.Brand);
            SaveState();
            ApplyTheme();
        }

        private void btnResetTheme_Click(object sender, EventArgs e)
        {
            _state.Brand = new BrandConfig();
            _theme = new ThemeManager(_state.Brand);
            SaveState();
            ApplyTheme();
        }

        private void btnSafePreset_Click(object sender, EventArgs e)
        {
            _state.Brand = new BrandConfig
            {
                PrimaryHex = "#0ea5e9",
                DarkMode = false,
                HighContrast = true
            };
            _theme = new ThemeManager(_state.Brand);
            SaveState();
            ApplyTheme();
        }
    
    private void btnOpenPriceList_Click(object sender, EventArgs e)
            {
                using (var dlg = new PriceListForm())
                    dlg.ShowDialog(this);
            }

    private void btnOpenQuote_Click(object sender, EventArgs e)
            {
                using (var dlg = new QuoteForm())
                    dlg.ShowDialog(this);
            }
}

    public class AppState
    {
        public List<InstanceInfo> Instances { get; set; } = new List<InstanceInfo>();
        public BrandConfig Brand { get; set; } = new BrandConfig();
        public bool ControlsCollapsed { get; set; } = false;
    }

    public class BrandConfig
    {
        public string PrimaryHex { get; set; } = "#0ea5e9";
        public bool DarkMode { get; set; } = false;
        public bool HighContrast { get; set; } = true;
        public string LogoPath { get; set; } = null;
    }

    public class InstanceInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Service { get; set; } = "";
        public string Url { get; set; } = "";
        public string UserDataFolder { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsPinned { get; set; } = false;
        public bool IsMuted { get; set; } = false;
    }

    public class ColorScheme
    {
        public Color Primary { get; set; }
        public Color AppBg { get; set; }
        public Color PanelBg { get; set; }
        public Color Text { get; set; }
        public Color Subtext { get; set; }
        public Color Border { get; set; }
        public Color InputBg { get; set; }
    }

    public class ThemeManager
    {
        public ColorScheme Colors { get; private set; }
        private readonly BrandConfig _brand;

        public ThemeManager(BrandConfig brand)
        {
            _brand = brand ?? new BrandConfig();
            Colors = Build();
        }

        public Color TextOn(Color bg)
        {
            var white = Color.White;
            var black = Color.Black;
            var cWhite = ContrastRatio(bg, white);
            var cBlack = ContrastRatio(bg, black);
            return cWhite >= cBlack ? Color.White : Color.Black;
        }

        public Color TextForBg(Color bg)
        {
            var black = Color.FromArgb(17, 24, 39);
            var white = Color.White;
            var cb = ContrastRatio(black, bg);
            var cw = ContrastRatio(white, bg);
            if (cb >= 7.0 && cw >= 7.0) return cb >= cw ? black : white;
            if (cb >= 7.0) return black;
            if (cw >= 7.0) return white;
            return cb >= cw ? black : white;
        }

        public Color AltBg()
        {
            var p = Colors.PanelBg;
            return Lighten(p, Colors.PanelBg.GetBrightness() > 0.5f ? -0.03f : 0.06f);
        }

        private ColorScheme Build()
        {
            var primary = ColorTranslator.FromHtml(_brand.PrimaryHex ?? "#0ea5e9");
            bool dark = _brand.DarkMode;

            Color appBg, panelBg, text, sub, border, inputBg;
            if (dark)
            {
                appBg = Color.FromArgb(18, 20, 23);
                panelBg = Color.FromArgb(28, 31, 36);
                text   = Color.FromArgb(241, 245, 249);
                sub    = Color.FromArgb(148, 163, 184);
                border = Color.FromArgb(42, 47, 54);
                inputBg= Color.FromArgb(38, 41, 47);
            }
            else
            {
                appBg = Color.FromArgb(245, 246, 248);
                panelBg = Color.White;
                text   = Color.FromArgb(17, 24, 39);
                sub    = Color.FromArgb(75, 85, 99);
                border = Color.FromArgb(221, 226, 232);
                inputBg= Color.White;
            }

            if (_brand.HighContrast) text = EnsureContrast(text, panelBg, 7.0);

            return new ColorScheme
            {
                Primary = primary,
                AppBg = appBg,
                PanelBg = panelBg,
                Text = text,
                Subtext = sub,
                Border = border,
                InputBg = inputBg
            };
        }

        public static double ContrastRatio(Color a, Color b)
        {
            double la = RelativeLuminance(a);
            double lb = RelativeLuminance(b);
            var L1 = Math.Max(la, lb);
            var L2 = Math.Min(la, lb);
            return (L1 + 0.05) / (L2 + 0.05);
        }

        public static double RelativeLuminance(Color c)
        {
            double RsRGB = c.R / 255.0;
            double GsRGB = c.G / 255.0;
            double BsRGB = c.B / 255.0;

            double R = RsRGB <= 0.03928 ? RsRGB / 12.92 : Math.Pow((RsRGB + 0.055) / 1.055, 2.4);
            double G = GsRGB <= 0.03928 ? GsRGB / 12.92 : Math.Pow((GsRGB + 0.055) / 1.055, 2.4);
            double B = BsRGB <= 0.03928 ? BsRGB / 12.92 : Math.Pow((BsRGB + 0.055) / 1.055, 2.4);

            return 0.2126 * R + 0.7152 * G + 0.0722 * B;
        }

        public static Color Lighten(Color c, float amount)
        {
            int r = Clamp(c.R + (int)(255 * amount));
            int g = Clamp(c.G + (int)(255 * amount));
            int b = Clamp(c.B + (int)(255 * amount));
            return Color.FromArgb(r, g, b);
        }

        public static Color EnsureContrast(Color fg, Color bg, double minRatio)
        {
            if (ContrastRatio(fg, bg) >= minRatio) return fg;
            var white = Color.White;
            var black = Color.Black;
            var cw = ContrastRatio(white, bg);
            var cb = ContrastRatio(black, bg);
            return cw >= cb ? white : black;
        }

        private static int Clamp(int v) { return Math.Min(255, Math.Max(0, v)); }
    }

    public static class ServiceIcons
    {
        public static void LoadInto(ImageList list)
        {
            list.Images.Add("whatsapp", DrawWhatsApp());
            list.Images.Add("instagram", DrawInstagram());
            list.Images.Add("facebook", DrawFacebook());
            list.Images.Add("youtube", DrawYouTube());
            list.Images.Add("custom", DrawGlobe());
        }

        public static Bitmap DrawAppLogo(ColorScheme c)
        {
            var bmp = new Bitmap(28, 28);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var br = new SolidBrush(c.Primary))
                    g.FillEllipse(br, 0, 0, 28, 28);
                using (var f = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("MS", f, Brushes.White, new RectangleF(0, 0, 28, 28), sf);
            }
            return bmp;
        }

        public static string KeyFor(InstanceInfo inst)
        {
            var s = (inst.Service ?? "").ToLowerInvariant();
            if (s.Contains("whatsapp")) return "whatsapp";
            if (s.Contains("instagram")) return "instagram";
            if (s.Contains("facebook")) return "facebook";
            var url = (inst.Url ?? "").ToLowerInvariant();
            if (url.Contains("youtube")) return "youtube";
            return "custom";
        }

        private static Bitmap DrawWhatsApp()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var green = Color.FromArgb(37, 211, 102);
                g.Clear(Color.Transparent);
                using (var br = new SolidBrush(green))
                    g.FillEllipse(br, 0, 0, 16, 16);
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawArc(pen, 4, 3, 8, 8, 140, 230);
                    g.DrawLine(pen, 8, 10, 7, 13);
                }
            }
            return bmp;
        }

        private static Bitmap DrawInstagram()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                var bg = Color.FromArgb(193, 53, 132);
                using (var br = new SolidBrush(bg))
                    g.FillRectangle(br, 1, 1, 14, 14);
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawRectangle(pen, 3, 3, 10, 10);
                    g.DrawEllipse(pen, 6, 6, 4, 4);
                }
                g.FillEllipse(Brushes.White, 10, 4, 2, 2);
            }
            return bmp;
        }

        private static Bitmap DrawFacebook()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var blue = Color.FromArgb(24, 119, 242);
                g.Clear(Color.Transparent);
                using (var br = new SolidBrush(blue))
                    g.FillRectangle(br, 0, 0, 16, 16);
                using (var f = new Font(new FontFamily("Segoe UI"), 10, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("f", f, Brushes.White, new RectangleF(0, 0, 16, 16), sf);
            }
            return bmp;
        }

        private static Bitmap DrawYouTube()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var br = new SolidBrush(Color.FromArgb(230, 33, 23)))
                    g.FillRectangle(br, 1, 3, 14, 10);
                Point[] tri = { new Point(6, 6), new Point(6, 10), new Point(10, 8) };
                g.FillPolygon(Brushes.White, tri);
            }
            return bmp;
        }

        private static Bitmap DrawGlobe()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                var c = Color.FromArgb(100, 116, 139);
                using (var pen = new Pen(c, 1.8f))
                {
                    g.DrawEllipse(pen, 2, 2, 12, 12);
                    g.DrawLine(pen, 8, 2, 8, 14);
                    g.DrawEllipse(pen, 2, 5, 12, 6);
                }
            }
            return bmp;
        }

        public static void DrawPin(Graphics g, Rectangle r, Color color)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var br  = new SolidBrush(color))
            using (var pen = new Pen(color, 1.6f))
            {
                var head = new Rectangle(r.Left, r.Top, Math.Max(4, r.Width / 2), Math.Max(4, r.Height / 2));
                g.FillEllipse(br, head);
                int cx = head.Left + head.Width / 2;
                Point top = new Point(cx, head.Bottom - 1);
                Point bottom = new Point(r.Right - 1, r.Bottom - 1);
                g.DrawLine(pen, top, bottom);
            }
        }

        public static void DrawSpeakerMuted(Graphics g, Rectangle r, Color color)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, 1.6f))
            using (var br = new SolidBrush(color))
            {
                var spk = new Rectangle(r.Left, r.Top + r.Height/4, r.Width/3, r.Height/2);
                g.FillRectangle(br, spk);
                Point[] horn = { new Point(spk.Right, spk.Top), new Point(r.Left + r.Width*2/3, r.Top + r.Height/2), new Point(spk.Right, spk.Bottom) };
                g.FillPolygon(br, horn);
                g.DrawLine(pen, r.Left + r.Width*2/3, r.Top + 2, r.Right - 2, r.Bottom - 2);
                g.DrawLine(pen, r.Left + r.Width*2/3, r.Bottom - 2, r.Right - 2, r.Top + 2);
            }
        }
    }

        

        

}
