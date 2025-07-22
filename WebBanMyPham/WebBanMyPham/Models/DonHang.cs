using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanMyPham.Models
{
    public class DonHangViewModel
    {

        public int MaDonHang { get; set; }
        public DateTime? Ngaydat { get; set; }
        public bool? Dathanhtoan { get; set; }
        public int? Tinhtranggiaohang { get; set; }
        public DateTime? Ngaygiao { get; set; }
        public List<SanPhamDonHang> SanPhams { get; set; } // Danh sách sản phẩm
    }

}