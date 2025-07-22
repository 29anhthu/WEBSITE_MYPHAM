using System;
using System.Linq;
using System.Web.Mvc;
using WebBanMyPham.Models;
using PagedList;
using WebBanMyPham.Service;

namespace WebBanMyPham.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4(); // Giả sử bạn có DbContext
        private int? page;
        public ActionResult Index()
        {
            return RedirectToAction("DanhSachSanPham");
        }
        public ActionResult DanhSachSanPham()
        {
            int pageSize = 12; 
            int pageNumber = (page ?? 1); 
            var SanPhams = db.SanPhams.ToList();
            return View(SanPhams.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult ChiTietSanPham(int id)
        {
            var SanPham = db.SanPhams.Find(id);
            if (SanPham == null)
            {
                return HttpNotFound();
            }
            var danhGias = db.DanhGias.Where(d => d.MaSP == id).OrderByDescending(d => d.NgayDanhGia).ToList();
            ViewBag.DanhGias = danhGias;
            return View(SanPham);
        }
        private readonly DanhGiaFacade danhGiaFacade = new DanhGiaFacade();
        [HttpPost]
        public ActionResult ThemDanhGia(int? maSP, int soSao, string noiDung)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
            }

            int maKH = (int)Session["MaKH"]; // Lấy mã khách hàng từ session

            if (!maSP.HasValue) 
            {
                TempData["Error"] = "Lỗi: Không xác định được sản phẩm.";
                return RedirectToAction("DanhSachSanPham");
            }
            if (maKH == 0)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đánh giá.";
                return RedirectToAction("DangNhap");
            }
            if (!danhGiaFacade.KiemTraDaMua(maKH, maSP.Value))
            {
                TempData["Error"] = "Bạn chưa mua sản phẩm này nên không thể đánh giá.";
                return RedirectToAction("ChiTietSanPham", new { id = maSP.Value });
            }
            bool ketQua = danhGiaFacade.ThemDanhGia(maKH, maSP.Value, soSao, noiDung);
            if (ketQua)
            {
                TempData["Success"] = "Đánh giá thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi thêm đánh giá.";
            }
            return RedirectToAction("ChiTietSanPham", new { id = maSP.Value });
        }
        public ActionResult DanhSachDanhGia(int maSP)
        {
            var danhGias = danhGiaFacade.LayDanhSachDanhGia(maSP)?.ToList();
            if (danhGias == null || !danhGias.Any())
            {
                return HttpNotFound("Không có đánh giá nào cho sản phẩm này.");
            }
            return RedirectToAction("DanhSachSanPham");
        }
    }
 }