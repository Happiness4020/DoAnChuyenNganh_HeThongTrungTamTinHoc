using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnChuyenNganh_HeThongTrungTamTinHoc.Areas.Admin.Controllers
{
    public class AdminSaoLuuPhucHoiController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TrungTamTinHocEntities"].ConnectionString;
        // GET: Admin/AdminSaoLuuPhucHoi
        public ActionResult SaoLuuPhucHoi()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaoLuu()
        {
            try
            {
                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(connectionString);
                string sqlConnectionString = entityBuilder.ProviderConnectionString;
                string duongdansaoluu = Server.MapPath("~/App_Data/Backup/");
                if (!Directory.Exists(duongdansaoluu))
                {
                    Directory.CreateDirectory(duongdansaoluu);
                }

                string tenfile = $"DatabaseBackup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string noisaoluu = Path.Combine(duongdansaoluu, tenfile);

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    string backupQuery = $"BACKUP DATABASE [QL_TrungTamTinHoc] TO DISK = @BackupPath";
                    using (var command = new SqlCommand(backupQuery, connection))
                    {
                        command.Parameters.AddWithValue("@BackupPath", noisaoluu);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = $"Đã sao lưu thành công dữ liệu. Tệp sao lưu: {tenfile}";
                return RedirectToAction("SaoLuuPhucHoi");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi thực hiện sao lưu dữ liệu: {ex.Message}";
                return RedirectToAction("SaoLuuPhucHoi");
            }
        }

        [HttpPost]
        public ActionResult PhucHoi(HttpPostedFileBase filephuchoi)
        {
            try
            {
                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(connectionString);
                string sqlConnectionString = entityBuilder.ProviderConnectionString;
                if (filephuchoi != null && filephuchoi.ContentLength > 0)
                {
                    string duongdansaoluu = Server.MapPath("~/App_Data/Backup/");
                    string noisaoluu = Path.Combine(duongdansaoluu, filephuchoi.FileName);

                    if (!Directory.Exists(duongdansaoluu))
                    {
                        Directory.CreateDirectory(duongdansaoluu);
                    }

                    filephuchoi.SaveAs(noisaoluu);

                    string masterConnectionString = sqlConnectionString.Replace("QL_TrungTamTinHoc", "master");
                    using (var ketnoi = new SqlConnection(masterConnectionString))
                    {
                        string chuoiphuchoi = @"
                            ALTER DATABASE [QL_TrungTamTinHoc] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            RESTORE DATABASE [QL_TrungTamTinHoc] FROM DISK = @BackupPath WITH REPLACE;
                            ALTER DATABASE [QL_TrungTamTinHoc] SET MULTI_USER;";
                        using (var command = new SqlCommand(chuoiphuchoi, ketnoi))
                        {
                            command.Parameters.AddWithValue("@BackupPath", noisaoluu);
                            ketnoi.Open();
                            command.ExecuteNonQuery();
                        }
                    }

                    TempData["SuccessMessage"] = "Phục hồi dữ liệu thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Tệp sao lưu trống hoặc không hợp lệ!!! Hãy chọn tệp khác.";
                }
                return RedirectToAction("SaoLuuPhucHoi");
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi phục hồi dữ liệu: {ex.Message}";
                return RedirectToAction("SaoLuuPhucHoi");
            }
        }
    }
}