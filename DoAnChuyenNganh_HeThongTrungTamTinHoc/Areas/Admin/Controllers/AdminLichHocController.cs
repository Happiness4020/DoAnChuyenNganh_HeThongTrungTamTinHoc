using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminLichHocController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string malh = Utility.TaoMaNgauNhien("LH", 3);
        // GET: Admin/AdminLichHoc
        public async Task<ActionResult> LichHocList()
        {
            var lichHoc = db.LichHoc.Include(l => l.HocVien).Include(l => l.LopHoc);
            return View(await lichHoc.ToListAsync());
        }


       
        public ActionResult LichHocAdd()
        {
            ViewBag.MaLichHoc = malh;
            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen");
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenLop");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LichHocAdd(LichHoc lichHoc)
        {
            // Kiểm tra xem lịch học của học viên này trong lớp này đã tồn tại hay chưa
            var existingLichHoc = db.LichHoc
                .FirstOrDefault(l => l.MaHV == lichHoc.MaHV && l.MaLH == lichHoc.MaLH);

            if (existingLichHoc != null)
            {
                // Thêm thông báo lỗi nếu đã tồn tại
                ModelState.AddModelError("", "Lịch học của học viên này cho lớp này đã tồn tại.");
            }
            if (ModelState.IsValid)
            {
                db.LichHoc.Add(lichHoc);
                await db.SaveChangesAsync();
                return RedirectToAction("LichHocList");
            }

            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichHoc.MaLH);
            return View(lichHoc);
        }

        
        public ActionResult LichHocEdit(string id)
        {
            LichHoc lichHoc = db.LichHoc.FirstOrDefault(ld => ld.MaLichHoc == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if (lichHoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
            ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichHoc.MaLH);
            return View(lichHoc);
        }

       
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LichHocEdit(LichHoc lichHoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichHoc).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("LichHocList");
            }
            else
            {
                ViewBag.MaHVList = new SelectList(db.HocVien, "MaHV", "HoTen", lichHoc.MaHV);
                ViewBag.MaLHList = new SelectList(db.LopHoc, "MaLH", "TenLop", lichHoc.MaLH);
                return View(lichHoc);
            }    
            
        }

        
        public async Task<ActionResult> LichHocDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LichHoc lichHoc = await db.LichHoc.FirstOrDefaultAsync(ld => ld.MaLichHoc == id);
            if (lichHoc == null)
            {
                return HttpNotFound();
            }
            return View(lichHoc);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LichHocDelete(string id, LichHoc lichHoc)
        {
            lichHoc = db.LichHoc.FirstOrDefault(ld => ld.MaLichHoc == id);
            db.LichHoc.Remove(lichHoc);
            await db.SaveChangesAsync();
            return RedirectToAction("LichHocList");
        }

      
    }
}
