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
using System.Text;  // Dòng này cho StringBuilder
using System.IO;
using System.Web.Routing;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;  // Đảm bảo bạn đã thêm using này

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class HocVienController : Controller
    {

        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: HocVien
        public ActionResult Index()
        {
            // Kiểm tra xem học viên đã đăng nhập hay chưa
            if (Session["MaHV"] != null)
            {
                // Lấy MaHV từ session
                string maHV = Session["MaHV"].ToString();

                // Tìm học viên từ cơ sở dữ liệu dựa trên MaHV
                var hocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHV);

                if (hocVien != null)
                {
                    // Trả về view với model là thông tin học viên
                    return View(hocVien);
                }

                // Nếu không tìm thấy học viên, chuyển đến trang lỗi hoặc đăng nhập lại
                return RedirectToAction("DangNhap", "Account");
            }

            // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
            return RedirectToAction("DangNhap", "Account");
        }
        public ActionResult LichHoc()
        {
            using (var db = new TrungTamTinHocEntities())
            {
                // Lấy ngày hiện tại             

                // Truy vấn dữ liệu từ các bảng liên quan, chỉ lấy các trường cần thiết
                var lichHoc = (from lh in db.LichHoc.AsNoTracking()
                               join lop in db.LopHoc.AsNoTracking() on lh.MaLH equals lop.MaLH
                               join gv in db.GiaoVien.AsNoTracking() on lop.MaGV equals gv.MaGV
                               join kh in db.KhoaHoc.AsNoTracking() on lop.MaKH equals kh.MaKH                               
                               select new
                               {
                                   MaLop = lop.MaLH,
                                   TenLop = lop.TenLop,
                                   GioBatDau = lh.GioBatDau,
                                   GioKetThuc = lh.GioKetThuc,
                                   TenGV = gv.HoTen,
                                   NgayBatDau = kh.NgayBatDau,
                                   NgayKetThuc = kh.NgayKetThuc
                               }).ToList();

                // Thực hiện chuyển đổi sang LichHocView sau khi dữ liệu đã được tải về bộ nhớ
                var lichHocViewList = lichHoc.Select(x => new LichHocView
                {
                    MaLop = x.MaLop,
                    TenLop = x.TenLop,
                    GioBatDau = x.GioBatDau.ToString("hh:mm"),
                    GioKetThuc = x.GioKetThuc.ToString("hh:mm"),
                    TenGV = x.TenGV,
                    NgayBatDau = x.NgayBatDau.ToString("dd/MM/yyyy"),
                    NgayKetThuc = x.NgayKetThuc.ToString("dd/MM/yyyy")
                }).ToList();

                // Truyền dữ liệu sang View
                return View(lichHocViewList);
            }

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
        // Phương thức xử lý xác nhận thanh toán
        [HttpPost]
        public ActionResult XacNhanThanhToan()
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

                        // Gửi email xác nhận thanh toán
                        SendEmailInternal(cart, cart.FirstOrDefault()?.Email);
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Gửi email với thông tin giỏ hàng
                    SendEmailInternal(cart, cart.FirstOrDefault().Email);

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    Session["Cart"] = null;

                    TempData["Message"] = "Hãy xác nhận chính xác thông tin chuyển khoản trước khi ấn thanh toán. Mọi sai xót xin hãy liên hệ lại với nhân viên của chúng tôi!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
            }

            return RedirectToAction("HocPhi");
        }

        [HttpPost]
        public ActionResult CapNhatThongTinHocVien(HocVien thontinhocvien)
        {
            if (ModelState.IsValid)
            {
                string mahv = Session["MaHV"]?.ToString();

                // Kiểm tra nếu không có MaHV trong session
                if (string.IsNullOrEmpty(mahv))
                {
                    TempData["ErrorMessage"] = "Mã học viên không tồn tại!";
                    return RedirectToAction("Index");
                }

                var hocvien = db.HocVien.Where(hv => hv.MaHV == mahv).FirstOrDefault();

                if (hocvien != null)
                {
                    hocvien.HoTen = thontinhocvien.HoTen;
                    hocvien.NgaySinh = thontinhocvien.NgaySinh;
                    hocvien.GioiTinh = thontinhocvien.GioiTinh;
                    hocvien.Email = thontinhocvien.Email;
                    hocvien.SoDT = thontinhocvien.SoDT;
                    hocvien.DiaChi = thontinhocvien.DiaChi;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy học viên với mã đã cung cấp!";
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CapNhatAnhHocVien(HttpPostedFileBase Anh)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            string mahv = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(mahv))
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn!";
                return RedirectToAction("Login");
            }

            // Kiểm tra nếu không có tệp ảnh nào được chọn hoặc tệp có kích thước không hợp lệ
            if (Anh == null || Anh.ContentLength <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một ảnh hợp lệ!";
                return RedirectToAction("Index");
            }

            // Kiểm tra định dạng ảnh hợp lệ
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(Anh.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                return RedirectToAction("Index");
            }

            // Kiểm tra kích thước ảnh (không quá 5MB)
            if (Anh.ContentLength > 5 * 1024 * 1024) // 5MB
            {
                TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 5MB!";
                return RedirectToAction("Index");
            }

            try
            {
                // Tìm kiếm học viên trong cơ sở dữ liệu
                var hocvien = db.HocVien.FirstOrDefault(hv => hv.MaHV == mahv);
                if (hocvien == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy học viên!";
                    return RedirectToAction("Index");
                }

                // Tạo tên tệp ngẫu nhiên để tránh trùng lặp
                string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + fileExtension;
                string path = Path.Combine(Server.MapPath("~/AnhHocVien"), fileName);

                // Lưu ảnh mới
                Anh.SaveAs(path);

                // Xóa ảnh cũ nếu không phải là ảnh mặc định
                if (!string.IsNullOrEmpty(hocvien.Anh) && hocvien.Anh != "noimage.jpg")
                {
                    string oldImagePath = Path.Combine(Server.MapPath("~/AnhHocVien"), hocvien.Anh);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Cập nhật thông tin ảnh trong cơ sở dữ liệu
                hocvien.Anh = fileName;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật ảnh thành công!";
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật ảnh: " + ex.Message;
            }

            return RedirectToAction("Index");
        }



    }
}