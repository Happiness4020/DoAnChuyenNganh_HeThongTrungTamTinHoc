using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminPhuongThucThanhToanController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        public ActionResult PhuongThucThanhToanList()
        {
            var pt = db.PhuongThucThanhToan.ToList();
            return View(pt);
        }


        
       

    }
}
