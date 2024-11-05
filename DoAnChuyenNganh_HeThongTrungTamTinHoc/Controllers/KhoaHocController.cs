using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class KhoaHocController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: KhoaHoc
        public ActionResult Index(string search = "")
        {
            var khs = db.KhoaHoc;
            List<KhoaHoc> khoahocs = db.KhoaHoc.Where(e => e.TenKH.Contains(search)).ToList();
            ViewBag.Search = search;
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;
            return View(khoahocs);
        }

        public ActionResult ChiTietKhoaHoc(string id)
        {
            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == id).FirstOrDefault();
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;
            return View(kh);
        }
    }
}