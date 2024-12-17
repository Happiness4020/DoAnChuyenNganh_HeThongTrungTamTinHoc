using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class TinTucThongBaoController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: TinTucThongBao
        public ActionResult Index(string search = "", string sortOrder = "ngaytao", int page = 1, int pageSize = 6)
        {
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            var tintucthongbaoQuery = db.TinTucThongBao.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                tintucthongbaoQuery = tintucthongbaoQuery.Where(tt => tt.TieuDe.Contains(search));
            }

            switch (sortOrder)
            {
                case "ngaytao":
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderByDescending(tt => tt.NgayTao);
                    break;
                default:
                    tintucthongbaoQuery = tintucthongbaoQuery.OrderByDescending(tt => tt.NgayTao);
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

        public ActionResult ChiTiet(int id)
        {
            var tinTuc = db.TinTucThongBao.FirstOrDefault(t => t.ID == id && t.TrangThai == true);

            if (tinTuc == null)
            {
                return HttpNotFound();
            }

            return View(tinTuc);
        }
    }
}