using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RandevuTakip.Models;

namespace RandevuTakip.Services
{
    public class RandevuService
    {
        private readonly string _veriDosyasi;
        private List<Randevu> _randevular;

        public RandevuService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string klasor = Path.Combine(appData, "RandevuTakip");
            Directory.CreateDirectory(klasor);
            _veriDosyasi = Path.Combine(klasor, "randevular.json");
            _randevular = VeriYukle();
        }

        // ─── CRUD ────────────────────────────────────────────────────────────

        public List<Randevu> TumRandevular() => _randevular.ToList();

        public List<Randevu> GunRandevulari(DateTime tarih) =>
            _randevular
                .Where(r => r.Tarih.Date == tarih.Date)
                .OrderBy(r => r.BaslangicSaati)
                .ToList();

        public List<Randevu> AyRandevulari(int yil, int ay) =>
            _randevular
                .Where(r => r.Tarih.Year == yil && r.Tarih.Month == ay)
                .ToList();

        public void Ekle(Randevu randevu)
        {
            _randevular.Add(randevu);
            VeriKaydet();
        }

        public void Guncelle(Randevu randevu)
        {
            int idx = _randevular.FindIndex(r => r.Id == randevu.Id);
            if (idx >= 0)
            {
                _randevular[idx] = randevu;
                VeriKaydet();
            }
        }

        public void Sil(Guid id)
        {
            _randevular.RemoveAll(r => r.Id == id);
            VeriKaydet();
        }

        public Randevu? BulById(Guid id) =>
            _randevular.FirstOrDefault(r => r.Id == id);

        // ─── İstatistik ──────────────────────────────────────────────────────

        public int BuAyRandevuSayisi()
        {
            var simdi = DateTime.Now;
            return _randevular.Count(r => r.Tarih.Year == simdi.Year && r.Tarih.Month == simdi.Month);
        }

        public int BugunRandevuSayisi() =>
            _randevular.Count(r => r.Tarih.Date == DateTime.Today);

        public int BekleyenSayi() =>
            _randevular.Count(r => r.Durum == RandevuDurumu.Bekliyor && r.Tarih >= DateTime.Today);

        // ─── Kalıcılık ───────────────────────────────────────────────────────

        private void VeriKaydet()
        {
            string json = JsonConvert.SerializeObject(_randevular, Formatting.Indented);
            File.WriteAllText(_veriDosyasi, json);
        }

        private List<Randevu> VeriYukle()
        {
            if (!File.Exists(_veriDosyasi))
                return new List<Randevu>();

            try
            {
                string json = File.ReadAllText(_veriDosyasi);
                return JsonConvert.DeserializeObject<List<Randevu>>(json) ?? new List<Randevu>();
            }
            catch
            {
                return new List<Randevu>();
            }
        }

        // ─── Çakışma Kontrolü ────────────────────────────────────────────────

        public bool SaatCakisiyor(Randevu yeni, Guid? haricTut = null)
        {
            return _randevular
                .Where(r => r.Tarih.Date == yeni.Tarih.Date && r.Id != (haricTut ?? Guid.Empty))
                .Any(r =>
                    yeni.BaslangicSaati < r.BitisSaati &&
                    yeni.BitisSaati > r.BaslangicSaati);
        }
    }
}
