using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebBanMyPham.Models;
using System.Data.Entity;
using WebBanMyPham.Models.Enums;
using WebBanMyPham.Service;
using static WebBanMyPham.Service.DonHangContext;


namespace WebBanMyPham.Areas.Admin.Controllers
{
    public class QLDonHangController : Controller
    {
        private DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
        public ActionResult DanhSachDonHang(int? page, string search)
        {
            int pageSize = 10; // Số đơn hàng hiển thị mỗi trang
            int pageNumber = (page ?? 1);

            var danhSachDonHang = db.DONDATHANGs.Include("KHACHHANG").AsQueryable();

            // Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                danhSachDonHang = danhSachDonHang.Where(d =>
                    d.MaDonHang.ToString().Contains(search) ||
                    d.KHACHHANG.Taikhoan.Contains(search)
                );
            }

            return View(danhSachDonHang.OrderByDescending(d => d.Ngaydat).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult CapNhatTrangThai(int maDonHang)
        {
            var donHang = db.DONDATHANGs.Find(maDonHang);
            if (donHang == null)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("DanhSachDonHang");
            }
            var donHangContext = new DonHangContext((int)donHang.Tinhtranggiaohang);
            donHangContext.MaDonHang = donHang.MaDonHang;

            try
            {
                donHangContext.CapNhatTrangThai(); 
                donHang.Tinhtranggiaohang = (byte)donHangContext.GetTrangThaiValue();   
                if (donHangContext.TrangThaiHienThi == "Đã giao hàng" && donHang.Ngaygiao == null)
                {
                    donHang.Ngaygiao = DateTime.Now;

                }
                if (donHang.Tinhtranggiaohang == 4) 
                {
                    TempData["Message"] = "Đơn hàng đã bị hủy, không thể thay đổi trạng thái!";
                    return RedirectToAction("DanhSachDonHang");
                }

                var observer = new KhachHangObserver();
                donHangContext.Attach(observer);
                donHangContext.CapNhatTrangThai();

                db.SaveChanges();
                TempData["Message"] = "Cập nhật trạng thái thành công!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("DanhSachDonHang");
        }


        public ActionResult ChiTietDonHang(int? MaDonHang, int? MaKH)
        {
            if (MaDonHang == null)
            {
                return RedirectToAction("DanhSachDonHang");
            }

            // Lấy danh sách chi tiết đơn hàng
            var chiTietDonHang = db.CHITIETDONHANGs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.MaDonHang == MaDonHang)
                .ToList();

            if (chiTietDonHang == null || !chiTietDonHang.Any())
            {
                return HttpNotFound("Không tìm thấy chi tiết đơn hàng!");
            }

            return View(chiTietDonHang);
        }

        //Người dùng
        public ActionResult Chitietnguoidung(int id)
        {
            //Lay doi tuong nguoi dung theo ma
            KHACHHANG nguoidung = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = nguoidung.MaKH;
            if (nguoidung == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(nguoidung);
        }
    }
}
