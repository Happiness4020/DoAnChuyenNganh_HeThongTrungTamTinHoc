using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LopHocViewModel
    {
        public string MaLH { get; set; }
        public string TenPhong { get; set; }
        public string MaKH { get; set; }
        public int SiSo { get; set; }
        public System.TimeSpan GioBatDau { get; set; }
        public System.TimeSpan GioKetThuc { get; set; }
        public string MaGV { get; set; }
        public bool TrangThai { get; set; }
        public string ThuHoc { get; set; }
        public LopHoc LopHoc { get; set; }
        public int SoLuongHocVien { get; set; }
        public string GiaoVienHoTen { get; set; }
        public string KhoaHocTenKH { get; set; }
        public bool? ChoPhepNhapDiem { get; set; }
    }
}