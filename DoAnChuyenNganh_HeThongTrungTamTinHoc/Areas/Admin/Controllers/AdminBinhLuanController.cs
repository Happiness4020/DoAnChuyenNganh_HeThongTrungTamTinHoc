using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Filter;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminBinhLuanController : Controller
    {
        // Tạo instance cho context
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        // GET: Admin/AdminBinhLuan
        // Action hiển thị danh sách bình luận
        public ActionResult BinhLuanList()
        {
            var binhLuan = db.BinhLuanKhoaHoc.ToList();
            return View(binhLuan);
        }

        // Action hiển thị form xác nhận xóa bình luận
        public ActionResult BinhLuanDelete(string id)
        {
            // Lấy bình luận dựa theo ID
            BinhLuanKhoaHoc binhLuan = db.BinhLuanKhoaHoc.FirstOrDefault(bl => bl.MaHV == id);
            return View(binhLuan);
        }

        [HttpPost]
        // Action xử lý xóa bình luận
        public ActionResult BinhLuanDelete(string id, BinhLuanKhoaHoc bl)
        {
            // Tìm bình luận cần xóa
            bl = db.BinhLuanKhoaHoc.FirstOrDefault(t => t.MaHV == id);

            // Xóa bình luận nếu tìm thấy
            if (bl != null)
            {
                db.BinhLuanKhoaHoc.Remove(bl);
                db.SaveChanges();
            }
            return RedirectToAction("BinhLuanList");
        }
    }
}
