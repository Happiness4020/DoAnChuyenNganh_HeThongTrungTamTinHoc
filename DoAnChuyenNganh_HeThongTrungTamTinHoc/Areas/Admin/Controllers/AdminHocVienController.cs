using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminHocVienController : Controller
    {
        // GET: Admin/AdminHocVien
        TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        public ActionResult HocVienList(string search = "")
        {
            List<HocVien> hocvien = ttth.HocVien.Where(t => t.HoTen.Contains(search)).ToList();
            ViewBag.Search = search;
            return View(hocvien);
        }
    }
}