using AliEns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Utility
{
    public static class SD
    {
        // Constant Roles Names
        public const string Admin = "Admin";
        public const string ManagerUser = "Manager";
        public const string KitchenUser = "Kitchen";
        public const string FrontDeskUser = "Front Desk";
        public const string EndCustomerUser = "Customer";

        // Session Names
        public const string ShoppingCartCount = "ShoppingCartCount";
        public const string ssCouponCode = "CouponCode";

        // Order Status
        public const string StatusSubmitted = "Submitted";
        public const string StatusInProcess = "Begin Prepared";
        public const string StatusReady = "Ready For Pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";

        // Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";

        // Return String Without Html Tages
        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];

                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        // Return Order Total After Discount
        public static decimal DiscountPrice(Coupon coupon, decimal orderOrginalTotal)
        {
            if (coupon == null)
            {
                return Math.Round(orderOrginalTotal, 2);
            }

            if (coupon.MinimumAmount > orderOrginalTotal)
            {
                return Math.Round(orderOrginalTotal, 2);
            }

            if (int.Parse(coupon.CouponType) == (int)Coupon.ECouponType.Doller)
            {
                return Math.Round(orderOrginalTotal - coupon.Discount, 2);
            }

            return Math.Round(orderOrginalTotal - (orderOrginalTotal * (coupon.Discount / 100)), 2);
        }
    }
}
