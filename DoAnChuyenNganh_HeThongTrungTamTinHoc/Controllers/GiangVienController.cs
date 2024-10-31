using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class GiangVienController : Controller
    {
        // GET: GiangVien
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LichDay()
        {
            return View();
        }
       
    }
}