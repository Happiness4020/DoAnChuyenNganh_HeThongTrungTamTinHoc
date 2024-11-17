using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
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
       
        public ActionResult DanhSachHocVien(string malh, DateTime ngayday)
        {
            var hocviens = db.LichHoc
            .Where(lh => lh.MaLH == malh)
            .Select(lh => lh.HocVien)
            .ToList();

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

            return View(lopHocs);
        }

        public ActionResult ChiTietLopHoc(string malh)
        {
            if (string.IsNullOrEmpty(malh))
            {
                // Nếu không có mã lớp học, trả về danh sách trống hoặc lỗi
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

            ViewBag.TenLop = lop.TenLop;
            ViewBag.MaLop = malh;

            return View(hocVienList);
        }

        [HttpPost]
        public ActionResult CapNhatDiem(List<HocVienViewModel> HocVienList, string malh)
        {
            if (HocVienList == null || !HocVienList.Any())
            {
                ViewBag.ErrorMessage = "Danh sách học viên trống!";
                return RedirectToAction("ChiTietLopHoc", new { malh = malh });
            }

            ViewBag.MaLop = malh;

            foreach (var hv in HocVienList)
            {
                var hocvien = db.ChiTiet_HocVien_LopHoc
                                .FirstOrDefault(ct => ct.MaHV == hv.MaHV && ct.MaLH == malh);
                if (hocvien != null)
                {
                    hocvien.DiemKiemTraLan1 = Math.Round((double)hv.DiemKiemTraLan1, 2);
                    hocvien.DiemKiemTraLan2 = Math.Round((double)hv.DiemKiemTraLan2, 2);
                    hocvien.DiemKiemTraLan3 = Math.Round((double)hv.DiemKiemTraLan3, 2);
                    hocvien.Sobuoivang = hv.Sobuoivang;
                    hocvien.Daketthuc = hv.Daketthuc;

                }
            }

            db.SaveChanges();

            return RedirectToAction("ChiTietLopHoc", new { malh = malh });
        }
    }
}