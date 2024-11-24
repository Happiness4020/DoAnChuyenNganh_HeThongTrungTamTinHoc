using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Services;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLichDayController : Controller
    {
        // Kết nối đến cơ sở dữ liệu
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string maLichDay = Utility.TaoMaNgauNhien("LD", 5);

        // Danh sách Lịch dạy
        public async Task<ActionResult> LichDayList(string search = "")
        {
            List<LichDay> lichDays = await db.LichDay
                .Where(ld => ld.GiaoVien.HoTen.Contains(search) || ld.LopHoc.MaLH.Contains(search))
                .ToListAsync();

            ViewBag.Search = search;
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
        public async Task<ActionResult> LichDayAdd(LichDay lichDay)
        {
            if (ModelState.IsValid)
            {
                var existingLichDay = await db.LichDay.FirstOrDefaultAsync(ld => ld.MaLichDay == lichDay.MaLichDay);
                if (existingLichDay != null)
                {
                    ModelState.AddModelError("MaLichDay", "Mã lịch dạy đã tồn tại!");
                    return View();
                }

                lichDay.MaLichDay = maLichDay;
                db.LichDay.Add(lichDay);
                await db.SaveChangesAsync();
                return RedirectToAction("LichDayList");
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop");
            return View();
        }


        // Xóa Lịch dạy
        public async Task<ActionResult> LichDayDelete(string id)
        {
            LichDay lichDay = await db.LichDay.FirstOrDefaultAsync(ld => ld.MaLichDay == id);
            return View(lichDay);
        }

        [HttpPost]
        public async Task<ActionResult> LichDayDeleteConfirmed(string id)
        {
            var lichDay = await db.LichDay.FirstOrDefaultAsync(ld => ld.MaLichDay == id);
            if (lichDay != null)
            {
                db.LichDay.Remove(lichDay);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("LichDayList");
        }

        // Chỉnh sửa Lịch dạy
        public async Task<ActionResult> LichDayEdit(string id)
        {
            var lichDay = await db.LichDay.FirstOrDefaultAsync(ld => ld.MaLichDay == id);
            if (lichDay == null)
            {
                return HttpNotFound();
            }

            ViewBag.GiaoVienList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lichDay.MaGV);
            ViewBag.LopHocList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichDay.MaLH);
            return View(lichDay);
        }

        [HttpPost]
        public async Task<ActionResult> LichDayEdit(LichDay lichDay)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichDay).State = EntityState.Modified;
                await db.SaveChangesAsync();
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

                if(lichhocs != null)
                {
                    foreach (var lich in lichhocs)
                    {
                        string maLD = Utility.TaoMaNgauNhien("LD", 5);
                        // Kiểm tra xem đã có lịch dạy nào cho lớp, ngày và giờ này chưa
                        bool lichDayTonTai = db.LichDay.Any(ld =>
                            ld.MaLH == lich.MaLH &&
                            ld.NgayDay == lich.NgayHoc.Date &&
                            ld.GioBatDau == lich.GioBatDau &&
                            ld.GioKetThuc == lich.GioKetThuc);

                        if (!lichDayTonTai)
                        {
                            // Lấy thông tin lớp học từ bảng LopHoc
                            var ttLopHoc = db.LopHoc.Where(lh => lh.MaLH == lich.MaLH).FirstOrDefault();

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
            catch(DbEntityValidationException ex)
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
