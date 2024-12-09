using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLichDayController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string maLichDay = Utility.TaoMaNgauNhien("LD", 5);

        public ActionResult LichDayList(string search = "", int page = 1, int pageSize = 10, string sortOrder = "")
        {
            List<LichDay> lichDays = db.LichDay
                .Where(ld => ld.GiaoVien.HoTen.Contains(search) || ld.LopHoc.MaLH.Contains(search))
                .ToList();

            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "magv":
                    lichDays = lichDays.OrderBy(kh => kh.MaGV).ToList();
                    break;
                case "tengv":
                    lichDays = lichDays.OrderBy(kh => kh.GiaoVien.HoTen).ToList();
                    break;
                default:
                    lichDays = lichDays.OrderBy(kh => kh.NgayDay).ToList();
                    break;
            }

            int totalRecords = lichDays.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            lichDays = lichDays.Skip(recordsToSkip).Take(pageSize).ToList();

            return View(lichDays);
        }

        public ActionResult LichDayAdd()
        {
            ViewBag.MaLichDay = maLichDay;
            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenPhong");
            return View();
        }

        [HttpPost]
        public ActionResult LichDayAdd(LichDay lichDay)
        {
            if (ModelState.IsValid)
            {
                var existingLichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == lichDay.MaLichDay);
                if (existingLichDay != null)
                {
                    ModelState.AddModelError("MaLichDay", "Mã lịch dạy đã tồn tại!");
                    return View();
                }

                lichDay.MaLichDay = maLichDay;
                db.LichDay.Add(lichDay);
                db.SaveChanges();
                return RedirectToAction("LichDayList");
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenPhong");
            return View();
        }

        public ActionResult LichDayDelete(string id)
        {
            LichDay lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
            return View(lichDay);
        }

        [HttpPost]
        public ActionResult LichDayDeleteConfirmed(string id)
        {
            try
            {
                var lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
                if (lichDay != null)
                {
                    db.LichDay.Remove(lichDay);
                    db.SaveChanges();
                }
                TempData["SuccessMessage"] = "Đã xóa lịch dạy thành công";
                return RedirectToAction("LichDayList");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa lịch dạy: " + ex;
                return RedirectToAction("LichDayList");
            }
        }

        public ActionResult LichDayEdit(string id)
        {
            var lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
            if (lichDay == null)
            {
                return HttpNotFound();
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lichDay.MaGV);
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenPhong", lichDay.MaLH);
            return View(lichDay);
        }

        [HttpPost]
        public ActionResult LichDayEdit(LichDay lichDay)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichDay).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("LichDayList");
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lichDay.MaGV);
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenPhong", lichDay.MaLH);
            return View(lichDay);
        }

        [HttpPost]
        public ActionResult PhanLichDay()
        {
            try
            {
                var lichhocs = db.LichHoc.ToList();

                if (lichhocs != null)
                {
                    foreach (var lich in lichhocs)
                    {
                        string maLD = Utility.TaoMaNgauNhien("LD", 5);

                        bool lichDayTonTai = db.LichDay.Any(ld =>
                            ld.MaLH == lich.MaLH &&
                            ld.NgayDay == lich.NgayHoc.Date &&
                            ld.GioBatDau == lich.GioBatDau &&
                            ld.GioKetThuc == lich.GioKetThuc);

                        if (!lichDayTonTai)
                        {
                            var ttLopHoc = db.LopHoc.FirstOrDefault(lh => lh.MaLH == lich.MaLH);

                            if (ttLopHoc != null)
                            {
                                var lichday = new LichDay
                                {
                                    MaLichDay = maLD,
                                    MaGV = ttLopHoc.MaGV,
                                    MaLH = lich.MaLH,
                                    NgayDay = lich.NgayHoc.Date,
                                    GioBatDau = lich.GioBatDau,
                                    GioKetThuc = lich.GioKetThuc
                                };
                                db.LichDay.Add(lichday);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return RedirectToAction("LichDayList");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationError in ex.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                    }
                }
                return RedirectToAction("LichDayList");
            }
        }
    }
}
