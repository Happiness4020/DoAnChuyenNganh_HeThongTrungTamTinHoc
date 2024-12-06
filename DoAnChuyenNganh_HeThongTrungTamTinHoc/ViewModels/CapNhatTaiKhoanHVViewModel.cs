using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class CapNhatTaiKhoanHVViewModel
    {
        public string MaHV { get; set; } // Có thể NULL
        public string MaGV { get; set; } // Có thể NULL
        public string TenDangNhap { get; set; } // Cần thêm cho việc xác định tài khoản
        public string MatKhau { get; set; } // Mật khẩu người dùng
        public string QuyenHan { get; set; } // Quyền hạn (nếu cần quản lý)
        public string Email { get; set; }
        public string SDT { get; set; }
        public int MaTK { get; set; }
    }
}