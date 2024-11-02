using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System.Text;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminGiangVienController : Controller
    {
        private TrungTamTinHocEntities db = new TrungTamTinHocEntities();
        // GET: Admin/AdminGiangVien
        public ActionResult GiangVienList(string search = "")
        {
            var ds = db.GiaoVien;

            List<GiaoVien> giangviens = db.GiaoVien.Where(e => e.HoTen.Contains(search)).ToList();
            ViewBag.Search = search;
            return View(giangviens);
        }
        string magv = TaoMaGiangVien();
        public ActionResult GiangVienAdd()
        {
            ViewBag.MaGV = magv;
            return View();
        }
        [HttpPost]
        public ActionResult GiangVienAdd(GiaoVien gv)
        {
            if (ModelState.IsValid)
            {
                GiaoVien giaovien = db.GiaoVien.Where(t => t.MaGV == magv).FirstOrDefault();
                if (giaovien != null)
                {
                    ModelState.AddModelError("MaGV", "Giáo viên đã tồn tại!!");
                    return View();
                }

                if (string.IsNullOrEmpty(gv.HoTen) && string.IsNullOrEmpty(gv.Anh) && string.IsNullOrEmpty(gv.NgayVaoLam.ToString()) && string.IsNullOrEmpty(gv.Anh) && string.IsNullOrEmpty(gv.BangCapGV) && string.IsNullOrEmpty(gv.LinhVucDaoTao) && string.IsNullOrEmpty(gv.TrinhDo) && string.IsNullOrEmpty(gv.Email) && string.IsNullOrEmpty(gv.SoDT) && string.IsNullOrEmpty(gv.DiaChi) && string.IsNullOrEmpty(gv.Luong.ToString()))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin của giảng viên!!!");
                    return View();
                }
                else if (!string.IsNullOrEmpty(magv))
                {
                    giaovien = new GiaoVien
                    {
                        MaGV = magv,
                        HoTen = gv.HoTen,
                        Anh = gv.Anh,
                        NgayVaoLam = DateTime.Parse(gv.NgayVaoLam.ToString()),
                        BangCapGV = gv.BangCapGV,
                        LinhVucDaoTao = gv.LinhVucDaoTao,
                        TrinhDo = gv.TrinhDo,
                        Email = gv.Email,
                        SoDT = gv.SoDT,
                        DiaChi = gv.DiaChi,
                        Luong = gv.Luong
                    };
                }    

                db.GiaoVien.Add(giaovien);
                db.SaveChanges();

                return RedirectToAction("GiangVienList");
            }
            else
            {
                return View();
            }
        }

        private static Random random = new Random();
        public static string TaoMaGiangVien()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder maGiangVien = new StringBuilder("HV");

            for (int i = 0; i < 8; i++)
            {
                maGiangVien.Append(chars[random.Next(chars.Length)]);
            }

            return maGiangVien.ToString();
        }
    }
}