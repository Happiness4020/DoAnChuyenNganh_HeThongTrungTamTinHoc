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
        public async Task<ActionResult> LopHocList()
        {
            var lopHoc = db.LopHoc.Include(l => l.GiaoVien).Include(l => l.KhoaHoc);

            foreach (var item in lopHoc)
            {
                // Lấy số học viên đã đăng ký cho mỗi khóa học
                item.SiSo = await db.GiaoDichHocPhi
                    .Where(gd => gd.MaKH == item.MaKH)
                    .Select(gd => gd.MaHV)
                    .Distinct()
                    .CountAsync();
            }
            return View(await lopHoc.ToListAsync());
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
        public async Task<ActionResult> LopHocAdd(LopHoc lopHoc, string MaKH)
        {
            // Kiểm tra lớp học với tên lớp và giáo viên đã tồn tại chưa
            bool gv = await db.LopHoc
                .AnyAsync(l => l.TenLop == lopHoc.TenLop && l.MaGV == lopHoc.MaGV);

            if (gv)
            {
                // Nếu tồn tại thì thêm lỗi vào ModelState và hiển thị thông báo
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại với giáo viên đã chọn.");
            }

            bool lopHocdatontai = await db.LopHoc
               .AnyAsync(l => l.TenLop == lopHoc.TenLop);

            if (lopHocdatontai)
            {
                ModelState.AddModelError("TenLop", "Lớp học này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Nếu không tồn tại thì tiếp tục thêm lớp học
                lopHoc.MaLH = malh;

                var hocVienDaDangKy = LayDanhSachHocVienDaDangKy(MaKH);

                // Thêm học viên vào lớp học mới, bỏ qua những học viên đã có trong lớp khác
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
                await db.SaveChangesAsync();

                // Tìm khóa học có mã MaKH để cập nhật trạng thái
                var khoaHoc = db.KhoaHoc.Find(MaKH);
                if (khoaHoc != null)
                {
                    // Cập nhật trạng thái của khóa học sang "Đã mở lớp"
                    lopHoc.TrangThai = true;
                    db.Entry(lopHoc).State = EntityState.Modified;
                    db.SaveChanges();
                }

                // Xóa học viên đã đăng ký khóa học khỏi bảng GiaoDichHocPhi
                var giaoDichs = db.GiaoDichHocPhi.Where(gd => gd.MaKH == MaKH).ToList();
                foreach (var gd in giaoDichs)
                {
                    // Xóa từng giao dịch học phí liên quan đến khóa học
                    db.GiaoDichHocPhi.Remove(gd);
                }

                // Lưu thay đổi vào cơ sở dữ liệu sau khi xóa học viên
                await db.SaveChangesAsync();
                return RedirectToAction("LopHocList");
            }

            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        
        public async Task<ActionResult> LopHocEdit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
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
        public async Task<ActionResult> LopHocEdit(LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lopHoc).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("LopHocList");
            }
            ViewBag.MaGVList = new SelectList(db.GiaoVien, "MaGV", "HoTen", lopHoc.MaGV);
            ViewBag.MaKHList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", lopHoc.MaKH);
            return View(lopHoc);
        }

        public async Task<ActionResult> LopHocDelete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
            if (lopHoc == null)
            {
                return HttpNotFound();
            }
            return View(lopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LopHocDeleteConfirmed(string id)
        {
            LopHoc lopHoc = await db.LopHoc.FindAsync(id);
            db.LopHoc.Remove(lopHoc);
            await db.SaveChangesAsync();
            return RedirectToAction("LopHocList");
        }


        public async Task<ActionResult> HienSoHocVienDangKy(string maKH)
        {
            if (maKH == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy danh sách học viên đăng ký khóa học
            var soHocVien = await db.GiaoDichHocPhi
                                    .Where(gd => gd.MaKH == maKH)
                                    .Select(gd => gd.MaHV)
                                    .Distinct()
                                    .CountAsync();

            // Kiểm tra điều kiện mở lớp học
            bool moLop = soHocVien >= 20;

            // Tạo thông báo
            string message = moLop
                ? "Khóa học đủ số lượng và có thể mở lớp."
                : $"Khóa học hiện có {soHocVien} học viên đăng ký. Cần thêm {20 - soHocVien} học viên nữa để mở lớp.";

            // Trả về kết quả
            ViewBag.Message = message;
            ViewBag.SoHocVien = soHocVien;

            var khoaHoc = await db.KhoaHoc.FindAsync(maKH);
            if (khoaHoc == null)
            {
                return HttpNotFound();
            }
            return View(khoaHoc);
        }



        public List<string> LayDanhSachHocVienDaDangKy(string maKH)
        {
            // Lấy danh sách các mã học viên đã đăng ký khóa học
            var danhSachHocVien = db.GiaoDichHocPhi
                                     .Where(gd => gd.MaKH == maKH)
                                     .Select(gd => gd.MaHV)
                                     .ToList();
            return danhSachHocVien;
        }

    }
}
