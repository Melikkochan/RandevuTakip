using System;

namespace RandevuTakip.Models
{
    public class Randevu
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Tarih { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public TimeSpan BitisSaati { get; set; }
        public string BaslikAd { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public string KisiAdi { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;
        public RandevuKategori Kategori { get; set; } = RandevuKategori.Genel;
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        public string BaslangicSaatiStr => BaslangicSaati.ToString(@"hh\:mm");
        public string BitisSaatiStr => BitisSaati.ToString(@"hh\:mm");

        public string TamBilgi =>
            $"{BaslangicSaatiStr} - {BitisSaatiStr} | {BaslikAd} ({KisiAdi})";
    }

    public enum RandevuDurumu
    {
        Bekliyor,
        Onaylandi,
        Tamamlandi,
        Iptal
    }

    public enum RandevuKategori
    {
        Genel,
        Is,
        Saglik,
        Kisisel,
        Toplanti,
        Diger
    }
}
