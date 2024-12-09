using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminBinhLuanController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: Admin/AdminBinhLuan
        public ActionResult BinhLuanList()
        {
            var binhLuan = db.BinhLuanKhoaHoc.ToList();
            return View(binhLuan);
        }

        public ActionResult BinhLuanDelete(string id)
        {
            BinhLuanKhoaHoc binhLuan = db.BinhLuanKhoaHoc.FirstOrDefault(bl => bl.MaHV == id);
            return View(binhLuan);
        }

        [HttpPost]
        public ActionResult BinhLuanDelete(string id, BinhLuanKhoaHoc bl)
        {
            bl = db.BinhLuanKhoaHoc.FirstOrDefault(t => t.MaHV == id);

            if (bl != null)
            {
                db.BinhLuanKhoaHoc.Remove(bl);
                db.SaveChanges();
            }
            return RedirectToAction("BinhLuanList");
        }
    }
}
