using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
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
            try
            {
                string magv = Session["MaGV"]?.ToString();
                if (string.IsNullOrEmpty(magv))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cần mã giảng viên!!!");
                }
                ViewBag.MaGV = magv;

                var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

                ViewBag.Email = giaovien.Email;

                return View(giaovien);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải trang: " + ex);
                return View();
            }
        }

        public ActionResult LichDay(int? page)
        {
            try
            {
                string magv = Session["MaGV"]?.ToString();
                if (string.IsNullOrEmpty(magv))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bạn cần đăng nhập bằng tài khoản của giáo viên!!!");
                }

                DateTime ngayHienTai = DateTime.Today;

                var lichdays = db.LichDay
                    .Where(ld => ld.MaGV == magv )
                    .Include(ld => ld.LopHoc)
                    .Include(ld => ld.GiaoVien)
                    .OrderBy(ld => ld.NgayDay)
                    .ToList();

                var giaovien = db.GiaoVien.Where(gv => gv.MaGV == magv).FirstOrDefault();

                ViewBag.MaGV = magv;
                if (giaovien != null)
                {
                    ViewBag.TenGV = giaovien.HoTen ?? "Chưa cập nhật";
                    ViewBag.Email = giaovien.Email ?? "Chưa cập nhật";
                }

                var lichdaytheotuan = lichdays
                    .GroupBy(ld => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(ld.NgayDay, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .Select(group => new LichDayTheoTuanViewModel
                    {
                        Week = group.Key,
                        Days = group.OrderBy(ld => ld.NgayDay).ToList()
                    })
                    .ToList();

                int sotuan = 1;
                int sotrang = page ?? 1;
                var dulieu = new PagedList<LichDayTheoTuanViewModel>(lichdaytheotuan, sotrang, sotuan);

                return View(dulieu);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách lịch dạy: " + ex);
                return View();
            }
        }
       
        public ActionResult DanhSachHocVien(string malh, DateTime ngayday)
        {
            try
            {
                if (malh != null && ngayday != null)
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

                    if (giaovien != null)
                    {
                        ViewBag.Email = giaovien.Email;
                    }

                    if (lop != null)
                    {
                        ViewBag.TenLop = lop.TenPhong;
                    }

                    ViewBag.MaLH = malh;
                    ViewBag.NgayDay = ngayday.ToString("yyyy/MM/dd");
                    return View(hocviens);
                }
                else
                {
                    return View();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách học viên: " + ex);
                return View();
            }
        }

        public ActionResult DanhSachLopHoc()
        {
            try
            {
                string magv = Session["MaGV"]?.ToString();
                if (string.IsNullOrEmpty(magv))
                {
                    return RedirectToAction("Error");
                }

                var lopHocs = db.LopHoc
                    .Where(l => l.MaGV == magv && db.ChiTiet_HocVien_LopHoc.Any(ct => ct.MaLH == l.MaLH))
                    .ToList();

                var giaovien = db.GiaoVien.FirstOrDefault(gv => gv.MaGV == magv);

                if (giaovien != null)
                {
                    ViewBag.Email = giaovien.Email;
                }

                return View(lopHocs);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách lớp học: " + ex);
                return View();
            }
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
                KetQua = ctlh.KetQua,
                ChoPhepNhapDiem = ctlh.ChoPhepNhapDiem
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
            ViewBag.TenLop = lop.TenPhong;
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

            var ctlh = db.ChiTiet_HocVien_LopHoc.FirstOrDefault(lh => lh.MaLH == malh);
            if(ctlh == null || ctlh.ChoPhepNhapDiem == false)
            {
                TempData["ErrorMessage"] = "Chưa được phép nhập điểm cho lớp học này!!!";
                return RedirectToAction("ChiTietLopHoc", new { malh = malh });
            }    

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
                        hocvien.KetQua = hv.KetQua;

                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                    }
                    catch (Exception ex)
                    {
                        hocvien.DiemKiemTraLan1 = diemKTL1Cu;
                        hocvien.DiemKiemTraLan2 = diemKTL2Cu;
                        hocvien.DiemKiemTraLan3 = diemKTL3Cu;

                        TempData["ErrorMessage"] = $"Lỗi khi cập nhật điểm: {ex.Message}";
                        return RedirectToAction("ChiTietLopHoc", new { malh = malh });
                    }
                }
            }
            return RedirectToAction("ChiTietLopHoc", new { malh = malh });
        }

        [HttpPost]
        public ActionResult DiemDanh(string malh, string ngayday, Dictionary<string, bool> diemDanh)
        {
            try
            {
                if (string.IsNullOrEmpty(malh) || string.IsNullOrEmpty(ngayday) || diemDanh == null)
                {
                    TempData["ErrorMessage"] = "Thông tin điểm danh không hợp lệ!!!";
                    return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
                }

                DateTime ngayhoc;
                if (!DateTime.TryParse(ngayday, out ngayhoc))
                {
                    TempData["ErrorMessage"] = "Ngày học không hợp lệ!!!";
                    return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
                }

                if (ngayhoc.Date != DateTime.Now.Date)
                {
                    TempData["ErrorMessage"] = "Chỉ có thể điểm danh trong ngày lớp học diễn ra!!!";
                    return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
                }


                foreach (var item in diemDanh)
                {
                    string mahv = item.Key;
                    bool vanghoc = item.Value;

                    var lichhoc = db.LichHoc.FirstOrDefault(l => l.MaHV == mahv && l.MaLH == malh && l.NgayHoc == ngayhoc);

                    if (lichhoc != null)
                    {
                        if (vanghoc)
                        {
                            if (!lichhoc.DiemDanh)
                            {
                                var chiTiet = db.ChiTiet_HocVien_LopHoc.FirstOrDefault(ct => ct.MaLH == malh && ct.MaHV == mahv);
                                if (chiTiet != null)
                                {
                                    chiTiet.Sobuoivang++;
                                }
                            }
                        }
                        else
                        {
                            if (lichhoc.DiemDanh)
                            {
                                var chiTiet = db.ChiTiet_HocVien_LopHoc.FirstOrDefault(ct => ct.MaLH == malh && ct.MaHV == mahv);
                                if (chiTiet != null)
                                {
                                    chiTiet.Sobuoivang = Math.Max(0, chiTiet.Sobuoivang - 1);
                                }
                            }
                        }
                        lichhoc.DiemDanh = vanghoc ? true : false;
                    }
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật điểm danh thành công!";
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi điểm danh!!! Hãy thử lại sau";
                return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
            }
            return RedirectToAction("DanhSachHocVien", new { malh, ngayday });
        }

        [HttpPost]
        public ActionResult CapNhatThongTinGiaoVien(GiaoVien thontingiaovien)
        {
            try
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
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!!!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult CapNhatAnhGiangVien(HttpPostedFileBase Anh)
        {
            try
            {
                if (Anh != null && Anh.ContentLength > 0)
                {
                    string magv = Session["MaGV"]?.ToString();

                    if (string.IsNullOrEmpty(magv))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy mã giáo viên!!!";
                        return RedirectToAction("Index");
                    }

                    var dinhdangchophep = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var dinhdanganh = Path.GetExtension(Anh.FileName).ToLower();

                    if (!dinhdangchophep.Contains(dinhdanganh))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                        return RedirectToAction("Index");
                    }

                    if (Anh.ContentLength > 3 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 3MB!";
                        return RedirectToAction("Index");
                    }

                    string tenanh = magv + dinhdanganh;
                    string duongdan = Path.Combine(Server.MapPath("~/AnhHocVien"), tenanh);
                    Anh.SaveAs(duongdan);


                    try
                    {
                        var query = "UPDATE GiaoVien SET Anh = @Anh WHERE MaGV = @MaGV";
                        db.Database.ExecuteSqlCommand(query,
                            new SqlParameter("@Anh", tenanh),
                            new SqlParameter("@MaGV", magv));

                        TempData["SuccessMessage"] = "Cập nhật ảnh thành công!";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh: " + ex.Message;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn một ảnh hợp lệ!";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh: ";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}