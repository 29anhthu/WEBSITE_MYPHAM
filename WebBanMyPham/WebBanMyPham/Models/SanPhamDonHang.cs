using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanMyPham.Models
{
    public class SanPhamDonHang
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}