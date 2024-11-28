using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;

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

            int NumberOfRecordsPerPage = 8;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(khoahocs.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            khoahocs = khoahocs.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();

            return View(khoahocs);
        }

        public ActionResult ChiTietKhoaHoc(string id, int page = 1, string sort_by = "datetime_asc")
        {
            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == id).FirstOrDefault();

            var binhluans = db.BinhLuanKhoaHoc
                     .Where(bl => bl.MaKH == id)
                     .OrderByDescending(bl => bl.NgayBinhLuan)
                     .ToList();

            var tatcabinhluan = db.BinhLuanKhoaHoc
                    .Where(bl => bl.MaKH == id)
                    .OrderByDescending(bl => bl.NgayBinhLuan)
                    .ToList();

            ViewBag.TatCaBinhLuan = tatcabinhluan; 

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

            ViewBag.KhoaHocNoiBats = KhoaHocNoiBat();

            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemBinhLuan(string MaKH, string NoiDung)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                ViewBag.MAHV = mahv;

                if (string.IsNullOrEmpty(mahv))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để bình luận!!!";
                }
                if (string.IsNullOrEmpty(NoiDung))
                {
                    TempData["ErrorMessage"] = "Bạn chưa nhập nội dung bình luận!!!";
                }

                DateTime truncatedToSecond = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                BinhLuanKhoaHoc binhluan = new BinhLuanKhoaHoc
                {
                    MaHV = mahv,
                    MaKH = MaKH,
                    NoiDung = NoiDung,
                    NgayBinhLuan = truncatedToSecond
                };

                db.BinhLuanKhoaHoc.Attach(binhluan);
                db.BinhLuanKhoaHoc.Add(binhluan);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng bình luận thành công";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng bình luận!!! Hãy thử lại sau";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaBinhLuan(DateTime NgayBinhLuan, string MaKH)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                var binhluan = db.BinhLuanKhoaHoc
                        .Where(b => b.MaHV == mahv && b.MaKH == MaKH)
                        .ToList()
                        .FirstOrDefault(b => b.NgayBinhLuan.Date == NgayBinhLuan.Date &&
                                             b.NgayBinhLuan.Hour == NgayBinhLuan.Hour &&
                                             b.NgayBinhLuan.Minute == NgayBinhLuan.Minute &&
                                             b.NgayBinhLuan.Second == NgayBinhLuan.Second);

                if (binhluan != null)
                {
                    db.BinhLuanKhoaHoc.Remove(binhluan);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa bình luận thành công";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }
                else
                {
                    TempData["ErrorMessage"] = "Bình luận không tồn tại hoặc có thể đã bị xóa!!!";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bình luận!!!. Hãy thử lại sau";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        [HttpPost]
        public ActionResult SuaBinhLuan(string MaKH, string NoiDung)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                var binhluan = db.BinhLuanKhoaHoc.FirstOrDefault(b => b.MaHV == mahv);
                if (binhluan != null)
                {
                    binhluan.NoiDung = NoiDung;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Chỉnh sửa bình luận thành công!!!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bình luận không tồn tại!!!";
                }
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chỉnh sửa bình luận! Hãy thử lại sau.";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        public List<KhoaHocNoiBatViewModel> KhoaHocNoiBat()
        {
            List<KhoaHocNoiBatViewModel> khoahocs;
            try
            {
                var khoahocnoibat = db.GiaoDichHocPhi
                .GroupBy(gd => gd.MaKH)
                .Select(group => new
                {
                    MaKH = group.Key,
                    SoLanDangKy = group.Count()
                })
                .OrderByDescending(x => x.SoLanDangKy)
                .Take(10)
                .ToList();

                khoahocs = khoahocnoibat
                    .Join(db.KhoaHoc,
                        gd => gd.MaKH,
                        kh => kh.MaKH,
                        (gd, kh) => new KhoaHocNoiBatViewModel
                        {
                            MaKH = kh.MaKH,
                            TenKH = kh.TenKH,
                            Anh = kh.Anh,
                            NgayBatDau = kh.NgayBatDau.Date,
                            HocPhi = kh.HocPhi,
                            SoLanDangKy = gd.SoLanDangKy
                        })
                    .ToList();

                return khoahocs;
            }
            catch
            {
                return new List<KhoaHocNoiBatViewModel>();
            }
        }
    }
}