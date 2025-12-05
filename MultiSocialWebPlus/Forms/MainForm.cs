using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MultiSocialWebPlus.Forms
{
    public class MainForm : Form
    {
        private Panel leftPanel;
        private Button btnCustomers, btnProducts, btnQuotes, btnCategories, btnSocials;
        private Panel contentPanel;
        
        public MainForm()
        {
            Text = "MultiSocialWebPlus";
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;

            leftPanel = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(250, 250, 250) };
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            btnCustomers = MakeButton("Müşteriler");
            btnProducts  = MakeButton("Ürünler");
            btnQuotes    = MakeButton("Teklifler");
            btnCategories= MakeButton("Kategori");
            btnSocials   = MakeButton("Sosyal Bağlantılar");

            btnCustomers.Click += (s, e) => OpenForm(new CustomersForm());
            btnProducts.Click += (s, e) => OpenForm(new ProductsForm());
            btnQuotes.Click += (s, e) => OpenForm(new QuotesForm());
            btnCategories.Click += (s, e) => OpenForm(new CategoriesForm());
            btnSocials.Click += (s, e) => OpenForm(new SocialsForm());

            var v = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            v.Controls.AddRange(new Control[] { btnCustomers, btnProducts, btnQuotes, btnCategories, btnSocials });
            leftPanel.Controls.Add(v);

            Controls.Add(contentPanel);
            Controls.Add(leftPanel);
        }

        private Button MakeButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = false,
                Width = 200,
                Height = 60,
                Margin = new Padding(10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void OpenForm(Form f)
        {
            foreach (Control c in contentPanel.Controls) c.Dispose();
            contentPanel.Controls.Clear();
            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(f);
            f.Show();
        }
    }
}
