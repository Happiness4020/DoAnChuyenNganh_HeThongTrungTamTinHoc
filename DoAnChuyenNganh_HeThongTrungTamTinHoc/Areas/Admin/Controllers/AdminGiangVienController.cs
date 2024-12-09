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
    public class AdminGiangVienController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        string magv = Utility.TaoMaNgauNhien("GV", 8);
        // GET: Admin/AdminGiangVien
        public ActionResult GiangVienList(string search = "", string sortOrder = "ten", int page = 1, int pageSize = 10)
        {
            var giangviensQuery = db.GiaoVien.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                giangviensQuery = giangviensQuery.Where(gv => gv.HoTen.Contains(search));
            }

            switch (sortOrder)
            {
                case "ten":
                    giangviensQuery = giangviensQuery.OrderBy(gv => gv.HoTen);
                    break;
                case "magiangvien":
                    giangviensQuery = giangviensQuery.OrderBy(gv => gv.MaGV);
                    break;
                case "linhvuc":
                    giangviensQuery = giangviensQuery.OrderBy(gv => gv.LinhVucDaoTao);
                    break;
                default:
                    giangviensQuery = giangviensQuery.OrderBy(gv => gv.HoTen);
                    break;
            }

            int totalRecords = giangviensQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int skipRecords = (page - 1) * pageSize;

            var giangviens = giangviensQuery.Skip(skipRecords).Take(pageSize).ToList();

            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(giangviens);
        }

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
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }

                    filename = magv + fileEx;
                    var path = Path.Combine(Server.MapPath("~/AnhHocVien"), filename);
                    imageFile.SaveAs(path);
                }

                giaovien = new GiaoVien
                {
                    MaGV = magv,
                    HoTen = gv.HoTen,
                    Anh = filename, 
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

        public ActionResult GiangVienDelete(string id)
        {
            GiaoVien gv = db.GiaoVien.Where(t => t.MaGV == id).FirstOrDefault();
            return View(gv);
        }
        [HttpPost]
        public ActionResult GiangVienDelete(string id, GiaoVien giaovien)
        {
            try
            {
                GiaoVien gv = db.GiaoVien.Where(t => t.MaGV == id).FirstOrDefault();
                db.GiaoVien.Remove(gv);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa học viên thành công";
                return RedirectToAction("GiangVienList");
            }
            catch
            {
                TempData["ErrorMessage"] = "Dữ liệu giảng viên này vẫn đang được lưu tại nơi khác!!! Không thể xóa";
                return RedirectToAction("GiangVienList");
            }
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
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
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