using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(DangNhap dn)
        {
            if (ModelState.IsValid)
            {
                if (dn.Email == "duy" && dn.MatKhau == "1")
                {
                    return RedirectToAction("Index", "HocVien");
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin đăng nhập không đúng.");
                }
            }
            return View();
        }


        public ActionResult DangKy()
        {
            return View();
        }
    }
}