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

        private static readonly Dictionary<string, string> otpStore = new Dictionary<string, string>(); // Lưu trữ OTP tạm thời

        [HttpPost]
        public async Task<ActionResult> SendOTP(string email)
        {
            if (!IsValidEmail(email))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Email không hợp lệ!!!");
            }

            var otp = TaoMaOTP();

            otpStore[email] = otp;

            var sendGridClient = new SendGridClient("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");
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
    }
}