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
        public async Task<ActionResult> GiaoDichList()
        {
            var giaoDichHocPhi = db.GiaoDichHocPhi.Include(g => g.HocVien).Include(g => g.PhuongThucThanhToan).Include(g => g.KhoaHoc);
            return View(await giaoDichHocPhi.ToListAsync());
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
        public async Task<ActionResult> GiaoDichAdd(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.GiaoDichHocPhi.Add(giaoDichHocPhi);
                await db.SaveChangesAsync();
                return RedirectToAction("GiaoDichList");
            }

            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public async Task<ActionResult> GiaoDichEdit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
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
        public async Task<ActionResult> GiaoDichEdit(GiaoDichHocPhi giaoDichHocPhi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(giaoDichHocPhi).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("GiaoDichList");
            }

            // Gán lại danh sách nếu ModelState không hợp lệ
            ViewBag.HocVienList = new SelectList(db.HocVien, "MaHV", "HoTen", giaoDichHocPhi.MaHV);
            ViewBag.PhuongThucList = new SelectList(db.PhuongThucThanhToan, "MaPT", "TenPT", giaoDichHocPhi.MaPT);
            ViewBag.KhoaHocList = new SelectList(db.KhoaHoc, "MaKH", "TenKH", giaoDichHocPhi.MaKH);
            return View(giaoDichHocPhi);
        }


        public async Task<ActionResult> GiaoDichDelete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
            if (giaoDichHocPhi == null)
            {
                return HttpNotFound();
            }
            return View(giaoDichHocPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiaoDichDeleteConfirmed(int id)
        {
            GiaoDichHocPhi giaoDichHocPhi = await db.GiaoDichHocPhi.FindAsync(id);
            db.GiaoDichHocPhi.Remove(giaoDichHocPhi);
            await db.SaveChangesAsync();
            return RedirectToAction("GiaoDichList");
        }



        public ActionResult DuyetGiaoDich(int id)
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDich = db.GiaoDichHocPhi.Find(id);
                if (giaoDich != null)
                {
                    giaoDich.TrangThai = "Đã duyệt";
                    db.SaveChanges();

                    // Gửi email xác nhận cho học viên
                    SendEmailInternal(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);

                    TempData["Message"] = "Giao dịch đã được duyệt.";
                }
            }
            return RedirectToAction("GiaoDichList");
        }


        // Hàm gủi email thanh toán thành công
        private void SendEmailInternal(List<GiaoDichHocPhi> cart, string email)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học");
                var toAddress = new MailAddress(email);
                string subject = "Xác Nhận Thanh Toán Các Khóa Học";

                // Xây dựng nội dung email từ danh sách giao dịch
                var courseDetails = new StringBuilder();
                double totalAmount = 0;

                foreach (var item in cart)
                {
                    // Thêm thông tin chi tiết giao dịch vào email
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
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }








        public ActionResult TuChoiGiaoDich(int id)
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var giaoDich = db.GiaoDichHocPhi.Find(id);
                if (giaoDich != null)
                {
                    giaoDich.TrangThai = "Từ chối";
                    db.SaveChanges();

                    // Gửi email thông báo từ chối
                    SendEmailRejected(new List<GiaoDichHocPhi> { giaoDich }, giaoDich.Email);

                    TempData["Message"] = "Giao dịch đã bị từ chối.";
                }
            }
            return RedirectToAction("GiaoDichList");
        }


        // Hàm gửi email từ chối giao dịch
        private void SendEmailRejected(List<GiaoDichHocPhi> cart, string email)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học");
                var toAddress = new MailAddress(email);
                string subject = "Thông Báo Từ Chối Giao Dịch Thanh Toán";

                // Xây dựng nội dung email từ danh sách giao dịch
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
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }

    }
}

