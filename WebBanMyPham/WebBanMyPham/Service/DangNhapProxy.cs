using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanMyPham.Models;

namespace WebBanMyPham.Service
{
    public interface IAuthentication
    {
        object Login(string username, string password);
    }
    public class ProxyAuthentication : IAuthentication
    {
        private readonly RealAuthentication _realAuth = new RealAuthentication();

        public object Login(string username, string password)
        {
            return _realAuth.Login(username, password);
        }
    }
    public class RealAuthentication : IAuthentication
    {
        private DBQLMYPHAMEntities4 db = new DBQLMYPHAMEntities4();

        public object Login(string username, string password)
        {
            var kh = db.KHACHHANGs.SingleOrDefault(n => n.Taikhoan == username && n.Matkhau == password);
            var ad = db.Admins.SingleOrDefault(a => a.UserAdmin == username && a.PassAdmin == password);
            return kh ?? (object)ad;
        }
    }

}