using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminAccountController : Controller
    {
        // GET: Admin/Account

        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        public ActionResult AccountList(string search = "")
        {
            var ds = db.TaiKhoan;

            List<TaiKhoan> taikhoans = db.TaiKhoan.Where(e => e.TenDangNhap.Contains(search)).ToList();
            ViewBag.Search = search;

            return View(taikhoans);
        }

        public ActionResult AccountAdd()
        {
            List<TaiKhoan> taikhoans = db.TaiKhoan.ToList();
            return View();
        }
    }
}