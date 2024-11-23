using DoAnChuyenNganh_HeThongTrungTamTinHoc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class CustomControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                return null;
            }

            if (controllerType == typeof(AdminLichDayController))
            {
                var lichDayService = new LichDayService();
                return (IController)Activator.CreateInstance(controllerType, lichDayService);
            }

            return base.GetControllerInstance(requestContext, controllerType);
        }
    }
}