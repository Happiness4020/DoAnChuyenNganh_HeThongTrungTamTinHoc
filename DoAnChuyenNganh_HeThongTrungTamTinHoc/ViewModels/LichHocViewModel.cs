using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LichHocViewModel
    {
         // Mã lớp học
        public string MaLop { get; set; }

        // Tên lớp học
        public string TenLop { get; set; }

        // Thời gian bắt đầu buổi học
        public string GioBatDau { get; set; }

        // Thời gian kết thúc buổi học
        public string GioKetThuc { get; set; }

        // Tên giáo viên dạy lớp
        public string TenGV { get; set; }

        // Ngày bắt đầu lớp học
        public string NgayBatDau { get; set; }

        // Ngày kết thúc lớp học
        public string NgayKetThuc { get; set; }
    }
}