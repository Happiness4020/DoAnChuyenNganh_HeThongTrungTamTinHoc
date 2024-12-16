using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;
using Stripe.Checkout;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Controllers
{
    [AllowAnonymous]
    public class KhoaHocController : Controller
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: KhoaHoc
        public ActionResult Index(string search = "", int page = 1, string sort_by = "price_asc")
        {
            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            var khs = db.KhoaHoc;
            List<KhoaHoc> khoahocs = db.KhoaHoc.Where(e => e.TenKH.Contains(search)).ToList();
            ViewBag.Search = search;

            if (sort_by == "price_asc")
            {
                khoahocs = khoahocs.OrderBy(c => c.HocPhi).ToList();
            }
            else if (sort_by == "price_desc")
            {
                khoahocs = khoahocs.OrderByDescending(c => c.HocPhi).ToList();
            }
            ViewBag.SortBy = sort_by;

            int NumberOfRecordsPerPage = 8;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(khoahocs.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            khoahocs = khoahocs.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();

            return View(khoahocs);
        }

        public ActionResult ChiTietKhoaHoc(string id, int page = 1, string sort_by = "datetime_asc")
        {
            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == id).FirstOrDefault();

            var binhluans = db.BinhLuanKhoaHoc
                     .Where(bl => bl.MaKH == id)
                     .OrderByDescending(bl => bl.NgayBinhLuan)
                     .ToList();

            var tatcabinhluan = db.BinhLuanKhoaHoc
                    .Where(bl => bl.MaKH == id)
                    .OrderByDescending(bl => bl.NgayBinhLuan)
                    .ToList();

            ViewBag.TatCaBinhLuan = tatcabinhluan; 

            if (sort_by == "datetime_asc")
            {
                binhluans = binhluans.OrderBy(c => c.NgayBinhLuan).ToList();
            }
            else if (sort_by == "datetime_desc")
            {
                binhluans = binhluans.OrderByDescending(c => c.NgayBinhLuan).ToList();
            }
            ViewBag.SortBy = sort_by;

            int tongSoBinhLuan = binhluans.Count;
            ViewBag.TongSoBinhLuan = tongSoBinhLuan;

            int NumberOfRecordsPerPage = 3;
            int NumberOfPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(binhluans.Count) / Convert.ToDouble(NumberOfRecordsPerPage)));
            int NumberOfRecordsToSkip = (page - 1) * NumberOfRecordsPerPage;
            ViewBag.Page = page;
            ViewBag.NumberOfPages = NumberOfPages;
            binhluans = binhluans.Skip(NumberOfRecordsToSkip).Take(NumberOfRecordsPerPage).ToList();

            ViewBag.BinhLuans = binhluans;

            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            ViewBag.KhoaHocNoiBats = KhoaHocNoiBat();

            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemBinhLuan(string MaKH, string NoiDung)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                ViewBag.MAHV = mahv;

                if (string.IsNullOrEmpty(mahv))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để bình luận!!!";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }
                if (string.IsNullOrEmpty(NoiDung))
                {
                    TempData["ErrorMessage"] = "Bạn chưa nhập nội dung bình luận!!!";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }

                DateTime truncatedToSecond = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                BinhLuanKhoaHoc binhluan = new BinhLuanKhoaHoc
                {
                    MaHV = mahv,
                    MaKH = MaKH,
                    NoiDung = NoiDung,
                    NgayBinhLuan = truncatedToSecond
                };

                db.BinhLuanKhoaHoc.Attach(binhluan);
                db.BinhLuanKhoaHoc.Add(binhluan);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng bình luận thành công";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng bình luận!!! Hãy thử lại sau";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaBinhLuan(DateTime NgayBinhLuan, string MaKH)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                var binhluan = db.BinhLuanKhoaHoc
                        .Where(b => b.MaHV == mahv && b.MaKH == MaKH)
                        .ToList()
                        .FirstOrDefault(b => b.NgayBinhLuan.Date == NgayBinhLuan.Date &&
                                             b.NgayBinhLuan.Hour == NgayBinhLuan.Hour &&
                                             b.NgayBinhLuan.Minute == NgayBinhLuan.Minute &&
                                             b.NgayBinhLuan.Second == NgayBinhLuan.Second);

                if (binhluan != null)
                {
                    db.BinhLuanKhoaHoc.Remove(binhluan);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa bình luận thành công";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }
                else
                {
                    TempData["ErrorMessage"] = "Bình luận không tồn tại hoặc có thể đã bị xóa!!!";
                    return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa bình luận!!!. Hãy thử lại sau";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        [HttpPost]
        public ActionResult SuaBinhLuan(string MaKH, string NoiDung, DateTime NgayBinhLuan)
        {
            try
            {
                string mahv = Session["MaHV"]?.ToString();
                var binhluan = db.BinhLuanKhoaHoc.FirstOrDefault(b => b.MaHV == mahv && b.NgayBinhLuan == NgayBinhLuan);
                if (binhluan != null)
                {
                    binhluan.NoiDung = NoiDung;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Chỉnh sửa bình luận thành công!!!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bình luận không tồn tại!!!";
                }
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chỉnh sửa bình luận! Hãy thử lại sau.";
                return RedirectToAction("ChiTietKhoaHoc", new { id = MaKH });
            }
        }

        public List<KhoaHocNoiBatViewModel> KhoaHocNoiBat()
        {
            List<KhoaHocNoiBatViewModel> khoahocs;
            try
            {
                var khoahocnoibat = db.GiaoDichHocPhi
                .GroupBy(gd => gd.MaKH)
                .Select(group => new
                {
                    MaKH = group.Key,
                    SoLanDangKy = group.Count()
                })
                .OrderByDescending(x => x.SoLanDangKy)
                .Take(10)
                .ToList();

                khoahocs = khoahocnoibat
                    .Join(db.KhoaHoc,
                        gd => gd.MaKH,
                        kh => kh.MaKH,
                        (gd, kh) => new KhoaHocNoiBatViewModel
                        {
                            MaKH = kh.MaKH,
                            TenKH = kh.TenKH,
                            Anh = kh.Anh,
                            NgayBatDau = kh.NgayBatDau.Date,
                            HocPhi = kh.HocPhi,
                            SoLanDangKy = gd.SoLanDangKy
                        })
                    .ToList();

                return khoahocs;
            }
            catch
            {
                return new List<KhoaHocNoiBatViewModel>();
            }
        }

        public ActionResult DangKyKhoaHoc(string makh)
        {
            string mahv = Session["MaHV"]?.ToString();
            if(mahv != null)
            {
                return RedirectToAction("Error404", "Account");
            }    

            List<ChuongTrinhHoc> cths = db.ChuongTrinhHoc.ToList();
            ViewBag.ChuongTrinhHocs = cths;

            KhoaHoc kh = db.KhoaHoc.Where(t => t.MaKH == makh).FirstOrDefault();

            ViewBag.MaKH = makh;
            ViewBag.PaymentSuccess = TempData["PaymentSuccess"] != null;

            return View(kh);
        }


        [HttpPost]
        public ActionResult DangKyKhoaHoc(string makh, DangKyKhoaHocViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin!!!";
                return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
            }

            if (db.HocVien.Any(hv => hv.Email == model.Email))
            {
                TempData["ErrorMessage"] = "Email đã được sử dụng!!! Vui lòng dùng email khác";
                return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
            }

            if (db.HocVien.Any(hv => hv.SoDT == model.SoDT))
            {
                TempData["ErrorMessage"] = "Số điện thoại đã được sử dụng!!! Vui lòng dùng số khác";
                return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
            }

            TempData["DangKyKhoaHocData"] = model;
            TempData.Keep("DangKyKhoaHocData");

            try
            {
                var khoaHoc = db.KhoaHoc.FirstOrDefault(k => k.MaKH == makh);
                if (khoaHoc == null)
                {
                    TempData["ErrorMessage"] = "Khóa học không tồn tại.";
                    return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
                }

                var stripeSecretKey = System.Configuration.ConfigurationManager.AppSettings["StripeSecretKey"];
                StripeConfiguration.ApiKey = stripeSecretKey;

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)khoaHoc.HocPhi,
                            Currency = "vnd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = khoaHoc.TenKH,
                                Description = "Khóa học " + khoaHoc.LoaiKH,
                                Metadata = new Dictionary<string, string>
                                {
                                    { "MaKH", khoaHoc.MaKH },
                                    { "TenKH", khoaHoc.TenKH },
                                    { "HocPhi", khoaHoc.HocPhi.ToString() },
                                }
                            },
                        },
                        Quantity = 1,
                    },
                },
                    Mode = "payment",
                    SuccessUrl = Url.Action("ThanhToanThanhCong", "KhoaHoc", new { makh = makh, hocphi = khoaHoc.HocPhi }, Request.Url.Scheme),
                    CancelUrl = Url.Action("DangKyKhoaHoc", "KhoaHoc", new { makh = makh }, Request.Url.Scheme),
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Json(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình thanh toán: " + ex.Message;
                return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
            }
        }

        public async Task<ActionResult> GuiMaHV(string email, string mahv)
        {
            try
            {

                // Cấu hình thông tin email
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], "Trung Tâm Tin Học HUIT");
                var toAddress = new MailAddress(email);
                string subject = "Mã học viên";

                // Nội dung email
                string body = $@"
                <html>
                <body>
                    <p>Xin chào,</p>
                    <p>Mã học viên của bạn là:</p>
                    <h3>{mahv}</h3>
                    <p>Hãy dùng mã học viên của bạn để đăng ký tài khoản trên website của Trung tâm Tin học HUIT. Tài khoản học viên giúp bạn cập nhật thông tin học, các thông tin cá nhân và kết quả học tập của bạn.</p>
                </body>
                </html>";

                // Gửi email
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

                return Json(new { message = "Mã học viên đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Gửi email thất bại. Vui lòng thử lại sau. " + ex.Message);
            }
        }




        public async Task<ActionResult> ThanhToanThanhCong(string makh, double hocphi)
        {
            try
            {
                var khoaHoc = db.KhoaHoc.FirstOrDefault(k => k.MaKH == makh);
                if (khoaHoc == null)
                {
                    TempData["ErrorMessage"] = "Khóa học không tồn tại!";
                    return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
                }

                if (TempData["DangKyKhoaHocData"] is DangKyKhoaHocViewModel model)
                {
                    string mahv = Utility.TaoMaNgauNhien("HV", 8);

                    var hocvien = new HocVien
                    {
                        MaHV = mahv,
                        Anh = "noimage.jpg",
                        HoTen = model.HoTen,
                        NgaySinh = model.NgaySinh,
                        GioiTinh = model.GioiTinh,
                        Email = model.Email,
                        SoDT = model.SoDT,
                        DiaChi = model.DiaChi,
                    };
                    db.HocVien.Add(hocvien);
                    await db.SaveChangesAsync();

                    await GuiMaHV(model.Email, mahv);

                    Session["MaHV"] = mahv;

                    TempData["SuccessMessage"] = "Thanh toán thành công!!! Hãy dùng mã học viên được gửi đến email đã đăng ký của bạn để tạo tài khoản học viên. Vui lòng chọn và đăng ký lớp học ở bên dưới.";
                    return RedirectToAction("DanhSachLopHocTheoKhoaHoc", new { maKH = makh });
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký khóa học!!! Hãy thử lại sau";
                return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
            }
            return RedirectToAction("DangKyKhoaHoc", new { makh = makh });
        }



        public ActionResult DanhSachLopHocTheoKhoaHoc(string maKH)
        {
            try
            {
                var hocVienId = Session["MaHV"]?.ToString();
                if (string.IsNullOrEmpty(hocVienId))
                {
                    TempData["ErrorMessage"] = "Bạn phải hoàn tất đăng ký khóa học để chọn lớp.";
                    return RedirectToAction("DangKyKhoaHoc", new { makh = maKH });
                }

                var danhSachLop = db.LopHoc.Where(l => l.MaKH == maKH && l.TrangThai == false)
                                           .Select(l => new
                                           {
                                               l,
                                               SoLuongHocVien = db.ChiTiet_HocVien_LopHoc.Count(ctl => ctl.MaLH == l.MaLH)
                                           })
                                           .ToList();

                if (!danhSachLop.Any())
                {
                    TempData["ErrorMessage"] = "Không có lớp học nào cho khóa học này.";
                    return RedirectToAction("DangKyKhoaHoc", new { makh = maKH });
                }

                ViewBag.TenKhoaHoc = db.KhoaHoc.Where(kh => kh.MaKH == maKH).Select(kh => kh.TenKH).FirstOrDefault();
                ViewBag.MaKhoaHoc = maKH;

                var danhSachLopModel = danhSachLop.Select(d => new LopHocViewModel
                {
                    LopHoc = d.l,
                    SoLuongHocVien = d.SoLuongHocVien
                }).ToList();

                return View(danhSachLopModel);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách lớp: " + ex);
                return View();
            }
        }




        [HttpPost]
        public ActionResult DangKyLop(string maLH)
        {
            try
            {
                var hocVienId = Session["MaHV"]?.ToString();

                if (string.IsNullOrEmpty(hocVienId))
                {
                    TempData["Error"] = "Bạn phải đăng nhập để đăng ký lớp học.";
                    TempData["ErrorMessage"] = "Bạn phải đăng nhập để đăng ký lớp học.";
                    return RedirectToAction("Login", "Account");
                }

                var hocVien = db.HocVien.Find(hocVienId);
                var lopHoc = db.LopHoc.Find(maLH);

                if (hocVien != null && lopHoc != null)
                {
                    var daDangKy = db.GiaoDichHocPhi.Any(gd =>
                        gd.MaHV == hocVien.MaHV && gd.MaKH == lopHoc.MaKH && gd.TrangThai != "Hủy");

                    if (daDangKy)
                    {
                        TempData["ErrorMessage"] = "Bạn đã đăng ký một lớp trong khóa học này rồi.";
                        return RedirectToAction("DanhSachLopHocTheoKhoaHoc", new { maKH = lopHoc.MaKH });
                    }

                    var giaoDich = new GiaoDichHocPhi
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
                    };

                    db.GiaoDichHocPhi.Add(giaoDich);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đăng ký lớp học thành công. Vui lòng chờ admin duyệt.";
                    return RedirectToAction("ThanhToan");
                }
                else
                {
                    TempData["Error"] = "Lớp học không tồn tại hoặc đã có lỗi xảy ra.";
                    TempData["ErrorMessage"] = "Lớp học không tồn tại hoặc đã có lỗi xảy ra.";
                }
                return RedirectToAction("DanhSachLopHocTheoKhoaHoc", new { maKH = lopHoc?.MaKH });
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký khóa học!!!";
                return RedirectToAction("Index");
            }
        }

        public ActionResult ThanhToan()
        {
            return View();
        }
    }
}