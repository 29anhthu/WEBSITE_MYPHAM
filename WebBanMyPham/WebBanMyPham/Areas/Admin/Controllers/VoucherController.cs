using System;
using System.Linq;
using System.Web.Mvc;
using WebBanMyPham.Models;
using System.Diagnostics;
using PagedList;
using WebBanMyPham.Service;

namespace WebBanMyPham.Areas.Admin.Controllers
{
    public class VoucherController : Controller
    {
        private DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
        private readonly VoucherRepository _voucherRepo = new VoucherRepository();

        public ActionResult ThemVoucher()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThemVoucher(VOUCHER model)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            // Kiểm tra trùng mã voucher trước khi thêm
            var existingVoucher = db.VOUCHERs.FirstOrDefault(v => v.Code == model.Code);
            if (existingVoucher != null)
            {
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại. Vui lòng nhập mã khác!");
                return View(model);
            }

            if (!VoucherService.Instance.KiemTraNgayHopLe(model.NgayBatDau, model.NgayHetHan))
            {
                ModelState.AddModelError("NgayBatDau", "Ngày bắt đầu không được sau ngày hết hạn.");
                return View(model);
            }

            if (!VoucherService.Instance.KiemTraSoLuong(model.SoLuong))
            {
                ModelState.AddModelError("SoLuong", "Số lượng voucher không hợp lệ.");
                return View(model);
            }

            _voucherRepo.ThemVoucher(model);
            TempData["Success"] = "Thêm voucher thành công!";
            return RedirectToAction("DanhSachVoucher");
        }

        public ActionResult DanhSachVoucher(int page = 1)
        {
            var listVoucher = db.VOUCHERs.OrderBy(v => v.MaVoucher).ToList(); // Lấy danh sách voucher và sắp xếp theo ID (hoặc thuộc tính phù hợp)

            // Sử dụng PagedList để phân trang (10 voucher mỗi trang)
            int pageSize = 10;
            var pagedVouchers = listVoucher.ToPagedList(page, pageSize);

            return View(pagedVouchers); // Trả về dữ liệu phân trang vào View
        }
        [HttpGet]
        public ActionResult SuaVoucher(int id)
        {
            var voucher = db.VOUCHERs.Find(id);
            if (voucher == null)
            {
                return HttpNotFound();
            }
            return View(voucher);
        }

        [HttpPost]
        public ActionResult SuaVoucher(VOUCHER model)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }

            if (!VoucherService.Instance.KiemTraNgayHopLe(model.NgayBatDau, model.NgayHetHan))
            {
                ModelState.AddModelError("NgayBatDau", "Ngày bắt đầu không được sau ngày hết hạn.");
                return View(model);
            }

            if (!VoucherService.Instance.KiemTraSoLuong(model.SoLuong))
            {
                ModelState.AddModelError("SoLuong", "Số lượng voucher không hợp lệ.");
                return View(model);
            }

            _voucherRepo.SuaVoucher(model);
            TempData["Success"] = "Cập nhật voucher thành công!";
            return RedirectToAction("DanhSachVoucher");
        }

        [HttpPost]
        public ActionResult XoaVoucher(int id)
        {
            _voucherRepo.XoaVoucher(id);
            TempData["Success"] = "Xóa voucher thành công!";
            return RedirectToAction("DanhSachVoucher");
        }
    }
}
