using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public interface ISanPhamBuilder
    {
        ISanPhamBuilder SetTenSP(string tenSP);
        ISanPhamBuilder SetGiaBan(decimal? giaBan);
        ISanPhamBuilder SetMoTa(string moTa);
        ISanPhamBuilder SetNgayCapNhat(DateTime ngayCapNhat);
        ISanPhamBuilder SetSoLuongTon(int? soLuongTon);
        ISanPhamBuilder SetMaLoai(int? maLoai);
        ISanPhamBuilder SetHinhAnh(HttpServerUtilityBase server, HttpPostedFileBase fileupload, string currentImage);
        SanPham Build();
    }
    public class SanPhamBuilder : ISanPhamBuilder
    {
        private SanPham _sanPham = new SanPham();
        // Thêm constructor nhận đối tượng SanPham
        public SanPhamBuilder(SanPham sanPham)
        {
            _sanPham = sanPham ?? new SanPham(); // Nếu null thì khởi tạo mới
        }
        public ISanPhamBuilder SetTenSP(string tenSP)
        {
            _sanPham.TenSP = tenSP;
            return this;
        }

        public ISanPhamBuilder SetGiaBan(decimal? giaBan)
        {
            _sanPham.Giaban = giaBan ?? 0m; // Nếu null thì gán giá trị mặc định là 0
            return this;
        }
        public ISanPhamBuilder SetMoTa(string moTa)
        {
            _sanPham.Mota = moTa;
            return this;
        }

        public ISanPhamBuilder SetNgayCapNhat(DateTime ngayCapNhat)
        {
            _sanPham.Ngaycapnhat = ngayCapNhat;
            return this;
        }

        public ISanPhamBuilder SetSoLuongTon(int? soLuongTon)
        {
            _sanPham.Soluongton = soLuongTon ?? 0; // Nếu null thì gán 0
            return this;
        }

        public ISanPhamBuilder SetMaLoai(int? maLoai)
        {
            _sanPham.MaLoai = maLoai;
            return this;
        }

        public ISanPhamBuilder SetHinhAnh(HttpServerUtilityBase server, HttpPostedFileBase fileupload, string currentImage)
        {
            if (fileupload != null && fileupload.ContentLength > 0)
            {
                var fileName = Path.GetFileName(fileupload.FileName);
                var path = Path.Combine(server.MapPath("~/Upload/image_SP/"), fileName);

                if (!System.IO.File.Exists(path))
                {
                    fileupload.SaveAs(path);
                    _sanPham.Anhbia = fileName;
                }
                else
                {
                    throw new Exception("Hình ảnh đã tồn tại! Vui lòng đặt tên khác.");
                }
            }
            else
            {
                _sanPham.Anhbia = currentImage; // Giữ lại ảnh cũ nếu không có file mới
            }
            return this;
        }

        public SanPham Build()
        {
            return _sanPham;
        }
    }

}

