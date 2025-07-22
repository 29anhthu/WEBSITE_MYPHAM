using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public interface IGioHangCommand
    {
        void Execute();
    }
    public class ThemVaoGioHangCommand : IGioHangCommand
    {
        private List<GioHang> _gioHang;
        private GioHang _sanPham;
        private HttpSessionStateBase _session;

        public ThemVaoGioHangCommand(List<GioHang> gioHang, int iMaSP, HttpSessionStateBase session)
        {
            _gioHang = gioHang;
            _session = session;
            _sanPham = _gioHang.Find(n => n.iMaSP == iMaSP) ?? new GioHang(iMaSP);
        }

        public void Execute()
        {
            if (!_gioHang.Contains(_sanPham))
            {
                _gioHang.Add(_sanPham);
            }
            else
            {
                _sanPham.iSoluong++;
            }
            // Cập nhật số lượng sản phẩm trong Session
            _session["SL"] = _gioHang.Sum(n => n.iSoluong);
        }
    }
    public class XoaKhoiGioHangCommand : IGioHangCommand
    {
        private List<GioHang> _gioHang;
        private int _maSP;

        public XoaKhoiGioHangCommand(List<GioHang> gioHang, int iMaSP)
        {
            _gioHang = gioHang;
            _maSP = iMaSP;
        }

        public void Execute()
        {
            var sanPham = _gioHang.Find(n => n.iMaSP == _maSP);
            if (sanPham != null)
            {
                _gioHang.Remove(sanPham);
            }
        }
    }
    public class CapNhatSoLuongCommand : IGioHangCommand
    {
        private List<GioHang> _gioHang;
        private int _maSP;
        private int _soLuongMoi;

        public CapNhatSoLuongCommand(List<GioHang> gioHang, int iMaSP, int soLuongMoi)
        {
            _gioHang = gioHang;
            _maSP = iMaSP;
            _soLuongMoi = soLuongMoi;
        }

        public void Execute()
        {
            var sanPham = _gioHang.Find(n => n.iMaSP == _maSP);
            if (sanPham != null)
            {
                sanPham.iSoluong = _soLuongMoi;
            }
        }
    }
    public class XoaTatCaGioHangCommand : IGioHangCommand
    {
        private List<GioHang> _gioHang;
        private HttpSessionStateBase _session;

        public XoaTatCaGioHangCommand(List<GioHang> gioHang, HttpSessionStateBase session)
        {
            _gioHang = gioHang;
            _session = session;
        }

        public void Execute()
        {
            _gioHang.Clear();
            _session["SL"] = null; // Xóa luôn số lượng trong session
        }
    }

    // thực thi lệnh
    public class GioHangInvoker
    {
        public void ThucThiLenh(IGioHangCommand command)
        {
            command.Execute();
        }
    }

}