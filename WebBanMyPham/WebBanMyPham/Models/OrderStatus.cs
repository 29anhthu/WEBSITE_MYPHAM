using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanMyPham.Models.Enums
{
    public enum OrderStatus
    {
        ChoXacNhan = 0,  // Đơn hàng chờ xác nhận
        DaXacNhan = 1,   // Đơn hàng đã xác nhận
        DangGiao = 2,    // Đơn hàng đang giao
        DaGiao = 3,      // Đơn hàng đã giao
        DaHuy = 4        // Đơn hàng đã hủy
    }
}
