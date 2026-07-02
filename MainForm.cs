using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RandevuTakip.Controls;
using RandevuTakip.Models;
using RandevuTakip.Services;

namespace RandevuTakip
{
    public class MainForm : Form
    {
        private readonly RandevuService _service = new();

        // ─── Kontroller ──────────────────────────────────────────────────────
        private TakvimKontrol takvim = null!;
        private Panel pnlSol = null!;
        private Panel pnlSag = null!;
        private Panel pnlUst = null!;
        private Panel pnlIstatistik = null!;
        private ListView lstRandevular = null!;
        private Label lblSeciliGun = null!;
        private Button btnYeniRandevu = null!;
        private Button btnDuzenle = null!;
        private Button btnSil = null!;
        private Label lblBugun = null!;
        private Label lblBuAy = null!;
        private Label lblBekleyen = null!;

        public MainForm()
        {
            InitializeComponent();
            TakvimGuncelle();
            GunSecildi(DateTime.Today);
        }

        private void InitializeComponent()
        {
            Text = "Randevu Takip Sistemi";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(236, 240, 241);
            Font = new Font("Segoe UI", 9.5f);

            // ── Üst başlık paneli ──
            pnlUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(44, 62, 80)
            };

            var lblUstBaslik = new Label
            {
                Text = "  📅  RANDEVU TAKİP SİSTEMİ",
                Dock = DockStyle.Left,
                Width = 400,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            var lblTarihSaat = new Label
            {
                Dock = DockStyle.Right,
                Width = 300,
                ForeColor = Color.FromArgb(189, 195, 199),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 15, 0)
            };

            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) =>
            {
                lblTarihSaat.Text = DateTime.Now.ToString("dd MMMM yyyy  HH:mm:ss",
                    new System.Globalization.CultureInfo("tr-TR"));
            };
            timer.Start();
            lblTarihSaat.Text = DateTime.Now.ToString("dd MMMM yyyy  HH:mm:ss",
                new System.Globalization.CultureInfo("tr-TR"));

            pnlUst.Controls.Add(lblUstBaslik);
            pnlUst.Controls.Add(lblTarihSaat);

            // ── İstatistik paneli ──
            pnlIstatistik = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(10, 8, 10, 8)
            };

            var flpIstat = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            lblBugun = IstatistikKart("Bugün", "0", Color.FromArgb(52, 152, 219));
            lblBuAy = IstatistikKart("Bu Ay", "0", Color.FromArgb(46, 204, 113));
            lblBekleyen = IstatistikKart("Bekleyen", "0", Color.FromArgb(230, 126, 34));

            flpIstat.Controls.Add(lblBugun.Parent!);
            flpIstat.Controls.Add(lblBuAy.Parent!);
            flpIstat.Controls.Add(lblBekleyen.Parent!);
            pnlIstatistik.Controls.Add(flpIstat);

            // ── Sol panel (takvim) ──
            pnlSol = new Panel
            {
                Dock = DockStyle.Left,
                Width = 420,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            takvim = new TakvimKontrol
            {
                Location = new Point(10, 10),
                Size = new Size(400, 380)
            };
            takvim.GunTiklandi += (s, d) => GunSecildi(d);
            takvim.GunCiftTiklandi += (s, d) => YeniRandevuAc(d);
            takvim.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Önceki/Sonraki ay butonları (takvim içinde zaten var ama ekstra buton)
            var pnlTakvimButon = new Panel
            {
                Location = new Point(10, 400),
                Size = new Size(400, 40),
                BackColor = Color.White
            };

            var btnOnceki = new Button
            {
                Text = "◀  Önceki Ay",
                Location = new Point(0, 5),
                Size = new Size(130, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOnceki.FlatAppearance.BorderSize = 0;
            btnOnceki.Click += (s, e) => { takvim.OncekiAy(); TakvimGuncelle(); };

            var btnBugune = new Button
            {
                Text = "Bugün",
                Location = new Point(135, 5),
                Size = new Size(130, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBugune.FlatAppearance.BorderSize = 0;
            btnBugune.Click += (s, e) =>
            {
                takvim.GorunenAy = DateTime.Today;
                takvim.SeciliGun = DateTime.Today;
                TakvimGuncelle();
                GunSecildi(DateTime.Today);
            };

            var btnSonraki = new Button
            {
                Text = "Sonraki Ay  ▶",
                Location = new Point(270, 5),
                Size = new Size(130, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSonraki.FlatAppearance.BorderSize = 0;
            btnSonraki.Click += (s, e) => { takvim.SonrakiAy(); TakvimGuncelle(); };

            pnlTakvimButon.Controls.Add(btnOnceki);
            pnlTakvimButon.Controls.Add(btnBugune);
            pnlTakvimButon.Controls.Add(btnSonraki);

            pnlSol.Controls.Add(takvim);
            pnlSol.Controls.Add(pnlTakvimButon);

            // ── Sağ panel (randevu listesi) ──
            pnlSag = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(10)
            };

            // Başlık + butonlar
            var pnlSagUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(245, 246, 250)
            };

            lblSeciliGun = new Label
            {
                Dock = DockStyle.Left,
                Width = 300,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var pnlButonlar = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 380,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 5, 8)
            };

            btnSil = new Button
            {
                Text = "  Sil",
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSil.FlatAppearance.BorderSize = 0;
            btnSil.Click += BtnSil_Click;

            btnDuzenle = new Button
            {
                Text = "  Düzenle",
                Size = new Size(115, 32),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnDuzenle.FlatAppearance.BorderSize = 0;
            btnDuzenle.Click += BtnDuzenle_Click;

            btnYeniRandevu = new Button
            {
                Text = "Yeni",
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnYeniRandevu.FlatAppearance.BorderSize = 0;
            btnYeniRandevu.Click += (s, e) => YeniRandevuAc(takvim.SeciliGun);

            pnlButonlar.Controls.Add(btnSil);
            pnlButonlar.Controls.Add(btnDuzenle);
            pnlButonlar.Controls.Add(btnYeniRandevu);

            pnlSagUst.Controls.Add(lblSeciliGun);
            pnlSagUst.Controls.Add(pnlButonlar);

            // ListView
            lstRandevular = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                MultiSelect = false
            };
            lstRandevular.Columns.Add("Saat", 130);
            lstRandevular.Columns.Add("Başlık", 200);
            lstRandevular.Columns.Add("Kişi", 140);
            lstRandevular.Columns.Add("Kategori", 100);
            lstRandevular.Columns.Add("Durum", 100);
            lstRandevular.Columns.Add("Telefon", 130);

            lstRandevular.SelectedIndexChanged += (s, e) =>
            {
                bool secili = lstRandevular.SelectedItems.Count > 0;
                btnDuzenle.Enabled = secili;
                btnSil.Enabled = secili;
            };
            lstRandevular.DoubleClick += (s, e) => BtnDuzenle_Click(s, e);

            // Renk satırları için OwnerDraw
            lstRandevular.OwnerDraw = true;
            lstRandevular.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), e.Bounds);
                e.Graphics.DrawString(e.Header.Text,
                    new Font("Segoe UI", 9, FontStyle.Bold),
                    Brushes.White,
                    new Rectangle(e.Bounds.X + 5, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                    new StringFormat { LineAlignment = StringAlignment.Center });
            };
            lstRandevular.DrawItem += (s, e) => e.DrawDefault = true;
            lstRandevular.DrawSubItem += (s, e) => e.DrawDefault = true;

            var pnlListeKap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            pnlListeKap.Controls.Add(lstRandevular);

            // Detay paneli (alt)
            var pnlDetay = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblDetayBaslik = new Label
            {
                Text = "Randevu Detayları",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };

            var rtbDetay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            lstRandevular.SelectedIndexChanged += (s, e) =>
            {
                if (lstRandevular.SelectedItems.Count == 0)
                {
                    rtbDetay.Clear();
                    return;
                }
                var id = (Guid)lstRandevular.SelectedItems[0].Tag;
                var r = _service.BulById(id);
                if (r == null) return;

                rtbDetay.Clear();
                rtbDetay.AppendText($"📋 {r.BaslikAd}  |  {r.BaslangicSaatiStr} – {r.BitisSaatiStr}\n");
                rtbDetay.AppendText($"👤 {r.KisiAdi}   📞 {r.Telefon}   ✉ {r.Email}\n");
                rtbDetay.AppendText($"📝 {r.Aciklama}");
            };

            pnlDetay.Controls.Add(rtbDetay);
            pnlDetay.Controls.Add(lblDetayBaslik);

            pnlSag.Controls.Add(pnlListeKap);
            pnlSag.Controls.Add(pnlDetay);
            pnlSag.Controls.Add(pnlSagUst);

            // ── Yerleşim ──
            Controls.Add(pnlSag);
            Controls.Add(pnlSol);
            Controls.Add(pnlIstatistik);
            Controls.Add(pnlUst);

            Resize += (s, e) =>
            {
                takvim.Size = new Size(pnlSol.Width - 20, 380);
            };
        }

        // ─── İstatistik kart yardımcısı ──────────────────────────────────────

        private Label IstatistikKart(string baslik, string deger, Color renk)
        {
            var pnl = new Panel
            {
                Size = new Size(160, 55),
                BackColor = renk,
                Margin = new Padding(5, 0, 5, 0)
            };

            var lblBaslik = new Label
            {
                Text = baslik,
                Location = new Point(10, 5),
                Size = new Size(140, 18),
                ForeColor = Color.FromArgb(220, 255, 220),
                Font = new Font("Segoe UI", 8.5f)
            };

            var lblDeger = new Label
            {
                Text = deger,
                Location = new Point(10, 22),
                Size = new Size(140, 28),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            pnl.Controls.Add(lblBaslik);
            pnl.Controls.Add(lblDeger);

            return lblDeger;
        }

        // ─── İş Mantığı ──────────────────────────────────────────────────────

        private void TakvimGuncelle()
        {
            var ay = takvim.GorunenAy;
            var ayRandevular = _service.AyRandevulari(ay.Year, ay.Month);
            var harita = ayRandevular
                .GroupBy(r => r.Tarih.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
            takvim.RandevulariGuncelle(harita);
            IstatistikleriGuncelle();
        }

        private void IstatistikleriGuncelle()
        {
            lblBugun.Text = _service.BugunRandevuSayisi().ToString();
            lblBuAy.Text = _service.BuAyRandevuSayisi().ToString();
            lblBekleyen.Text = _service.BekleyenSayi().ToString();
        }

        private void GunSecildi(DateTime tarih)
        {
            lblSeciliGun.Text = tarih.ToString("dd MMMM yyyy, dddd",
                new System.Globalization.CultureInfo("tr-TR"));

            lstRandevular.Items.Clear();
            btnDuzenle.Enabled = false;
            btnSil.Enabled = false;

            var randevular = _service.GunRandevulari(tarih);

            foreach (var r in randevular)
            {
                var item = new ListViewItem($"{r.BaslangicSaatiStr} – {r.BitisSaatiStr}");
                item.SubItems.Add(r.BaslikAd);
                item.SubItems.Add(r.KisiAdi);
                item.SubItems.Add(KategoriMetin(r.Kategori));
                item.SubItems.Add(DurumMetin(r.Durum));
                item.SubItems.Add(r.Telefon);
                item.Tag = r.Id;
                item.BackColor = DurumRenk(r.Durum);
                lstRandevular.Items.Add(item);
            }

            if (randevular.Count == 0)
            {
                var bos = new ListViewItem("Bu gün için randevu bulunmuyor.");
                bos.ForeColor = Color.FromArgb(149, 165, 166);
                lstRandevular.Items.Add(bos);
            }
        }

        private void YeniRandevuAc(DateTime tarih)
        {
            using var form = new RandevuTakip.Forms.RandevuForm(_service, tarih);
            if (form.ShowDialog(this) == DialogResult.OK && form.SonucRandevu != null)
            {
                _service.Ekle(form.SonucRandevu);
                TakvimGuncelle();
                GunSecildi(tarih);
            }
        }

        private void BtnDuzenle_Click(object? sender, EventArgs e)
        {
            if (lstRandevular.SelectedItems.Count == 0) return;
            if (lstRandevular.SelectedItems[0].Tag is not Guid id) return;

            var randevu = _service.BulById(id);
            if (randevu == null) return;

            using var form = new RandevuTakip.Forms.RandevuForm(_service, randevu.Tarih, randevu);
            if (form.ShowDialog(this) == DialogResult.OK && form.SonucRandevu != null)
            {
                _service.Guncelle(form.SonucRandevu);
                TakvimGuncelle();
                GunSecildi(takvim.SeciliGun);
            }
        }

        private void BtnSil_Click(object? sender, EventArgs e)
        {
            if (lstRandevular.SelectedItems.Count == 0) return;
            if (lstRandevular.SelectedItems[0].Tag is not Guid id) return;

            var randevu = _service.BulById(id);
            if (randevu == null) return;

            var sonuc = MessageBox.Show(
                $"'{randevu.BaslikAd}' randevusunu silmek istediğinizden emin misiniz?",
                "Randevu Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (sonuc == DialogResult.Yes)
            {
                _service.Sil(id);
                TakvimGuncelle();
                GunSecildi(takvim.SeciliGun);
            }
        }

        // ─── Yardımcı Metinler ────────────────────────────────────────────────

        private static string KategoriMetin(RandevuKategori k) => k switch
        {
            RandevuKategori.Is       => "İş",
            RandevuKategori.Saglik   => "Sağlık",
            RandevuKategori.Kisisel  => "Kişisel",
            RandevuKategori.Toplanti => "Toplantı",
            RandevuKategori.Diger    => "Diğer",
            _                        => "Genel"
        };

        private static string DurumMetin(RandevuDurumu d) => d switch
        {
            RandevuDurumu.Onaylandi   => "Onaylandı",
            RandevuDurumu.Tamamlandi  => "Tamamlandı",
            RandevuDurumu.Iptal       => "İptal",
            _                         => "Bekliyor"
        };

        private static Color DurumRenk(RandevuDurumu d) => d switch
        {
            RandevuDurumu.Onaylandi   => Color.FromArgb(232, 245, 233),
            RandevuDurumu.Tamamlandi  => Color.FromArgb(227, 242, 253),
            RandevuDurumu.Iptal       => Color.FromArgb(255, 235, 238),
            _                         => Color.FromArgb(255, 253, 231)
        };
    }
}
