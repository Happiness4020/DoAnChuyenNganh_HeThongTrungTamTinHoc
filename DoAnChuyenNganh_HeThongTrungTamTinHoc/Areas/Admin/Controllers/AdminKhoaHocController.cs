using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using PagedList;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminKhoaHocController : Controller
    {
        private TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string makh = TaoMaKhoaHoc();

        // GET: Admin/AdminKhoaHoc
        public ActionResult KhoaHocList(string search = "", int page = 1, int pageSize = 10)
        {
            var khoahoc = ttth.KhoaHoc.Where(kh => kh.TenKH.Contains(search)).ToList();


            ViewBag.Search = search;

            // phân trang
            int NoOfRecordPerPage = 7;
            int NoOfPage = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(khoahoc.Count) / Convert.ToDouble(NoOfRecordPerPage)));
            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;
            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPage;
            khoahoc = khoahoc.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();
            return View(khoahoc);
        }

        public ActionResult KhoaHocAdd()
        {
            // Lấy danh sách các chương trình học từ database
            var chuongTrinhHocList = ttth.ChuongTrinhHoc.Select(ct => new SelectListItem
            {
                Value = ct.MaChuongTrinh.ToString(),
                Text = ct.TenChuongTrinh
            }).ToList();

            // Truyền danh sách này vào ViewBag với khóa 'MaChuongTrinh'
            ViewBag.MaChuongTrinh = chuongTrinhHocList;
            ViewBag.MaKH = makh;
            return View();
        }

        [HttpPost]
        public ActionResult KhoaHocAdd(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingKhoaHoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == khoahoc.MaKH);
                if (existingKhoaHoc != null)
                {
                    ModelState.AddModelError("MaKH", "Mã khóa học đã tồn tại!");
                    return View();
                }

                // Kiểm tra file hình ảnh (nếu có)
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Image", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }

                    // Lưu ảnh vào thư mục và đặt tên file theo mã khóa học
                    var fileName = khoahoc.MaKH + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), fileName);
                    imageFile.SaveAs(path);
                    khoahoc.Anh = fileName;
                }
                else
                {
                    khoahoc.Anh = "noimage.jpg"; // Đặt mặc định nếu không có hình
                }

                ttth.KhoaHoc.Add(khoahoc);
                ttth.SaveChanges();
                return RedirectToAction("KhoaHocList");
            }
            return View();
        }


        public static string TaoMaKhoaHoc()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maHocVien = new StringBuilder("KH");

            for (int i = 0; i < 3; i++)
            {
                maHocVien.Append(chars[random.Next(chars.Length)]);
            }

            return maHocVien.ToString();
        }


        public ActionResult KhoaHocEdit(string id)
        {
            var khoahoc = ttth.KhoaHoc.FirstOrDefault(kh => kh.MaKH == id);
            if (khoahoc == null)
            {
                return HttpNotFound("Không tìm thấy khóa học.");
            }
            // Lấy danh sách các chương trình học và đánh dấu mục đã chọn
            var chuongTrinhHocList = ttth.ChuongTrinhHoc.Select(ct => new SelectListItem
            {
                Value = ct.MaChuongTrinh.ToString(),
                Text = ct.TenChuongTrinh,
                Selected = ct.MaChuongTrinh == khoahoc.MaChuongTrinh // Đánh dấu mục đã chọn
            }).ToList();

            // Truyền danh sách này vào ViewBag với khóa 'MaChuongTrinh'
            ViewBag.MaChuongTrinh = chuongTrinhHocList;
            return View(khoahoc);
        }

        [HttpPost]
        public ActionResult KhoaHocEdit(KhoaHoc khoahoc, HttpPostedFileBase imageFile)
        {
            // Cập nhật lại ViewBag.MaChuongTrinh khi POST
            ViewBag.MaChuongTrinh = ttth.ChuongTrinhHoc.Select(ct => new SelectListItem
            {
                Value = ct.MaChuongTrinh.ToString(),
                Text = ct.TenChuongTrinh,
                Selected = ct.MaChuongTrinh == khoahoc.MaChuongTrinh // Đánh dấu mục đã chọn
            }).ToList();

            if (ModelState.IsValid)
            {
                var kh = ttth.KhoaHoc.FirstOrDefault(k => k.MaKH == khoahoc.MaKH);
                if (kh == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy khóa học.");
                    return View(khoahoc);
                }

                // Kiểm tra tên và các thông tin khác
                kh.TenKH = khoahoc.TenKH;
                kh.MoTa = khoahoc.MoTa;
                kh.NgayBatDau = khoahoc.NgayBatDau;
                kh.NgayKetThuc = khoahoc.NgayKetThuc;
                kh.HocPhi = khoahoc.HocPhi;
                kh.LoaiKH = khoahoc.LoaiKH;
                kh.MaChuongTrinh = khoahoc.MaChuongTrinh;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Image", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View(khoahoc);
                    }

                    // Xóa ảnh cũ và lưu ảnh mới
                    var oldImagePath = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), kh.Anh);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    var fileName = kh.MaKH + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhKhoaHoc"), fileName);
                    imageFile.SaveAs(path);
                    kh.Anh = fileName;
                }

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
    }
}
