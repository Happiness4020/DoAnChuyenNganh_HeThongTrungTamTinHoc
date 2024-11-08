using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class AccountController : Controller
    {
        TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(TaiKhoan tk, string retypePassword)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (tk.MatKhau != retypePassword)
            {
                ModelState.AddModelError("retypePassword", "Password không khớp.");
                return View();
            }


            TaiKhoan taikhoan = ttth.TaiKhoan.Where(t => t.TenDangNhap == tk.TenDangNhap).FirstOrDefault();
            if (taikhoan != null)
            {
                ModelState.AddModelError("TenDangNhap", "Tài khoản đã tồn tại.");
                return View();
            }

            taikhoan = ttth.TaiKhoan.Where(u => u.MaHV == tk.MaHV).FirstOrDefault();
            if (taikhoan != null)
            {
                ModelState.AddModelError("MaHV", "Mã học viên đã tồn tại.");
                return View();
            }

            taikhoan = new TaiKhoan();
            taikhoan.MaHV = tk.MaHV;
            taikhoan.TenDangNhap = tk.TenDangNhap;
            taikhoan.MatKhau = tk.MatKhau;
            taikhoan.QuyenHan = "Học viên";
            taikhoan.MaGV = null;
            ttth.TaiKhoan.Add(taikhoan);
            ttth.SaveChanges();

            return RedirectToAction("DangNhap");
        }


        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(TaiKhoan tk)
        {
           
            if (tk != null)
            {
                if (tk.MatKhau == null)
                {
                    ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu");
                    return View();
                }
                else
                {


                    TaiKhoan taikhoan = ttth.TaiKhoan.Where(t => t.TenDangNhap == tk.TenDangNhap).FirstOrDefault();
                    if (taikhoan != null)
                    {
                        if (taikhoan.MatKhau != tk.MatKhau)
                        {
                            ModelState.AddModelError("MatKhau", "Mật khẩu không chính xác");
                            return View();
                        }

                        HttpCookie NDCookie = new HttpCookie("NguoiDung", taikhoan.TenDangNhap);
                        HttpCookie roleCookie = new HttpCookie("QuyenHan", taikhoan.QuyenHan);

                        Response.Cookies.Add(NDCookie);
                        Response.Cookies.Add(roleCookie);
                        if (taikhoan.QuyenHan == "Quản lý")
                        {
                            return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        }
                        else if (taikhoan.QuyenHan == "Giáo viên")
                        {
                            return RedirectToAction("Index", "GiangVien");
                        }
                        else
                        {
                            Session["TenDangNhap"] = taikhoan.TenDangNhap;
                            return RedirectToAction("Index", "HocVien");
                        }
                    }
                }
            }
            return View();
        }



        public ActionResult DangXuat()
        {
            HttpCookie authCookie = new HttpCookie("NguoiDung");
            authCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(authCookie);

            HttpCookie roleCookie = new HttpCookie("QuyenHan");
            roleCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(roleCookie);

            Session["TenDangNhap"] = null;

            return RedirectToAction("DangNhap", "Account");
        }
    }
}