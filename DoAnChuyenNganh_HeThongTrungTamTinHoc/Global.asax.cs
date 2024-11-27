using DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Application["AppRestartTime"] = DateTime.Now;

        }

        protected void Application_BeginRequest()
        {
            if (HttpContext.Current.Request.Cookies["ThoiGianDangNhap"] != null)
            {
                DateTime appRestartTime = (DateTime)Application["AppRestartTime"];

                DateTime loginTime;
                if (DateTime.TryParse(HttpContext.Current.Request.Cookies["ThoiGianDangNhap"].Value, out loginTime))
                {
                    if (loginTime < appRestartTime)
                    {
                        FormsAuthentication.SignOut();
                        HttpContext.Current.Response.Cookies["NguoiDung"].Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Current.Response.Cookies["QuyenHan"].Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Current.Response.Cookies["ThoiGianDangNhap"].Expires = DateTime.Now.AddDays(-1);

                        HttpContext.Current.Response.Redirect("~/Home");
                    }
                }
            }
        }
    }
}
