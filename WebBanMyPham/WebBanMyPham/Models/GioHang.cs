using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanMyPham.Models
{
    public class GioHang
    {
        DBQLMYPHAMEntities4 data = new DBQLMYPHAMEntities4();
        public int iMaSP { set; get; }
        public string pTenSP { set; get; }
        public string pAnhbia { set; get; }
        public Double pDonggia { set; get; }
        public int iSoluong { set; get; }
        public Double dThanhtien
        {
            get { return iSoluong * pDonggia; }
        }
        public GioHang(int MaSP)
        {
            iMaSP = MaSP;
            SanPham sp = data.SanPhams.Single(n => n.MaSP == iMaSP);
            pTenSP = sp.TenSP;
            pAnhbia = sp.Anhbia;
            pDonggia = double.Parse(sp.Giaban.ToString());
            iSoluong = 1;
        }
    }
}
