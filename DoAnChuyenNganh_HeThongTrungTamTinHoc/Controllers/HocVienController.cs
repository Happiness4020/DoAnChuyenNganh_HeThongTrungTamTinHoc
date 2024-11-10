using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models; // Đảm bảo rằng namespace này đúng với nơi định nghĩa TrungTamTinHocEntities



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

                return View(khoaHocList.ToList());
            }
        }
        [HttpGet]
        public ActionResult AddToCart(string courseId)
        {
                    // Lấy thông tin người dùng từ Cookie
            var userId = HttpContext.Request.Cookies["NguoiDung"]?.Value;

            try
            {
                using (var db = new TrungTamTinHocEntities())
                {
                    // Tìm khóa học theo courseId (chuỗi)
                    var course = db.KhoaHoc.FirstOrDefault(c => c.MaKH == courseId); // Sử dụng FirstOrDefault thay vì Find
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
                            MaKH = course.MaKH,
                            SoTien = course.HocPhi,
                            NgayGD = DateTime.Now,
                            TrangThai = false
                        });

                        TempData["Message"] = "Khóa học đã được thêm vào giỏ hàng thành công!";
                    }
                    else
                    {
                        // Nếu khóa học đã có trong giỏ, thông báo cho người dùng
                        ViewBag.Message = "Khóa học đã có trong giỏ hàng.";
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
            var userId = HttpContext.Request.Cookies["NguoiDung"]?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Home");
            }

            List<GiaoDichHocPhi> cartItems = Session["Cart"] as List<GiaoDichHocPhi>;

            if (cartItems == null || cartItems.Count == 0)
            {
                ViewBag.Message = "Giỏ hàng của bạn trống.";
                return View();
            }

            return View(cartItems);
        }
        public ActionResult ThanhToan()
        {
            var cartItems = Session["Cart"] as List<GiaoDichHocPhi>;
            if (cartItems == null || cartItems.Count == 0)
            {
                return RedirectToAction("HocPhi");
            }

            using (var db = new TrungTamTinHocEntities())
            {
                foreach (var item in cartItems)
                {
                    var newTransaction = new GiaoDichHocPhi
                    {
                        MaHV = HttpContext.Request.Cookies["NguoiDung"]?.Value,
                        MaKH = item.MaKH,
                        SoTien = item.SoTien,
                        NgayGD = DateTime.Now,
                        TrangThai = true
                    };

                    db.GiaoDichHocPhi.Add(newTransaction);
                }

                db.SaveChanges();
            }

            // Xóa giỏ hàng sau khi thanh toán
            Session["Cart"] = null;

            TempData["Message"] = "Thanh toán thành công!";
            return RedirectToAction("Index", "HocVien");
        }

    }
}