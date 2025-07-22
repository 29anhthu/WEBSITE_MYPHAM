using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public sealed class VoucherService
    {
        private static VoucherService _instance;
        private static readonly object _lock = new object();
        private VoucherService() { }
        public static VoucherService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new VoucherService();
                    }
                    return _instance;
                }
            }
        }
        public bool KiemTraNgayHopLe(DateTime ngayBatDau, DateTime ngayHetHan)
        {
            return ngayBatDau <= ngayHetHan;
        }
        public bool KiemTraSoLuong(int soLuong)
        {
            return soLuong >= 0;
        }
    }
    public class VoucherRepository
    {
        private readonly DBQLMYPHAMEntities4 _db;

        public VoucherRepository()
        {
            _db = new DBQLMYPHAMEntities4();
        }

        public void ThemVoucher(VOUCHER model)
        {
            _db.VOUCHERs.Add(model);
            _db.SaveChanges();
        }

        public void SuaVoucher(VOUCHER model)
        {
            var voucher = _db.VOUCHERs.Find(model.MaVoucher);
            if (voucher != null)
            {
                voucher.Code = model.Code;
                voucher.GiamGia = model.GiamGia;
                voucher.LoaiGiamGia = model.LoaiGiamGia;
                voucher.SoLuong = model.SoLuong;
                voucher.NgayBatDau = model.NgayBatDau;
                voucher.NgayHetHan = model.NgayHetHan;

                _db.SaveChanges();
            }
        }

        public void XoaVoucher(int id)
        {
            var voucher = _db.VOUCHERs.Find(id);
            if (voucher != null)
            {
                _db.VOUCHERs.Remove(voucher);
                _db.SaveChanges();
            }
        }
    }

}