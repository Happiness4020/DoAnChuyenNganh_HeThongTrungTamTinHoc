using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class HocVienViewModel
    {
        public string MaHV { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string Email { get; set; }
        public string SoDT { get; set; }
        public string DiaChi { get; set; }
        public float? DiemKiemTraLan1 { get; set; }
        public float? DiemKiemTraLan2 { get; set; }
        public float? DiemKiemTraLan3 { get; set; }
        public float? DiemTrungBinh { get; set; }
        public int Sobuoivang { get; set; }
        public string KetQua { get; set; }
        public bool DiemDanh { get; set; }
        public bool? ChoPhepNhapDiem { get; set; }
    }
}