using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

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
            var hocviens = db.HocVien
                                   .Where(hv => !db.TaiKhoan.Any(tk => tk.MaHV == hv.MaHV))
                                   .ToList();

            var giaoviens = db.GiaoVien
                                   .Where(gv => !db.TaiKhoan.Any(tk => tk.MaGV == gv.MaGV))
                                   .ToList();
            ViewBag.Hocviens = hocviens ?? new List<HocVien>();
            ViewBag.Giaoviens = giaoviens ?? new List<GiaoVien>();
            return View();
        }
        [HttpPost]
        public ActionResult AccountAdd(TaiKhoan tk)
        {
            if (ModelState.IsValid)
            {
                TaiKhoan taikhoan = db.TaiKhoan.Where(t => t.TenDangNhap == tk.TenDangNhap).FirstOrDefault();
                if (taikhoan != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tài khoản đã tồn tại!!");
                    return View();
                }

                if (string.IsNullOrEmpty(tk.MaHV) || string.IsNullOrEmpty(tk.MaGV))
                {
                    ModelState.AddModelError("", "Vui lòng chọn mã học viên hoặc mã giáo viên!!!");
                    return View();
                }

                if (!string.IsNullOrEmpty(tk.MaHV))
                {
                    taikhoan = db.TaiKhoan.Where(t => t.MaHV == tk.MaHV).FirstOrDefault();
                    if (taikhoan != null)
                    {
                        ModelState.AddModelError("MaHV", "Học viên đã có tài khoản!!");
                        return View();
                    }

                    taikhoan = new TaiKhoan
                    {
                        MaHV = tk.MaHV,
                        TenDangNhap = tk.TenDangNhap,
                        MatKhau = tk.MatKhau,
                        QuyenHan = tk.QuyenHan,
                        MaGV = null
                    };
                }
                else if (!string.IsNullOrEmpty(tk.MaGV))
                {
                    taikhoan = db.TaiKhoan.Where(t => t.MaGV == tk.MaGV).FirstOrDefault();
                    if (taikhoan != null)
                    {
                        ModelState.AddModelError("MaGV", "Giáo viên đã có tài khoản!!");
                        return View();
                    }

                    taikhoan = new TaiKhoan
                    {
                        MaGV = tk.MaGV,
                        TenDangNhap = tk.TenDangNhap,
                        MatKhau = tk.MatKhau,
                        QuyenHan = tk.QuyenHan,
                        MaHV = null
                    };
                }

                db.TaiKhoan.Add(taikhoan);
                db.SaveChanges();

                return RedirectToAction("AccountList");
            }
            else
            {
                return View();
            }
        }
    }
}