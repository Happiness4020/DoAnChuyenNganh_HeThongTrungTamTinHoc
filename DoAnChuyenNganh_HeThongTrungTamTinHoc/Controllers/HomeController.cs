using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class HomeController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: Home

        public async Task<ActionResult> Index()
        {
            var khs = await db.KhoaHoc.Take(12).ToListAsync();

            List<ChuongTrinhHoc> cths = await db.ChuongTrinhHoc.ToListAsync();
            ViewBag.ChuongTrinhHocs = cths;

            if (khs == null || !khs.Any())
            {
                return Content("Không có dữ liệu khóa học.");
            }

            return View(khs);
        }

        public ActionResult HuongDanThanhToan()
        {
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;
            return View();
        }
    }
}