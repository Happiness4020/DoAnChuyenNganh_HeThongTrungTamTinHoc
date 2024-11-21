using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminHocVienController : Controller
    {
        // GET: Admin/AdminHocVien
        TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string mahv = Utility.TaoMaNgauNhien("HV", 8);


        public ActionResult HocVienList(string search = "", int page = 1, int pageSize = 10)
        {
            List<HocVien> hocvien = ttth.HocVien.Where(t => t.HoTen.Contains(search)).ToList();
            ViewBag.Search = search;

            // phân trang
            int NoOfRecordPerPage = 7;
            int NoOfPage = (int)Math.Ceiling((double)hocvien.Count / NoOfRecordPerPage);
            int NoOfRecordToSkip = (page - 1) * NoOfRecordPerPage;

            ViewBag.Page = page;
            ViewBag.NoOfPage = NoOfPage;
            hocvien = hocvien.Skip(NoOfRecordToSkip).Take(NoOfRecordPerPage).ToList();
            return View(hocvien);
        }

        public ActionResult HocVienAdd()
        {
            ViewBag.MaHV = mahv;
            return View();
        }
        [HttpPost]
        public ActionResult HocVienAdd(HocVien hocvien, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                HocVien hv = ttth.HocVien.Where(t => t.MaHV == hocvien.MaHV).FirstOrDefault();

                if (hv != null)
                {
                    ModelState.AddModelError("MaHV", "Mã học viên đã tồn tại!!");
                    return View();
                }

                if (string.IsNullOrEmpty(hocvien.HoTen) && string.IsNullOrEmpty(hocvien.Anh)
                    && string.IsNullOrEmpty(hocvien.NgaySinh.ToString()) && string.IsNullOrEmpty(hocvien.GioiTinh)
                    && string.IsNullOrEmpty(hocvien.Email) && string.IsNullOrEmpty(hocvien.SoDT)
                    && string.IsNullOrEmpty(hocvien.DiaChi))
                {
                    ModelState.AddModelError("submit", "Vui lòng nhập đầy đủ thông tin của học viên !");
                    return View();
                }

                var emailExists = ttth.HocVien.Where(h => h.Email == hocvien.Email).FirstOrDefault();
                if (emailExists != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!!");
                    return View();
                }

                string filename = "noimage.jpg";

                if (imageFile != null && imageFile.ContentLength > 0)
                {

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }



                    // Truy vấn lại và đổi tên ảnh
                    HocVien Hocvien = ttth.HocVien.ToList().Last();
                    var fileName = Hocvien.MaHV.ToString() + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    imageFile.SaveAs(path);


                }
                // Lưu thông tin vào CSDL
                hv = new HocVien
                {
                    MaHV = mahv,
                    HoTen = hocvien.HoTen,
                    Anh = filename,
                    NgaySinh = DateTime.Parse(hocvien.NgaySinh.ToString("dd/MM/yyyy")),
                    GioiTinh = hocvien.GioiTinh,
                    Email = hocvien.Email,
                    SoDT = hocvien.SoDT,
                    DiaChi = hocvien.DiaChi
                };

                ttth.HocVien.Add(hv);
                ttth.SaveChanges();
                return RedirectToAction("HocVienList");
            }
            else
            {
                return View();
            }

        }

        public ActionResult HocVienDelete(string id)
        {
            HocVien hv = ttth.HocVien.Where(t => t.MaHV == id).FirstOrDefault();
            return View(hv);
        }
        [HttpPost]
        public ActionResult HocVienDelete(string id, HocVien hocvien)
        {

            hocvien = ttth.HocVien.Where(t => t.MaHV == id).FirstOrDefault();
            ttth.HocVien.Remove(hocvien);
            ttth.SaveChanges();
            return RedirectToAction("HocVienList");
        }

        public ActionResult HocVienEdit(string id)
        {
            HocVien hocvien = ttth.HocVien.FirstOrDefault(h => h.MaHV == id);
            if (hocvien == null)
            {
                return HttpNotFound("Không tìm thấy học viên.");
            }
            return View(hocvien);
        }

        [HttpPost]
        public ActionResult HocVienEdit(HocVien hocvien, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                HocVien hv = ttth.HocVien.FirstOrDefault(h => h.MaHV == hocvien.MaHV);
                if (hv == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy học viên.");
                    return View(hocvien);
                }



                var emailExists = ttth.HocVien.Any(h => h.Email == hocvien.Email && h.MaHV != hocvien.MaHV);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!!");
                    return View(hocvien);
                }


                hv.HoTen = hocvien.HoTen;
                hv.NgaySinh = hocvien.NgaySinh;
                hv.GioiTinh = hocvien.GioiTinh;
                hv.Email = hocvien.Email;
                hv.SoDT = hocvien.SoDT;
                hv.DiaChi = hocvien.DiaChi;


                string fileName = "noimage.jpg";
                if (imageFile != null && imageFile.ContentLength > 0)
                {

                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }


                    if (!string.IsNullOrEmpty(hv.Anh))
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/AnhHocVien"), hv.Anh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }


                    fileName = hv.MaHV + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    imageFile.SaveAs(path);
                    hv.Anh = fileName;
                }
                else
                {
                    hv.Anh = fileName;
                }


                ttth.SaveChanges();

                return RedirectToAction("HocVienList");
            }
            else
            {
                return View();
            }

        }

    }
}