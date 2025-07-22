using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public interface ITrangThaiDonHang
    {
        void CapNhatTrangThai(DonHangContext context);
        void HuyDonHang(DonHangContext context);
        string HienThiTrangThai();
    }
    public interface IDonHangObserver
    {
        void CapNhatThongBao(DonHangContext donHang);
    }
    public class ChoXacNhanState : ITrangThaiDonHang
    {
        public void CapNhatTrangThai(DonHangContext context)
        {
            context.SetTrangThai(new DaXacNhanState()); 
        }
        public void HuyDonHang(DonHangContext context)
        {
            context.SetTrangThai(new HuyDonState());
        }

        public string HienThiTrangThai() => "Chờ xác nhận";
    }
    public class DaXacNhanState : ITrangThaiDonHang
    {
        public void CapNhatTrangThai(DonHangContext context)
        {
            context.SetTrangThai(new DangGiaoState()); 
        }
        public void HuyDonHang(DonHangContext context)
        {
            throw new InvalidOperationException("Đơn hàng đã xác nhận, không thể hủy!");
        }
        public string HienThiTrangThai() => "Đã xác nhận";
    }

    public class DangGiaoState : ITrangThaiDonHang
    {
        public void CapNhatTrangThai(DonHangContext context)
        {
            context.SetTrangThai(new DaGiaoState()); 
            context.Ngaygiao = DateTime.Now; 
        }
        public void HuyDonHang(DonHangContext context)
        {
            throw new InvalidOperationException("Đơn hàng đang giao, không thể hủy!");
        }
        public string HienThiTrangThai() => "Đang giao hàng";
    }

    public class DaGiaoState : ITrangThaiDonHang
    {
        public void CapNhatTrangThai(DonHangContext context)
        {
        }
        public void HuyDonHang(DonHangContext context)
        {
            throw new InvalidOperationException("Đơn hàng đã giao, không thể hủy!");
        }
        public string HienThiTrangThai() => "Đã giao hàng";
    }

    public class HuyDonState : ITrangThaiDonHang
    {
        public void CapNhatTrangThai(DonHangContext context)
        {
        }
        public void HuyDonHang(DonHangContext context)
        {
            throw new InvalidOperationException("Đơn hàng đã bị hủy!");
        }
        public string HienThiTrangThai() => "Đơn hàng bị hủy";
    }
    public class DonHangContext
    {
        private ITrangThaiDonHang _trangThaiDonHang;
        private List<IDonHangObserver> _observers = new List<IDonHangObserver>();
        public int MaDonHang { get; set; }
        public DateTime? Ngaydat { get; set; }
        public bool? Dathanhtoan { get; set; }
        public DateTime? Ngaygiao { get; set; }
        public List<SanPhamDonHang> SanPhams { get; set; }
        public void Attach(IDonHangObserver observer)
        {
            _observers.Add(observer);
        }
        public void Detach(IDonHangObserver observer)
        {
            _observers.Remove(observer);
        }
        private void ThongBaoObserver()
        {
            foreach (var observer in _observers)
            {
                observer.CapNhatThongBao(this);
            }
        }
        public void SetTrangThai(ITrangThaiDonHang trangThai)
        {
            _trangThaiDonHang = trangThai;
            ThongBaoObserver(); 
        }
        public DonHangContext(int tinhtranggiaohang)
        {
            SetTrangThai(tinhtranggiaohang);
        }
        public void SetTrangThai(int tinhtranggiaohang)
        {
            switch (tinhtranggiaohang)
            {
                case 0:
                    _trangThaiDonHang = new ChoXacNhanState();  
                    break;
                case 1:
                    _trangThaiDonHang = new DaXacNhanState();  
                    break;
                case 2:
                    _trangThaiDonHang = new DangGiaoState();   
                    break;
                case 3:
                    _trangThaiDonHang = new DaGiaoState();     
                    break;
                case 4:
                    _trangThaiDonHang = new HuyDonState();     
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tinhtranggiaohang), "Trạng thái không hợp lệ!");
            }
        }
        public void HuyDonHang()
        {
            _trangThaiDonHang.HuyDonHang(this);
        }
        public string TrangThaiHienThi => _trangThaiDonHang.HienThiTrangThai();

        public void CapNhatTrangThai()
        {
            _trangThaiDonHang.CapNhatTrangThai(this);
        }
        public int GetTrangThaiValue()
        {
            if (_trangThaiDonHang is ChoXacNhanState) return 0;
            if (_trangThaiDonHang is DaXacNhanState) return 1;
            if (_trangThaiDonHang is DangGiaoState) return 2;
            if (_trangThaiDonHang is DaGiaoState) return 3;
            if (_trangThaiDonHang is HuyDonState) return 4;

            throw new InvalidOperationException("Trạng thái không hợp lệ!");
        }
        public class KhachHangObserver : IDonHangObserver
        {
            public void CapNhatThongBao(DonHangContext donHang)
            {
                if (donHang.TrangThaiHienThi == "Đã giao hàng")
                {
                    HttpContext.Current.Session["ThongBao"] = $"Đơn hàng của bạn đã được giao!";
                }
            }
        }
    }

}