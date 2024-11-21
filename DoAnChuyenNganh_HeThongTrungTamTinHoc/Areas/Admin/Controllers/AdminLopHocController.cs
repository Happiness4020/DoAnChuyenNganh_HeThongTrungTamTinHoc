using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System.Text;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLopHocController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string malh = Utility.TaoMaNgauNhien("LH", 3);
        public async Task<ActionResult> LopHocList()
        {
            var lopHoc = db.LopHoc.Include(l => l.GiaoVien).Include(l => l.KhoaHoc);
            return View(await lopHoc.ToListAsync());
        }

        public ActionResult LopHocAdd()
        {
            ViewBag.MaLH = malh;
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LopHocAdd(LopHoc lopHoc)
        {
            // Kiểm tra lớp học với tên lớp và giáo viên đã tồn tại chưa
            bool gv = await db.LopHoc
                .AnyAsync(l => l.TenLop == lopHoc.TenLop && l.MaGV == lopHoc.MaGV);

            if (gv)
            {
                // Nếu tồn tại thì thêm lỗi vào ModelState và hiển thị thông báo
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại với giáo viên đã chọn.");
            }

            bool lopHocdatontai = await db.LopHoc
               .AnyAsync(l => l.TenLop == lopHoc.TenLop);

            if (lopHocdatontai)
            {
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Nếu không tồn tại thì tiếp tục thêm lớp học
                lopHoc.MaLH = malh;
                db.LopHoc.Add(lopHoc);
                await db.SaveChangesAsync();
                return RedirectToAction("LopHocList");
            }

            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        
        public async Task<ActionResult> LopHocEdit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
            if (lopHoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LopHocEdit(LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lopHoc).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("LopHocList");
            }
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        public async Task<ActionResult> LopHocDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
            if (lopHoc == null)
            {
                return HttpNotFound();
            }
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LopHocDeleteConfirmed(string id)
        {
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
            db.LopHoc.Remove(lopHoc);
            await db.SaveChangesAsync();
            return RedirectToAction("LopHocList");
        }


    }
}
