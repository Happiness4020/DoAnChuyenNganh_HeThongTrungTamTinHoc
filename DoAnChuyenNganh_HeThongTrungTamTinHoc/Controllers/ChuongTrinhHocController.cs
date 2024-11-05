using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class ChuongTrinhHocController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: ChuongTrinhHoc
        public ActionResult KhoaHocTheoChuongTrinh(string id)
        {
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;
            List<KhoaHoc> khoahocs = db.KhoaHoc.Where(t => t.MaChuongTrinh == id).ToList();
            return View(khoahocs);
        }
    }
}