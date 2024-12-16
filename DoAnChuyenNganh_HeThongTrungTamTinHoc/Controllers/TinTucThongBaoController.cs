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
        public ActionResult Index()
        {
            var tinTucThongBao = db.TinTucThongBao.Where(t => t.TrangThai == true).ToList();

            return View(tinTucThongBao);
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