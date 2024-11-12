using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class GiangVienController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: GiangVien
        public ActionResult Index()
        {
            string magv = Session["MaGV"]?.ToString();
            ViewBag.MaGV = magv;

            var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

            return View(giaovien);
        }

        public ActionResult LichDay()
        {
            string magv = Session["MaGV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cần mã giảng viên!");
            }

            DateTime ngayHienTai = DateTime.Today;

            var lichdays = db.LichDay
                .Where(ld => ld.MaGV == magv && ld.NgayDay >= ngayHienTai)
                .Include(ld => ld.LopHoc)
                .Include(ld => ld.GiaoVien)
                .ToList();

            ViewBag.MaGV = magv;
            ViewBag.LichDay = lichdays;
            return View(lichdays);
        }
       
    }
}