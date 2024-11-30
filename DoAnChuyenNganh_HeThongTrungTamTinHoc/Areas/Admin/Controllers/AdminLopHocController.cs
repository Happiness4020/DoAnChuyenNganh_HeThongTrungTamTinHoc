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
            var lopHocQuery = db.LopHoc.Include(l => l.GiaoVien).Include(l => l.KhoaHoc);

            // Lọc dữ liệu nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                lopHocQuery = lopHocQuery.Where(l => l.TenLop.Contains(search) || l.GiaoVien.HoTen.Contains(search) || l.KhoaHoc.TenKH.Contains(search));
            }

            var lopHoc = lopHocQuery.ToList();

            // Lấy số học viên đã đăng ký cho mỗi khóa học
            foreach (var item in lopHoc)
            {
                item.SiSo = db.GiaoDichHocPhi
                    .Where(gd => gd.MaKH == item.MaKH)
                    .Select(gd => gd.MaHV)
                    .Distinct()
                    .Count();
            }

            // Tính toán phân trang
            int totalRecords = lopHoc.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            // Lấy dữ liệu cho trang hiện tại
            var paginatedLopHoc = lopHoc.Skip(recordsToSkip).Take(pageSize).ToList();

            return View(paginatedLopHoc);
        }


        public ActionResult LopHocAdd(string MaKH, string tenKhoaHoc, int siSo, bool trangThai)
        {
            ViewBag.TrangThai = trangThai;
            ViewBag.MaKH = MaKH;
            ViewBag.TenKhoaHoc = tenKhoaHoc;
            ViewBag.SiSo = siSo;
            ViewBag.MaLH = malh;
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen");
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LopHocAdd(LopHoc lopHoc, string MaKH)
        {
            bool gv = db.LopHoc.Any(l => l.TenLop == lopHoc.TenLop && l.MaGV == lopHoc.MaGV);
            if (gv)
            {
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại với giáo viên đã chọn.");
            }

            bool lopHocdatontai = db.LopHoc.Any(l => l.TenLop == lopHoc.TenLop);
            if (lopHocdatontai)
            {
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                lopHoc.MaLH = malh;

                // Lấy 20 học viên đầu tiên đăng ký khóa học
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

                // Xóa giao dịch học phí của 20 học viên đã thêm vào lớp
                var giaoDichs = db.GiaoDichHocPhi
                                  .Where(gd => hocVienDaDangKy.Contains(gd.MaHV) && gd.MaKH == MaKH)
                                  .ToList();

                foreach (var gd in giaoDichs)
                {
                    db.GiaoDichHocPhi.Remove(gd);
                }

                db.SaveChanges();

                // Kiểm tra nếu còn học viên dư, giữ lại họ để chờ mở lớp tiếp theo
                var hocVienConLai = db.GiaoDichHocPhi
                                      .Where(gd => gd.MaKH == MaKH)
                                      .OrderBy(gd => gd.NgayGD)
                                      .Skip(20) // Bỏ qua 20 người đầu tiên
                                      .ToList();

                if (hocVienConLai.Count >= 20)
                {
                    // Nếu có đủ 20 học viên tiếp theo, mở lớp tiếp theo
                    return RedirectToAction("LopHocAdd", new { MaKH = MaKH });
                }
                else
                {
                    // Nếu chưa đủ 20 học viên, chờ cho đến khi đủ
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
            LopHoc lopHoc = db.LopHoc.Find(id);
            db.LopHoc.Remove(lopHoc);
            db.SaveChanges();
            return RedirectToAction("LopHocList");
        }



        public List<string> LayDanhSachHocVienDaDangKy(string maKH, int soLuongCanLay = 20)
        {
            // Lấy danh sách các mã học viên đã đăng ký khóa học và giới hạn 20 người đầu tiên
            var danhSachHocVien = db.GiaoDichHocPhi
                                     .Where(gd => gd.MaKH == maKH && gd.TrangThai == "Đã duyệt")
                                     .OrderBy(gd => gd.NgayGD) // Sắp xếp theo ngày đăng ký
                                     .Select(gd => gd.MaHV)
                                     .Take(soLuongCanLay) // Lấy tối đa 20 học viên
                                     .ToList();
            return danhSachHocVien;
        }



        public ActionResult PhanLichHocChoHocVien(string maLH, int soBuoiHoc)
        {
            // Bước 1: Kiểm tra lớp học tồn tại không
            var lopHoc = db.LopHoc.Find(maLH);
            if (lopHoc == null)
            {
                return HttpNotFound("Lớp học không tồn tại.");
            }

            // Lấy thông tin khóa học
            var khoaHoc = db.KhoaHoc.Find(lopHoc.MaKH);
            if (khoaHoc == null)
            {
                return HttpNotFound("Khóa học không tồn tại.");
            }

            DateTime ngayBatDau = khoaHoc.NgayBatDau;

            // Bước 2: Lấy danh sách học viên đã đăng ký vào lớp học
            var danhSachHocVien = db.ChiTiet_HocVien_LopHoc
                                    .Where(ct => ct.MaLH == maLH)
                                    .Select(ct => ct.MaHV)
                                    .ToList();

            if (danhSachHocVien.Count == 0)
            {
                return Content("Không có học viên trong lớp này.");
            }

            // Bước 3: Xác định lịch học cho lớp (ngày học trong tuần - thứ 2, 4, 6)
            List<DayOfWeek> ngayHocTrongTuan = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
            List<DateTime> dsNgayHoc = new List<DateTime>();
            DateTime ngayHienTai = ngayBatDau;

            // Tính lịch học dựa trên số buổi học yêu cầu
            while (dsNgayHoc.Count < soBuoiHoc)
            {
                if (ngayHocTrongTuan.Contains(ngayHienTai.DayOfWeek))
                {
                    dsNgayHoc.Add(ngayHienTai);
                }
                ngayHienTai = ngayHienTai.AddDays(1); // Tăng thêm một ngày
            }

            // Ngày kết thúc sẽ là ngày của buổi học cuối cùng
            DateTime ngayKetThuc = khoaHoc.NgayKetThuc;


            // Bước 4: Phân lịch học cho từng học viên
            foreach (var maHV in danhSachHocVien)
            {
                foreach (var ngayHoc in dsNgayHoc)
                {
                    string malichhoc = Utility.TaoMaNgauNhien("LH", 4);

                    // Tạo bản ghi lịch học cho học viên
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

                    // Thêm lịch học vào cơ sở dữ liệu
                    db.LichHoc.Add(lichHoc);
                }
            }

            // Lưu lịch học cho tất cả học viên và cập nhật ngày kết thúc
            db.SaveChanges();

            // Trả về thông báo thành công
            ViewBag.Message = $"Đã phân lịch học cho {danhSachHocVien.Count} học viên trong lớp {lopHoc.TenLop}.";
            return RedirectToAction("LopHocList");
        }





    }
}
