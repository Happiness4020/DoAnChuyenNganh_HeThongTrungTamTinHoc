using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminTinTucThongBaoController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: Admin/AdminTinTucThongBao
        public ActionResult DanhSachTinTucThongBao(string search = "", string sortOrder = "tieude", int page = 1, int pageSize = 10)
        {
            var tintucthongbaoQuery = db.TinTucThongBao.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                tintucthongbaoQuery = tintucthongbaoQuery.Where(tt => tt.TieuDe.Contains(search));
            }

            switch (sortOrder)
            {
                case "tieude":
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderBy(tt => tt.TieuDe);
                    break;
                case "noidung":
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderBy(tt => tt.NoiDung);
                    break;
                case "ngaytao":
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderBy(tt => tt.NgayTao);
                    break;
                default:
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderBy(tt => tt.NgayTao);
                    break;
            }

            int totalRecords = tintucthongbaoQuery.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            int recordsToSkip = (page - 1) * pageSize;

            var dstt = tintucthongbaoQuery.Skip(recordsToSkip).Take(pageSize).ToList();

            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(dstt);
        }

        public ActionResult TinTucThongBaoAdd()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TinTucThongBaoAdd(TinTucThongBao tt, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(tt.TieuDe)
                    && string.IsNullOrEmpty(tt.Anh)
                    && string.IsNullOrEmpty(tt.NoiDung)
                    && string.IsNullOrEmpty(tt.LoaiTin))
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

                    filename = imageFile.FileName;
                    var path = Path.Combine(Server.MapPath("~/images"), filename);
                    imageFile.SaveAs(path);
                }

                DateTime truncatedToSecond = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                try
                {
                    TinTucThongBao tintuc = new TinTucThongBao
                    {
                        Anh = filename,
                        TieuDe = tt.TieuDe,
                        NoiDung = tt.NoiDung,
                        LoaiTin = tt.LoaiTin,
                        NgayTao = truncatedToSecond,
                        TrangThai = true
                    };

                    db.TinTucThongBao.Add(tintuc);
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng bài: " + ex;
                    return RedirectToAction("DanhSachTinTucThongBao");
                }

                TempData["SuccessMessage"] = "Đã đăng bài thành công";
                return RedirectToAction("DanhSachTinTucThongBao");
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng bài!!! Hãy thử lại sau";
                return RedirectToAction("DanhSachTinTucThongBao");
            }
        }

        public ActionResult TinTucThongBaoEdit(int id)
        {
            TinTucThongBao tt = db.TinTucThongBao.FirstOrDefault(t => t.ID == id);
            if (tt == null)
            {
                return HttpNotFound("Không tìm thấy học viên.");
            }
            return View(tt);
        }
        [HttpPost]
        public ActionResult TinTucThongBaoEdit(TinTucThongBao tintuc, HttpPostedFileBase imageFile)
        {
            try
            {
                TinTucThongBao tt = db.TinTucThongBao.FirstOrDefault(t => t.ID == tintuc.ID);
                if (tt == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài đăng!!!";
                    return RedirectToAction("TinTucThongBaoEdit", new { tintuc.ID });
                }

                tt.TieuDe = tintuc.TieuDe;
                tt.NoiDung = tintuc.NoiDung;
                tt.LoaiTin = tintuc.LoaiTin;
                tt.TrangThai = tintuc.TrangThai;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".png" };
                    var fileEx = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileEx) || imageFile.ContentLength > 2000000)
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận hình ảnh JPG hoặc PNG và không lớn hơn 2MB.");
                        return View();
                    }

                    string fileName = tt.ID + fileEx;
                    var path = Path.Combine(Server.MapPath("~/images"), fileName);
                    imageFile.SaveAs(path);
                    tt.Anh = fileName;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã sửa bài đăng thành công";
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi sửa bài đăng: " + ex;
            }
            return RedirectToAction("TinTucThongBaoEdit", new { tintuc.ID });
        }

        public ActionResult TinTucThongBaoDelete(int id)
        {
            var tintuc = db.TinTucThongBao.FirstOrDefault(t => t.ID == id);
            if(tintuc == null)
            {
                TempData["ErrorMessage"] = "Bài đăng không tồn tại";
                return RedirectToAction("DanhSachTinTucThongBao");
            }
            return View(tintuc);
        }
        [HttpPost]
        public ActionResult TinTucThongBaoDelete(int id, TinTucThongBao tintuc)
        {
            try
            {
                tintuc = db.TinTucThongBao.Where(t => t.ID == id).FirstOrDefault();
                db.TinTucThongBao.Remove(tintuc);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa bài đăng thành công";
            }   
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bài đăng: " + ex;
            }
            return RedirectToAction("DanhSachTinTucThongBao");
        }    
    }
}