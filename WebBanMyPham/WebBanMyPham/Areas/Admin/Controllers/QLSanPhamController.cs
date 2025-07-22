using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanMyPham.Models;
using PagedList;
using System.IO;
using System.Data.Entity;
using WebBanMyPham.Service;
using System.Web.UI.WebControls;


namespace WebBanMyPham.Areas.Admin.Controllers
{
    public class QLSanPhamController : Controller
    {
        DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4(); 
        public ActionResult SanPham(int? page)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            //return View(db.SanPhams.ToList());
            return View(db.SanPhams.ToList().OrderBy(n => n.MaSP)
                                   .ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult ThemmoiSanPham()
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            ViewBag.MaLoai = new SelectList(db.LoaiSPs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemmoiSanPham(SanPham sanPham, HttpPostedFileBase fileupload)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");

            if (!ModelState.IsValid)
            {
                return View(sanPham);
            }

            try
            {
                var builder = new SanPhamBuilder(sanPham)
                    .SetTenSP(sanPham.TenSP)
                    .SetGiaBan(sanPham.Giaban)
                    .SetMoTa(sanPham.Mota)
                    .SetNgayCapNhat(DateTime.Now)
                    .SetSoLuongTon(sanPham.Soluongton)
                    .SetMaLoai(sanPham.MaLoai)
                    .SetHinhAnh(Server, fileupload, null)
                    .Build();

                db.SanPhams.Add(builder);
                db.SaveChanges();

                TempData["Thongbao1"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("SanPham");
            }
            catch (Exception ex)
            {
                ViewBag.Thongbao = "Lỗi: " + ex.Message;
                return View(sanPham);
            }
        }


        public ActionResult ChitietSanPham(int id)
        {
            //Lay doi tuong SanPham theo ma
            SanPham SanPham = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            ViewBag.MaSP = SanPham.MaSP;
            if (SanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(SanPham);
        }
        public ActionResult XoaSanPham(int id)
        {
            //Lay ra doi tuong SanPham can xoa theo ma
            SanPham SanPham = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            ViewBag.MaSP = SanPham.MaSP;
            if (SanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(SanPham);
        }
        [HttpPost, ActionName("XoaSanPham")]
        public ActionResult Xacnhanxoa(int id)
        {
            //Lay ra doi tuong SanPham can xoa theo ma
            SanPham SanPham = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            ViewBag.MaSP = SanPham.MaSP;
            if (SanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.SanPhams.Remove(SanPham);
            db.SaveChanges();
            return RedirectToAction("SanPham");
        }
        // GET: Sửa sản phẩm
        [HttpGet]
        public ActionResult SuaSanPham(int id)
        {
            // Lấy đối tượng sách cần sửa theo mã
            SanPham SanPham = db.SanPhams.SingleOrDefault(n => n.MaSP == id);
            if (SanPham == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy sách
            }

            // Lấy danh sách từ table LOAI, sắp xếp theo tên loại
            ViewBag.MaLoai = new SelectList(db.LoaiSPs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai", SanPham.MaLoai);
            return View(SanPham);
        }

        // POST: Sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(SanPham sanPham, HttpPostedFileBase fileupload)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai", sanPham.MaLoai);

            if (!ModelState.IsValid)
            {
                return View(sanPham);
            }

            try
            {
                var sanPhamHienTai = db.SanPhams.AsNoTracking().FirstOrDefault(n => n.MaSP == sanPham.MaSP);
                if (sanPhamHienTai == null)
                {
                    return HttpNotFound();
                }

                var sanPhamCapNhat = new SanPhamBuilder(sanPhamHienTai)
                    .SetTenSP(sanPham.TenSP)
                    .SetGiaBan(sanPham.Giaban)
                    .SetMoTa(sanPham.Mota)
                    .SetNgayCapNhat(DateTime.Now)
                    .SetSoLuongTon(sanPham.Soluongton)
                    .SetMaLoai(sanPham.MaLoai)
                    .SetHinhAnh(Server, fileupload, sanPhamHienTai.Anhbia)
                    .Build();

                db.Entry(sanPhamCapNhat).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Thongbao1"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("SanPham");
            }
            catch (Exception ex)
            {
                ViewBag.Thongbao = "Lỗi: " + ex.Message;
                return View(sanPham);
            }
        }

        //Phân loại sản phẩm
        public ActionResult Loai(int? page)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            //return View(db.SACHes.ToList());
            return View(db.LoaiSPs.ToList().OrderBy(n => n.MaLoai).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult ThemmoiLoai()
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            // Không cần các ViewBag cho MaLoai hay MaNXB vì đây là thêm loại
            return View();
        }
        [HttpPost]
        //[ValidateInput(false)]
        public ActionResult ThemmoiLoai(LoaiSP loai)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            // Kiểm tra nếu Model hợp lệ
            if (ModelState.IsValid)
            {
                // Thêm vào CSDL
                db.LoaiSPs.Add(loai);
                db.SaveChanges();

                TempData["Thongbao1"] = "Thêm loại sản phẩm thành công!";
                return RedirectToAction("Loai", "QLSanPham"); // Điều hướng tới danh sách loại sau khi thêm
            }

            return View(loai);
        }

        // Sửa loại
        [HttpGet]
        public ActionResult Sualoai(int id)
        {
            // Lấy ra đối tượng loại cần sửa theo mã
            LoaiSP loai = db.LoaiSPs.SingleOrDefault(n => n.MaLoai == id);
            if (loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(loai);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sualoai(LoaiSP loai)
        {
            if (Session["TaikhoanAd"] == null)
            {
                return RedirectToAction("Dangnhap", "NguoiDung", new { area = "" });
            }
            if (ModelState.IsValid)
            {
                // Cập nhật thông tin loại
                var loaiToUpdate = db.LoaiSPs.FirstOrDefault(n => n.MaLoai == loai.MaLoai);

                if (loaiToUpdate != null)
                {
                    loaiToUpdate.TenLoai = loai.TenLoai; // Cập nhật tên loại
                    db.SaveChanges(); // Lưu thay đổi vào CSDL

                    TempData["ThongBao1"] = "Cập nhật thành công!";
                    return RedirectToAction("Loai");
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy loại cần sửa");
                    return View(loai);
                }
            }

            return View(loai);
        }

        [HttpGet]
        public ActionResult XoaLoai(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            LoaiSP Loai = db.LoaiSPs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = Loai.MaLoai;
            if (Loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(Loai);
        }
        [HttpPost, ActionName("XoaLoai")]
        public ActionResult Xacnhanxoaloai(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            LoaiSP Loai = db.LoaiSPs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = Loai.MaLoai;
            if (Loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.LoaiSPs.Remove(Loai);
            db.SaveChanges();
            return RedirectToAction("Loai");
        }
   
    }
}