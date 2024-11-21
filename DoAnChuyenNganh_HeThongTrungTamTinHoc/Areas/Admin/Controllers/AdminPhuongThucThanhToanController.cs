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

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminPhuongThucThanhToanController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: Admin/AdminPhuongThucThanhToan
        public async Task<ActionResult> PhuongThucThanhToanList()
        {
            return View(await db.PhuongThucThanhToan.ToListAsync());
        }


        // GET: Admin/AdminPhuongThucThanhToan/Create
        public ActionResult PhuongThucThanhToanAdd()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhuongThucThanhToanAdd( PhuongThucThanhToan phuongThucThanhToan)
        {
            if (ModelState.IsValid)
            {
                db.PhuongThucThanhToan.Add(phuongThucThanhToan);
                await db.SaveChangesAsync();
                return RedirectToAction("PhuongThucThanhToanList");
            }

            return View(phuongThucThanhToan);
        }

        // GET: Admin/AdminPhuongThucThanhToan/Edit/5
        public async Task<ActionResult> PhuongThucThanhToanEdit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhuongThucThanhToan phuongThucThanhToan = await db.PhuongThucThanhToan.FindAsync(id);
            if (phuongThucThanhToan == null)
            {
                return HttpNotFound();
            }
            return View(phuongThucThanhToan);
        }

        // POST: Admin/AdminPhuongThucThanhToan/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhuongThucThanhToanEdit( PhuongThucThanhToan phuongThucThanhToan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(phuongThucThanhToan).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("PhuongThucThanhToanList");
            }
            return View(phuongThucThanhToan);
        }

        // GET: Admin/AdminPhuongThucThanhToan/Delete/5
        public async Task<ActionResult> PhuongThucThanhToanDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PhuongThucThanhToan phuongThucThanhToan = await db.PhuongThucThanhToan.FindAsync(id);
            if (phuongThucThanhToan == null)
            {
                return HttpNotFound();
            }
            return View(phuongThucThanhToan);
        }

        // POST: Admin/AdminPhuongThucThanhToan/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhuongThucThanhToanDeleteConfirmed(int id)
        {
            PhuongThucThanhToan phuongThucThanhToan = await db.PhuongThucThanhToan.FindAsync(id);
            db.PhuongThucThanhToan.Remove(phuongThucThanhToan);
            await db.SaveChangesAsync();
            return RedirectToAction("PhuongThucThanhToanList");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
