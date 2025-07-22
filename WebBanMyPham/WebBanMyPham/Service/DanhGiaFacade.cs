using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
	public class DanhGiaFacade
	{
        private readonly DBQLMYPHAMEntities4 db;

        public DanhGiaFacade()
        {
            db = new DBQLMYPHAMEntities4();
        }
        public bool KiemTraDaMua(int maKH, int maSP)
        {
            return db.DONDATHANGs.Any(dh =>
                dh.MaKH == maKH &&
                dh.CHITIETDONHANGs.Any(ct => ct.MaSP == maSP) &&
                dh.Tinhtranggiaohang == 3 
            );
        }
        public bool ThemDanhGia(int maKH, int maSP, int soSao, string noiDung)
        {
            if (!KiemTraDaMua(maKH, maSP))
            {
                return false; 
            }

            var danhGia = new DanhGia
            {
                MaKH = maKH,
                MaSP = maSP,
                SoSao = soSao,
                NoiDung = noiDung,
                NgayDanhGia = DateTime.Now
            };

            db.DanhGias.Add(danhGia);
            db.SaveChanges();
            return true;
        }

        // Lấy danh sách đánh giá của sản phẩm
        public IQueryable<DanhGia> LayDanhSachDanhGia(int maSP)
        {
            return db.DanhGias.Where(d => d.MaSP == maSP);
        }
    }
}