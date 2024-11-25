using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminKhoaHocController : Controller
    {
        private TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string makh = Utility.TaoMaNgauNhien("KH", 3);

        // GET: Admin/AdminKhoaHoc
        public async Task<ActionResult> KhoaHocList(string search = "", int page = 1, int pageSize = 10)
        {
            var khoahoc = await ttth.KhoaHoc
                .Where(kh => kh.TenKH.Contains(search))
                .ToListAsync();

            ViewBag.Search = search;

            // phân trang
            int NoOfRecordPerPage = 7;
            int NoOfPage = (int)Math.Ceiling((double)khoahoc.Count / NoOfRecordPerPage);
            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;

            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPage;
            khoahoc = khoahoc.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();


            return View(khoahoc);
        }

        public async Task<ActionResult> DanhSachHocVienThamGiaKhoaHoc()
        {
            // Lấy tất cả các khóa học
            var khoaHocList = await ttth.KhoaHoc.ToListAsync();

            // Dùng ViewModel để hiển thị khóa học cùng số học viên đăng ký
            var khoaHocViewModelList = new List<KhoaHocViewModel>();

            foreach (var khoaHoc in khoaHocList)
            {
                // Đếm số học viên đã đăng ký khóa học này
                var soHocVien = await ttth.GiaoDichHocPhi
                                        .Where(gd => gd.MaKH == khoaHoc.MaKH)
                                        .Select(gd => gd.MaHV)
                                        .Distinct()
                                        .CountAsync();

                // Kiểm tra điều kiện mở lớp
                bool moLop = soHocVien >= 20;

                // Thêm vào ViewModel
                var khoaHocViewModel = new KhoaHocViewModel
                {
                    MaKH = khoaHoc.MaKH,
                    TenKH = khoaHoc.TenKH,
                    SoHocVien = soHocVien,
                    MoLop = moLop
                };

                khoaHocViewModelList.Add(khoaHocViewModel);
            }

            return View(khoaHocViewModelList);
        }


        public async Task<ActionResult> KhoaHocAdd()
        {
            ViewBag.MaChuongTrinh = new SelectList(await ttth.ChuongTrinhHoc.ToListAsync(), "MaChuongTrinh", "TenChuongTrinh");
            ViewBag.MaKH = makh;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> KhoaHocAdd(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(khoahoc.MaChuongTrinh))
            {
                ModelState.AddModelError("MaChuongTrinh", "Vui lòng chọn chương trình");
            }

            ViewBag.MaChuongTrinh = new SelectList(await ttth.ChuongTrinhHoc.ToListAsync(), "MaChuongTrinh", "TenChuongTrinh");

            if (ModelState.IsValid)
            {
                var existingKhoaHoc = await ttth.KhoaHoc.FirstOrDefaultAsync(kh => kh.MaKH == khoahoc.MaKH);
                if (existingKhoaHoc != null)
                {
                    ModelState.AddModelError("MaKH", "Mã khóa học đã tồn tại!");
                    return View();
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }

                    var fileName = khoahoc.MaKH + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), fileName);
                    imageFile.SaveAs(path);
                    khoahoc.Anh = fileName;
                }
                else
                {
                    khoahoc.Anh = "noimage.jpg";
                }

                ttth.KhoaHoc.Add(khoahoc);
                await ttth.SaveChangesAsync();
                return RedirectToAction("KhoaHocList");
            }
            return View();
        }   


        public async Task<ActionResult> KhoaHocDelete(string id)
        {
            var khoahoc = await ttth.KhoaHoc.FirstOrDefaultAsync(kh => kh.MaKH == id);
            if (khoahoc == null)
            {
                return HttpNotFound("Không tìm thấy khóa học.");
            }
            return View(khoahoc);
        }

        [HttpPost]
        public async Task<ActionResult> KhoaHocDelete(string id, KhoaHoc khoahoc)
        {
            khoahoc = await ttth.KhoaHoc.FirstOrDefaultAsync(kh => kh.MaKH == id);
            if (khoahoc != null)
            {
                ttth.KhoaHoc.Remove(khoahoc);
                await ttth.SaveChangesAsync();
            }
            return RedirectToAction("KhoaHocList");
        }

        public async Task<ActionResult> KhoaHocEdit(string id)
        {
            var khoahoc = await ttth.KhoaHoc.FirstOrDefaultAsync(kh => kh.MaKH == id);
            if (khoahoc == null)
            {
                return HttpNotFound("Không tìm thấy khóa học.");
            }
            ViewBag.MaChuongTrinhList = new SelectList(await ttth.ChuongTrinhHoc.ToListAsync(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
            return View(khoahoc);
        }

        [HttpPost]
        public async Task<ActionResult> KhoaHocEdit(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var kh = await ttth.KhoaHoc.FirstOrDefaultAsync(k => k.MaKH == khoahoc.MaKH);
                if (kh == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy khóa học.");
                    return View(khoahoc);
                }

                kh.TenKH = khoahoc.TenKH;
                kh.MoTa = khoahoc.MoTa;
                kh.NgayBatDau = khoahoc.NgayBatDau;
                kh.NgayKetThuc = khoahoc.NgayKetThuc;
                kh.HocPhi = khoahoc.HocPhi;
                kh.LoaiKH = khoahoc.LoaiKH;
                kh.Sobuoihoc = khoahoc.Sobuoihoc;
                kh.MaChuongTrinh = khoahoc.MaChuongTrinh;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        ViewBag.MaChuongTrinhList = new SelectList(await ttth.ChuongTrinhHoc.ToListAsync(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
                        return View(khoahoc);
                    }

                    var fileName = kh.MaKH + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), fileName);
                    imageFile.SaveAs(path);
                    kh.Anh = fileName;
                }

                await ttth.SaveChangesAsync();
                return RedirectToAction("KhoaHocList");
            }

            ViewBag.MaChuongTrinhList = new SelectList(await ttth.ChuongTrinhHoc.ToListAsync(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
            return View(khoahoc);
        }
    }
}
