using Microsoft.Ajax.Utilities;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanMyPham.Models;
using WebBanMyPham.Models.Enums;
using WebBanMyPham.Service;
using static WebBanMyPham.Service.DonHangContext;


namespace WebBanMyPham.Controllers
{
    public class GioHangController : Controller
    {
        //Tao doi tuong data chua du lieu tu model dbBansach da tao
        DBQLMYPHAMEntities4 data = new DBQLMYPHAMEntities4();
        //Lay gio hang
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                //Neu gio hang chua ton tai thoi khoi tao listGioHang
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }
       
        // VOUCHER  
        public ActionResult DanhSachVoucher(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var vouchers = data.VOUCHERs.Where(v => v.SoLuong > 0 && v.NgayBatDau <= DateTime.Now && v.NgayHetHan >= DateTime.Now).ToList();
            return View(vouchers);
        }
        [HttpPost]
        public ActionResult ApDungVoucher(string code, string returnUrl)
        {
            var voucher = data.VOUCHERs.FirstOrDefault(v => v.Code == code && v.SoLuong > 0
                        && v.NgayBatDau <= DateTime.Now && v.NgayHetHan >= DateTime.Now);

            if (voucher != null)
            {
                if (Session["VoucherCode"] != null && Session["VoucherCode"].ToString() == voucher.Code)
                {
                    TempData["Error"] = "Voucher đã được áp dụng!";
                    return Redirect(returnUrl);
                }

                decimal tongTienGoc = LayGioHang().Sum(n => Convert.ToDecimal(n.dThanhtien));

                // Lấy Strategy phù hợp từ Factory
                IVoucherStrategy discountStrategy = VoucherStrategyFactory.GetStrategy(voucher.LoaiGiamGia);

                // Áp dụng giảm giá
                decimal discount = discountStrategy.ApplyDiscount(tongTienGoc, voucher.GiamGia);

                // Lưu thông tin vào session
                Session["Voucher"] = voucher;
                Session["VoucherDiscount"] = discount;
                Session["VoucherCode"] = voucher.Code;

                return Redirect(returnUrl);
            }
            TempData["Error"] = "Voucher không hợp lệ hoặc đã hết hạn.";
            return Redirect(returnUrl);
        }

        [HttpGet]
        public ActionResult XoaVoucher()
        {
            // Xóa session voucher
            Session["VoucherCode"] = null;
            Session["VoucherDiscount"] = null;

            // Cập nhật tổng tiền
            ViewBag.TongTien = TongTien();

            return RedirectToAction("GioHang");
        }

        //Tong so luong
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;

            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoluong);
            }
            return iTongSoLuong;

        }
        private decimal TongTien()
        {
            decimal iTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;

            if (lstGioHang != null)
            {
                iTongTien = lstGioHang.Sum(n => Convert.ToDecimal(n.dThanhtien));
            }

            // Lấy giá trị giảm giá từ session
            decimal discount = 0;
            if (Session["VoucherDiscount"] != null)
            {
                discount = Convert.ToDecimal(Session["VoucherDiscount"]);
            }

            // Cập nhật tổng tiền sau khi trừ giảm giá
            iTongTien -= discount;
            if (iTongTien < 0) iTongTien = 0; // Đảm bảo không bị âm

            return iTongTien;
        }
        public ActionResult GioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang.Count == 0)
            {
                return RedirectToAction("DanhSachSanPham", "SanPham");
            }

            ViewBag.Tongsoluong = TongSoLuong();
            decimal tongTien = lstGioHang.Sum(n => Convert.ToDecimal(n.dThanhtien)); // Tính tổng tiền gốc

            // Kiểm tra và áp dụng voucher
            decimal giamGia = 0;
            string maVoucher = null;

            if (Session["Voucher"] is WebBanMyPham.Models.VOUCHER voucher)
            {
                giamGia = voucher.LoaiGiamGia
                    ? (tongTien * (voucher.GiamGia / 100m))  // Giảm theo %
                    : voucher.GiamGia;                       // Giảm số tiền cố định

                maVoucher = voucher.Code;

                // Đảm bảo giảm giá không lớn hơn tổng tiền
                if (giamGia > tongTien) giamGia = tongTien;
            }

            ViewBag.GiamGia = giamGia;
            ViewBag.MaVoucher = maVoucher;
            ViewBag.TongTien = TongTien(); // Luôn cập nhật tổng tiền khi vào giỏ hàng // Cập nhật tổng tiền sau giảm giá
                                           // Nếu giỏ hàng trống thì cập nhật lại số lượng trong session
                                           // Kiểm tra Session có dữ liệu không
            if (Session["SL"] == null)
            {
                Session["SL"] = 0;
            }
            else
            {
                Session["SL"] = lstGioHang.Sum(n => n.iSoluong);
            }

            ViewBag.SoLuongGioHang = Session["SL"];
            return View(lstGioHang);
        }
        private GioHangInvoker _gioHangInvoker = new GioHangInvoker();
        public ActionResult ThemGioHang(int iMaSP, string strURL)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanpham = lstGioHang.Find(n => n.iMaSP == iMaSP);

            if (sanpham == null)
            {
                sanpham = new GioHang(iMaSP);
                lstGioHang.Add(sanpham);
            }
            else
            {
                sanpham.iSoluong++;
            }

            // Cập nhật tổng số lượng sản phẩm trong giỏ vào Session["SL"]
            Session["SL"] = lstGioHang.Sum(n => n.iSoluong);

            return Redirect(strURL);
        }
        public ActionResult XoaGioHang(int iMaSP)
        {
            //Lay gio hang Session
            List<GioHang> lstGioHang = LayGioHang();
            var command = new XoaKhoiGioHangCommand(lstGioHang, iMaSP);
            _gioHangInvoker.ThucThiLenh(command);
            // Cập nhật lại số lượng sản phẩm trong Session
            Session["SL"] = lstGioHang.Sum(n => n.iSoluong);
            return RedirectToAction("GioHang");
        }
        //Cap nhat Gio hang
        public ActionResult CapnhatGioHang(int iMaSP, FormCollection f)
        {
            //Lay gio hang tu Session
            List<GioHang> lstGioHang = LayGioHang();
            int soLuongMoi = int.Parse(f["txtSoluong"].ToString());
            var command = new CapNhatSoLuongCommand(lstGioHang, iMaSP, soLuongMoi);
            _gioHangInvoker.ThucThiLenh(command);
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaTatCaGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang> ?? new List<GioHang>();

            var command = new XoaTatCaGioHangCommand(lstGioHang, Session);
            var invoker = new GioHangInvoker();
            invoker.ThucThiLenh(command);

            return RedirectToAction("DanhSachSanPham", "SanPham");
        }

        //Hien thi View DatHang de cap nhat cac thong tin cho Don Hang
        [HttpGet]
        public ActionResult DatHang(int? iMaSP)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Kiểm tra giỏ hàng
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang == null || lstGioHang.Count == 0)
            {
                return RedirectToAction("DanhSachSanPham", "SanPham");
            }

            // Lấy thông tin khách hàng từ session
            KHACHHANG kh = Session["Taikhoan"] as KHACHHANG;
            if (kh != null)
            {
                ViewBag.HoTen = kh.HoTen;
                ViewBag.DiachiKH = kh.DiachiKH;
                ViewBag.DienthoaiKH = kh.DienthoaiKH;
            }

            // Lấy mã voucher từ session nếu đã nhập trước đó
            ViewBag.MaVoucher = Session["MaVoucher"] as int?;

            // Cập nhật lại số lượng hiển thị trên giỏ hàng
            int tongSoLuong = lstGioHang.Sum(n => n.iSoluong);
            Session["SL"] = tongSoLuong;

            ViewBag.Tongsoluong = tongSoLuong;
            ViewBag.Tongtien = TongTien();

            return View(lstGioHang);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            //Them Don Hang
            DONDATHANG ddh = new DONDATHANG();
            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
            if (kh == null)
            {
                return RedirectToAction("Dangnhap", "Nguoidung");
            }
            List<GioHang> gh = LayGioHang();
            if (gh == null || gh.Count == 0)
            {
                return RedirectToAction("GioHang", "GioHang");
            }
            ddh.MaKH = kh.MaKH;
            ddh.Ngaydat = DateTime.Now;
            var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["Ngaygiao"]);
            ddh.Ngaygiao = DateTime.Parse(ngaygiao);
            ddh.Tinhtranggiaohang = (int)OrderStatus.ChoXacNhan;
            ddh.Dathanhtoan = false;
           
            data.DONDATHANGs.Add(ddh);
            data.SaveChanges();
            //Them chi tiet don hang
            foreach (var item in gh)
            {
                CHITIETDONHANG ctdh = new CHITIETDONHANG();
                ctdh.MaDonHang = ddh.MaDonHang;
                ctdh.MaSP = item.iMaSP;
                ctdh.Soluong = item.iSoluong;
                ctdh.Dongia = (decimal)item.pDonggia;
                data.CHITIETDONHANGs.Add(ctdh);
            }

            data.SaveChanges();
            // Lấy mã voucher từ form
            int maVoucher;
            if (int.TryParse(collection["MaVoucher"], out maVoucher))
            {
                var voucher = data.VOUCHERs.FirstOrDefault(v => v.MaVoucher == ddh.MaVoucher);
                if (voucher != null && voucher.SoLuong > 0)
                {
                    voucher.SoLuong--; // Giảm số lượng voucher sau khi sử dụng
                    data.SaveChanges();
                }
            }
            Session["Giohang"] = null;
            Session["VoucherCode"] = null;
            Session["VoucherDiscount"] = null;
            return RedirectToAction("Xacnhandonhang", "GioHang");
        }
        public ActionResult Xacnhandonhang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return View();
        }
        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return View();
        }
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Giohang/PaymentWithPayPal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                    else
                    {
                        // Lưu sản phẩm vào cơ sở dữ liệu sau khi thanh toán thành công
                        var gioHangList = Session["GioHang"] as List<GioHang>;
                        if (gioHangList != null)
                        {
                            using (DBQLMYPHAMEntities4 data = new DBQLMYPHAMEntities4())
                            {
                                if (Session["Taikhoan"] != null)
                                {
                                    KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];

                                    var order = new DONDATHANG
                                    {
                                        Dathanhtoan = true,
                                        Tinhtranggiaohang = 0,
                                        Ngaydat = DateTime.Now,
                                        Ngaygiao = null, // cập nhật sau khi giao hàng
                                        MaKH = kh.MaKH
                                    };

                                    data.DONDATHANGs.Add(order);
                                    data.SaveChanges();

                                    // Lưu chi tiết đơn hàng vào CHITIETDONHANG
                                    foreach (var item in gioHangList)
                                    {
                                        var orderDetail = new CHITIETDONHANG
                                        {
                                            MaDonHang = order.MaDonHang,
                                            MaSP = item.iMaSP,
                                            Soluong = item.iSoluong,
                                            Dongia = (decimal)item.pDonggia
                                        };
                                        data.CHITIETDONHANGs.Add(orderDetail);
                                    }
                                    data.SaveChanges();
                                }
                            }

                            // Xóa giỏ hàng sau khi thanh toán thành công
                            Session["GioHang"] = null;
                            Session["SL"] = 0; // Cập nhật số lượng giỏ hàng về 0

                        }
                        else
                        {
                            // Nếu người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                            return RedirectToAction("Dangnhap", "NguoiDung");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                return View("FailureView");
            }
            return View("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var gioHangList = Session["GioHang"] as List<GioHang>;

            if (gioHangList == null || !gioHangList.Any())
            {
                return null;
            }

            // Tạo danh sách các mục sản phẩm
            var itemList = new ItemList() { items = new List<Item>() };
            double totalAmountUSD = 0; // Tổng tiền bằng USD

            foreach (var item in gioHangList)
            {
                var priceUSD = Math.Round(item.pDonggia / 25300, 2);
                itemList.items.Add(new Item()
                {
                    name = item.pTenSP,
                    currency = "USD",
                    price = priceUSD.ToString("F2"), // Chuyển đổi giá về USD và định dạng chuỗi
                    quantity = item.iSoluong.ToString(),
                    sku = item.iMaSP.ToString()
                });
                totalAmountUSD += priceUSD * item.iSoluong;
            }

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Thêm chi tiết vào đối tượng `Details`
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = totalAmountUSD.ToString("F2")
            };

            // Tổng tiền cuối cùng
            var amount = new Amount()
            {
                currency = "USD",
                total = totalAmountUSD.ToString("F2"),
                details = details
            };

            var transactionList = new List<Transaction>();
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(),
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            var createdPayment = this.payment.Create(apiContext);

            var links = createdPayment.links.GetEnumerator();
            string paypalRedirectUrl = null;
            while (links.MoveNext())
            {
                Links lnk = links.Current;
                if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                {
                    paypalRedirectUrl = lnk.href;
                    break;
                }
            }

            return createdPayment;
        }
        public ActionResult ThanhToanKhiNhanHang(int? maDonHang)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap","NguoiDung");
            }
            // Lấy giỏ hàng từ session
            var gioHangList = Session["GioHang"] as List<GioHang>;
            if (gioHangList == null || !gioHangList.Any())
            {
                // Nếu giỏ hàng trống, chuyển hướng đến trang giỏ hàng
                return RedirectToAction("GioHang", "SanPham");
            }

            try
            {
                using (var data = new DBQLMYPHAMEntities4())
                {
                    // Lấy thông tin khách hàng từ session
                    KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];

                    // Tạo mới đơn đặt hàng
                    var order = new DONDATHANG
                    {
                        Dathanhtoan = false,              // Đánh dấu đã thanh toán
                        Tinhtranggiaohang = 0,       // chờ xác nhận
                        Ngaydat = DateTime.Now,
                        Ngaygiao = null,                 // Sẽ cập nhật sau khi giao hàng
                        MaKH = kh.MaKH
                    };

                    // Lưu đơn đặt hàng vào cơ sở dữ liệu
                    data.DONDATHANGs.Add(order);
                    data.SaveChanges();

                    // Lưu chi tiết từng sản phẩm trong giỏ hàng vào CHITIETDONHANG
                    foreach (var item in gioHangList)
                    {
                        var orderDetail = new CHITIETDONHANG
                        {
                            MaDonHang = order.MaDonHang,
                            MaSP = item.iMaSP,
                            Soluong = item.iSoluong,
                            Dongia = (decimal)item.pDonggia
                        };
                        data.CHITIETDONHANGs.Add(orderDetail);
                    }

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    data.SaveChanges();
                    Session["GioHang"] = null;
                    Session["SL"] = 0; // Cập nhật số lượng giỏ hàng về 0
                    // Xóa giỏ hàng sau khi thanh toán thành công
                    Session["GioHang"] = null;
                }

                // Chuyển hướng đến trang xác nhận đơn hàng
                return RedirectToAction("XacNhanDonHang", new { maDonHang });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và chuyển hướng đến trang lỗi
                return View("Error", new HandleErrorInfo(ex, "Giohang", "FailureView"));
            }
        }
        public ActionResult TheoDoiDonHang()
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }
            int maKH = int.Parse(Session["MaKH"].ToString());

            var donHangList = data.DONDATHANGs
             .Where(d => d.MaKH == maKH)
             .AsEnumerable() // Đưa dữ liệu về bộ nhớ
             .Select(d =>
             {
                 // Tạo đối tượng DonHangContext theo trạng thái đơn hàng
                 var donHangContext = new DonHangContext(d.Tinhtranggiaohang ?? 0)
                 {
                     MaDonHang = d.MaDonHang,
                     Ngaydat = d.Ngaydat,
                     Dathanhtoan = d.Dathanhtoan,
                     Ngaygiao = d.Ngaygiao,
                     SanPhams = d.CHITIETDONHANGs.Select(ct => new SanPhamDonHang
                     {
                         MaSP = ct.MaSP,
                         TenSP = ct.SanPham.TenSP,
                         SoLuong = (int)ct.Soluong,
                         DonGia = (decimal)ct.Dongia
                     }).ToList()
                 };
                 // Đăng ký Observer để nhận thông báo
                 var observer = new KhachHangObserver();
                 donHangContext.Attach(observer);
                 return donHangContext;
             })

             .ToList();
            return View(donHangList);
        }

        [HttpPost]
        public ActionResult HuyDonHang(int maDonHang)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            int maKH = int.Parse(Session["MaKH"].ToString());

            var donHang = data.DONDATHANGs.FirstOrDefault(d => d.MaDonHang == maDonHang && d.MaKH == maKH);

            if (donHang == null)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("TheoDoiDonHang");
            }

            try
            {
                var donHangContext = new DonHangContext(donHang.Tinhtranggiaohang ?? 0);
                donHangContext.HuyDonHang();  // Thực hiện hủy đơn hàng theo State Pattern
                donHang.Tinhtranggiaohang = 4; // Cập nhật trạng thái "Đã hủy" vào database

                data.SaveChanges();
                TempData["Message"] = "Hủy đơn hàng thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = ex.Message;  // Hiển thị lỗi nếu không thể hủy
            }

            return RedirectToAction("TheoDoiDonHang");
        }
    }
}

