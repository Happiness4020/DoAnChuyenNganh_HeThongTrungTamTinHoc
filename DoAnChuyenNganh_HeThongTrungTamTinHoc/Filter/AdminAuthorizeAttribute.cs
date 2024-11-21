using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            string username = httpContext.User.Identity.Name;

            using (var db = new TrungTamTinHocEntities())
            {
                // Lấy thông tin người dùng từ CSDL
                var user = db.TaiKhoan.FirstOrDefault(u => u.TenDangNhap == username);

                if (user != null && user.QuyenHan == "Quản lý") // Chỉ cho phép quyền "Quản lý"
                {
                    return user.QuyenHan == "Quản lý";
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu không được phép, chuyển hướng về trang lỗi hoặc trang đăng nhập
            filterContext.Result = new RedirectResult("~/Account/Error404");
        }
    }

}
