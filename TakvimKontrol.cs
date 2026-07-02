using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RandevuTakip.Models;

namespace RandevuTakip.Controls
{
    public class TakvimKontrol : Control
    {
        // ─── Olaylar ─────────────────────────────────────────────────────────
        public event EventHandler<DateTime>? GunTiklandi;
        public event EventHandler<DateTime>? GunCiftTiklandi;

        // ─── Durum ───────────────────────────────────────────────────────────
        private DateTime _gorunenAy;
        private DateTime _seciliGun;
        private Dictionary<DateTime, List<Randevu>> _randevuHaritasi = new();

        // ─── Renkler ─────────────────────────────────────────────────────────
        private readonly Color _baslikArkaplan  = Color.FromArgb(41, 128, 185);
        private readonly Color _baslikYazi      = Color.White;
        private readonly Color _gunArkaplan     = Color.White;
        private readonly Color _gunYazi         = Color.FromArgb(44, 62, 80);
        private readonly Color _bugunArkaplan   = Color.FromArgb(52, 152, 219);
        private readonly Color _bugunYazi       = Color.White;
        private readonly Color _seciliArkaplan  = Color.FromArgb(46, 204, 113);
        private readonly Color _seciliYazi      = Color.White;
        private readonly Color _digerAyGun      = Color.FromArgb(189, 195, 199);
        private readonly Color _haftalikBaslik  = Color.FromArgb(236, 240, 241);
        private readonly Color _randevuRenk     = Color.FromArgb(231, 76, 60);
        private readonly Color _sinirRenk       = Color.FromArgb(220, 220, 220);

        private const int BASLIK_YUKSEKLIK = 50;
        private const int GUN_BASLIK_YUKSEKLIK = 28;

        public TakvimKontrol()
        {
            _gorunenAy = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _seciliGun = DateTime.Today;

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
        }

        // ─── Public API ──────────────────────────────────────────────────────

        public DateTime GorunenAy
        {
            get => _gorunenAy;
            set { _gorunenAy = new DateTime(value.Year, value.Month, 1); Invalidate(); }
        }

        public DateTime SeciliGun
        {
            get => _seciliGun;
            set { _seciliGun = value; Invalidate(); }
        }

        public void RandevulariGuncelle(Dictionary<DateTime, List<Randevu>> harita)
        {
            _randevuHaritasi = harita;
            Invalidate();
        }

        public void OncekiAy()
        {
            _gorunenAy = _gorunenAy.AddMonths(-1);
            Invalidate();
        }

        public void SonrakiAy()
        {
            _gorunenAy = _gorunenAy.AddMonths(1);
            Invalidate();
        }

        // ─── Çizim ───────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;

            // Arka plan
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, w, h);

            // Başlık çiz
            BaslikCiz(g, w);

            // Gün başlıkları
            GunBasliklariCiz(g, w);

            // Günleri çiz
            GunleriCiz(g, w, h);
        }

        private void BaslikCiz(Graphics g, int w)
        {
            var rect = new Rectangle(0, 0, w, BASLIK_YUKSEKLIK);
            using var brush = new LinearGradientBrush(rect,
                Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219),
                LinearGradientMode.Horizontal);
            g.FillRectangle(brush, rect);

            string ayYil = _gorunenAy.ToString("MMMM yyyy",
                new System.Globalization.CultureInfo("tr-TR")).ToUpper();

            using var font = new Font("Segoe UI", 13, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(ayYil, font, Brushes.White, rect, sf);

            // Ok butonları
            using var arrowFont = new Font("Segoe UI", 14, FontStyle.Bold);
            g.DrawString("‹", arrowFont, Brushes.White,
                new Rectangle(5, 0, 40, BASLIK_YUKSEKLIK), sf);
            g.DrawString("›", arrowFont, Brushes.White,
                new Rectangle(w - 45, 0, 40, BASLIK_YUKSEKLIK), sf);
        }

        private void GunBasliklariCiz(Graphics g, int w)
        {
            string[] gunler = { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };
            float gunGenis = w / 7f;
            var rect = new Rectangle(0, BASLIK_YUKSEKLIK, w, GUN_BASLIK_YUKSEKLIK);
            g.FillRectangle(new SolidBrush(_haftalikBaslik), rect);

            using var font = new Font("Segoe UI", 9, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < 7; i++)
            {
                var r = new RectangleF(i * gunGenis, BASLIK_YUKSEKLIK, gunGenis, GUN_BASLIK_YUKSEKLIK);
                Color yazıRenk = (i >= 5) ? Color.FromArgb(231, 76, 60) : Color.FromArgb(44, 62, 80);
                g.DrawString(gunler[i], font, new SolidBrush(yazıRenk), r, sf);
            }

            g.DrawLine(new Pen(_sinirRenk), 0, BASLIK_YUKSEKLIK + GUN_BASLIK_YUKSEKLIK,
                w, BASLIK_YUKSEKLIK + GUN_BASLIK_YUKSEKLIK);
        }

        private void GunleriCiz(Graphics g, int w, int h)
        {
            int baslangicY = BASLIK_YUKSEKLIK + GUN_BASLIK_YUKSEKLIK;
            int kalanYukseklik = h - baslangicY;

            // Ayın ilk günü hangi haftanın günü (Pazartesi=0)
            int ilkGunHaftaGunu = ((int)_gorunenAy.DayOfWeek + 6) % 7;
            int ayGunSayisi = DateTime.DaysInMonth(_gorunenAy.Year, _gorunenAy.Month);
            int satirSayisi = (int)Math.Ceiling((ilkGunHaftaGunu + ayGunSayisi) / 7.0);
            if (satirSayisi < 5) satirSayisi = 5;

            float gunGenis = w / 7f;
            float satirYukseklik = kalanYukseklik / (float)satirSayisi;

            DateTime ilkGun = _gorunenAy.AddDays(-ilkGunHaftaGunu);

            using var normalFont = new Font("Segoe UI", 10, FontStyle.Regular);
            using var boldFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using var kucukFont = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            var sfDot = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < satirSayisi * 7; i++)
            {
                DateTime gun = ilkGun.AddDays(i);
                int sutun = i % 7;
                int satir = i / 7;

                float x = sutun * gunGenis;
                float y = baslangicY + satir * satirYukseklik;
                var gunRect = new RectangleF(x, y, gunGenis, satirYukseklik);

                bool buAy = gun.Month == _gorunenAy.Month;
                bool bugun = gun.Date == DateTime.Today;
                bool secili = gun.Date == _seciliGun.Date;
                bool haftalikGun = sutun >= 5;

                // Arka plan
                Color arkaPlan;
                Color yazıRenk;

                if (secili)
                {
                    arkaPlan = _seciliArkaplan;
                    yazıRenk = _seciliYazi;
                }
                else if (bugun)
                {
                    arkaPlan = _bugunArkaplan;
                    yazıRenk = _bugunYazi;
                }
                else if (!buAy)
                {
                    arkaPlan = Color.FromArgb(248, 249, 250);
                    yazıRenk = _digerAyGun;
                }
                else if (haftalikGun)
                {
                    arkaPlan = Color.FromArgb(255, 252, 252);
                    yazıRenk = Color.FromArgb(192, 57, 43);
                }
                else
                {
                    arkaPlan = _gunArkaplan;
                    yazıRenk = _gunYazi;
                }

                g.FillRectangle(new SolidBrush(arkaPlan), gunRect);

                // Sınır çizgisi
                g.DrawRectangle(new Pen(_sinirRenk, 0.5f),
                    gunRect.X, gunRect.Y, gunRect.Width, gunRect.Height);

                // Gün numarası
                var numRect = new RectangleF(x + 2, y + 3, gunGenis - 4, 20);
                g.DrawString(gun.Day.ToString(),
                    (bugun || secili) ? boldFont : normalFont,
                    new SolidBrush(yazıRenk), numRect, sf);

                // Randevu göstergesi
                if (buAy && _randevuHaritasi.TryGetValue(gun.Date, out var randevular) && randevular.Count > 0)
                {
                    int gosterilecek = Math.Min(randevular.Count, 3);
                    for (int ri = 0; ri < gosterilecek; ri++)
                    {
                        float etiketY = y + 24 + ri * 14;
                        if (etiketY + 12 > y + satirYukseklik) break;

                        var etiketRect = new RectangleF(x + 3, etiketY, gunGenis - 6, 12);
                        Color etiketRenk = KategoriRenk(randevular[ri].Kategori);
                        using var etiketBrush = new SolidBrush(etiketRenk);
                        g.FillRoundedRect(etiketBrush, etiketRect, 3);

                        string etiketMetin = randevular[ri].BaslikAd.Length > 8
                            ? randevular[ri].BaslikAd[..8] + "…"
                            : randevular[ri].BaslikAd;
                        g.DrawString(etiketMetin, kucukFont, Brushes.White, etiketRect, sfDot);
                    }

                    if (randevular.Count > 3)
                    {
                        float fazlaY = y + 24 + 3 * 14;
                        if (fazlaY + 12 <= y + satirYukseklik)
                        {
                            var fazlaRect = new RectangleF(x + 3, fazlaY, gunGenis - 6, 12);
                            g.DrawString($"+{randevular.Count - 3} daha", kucukFont,
                                new SolidBrush(Color.FromArgb(127, 140, 141)), fazlaRect, sfDot);
                        }
                    }
                }
            }
        }

        private Color KategoriRenk(RandevuKategori kategori) => kategori switch
        {
            RandevuKategori.Is       => Color.FromArgb(52, 152, 219),
            RandevuKategori.Saglik   => Color.FromArgb(231, 76, 60),
            RandevuKategori.Kisisel  => Color.FromArgb(155, 89, 182),
            RandevuKategori.Toplanti => Color.FromArgb(230, 126, 34),
            RandevuKategori.Diger    => Color.FromArgb(149, 165, 166),
            _                        => Color.FromArgb(46, 204, 113),
        };

        // ─── Fare Olayları ────────────────────────────────────────────────────

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            // Ok butonları
            if (e.Y < BASLIK_YUKSEKLIK)
            {
                if (e.X < 45) OncekiAy();
                else if (e.X > Width - 45) SonrakiAy();
                return;
            }

            DateTime? gun = KoordinatdanGun(e.X, e.Y);
            if (gun.HasValue)
            {
                _seciliGun = gun.Value;
                Invalidate();
                GunTiklandi?.Invoke(this, gun.Value);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            DateTime? gun = KoordinatdanGun(e.X, e.Y);
            if (gun.HasValue)
                GunCiftTiklandi?.Invoke(this, gun.Value);
        }

        private DateTime? KoordinatdanGun(int x, int y)
        {
            int baslangicY = BASLIK_YUKSEKLIK + GUN_BASLIK_YUKSEKLIK;
            if (y < baslangicY) return null;

            int ilkGunHaftaGunu = ((int)_gorunenAy.DayOfWeek + 6) % 7;
            int ayGunSayisi = DateTime.DaysInMonth(_gorunenAy.Year, _gorunenAy.Month);
            int satirSayisi = (int)Math.Ceiling((ilkGunHaftaGunu + ayGunSayisi) / 7.0);
            if (satirSayisi < 5) satirSayisi = 5;

            float gunGenis = Width / 7f;
            float satirYukseklik = (Height - baslangicY) / (float)satirSayisi;

            int sutun = (int)(x / gunGenis);
            int satir = (int)((y - baslangicY) / satirYukseklik);

            if (sutun < 0 || sutun > 6 || satir < 0 || satir >= satirSayisi) return null;

            DateTime ilkGun = _gorunenAy.AddDays(-ilkGunHaftaGunu);
            return ilkGun.AddDays(satir * 7 + sutun);
        }
    }

    // ─── Yardımcı Uzantı ─────────────────────────────────────────────────────
    public static class GraphicsExtensions
    {
        public static void FillRoundedRect(this Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }
}
