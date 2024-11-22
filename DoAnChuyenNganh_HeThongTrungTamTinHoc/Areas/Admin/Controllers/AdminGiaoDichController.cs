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
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminGiaoDichController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: Admin/AdminGiaoDichHocPhi
        public async Task<ActionResult> GiaoDichList()
        {
            var giaoDichHocPhi = db.GiaoDichHocPhi.Include(g => g.HocVien).Include(g => g.PhuongThucThanhToan).Include(g => g.KhoaHoc);
            return View(await giaoDichHocPhi.ToListAsync());
        }



        public ActionResult GiaoDichAdd()
        {
            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen");
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT");
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiaoDichAdd(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.GiaoDichHocPhi.Add(giaoDichHocPhi);
                await db.SaveChangesAsync();
                return RedirectToAction("GiaoDichList");
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public async Task<ActionResult> GiaoDichEdit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
            if (giaoDichHocPhi == null)
            {
                return HttpNotFound();
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiaoDichEdit(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(giaoDichHocPhi).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("GiaoDichList");
            }

            // Gán lại danh sách nếu ModelState không hợp lệ
            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public async Task<ActionResult> GiaoDichDelete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
            if (giaoDichHocPhi == null)
            {
                return HttpNotFound();
            }
            return View(giaoDichHocPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiaoDichDeleteConfirmed(int id)
        {
            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
            db.GiaoDichHocPhi.Remove(giaoDichHocPhi);
            await db.SaveChangesAsync();
            return RedirectToAction("GiaoDichList");
        }



    }
}

