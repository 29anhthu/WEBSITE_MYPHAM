using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanMyPham.Models;
using System.Threading;
using PagedList;
using System.Web.UI;
using System.Text.RegularExpressions;
using WebBanMyPham.Service;


namespace WebBanMyPham.Controllers
{
    public class NguoiDungController : Controller
    {
        // GET: NguoiDung
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection, KHACHHANG kh)
        {
            // Lấy dữ liệu từ form
            var hoten = collection["HotenKH"];
            var tendn = collection["TenDN"];
            var matkhau = collection["Matkhau"];
            var matkhaunhaplai = collection["Matkhaunhaplai"];
            var email = collection["Email"];
            var diachi = collection["Diachi"];
            var dienthoai = collection["Dienthoai"];
            var ngaysinh = collection["Ngaysinh"];

            var existingUser = db.KHACHHANGs.FirstOrDefault(k => k.Taikhoan == tendn);
            if (existingUser != null)
            {
                ViewBag.ThongBao = "Tài khoản đã được đăng ký trước đó!";
                return View();
            }

            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(hoten)) errors["Loi1"] = "Họ tên không được để trống!";
            if (string.IsNullOrEmpty(tendn)) errors["Loi2"] = "Tên đăng nhập không được để trống!";
            if (string.IsNullOrEmpty(matkhau)) errors["Loi3"] = "Mật khẩu không được để trống!";
            if (string.IsNullOrEmpty(matkhaunhaplai)) errors["Loi4"] = "Mật khẩu nhập lại không được để trống!";
            if (matkhaunhaplai != matkhau) errors["Loi4"] = "Mật khẩu nhập lại không khớp!";
            if (string.IsNullOrEmpty(email)) errors["Loi5"] = "Email không được để trống!";
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) errors["LoiEmail"] = "Email không hợp lệ!";
            if (string.IsNullOrEmpty(diachi)) errors["Loi6"] = "Địa chỉ không được để trống!";
            if (string.IsNullOrEmpty(dienthoai)) errors["Loi7"] = "Số điện thoại không được để trống!";
            if (!DateTime.TryParse(ngaysinh, out DateTime parsedNgaySinh)) errors["Loi8"] = "Ngày sinh không hợp lệ!";

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ViewData[error.Key] = error.Value;
                }
                return View();
            }

            kh.HoTen = hoten;
            kh.Taikhoan = tendn;
            kh.Matkhau = matkhau;
            kh.Email = email;
            kh.DiachiKH = diachi;
            kh.DienthoaiKH = dienthoai;
            kh.Ngaysinh = parsedNgaySinh;

            // Áp dụng Decorator Pattern
            IDangKyKhachHang dangKy = new DangKyKhachHang(db);
            IDangKyKhachHang dangKyCoThongBao = new ThongBaoDecorator(dangKy);

            // Thực hiện đăng ký kèm thông báo
            dangKyCoThongBao.DangKy(kh);

            TempData["ThongBao"] = $"🎉 Chúc mừng {kh.Taikhoan}, bạn đã đăng ký thành công!";
            return RedirectToAction("Dangnhap");
        }

        [HttpGet]
        public ActionResult Dangnhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Dangnhap(FormCollection collection)
        {
            var tendn = collection["TenDN"];
            var matkhau = collection["Matkhau"];

            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Chưa nhập tên đăng nhập!";
                return View();
            }
            if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Chưa nhập mật khẩu!";
                return View();
            }

            IAuthentication auth = new ProxyAuthentication();
            var user = auth.Login(tendn, matkhau);

            if (user is KHACHHANG kh)
            {
                Session["Taikhoan"] = kh;
                Session["Role"] = "User";
                Session["TDN"] = kh.HoTen;
                Session["HoTen"] = kh.HoTen; // Cần thêm dòng này
                Session["MaKH"] = kh.MaKH;
                Session["DiachiKH"] = kh.DiachiKH;
                Session["DienthoaiKH"] = kh.DienthoaiKH;
                return RedirectToAction("DatHang", "Giohang");
            }
            else if (user is Admin ad)
            {
                Session["TaikhoanAd"] = ad;
                Session["Role"] = "Admin";
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }

            ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        public ActionResult TimKiem()
        {
            return View();
        }
        DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
        [HttpGet]
        public ActionResult TimKiem(string sTuKhoa, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;
            var lstSP = db.SanPhams.Where(n => n.TenSP.Contains(sTuKhoa) || string.IsNullOrEmpty(sTuKhoa));
            ViewBag.TuKhoa = sTuKhoa;
            return View(lstSP.OrderBy(n => n.TenSP).ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        public ActionResult LayTuKhoaTimKiem(string sTuKhoa)
        {
            if (string.IsNullOrEmpty(sTuKhoa))
            {
                return RedirectToAction("Layout", "Share"); // hoặc trang chính của bạn
            }
            return RedirectToAction("TimKiem", new { sTuKhoa = sTuKhoa });
        }
        public ActionResult Thoat()
        {
            Session["Taikhoan"] = null; // Xóa thông tin tài khoản trong session
            return RedirectToAction("DanhSachSanPham", "SanPham"); // Redirect về trang danh sách sản phẩm
        }
        // Hiển thị form cập nhật thông tin
        public ActionResult CapNhatThongTin()
        {
            // Giả sử đang có ID khách hàng đang đăng nhập
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
            int khachhangID = ((KHACHHANG)Session["Taikhoan"]).MaKH;
            var khachhang = db.KHACHHANGs.Find(khachhangID);

            if (khachhang == null)
            {
                return HttpNotFound();
            }

            return View(khachhang);
        }

        // POST: Cập nhật thông tin khách hàng
        [HttpPost]
        public ActionResult CapNhatThongTin(FormCollection collection)
        {
            // Giả sử đang có ID khách hàng đang đăng nhập
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
            int khachhangID = ((KHACHHANG)Session["Taikhoan"]).MaKH;
            var kh = db.KHACHHANGs.Find(khachhangID);

            // Lấy dữ liệu từ form
            var hoten = collection["HoTen"];
            var email = collection["Email"];
            var diachi = collection["DiachiKH"];
            var dienthoai = collection["DienthoaiKH"];
            var ngaysinh = collection["Ngaysinh"];

            // Kiểm tra các điều kiện nhập liệu
            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["LoiHoTen"] = "Họ tên không được để trống!";
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewData["LoiEmail"] = "Email không đúng định dạng!";
            }
            else if (String.IsNullOrEmpty(diachi))
            {
                ViewData["LoiDiaChi"] = "Địa chỉ không được để trống!";
            }
            else if (String.IsNullOrEmpty(dienthoai))
            {
                ViewData["LoiDienThoai"] = "Số điện thoại không được để trống!";
            }
            else
            {
                // Cập nhật dữ liệu
                kh.HoTen = hoten;
                kh.Email = email;
                kh.DiachiKH = diachi;
                kh.DienthoaiKH = dienthoai;
                kh.Ngaysinh = DateTime.Parse(ngaysinh);

                db.SaveChanges();
                return RedirectToAction("ThongTinCaNhan"); // Chuyển hướng sau khi cập nhật thành công
            }

            return View(kh);
        }
        public ActionResult ThongTinCaNhan()
        {
            int khachhangID = ((KHACHHANG)Session["Taikhoan"]).MaKH;
            DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();
            var khachhang = db.KHACHHANGs.Find(khachhangID);
            return View(khachhang);
        }
    }
}
 