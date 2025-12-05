using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using MultiSocialWebPlus.Data;
using MultiSocialWebPlus.Models;

namespace MultiSocialWebPlus.Forms
{
    public class CategoriesForm : Form
    {
        private DataGridView dgv;
        private TextBox txtName;
        private Button btnAdd, btnSave, btnDelete;
        private int? editingId = null;

        public CategoriesForm()
        {
            Text = "Kategoriler";
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            dgv.SelectionChanged += Dgv_SelectionChanged;
            panel.Controls.Add(dgv, 0, 0);

            var editPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };
            
            editPanel.Controls.Add(new Label { Text = "Kategori Adı:", AutoSize = true });
            txtName = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtName);

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
            LoadData();
        }

        private void LoadData()
        {
            using var db = new AppDbContext();
            dgv.DataSource = db.Categories.ToList();
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Category c)
            {
                editingId = c.Id;
                txtName.Text = c.Name;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            editingId = null;
            txtName.Text = "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var db = new AppDbContext();
            Category c;
            if (editingId.HasValue)
            {
                c = db.Categories.Find(editingId.Value)!;
            }
            else
            {
                c = new Category();
                db.Categories.Add(c);
            }
            c.Name = txtName.Text;
            db.SaveChanges();
            LoadData();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!editingId.HasValue) return;
            using var db = new AppDbContext();
            var c = db.Categories.Find(editingId.Value);
            if (c != null)
            {
                db.Categories.Remove(c);
                db.SaveChanges();
            }
            editingId = null;
            txtName.Text = "";
            LoadData();
        }
    }
}
