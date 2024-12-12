using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using BCrypt.Net;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminAccountController : Controller
    {
        // GET: Admin/Account

        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        public ActionResult AccountList(string search = "")
        {

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

                if (string.IsNullOrEmpty(tk.QuyenHan))
                {
                    ModelState.AddModelError("QuyenHan", "Vui lòng chọn quyền hạn");
                    return View();
                }

                if (string.IsNullOrEmpty(tk.MaHV) && string.IsNullOrEmpty(tk.MaGV))
                {
                    ModelState.AddModelError("", "Vui lòng chọn mã học viên hoặc mã giáo viên!!!");
                    return View();
                }

                // Mã hóa mật khẩu bằng BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tk.MatKhau);

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
                        MatKhau = hashedPassword,
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
                        MatKhau = hashedPassword,
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

        public ActionResult AccountDelete(int id)
        {
            TaiKhoan tk = db.TaiKhoan.Where(t => t.MaTK == id).FirstOrDefault();
            return View(tk);
        }
        [HttpPost]
        public ActionResult AccountDelete(int id, TaiKhoan taiKhoan)
        {
            taiKhoan = db.TaiKhoan.Where(t => t.MaTK == id).FirstOrDefault();
            db.TaiKhoan.Remove(taiKhoan);
            db.SaveChanges();
            return RedirectToAction("AccountList");
        }



        public ActionResult AccountEdit(int id)
        {
            TaiKhoan tk = db.TaiKhoan.FirstOrDefault(t => t.MaTK == id);
            if (tk == null)
            {
                return HttpNotFound("Không tìm thấy tài khoản.");
            }

            var hocviens = db.HocVien
                             .Where(hv => !db.TaiKhoan.Any(t => t.MaHV == hv.MaHV) || hv.MaHV == tk.MaHV)
                             .ToList();

            var giaoviens = db.GiaoVien
                              .Where(gv => !db.TaiKhoan.Any(t => t.MaGV == gv.MaGV) || gv.MaGV == tk.MaGV)
                              .ToList();

            ViewBag.Hocviens = hocviens;
            ViewBag.Giaoviens = giaoviens;

            return View(tk);
        }

        [HttpPost]
        public ActionResult AccountEdit(int id, TaiKhoan tk)
        {
            if (ModelState.IsValid)
            {
                var taikhoan = db.TaiKhoan.FirstOrDefault(t => t.TenDangNhap == tk.TenDangNhap && t.MaTK != id);
                if (taikhoan != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(tk);
                }

                if (string.IsNullOrEmpty(tk.MaHV) && string.IsNullOrEmpty(tk.MaGV))
                {
                    ModelState.AddModelError("", "Vui lòng chọn Mã học viên hoặc Mã giáo viên.");
                    return View(tk);
                }

                if (!string.IsNullOrEmpty(tk.MaHV))
                {
                    taikhoan = db.TaiKhoan.FirstOrDefault(t => t.MaHV == tk.MaHV && t.MaTK != id);
                    if (taikhoan != null)
                    {
                        ModelState.AddModelError("MaHV", "Học viên này đã có tài khoản khác.");
                        return View(tk);
                    }
                }

                if (!string.IsNullOrEmpty(tk.MaGV))
                {
                    taikhoan = db.TaiKhoan.FirstOrDefault(t => t.MaGV == tk.MaGV && t.MaTK != id);
                    if (taikhoan != null)
                    {
                        ModelState.AddModelError("MaGV", "Giáo viên này đã có tài khoản khác.");
                        return View(tk);
                    }
                }

                TaiKhoan taikhoanUpdate = db.TaiKhoan.FirstOrDefault(t => t.MaTK == id);
                if (taikhoanUpdate != null)
                {
                    taikhoanUpdate.TenDangNhap = tk.TenDangNhap;
                    taikhoanUpdate.QuyenHan = tk.QuyenHan;
                    taikhoanUpdate.MaHV = tk.MaHV;
                    taikhoanUpdate.MaGV = tk.MaGV;

                    db.SaveChanges();
                    return RedirectToAction("AccountList");
                }
            }

            var hocviens = db.HocVien
                             .Where(hv => !db.TaiKhoan.Any(t => t.MaHV == hv.MaHV) || hv.MaHV == tk.MaHV)
                             .ToList();
            var giaoviens = db.GiaoVien
                              .Where(gv => !db.TaiKhoan.Any(t => t.MaGV == gv.MaGV) || gv.MaGV == tk.MaGV)
                              .ToList();
            ViewBag.Hocviens = hocviens;
            ViewBag.Giaoviens = giaoviens;

            return View(tk);
        }
    }
}