using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LichSuGiaoDichViewModel
    {
        public int MaGD { get; set; }
        public string MaHV { get; set; }
        public string MaKH { get; set; }
        public int? MaPT { get; set; }
        public string NgayGD { get; set; }
        public double SoTien { get; set; }
        public string SoDT { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; }
        public string TenKhoaHoc { get; set; }
        public string TenHocVien { get; set; }
        public string TenPhuongThuc { get; set; }
    }
}