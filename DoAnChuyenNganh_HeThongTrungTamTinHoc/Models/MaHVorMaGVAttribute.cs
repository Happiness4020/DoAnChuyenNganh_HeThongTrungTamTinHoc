using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Models
{
    public class MaHVorMaGVAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Lấy các giá trị của các trường MaHV và MaGV từ đối tượng model
            var taiKhoan = (TaiKhoan)validationContext.ObjectInstance;
            bool hasMaHV = !string.IsNullOrEmpty(taiKhoan.MaHV);
            bool hasMaGV = !string.IsNullOrEmpty(taiKhoan.MaGV);

            // Kiểm tra điều kiện chỉ một trong hai mã được chọn
            if (hasMaHV && hasMaGV)
            {
                return new ValidationResult("Chỉ được chọn một trong hai: Mã học viên hoặc Mã giáo viên.");
            }
            if (!hasMaHV && !hasMaGV)
            {
                return new ValidationResult("Bạn phải chọn Mã học viên hoặc Mã giáo viên.");
            }

            return ValidationResult.Success;
        }
    }
}