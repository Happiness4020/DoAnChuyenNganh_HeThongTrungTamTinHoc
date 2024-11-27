using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Services;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLichDayController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string maLichDay = Utility.TaoMaNgauNhien("LD", 5);

        // Danh sách Lịch dạy
        public ActionResult LichDayList(string search = "", int page = 1, int pageSize = 10, string sortOrder = "")
        {
            List<LichDay> lichDays = db.LichDay
                .Where(ld => ld.GiaoVien.HoTen.Contains(search) || ld.LopHoc.MaLH.Contains(search))
                .ToList();

            ViewBag.Search = search;

            int NoOfRecordPerPage = 7;
            int NoOfPage = (int)Math.Ceiling((double)lichDays.Count / NoOfRecordPerPage);
            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;

            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPage;
            lichDays = lichDays.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();

            switch (sortOrder)
            {
                case "magiaovien":
                    lichDays = lichDays.OrderBy(e => e.MaGV).ToList();
                    break;
                case "malophoc":
                    lichDays = lichDays.OrderBy(e => e.MaLH).ToList();
                    break;
            }

            return View(lichDays);
        }

        // Thêm mới Lịch dạy
        public ActionResult LichDayAdd()
        {
            ViewBag.MaLichDay = maLichDay;
            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop");
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
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop");
            return View();
        }

        // Xóa Lịch dạy
        public ActionResult LichDayDelete(string id)
        {
            LichDay lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
            return View(lichDay);
        }

        [HttpPost]
        public ActionResult LichDayDeleteConfirmed(string id)
        {
            var lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
            if (lichDay != null)
            {
                db.LichDay.Remove(lichDay);
                db.SaveChanges();
            }
            return RedirectToAction("LichDayList");
        }

        // Chỉnh sửa Lịch dạy
        public ActionResult LichDayEdit(string id)
        {
            var lichDay = db.LichDay.FirstOrDefault(ld => ld.MaLichDay == id);
            if (lichDay == null)
            {
                return HttpNotFound();
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lichDay.MaGV);
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichDay.MaLH);
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
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichDay.MaLH);
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
