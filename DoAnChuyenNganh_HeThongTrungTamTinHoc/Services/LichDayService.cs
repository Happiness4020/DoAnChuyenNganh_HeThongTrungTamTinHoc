using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DoAnChuyenNganh_HeThongTrungTamTinHoc.Models;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Services
{
    public class LichDayService
    {
        private readonly TrungTamTinHocEntities db;

        public LichDayService(TrungTamTinHocEntities DB)
        {
            db = DB;
        }

       
    }
}