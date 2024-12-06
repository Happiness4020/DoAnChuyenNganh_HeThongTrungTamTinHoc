using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminThongKeController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: Admin/AdminThongKe
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ThongKeGiaoDich(DateTime? fromDate, DateTime? toDate)
        {
            var giaoDichQuery = db.GiaoDichHocPhi.AsQueryable();

            // Lọc theo ngày tháng nếu có
            if (fromDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD <= toDate.Value);
            }

            // Kiểm tra dữ liệu trước khi tính tổng và đếm
            var giaoDichList = giaoDichQuery.ToList(); // Chuyển thành danh sách để tránh truy vấn nhiều lần

            ViewBag.TongTien = giaoDichList.Any() ? giaoDichList.Sum(g => g.SoTien ?? 0) : 0;
            ViewBag.SoNguoiDangKy = giaoDichList.Any() ? giaoDichList.Count() : 0;

            // Thống kê theo tháng
            var thongKeTheoThang = giaoDichList
                .GroupBy(gd => new { gd.NgayGD.Value.Year, gd.NgayGD.Value.Month })
                .Select(g => new ThongKeGiaoDichViewModel
                {
                    ThangNam = g.Key.Month + "/" + g.Key.Year,
                    TongTien = g.Sum(x => x.SoTien ?? 0),
                    SoNguoiDangKy = g.Count()
                })
                .ToList();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(thongKeTheoThang);
        }

    }

}
