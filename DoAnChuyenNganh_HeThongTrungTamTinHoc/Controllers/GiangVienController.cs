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
            //var lichDayList = new List<LichDay>
            //{
            //    new LichDay { Ngay = DateTime.Now, Gio = "08:00 - 10:00", MonHoc = "Toán Cao Cấp", PhongHoc = "Phòng A101" },
            //    new LichDay { Ngay = DateTime.Now.AddDays(2), Gio = "10:00 - 12:00", MonHoc = "Lập Trình C", PhongHoc = "Phòng B201" }
            //};

            return View();
        }
        //
        public ActionResult lich()
        {
            var lichDayList = new List<LichDay>
            {
                new LichDay { Ngay = DateTime.Now, Gio = "08:00 - 10:00", MonHoc = "Toán Cao Cấp", PhongHoc = "Phòng A101" },
                new LichDay { Ngay = DateTime.Now.AddDays(2), Gio = "10:00 - 12:00", MonHoc = "Lập Trình C", PhongHoc = "Phòng B201" }
            };

            return View(lichDayList);
        }

    }
}