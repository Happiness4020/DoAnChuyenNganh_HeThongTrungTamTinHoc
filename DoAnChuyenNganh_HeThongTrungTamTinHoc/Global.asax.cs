using DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Đăng ký CustomControllerFactory
            ControllerBuilder.Current.SetControllerFactory(new CustomControllerFactory());
        }

        //protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        //{
        //    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
        //    {
        //        // Lấy tên từ Authentication Cookie
        //        var identity = HttpContext.Current.User.Identity.Name;

        //        // Tạo thông tin Principal (nếu cần thêm quyền)
        //        var roles = new string[] { "Quản lý" }; // Lấy vai trò từ database nếu cần
        //        HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
        //            new System.Security.Principal.GenericIdentity(identity),
        //            roles
        //        );
        //    }
        //}
    }
}
