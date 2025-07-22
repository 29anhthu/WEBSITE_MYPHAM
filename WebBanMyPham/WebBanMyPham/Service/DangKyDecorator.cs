using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public interface IDangKyKhachHang
    {
        void DangKy(KHACHHANG kh);
    }

    public class DangKyKhachHang : IDangKyKhachHang
    {
        private readonly DBQLMYPHAMEntities4 _db;

        public DangKyKhachHang(DBQLMYPHAMEntities4 db)
        {
            _db = db;
        }

        public void DangKy(KHACHHANG kh)
        {
            _db.KHACHHANGs.Add(kh);
            _db.SaveChanges();
        }
    }

    public class ThongBaoDecorator : IDangKyKhachHang
    {
        private readonly IDangKyKhachHang _dangKyKhachHang;

        public ThongBaoDecorator(IDangKyKhachHang dangKyKhachHang)
        {
            _dangKyKhachHang = dangKyKhachHang;
        }

        public void DangKy(KHACHHANG kh)
        {
            _dangKyKhachHang.DangKy(kh);
            GuiThongBao(kh);
        }

        private void GuiThongBao(KHACHHANG kh)
        {
            // Gửi thông báo đến email hoặc số điện thoại
            Console.WriteLine($"Xin chào {kh.Taikhoan}, bạn đã đăng ký thành công! Chào mừng bạn đến với Mỹ Phẩm LANT");
        }
    }


}
