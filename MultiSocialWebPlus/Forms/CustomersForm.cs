using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using MultiSocialWebPlus.Data;
using MultiSocialWebPlus.Models;

namespace MultiSocialWebPlus.Forms
{
    public class CustomersForm : Form
    {
        private DataGridView dgv;
        private TextBox txtName, txtCompany, txtPhone, txtEmail, txtAddress, txtNotes;
        private Button btnAdd, btnSave, btnDelete;
        private int? editingId = null;

        public CustomersForm()
        {
            Text = "Müşteriler";
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };
            dgv.SelectionChanged += Dgv_SelectionChanged;
            panel.Controls.Add(dgv, 0, 0);

            var editPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true };
            
            editPanel.Controls.Add(new Label { Text = "Ad Soyad:", AutoSize = true });
            txtName = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtName);

            editPanel.Controls.Add(new Label { Text = "Şirket:", AutoSize = true });
            txtCompany = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtCompany);

            editPanel.Controls.Add(new Label { Text = "Telefon:", AutoSize = true });
            txtPhone = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtPhone);

            editPanel.Controls.Add(new Label { Text = "E-posta:", AutoSize = true });
            txtEmail = new TextBox { Width = 250 };
            editPanel.Controls.Add(txtEmail);

            editPanel.Controls.Add(new Label { Text = "Adres:", AutoSize = true });
            txtAddress = new TextBox { Width = 250, Multiline = true, Height = 60 };
            editPanel.Controls.Add(txtAddress);

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
            LoadData();
        }

        private void LoadData()
        {
            using var db = new AppDbContext();
            dgv.DataSource = db.Customers.ToList();
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow?.DataBoundItem is Customer c)
            {
                editingId = c.Id;
                txtName.Text = c.Name;
                txtCompany.Text = c.Company;
                txtPhone.Text = c.Phone;
                txtEmail.Text = c.Email;
                txtAddress.Text = c.Address;
                txtNotes.Text = c.Notes;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            editingId = null;
            txtName.Text = txtCompany.Text = txtPhone.Text = txtEmail.Text = txtAddress.Text = txtNotes.Text = "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var db = new AppDbContext();
            Customer c;
            if (editingId.HasValue)
            {
                c = db.Customers.Find(editingId.Value)!;
            }
            else
            {
                c = new Customer();
                db.Customers.Add(c);
            }
            c.Name = txtName.Text;
            c.Company = txtCompany.Text;
            c.Phone = txtPhone.Text;
            c.Email = txtEmail.Text;
            c.Address = txtAddress.Text;
            c.Notes = txtNotes.Text;
            db.SaveChanges();
            LoadData();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!editingId.HasValue) return;
            using var db = new AppDbContext();
            var c = db.Customers.Find(editingId.Value);
            if (c != null)
            {
                db.Customers.Remove(c);
                db.SaveChanges();
            }
            editingId = null;
            txtName.Text = txtCompany.Text = txtPhone.Text = txtEmail.Text = txtAddress.Text = txtNotes.Text = "";
            LoadData();
        }
    }
}
