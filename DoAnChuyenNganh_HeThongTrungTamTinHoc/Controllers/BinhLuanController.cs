using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class BinhLuanController : Controller
    {
        // GET: BinhLuan
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        [HttpPost]
        public ActionResult ThemBinhLuan(string id, string noidung)
        {

            return View();
        }
    }
}