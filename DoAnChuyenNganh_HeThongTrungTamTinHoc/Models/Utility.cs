using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Models
{
    public class Utility
    {
        private static Random random = new Random();
        public static string TaoMaNgauNhien(string prefix, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder(prefix);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }

    }
}