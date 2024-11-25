using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class KhoaHocNoiBatViewModel
    {
        public string MaKH { get; set; }
        public string TenKH { get; set; }
        public string Anh { get; set; }
        public DateTime NgayBatDau { get; set; }
        public double HocPhi { get; set; }
        public int SoLanDangKy { get; set; }
    }
}