using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    [AllowAnonymous]
    public class KhoaHocController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: KhoaHoc
        public ActionResult Index(string search = "", int page = 1, string sort_by = "price_asc")
        {
            var khs = db.KhoaHoc;
            List<KhoaHoc> khoahocs = db.KhoaHoc.Where(e => e.TenKH.Contains(search)).ToList();
            ViewBag.Search = search;
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

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

            // Phân trang
            int NumberOfRecordsPerPage = 8;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(khoahocs.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            khoahocs = khoahocs.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();

            return View(khoahocs);
        }

        public ActionResult ChiTietKhoaHoc(string id, int page = 1, string sort_by = "time_asc")
        {
            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == id).FirstOrDefault();

            var binhluans = db.BinhLuanKhoaHoc
                     .Where(bl => bl.MaKH == id)
                     .ToList();

            // Sắp xếp
            if (sort_by == "datetime_asc")
            {
                binhluans = binhluans.OrderBy(c => c.NgayBinhLuan).ToList();
            }
            else if (sort_by == "datetime_desc")
            {
                binhluans = binhluans.OrderByDescending(c => c.NgayBinhLuan).ToList();
            }
            ViewBag.SortBy = sort_by;

            int tongSoBinhLuan = binhluans.Count;
            ViewBag.TongSoBinhLuan = tongSoBinhLuan;

            int NumberOfRecordsPerPage = 3;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(binhluans.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            binhluans = binhluans.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();   

            ViewBag.BinhLuans = binhluans;

            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            return View(kh);
        }

        [HttpPost]
        public ActionResult ThemBinhLuan(string MaKH, string NoiDung)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                ViewBag.MAHV = mahv;

                if (string.IsNullOrEmpty(mahv))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm bình luận.";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }

                BinhLuanKhoaHoc binhluan = new BinhLuanKhoaHoc
                {
                    MaKH = MaKH,
                    MaHV = mahv,
                    NoiDung = NoiDung,
                    NgayBinhLuan = DateTime.Now
                };

                db.BinhLuanKhoaHoc.Add(binhluan);
                db.SaveChanges();

                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }
        [HttpPost]
        public ActionResult XoaBinhLuan(string MaKH)
        {
            try
            {
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

    }
}