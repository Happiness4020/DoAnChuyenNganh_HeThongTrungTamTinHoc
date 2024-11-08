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

        public ActionResult ChiTietKhoaHoc(string id)
        {
            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == id).FirstOrDefault();

            var binhluans = db.BinhLuanKhoaHoc
                     .Where(bl => bl.MaKH == id)
                     .OrderByDescending(bl => bl.NgayBinhLuan) // Sắp xếp theo ngày bình luận
                     .ToList();            

            ViewBag.BinhLuans = binhluans;

            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            return View(kh);
        }

        public string LayMaHV()
        {
            string tenDangNhap = Session["TenDangNhap"]?.ToString();
            var taiKhoan = db.TaiKhoan.SingleOrDefault(tk => tk.TenDangNhap == tenDangNhap);
            return taiKhoan?.MaHV;
        }

        [HttpPost]
        public ActionResult ThemBinhLuan(string MaKH, string NoiDung)
        {


            ViewBag.MAHV = LayMaHV();

            if (string.IsNullOrEmpty(LayMaHV()))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm bình luận.";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }

            BinhLuanKhoaHoc binhluan = new BinhLuanKhoaHoc
            {
                MaKH = MaKH,
                MaHV = LayMaHV(),
                NoiDung = NoiDung,
                NgayBinhLuan = DateTime.Now
            };

            db.BinhLuanKhoaHoc.Add(binhluan);
            db.SaveChanges();

            return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
        }
    }
}