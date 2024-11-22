using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private string maLichDay = Utility.TaoMaNgauNhien("LD", 3);

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

        // Tạo mã Lịch dạy ngẫu nhiên
        public static string TaoMaLichDay()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maLichDay = new StringBuilder("LD");

            for (int i = 0; i < 3; i++)
            {
                maLichDay.Append(chars[random.Next(chars.Length)]);
            }

            return maLichDay.ToString();
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

        private readonly LichDayService lichdayservice;

        public AdminLichDayController(LichDayService lichDayService)
        {
            lichdayservice = lichDayService;
        }

        public ActionResult PhanLichDay(string magv)
        {
            // Gọi service phân lịch dạy
            lichdayservice.PhanLichDayChoGV(magv);

            // Quay lại trang hiện tại hoặc trang danh sách lịch dạy
            return RedirectToAction("Index");
        }
    }
}
