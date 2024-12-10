using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.WebPages;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.IO;
using System.Web.Routing;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    public class HocVienController : Controller
    {

        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: HocVien
        public ActionResult Index()
        {
            if (Session["MaHV"] != null)
            {
                string maHV = Session["MaHV"].ToString();

                ViewBag.MaHV = maHV;

                var hocVien = db.HocVien.Where(hv => hv.MaHV == maHV).FirstOrDefault();

                ViewBag.Email = hocVien.Email;

                if (hocVien != null)
                {
                    return View(hocVien);
                }

                return RedirectToAction("DangNhap", "Account");
            }

            return RedirectToAction("DangNhap", "Account");
        }


        public ActionResult LichHoc()
        {
            try
            {
                string maHV = Session["MaHV"]?.ToString();
                if (string.IsNullOrEmpty(maHV))
                {
                    return RedirectToAction("Login", "HocVien");
                }

                using (var db = new TrungTamTinHocEntities())
                {
                    var lichHoc = (from lh in db.LichHoc.AsNoTracking()
                                   join lop in db.LopHoc.AsNoTracking() on lh.MaLH equals lop.MaLH
                                   join gv in db.GiaoVien.AsNoTracking() on lop.MaGV equals gv.MaGV
                                   join kh in db.KhoaHoc.AsNoTracking() on lop.MaKH equals kh.MaKH
                                   join ct in db.ChiTiet_HocVien_LopHoc.AsNoTracking() on lop.MaLH equals ct.MaLH
                                   where ct.MaHV == maHV
                                   select new
                                   {
                                       MaLop = lop.MaLH,
                                       TenLop = lop.TenPhong,
                                       GioBatDau = lh.GioBatDau,
                                       GioKetThuc = lh.GioKetThuc,
                                       TenGV = gv.HoTen,
                                       NgayHoc = lh.NgayHoc,
                                       NgayBatDau = kh.NgayBatDau,
                                       NgayKetThuc = kh.NgayKetThuc
                                   }).Distinct().ToList();

                    var lichHocViewList = lichHoc.Select(x => new LichHocViewModel
                    {
                        MaLop = x.MaLop,
                        TenLop = x.TenLop,
                        GioBatDau = x.GioBatDau.ToString(@"hh\:mm"),
                        GioKetThuc = x.GioKetThuc.ToString(@"hh\:mm"),
                        TenGV = x.TenGV,
                        NgayHoc = x.NgayHoc.ToString("dd/MM/yyyy"),
                    }).ToList();

                    ViewBag.HocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHV);

                    return View(lichHocViewList);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách lịch học: " + ex);
                return View();
            }
        }

        public ActionResult Ketquahoctap()
        {
            try
            {
                using (var db = new TrungTamTinHocEntities())
                {
                    var maHocVien = Session["MaHV"]?.ToString();

                    if (string.IsNullOrEmpty(maHocVien))
                    {
                        return RedirectToAction("HocVien", "Index");
                    }

                    var ketQuaHocTap = db.ChiTiet_HocVien_LopHoc
                                         .Where(c => c.MaHV == maHocVien)
                                         .ToList();

                    var hocvien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHocVien);
                    ViewBag.HocVien = hocvien;

                    var lopCanhBao = ketQuaHocTap
                                       .Where(kq => kq.Sobuoivang == 3)
                                       .Select(kq => new LopCanhBaoViewModel
                                       {
                                           MaLH = kq.MaLH,
                                           Sobuoivang = kq.Sobuoivang
                                       })
                                       .ToList();

                    ViewBag.LopCanhBao = lopCanhBao;

                    return View(ketQuaHocTap);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách lịch học: " + ex);
                return View();
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

                ViewBag.SearchTerm = search;

                return View(khoaHocList.ToList());
            }
        }

        [HttpGet]
        public ActionResult ThanhToan()
        {
            var cart = Session["Cart"] as List<GiaoDichHocPhi>;
            return RedirectToAction("ThongTinThanhToan");
        }



        public ActionResult ThongTinThanhToan()
        {
            return View();
        }


        [HttpPost]
        public ActionResult XacNhanThanhToan()
        {
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
                        db.GiaoDichHocPhi.Add(new GiaoDichHocPhi
                        {
                            MaHV = item.MaHV,
                            MaKH = item.MaKH,
                            MaLH = item.MaLH,
                            MaPT = 1,
                            NgayGD = DateTime.Now,
                            SoTien = item.SoTien,
                            SoDT = item.SoDT,
                            Email = item.Email,
                            TrangThai = "Chờ duyệt"
                        });
                    }

                    db.SaveChanges();
                    Session["Cart"] = null;
                    TempData["Message"] = "Thanh toán của bạn đang chờ duyệt.";
                    return RedirectToAction("ThongTinThanhToan");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
            }

            return RedirectToAction("HocPhi");
        }




        //[HttpGet]
        //public ActionResult AddToCart(string courseId, string maLH)
        //{
        //    var maHocVien = Session["MaHV"]?.ToString();

        //    if (string.IsNullOrEmpty(maHocVien))
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    try
        //    {
        //        using (var db = new TrungTamTinHocEntities())
        //        {
        //            var hocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHocVien);
        //            if (hocVien == null)
        //            {
        //                return HttpNotFound();
        //            }

        //            var course = db.KhoaHoc.FirstOrDefault(c => c.MaKH == courseId);
        //            if (course == null)
        //            {
        //                return HttpNotFound();
        //            }

        //            var daDangKy = db.GiaoDichHocPhi.Any(gd => gd.MaHV == maHocVien && gd.MaKH == courseId);

        //            if (daDangKy)
        //            {
        //                TempData["ErrorMessage"] = "Bạn đã đăng ký khóa học này. Không thể đăng ký lại.";
        //                return RedirectToAction("Dangkihocphan");
        //            }
        //            else
        //            {
        //                List<GiaoDichHocPhi> cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

        //                var existingCourse = cart.FirstOrDefault(c => c.MaKH == course.MaKH);
        //                if (existingCourse == null)
        //                {
        //                    cart.Add(new GiaoDichHocPhi
        //                    {
        //                        MaHV = hocVien.MaHV,
        //                        MaKH = course.MaKH,
        //                        MaLH = maLH,
        //                        MaPT = 1,
        //                        NgayGD = DateTime.Now,
        //                        SoTien = course.HocPhi,
        //                        SoDT = hocVien.SoDT,
        //                        Email = hocVien.Email
        //                    });

        //                    TempData["Message"] = "Khóa học đã được thêm vào giỏ hàng thành công!";
        //                }
        //                else
        //                {
        //                    TempData["Message"] = "Khóa học đã có trong giỏ hàng.";
        //                }

        //                Session["Cart"] = cart;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm khóa học vào giỏ hàng: " + ex.Message;
        //    }

        //    return RedirectToAction("HocPhi");
        //}




        public ActionResult RemoveFromCart(string courseId)
        {
            try
            {
                var cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

                var courseToRemove = cart.FirstOrDefault(c => c.MaKH == courseId);
                if (courseToRemove != null)
                {
                    cart.Remove(courseToRemove);
                }

                Session["Cart"] = cart;

                TempData["SuccessMessage"] = "Xóa khóa học khỏi giỏ hàng thành công";
                return RedirectToAction("HocPhi");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khóa học khỏi giỏ hàng: " + ex;
                return RedirectToAction("HocPhi");
            }
        }


        public ActionResult HocPhi()
        {
            string magv = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(magv))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cần mã học viên!!!");
            }
            using (var db = new TrungTamTinHocEntities())
            {
                var cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

                foreach (var item in cart)
                {
                    item.KhoaHoc = db.KhoaHoc.FirstOrDefault(k => k.MaKH == item.MaKH);
                    item.HocVien = db.HocVien.FirstOrDefault(h => h.MaHV == item.MaHV);
                }

                var totalAmount = cart.Sum(t => t.SoTien);
                ViewBag.TotalAmount = totalAmount;

                return View(cart);
            }
        }


        private void SendEmailInternal(List<GiaoDichHocPhi> cart, string email)
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
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể gửi email: " + ex.Message);
            }
        }



        [HttpPost]
        public ActionResult CapNhatThongTinHocVien(HocVien thongtinhocvien)
        {
            string mahv = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(mahv))
            {
                TempData["ErrorMessage"] = "Mã học viên không tồn tại!";
                return RedirectToAction("Index");
            }

            var hocvien = db.HocVien.Where(hv => hv.MaHV == mahv).FirstOrDefault();

            if (hocvien != null)
            {
                hocvien.HoTen = thongtinhocvien.HoTen;
                hocvien.NgaySinh = thongtinhocvien.NgaySinh;
                hocvien.GioiTinh = thongtinhocvien.GioiTinh;
                hocvien.Email = thongtinhocvien.Email;
                hocvien.SoDT = thongtinhocvien.SoDT;
                hocvien.DiaChi = thongtinhocvien.DiaChi;

                try
                {
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                    TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình cập nhật!!! Vui lòng kiểm tra lại dữ liệu.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy học viên!";
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        public ActionResult CapNhatAnhHocVien(HttpPostedFileBase Anh)
        {
            try
            {
                if (Anh != null && Anh.ContentLength > 0)
                {
                    string mahv = Session["MaHV"]?.ToString();
                    if (string.IsNullOrEmpty(mahv))
                    {
                        TempData["ErrorMessage"] = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn!";
                        return RedirectToAction("Login");
                    }

                    var dinhdangchophep = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var dinhdanganh = Path.GetExtension(Anh.FileName).ToLower();

                    if (!dinhdangchophep.Contains(dinhdanganh))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                        return RedirectToAction("Index");
                    }

                    if (Anh.ContentLength > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 5MB!";
                        return RedirectToAction("Index");
                    }

                    string tenanh = mahv + dinhdanganh;
                    string duongdan = Path.Combine(Server.MapPath("~/AnhHocVien"), tenanh);

                    Anh.SaveAs(duongdan);

                    try
                    {
                        var query = "UPDATE HocVien SET Anh = @Anh WHERE MaHV = @MaHV";
                        db.Database.ExecuteSqlCommand(query,
                            new SqlParameter("@Anh", tenanh),
                            new SqlParameter("@MaHV", mahv));

                        TempData["SuccessMessage"] = "Cập nhật ảnh thành công!";

                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh: " + ex.Message;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn một ảnh hợp lệ!";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh!!! Hãy thử lại sau";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        public ActionResult LichSuGiaoDich()
        {
            string maHV = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(maHV))
            {
                return RedirectToAction("Login", "HocVien");
            }
            using (var db = new TrungTamTinHocEntities())
            {
                var lichsugiaodich = (from gd in db.GiaoDichHocPhi.AsNoTracking()
                                      join kh in db.KhoaHoc.AsNoTracking() on gd.MaKH equals kh.MaKH
                                      join pt in db.PhuongThucThanhToan.AsNoTracking() on gd.MaPT equals pt.MaPT
                                      join hv in db.HocVien.AsNoTracking() on gd.MaHV equals hv.MaHV

                                      where gd.MaHV == maHV
                                      select new
                                      {
                                          MaHocVien = gd.MaHV,
                                          TenHocVien = hv.HoTen,
                                          MaKhoaHoc = gd.MaKH,
                                          TenKhoaHoc = kh.TenKH,
                                          TenPhuongThuc = pt.TenPT,
                                          NgayGiaoDich = gd.NgayGD,
                                          SoTien = gd.SoTien,
                                          SDT = gd.SoDT,
                                          Email = gd.Email,
                                          TrangThai = gd.TrangThai
                                      }).Distinct().ToList();
                var lichsuGDViewList = lichsugiaodich.Select(x => new LichSuGiaoDichViewModel
                {
                    MaHV = x.MaHocVien,
                    TenHocVien = x.TenHocVien,
                    MaKH = x.MaKhoaHoc,
                    TenKhoaHoc = x.TenKhoaHoc,
                    TenPhuongThuc = x.TenPhuongThuc,
                    NgayGD = x.NgayGiaoDich?.ToString("dd/MM/yyyy") ?? "N/A",
                    SoTien = x.SoTien ?? 0.0,
                    SoDT = x.SDT,
                    Email = x.Email,
                    TrangThai = x.TrangThai,

                }).ToList();

                ViewBag.HocVien = db.HocVien.FirstOrDefault(hv => hv.MaHV == maHV);

                return View(lichsuGDViewList);
            }
        }



        public ActionResult DanhSachLopHocTheoKhoaHoc(string maKH)
        {
            var hocVienId = Session["MaHV"]?.ToString();

            if (string.IsNullOrEmpty(hocVienId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để xem danh sách lớp học.";
                return RedirectToAction("Login", "Account");
            }

            var daDangKy = db.GiaoDichHocPhi.Any(gd => gd.MaHV == hocVienId && gd.MaKH == maKH);

            if (daDangKy)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký khóa học này. Không thể đăng ký lại.";
                return RedirectToAction("Dangkihocphan");
            }

            // Lấy danh sách lớp học của khóa
            var danhSachLop = db.LopHoc
                .Where(l => l.MaKH == maKH && l.TrangThai == false)
                .Select(l => new
                {
                    l,
                    SoLuongHocVien = db.ChiTiet_HocVien_LopHoc.Count(ctl => ctl.MaLH == l.MaLH)
                })
                .ToList();

            if (!danhSachLop.Any())
            {
                TempData["ErrorMessage"] = "Không có lớp học nào cho khóa học này.";
                return RedirectToAction("Dangkihocphan");
            }

            // Lấy danh sách lớp học mà học viên đã đăng ký
            var lopHocDaDangKy = db.ChiTiet_HocVien_LopHoc
                .Where(ctl => ctl.MaHV == hocVienId)
                .Select(ctl => ctl.LopHoc)
                .ToList();


            ViewBag.LopHocDaDangKy = lopHocDaDangKy;


            // Loại bỏ các lớp trùng lịch (nếu có)
            var danhSachLopKhongTrungLich = danhSachLop
        .Where(d => !lopHocDaDangKy.Any(lhdk => lhdk.ThuHoc == d.l.ThuHoc &&
                                                lhdk.GioBatDau < d.l.GioKetThuc &&
                                                d.l.GioBatDau < lhdk.GioKetThuc))
        .ToList();

            ViewBag.TenKhoaHoc = db.KhoaHoc.Where(kh => kh.MaKH == maKH).Select(kh => kh.TenKH).FirstOrDefault();
            ViewBag.MaKhoaHoc = maKH;

            // Chuyển đổi dữ liệu thành danh sách lớp học view model
            var danhSachLopModel = danhSachLopKhongTrungLich
                .Select(d => new LopHocViewModel
                {
                    LopHoc = d.l,
                    SoLuongHocVien = d.SoLuongHocVien
                })
                .ToList();

            return View(danhSachLopModel);
        }




        [HttpPost]
        public ActionResult DangKyLop(string maLH)
        {
            var hocVienId = Session["MaHV"]?.ToString();

            if (string.IsNullOrEmpty(hocVienId))
            {
                TempData["Error"] = "Bạn phải đăng nhập để đăng ký lớp học.";
                return RedirectToAction("Login", "Account");
            }

            var hocVien = db.HocVien.Find(hocVienId);
            var lopHoc = db.LopHoc.Find(maLH);



            if (hocVien != null && lopHoc != null)
            {
                List<GiaoDichHocPhi> cart = Session["Cart"] as List<GiaoDichHocPhi> ?? new List<GiaoDichHocPhi>();

                if (!cart.Any(x => x.MaLH == maLH))
                {
                    cart.Add(new GiaoDichHocPhi
                    {
                        MaHV = hocVien.MaHV,
                        MaKH = lopHoc.MaKH,
                        MaLH = maLH,
                        MaPT = 1,
                        NgayGD = DateTime.Now,
                        SoTien = lopHoc.KhoaHoc.HocPhi,
                        SoDT = hocVien.SoDT,
                        Email = hocVien.Email,
                        TrangThai = "Chờ duyệt"
                    });

                    TempData["Success"] = "Lớp học của khóa đã được thêm vào giỏ hàng.";
                    Session["Cart"] = cart;
                    return RedirectToAction("HocPhi");
                }
                else
                {
                    TempData["Error"] = "Lớp học của khóa đã có trong giỏ hàng.";
                }
            }
            else
            {
                TempData["Error"] = "Lớp học của khóa không tồn tại.";
            }

            return RedirectToAction("DanhSachLopHocTheoKhoaHoc", new { maKH = lopHoc?.MaKH });
        }





        [HttpGet]
        public ActionResult Thaydoimatkhau()
        {
            string mahv = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(mahv))
            {
                TempData["ErrorMessage"] = "Mã học viên không tồn tại!";
                return RedirectToAction("Index");
            }

            // Trả về view cho người dùng nhập mật khẩu cũ và mật khẩu mới
            return View();
        }


        [HttpPost]
        public ActionResult Thaydoimatkhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhauMoi)
        {
            string mahv = Session["MaHV"]?.ToString();
            if (string.IsNullOrEmpty(mahv))
            {
                TempData["ErrorMessage"] = "Mã học viên không tồn tại!";
                return RedirectToAction("Index");
            }

            // Lấy tài khoản của học viên từ cơ sở dữ liệu
            var taikhoan = db.TaiKhoan.FirstOrDefault(hv => hv.MaHV == mahv);
            if (taikhoan == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản của học viên!";
                return RedirectToAction("Index");
            }

            // Kiểm tra mật khẩu cũ
            if (taikhoan.MatKhau != matKhauCu)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không đúng!";
                return View();
            }

            // Kiểm tra mật khẩu mới và xác nhận
            if (matKhauMoi != xacNhanMatKhauMoi)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận không khớp!";
                return View();
            }

            // Cập nhật mật khẩu mới
            taikhoan.MatKhau = matKhauMoi;

            try
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thay đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình cập nhật mật khẩu! Vui lòng thử lại.";
                return View();
            }
        }
    }

}



