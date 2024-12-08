using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Validation;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using System.Web.Security;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using RestSharp;


namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    
    //[Authorize(Roles = "Quản lý")]
    public class AccountController : Controller
    {
        TrungTamTinHocEntities ttth = new TrungTamTinHocEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(TaiKhoan tk, string retypePassword)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (tk.MatKhau != retypePassword)
            {
                ModelState.AddModelError("retypePassword", "Password không khớp.");
                return View();
            }


            TaiKhoan taikhoan = ttth.TaiKhoan.Where(t => t.TenDangNhap == tk.TenDangNhap).FirstOrDefault();
            if (taikhoan != null)
            {
                ModelState.AddModelError("TenDangNhap", "Tài khoản đã tồn tại.");
                return View();
            }

            taikhoan = ttth.TaiKhoan.Where(u => u.MaHV == tk.MaHV).FirstOrDefault();
            if (taikhoan != null)
            {
                ModelState.AddModelError("MaHV", "Mã học viên đã tồn tại.");
                return View();
            }

            taikhoan = new TaiKhoan();
            taikhoan.MaHV = tk.MaHV;
            taikhoan.TenDangNhap = tk.TenDangNhap;
            taikhoan.MatKhau = tk.MatKhau;
            taikhoan.QuyenHan = "Học viên";
            taikhoan.MaGV = null;
            ttth.TaiKhoan.Add(taikhoan);
            ttth.SaveChanges();

            return RedirectToAction("DangNhap");
        }


        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(TaiKhoan tk)
        {
            if (tk != null)
            {
                if (tk.MatKhau == null)
                {
                    ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu");
                    return View();
                }
                else
                {
                    TaiKhoan taikhoan = ttth.TaiKhoan.Where(t => t.TenDangNhap == tk.TenDangNhap).FirstOrDefault();
                    if (taikhoan != null)
                    {
                        // Sử dụng BCrypt để kiểm tra mật khẩu nhập vào với mật khẩu đã mã hóa trong database
                        //bool isPasswordValid = BCrypt.Net.BCrypt.Verify(tk.MatKhau, taikhoan.MatKhau);

                        //if (!isPasswordValid) // Mật khẩu không đúng
                        //{
                        //    ModelState.AddModelError("MatKhau", "Mật khẩu không chính xác");
                        //    return View();
                        //}
                        if (taikhoan.MatKhau != tk.MatKhau)
                        {
                            ModelState.AddModelError("MatKhau", "Mật khẩu không chính xác");
                            return View();
                        }

                        FormsAuthentication.SetAuthCookie(taikhoan.TenDangNhap, false);

                        HttpCookie loginTimeCookie = new HttpCookie("ThoiGianDangNhap", DateTime.Now.ToString());
                        Response.Cookies.Add(loginTimeCookie);

                        HttpCookie NDCookie = new HttpCookie("NguoiDung", taikhoan.TenDangNhap);
                        HttpCookie roleCookie = new HttpCookie("QuyenHan", taikhoan.QuyenHan);

                        Response.Cookies.Add(NDCookie);
                        Response.Cookies.Add(roleCookie);

                        if (taikhoan.QuyenHan == "Quản lý")
                        {
                            return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        }
                        else if (taikhoan.QuyenHan == "Giáo viên")
                        {
                            Session["MaGV"] = taikhoan.MaGV;
                            return RedirectToAction("Index", "GiangVien");
                        }
                        else
                        {
                            Session["MaHV"] = taikhoan.MaHV;
                            return RedirectToAction("Index", "HocVien");
                        }
                    }
                }
            }
            return View();
        }




        public ActionResult DangXuat()
        {
            HttpCookie authCookie = new HttpCookie("NguoiDung");
            authCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(authCookie);

            HttpCookie roleCookie = new HttpCookie("QuyenHan");
            roleCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(roleCookie);

            FormsAuthentication.SignOut();
            Session["TenDangNhap"] = null;
            Session["MaHV"] = null;
            Session["MaGV"] = null;


            Session.Clear();

            return RedirectToAction("DangNhap", "Account");
        }

        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }

        private static readonly Dictionary<string, string> otpStore = new Dictionary<string, string>();

        [HttpPost]
        public async Task<ActionResult> SendOTP(string email)
        {
            if (!IsValidEmail(email))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Email không hợp lệ!!!");
            }

            var otp = TaoMaOTP();

            otpStore[email] = otp;

            var apikey = System.Configuration.ConfigurationManager.AppSettings["SendGridAPIKey"];
            var sendGridClient = new SendGridClient(apikey);
            var from = new EmailAddress("buikhanhduy13082003@gmail.com", "Trung Tâm Tin Học HUIT");
            var subject = "Mã OTP xác nhận";
            var to = new EmailAddress(email);
            var plainTextContent = $"Mã OTP của bạn là: {otp}";
            var htmlContent = $"<strong>Mã OTP của bạn là: {otp}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return Json(new { message = "Mã OTP đã được gửi đến email của bạn." });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Gửi email thất bại. Vui lòng thử lại sau.");
            }
        }

        private string TaoMaOTP()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString(); // Tạo OTP 6 chữ số
            return otp;
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        [HttpPost]
        public ActionResult VerifyOTPAndChangePassword(string email, string otp, string currentPassword, string newPassword)
        {
            // Bước 1: Kiểm tra xem mã OTP có hợp lệ không
            if (!otpStore.ContainsKey(email) || otpStore[email] != otp)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            if (!IsCurrentPasswordValid(currentPassword))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Mật khẩu hiện tại không đúng.");
            }

            CapNhatMatKhau(newPassword);

            otpStore.Remove(email);

            return Json(new { message = "Mật khẩu đã được thay đổi thành công." });
        }

        public bool IsCurrentPasswordValid(string currentPassword)
        {
            try
            {
                string magv = Session["MaGV"]?.ToString();

                var taikhoan = ttth.TaiKhoan.Where(tk => tk.MaGV == magv).FirstOrDefault();
                if (currentPassword == taikhoan.MatKhau)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void CapNhatMatKhau(string matkhaumoi)
        {
            try
            {
                string magv = Session["MaGV"]?.ToString();
                TaiKhoan taikhoan = ttth.TaiKhoan.FirstOrDefault(tk => tk.MaGV == magv);
                if (taikhoan != null)
                {
                    taikhoan.MatKhau = matkhaumoi;

                    ttth.SaveChanges();
                }
            }
            catch
            {
                ModelState.AddModelError("MatKhau", "Lỗi xảy ra khi đổi mật khẩu!!");
            }
        }

        ////////////////////////////////

        [HttpGet]
        public ActionResult Quenmatkhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Quenmatkhau(string MaHV, string MaGV, string Email)
        {
            try
            { 
            
                TaiKhoan taiKhoan = null;

                if (!string.IsNullOrEmpty(MaHV))
                {
                    var hocVien = ttth.HocVien.FirstOrDefault(hv => hv.MaHV == MaHV && hv.Email == Email);
                    if (hocVien != null)
                    {
                        taiKhoan = ttth.TaiKhoan.FirstOrDefault(tk => tk.MaHV == MaHV);
                    }
                }
                else if (!string.IsNullOrEmpty(MaGV))
                {
                    var giaoVien = ttth.GiaoVien.FirstOrDefault(gv => gv.MaGV == MaGV && gv.Email == Email);
                    if (giaoVien != null)
                    {
                        taiKhoan = ttth.TaiKhoan.FirstOrDefault(tk => tk.MaGV == MaGV);
                    }
                }

                if (taiKhoan == null)
                {
                    TempData["ErrorMessage"] = "Thông tin bạn cung cấp không chính xác. Vui lòng kiểm tra lại.";
                    return RedirectToAction("Quenmatkhau", "Account");
                }

                var otp = TaoMaOTP();
                otpStore[Email] = otp;

                var apikey = System.Configuration.ConfigurationManager.AppSettings["SendGridAPIKey"];
                var sendGridClient = new SendGridClient(apikey);
                var from = new EmailAddress("buikhanhduy13082003@gmail.com", "Trung Tâm Tin Học HUIT");
                var to = new EmailAddress(Email);
                var subject = "Mã OTP để đặt lại mật khẩu";
                var plainTextContent = $"Mã OTP của bạn là: {otp}";
                var htmlContent = $"<strong>Mã OTP của bạn là: {otp}</strong>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await sendGridClient.SendEmailAsync(msg);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    TempData["ErrorMessage"] = "Gửi OTP thất bại. Vui lòng thử lại.";
                    return View();
                }

                Session["Email"] = Email;
                Session["MaTaiKhoan"] = taiKhoan.MaTK;

                return RedirectToAction("NhapOTP");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Thông tin bạn cung cấp không chính xác. Vui lòng kiểm tra lại.";
                return RedirectToAction("Quenmatkhau", "Account");
            }
        }

        public ActionResult NhapOTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NhapOTP(string otp, string newPassword)
        {
            try
            {
                string email = Session["Email"]?.ToString();
                string maTaiKhoanString = Session["MaTaiKhoan"]?.ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(maTaiKhoanString))
                {
                    TempData["ErrorMessage"] = "Thông tin không hợp lệ!!!";
                    return View();
                }

                if (!int.TryParse(maTaiKhoanString, out int maTaiKhoan))
                {
                    TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ!!!";
                    return View();
                }

                if (!otpStore.ContainsKey(email) || otpStore[email] != otp)
                {
                    ModelState.AddModelError("", "Mã OTP không chính xác hoặc đã hết hạn.");
                    return View();
                }

                var taiKhoan = ttth.TaiKhoan.FirstOrDefault(tk => tk.MaTK == maTaiKhoan);
                if (taiKhoan != null)
                {
                    taiKhoan.MatKhau = newPassword;
                    try
                    {
                        ttth.SaveChanges();
                        Console.WriteLine("Cập nhật mật khẩu thành công.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi lưu thay đổi: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Tài khoản không tồn tại.");
                }

                otpStore.Remove(email);

                TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi thành công";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lấy lại mật khẩu: " + ex;
                return RedirectToAction("DangNhap");
            }
        }
    }
}