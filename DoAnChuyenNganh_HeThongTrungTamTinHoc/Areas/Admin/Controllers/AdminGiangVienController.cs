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
    public class AdminGiangVienController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: Admin/AdminGiangVien
        public ActionResult GiangVienList(string search = "")
        {
            var ds = db.GiaoVien;

            List<GiaoVien> giangviens = db.GiaoVien.Where(e => e.HoTen.Contains(search)).ToList();
            ViewBag.Search = search;
            return View(giangviens);
        }
        string magv = TaoMaGiangVien();
        public ActionResult GiangVienAdd()
        {
            ViewBag.MaGV = magv;
            return View();
        }
        [HttpPost]
        public ActionResult GiangVienAdd(GiaoVien gv, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                GiaoVien giaovien = db.GiaoVien.Where(t => t.MaGV == magv).FirstOrDefault();
                if (giaovien != null)
                {
                    ModelState.AddModelError("MaGV", "Giáo viên đã tồn tại!!");
                    return View();
                }

                var emailDaTonTai = db.GiaoVien.Where(h => h.Email == gv.Email).FirstOrDefault();
                if (emailDaTonTai != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!!");
                    return View();
                }

                if (string.IsNullOrEmpty(gv.HoTen) 
                    && string.IsNullOrEmpty(gv.Anh) 
                    && string.IsNullOrEmpty(gv.NgayVaoLam.ToString()) 
                    && string.IsNullOrEmpty(gv.Anh) 
                    && string.IsNullOrEmpty(gv.BangCapGV) 
                    && string.IsNullOrEmpty(gv.LinhVucDaoTao) 
                    && string.IsNullOrEmpty(gv.Email) 
                    && string.IsNullOrEmpty(gv.SoDT) 
                    && string.IsNullOrEmpty(gv.DiaChi) 
                    && string.IsNullOrEmpty(gv.Luong.ToString()))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin của giảng viên!!!");
                    return View();
                }

                string filename = "noimage.jpg";
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
                        ModelState.AddModelError("Image", "Chỉ chấp nhận định dạng JPG hoặc PNG.");
                        return View();
                    }

                    // Tạo tên file ảnh từ mã giảng viên
                    filename = magv + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), filename);
                    imageFile.SaveAs(path);
                }

                // Lưu thông tin giảng viên vào CSDL
                giaovien = new GiaoVien
                {
                    MaGV = magv,
                    HoTen = gv.HoTen,
                    Anh = filename, // Nếu không có ảnh, `Anh` sẽ được lưu là chuỗi rỗng
                    NgayVaoLam = gv.NgayVaoLam,
                    BangCapGV = gv.BangCapGV,
                    LinhVucDaoTao = gv.LinhVucDaoTao,
                    Email = gv.Email,
                    SoDT = gv.SoDT,
                    DiaChi = gv.DiaChi,
                    Luong = gv.Luong
                };

                db.GiaoVien.Add(giaovien);
                db.SaveChanges();

                return RedirectToAction("GiangVienList");
            }
            else
            {
                return View();
            }
        }

        private static Random random = new Random();
        public static string TaoMaGiangVien()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maGiangVien = new StringBuilder("GV");

            for (int i = 0; i < 8; i++)
            {
                maGiangVien.Append(chars[random.Next(chars.Length)]);
            }

            return maGiangVien.ToString();
        }

        public ActionResult GiangVienDelete(string id)
        {
            GiaoVien gv = db.GiaoVien.Where(t => t.MaGV == id).FirstOrDefault();
            return View(gv);
        }
        [HttpPost]
        public ActionResult GiangVienDelete(string id, GiaoVien giaovien)
        {
            GiaoVien gv = db.GiaoVien.Where(t => t.MaGV == id).FirstOrDefault();
            db.GiaoVien.Remove(gv);
            db.SaveChanges();
            return RedirectToAction("GiangVienList");
        }

        public ActionResult GiangVienEdit(string id)
        {
            GiaoVien gv = db.GiaoVien.Where(t => t.MaGV == id).FirstOrDefault();
            return View(gv);
        }    
        [HttpPost]
        public ActionResult GiangVienEdit(GiaoVien gv, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                GiaoVien giangVienTonTai = db.GiaoVien.FirstOrDefault(h => h.MaGV == gv.MaGV);
                if (giangVienTonTai == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy giảng viên.");
                    return View(gv);
                }

                var emailDaTonTai = db.GiaoVien.Any(t => t.Email == gv.Email && t.MaGV != gv.MaGV);
                if (emailDaTonTai)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng!!");
                    return View(gv);
                }


                giangVienTonTai.HoTen = gv.HoTen;
                giangVienTonTai.NgayVaoLam = gv.NgayVaoLam;
                giangVienTonTai.BangCapGV = gv.BangCapGV;
                giangVienTonTai.LinhVucDaoTao = gv.LinhVucDaoTao;
                giangVienTonTai.Email = gv.Email;
                giangVienTonTai.SoDT = gv.SoDT;
                giangVienTonTai.DiaChi = gv.DiaChi;
                giangVienTonTai.Luong = gv.Luong;


                if (imageFile != null && imageFile.ContentLength > 0)
                {

                    if (imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Image", "Kích thước file không được lớn hơn 2MB.");
                        return View(gv);
                    }


                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx))
                    {
                        ModelState.AddModelError("Image", "Chỉ chấp nhận định JPG hoặc PNG.");
                        return View(gv);
                    }


                    if (!string.IsNullOrEmpty(giangVienTonTai.Anh))
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/AnhHocVien"), giangVienTonTai.Anh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }


                    var fileName = giangVienTonTai.MaGV + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);
                    imageFile.SaveAs(path);
                    giangVienTonTai.Anh = fileName;
                }


                db.SaveChanges();

                return RedirectToAction("GiangVienList");
            }

            return View(gv);
        }
    }
}