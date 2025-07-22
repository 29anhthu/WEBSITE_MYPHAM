using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanMyPham.Models;
using WebBanMyPham.Models.Enums;

namespace WebBanMyPham.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
        // GET: Admin/Home
        public ActionResult Index()
        {
             int soLuongSanPham = db.SanPhams.Count();

            int soLuongDonHang = db.DONDATHANGs.Count();
            decimal tongDoanhThu = db.CHITIETDONHANGs
       .Where(ct => ct.DONDATHANG.Dathanhtoan == true && ct.DONDATHANG.Tinhtranggiaohang == 3)
       .Sum(ct => (decimal?)ct.Soluong * ct.Dongia) ?? 0; // Dùng nullable để tránh lỗi ép kiểu

            ViewBag.SoLuongSanPham = soLuongSanPham;
            ViewBag.SoLuongDonHang = soLuongDonHang;
            ViewBag.TongDoanhThu = tongDoanhThu;

            return View();
        }
    }
}