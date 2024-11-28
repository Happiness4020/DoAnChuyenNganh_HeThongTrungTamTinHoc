using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
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
        public ActionResult KhoaHocList(string search = "", int page = 1, int pageSize = 10, string sortOrder = "tenkh")
        {
            var khoahoc = ttth.KhoaHoc
                .Where(kh => kh.TenKH.Contains(search))
                .ToList();

            ViewBag.Search = search; // Giữ giá trị tìm kiếm
            ViewBag.SortOrder = sortOrder; // Giữ giá trị sắp xếp

            // Sắp xếp theo sortOrder
            switch (sortOrder)
            {
                case "tenkh":
                    khoahoc = khoahoc.OrderBy(kh => kh.TenKH).ToList();
                    break;
                case "ngaybatdau":
                    khoahoc = khoahoc.OrderBy(kh => kh.NgayBatDau).ToList();
                    break;
                case "hocphi":
                    khoahoc = khoahoc.OrderBy(kh => kh.HocPhi).ToList();
                    break;
                default:
                    khoahoc = khoahoc.OrderBy(kh => kh.TenKH).ToList();
                    break;
            }

            // Phân trang
            int totalRecords = khoahoc.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            khoahoc = khoahoc.Skip(recordsToSkip).Take(pageSize).ToList();

            return View(khoahoc);
        }

        public ActionResult DanhSachHocVienThamGiaKhoaHoc()
        {
            var khoaHocList = ttth.KhoaHoc.ToList();
            var khoaHocViewModelList = new List<KhoaHocViewModel>();

            foreach (var khoaHoc in khoaHocList)
            {
                var soHocVien = ttth.GiaoDichHocPhi
                                    .Where(gd => gd.MaKH == khoaHoc.MaKH)
                                    .Select(gd => gd.MaHV)
                                    .Distinct()
                                    .Count();

                bool moLop = soHocVien >= 20;

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

        public ActionResult KhoaHocAdd()
        {
            ViewBag.MaChuongTrinh = new SelectList(ttth.ChuongTrinhHoc.ToList(), "MaChuongTrinh", "TenChuongTrinh");
            ViewBag.MaKH = makh;
            return View();
        }

        [HttpPost]
        public ActionResult KhoaHocAdd(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(khoahoc.MaChuongTrinh))
            {
                ModelState.AddModelError("MaChuongTrinh", "Vui lòng chọn chương trình");
            }

            ViewBag.MaChuongTrinh = new SelectList(ttth.ChuongTrinhHoc.ToList(), "MaChuongTrinh", "TenChuongTrinh");

            if (ModelState.IsValid)
            {
                var existingKhoaHoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == khoahoc.MaKH);
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
                ttth.SaveChanges();
                return RedirectToAction("KhoaHocList");
            }
            return View();
        }

        public ActionResult KhoaHocDelete(string id)
        {
            var khoahoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == id);
            if (khoahoc == null)
            {
                return HttpNotFound("Không tìm thấy khóa học.");
            }
            return View(khoahoc);
        }

        [HttpPost]
        public ActionResult KhoaHocDelete(string id, KhoaHoc khoahoc)
        {
            khoahoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == id);
            if (khoahoc != null)
            {
                ttth.KhoaHoc.Remove(khoahoc);
                ttth.SaveChanges();
            }
            return RedirectToAction("KhoaHocList");
        }

        public ActionResult KhoaHocEdit(string id)
        {
            var khoahoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == id);
            if (khoahoc == null)
            {
                return HttpNotFound("Không tìm thấy khóa học.");
            }
            ViewBag.MaChuongTrinhList = new SelectList(ttth.ChuongTrinhHoc.ToList(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
            return View(khoahoc);
        }

        [HttpPost]
        public ActionResult KhoaHocEdit(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var kh = ttth.KhoaHoc.FirstOrDefault(k => k.MaKH == khoahoc.MaKH);
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
                        ViewBag.MaChuongTrinhList = new SelectList(ttth.ChuongTrinhHoc.ToList(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
                        return View(khoahoc);
                    }

                    var fileName = kh.MaKH + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), fileName);
                    imageFile.SaveAs(path);
                    kh.Anh = fileName;
                }

                ttth.SaveChanges();
                return RedirectToAction("KhoaHocList");
            }

            ViewBag.MaChuongTrinhList = new SelectList(ttth.ChuongTrinhHoc.ToList(), "MaChuongTrinh", "TenChuongTrinh", khoahoc.MaChuongTrinh);
            return View(khoahoc);
        }
    }
}
