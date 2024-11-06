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
        public ActionResult KhoaHocTheoChuongTrinh(string id, int page = 1, string sort_by = "price_asc")
        {
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;
            List<KhoaHoc> khoahocs = db.KhoaHoc.Where(t => t.MaChuongTrinh == id).ToList();

            ViewBag.MaCT = id;

            // Phân trang
            int NumberOfRecordsPerPage = 8;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(khoahocs.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            khoahocs = khoahocs.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();

            // Sắp xếp
            if (sort_by == "price_asc")
            {
                khoahocs = khoahocs.OrderBy(c => c.HocPhi).ToList();
            }
            else if (sort_by == "price_desc")
            {
                khoahocs = khoahocs.OrderByDescending(c => c.HocPhi).ToList();
            }
            ViewBag.SortBy = sort_by;

            return View(khoahocs);
        }
    }
}