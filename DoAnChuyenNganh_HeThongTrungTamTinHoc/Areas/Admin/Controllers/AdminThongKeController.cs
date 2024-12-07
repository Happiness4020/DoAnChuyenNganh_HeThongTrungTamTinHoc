using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;

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

            if (fromDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD <= toDate.Value);
            }

            var giaoDichList = giaoDichQuery.ToList();

            ViewBag.TongTien = giaoDichList.Any() ? giaoDichList.Sum(g => g.SoTien ?? 0) : 0;
            ViewBag.SoNguoiDangKy = giaoDichList.Any() ? giaoDichList.Count() : 0;

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


        public ActionResult ExportToExcel(DateTime? fromDate, DateTime? toDate)
        {
            var giaoDichQuery = db.GiaoDichHocPhi.AsQueryable();

            if (fromDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                giaoDichQuery = giaoDichQuery.Where(gd => gd.NgayGD <= toDate.Value);
            }

            var giaoDichList = giaoDichQuery.ToList();

            var thongKeTheoThang = giaoDichList
                .GroupBy(gd => new { gd.NgayGD.Value.Year, gd.NgayGD.Value.Month })
                .Select(g => new
                {
                    ThangNam = g.Key.Month + "/" + g.Key.Year,
                    TongSoHocVien = g.Count(),
                    TongTien = g.Sum(x => x.SoTien ?? 0)
                })
                .ToList();

            var tongSoHocVien = thongKeTheoThang.Sum(x => x.TongSoHocVien);
            var tongDoanhThu = thongKeTheoThang.Sum(x => x.TongTien);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thống Kê Doanh Thu");

                worksheet.Cells["A1"].Value = "BÁO CÁO DOANH THU THEO THÁNG";
                worksheet.Cells["A1:C1"].Merge = true; // Gộp ô
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[4, 1].Value = "Tháng/Năm";
                worksheet.Cells[4, 2].Value = "Tổng Số Học Viên Đăng Ký";
                worksheet.Cells[4, 3].Value = "Tổng Doanh Thu (VNĐ)";

                worksheet.Cells[4, 1, 4, 3].Style.Font.Bold = true;
                worksheet.Cells[4, 1, 4, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                for (int i = 0; i < thongKeTheoThang.Count; i++)
                {
                    worksheet.Cells[i + 5, 1].Value = thongKeTheoThang[i].ThangNam;
                    worksheet.Cells[i + 5, 2].Value = thongKeTheoThang[i].TongSoHocVien;
                    worksheet.Cells[i + 5, 3].Value = thongKeTheoThang[i].TongTien.ToString("N0");
                }

                int totalRow = thongKeTheoThang.Count + 5;
                worksheet.Cells[totalRow, 1].Value = "Tổng Cộng";
                worksheet.Cells[totalRow, 1].Style.Font.Bold = true;
                worksheet.Cells[totalRow, 2].Value = tongSoHocVien;
                worksheet.Cells[totalRow, 2].Style.Font.Bold = true;
                worksheet.Cells[totalRow, 3].Value = tongDoanhThu.ToString("N0");
                worksheet.Cells[totalRow, 3].Style.Font.Bold = true;

                worksheet.Cells[5, 3, totalRow, 3].Style.Numberformat.Format = "#,##0 VNĐ";

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"ThongKeGiaoDich_{DateTime.Now:dd_MM_yyyy_HHmmss}.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }







        //public ActionResult ThongKeKhoaHoc()
        //{
        //    // Truy xuất danh sách khóa học và số lượng học viên đăng ký
        //    var thongKeKhoaHoc = db.KhoaHoc
        //        .Select(kh => new ThongKeKhoaHocViewModel
        //        {
        //            TenKhoaHoc = kh.TenKH,
        //            SoLuongDangKy = kh.LopHoc.SelectMany(lh => lh.ChiTiet_HocVien_LopHoc).Count()
        //        })
        //        .ToList();

        //    // Tìm khóa học đăng ký nhiều nhất và ít nhất
        //    ViewBag.KhoaHocMax = thongKeKhoaHoc.OrderByDescending(kh => kh.SoLuongDangKy).FirstOrDefault();
        //    ViewBag.KhoaHocMin = thongKeKhoaHoc.OrderBy(kh => kh.SoLuongDangKy).FirstOrDefault();

        //    // Gửi dữ liệu sang View
        //    return View(thongKeKhoaHoc);
        //}


    }

}
