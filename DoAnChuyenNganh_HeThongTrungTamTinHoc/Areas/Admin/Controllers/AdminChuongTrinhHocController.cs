using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminChuongTrinhHocController : Controller
    {
        // GET: Admin/AdminChuongTrinhHoc
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string mact = Utility.TaoMaNgauNhien("CT", 3);


        public ActionResult ChuongTrinhHocList(string search = "")
        {
            List<ChuongTrinhHoc> chuongTrinhHocs = db.ChuongTrinhHoc.Where(cth => cth.TenChuongTrinh.Contains(search)).ToList();
            ViewBag.Search = search;

            return View(chuongTrinhHocs);
        }


        public ActionResult ChuongTrinhHocAdd()
        {
            ViewBag.MaCT = mact;
            return View();
        }
        [HttpPost]
        public ActionResult ChuongTrinhHocAdd(ChuongTrinhHoc cth)
        {
            if (string.IsNullOrEmpty(cth.TenChuongTrinh))
            {
                ModelState.AddModelError("TenChuongTrinh", "Vui lòng nhập tên chương trình");
                return View();
            }
            if (ModelState.IsValid)
            {
                ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == cth.MaChuongTrinh).FirstOrDefault();
               

                var tenCT = db.ChuongTrinhHoc.Where(t => t.TenChuongTrinh == cth.TenChuongTrinh).FirstOrDefault();
                if (tenCT != null)
                {
                    ModelState.AddModelError("TenChuongTrinh", "Tên chương trình đã tồn tại!!");
                    return View();
                }

                chuongTrinhHoc = new ChuongTrinhHoc
                {
                    MaChuongTrinh = mact,
                    TenChuongTrinh = cth.TenChuongTrinh
                };

                db.ChuongTrinhHoc.Add(chuongTrinhHoc);
                db.SaveChanges();
                return RedirectToAction("ChuongTrinhHocList");
            }
            else
            {
                return View();
            }    
        }


        public ActionResult ChuongTrinhHocDelete(string id)
        {
            ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
            return View(chuongTrinhHoc);
        }
        [HttpPost]
        public ActionResult ChuongTrinhHocDelete(string id, ChuongTrinhHoc cth)
        {
            try
            {
                cth = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
                db.ChuongTrinhHoc.Remove(cth);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa chương trình học thành công";
                return RedirectToAction("ChuongTrinhHocList");
            }
            catch
            {
                TempData["ErrorMessage"] = "Chương trình học vẫn đang được duy trì!!! Không thể xóa";
                return RedirectToAction("ChuongTrinhHocList");
            }
        }

        public ActionResult ChuongTrinhHocEdit(string id)
        {
            ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
            return View(chuongTrinhHoc);
        }
        [HttpPost]
        public ActionResult ChuongTrinhHocEdit(ChuongTrinhHoc cth)
        {
            try
            {
                ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == cth.MaChuongTrinh).FirstOrDefault();

                var tenct = db.ChuongTrinhHoc.FirstOrDefault(ct => ct.TenChuongTrinh == cth.TenChuongTrinh);
                if (tenct != null)
                {
                    TempData["ErrorMessage"] = "Chương trình đã tồn tại!!!";
                    return RedirectToAction("ChuongTrinhHocEdit");
                }
                else
                {
                    chuongTrinhHoc.TenChuongTrinh = cth.TenChuongTrinh;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đã cập nhật chương trình học thành công";
                    return RedirectToAction("ChuongTrinhHocList");
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật chương trình học: " + ex;
                return RedirectToAction("ChuongTrinhHocEdit");
            }
        }
    }
}