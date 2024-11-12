using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminChuongTrinhHocController : Controller
    {
        // GET: Admin/AdminChuongTrinhHoc
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        private static Random random = new Random();
        private string mact = TaoMaChuongTrinh();


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

        public static string TaoMaChuongTrinh()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maHocVien = new StringBuilder("CT");

            for (int i = 0; i < 3; i++)
            {
                maHocVien.Append(chars[random.Next(chars.Length)]);
            }

            return maHocVien.ToString();
        }

        public ActionResult ChuongTrinhHocDelete(string id)
        {
            ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
            return View(chuongTrinhHoc);
        }
        [HttpPost]
        public ActionResult ChuongTrinhHocDelete(string id, ChuongTrinhHoc cth)
        {
            cth = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
            db.ChuongTrinhHoc.Remove(cth);
            db.SaveChanges();
            return RedirectToAction("ChuongTrinhHocList");
        }

        public ActionResult ChuongTrinhHocEdit(string id)
        {
            ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == id).FirstOrDefault();
            return View(chuongTrinhHoc);
        }
        [HttpPost]
        public ActionResult ChuongTrinhHocEdit(ChuongTrinhHoc cth)
        {
            ChuongTrinhHoc chuongTrinhHoc = db.ChuongTrinhHoc.Where(t => t.MaChuongTrinh == cth.MaChuongTrinh).FirstOrDefault();

            chuongTrinhHoc.TenChuongTrinh = cth.TenChuongTrinh;

            db.SaveChanges();
            return RedirectToAction("ChuongTrinhHocList");
        }
    }
}