
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MultiSocialWeb
{
    public class PriceListForm : Form
    {
        private DataGridView grid;
        private Button btnAddRow, btnDeleteRow, btnExport;
        private string _logoPath = null;

        // --- Kategori filtresi için alanlar ---
        private ComboBox cbFilterCategory;
        private ComboBox cbFilterSubCategory;

        // --- Sticky selection (seçimlerin kaybolmaması) ---
        private readonly Dictionary<string,bool> _rowSelection = new Dictionary<string,bool>();
        private bool _suppressFilterEvents = false;
        private string _activeFilterCategory = "Tümü";
        private string _activeFilterSubCategory = "Tümü";

        // (Projedeki mevcut alanlar)
        private Dictionary<string,bool> _selectedKeys = new Dictionary<string,bool>();
        private bool _isApplyingFilter = false;
        private bool _isRendering = false;
        private List<string[]> _allRowsRaw = new List<string[]>();
private const string DataDirName = "MultiSocialWeb";
        private const string DataFileName = "price_list_data.csv";
        private string DataDirPath  => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataDirName);
        private string DataFilePath => Path.Combine(DataDirPath, DataFileName);
        private string SettingsFilePath => Path.Combine(DataDirPath, "price_list_settings.json");

        private readonly Dictionary<string, string[]> _subcats = new Dictionary<string, string[]>
        {
            ["Plise Sineklik"] = new [] { "Alüminyum", "Tül", "Aksesuar", "Yardımcı malzemeler", "Makinalar" },
            ["Plise Perde"]    = new [] { "Profil", "Kumaş", "Aksesuar", "Yardımcı malzemeler", "Makinalar" }
        };

        private TextBox HeaderBox
        {
            get
            {
                foreach (Control c in this.Controls)
                {
                    if (c is FlowLayoutPanel flp)
                    {
                        foreach (Control cc in flp.Controls)
                            if (cc is TextBox tb && tb.Name == "txtHeaderInfo") return tb;
                    }
                }
                return null;
            }
        }

        public PriceListForm()
        {
            Text = "Fiyat Listesi";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1180; Height = 680;

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Clear();

            var colImagePath = new DataGridViewTextBoxColumn { HeaderText = "ImagePath", Name = "colImagePath", Visible = false };
            grid.Columns.Add(colImagePath);

            var colImage = new DataGridViewImageColumn { HeaderText = "Ürün Görseli", Name = "colImage", ImageLayout = DataGridViewImageCellLayout.Zoom };
            grid.RowTemplate.Height = 56;
            grid.Columns.Add(colImage);

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ürün Adı", Name = "colName" });

            var colUnit = new DataGridViewComboBoxColumn { HeaderText = "Birim", Name = "colUnit" };
            colUnit.Items.AddRange(new object[] { "kg", "metrekare", "metre", "adet" });
            colUnit.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            grid.Columns.Add(colUnit);

            // Checkbox selection column
            var colSelect = new DataGridViewCheckBoxColumn { HeaderText = "", Name = "colSelect", Width = 28 };
            grid.Columns.Insert(0, colSelect);

            var colCategory = new DataGridViewComboBoxColumn { HeaderText = "Kategori", Name = "colCategory", DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton };
            colCategory.Items.AddRange(new object[] { "Plise Sineklik", "Plise Perde" });
            grid.Columns.Add(colCategory);

            var colSubCategory = new DataGridViewComboBoxColumn { HeaderText = "Alt Kategori", Name = "colSubCategory", DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton };
            colSubCategory.Items.Add("Tümü");
            grid.Columns.Add(colSubCategory);

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Renk", Name = "colColor" });

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat", Name = "colPrice" });

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 86, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };
            btnAddRow    = new Button { Text = "Ürün Ekle", Width = 110, Height = 28 };
            btnDeleteRow = new Button { Text = "Satırı Sil", Width = 110, Height = 28 };
            btnExport    = new Button { Text = "Dışa Aktar (CSV)", Width = 140, Height = 28 };
            var btnAddImage   = new Button { Text = "Resim Ekle", Width = 110, Height = 28 };
            var btnExportPdf  = new Button { Text = "PDF (Logo+Başlık)", Width = 150, Height = 28 };
            var btnChooseLogo = new Button { Text = "Logo Seç", Width = 100, Height = 28 };

            var lblCurr   = new Label { Text = " Para Birimi:", AutoSize = true, Padding = new Padding(12, 6, 0, 0) };
            var cbCurrency = new ComboBox { Name = "cbCurrency", Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCurrency.Items.AddRange(new object[] { "TRY", "USD", "EUR" });
            cbCurrency.SelectedIndex = 0;

            var lblHeader = new Label { Text = "Başlık / Firma Bilgisi:", AutoSize = true, Padding = new Padding(12, 6, 0, 0) };
            var txtHeader = new TextBox { Name = "txtHeaderInfo", Width = 360, Height = 56, Multiline = true, ScrollBars = ScrollBars.Vertical };

            btnAddRow.Click += (s, e) => 
            { 
                int idx = grid.Rows.Add(); 
                var row = grid.Rows[idx];
                var cat = Convert.ToString(cbFilterCategory?.SelectedItem ?? "");
                if (!string.IsNullOrWhiteSpace(cat) && cat != "Tümü")
                    row.Cells["colCategory"].Value = cat;
                var sub = Convert.ToString(cbFilterSubCategory?.SelectedItem ?? "");
                if (!string.IsNullOrWhiteSpace(sub) && sub != "Tümü")
                    row.Cells["colSubCategory"].Value = sub;
                SaveData(); 
            };
            btnDeleteRow.Click += (s, e) =>
            {
                foreach (DataGridViewRow r in grid.SelectedRows)
                    if (!r.IsNewRow) grid.Rows.Remove(r);
                SaveData();
            };
            btnExport.Click += (s, e) => ExportCsv();
            btnAddImage.Click += (s, e) => AddImageToSelectedRow();
            btnExportPdf.Click += (s, e) => ExportPdfWithHeader();

            // --- Kategori filtresi kontrolleri ---
            var lblFilter = new Label { Text = " Kategori:", AutoSize = true, Padding = new Padding(12, 6, 0, 0) };
            cbFilterCategory = new ComboBox { Name = "cbFilterCategory", Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilterCategory.Items.AddRange(new object[] { "Tümü", "Plise Sineklik", "Plise Perde" });
            cbFilterCategory.SelectedIndex = 0;
            top.Controls.Add(lblFilter);
            top.Controls.Add(cbFilterCategory);

            cbFilterCategory.SelectedIndexChanged += OnCategoryChanged;
            // Alt Kategori filtresi
            var lblFilterSub = new Label { Text = " Alt Kategori:", AutoSize = true, Padding = new Padding(12, 6, 0, 0) };
            cbFilterSubCategory = new ComboBox { Name = "cbFilterSubCategory", Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilterSubCategory.Items.AddRange(new object[] { "Tümü" });
            cbFilterSubCategory.SelectedIndex = 0;
            top.Controls.Add(lblFilterSub);
            top.Controls.Add(cbFilterSubCategory);
            cbFilterSubCategory.SelectedIndexChanged += OnSubCategoryChanged;
    

            btnChooseLogo.Click += (s, e) => ChooseLogo();

            top.Controls.AddRange(new Control[] { btnAddRow, btnDeleteRow, btnExport, btnAddImage, btnExportPdf, btnChooseLogo, lblCurr, cbCurrency, lblHeader, txtHeader });

            Controls.Add(grid);
            Controls.Add(top);

            cbCurrency.SelectedIndexChanged += (s, e) =>
            {
                SaveSettingsCurrency(Convert.ToString(cbCurrency.SelectedItem));
                FormatAllPrices(Convert.ToString(cbCurrency.SelectedItem));
            };

            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            grid.CellValueChanged += (s, e) =>
        {
            // Kategori değişince alt kategori hücresini güncelle
            if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colCategory")
            {
                var row = grid.Rows[e.RowIndex];
                string cat = Convert.ToString(row.Cells["colCategory"].Value) ?? "Tümü";
                var cell = row.Cells["colSubCategory"] as DataGridViewComboBoxCell;
                if (cell != null && cell.Value == null) cell.Value = "Tümü";
                if (cell != null)
                {
                    cell.Items.Clear();
                    cell.Items.Add("Tümü");
                    if (!string.IsNullOrWhiteSpace(cat) && cat != "Tümü" && _subcats.ContainsKey(cat))
                        foreach (var ssub in _subcats[cat]) cell.Items.Add(ssub);

                    var cur = Convert.ToString(cell.Value);
                    if (string.IsNullOrEmpty(cur) || !cell.Items.Contains(cur)) cell.Value = "Tümü";
                }
            }

            // Checkbox değişince seçimi sakla (sticky)
            if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colSelect")
            {
                var row = grid.Rows[e.RowIndex];
                var id = GetRowId(row);
                bool sel = false;
                var val = row.Cells["colSelect"].Value;
                if (val is bool b) sel = b; else if (val != null && bool.TryParse(val.ToString(), out var bb)) sel = bb;
                _rowSelection[id] = sel;
            }
if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colCategory")
                {
                    grid.Rows[e.RowIndex].Cells["colSubCategory"].Value = null;
                    SaveData();
                }
            };

            
            grid.EditingControlShowing += (s, e) =>
            {
                if (e.Control is ComboBox cb)
                {
                    cb.DropDownStyle = ComboBoxStyle.DropDown;
                }
            };

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colColor")
                {
                    using (var cd = new ColorDialog())
                    {
                        if (cd.ShowDialog(this) == DialogResult.OK)
                        {
                            var hex = $"#{cd.Color.R:X2}{cd.Color.G:X2}{cd.Color.B:X2}";
                            var cell = grid.Rows[e.RowIndex].Cells["colColor"];
                            cell.Value = hex;
                            cell.Style.BackColor = cd.Color;
                            cell.Style.SelectionBackColor = cd.Color;
                            SaveData();
                        }
                    }
                }
            };

            grid.CellEndEdit += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colPrice")
                {
                    ApplyPriceFormat(e.RowIndex, Convert.ToString(cbCurrency.SelectedItem));
                    SaveData();
                }
                else
                {
                    SaveData();
                }
            };

            grid.DataError += (s, e) => { e.ThrowException = false; };

            AllowDrop = true;
            DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            DragDrop += (s, e) =>
            {
                try
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
                    {
                        var pt = grid.PointToClient(new System.Drawing.Point(((DragEventArgs)e).X, ((DragEventArgs)e).Y));
                        var hit = grid.HitTest(pt.X, pt.Y);
                        int row = hit.RowIndex >= 0 ? hit.RowIndex : (grid.CurrentRow?.Index ?? -1);
                        if (row >= 0)
                        {
                            var path = files[0];
                            using (var img = Image.FromFile(path))
                            {
                                grid.Rows[row].Cells["colImage"].Value = new Bitmap(img);
                                grid.Rows[row].Cells["colImagePath"].Value = path;
                                SaveData();
                            }
                        }
                    }
                }
                catch { }
            };

            grid.RowsAdded += (s, e) => SaveData();
            grid.UserDeletingRow += (s, e) => SaveData();
            grid.UserDeletedRow += (s, e) => SaveData();

            LoadData();
            try
            {
                var curr = LoadSettingsCurrency();
                cbCurrency.SelectedItem = curr;
            }
            catch { }
        }

        
        private void OnCategoryChanged(object? sender, EventArgs e)
        {
            _activeFilterCategory = Convert.ToString(cbFilterCategory?.SelectedItem) ?? "Tümü";
            PopulateSubCategories(_activeFilterCategory);
            ApplyCategoryFilter();
        }

        private void OnSubCategoryChanged(object? sender, EventArgs e)
        {
            _activeFilterSubCategory = Convert.ToString(cbFilterSubCategory?.SelectedItem) ?? "Tümü";
            ApplyCategoryFilter();
        }

        private void PopulateSubCategories(string category)
        {
            if (cbFilterSubCategory == null) return;
            cbFilterSubCategory.SelectedIndexChanged -= OnSubCategoryChanged;
            cbFilterSubCategory.BeginUpdate();
            try
            {
                cbFilterSubCategory.Items.Clear();
                cbFilterSubCategory.Items.Add("Tümü");
                if (!string.IsNullOrWhiteSpace(category) && category != "Tümü" && _subcats.ContainsKey(category))
                {
                    foreach (var s in _subcats[category]) cbFilterSubCategory.Items.Add(s);
                }
                if (cbFilterSubCategory.Items.Count > 0) cbFilterSubCategory.SelectedIndex = 0;
                _activeFilterSubCategory = "Tümü";
            }
            finally
            {
                cbFilterSubCategory.EndUpdate();
                cbFilterSubCategory.SelectedIndexChanged += OnSubCategoryChanged;
            }
        }
        private string GetRowId(DataGridViewRow r)
        {
            if (r == null) return string.Empty;
            if (r.Tag is string s && !string.IsNullOrWhiteSpace(s)) return s;
            string id = Guid.NewGuid().ToString("N");
            r.Tag = id;
            return id;
        }


        private string BuildRowKey(DataGridViewRow r)
        {
            string path  = Convert.ToString(r.Cells["colImagePath"].Value) ?? "";
            string name  = Convert.ToString(r.Cells["colName"].Value) ?? "";
            string unit  = Convert.ToString(r.Cells["colUnit"].Value) ?? "";
            string cat   = Convert.ToString(r.Cells["colCategory"].Value) ?? "";
            string scat  = Convert.ToString(r.Cells["colSubCategory"].Value) ?? "";
            string color = Convert.ToString(r.Cells["colColor"].Value) ?? "";
            string price = Convert.ToString(r.Cells["colPrice"].Value) ?? "";
            return string.Join("|", new[]{path,name,unit,cat,scat,color,price}).ToLowerInvariant();
        }

        private static string ToTitleTr(string s)
        {
            try
            {
                var ci = new System.Globalization.CultureInfo("tr-TR");
                return ci.TextInfo.ToTitleCase((s ?? string.Empty).ToLower(ci));
            }
            catch { return s; }
        }
private void EnsureDataFolder()
        {
            if (!Directory.Exists(DataDirPath)) Directory.CreateDirectory(DataDirPath);
        }

        
        private void SaveData()
        {
            if (_isRendering) return;
            try
            {
                EnsureDataFolder();
                var currentShown = BuildRowsFromGrid();
                if (string.IsNullOrEmpty(_activeFilterCategory) || _activeFilterCategory == "Tümü")
                {
                    _allRowsRaw = currentShown;
                }
                else
                {
                    _allRowsRaw.RemoveAll(p => p.Length>3 && string.Equals(p[3], _activeFilterCategory, StringComparison.OrdinalIgnoreCase));
                    _allRowsRaw.AddRange(currentShown);
                }
                using (var w = new StreamWriter(DataFilePath, false, System.Text.Encoding.UTF8))
                {
                    w.WriteLine("ImagePath;Name;Unit;Category;SubCategory;Color;Price");
                    foreach (var parts in _allRowsRaw)
                    {
                        w.WriteLine(string.Join(";", new [] {
                            (parts.Length>0?parts[0]:""),
                            (parts.Length>1?parts[1]:"").Replace("\r"," ").Replace("\n"," "),
                            (parts.Length>2?parts[2]:""),
                            (parts.Length>3?parts[3]:""),
                            (parts.Length>4?parts[4]:""),
                            (parts.Length>5?parts[5]:""),
                            (parts.Length>6?parts[6]:"")
                        }));
                    }
                }
            }
            catch { }
        }


        
        private void LoadData()
        {
            try
            {
                _allRowsRaw.Clear();
                if (File.Exists(DataFilePath))
                {
                    using (var r = new StreamReader(DataFilePath, System.Text.Encoding.UTF8))
                    {
                        string line; bool first = true;
                        while ((line = r.ReadLine()) != null)
                        {
                            if (first) { first = false; continue; }
                            var parts = line.Split(';');
                            var arr = new string[7];
                            for (int i=0; i<7; i++) arr[i] = i < parts.Length ? parts[i] : "";
                            _allRowsRaw.Add(arr);
                        }
                    }
                }
                RenderRowsFromAll();
            }
            catch { }
        }


        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "fiyat_listesi.csv" })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                using (var w = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                {
                    w.WriteLine("ImagePath;Ürün Adı;Birim;Kategori;Alt Kategori;Renk;Fiyat");
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        string path  = Convert.ToString(row.Cells["colImagePath"].Value) ?? "";
                        string name  = Convert.ToString(row.Cells["colName"].Value) ?? "";
                        string unit  = Convert.ToString(row.Cells["colUnit"].Value) ?? "";
                        string cat   = Convert.ToString(row.Cells["colCategory"].Value) ?? "";
                        string scat  = Convert.ToString(row.Cells["colSubCategory"].Value) ?? "";
                        string color = Convert.ToString(row.Cells["colColor"].Value) ?? "";
                        string price = Convert.ToString(row.Cells["colPrice"].Value) ?? "";
                        w.WriteLine(string.Join(";", new[] { path, name, unit, cat, scat, color, price }).Replace("\\r"," ").Replace("\\n"," "));
                    }
                }
                MessageBox.Show("CSV kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddImageToSelectedRow()
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Önce bir satır seçin."); return; }
            using (var ofd = new OpenFileDialog { Filter = "Resimler|*.png;*.jpg;*.jpeg;*.bmp;*.gif" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        grid.CurrentRow.Cells["colImage"].Value = new Bitmap(img);
                        grid.CurrentRow.Cells["colImagePath"].Value = ofd.FileName;
                    }
                    SaveData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Görsel yüklenemedi: " + ex.Message);
                }
            }
        }

        private void ChooseLogo()
        {
            using (var ofd = new OpenFileDialog { Filter = "Resimler|*.png;*.jpg;*.jpeg;*.bmp;*.gif" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                _logoPath = ofd.FileName;
                MessageBox.Show("Logo seçildi.");
            }
        }

        private void ExportPdfWithHeader()
        {
            using (var pd = new System.Drawing.Printing.PrintDocument())
            using (var dlg = new PrintPreviewDialog())
            {
                pd.DocumentName = "Fiyat Listesi";
                pd.PrintPage += (s, e) =>
                {
                    float x = e.MarginBounds.Left;
                    float y = e.MarginBounds.Top;
                    var g = e.Graphics;

                    // Logo + Başlık
                    if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
                    {
                        try
                        {
                            using (var logo = Image.FromFile(_logoPath))
                            {
                                float logoH = 60f;
                                float lw = logoH * (logo.Width / (float)logo.Height);
                                g.DrawImage(logo, x, y, lw, logoH);
                                x += lw + 12;
                            }
                        }
                        catch { }
                    }

                    string header = HeaderBox?.Text ?? "";
                    using (var fTitle = new Font("Segoe UI", 12, FontStyle.Bold))
                    {
                        g.DrawString(header, fTitle, Brushes.Black, x, y + 20);
                    }
                    y += 80;

                    float rowH = 30;
                    float imgW = 100;
                    float nameW = 240;
                    float unitW = 60;
                    float colorW= 90;
                    float priceW= 110;
                    float tableW = imgW + nameW + unitW + colorW + priceW;
                    float tableX = e.MarginBounds.Left;

                    using (var fHeader = new Font("Segoe UI", 9, FontStyle.Bold))
                    using (var fRow = new Font("Segoe UI", 9))
                    using (var pen = new Pen(Color.Gray))
                    {
                        // Seçili satırlar
                        var selected = new List<DataGridViewRow>();
                        foreach (DataGridViewRow r in grid.Rows)
                        {
                            if (r.IsNewRow) continue;
                            bool sel = false;
                            var id = GetRowId(r);
                            if (_rowSelection.TryGetValue(id, out var keep)) sel = keep;
                            else { try { sel = Convert.ToBoolean(r.Cells["colSelect"].Value ?? false); } catch {} }
                            if (sel) selected.Add(r);
                        }
                        if (selected.Count == 0) return;

                        // Alt kategoriye göre grupla (alfabetik)
                        var comparer = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true);
                        var keys = new SortedSet<string>(comparer);
                        var groups = new Dictionary<string, List<DataGridViewRow>>(comparer);
                        foreach (var r in selected)
                        {
                            string scat = Convert.ToString(r.Cells["colSubCategory"].Value) ?? "";
                            if (string.IsNullOrWhiteSpace(scat)) scat = "Diğer";
                            if (!groups.ContainsKey(scat)) groups[scat] = new List<DataGridViewRow>();
                            groups[scat].Add(r);
                            keys.Add(scat);
                        }

                        foreach (var scat in keys)
                        {
                            // Grup başlığı (üstte ve ortalı)
                            using (var fBand = new Font("Segoe UI", 10, FontStyle.Bold))
                            using (var bandBrush = new SolidBrush(Color.FromArgb(111, 58, 0)))
                            using (var white = new SolidBrush(Color.White))
                            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            {
                                string bandText = ToTitleTr(scat) + " Grubu";
                                if (y + rowH*2 > e.MarginBounds.Bottom) { e.HasMorePages = true; return; }
                                g.FillRectangle(bandBrush, tableX, y, tableW, rowH);
                                g.DrawRectangle(Pens.Black, tableX, y, tableW, rowH);
                                g.DrawString(bandText, fBand, white, new RectangleF(tableX, y, tableW, rowH), fmt);
                                y += rowH;
                            }

                            // Başlık satırı
                            float xh = tableX;
                            g.DrawRectangle(pen, xh, y, tableW, rowH);
                            g.DrawString("Görsel",   fHeader, Brushes.Black, xh + 4, y + 6); xh += imgW;  g.DrawLine(pen, xh, y, xh, y + rowH);
                            g.DrawString("Ürün Adı", fHeader, Brushes.Black, xh + 4, y + 6); xh += nameW; g.DrawLine(pen, xh, y, xh, y + rowH);
                            g.DrawString("Birim",    fHeader, Brushes.Black, xh + 4, y + 6); xh += unitW; g.DrawLine(pen, xh, y, xh, y + rowH);
                            g.DrawString("Renk",     fHeader, Brushes.Black, xh + 4, y + 6); xh += colorW;g.DrawLine(pen, xh, y, xh, y + rowH);
                            g.DrawString("Fiyat",    fHeader, Brushes.Black, xh + 4, y + 6);
                            y += rowH;

                            foreach (var r in groups[scat])
                            {
                                if (y + rowH > e.MarginBounds.Bottom) { e.HasMorePages = true; return; }

                                float xr = tableX;
                                g.DrawRectangle(pen, xr, y, tableW, rowH);

                                var cellImg = r.Cells["colImage"].Value as Image;
                                if (cellImg != null)
                                {
                                    float h = rowH - 6;
                                    float w = h * (cellImg.Width / (float)cellImg.Height);
                                    g.DrawImage(cellImg, xr + 3, y + 3, w, h);
                                }
                                xr += imgW; g.DrawLine(pen, xr, y, xr, y + rowH);

                                string name = Convert.ToString(r.Cells["colName"].Value) ?? "";
                                g.DrawString(name, fRow, Brushes.Black, xr + 4, y + 6);
                                xr += nameW; g.DrawLine(pen, xr, y, xr, y + rowH);

                                string unit = Convert.ToString(r.Cells["colUnit"].Value) ?? "";
                                g.DrawString(unit, fRow, Brushes.Black, xr + 4, y + 6);
                                xr += unitW; g.DrawLine(pen, xr, y, xr, y + rowH);

                                string color = Convert.ToString(r.Cells["colColor"].Value) ?? "";
                                g.DrawString(color, fRow, Brushes.Black, xr + 4, y + 6);
                                xr += colorW; g.DrawLine(pen, xr, y, xr, y + rowH);

                                string price = Convert.ToString(r.Cells["colPrice"].Value) ?? "";
                                g.DrawString(price, fRow, Brushes.Black, xr + 4, y + 6);

                                y += rowH;
                            }
                        }
                    }
                };
                dlg.Document = pd;
                dlg.ShowDialog(this);
            }
        }


        private void SaveSettingsCurrency(string code)
        {
            try
            {
                EnsureDataFolder();
                File.WriteAllText(SettingsFilePath, "{\"currency\":\"" + (code ?? "TRY") + "\"}");
            }
            catch { }
        }

        private string LoadSettingsCurrency()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var txt = File.ReadAllText(SettingsFilePath);
                    var m = System.Text.RegularExpressions.Regex.Match(txt, "\"currency\"\\s*:\\s*\"(TRY|USD|EUR)\"");
                    if (m.Success) return m.Groups[1].Value;
                }
            }
            catch { }
            return "TRY";
        }

        private void FormatAllPrices(string curr)
        {
            for (int r = 0; r < grid.Rows.Count; r++) ApplyPriceFormat(r, curr);
        }

        private decimal ParseDecimal(object v)
        {
            if (v == null) return 0m;
            var s = Convert.ToString(v);
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            s = s.Replace("₺", "").Replace("$", "").Replace("€", "").Replace(".", "").Replace(" ", "");
            s = s.Replace(",", ".");
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return 0m;
        }

        private void ApplyPriceFormat(int row, string curr)
        {
            if (row < 0 || row >= grid.Rows.Count) return;
            var cell = grid.Rows[row].Cells["colPrice"];
            decimal val = ParseDecimal(cell.Value);
            string symbol = curr == "USD" ? "$" : curr == "EUR" ? "€" : "₺";
            string formatted = string.Format(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), "{0:N2} {1}", val, symbol);
            cell.Value = formatted;
        }
    
        // --- Kategori filtreleme yardımcıları ---
        private void ApplyCategoryFilter()
        {
            if (_isApplyingFilter) return;
            _isApplyingFilter = true;
            try
            {
                _activeFilterCategory = Convert.ToString(cbFilterCategory?.SelectedItem) ?? "Tümü";
                _activeFilterSubCategory = Convert.ToString(cbFilterSubCategory?.SelectedItem) ?? "Tümü";
                RenderRowsFromAll();
                EnsureSeedIfEmptyForCategory();
            }
            finally { _isApplyingFilter = false; }
        }


        private void RenderRowsFromAll()
        {
            try
            {
                _isRendering = true;
                grid.Rows.Clear();
                foreach (var parts in _allRowsRaw)
                {
                    if (!string.IsNullOrEmpty(_activeFilterCategory) && _activeFilterCategory != "Tümü")
                    {
                        if (!string.Equals(parts[3], _activeFilterCategory, StringComparison.OrdinalIgnoreCase)) continue;
                    }
                    // alt kategori filtresi
                    var _activeSub = Convert.ToString(cbFilterSubCategory?.SelectedItem ?? "Tümü");
                    if (!string.IsNullOrEmpty(_activeSub) && _activeSub != "Tümü")
                    {
                        if (!string.Equals(parts[4], _activeSub, StringComparison.OrdinalIgnoreCase)) continue;
                    }
                    int rowIndex = grid.Rows.Add();
                    var row = grid.Rows[rowIndex];
                    try
                    {
                        string path = parts[0] ?? "";
                        row.Cells["colImagePath"].Value = path;
                        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                        {
                            using (var img = Image.FromFile(path))
                                row.Cells["colImage"].Value = new Bitmap(img);
                        }
                    } catch { }
                    row.Cells["colName"].Value        = parts.Length>1 ? parts[1] : "";
                    row.Cells["colUnit"].Value        = parts.Length>2 ? parts[2] : "";
                    row.Cells["colCategory"].Value    = parts.Length>3 ? parts[3] : "";
                    row.Cells["colSubCategory"].Value = parts.Length>4 ? parts[4] : "";
                    row.Cells["colColor"].Value       = parts.Length>5 ? parts[5] : "";
                    row.Cells["colPrice"].Value       = parts.Length>6 ? parts[6] : "";
                    try
                    {
                        var col = Convert.ToString(row.Cells["colColor"].Value);
                        if (!string.IsNullOrWhiteSpace(col) && col.StartsWith("#") && col.Length >= 7)
                        {
                            int rC = Convert.ToInt32(col.Substring(1, 2), 16);
                            int gC = Convert.ToInt32(col.Substring(3, 2), 16);
                            int bC = Convert.ToInt32(col.Substring(5, 2), 16);
                            var color = Color.FromArgb(rC, gC, bC);
                            row.Cells["colColor"].Style.BackColor = color;
                            row.Cells["colColor"].Style.SelectionBackColor = color;
                        }
                    } catch { }
                }
            }
            finally { _isRendering = false; }
        }

        private List<string[]> BuildRowsFromGrid()
        {
            var list = new List<string[]>();
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (r.IsNewRow) continue;
                string path  = Convert.ToString(r.Cells["colImagePath"].Value) ?? "";
                string name  = Convert.ToString(r.Cells["colName"].Value) ?? "";
                string unit  = Convert.ToString(r.Cells["colUnit"].Value) ?? "";
                string cat   = Convert.ToString(r.Cells["colCategory"].Value) ?? "";
                string scat  = Convert.ToString(r.Cells["colSubCategory"].Value) ?? "";
                string color = Convert.ToString(r.Cells["colColor"].Value) ?? "";
                string price = Convert.ToString(r.Cells["colPrice"].Value) ?? "";
                list.Add(new []{ path, name, unit, cat, scat, color, price });
            }
            return list;
        }

        private void EnsureSeedIfEmptyForCategory()
        {
            if (string.IsNullOrEmpty(_activeFilterCategory) || _activeFilterCategory == "Tümü") return;
            if (grid.Rows.Count > 0) return;
            if (!_subcats.ContainsKey(_activeFilterCategory)) return;
            foreach (var sc in _subcats[_activeFilterCategory])
            {
                int i = grid.Rows.Add();
                var row = grid.Rows[i];
                row.Cells["colCategory"].Value = _activeFilterCategory;
                row.Cells["colSubCategory"].Value = sc;
            }
        }
}
}
