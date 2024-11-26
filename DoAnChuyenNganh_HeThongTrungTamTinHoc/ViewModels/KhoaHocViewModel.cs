using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class KhoaHocViewModel
    {
        public string MaKH { get; set; }
        public string TenKH { get; set; }
        public int SoHocVien { get; set; }
        public bool MoLop { get; set; }
        public bool TrangThai { get; set; }
    }
}