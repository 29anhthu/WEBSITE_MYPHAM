using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanMyPham.Service
{
    public interface IVoucherStrategy
    {
        decimal ApplyDiscount(decimal totalAmount, decimal discountValue);
    }
    public class FixedAmountDiscount : IVoucherStrategy
    {
        public decimal ApplyDiscount(decimal totalAmount, decimal discountValue)
        {
            return Math.Min(discountValue, totalAmount);
        }
    }
    public class PercentageDiscount : IVoucherStrategy
    {
        public decimal ApplyDiscount(decimal totalAmount, decimal discountValue)
        {
            return Math.Min(totalAmount * (discountValue / 100), totalAmount);
        }
    }
    public static class VoucherStrategyFactory
    {
        public static IVoucherStrategy GetStrategy(bool isPercentageDiscount)
        {
            if (isPercentageDiscount)
            {
                return new PercentageDiscount();
            }
            return new FixedAmountDiscount();
        }
    }
}