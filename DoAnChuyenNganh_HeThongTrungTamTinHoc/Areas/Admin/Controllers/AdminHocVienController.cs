using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminHocVienController : Controller
    {
        // GET: Admin/AdminHocVien
        TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string mahv = TaoMaHocVien();


        public ActionResult HocVienList(string search = "")
        {
            List<HocVien> hocvien = ttth.HocVien.Where(t => t.HoTen.Contains(search)).ToList();
            ViewBag.Search = search;
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

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Kiểm tra kích thước
                    if (imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Image", "Kích thước file không được lớn hơn 2MB.");
                        return View();
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx))
                    {
                        ModelState.AddModelError("Image", "Chỉ chấp nhận hình ảnh dạng JPG hoặc PNG.");
                        return View();
                    }
                    hocvien.Anh = "";
                    // Lưu thông tin vào CSDL
                    hv = new HocVien
                    {
                        MaHV = mahv,
                        HoTen = hocvien.HoTen,
                        Anh = hocvien.Anh,
                        NgaySinh = DateTime.Parse(hocvien.NgaySinh.ToString()),
                        GioiTinh = hocvien.GioiTinh,
                        Email = hocvien.Email,
                        SoDT = hocvien.SoDT,
                        DiaChi = hocvien.DiaChi
                    };
                    
                    ttth.HocVien.Add(hocvien);
                    ttth.SaveChanges();

                    // Truy vấn lại và đổi tên ảnh
                    HocVien Hocvien = ttth.HocVien.ToList().Last();
                    var fileName = Hocvien.MaHV.ToString() + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    imageFile.SaveAs(path);

                    Hocvien.Anh = fileName;
                    ttth.SaveChanges();
                }
                else
                {
                    // Lưu thông tin vào CSDL
                    hocvien.Anh = "";
                    ttth.HocVien.Add(hocvien);
                    ttth.SaveChanges();
                }
                return RedirectToAction("HocVienList");
            }
            else
            {
                return View();
            }
            
        }

        public static string TaoMaHocVien()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maHocVien = new StringBuilder("HV");

            for (int i = 0; i < 8; i++)
            {
                maHocVien.Append(chars[random.Next(chars.Length)]);
            }

            return maHocVien.ToString();
        }

        public ActionResult HocVienDelete(string id)
        {
            HocVien hv = ttth.HocVien.Where(t => t.MaHV == id).FirstOrDefault();
            return View(hv);
        }
        [HttpPost]
        public ActionResult HocVienDelete( string id, HocVien hocvien)
        {
            HocVien hv = ttth.HocVien.Where(t => t.MaHV == id).FirstOrDefault();
            ttth.HocVien.Remove(hv);
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
                HocVien existingHocVien = ttth.HocVien.FirstOrDefault(h => h.MaHV == hocvien.MaHV);
                if (existingHocVien == null)
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

              
                existingHocVien.HoTen = hocvien.HoTen;
                existingHocVien.NgaySinh = hocvien.NgaySinh;
                existingHocVien.GioiTinh = hocvien.GioiTinh;
                existingHocVien.Email = hocvien.Email;
                existingHocVien.SoDT = hocvien.SoDT;
                existingHocVien.DiaChi = hocvien.DiaChi;

           
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                   
                    if (imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Image", "Kích thước file không được lớn hơn 2MB.");
                        return View(hocvien);
                    }

                    
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx))
                    {
                        ModelState.AddModelError("Image", "Chỉ chấp nhận hình ảnh dạng JPG hoặc PNG.");
                        return View(hocvien);
                    }

                    
                    if (!string.IsNullOrEmpty(existingHocVien.Anh))
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/AnhHocVien"), existingHocVien.Anh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    
                    var fileName = existingHocVien.MaHV + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    imageFile.SaveAs(path);
                    existingHocVien.Anh = fileName;
                }

                
                ttth.SaveChanges();

                return RedirectToAction("HocVienList");
            }

            return View(hocvien);
        }

    }
}