using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.ViewModels
{
    public class LichDayTheoTuanViewModel
    {
        public int Week { get; set; }
        public List<LichDay> Days { get; set; }
    }
}