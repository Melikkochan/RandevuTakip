using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RandevuTakip.Models;
using RandevuTakip.Services;

namespace RandevuTakip.Forms
{
    public class RandevuForm : Form
    {
        private readonly RandevuService _service;
        private readonly Randevu? _mevcutRandevu;
        private readonly DateTime _seciliTarih;

        // ─── Kontroller ──────────────────────────────────────────────────────
        private DateTimePicker dtpTarih = null!;
        private DateTimePicker dtpBaslangic = null!;
        private DateTimePicker dtpBitis = null!;
        private TextBox txtBaslik = null!;
        private TextBox txtKisi = null!;
        private TextBox txtTelefon = null!;
        private TextBox txtEmail = null!;
        private TextBox txtAciklama = null!;
        private ComboBox cmbDurum = null!;
        private ComboBox cmbKategori = null!;
        private Button btnKaydet = null!;
        private Button btnIptal = null!;
        private Panel pnlBaslik = null!;
        private Label lblFormBaslik = null!;

        public Randevu? SonucRandevu { get; private set; }

        public RandevuForm(RandevuService service, DateTime tarih, Randevu? mevcutRandevu = null)
        {
            _service = service;
            _seciliTarih = tarih;
            _mevcutRandevu = mevcutRandevu;

            InitializeComponent();
            FormuDoldur();
        }

        private void InitializeComponent()
        {
            // Form ayarları
            Text = _mevcutRandevu == null ? "Yeni Randevu" : "Randevu Düzenle";
            Size = new Size(480, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 246, 250);
            Font = new Font("Segoe UI", 9.5f);

            // Başlık paneli
            pnlBaslik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            lblFormBaslik = new Label
            {
                Text = _mevcutRandevu == null ? "  ＋  Yeni Randevu Ekle" : "  ✎  Randevu Düzenle",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            pnlBaslik.Controls.Add(lblFormBaslik);

            // Ana panel (kaydırılabilir)
            var pnlIcerik = new Panel
            {
                Location = new Point(0, 55),
                Size = new Size(480, 440),
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 246, 250)
            };

            int y = 15;
            int etiketGenis = 130;
            int kontrolGenis = 290;
            int kontrolX = 150;
            int satirAraligi = 45;

            // ── Tarih ──
            EtiketEkle(pnlIcerik, "Tarih:", etiketGenis, y);
            dtpTarih = new DateTimePicker
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                Format = DateTimePickerFormat.Long,
                CalendarFont = new Font("Segoe UI", 9)
            };
            pnlIcerik.Controls.Add(dtpTarih);
            y += satirAraligi;

            // ── Başlangıç Saati ──
            EtiketEkle(pnlIcerik, "Başlangıç:", etiketGenis, y);
            dtpBaslangic = new DateTimePicker
            {
                Location = new Point(kontrolX, y),
                Size = new Size(140, 28),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            pnlIcerik.Controls.Add(dtpBaslangic);

            EtiketEkle(pnlIcerik, "Bitiş:", 50, y, kontrolX + 155);
            dtpBitis = new DateTimePicker
            {
                Location = new Point(kontrolX + 210, y),
                Size = new Size(130, 28),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            pnlIcerik.Controls.Add(dtpBitis);
            y += satirAraligi;

            // ── Başlık ──
            EtiketEkle(pnlIcerik, "Başlık: *", etiketGenis, y);
            txtBaslik = new TextBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Randevu başlığı..."
            };
            pnlIcerik.Controls.Add(txtBaslik);
            y += satirAraligi;

            // ── Kategori ──
            EtiketEkle(pnlIcerik, "Kategori:", etiketGenis, y);
            cmbKategori = new ComboBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbKategori.Items.AddRange(new object[]
            {
                "Genel", "İş", "Sağlık", "Kişisel", "Toplantı", "Diğer"
            });
            cmbKategori.SelectedIndex = 0;
            pnlIcerik.Controls.Add(cmbKategori);
            y += satirAraligi;

            // ── Durum ──
            EtiketEkle(pnlIcerik, "Durum:", etiketGenis, y);
            cmbDurum = new ComboBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDurum.Items.AddRange(new object[]
            {
                "Bekliyor", "Onaylandı", "Tamamlandı", "İptal"
            });
            cmbDurum.SelectedIndex = 0;
            pnlIcerik.Controls.Add(cmbDurum);
            y += satirAraligi;

            // ── Kişi Adı ──
            EtiketEkle(pnlIcerik, "Kişi Adı:", etiketGenis, y);
            txtKisi = new TextBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                PlaceholderText = "Ad Soyad..."
            };
            pnlIcerik.Controls.Add(txtKisi);
            y += satirAraligi;

            // ── Telefon ──
            EtiketEkle(pnlIcerik, "Telefon:", etiketGenis, y);
            txtTelefon = new TextBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                PlaceholderText = "0(5xx) xxx xx xx"
            };
            pnlIcerik.Controls.Add(txtTelefon);
            y += satirAraligi;

            // ── E-posta ──
            EtiketEkle(pnlIcerik, "E-posta:", etiketGenis, y);
            txtEmail = new TextBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 28),
                PlaceholderText = "ornek@mail.com"
            };
            pnlIcerik.Controls.Add(txtEmail);
            y += satirAraligi;

            // ── Açıklama ──
            EtiketEkle(pnlIcerik, "Açıklama:", etiketGenis, y);
            txtAciklama = new TextBox
            {
                Location = new Point(kontrolX, y),
                Size = new Size(kontrolGenis, 70),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Notlar, detaylar..."
            };
            pnlIcerik.Controls.Add(txtAciklama);
            y += 80;

            pnlIcerik.Height = y + 20;

            // ── Butonlar ──
            var pnlButon = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            btnKaydet = new Button
            {
                Text = "  Kaydet",
                Size = new Size(130, 38),
                Location = new Point(200, 11),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;

            btnIptal = new Button
            {
                Text = "  İptal",
                Size = new Size(120, 38),
                Location = new Point(340, 11),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIptal.FlatAppearance.BorderSize = 0;
            btnIptal.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlButon.Controls.Add(btnKaydet);
            pnlButon.Controls.Add(btnIptal);

            Controls.Add(pnlBaslik);
            Controls.Add(pnlIcerik);
            Controls.Add(pnlButon);

            AcceptButton = btnKaydet;
            CancelButton = btnIptal;
        }

        private void EtiketEkle(Panel panel, string metin, int genis, int y, int x = 10)
        {
            var lbl = new Label
            {
                Text = metin,
                Location = new Point(x, y + 5),
                Size = new Size(genis, 22),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(lbl);
        }

        private void FormuDoldur()
        {
            if (_mevcutRandevu == null)
            {
                dtpTarih.Value = _seciliTarih;
                dtpBaslangic.Value = DateTime.Today.AddHours(9);
                dtpBitis.Value = DateTime.Today.AddHours(10);
            }
            else
            {
                dtpTarih.Value = _mevcutRandevu.Tarih;

                var bs = DateTime.Today.Add(_mevcutRandevu.BaslangicSaati);
                var bt = DateTime.Today.Add(_mevcutRandevu.BitisSaati);
                dtpBaslangic.Value = bs;
                dtpBitis.Value = bt;

                txtBaslik.Text = _mevcutRandevu.BaslikAd;
                txtKisi.Text = _mevcutRandevu.KisiAdi;
                txtTelefon.Text = _mevcutRandevu.Telefon;
                txtEmail.Text = _mevcutRandevu.Email;
                txtAciklama.Text = _mevcutRandevu.Aciklama;
                cmbDurum.SelectedIndex = (int)_mevcutRandevu.Durum;
                cmbKategori.SelectedIndex = (int)_mevcutRandevu.Kategori;
            }
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBaslik.Text))
            {
                MessageBox.Show("Lütfen randevu başlığını giriniz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBaslik.Focus();
                return;
            }

            var baslangic = dtpBaslangic.Value.TimeOfDay;
            var bitis = dtpBitis.Value.TimeOfDay;

            if (bitis <= baslangic)
            {
                MessageBox.Show("Bitiş saati başlangıç saatinden sonra olmalıdır.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var randevu = new Randevu
            {
                Id = _mevcutRandevu?.Id ?? Guid.NewGuid(),
                Tarih = dtpTarih.Value.Date,
                BaslangicSaati = baslangic,
                BitisSaati = bitis,
                BaslikAd = txtBaslik.Text.Trim(),
                KisiAdi = txtKisi.Text.Trim(),
                Telefon = txtTelefon.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Aciklama = txtAciklama.Text.Trim(),
                Durum = (RandevuDurumu)cmbDurum.SelectedIndex,
                Kategori = (RandevuKategori)cmbKategori.SelectedIndex,
                OlusturulmaTarihi = _mevcutRandevu?.OlusturulmaTarihi ?? DateTime.Now
            };

            // Çakışma kontrolü
            if (_service.SaatCakisiyor(randevu, _mevcutRandevu?.Id))
            {
                var sonuc = MessageBox.Show(
                    "Bu saatte başka bir randevu mevcut. Yine de kaydetmek istiyor musunuz?",
                    "Saat Çakışması",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (sonuc != DialogResult.Yes) return;
            }

            SonucRandevu = randevu;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
