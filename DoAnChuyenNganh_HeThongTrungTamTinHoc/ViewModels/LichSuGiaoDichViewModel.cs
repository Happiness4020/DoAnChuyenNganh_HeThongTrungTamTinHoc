using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LichSuGiaoDichViewModel
    {
        public int MaGD { get; set; } // Khóa chính
        public string MaHV { get; set; } // Có thể NULL
        public string MaKH { get; set; } // Có thể NULL
        public int? MaPT { get; set; } // Có thể NULL
        public string NgayGD { get; set; } // Kiểu string để hiển thị
        public double SoTien { get; set; } // Có thể NULL
        public string SoDT { get; set; } // Có thể NULL
        public string Email { get; set; } // Có thể NULL
        public string TrangThai { get; set; } // Có thể NULL
        public string TenKhoaHoc { get; set; } // Có thể NULL
        public string TenHocVien { get; set; } // Có thể NULL
        public string TenPhuongThuc { get; set; }
    }
}