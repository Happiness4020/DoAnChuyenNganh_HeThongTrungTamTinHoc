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
using System.Drawing;

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
            try
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

                    // Tiêu đề
                    worksheet.Cells["A1"].Value = "BÁO CÁO DOANH THU THEO THÁNG";
                    worksheet.Cells["A1:C1"].Merge = true;
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Tiêu đề cột
                    worksheet.Cells[4, 1].Value = "Tháng/Năm";
                    worksheet.Cells[4, 2].Value = "Tổng Số Học Viên Đăng Ký";
                    worksheet.Cells[4, 3].Value = "Tổng Doanh Thu (VNĐ)";
                    worksheet.Cells[4, 1, 4, 3].Style.Font.Bold = true;
                    worksheet.Cells[4, 1, 4, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[4, 1, 4, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[4, 1, 4, 3].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                    // Dữ liệu
                    for (int i = 0; i < thongKeTheoThang.Count; i++)
                    {
                        worksheet.Cells[i + 5, 1].Value = thongKeTheoThang[i].ThangNam;
                        worksheet.Cells[i + 5, 2].Value = thongKeTheoThang[i].TongSoHocVien;
                        worksheet.Cells[i + 5, 3].Value = thongKeTheoThang[i].TongTien;

                        worksheet.Cells[i + 5, 1, i + 5, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Dòng tổng cộng
                    int totalRow = thongKeTheoThang.Count + 5;
                    worksheet.Cells[totalRow, 1].Value = "Tổng Cộng";
                    worksheet.Cells[totalRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[totalRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[totalRow, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells[totalRow, 2].Value = tongSoHocVien;
                    worksheet.Cells[totalRow, 2].Style.Font.Bold = true;
                    worksheet.Cells[totalRow, 3].Value = tongDoanhThu;
                    worksheet.Cells[totalRow, 3].Style.Font.Bold = true;
                    worksheet.Cells[totalRow, 1, totalRow, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    // Định dạng số tiền
                    worksheet.Cells[5, 3, totalRow, 3].Style.Numberformat.Format = "#,##0";

                    // Thêm viền cho tất cả các ô
                    worksheet.Cells[4, 1, totalRow, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[4, 1, totalRow, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[4, 1, totalRow, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[4, 1, totalRow, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    // Căn chỉnh cột
                    worksheet.Cells[4, 1, totalRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[4, 1, totalRow, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Điều chỉnh độ rộng cột
                    worksheet.Column(1).Width = 15; // Tháng/Năm
                    worksheet.Column(2).Width = 30; // Tổng Số Học Viên Đăng Ký
                    worksheet.Column(3).Width = 25; // Tổng Doanh Thu

                    // Xuất file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"ThongKeGiaoDich_{DateTime.Now:dd_MM_yyyy_HHmmss}.xlsx";
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    return File(stream, contentType, fileName);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo!!! Hãy thử lại sau";
                return RedirectToAction("ThongKeGiaoDich");
            }
        }


    }
}
