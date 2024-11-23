using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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

            ViewBag.Email = giaovien.Email;

            return View(giaovien);
        }

        public ActionResult LichDay()
        {
            string magv = Session["MaGV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cần mã giảng viên!!!");
            }

            DateTime ngayHienTai = DateTime.Today;

            var lichdays = db.LichDay
                .Where(ld => ld.MaGV == magv && ld.NgayDay >= ngayHienTai)
                .Include(ld => ld.LopHoc)
                .Include(ld => ld.GiaoVien)
                .ToList();

            var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

            ViewBag.MaGV = magv;
            ViewBag.TenGV = giaovien.HoTen;
            ViewBag.LichDay = lichdays;
            ViewBag.Email = giaovien.Email;
            return View(lichdays);
        }
       
        public ActionResult DanhSachHocVien(string malh, DateTime ngayday)
        {
            var hocviens = db.LichHoc
            .Where(lh => lh.MaLH == malh && lh.NgayHoc == ngayday)
            .Select(lh => new
            {
                lh.HocVien.MaHV,
                lh.HocVien.HoTen,
                lh.HocVien.SoDT,
                lh.HocVien.Email,
                lh.DiemDanh
            })
            .ToList()
            .Select(hv => new HocVienViewModel
            {
                MaHV = hv.MaHV,
                HoTen = hv.HoTen,
                SoDT = hv.SoDT,
                Email = hv.Email,
                DiemDanh = hv.DiemDanh
            })
            .ToList();

            var lop = db.LopHoc.Where(lh => lh.MaLH == malh).FirstOrDefault();

            string magv = Session["MaGV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cần mã giảng viên!!!");
            }
            var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

            ViewBag.Email = giaovien.Email;
            ViewBag.TenLop = lop.TenLop;
            ViewBag.MaLH = malh;
            ViewBag.NgayDay = ngayday.ToShortDateString();
            return View(hocviens);
        }

        public ActionResult DanhSachLopHoc()
        {
            string magv = Session["MaGV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return RedirectToAction("Error");
            }

            var lopHocs = db.LopHoc
                .Where(l => l.MaGV == magv)
                .ToList();

            var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

            ViewBag.Email = giaovien.Email;

            return View(lopHocs);
        }

        public ActionResult ChiTietLopHoc(string malh)
        {
            if (string.IsNullOrEmpty(malh))
            {
                return RedirectToAction("Error");
            }

            var hocVienList = db.ChiTiet_HocVien_LopHoc
            .Where(ctlh => ctlh.MaLH == malh)
            .Select(ctlh => new HocVienViewModel
            {
                MaHV = ctlh.HocVien.MaHV,
                HoTen = ctlh.HocVien.HoTen,
                NgaySinh = ctlh.HocVien.NgaySinh,
                GioiTinh = ctlh.HocVien.GioiTinh,
                Email = ctlh.HocVien.Email,
                SoDT = ctlh.HocVien.SoDT,
                DiaChi = ctlh.HocVien.DiaChi,
                DiemKiemTraLan1 = (float)ctlh.DiemKiemTraLan1,
                DiemKiemTraLan2 = (float)ctlh.DiemKiemTraLan2,
                DiemKiemTraLan3 = (float)ctlh.DiemKiemTraLan3,
                DiemTrungBinh = (float)ctlh.DiemTrungBinh,
                Sobuoivang = ctlh.Sobuoivang,
                Daketthuc = ctlh.Daketthuc
            })
            .ToList();

            var lop = db.LopHoc.FirstOrDefault(lh => lh.MaLH == malh);
            if (lop == null)
            {
                return RedirectToAction("Error");
            }

            string magv = Session["MaGV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return RedirectToAction("Error");
            }
            var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

            ViewBag.Email = giaovien.Email;
            ViewBag.TenLop = lop.TenLop;
            ViewBag.MaLop = malh;

            return View(hocVienList);
        }

        [HttpPost]
        public ActionResult CapNhatDiem(List<HocVienViewModel> HocVienList, string malh)
        {
            if (HocVienList == null || !HocVienList.Any())
            {
                ViewBag.ErrorMessage = "Danh sách học viên trống!!!";
                return RedirectToAction("ChiTietLopHoc", new { malh = malh });
            }

            ViewBag.MaLop = malh;

            foreach (var hv in HocVienList)
            {
                var hocvien = db.ChiTiet_HocVien_LopHoc
                                .FirstOrDefault(ct => ct.MaHV == hv.MaHV && ct.MaLH == malh);

                if (hocvien != null)
                {
                    var diemKTL1Cu = hocvien.DiemKiemTraLan1;
                    var diemKTL2Cu = hocvien.DiemKiemTraLan2;
                    var diemKTL3Cu = hocvien.DiemKiemTraLan3;

                    try
                    {
                        hocvien.DiemKiemTraLan1 = Math.Round((double)hv.DiemKiemTraLan1, 2);
                        hocvien.DiemKiemTraLan2 = Math.Round((double)hv.DiemKiemTraLan2, 2);
                        hocvien.DiemKiemTraLan3 = Math.Round((double)hv.DiemKiemTraLan3, 2);
                        hocvien.Daketthuc = hv.Daketthuc;

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        hocvien.DiemKiemTraLan1 = diemKTL1Cu;
                        hocvien.DiemKiemTraLan2 = diemKTL2Cu;
                        hocvien.DiemKiemTraLan3 = diemKTL3Cu;

                        TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                        return RedirectToAction("ChiTietLopHoc", new { malh = malh });
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("ChiTietLopHoc", new { malh = malh });
        }

        [HttpPost]
        public ActionResult DiemDanh(string malh, string ngayday, Dictionary<string, bool> diemDanh)
        {
            if (string.IsNullOrEmpty(malh) || string.IsNullOrEmpty(ngayday) || diemDanh == null)
            {
                TempData["Error"] = "Thông tin điểm danh không hợp lệ!!!";
                return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
            }

            DateTime ngayHoc;
            if (!DateTime.TryParse(ngayday, out ngayHoc))
            {
                TempData["Error"] = "Ngày học không hợp lệ!!!";
                return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
            }

            foreach (var item in diemDanh)
            {
                string maHV = item.Key;
                bool vangHoc = item.Value;

                var lichHoc = db.LichHoc.FirstOrDefault(l => l.MaHV == maHV && l.MaLH == malh && l.NgayHoc == ngayHoc);

                if (lichHoc != null)
                {
                    if (vangHoc)
                    {
                        if (!lichHoc.DiemDanh)
                        {
                            var chiTiet = db.ChiTiet_HocVien_LopHoc.FirstOrDefault(ct => ct.MaLH == malh && ct.MaHV == maHV);
                            if (chiTiet != null)
                            {
                                chiTiet.Sobuoivang++;
                            }
                        }
                    }
                    else
                    {
                        if (lichHoc.DiemDanh)
                        {
                            var chiTiet = db.ChiTiet_HocVien_LopHoc.FirstOrDefault(ct => ct.MaLH == malh && ct.MaHV == maHV);
                            if (chiTiet != null)
                            {
                                chiTiet.Sobuoivang = Math.Max(0, chiTiet.Sobuoivang - 1);
                            }
                        }
                    }

                    lichHoc.DiemDanh = vangHoc ? true : false;
                }
            }

            db.SaveChanges();

            TempData["Success"] = "Cập nhật điểm danh thành công!";
            return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
        }

        [HttpPost]
        public ActionResult CapNhatThongTinGiaoVien(GiaoVien thontingiaovien)
        {
            if (ModelState.IsValid)
            {
                string magv = Session["MaGV"]?.ToString();
                var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

                if (giaovien != null)
                {
                    giaovien.HoTen = thontingiaovien.HoTen;
                    giaovien.Email = thontingiaovien.Email;
                    giaovien.SoDT = thontingiaovien.SoDT;
                    giaovien.DiaChi = thontingiaovien.DiaChi;
                    giaovien.BangCapGV = thontingiaovien.BangCapGV;
                    giaovien.LinhVucDaoTao = thontingiaovien.LinhVucDaoTao;
                    giaovien.NgayVaoLam = thontingiaovien.NgayVaoLam;
                    giaovien.Luong = thontingiaovien.Luong;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy giảng viên!";
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CapNhatAnhGiangVien(HttpPostedFileBase Anh)
        {
            if (Anh != null && Anh.ContentLength > 0)
            {
                string magv = Session["MaGV"]?.ToString();
                var giaoVien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

                if (giaoVien != null)
                {
                    string fileName = Path.GetFileName(Anh.FileName);
                    string path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    Anh.SaveAs(path);

                    giaoVien.Anh = fileName;
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật ảnh thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy giảng viên!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một ảnh hợp lệ!";
            }

            return RedirectToAction("Index");
        }
    }
}