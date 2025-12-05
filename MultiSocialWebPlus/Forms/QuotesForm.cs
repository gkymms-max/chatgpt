using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using MultiSocialWebPlus.Data;
using MultiSocialWebPlus.Models;

namespace MultiSocialWebPlus.Forms
{
    public class QuotesForm : Form
    {
        private DataGridView dgvQuotes, dgvItems;
        private ComboBox cmbCustomer;
        private DateTimePicker dtpValidUntil;
        private TextBox txtTotal;
        private Button btnNewQuote, btnSaveQuote, btnDeleteQuote;
        private Button btnAddItem, btnRemoveItem;
        private int? editingQuoteId = null;

        public QuotesForm()
        {
            Text = "Teklifler";
            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // Quotes grid
            dgvQuotes = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            dgvQuotes.SelectionChanged += DgvQuotes_SelectionChanged;
            mainPanel.Controls.Add(dgvQuotes, 0, 0);

            // Quote items grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            mainPanel.Controls.Add(dgvItems, 0, 1);

            // Edit panel for quote
            var editPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };
            
            editPanel.Controls.Add(new Label { Text = "Müşteri:", AutoSize = true });
            cmbCustomer = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            editPanel.Controls.Add(cmbCustomer);

            editPanel.Controls.Add(new Label { Text = "Geçerlilik Tarihi:", AutoSize = true });
            dtpValidUntil = new DateTimePicker { Width = 250, Format = DateTimePickerFormat.Short };
            dtpValidUntil.Value = DateTime.Now.AddDays(30);
            editPanel.Controls.Add(dtpValidUntil);

            editPanel.Controls.Add(new Label { Text = "Toplam:", AutoSize = true });
            txtTotal = new TextBox { Width = 250, ReadOnly = true };
            editPanel.Controls.Add(txtTotal);

            var btnPanel = new FlowLayoutPanel { Width = 260, Height = 40 };
            btnNewQuote = new Button { Text = "Yeni", Width = 75 };
            btnNewQuote.Click += BtnNewQuote_Click;
            btnSaveQuote = new Button { Text = "Kaydet", Width = 75 };
            btnSaveQuote.Click += BtnSaveQuote_Click;
            btnDeleteQuote = new Button { Text = "Sil", Width = 75 };
            btnDeleteQuote.Click += BtnDeleteQuote_Click;
            btnPanel.Controls.AddRange(new Control[] { btnNewQuote, btnSaveQuote, btnDeleteQuote });
            editPanel.Controls.Add(btnPanel);

            mainPanel.Controls.Add(editPanel, 1, 0);

            // Items panel
            var itemsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };
            itemsPanel.Controls.Add(new Label { Text = "Kalem Yönetimi:", AutoSize = true });
            var itemBtnPanel = new FlowLayoutPanel { Width = 260, Height = 40 };
            btnAddItem = new Button { Text = "Kalem Ekle", Width = 100 };
            btnAddItem.Click += BtnAddItem_Click;
            btnRemoveItem = new Button { Text = "Kalem Sil", Width = 100 };
            btnRemoveItem.Click += BtnRemoveItem_Click;
            itemBtnPanel.Controls.AddRange(new Control[] { btnAddItem, btnRemoveItem });
            itemsPanel.Controls.Add(itemBtnPanel);
            mainPanel.Controls.Add(itemsPanel, 1, 1);

            Controls.Add(mainPanel);
            LoadCustomers();
            LoadQuotes();
        }

        private void LoadCustomers()
        {
            using var db = new AppDbContext();
            cmbCustomer.Items.Clear();
            foreach (var c in db.Customers.ToList())
            {
                cmbCustomer.Items.Add(c);
            }
            cmbCustomer.DisplayMember = "Name";
            if (cmbCustomer.Items.Count > 0) cmbCustomer.SelectedIndex = 0;
        }

        private void LoadQuotes()
        {
            using var db = new AppDbContext();
            dgvQuotes.DataSource = db.Quotes.Include(q => q.Customer).ToList();
        }

        private void LoadQuoteItems(int quoteId)
        {
            using var db = new AppDbContext();
            dgvItems.DataSource = db.QuoteItems.Include(qi => qi.Product).Where(qi => qi.QuoteId == quoteId).ToList();
        }

        private void DgvQuotes_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvQuotes.CurrentRow?.DataBoundItem is Quote q)
            {
                editingQuoteId = q.Id;
                txtTotal.Text = q.Total.ToString("N2");
                if (q.ValidUntil.HasValue) dtpValidUntil.Value = q.ValidUntil.Value;
                
                for (int i = 0; i < cmbCustomer.Items.Count; i++)
                {
                    if (cmbCustomer.Items[i] is Customer c && c.Id == q.CustomerId)
                    {
                        cmbCustomer.SelectedIndex = i;
                        break;
                    }
                }
                LoadQuoteItems(q.Id);
            }
        }

        private void BtnNewQuote_Click(object? sender, EventArgs e)
        {
            editingQuoteId = null;
            txtTotal.Text = "0.00";
            dtpValidUntil.Value = DateTime.Now.AddDays(30);
            dgvItems.DataSource = null;
        }

        private void BtnSaveQuote_Click(object? sender, EventArgs e)
        {
            if (cmbCustomer.SelectedItem is not Customer selectedCustomer) return;
            
            using var db = new AppDbContext();
            Quote q;
            if (editingQuoteId.HasValue)
            {
                q = db.Quotes.Find(editingQuoteId.Value)!;
            }
            else
            {
                q = new Quote();
                db.Quotes.Add(q);
            }
            q.CustomerId = selectedCustomer.Id;
            q.ValidUntil = dtpValidUntil.Value;
            db.SaveChanges();
            editingQuoteId = q.Id;
            LoadQuotes();
        }

        private void BtnDeleteQuote_Click(object? sender, EventArgs e)
        {
            if (!editingQuoteId.HasValue) return;
            using var db = new AppDbContext();
            var q = db.Quotes.Find(editingQuoteId.Value);
            if (q != null)
            {
                var items = db.QuoteItems.Where(qi => qi.QuoteId == q.Id).ToList();
                db.QuoteItems.RemoveRange(items);
                db.Quotes.Remove(q);
                db.SaveChanges();
            }
            editingQuoteId = null;
            dgvItems.DataSource = null;
            LoadQuotes();
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            if (!editingQuoteId.HasValue)
            {
                MessageBox.Show("Önce teklifi kaydedin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new AddItemDialog();
            if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedProduct != null)
            {
                using var db = new AppDbContext();
                var item = new QuoteItem
                {
                    QuoteId = editingQuoteId.Value,
                    ProductId = dlg.SelectedProduct.Id,
                    Quantity = dlg.Quantity,
                    UnitPrice = dlg.SelectedProduct.UnitPrice,
                    LineTotal = dlg.Quantity * dlg.SelectedProduct.UnitPrice
                };
                db.QuoteItems.Add(item);
                db.SaveChanges();

                // Update total
                var quote = db.Quotes.Find(editingQuoteId.Value)!;
                quote.Total = db.QuoteItems.Where(qi => qi.QuoteId == editingQuoteId.Value).Sum(qi => qi.LineTotal);
                db.SaveChanges();

                LoadQuoteItems(editingQuoteId.Value);
                LoadQuotes();
            }
        }

        private void BtnRemoveItem_Click(object? sender, EventArgs e)
        {
            if (!editingQuoteId.HasValue) return;
            if (dgvItems.CurrentRow?.DataBoundItem is QuoteItem qi)
            {
                using var db = new AppDbContext();
                var item = db.QuoteItems.Find(qi.Id);
                if (item != null)
                {
                    db.QuoteItems.Remove(item);
                    db.SaveChanges();

                    // Update total
                    var quote = db.Quotes.Find(editingQuoteId.Value)!;
                    quote.Total = db.QuoteItems.Where(q => q.QuoteId == editingQuoteId.Value).Sum(q => q.LineTotal);
                    db.SaveChanges();
                }
                LoadQuoteItems(editingQuoteId.Value);
                LoadQuotes();
            }
        }
    }

    public class AddItemDialog : Form
    {
        public Product? SelectedProduct { get; private set; }
        public decimal Quantity { get; private set; }
        
        private ComboBox cmbProduct;
        private NumericUpDown numQuantity;

        public AddItemDialog()
        {
            Text = "Kalem Ekle";
            Size = new Size(350, 180);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            
            panel.Controls.Add(new Label { Text = "Ürün:", AutoSize = true });
            cmbProduct = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            using (var db = new AppDbContext())
            {
                foreach (var p in db.Products.ToList())
                {
                    cmbProduct.Items.Add(p);
                }
            }
            cmbProduct.DisplayMember = "Name";
            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
            panel.Controls.Add(cmbProduct);

            panel.Controls.Add(new Label { Text = "Miktar:", AutoSize = true });
            numQuantity = new NumericUpDown { Width = 100, Minimum = 1, Maximum = 10000, DecimalPlaces = 2, Value = 1 };
            panel.Controls.Add(numQuantity);

            var btnPanel = new FlowLayoutPanel { Width = 300, Height = 40 };
            var btnOK = new Button { Text = "Ekle", Width = 75, DialogResult = DialogResult.OK };
            btnOK.Click += (s, e) => {
                SelectedProduct = cmbProduct.SelectedItem as Product;
                Quantity = numQuantity.Value;
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
