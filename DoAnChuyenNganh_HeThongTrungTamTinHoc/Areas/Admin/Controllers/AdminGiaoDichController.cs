using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using System.Net.Mail;
using System.Configuration;
using System.Text;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminGiaoDichController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: Admin/AdminGiaoDichHocPhi
        public ActionResult GiaoDichList(string search = "", string sortOrder = "trangthai")
        {
            var giaoDichHocPhi = db.GiaoDichHocPhi
                .Include(g => g.HocVien)
                .Include(g => g.PhuongThucThanhToan)
                .Include(g => g.KhoaHoc)
                .Where(g => g.HocVien.HoTen.Contains(search)
                         || g.KhoaHoc.TenKH.Contains(search)
                         || g.PhuongThucThanhToan.TenPT.Contains(search))
                .ToList();

            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "tenhocvien":
                    giaoDichHocPhi = giaoDichHocPhi.OrderBy(g => g.HocVien.HoTen).ToList();
                    break;
                case "tenkhoahoc":
                    giaoDichHocPhi = giaoDichHocPhi.OrderBy(g => g.KhoaHoc.TenKH).ToList();
                    break;
                case "phuongthucthanhtoan":
                    giaoDichHocPhi = giaoDichHocPhi.OrderBy(g => g.PhuongThucThanhToan.TenPT).ToList();
                    break;
                case "trangthai":
                    giaoDichHocPhi = giaoDichHocPhi
                        .OrderBy(g => g.TrangThai == "Chờ duyệt" ? 0 : 1)
                        .ThenBy(g => g.HocVien.HoTen)
                        .ToList();
                    break;
                default:
                    giaoDichHocPhi = giaoDichHocPhi.OrderBy(g => g.TrangThai == "Chờ duyệt" ? 0 : 1).ToList();
                    break;
            }

            return View(giaoDichHocPhi);
        }

        public ActionResult GiaoDichAdd()
        {
            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen");
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT");
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiaoDichAdd(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.GiaoDichHocPhi.Add(giaoDichHocPhi);
                db.SaveChanges();
                return RedirectToAction("GiaoDichList");
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public ActionResult GiaoDichEdit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GiaoDichHocPhi giaoDichHocPhi = db.GiaoDichHocPhi.Find(id);
            if (giaoDichHocPhi == null)
            {
                return HttpNotFound();
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiaoDichEdit(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(giaoDichHocPhi).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("GiaoDichList");
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public ActionResult GiaoDichDelete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiaoDichHocPhi giaoDichHocPhi = db.GiaoDichHocPhi.Find(id);
            if (giaoDichHocPhi == null)
            {
                return HttpNotFound();
            }
            return View(giaoDichHocPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiaoDichDeleteConfirmed(int id)
        {
            GiaoDichHocPhi giaoDichHocPhi =  db.GiaoDichHocPhi.Find(id);
            db.GiaoDichHocPhi.Remove(giaoDichHocPhi);
            db.SaveChanges();
            return RedirectToAction("GiaoDichList");
        }



        public async Task<ActionResult> DuyetTatCaGiaoDich()
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDichList = db.GiaoDichHocPhi.Where(gd => gd.TrangThai == "Chờ duyệt").ToList();

                if (giaoDichList.Any())
                {
                    foreach (var giaoDich in giaoDichList)
                    {
                        giaoDich.TrangThai = "Đã duyệt";

                        var chiTietHocVienLop = new ChiTiet_HocVien_LopHoc
                        {
                            MaHV = giaoDich.MaHV,
                            MaLH = db.LopHoc.FirstOrDefault(l => l.MaKH == giaoDich.MaKH)?.MaLH
                        };

                        if (chiTietHocVienLop.MaLH != null)
                        {
                            db.ChiTiet_HocVien_LopHoc.Add(chiTietHocVienLop);
                        }

                        // Chuyển sang gọi hàm bất đồng bộ
                        await SendEmailInternalAsync(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);
                    }

                    await db.SaveChangesAsync(); // Lưu dữ liệu bất đồng bộ
                    TempData["Message"] = "Tất cả giao dịch đã được duyệt.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không có giao dịch nào cần duyệt.";
                }
            }
            return RedirectToAction("GiaoDichList");
        }




        public async Task<ActionResult> TuChoiTatCaGiaoDich()
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDichList = db.GiaoDichHocPhi.Where(gd => gd.TrangThai == "Chờ duyệt").ToList();

                if (giaoDichList.Any())
                {
                    foreach (var giaoDich in giaoDichList)
                    {
                        giaoDich.TrangThai = "Từ chối";

                        // Gọi hàm gửi email bất đồng bộ
                        await SendEmailRejectedAsync(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);
                    }

                    await db.SaveChangesAsync(); // Lưu thay đổi bất đồng bộ
                    TempData["ErrorMessage"] = "Tất cả giao dịch đã bị từ chối.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không có giao dịch nào cần từ chối.";
                }
            }

            return RedirectToAction("GiaoDichList");
        }






        public async Task<ActionResult> DuyetGiaoDich(int id)
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDich = db.GiaoDichHocPhi.Find(id);
                if (giaoDich != null)
                {
                    giaoDich.TrangThai = "Đã duyệt";
                    db.SaveChanges();

                    var chiTietHocVienLop = new ChiTiet_HocVien_LopHoc
                    {
                        MaHV = giaoDich.MaHV,
                        MaLH = giaoDich.MaLH
                    };

                    if (chiTietHocVienLop.MaLH != null)
                    {
                        db.ChiTiet_HocVien_LopHoc.Add(chiTietHocVienLop);
                        db.SaveChanges();
                    }

                    await SendEmailInternalAsync(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);

                    TempData["Message"] = "Giao dịch đã được duyệt.";
                }
            }
            return RedirectToAction("GiaoDichList");
        }

        private async Task SendEmailInternalAsync(List<GiaoDichHocPhi> cart, string email)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học");
                var toAddress = new MailAddress(email);
                string subject = "Xác Nhận Thanh Toán Các Khóa Học";

                var courseDetails = new StringBuilder();
                double totalAmount = 0;

                foreach (var item in cart)
                {
                    courseDetails.AppendLine(
                        $"<tr><td>{item.MaHV}</td><td>{item.MaKH}</td><td>{(item.MaPT == 1 ? "Chuyển khoản" : "Khác")}</td><td>{item.NgayGD?.ToString("dd/MM/yyyy") ?? ""}</td><td>{item.SoTien:C}</td><td>{item.SoDT}</td><td>{item.Email}</td></tr>");
                    totalAmount += item.SoTien ?? 0;
                }

                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ padding: 20px; max-width: 800px; margin: auto; background-color: #f4f4f9; border-radius: 10px; }}
                        .header {{ text-align: center; background-color: #007BFF; color: white; padding: 15px; border-radius: 10px 10px 0 0; }}
                        .footer {{ text-align: center; font-size: 12px; color: #777; margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; }}
                        .content {{ margin: 20px 0; }}
                        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                        th, td {{ padding: 10px; border: 1px solid #ddd; text-align: left; }}
                        th {{ background-color: #f2f2f2; }}
                        td {{ background-color: #ffffff; }}
                        .total {{ font-size: 16px; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Xác Nhận Thanh Toán Các Khóa Học</h2>
                        </div>
                        <div class='content'>
                            <p>Xin chào,</p>
                            <p>Bạn đã thanh toán thành công cho các khóa học sau:</p>
                            <table>
                                <thead>
                                    <tr>
                                        <th>Mã Học Viên</th>
                                        <th>Mã Khóa Học</th>
                                        <th>Phương Thức Thanh Toán</th>
                                        <th>Ngày Giao Dịch</th>
                                        <th>Số Tiền</th>
                                        <th>Số Điện Thoại</th>
                                        <th>Email</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {courseDetails.ToString()}
                                </tbody>
                            </table>
                            <p class='total'>Tổng số tiền thanh toán: <b>{totalAmount:C}</b></p>
                            <p>Cảm ơn bạn đã tin tưởng Trung Tâm Tin Học của chúng tôi! Chúng tôi rất hân hạnh được gặp bạn trong buổi học sắp tới.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Trung Tâm Tin Học. Mọi thắc mắc vui lòng liên hệ chúng tôi.</p>
                        </div>
                    </div>
                </body>
                </html>";

                using (var smtpClient = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["SMTPHost"],
                    Port = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]),
                    EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, ConfigurationManager.AppSettings["FromEmailPassword"])
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Không thể gửi email: {ex.Message}");
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }









        public async Task<ActionResult> TuChoiGiaoDich(int id)
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDich = db.GiaoDichHocPhi.Find(id);
                if (giaoDich != null)
                {
                    giaoDich.TrangThai = "Từ chối";
                    await db.SaveChangesAsync();

                    await SendEmailRejectedAsync(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);

                    TempData["ErrorMessage"] = "Giao dịch đã bị từ chối.";
                }
            }
            return RedirectToAction("GiaoDichList");
        }

        private async Task SendEmailRejectedAsync(List<GiaoDichHocPhi> cart, string email)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học");
                var toAddress = new MailAddress(email);
                string subject = "Thông Báo Từ Chối Giao Dịch Thanh Toán";

                var courseDetails = new StringBuilder();
                foreach (var item in cart)
                {
                    courseDetails.AppendLine(
                        $"<tr><td>{item.MaHV}</td><td>{item.MaKH}</td><td>{(item.MaPT == 1 ? "Chuyển khoản" : "Khác")}</td><td>{item.NgayGD?.ToString("dd/MM/yyyy") ?? ""}</td><td>{item.SoTien:C}</td><td>{item.SoDT}</td></tr>");
                }

                string body = $@"
                <html>
                <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    .container {{ padding: 20px; max-width: 800px; margin: auto; background-color: #f4f4f9; border-radius: 10px; }}
                    .header {{ text-align: center; background-color: #FF5733; color: white; padding: 15px; border-radius: 10px 10px 0 0; }}
                    .footer {{ text-align: center; font-size: 12px; color: #777; margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; }}
                    .content {{ margin: 20px 0; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ padding: 10px; border: 1px solid #ddd; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    td {{ background-color: #ffffff; }}
                </style>
                </head>
                <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Thông Báo Từ Chối Giao Dịch</h2>
                    </div>
                    <div class='content'>
                        <p>Xin chào,</p>
                        <p>Rất tiếc, giao dịch thanh toán của bạn cho các khóa học sau đã bị từ chối:</p>
                        <table>
                            <thead>
                                <tr>
                                    <th>Mã Học Viên</th>
                                    <th>Mã Khóa Học</th>
                                    <th>Phương Thức Thanh Toán</th>
                                    <th>Ngày Giao Dịch</th>
                                    <th>Số Tiền</th>
                                    <th>Số Điện Thoại</th>
                                </tr>
                            </thead>
                            <tbody>
                                {courseDetails.ToString()}
                            </tbody>
                        </table>
                        <p>Vui lòng liên hệ với Trung Tâm Tin Học để biết thêm thông tin chi tiết hoặc thực hiện giao dịch lại.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2024 Trung Tâm Tin Học. Mọi thắc mắc vui lòng liên hệ chúng tôi.</p>
                    </div>
                </div>
                </body>
                </html>";

                using (var smtpClient = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["SMTPHost"],
                    Port = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]),
                    EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, ConfigurationManager.AppSettings["FromEmailPassword"])
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }

    }
}

