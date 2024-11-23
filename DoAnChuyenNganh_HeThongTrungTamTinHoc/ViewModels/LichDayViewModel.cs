using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LichDayViewModel
    {
        public List<LichHoc> LichHocList { get; set; }
        public List<GiaoVien> GiaoVienList { get; set; }
    }
}