using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLichHocController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string malh = Utility.TaoMaNgauNhien("LH", 3);

        // GET: Admin/AdminLichHoc
        public ActionResult LichHocList(string search = "", int page = 1, int pageSize = 10, string sortOrder = "ngayhoc")
        {
            List<LichHoc> lichHocs = db.LichHoc
                .Where(lh => lh.HocVien.HoTen.Contains(search) || lh.LopHoc.MaLH.Contains(search))
                .ToList();

            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "tenhocvien":
                    lichHocs = lichHocs.OrderBy(lh => lh.HocVien.HoTen).ToList();
                    break;
                case "malophoc":
                    lichHocs = lichHocs.OrderBy(lh => lh.LopHoc.MaLH).ToList();
                    break;
                case "ngayhoc":
                default:
                    lichHocs = lichHocs.OrderBy(lh => lh.NgayHoc).ToList();
                    break;
            }

            int totalRecords = lichHocs.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            lichHocs = lichHocs.Skip(recordsToSkip).Take(pageSize).ToList();

            return View(lichHocs);
        }

        public ActionResult LichHocAdd()
        {
            ViewBag.MaLichHoc = malh;
            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen");
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenPhong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LichHocAdd(LichHoc lichHoc)
        {
            var existingLichHoc = db.LichHoc
                .FirstOrDefault(l => l.MaHV == lichHoc.MaHV && l.MaLH == lichHoc.MaLH);

            if (existingLichHoc != null)
            {
                ModelState.AddModelError("", "Lịch học của học viên này cho lớp này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                db.LichHoc.Add(lichHoc);
                db.SaveChanges();
                return RedirectToAction("LichHocList");
            }

            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenPhong", lichHoc.MaLH);
            return View(lichHoc);
        }

        public ActionResult LichHocEdit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LichHoc lichHoc = db.LichHoc.FirstOrDefault(ld => ld.MaLichHoc == id);

            if (lichHoc == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenPhong", lichHoc.MaLH);
            return View(lichHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LichHocEdit(LichHoc lichHoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichHoc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("LichHocList");
            }

            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenPhong", lichHoc.MaLH);
            return View(lichHoc);
        }

        public ActionResult LichHocDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LichHoc lichHoc = db.LichHoc.FirstOrDefault(ld => ld.MaLichHoc == id);

            if (lichHoc == null)
            {
                return HttpNotFound();
            }

            return View(lichHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LichHocDelete(string id, LichHoc lichHoc)
        {
            try
            {
                lichHoc = db.LichHoc.FirstOrDefault(ld => ld.MaLichHoc == id);
                db.LichHoc.Remove(lichHoc);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa lịch học thành công";
                return RedirectToAction("LichHocList");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lịch học: " + ex;
                return RedirectToAction("LichHocList");
            }
        }
    }
}
