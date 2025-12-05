using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using MultiSocialWebPlus.Data;
using MultiSocialWebPlus.Models;

namespace MultiSocialWebPlus.Forms
{
    public class ProductsForm : Form
    {
        private DataGridView dgv;
        private TextBox txtName, txtUnitPrice, txtStock, txtNotes;
        private ComboBox cmbCategory, cmbUnit;
        private Button btnAdd, btnSave, btnDelete;
        private int? editingId = null;

        public ProductsForm()
        {
            Text = "Ürünler";
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            dgv.SelectionChanged += Dgv_SelectionChanged;
            panel.Controls.Add(dgv, 0, 0);

            var editPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };
            
            editPanel.Controls.Add(new Label { Text = "Ürün Adı:", AutoSize = true });
            txtName = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtName);

            editPanel.Controls.Add(new Label { Text = "Kategori:", AutoSize = true });
            cmbCategory = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            editPanel.Controls.Add(cmbCategory);

            editPanel.Controls.Add(new Label { Text = "Birim:", AutoSize = true });
            cmbUnit = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(Enum.GetNames(typeof(UnitType)));
            cmbUnit.SelectedIndex = 0;
            editPanel.Controls.Add(cmbUnit);

            editPanel.Controls.Add(new Label { Text = "Birim Fiyatı:", AutoSize = true });
            txtUnitPrice = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtUnitPrice);

            editPanel.Controls.Add(new Label { Text = "Stok:", AutoSize = true });
            txtStock = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtStock);

            editPanel.Controls.Add(new Label { Text = "Notlar:", AutoSize = true });
            txtNotes = new TextBox { Width = 250, Multiline = true, Height = 60 };
            editPanel.Controls.Add(txtNotes);

            var btnPanel = new FlowLayoutPanel { Width = 260, Height = 40 };
            btnAdd = new Button { Text = "Yeni", Width = 75 };
            btnAdd.Click += BtnAdd_Click;
            btnSave = new Button { Text = "Kaydet", Width = 75 };
            btnSave.Click += BtnSave_Click;
            btnDelete = new Button { Text = "Sil", Width = 75 };
            btnDelete.Click += BtnDelete_Click;
            btnPanel.Controls.AddRange(new Control[] { btnAdd, btnSave, btnDelete });
            editPanel.Controls.Add(btnPanel);

            panel.Controls.Add(editPanel, 1, 0);
            Controls.Add(panel);
            LoadCategories();
            LoadData();
        }

        private void LoadCategories()
        {
            using var db = new AppDbContext();
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("(Yok)");
            foreach (var cat in db.Categories.ToList())
            {
                cmbCategory.Items.Add(cat);
            }
            cmbCategory.DisplayMember = "Name";
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadData()
        {
            using var db = new AppDbContext();
            dgv.DataSource = db.Products.Include(p => p.Category).ToList();
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Product p)
            {
                editingId = p.Id;
                txtName.Text = p.Name;
                txtUnitPrice.Text = p.UnitPrice.ToString();
                txtStock.Text = p.Stock.ToString();
                txtNotes.Text = p.Notes;
                cmbUnit.SelectedItem = p.Unit.ToString();
                
                if (p.CategoryId.HasValue)
                {
                    for (int i = 1; i < cmbCategory.Items.Count; i++)
                    {
                        if (cmbCategory.Items[i] is Category cat && cat.Id == p.CategoryId)
                        {
                            cmbCategory.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    cmbCategory.SelectedIndex = 0;
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            editingId = null;
            txtName.Text = txtUnitPrice.Text = txtStock.Text = txtNotes.Text = "";
            cmbUnit.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var db = new AppDbContext();
            Product p;
            if (editingId.HasValue)
            {
                p = db.Products.Find(editingId.Value)!;
            }
            else
            {
                p = new Product();
                db.Products.Add(p);
            }
            p.Name = txtName.Text;
            p.UnitPrice = decimal.TryParse(txtUnitPrice.Text, out var price) ? price : 0;
            p.Stock = decimal.TryParse(txtStock.Text, out var stock) ? stock : 0;
            p.Notes = txtNotes.Text;
            p.Unit = Enum.TryParse<UnitType>(cmbUnit.SelectedItem?.ToString(), out var unit) ? unit : UnitType.Adet;
            
            if (cmbCategory.SelectedIndex > 0 && cmbCategory.SelectedItem is Category cat)
            {
                p.CategoryId = cat.Id;
            }
            else
            {
                p.CategoryId = null;
            }
            
            db.SaveChanges();
            LoadData();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!editingId.HasValue) return;
            using var db = new AppDbContext();
            var p = db.Products.Find(editingId.Value);
            if (p != null)
            {
                db.Products.Remove(p);
                db.SaveChanges();
            }
            editingId = null;
            txtName.Text = txtUnitPrice.Text = txtStock.Text = txtNotes.Text = "";
            cmbUnit.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            LoadData();
        }
    }
}
