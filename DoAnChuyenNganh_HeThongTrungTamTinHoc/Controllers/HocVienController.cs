using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.WebPages;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models; // Đảm bảo rằng namespace này đúng với nơi định nghĩa TrungTamTinHocEntities
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Configuration;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class HocVienController : Controller
    {



        // GET: HocVien
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LichHoc()
        {
            return View();
        }
        public ActionResult Ketquahoctap()
        {
            // Khởi tạo context
            using (var db = new TrungTamTinHocEntities())
            {
                // Lấy mã học viên hiện tại từ session (giả sử bạn đã lưu mã học viên vào session)
                var maHocVien = Session["MaHV"]?.ToString(); // Hoặc cách lấy mã học viên khác tùy vào cách bạn quản lý session

                // Kiểm tra xem mã học viên có null hay không
                if (string.IsNullOrEmpty(maHocVien))
                {
                    // Nếu mã học viên không tồn tại, có thể trả về một trang thông báo hoặc điều hướng đến trang khác
                    return RedirectToAction("HocVien", "Index"); // Hoặc trang khác bạn muốn điều hướng
                }

                // Truy vấn kết quả học tập cho học viên
                var ketQuaHocTap = db.ChiTiet_HocVien_LopHoc
                                     .Where(c => c.MaHV == maHocVien)
                                     .ToList();

                // Trả về view với kết quả học tập
                return View(ketQuaHocTap);
            }
        }
        public ActionResult Dangkihocphan(string search)
        {
            using (var db = new TrungTamTinHocEntities())
            {
                var khoaHocList = db.KhoaHoc.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    khoaHocList = khoaHocList.Where(k => k.TenKH.Contains(search));
                }

                // Truyền giá trị tìm kiếm để hiển thị lại trên giao diện
                ViewBag.SearchTerm = search;

                return View(khoaHocList.ToList());
            }
        }
        [HttpGet]
        public ActionResult AddToCart(string courseId)
        {
            // Lấy mã học viên từ session (giả sử mã học viên đã được lưu vào session khi học viên đăng nhập)
            var maHocVien = Session["MaHV"]?.ToString();

            if (string.IsNullOrEmpty(maHocVien))
            {
                // Nếu không có mã học viên trong session, yêu cầu đăng nhập lại
                return RedirectToAction("Login", "Home");
            }

            try
            {
                using (var db = new TrungTamTinHocEntities())
                {
                    // Lấy thông tin học viên từ bảng HocVien
                    var hocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHocVien);
                    if (hocVien == null)
                    {
                        // Nếu không tìm thấy học viên, trả về thông báo lỗi
                        return HttpNotFound();
                    }

                    // Lấy thông tin khóa học dựa trên MaKH (courseId)
                    var course = db.KhoaHoc.FirstOrDefault(c => c.MaKH == courseId);
                    if (course == null)
                    {
                        // Nếu không tìm thấy khóa học, trả về thông báo lỗi
                        return HttpNotFound();
                    }

                    // Lấy giỏ hàng từ Session, nếu giỏ hàng chưa tồn tại thì khởi tạo mới
                    List<GiaoDichHocPhi> cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

                    // Kiểm tra nếu khóa học đã tồn tại trong giỏ hàng
                    var existingCourse = cart.FirstOrDefault(c => c.MaKH == course.MaKH);
                    if (existingCourse == null)
                    {
                        // Nếu khóa học chưa có trong giỏ, thêm khóa học vào giỏ hàng
                        cart.Add(new GiaoDichHocPhi
                        {
                            MaHV = hocVien.MaHV,
                            MaKH = course.MaKH,
                            MaPT = 1, // Mặc định là 'Thanh toán chuyển khoản' với MaPT = 1
                            NgayGD = DateTime.Now, // Ngày giao dịch là ngày hiện tại
                            SoTien = course.HocPhi, // Số tiền lấy từ học phí của khóa học
                            SoDT = hocVien.SoDT, // Số điện thoại học viên
                            Email = hocVien.Email // Email học viên
                        });

                        TempData["Message"] = "Khóa học đã được thêm vào giỏ hàng thành công!";
                    }
                    else
                    {
                        // Nếu khóa học đã có trong giỏ, thông báo cho người dùng
                        TempData["Message"] = "Khóa học đã có trong giỏ hàng.";
                    }

                    // Cập nhật giỏ hàng vào Session
                    Session["Cart"] = cart;
                }
            }
            catch (Exception ex)
            {
                // Nếu xảy ra lỗi, lưu thông báo lỗi vào TempData để hiển thị
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm khóa học vào giỏ hàng: " + ex.Message;
            }

            // Chuyển hướng người dùng về trang thanh toán học phí hoặc giỏ hàng
            return RedirectToAction("HocPhi");
        }
        public ActionResult HocPhi()
        {
            using (var db = new TrungTamTinHocEntities())
            {
                // Lấy giỏ hàng từ Session
                var cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

                // Thêm thông tin khóa học và học viên nếu chưa có
                foreach (var item in cart)
                {
                    item.KhoaHoc = db.KhoaHoc.FirstOrDefault(k => k.MaKH == item.MaKH);
                    item.HocVien = db.HocVien.FirstOrDefault(h => h.MaHV == item.MaHV);
                }

                // Tính tổng học phí cho giỏ hàng
                var totalAmount = cart.Sum(t => t.SoTien);
                ViewBag.TotalAmount = totalAmount;

                // Truyền giỏ hàng vào View để hiển thị
                return View(cart);
            }
        }
        [HttpGet]
        public ActionResult ThanhToan()
        {
            // Lấy giỏ hàng từ session
            var cart = Session["Cart"] as List<GiaoDichHocPhi>;

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("HocPhi");
            }

            try
            {
                using (var db = new TrungTamTinHocEntities())
                {
                    foreach (var item in cart)
                    {
                        // Tạo một giao dịch học phí mới và lưu vào cơ sở dữ liệu
                        db.GiaoDichHocPhi.Add(new GiaoDichHocPhi
                        {
                            MaHV = item.MaHV,
                            MaKH = item.MaKH,
                            MaPT = 1, // Thanh toán chuyển khoản
                            NgayGD = DateTime.Now,
                            SoTien = item.SoTien,
                            SoDT = item.SoDT,
                            Email = item.Email
                        });

                    }
                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    Session["Cart"] = null;

                    TempData["Message"] = "Hãy xác nhận chính xác thông tin chuyển khoản trước khi ấn thanh toán.Mọi sai xót xin hãy liên hệ lại với nhân viên của chúng tôi!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
            }

            return RedirectToAction("ThongTinThanhToan");
        }


        public ActionResult RemoveFromCart(string courseId)
        {
            // Lấy giỏ hàng từ session
            var cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

            // Tìm và xóa khóa học trong giỏ hàng
            var courseToRemove = cart.FirstOrDefault(c => c.MaKH == courseId);
            if (courseToRemove != null)
            {
                cart.Remove(courseToRemove);
            }

            // Cập nhật lại giỏ hàng trong session
            Session["Cart"] = cart;

            // Chuyển hướng lại trang giỏ hàng
            return RedirectToAction("HocPhi");
        }

        public ActionResult ThongTinThanhToan()
        {
            return View();
        }

        private void SendEmailInternal(string maKH, double soTien, string email)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học");
                var toAddress = new MailAddress(email);
                string subject = "Xác Nhận Thanh Toán Khóa Học";

                string body = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
        .container {{ padding: 20px; max-width: 600px; margin: auto; background-color: #f9f9f9; border-radius: 10px; }}
        .header {{ text-align: center; background-color: #007BFF; color: white; padding: 10px; border-radius: 10px 10px 0 0; }}
        .content {{ margin: 20px 0; }}
        .footer {{ text-align: center; font-size: 12px; color: #777; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Xác Nhận Thanh Toán</div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Bạn đã thanh toán thành công cho khóa học <b>{maKH}</b>.</p>
            <p>Số tiền: <b>{soTien:C}</b>.</p>
            <p>Cảm ơn bạn đã tin tưởng Trung Tâm Tin Học của chúng tôi!</p>
        </div>
        <div class='footer'>
            © 2024 Trung Tâm Tin Học. Mọi thắc mắc vui lòng liên hệ chúng tôi.
        </div>
    </div>
</body>
</html>";

                // Cấu hình SMTP
                string smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
                bool enabledSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"]);
                string fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"];

                using (var smtp = new SmtpClient
                {
                    Host = smtpHost,
                    Port = smtpPort,
                    EnableSsl = enabledSsl,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FromEmailAddress"], fromEmailPassword)
                })
                {
                    var mailMessage = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    smtp.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }

        // Phương thức xử lý xác nhận thanh toán
        [HttpPost]
        public ActionResult XacNhanThanhToan()
        {
            try
            {
                // Lấy giỏ hàng từ session
                var cart = Session["Cart"] as List<GiaoDichHocPhi>;

                if (cart == null || !cart.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống, không thể xác nhận thanh toán.";
                    return RedirectToAction("HocPhi");
                }

                using (var db = new TrungTamTinHocEntities())
                {
                    foreach (var item in cart)
                    {
                        // Kiểm tra dữ liệu đầu vào
                        if (string.IsNullOrEmpty(item.MaKH) || string.IsNullOrEmpty(item.Email) || item.SoTien <= 0)
                        {
                            TempData["ErrorMessage"] = "Thông tin giao dịch không hợp lệ.";
                            return RedirectToAction("HocPhi");
                        }

                        // Thêm giao dịch vào cơ sở dữ liệu
                        db.GiaoDichHocPhi.Add(new GiaoDichHocPhi
                        {
                            MaHV = item.MaHV,
                            MaKH = item.MaKH,
                            MaPT = 1, // Thanh toán chuyển khoản
                            NgayGD = DateTime.Now,
                            SoTien = item.SoTien
                        });

                        // Gửi email xác nhận thanh toán
                        SendEmailInternal(item.MaKH, item.SoTien, item.Email);
                    }

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    Session["Cart"] = null;

                    TempData["Message"] = "Xác nhận thanh toán thành công. Vui lòng kiểm tra email để nhận thông báo.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận thanh toán: " + ex.Message;
            }

            return RedirectToAction("ThongTinThanhToan");
        }




    }
}