using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System.Text;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using System.Data.SqlClient;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminLopHocController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string malh = Utility.TaoMaNgauNhien("LH", 3);


        public ActionResult LopHocList(string search = "", int page = 1, int pageSize = 10)
        {
            var lopHocQuery = db.LopHoc
                                .Include(l => l.GiaoVien)
                                .Include(l => l.KhoaHoc)
                                .Include(l => l.GiaoDichHocPhi);

            if (!string.IsNullOrEmpty(search))
            {
                lopHocQuery = lopHocQuery.Where(l => l.TenPhong.Contains(search) ||
                                                     l.GiaoVien.HoTen.Contains(search) ||
                                                     l.KhoaHoc.TenKH.Contains(search));
            }

            var lopHoc = lopHocQuery.ToList();

            var lopHocWithStudentCount = lopHoc.Select(l => new
            {
                LopHoc = l,
                SoHocVienDuocDuyet = l.GiaoDichHocPhi.Count(hv => hv.TrangThai == "Đã duyệt")
            }).ToList();

            int totalRecords = lopHocWithStudentCount.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            var paginatedLopHoc = lopHocWithStudentCount.Skip(recordsToSkip).Take(pageSize).ToList();

            return View(paginatedLopHoc.Select(l => l.LopHoc).ToList());
        }

        public ActionResult LopHocAdd()
        {

            //ViewBag.TrangThai = trangThai;
            //ViewBag.TenKhoaHoc = db.KhoaHoc.Find(model.MaKH)?.TenKH;
            //ViewBag.MaKH = model.MaKH;
            ViewBag.MaLH = malh;
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LopHocAdd(LopHoc lopHoc, string MaKH)
        {
            bool gv = db.LopHoc.Any(l => l.TenPhong == lopHoc.TenPhong && l.MaGV == lopHoc.MaGV);

            if (gv)
            {
                ModelState.AddModelError("TenPhong", "Lớp học này đã tồn tại với giáo viên đã chọn.");
            }

            bool lopHocdatontai = db.LopHoc.Any(l => l.TenPhong == lopHoc.TenPhong);

            if (lopHocdatontai)
            {
                ModelState.AddModelError("TenPhong", "Lớp học này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                lopHoc.MaLH = malh;
                db.LopHoc.Add(lopHoc);
                db.SaveChanges();
                return RedirectToAction("LopHocList");
            }

            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        public ActionResult LopHocAddGV( LopHoc model ,string MaKH, string tenKhoaHoc, bool trangThai)
        {

            ViewBag.TrangThai = trangThai;
            ViewBag.TenKhoaHoc = db.KhoaHoc.Find(model.MaKH)?.TenKH;
            ViewBag.MaKH = model.MaKH;
            ViewBag.MaLH = malh;
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LopHocAddGV(LopHoc lopHoc, string MaKH)
        {
            bool gv = db.LopHoc.Any(l => l.TenPhong == lopHoc.TenPhong && l.MaGV == lopHoc.MaGV);
            if (gv)
            {
                ModelState.AddModelError("TenPhong", "Lớp học này đã tồn tại với giáo viên đã chọn.");
            }

            bool lopHocdatontai = db.LopHoc.Any(l => l.TenPhong == lopHoc.TenPhong);
            if (lopHocdatontai)
            {
                ModelState.AddModelError("TenPhong", "Lớp học này đã tồn tại.");
            }

            bool lopHocTrungThoiGian = db.LopHoc.Any(l =>
                l.ThuHoc == lopHoc.ThuHoc &&
                ((lopHoc.GioBatDau >= l.GioBatDau && lopHoc.GioBatDau < l.GioKetThuc) ||   
                (lopHoc.GioKetThuc > l.GioBatDau && lopHoc.GioKetThuc <= l.GioKetThuc) ||
                (lopHoc.GioBatDau <= l.GioBatDau && lopHoc.GioKetThuc >= l.GioKetThuc))  
            );

            if (lopHocTrungThoiGian)
            {
                ModelState.AddModelError("ThuHoc", "Lớp học đã tồn tại trong khoảng thời gian và thứ đã chọn.");
            }

            if (ModelState.IsValid)
            {
                lopHoc.MaLH = malh;

                var hocVienDaDangKy = LayDanhSachHocVienDaDangKy(MaKH);
                foreach (var maHV in hocVienDaDangKy)
                {
                    var hocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHV);
                    if (hocVien != null)
                    {
                        lopHoc.ChiTiet_HocVien_LopHoc.Add(new ChiTiet_HocVien_LopHoc
                        {
                            MaHV = hocVien.MaHV,
                            MaLH = lopHoc.MaLH
                        });
                    }
                }

                db.LopHoc.Add(lopHoc);
                db.SaveChanges();

                var khoaHoc = db.KhoaHoc.Find(MaKH);
                if (khoaHoc != null)
                {
                    lopHoc.TrangThai = true;
                    db.Entry(lopHoc).State = EntityState.Modified;
                    db.SaveChanges();
                }


                var hocVienConLai = db.GiaoDichHocPhi
                                      .Where(gd => gd.MaKH == MaKH)
                                      .OrderBy(gd => gd.NgayGD)
                                      .Skip(20) 
                                      .ToList();

                if (hocVienConLai.Count >= 20)
                {
                    return RedirectToAction("LopHocAdd", new { MaKH = MaKH });
                }
                else
                {
                    ViewBag.Message = "Lớp đã được mở, các học viên dư sẽ chờ đến khi đủ 20 người để mở lớp tiếp theo.";
                }

                return RedirectToAction("LopHocList");
            }

            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        public ActionResult LopHocEdit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = db.LopHoc.Find(id);
            if (lopHoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LopHocEdit(LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lopHoc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("LopHocList");
            }
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        public ActionResult LopHocDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = db.LopHoc.Find(id);
            if (lopHoc == null)
            {
                return HttpNotFound();
            }
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LopHocDeleteConfirmed(string id)
        {
            try
            {
                LopHoc lopHoc = db.LopHoc.Find(id);
                db.LopHoc.Remove(lopHoc);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa lớp học thành công";
                return RedirectToAction("LopHocList");
            }
            catch
            {
                TempData["ErrorMessage"] = "Dữ liệu lớp học này vẫn đang được lưu tại nơi khác!!! Không thể xóa";
                return RedirectToAction("LopHocList");
            }
        }

        public List<string> LayDanhSachHocVienDaDangKy(string maKH, int soLuongCanLay = 20)
        {
            var danhSachHocVien = db.GiaoDichHocPhi
                                     .Where(gd => gd.MaKH == maKH && gd.TrangThai == "Đã duyệt")
                                     .OrderBy(gd => gd.NgayGD)
                                     .Select(gd => gd.MaHV)
                                     .Take(soLuongCanLay) 
                                     .ToList();
            return danhSachHocVien;
        }

        public ActionResult PhanLichHocChoHocVien(string maLH, int soBuoiHoc)
        {
            try
            {
                var lopHoc = db.LopHoc.Find(maLH);
                if (lopHoc == null)
                {
                    return HttpNotFound("Lớp học không tồn tại.");
                }

                var khoaHoc = db.KhoaHoc.Find(lopHoc.MaKH);
                if (khoaHoc == null)
                {
                    return HttpNotFound("Khóa học không tồn tại.");
                }

                DateTime ngayBatDau = khoaHoc.NgayBatDau;


                var danhSachHocVien = db.ChiTiet_HocVien_LopHoc
                                        .Where(ct => ct.MaLH == maLH)
                                        .Select(ct => ct.MaHV)
                                        .ToList();

                if (danhSachHocVien.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có học viên trong lớp này.";
                    return RedirectToAction("LopHocList");
                }

                DateTime ngayKetThuc = db.Database.SqlQuery<DateTime>(
                    "SELECT dbo.TinhNgayKetThuc(@NgayBatDau, @SoBuoiHoc, @ThuHoc)",
                    new SqlParameter("@NgayBatDau", ngayBatDau),
                    new SqlParameter("@SoBuoiHoc", soBuoiHoc),
                    new SqlParameter("@ThuHoc", lopHoc.ThuHoc)
                ).FirstOrDefault();


                if (khoaHoc.NgayKetThuc == null || khoaHoc.NgayKetThuc != ngayKetThuc)
                {
                    khoaHoc.NgayKetThuc = ngayKetThuc;
                    db.SaveChanges();
                }

                List<DayOfWeek> ngayHocTrongTuan = new List<DayOfWeek>();

                if (lopHoc.ThuHoc == "2-4-6")
                {
                    ngayHocTrongTuan = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
                }
                else if (lopHoc.ThuHoc == "3-5-7")
                {
                    ngayHocTrongTuan = new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday };
                }

                List<DateTime> dsNgayHoc = new List<DateTime>();
                DateTime ngayHienTai = ngayBatDau;


                while (dsNgayHoc.Count < soBuoiHoc)
                {
                    if (ngayHocTrongTuan.Contains(ngayHienTai.DayOfWeek))
                    {
                        dsNgayHoc.Add(ngayHienTai);
                    }
                    ngayHienTai = ngayHienTai.AddDays(1);
                }

                foreach (var maHV in danhSachHocVien)
                {
                    foreach (var ngayHoc in dsNgayHoc)
                    {
                        string malichhoc = Utility.TaoMaNgauNhien("LH", 8);

                        var lichHoc = new LichHoc
                        {
                            MaLichHoc = malichhoc,
                            MaLH = maLH,
                            MaHV = maHV,
                            NgayHoc = ngayHoc,
                            DiemDanh = false,
                            GioBatDau = lopHoc.GioBatDau,
                            GioKetThuc = lopHoc.GioKetThuc
                        };
                        db.LichHoc.Add(lichHoc);
                    }
                }
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Đã phân lịch học cho {danhSachHocVien.Count} học viên trong lớp {lopHoc.TenPhong}.";
                return RedirectToAction("LopHocList");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi phân lịch học: " + ex;
                return RedirectToAction("LopHocList");
            }
            
        }
    }
}
