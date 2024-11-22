using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Services
{
    public class LichDayService
    {
        TrungTamTinHocEntities db = new TrungTamTinHocEntities();

        public LichDayService()
        {
            
        }

        public void PhanLichDayChoGV(string magv)
        {
            // Lấy danh sách các lớp học của giáo viên
            var lopHocList = db.LopHoc.Where(lh => lh.MaGV == magv).ToList();

            foreach (var lopHoc in lopHocList)
            {
                // Lấy danh sách lịch học tương ứng với lớp học
                var lichHocList = db.LichHoc.Where(lh => lh.MaLH == lopHoc.MaLH).ToList();

                foreach (var lichHoc in lichHocList)
                {
                    // Tạo lịch dạy cho giáo viên
                    var lichDay = new LichDay
                    {
                        MaLichDay = Guid.NewGuid().ToString(), // Tạo MaLichDay tự động
                        MaGV = magv,
                        MaLH = lopHoc.MaLH,
                        NgayDay = lichHoc.NgayHoc.Date, // Ngày dạy trùng với ngày học
                        GioBatDau = lichHoc.GioBatDau, // Giờ bắt đầu trùng với giờ học
                        GioKetThuc = lichHoc.GioKetThuc, // Giờ kết thúc trùng với giờ học
                    };

                    // Lưu lịch dạy vào cơ sở dữ liệu
                    db.LichDay.Add(lichDay);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();
        }
    }
}