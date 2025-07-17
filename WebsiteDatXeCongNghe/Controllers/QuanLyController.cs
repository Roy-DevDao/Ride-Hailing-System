using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteDatXeCongNghe.Models;
using WebsiteDatXeCongNghe.ViewModel.MultipleData;

namespace WebsiteDatXeCongNghe.Controllers
{
    public class QuanLyController : Controller
    {
        private DichVuDatXeWebsite3Entities db = new DichVuDatXeWebsite3Entities();
        // GET: QuanLy
        public ActionResult QuanLy()
        {
            var mymodel = new MultipleData();
            mymodel.khachHangs = db.KhachHangs.ToList();
            mymodel.thongTinDatXes = db.ThongTinDatXes.ToList();
            mymodel.taiKhoans = db.TaiKhoans.ToList();
            mymodel.taiXes = db.TaiXes.ToList();
            mymodel.danhGias = db.DanhGiaUngDungs.ToList();
            mymodel.khuyenMais = db.KhuyenMais.ToList();
            mymodel.thanhToans = db.ThanhToans.ToList();
            mymodel.danhGiaTaiXes = db.DanhGiaTaiXes.ToList();
            mymodel.taiXeNhanChuyenXes = db.TaiXeNhanChuyenXes.ToList();
            mymodel.taiKhoanNganHangs = db.TaiKhoanNganHangs.ToList();
            mymodel.quyenTaiKhoans = db.QuyenTaiKhoans.ToList();
            mymodel.phanQuyens = db.PhanQuyens.ToList();
            return View(mymodel);
        }

        [HttpGet]
        public ActionResult BangDieuKhien()
        {
            var mymodel = new MultipleData();

            // Retrieve top 3 invoice transactions with the most recent MaThanhToan
            var top3Invoices = db.ThanhToans.OrderByDescending(t => t.MaThanhToan).Take(3).ToList();

            // Verify top3Invoices contains data
            if (top3Invoices.Count > 0)
            {
                // Extract MaDatXe values from top3Invoices
                var maDatXes = top3Invoices.Select(t => t.MaDatXe).ToList();

                // Load ThongTinDatXes for corresponding MaDatXe in the top 3 invoices
                mymodel.thongTinDatXes = db.ThongTinDatXes.Where(tt => maDatXes.Contains(tt.MaDatXe)).ToList();

                // Load ThanhToans
                mymodel.thanhToans = top3Invoices;
            }

            return View(mymodel);
        }

        [HttpGet]
        public ActionResult GetTotalDoanhThu()
        {
            // Calculate the total sum of TongTien and divide by 30%
            decimal? totalDoanhThu = db.ThanhToans.Sum(t => (decimal?)t.TongTien) * 0.3m;

            return Json(totalDoanhThu, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTotalDoanhThuNamNay()
        {
            // Calculate the total sum of TongTien for the year 2023 and multiply by 0.3
            DateTime startOfYear = new DateTime(2024, 1, 1);
            DateTime endOfYear = new DateTime(2024, 12, 31);

            decimal? totalDoanhThuNamNay = db.ThanhToans
                .Where(t => t.NgayThanhToan >= startOfYear && t.NgayThanhToan <= endOfYear)
                .Sum(t => (decimal?)t.TongTien) * 0.3m;

            return Json(totalDoanhThuNamNay, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTotalDoanhThuHomNay()
        {
            // Get the current date and time in Vietnam's timezone (UTC+7)
            DateTime nowInVietnam = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            // Calculate the total sum of TongTien for today
            DateTime todayStart = nowInVietnam.Date;
            DateTime todayEnd = todayStart.AddDays(1);

            decimal? totalDoanhThuHomNay = db.ThanhToans
                .Where(t => t.NgayThanhToan >= todayStart && t.NgayThanhToan < todayEnd)
                .Sum(t => (decimal?)t.TongTien) * 0.3m;

            return Json(totalDoanhThuHomNay ?? 0m, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetTotalBill()
        {
            // Count the total number of MaThanhToan
            int totalBill = db.ThanhToans.Count();

            return Json(totalBill, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTotalDriver()
        {
            // Count the total number of MaThanhToan
            int GetTotalDriver = db.TaiXes.Count();

            return Json(GetTotalDriver, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTotalPassenger()
        {
            // Count the total number of MaThanhToan
            int GetTotalPassenger = db.KhachHangs.Count();

            return Json(GetTotalPassenger, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult LoadBarChart(int? selectedYear)
        //{
        //    // Ensure a valid selectedYear is provided, otherwise default to the current year
        //    selectedYear = selectedYear ?? DateTime.Now.Year;

        //    // Retrieve ThanhToans data for the selected year
        //    var thanhToansForYear = db.ThanhToans
        //        .Where(t => DbFunctions.TruncateTime(t.NgayThanhToan).Value.Year == selectedYear)
        //        .Select(t => new
        //        {
        //            t.MaThanhToan,
        //            t.TongTien,
        //            t.NgayThanhToan,
        //            // Add other necessary properties
        //        })
        //        .ToList();

        //    return Json(thanhToansForYear, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult LoadBarChart(int? selectedYear)
        {
            var mymodel = new MultipleData();

            // Filter ThanhToans based on the selected year
            var invoices = db.ThanhToans
                .Where(t => selectedYear == null || t.NgayThanhToan.Year == selectedYear)
                .ToList();

            // Group invoices by month and calculate the rounded sum of TongTien for each month
            var monthlyRevenue = invoices
                .GroupBy(t => t.NgayThanhToan.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    TotalRevenue = Math.Round((double)(group.Sum(t => t.TongTien) ?? 0) * 0.3, 3) // Rounded to 3 decimal places
                })
                .OrderBy(entry => entry.Month) // Keep ascending order of months
                .ToList();

            return Json(monthlyRevenue, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActivePhoneNumbersCount()
        {
            var activePhoneNumbersCount = db.TrangThaiHoatDongs
                .Count(t => (bool)t.TrangThai);

            return Json(activePhoneNumbersCount, JsonRequestBehavior.AllowGet);
        }



        public ActionResult QuanLyKhachHang()
        {
            var mymodel = new MultipleData();
            mymodel.khachHangs = db.KhachHangs.ToList();
            
            return View(mymodel);
        }

        public ActionResult QuanLyThongTinDatXe()
        {
            var mymodel = new MultipleData();
            mymodel.thongTinDatXes = db.ThongTinDatXes.ToList();
            mymodel.khachHangs = db.KhachHangs.ToList();
           
            return View(mymodel);
        }

        public ActionResult QuanLyTaiKhoan()
        {
            var mymodel = new MultipleData();
            mymodel.taiKhoans = db.TaiKhoans.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyTaiXe()
        {
            var mymodel = new MultipleData();
            mymodel.taiXes = db.TaiXes.ToList();
            mymodel.trangThaiHoatDongs = db.TrangThaiHoatDongs.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyDanhGia()
        {
            var mymodel = new MultipleData();
            mymodel.danhGias = db.DanhGiaUngDungs.ToList();
            mymodel.khachHangs = db.KhachHangs.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyKhuyenMai()
        {
            var mymodel = new MultipleData();
            mymodel.khuyenMais = db.KhuyenMais.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyThanhToan()
        {
            var mymodel = new MultipleData();
            mymodel.thanhToans = db.ThanhToans.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyDanhGiaTaiXe()
        {
            var mymodel = new MultipleData();
            mymodel.danhGiaTaiXes = db.DanhGiaTaiXes.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyTaiXeNhanChuyenXe()
        {
            var mymodel = new MultipleData();
            mymodel.taiXeNhanChuyenXes = db.TaiXeNhanChuyenXes.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyTaiKhoanNganHang()
        {
            var mymodel = new MultipleData();
            mymodel.taiKhoanNganHangs = db.TaiKhoanNganHangs.ToList();

            return View(mymodel);
        }

        public ActionResult QuanLyQuyenTaiKhoan()
        {
            var mymodel = new MultipleData();
            mymodel.quyenTaiKhoans = db.QuyenTaiKhoans
                .Where(qtk => qtk.MaQuyen == 1 || qtk.MaQuyen == 5)
                .ToList();

            return View(mymodel);
        }


        public ActionResult QuanLyPhanQuyen()
        {
            var mymodel = new MultipleData();
            mymodel.phanQuyens = db.PhanQuyens.ToList();

            return View(mymodel);
        }
        [HttpPost]
        public ActionResult XoaQuyenTaiKhoan(int id)
        {
            var quyen = db.QuyenTaiKhoans.FirstOrDefault(q => q.MaQuyenTK == id);
            if (quyen != null)
            {
                db.QuyenTaiKhoans.Remove(quyen);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        [HttpPost]
        public ActionResult SuaQuyenTaiKhoan(int id, int maquyen, string sdt)
        {
            var quyen = db.QuyenTaiKhoans.FirstOrDefault(q => q.MaQuyenTK == id);
            if (quyen != null)
            {
                quyen.MaQuyen = maquyen;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        [HttpPost]
        public ActionResult XoaPhanQuyen(int id)
        {
            var quyen = db.PhanQuyens.FirstOrDefault(q => q.MaQuyen == id);
            if (quyen != null)
            {
                db.PhanQuyens.Remove(quyen);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        [HttpPost]
        public ActionResult SuaPhanQuyen(int id,string tenquyen)
        {
            var quyen = db.PhanQuyens.FirstOrDefault(q => q.MaQuyen == id);
            if (quyen != null)
            {
                quyen.TenQuyen = tenquyen;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        [HttpPost]
        public JsonResult SuaKhuyenMai(int id, string maCode, string ten, int giamgia,
    DateTime ngayBatDau, DateTime ngayKetThuc, string noiDung, bool trangThai)
        {
            try
            {
                // Validate dữ liệu
                if (string.IsNullOrEmpty(maCode) || string.IsNullOrEmpty(ten))
                {
                    return Json(new { success = false, message = "Thiếu thông tin bắt buộc" });
                }

                if (ngayKetThuc <= ngayBatDau)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });
                }

                if (giamgia < 0 || giamgia > 100)
                {
                    return Json(new { success = false, message = "Phần trăm giảm giá phải từ 0 đến 100" });
                }

                var km = db.KhuyenMais.FirstOrDefault(x => x.MaKhuyenMai == id);
                if (km != null)
                {
                    // Kiểm tra mã code có trùng không (trừ chính nó)
                    var existingCode = db.KhuyenMais.FirstOrDefault(x => x.MaCode == maCode && x.MaKhuyenMai != id);
                    if (existingCode != null)
                    {
                        return Json(new { success = false, message = "Mã code đã tồn tại" });
                    }

                    km.MaCode = maCode;
                    km.TenKhuyenMai = ten;
                    km.PhanTram = giamgia;
                    km.NgayBatDau = ngayBatDau;
                    km.NgayKetThuc = ngayKetThuc;
                    km.NoiDung = noiDung;
                    km.TrangThai = trangThai;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Cập nhật thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Error in SuaKhuyenMai: {ex.Message}");
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaKhuyenMai(int id)
        {
            var km = db.KhuyenMais.FirstOrDefault(x => x.MaKhuyenMai == id);
            if (km != null)
            {
                db.KhuyenMais.Remove(km);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public JsonResult ThemKhuyenMai(string maCode, string ten, int giamgia, DateTime ngayBatDau, DateTime ngayKetThuc, string noiDung, bool trangThai)
        {
            try
            {
                var existed = db.KhuyenMais.Any(km => km.MaCode.ToLower() == maCode.ToLower());
                if (existed)
                {
                    return Json(new { success = false, message = "Mã khuyến mãi đã tồn tại!" });
                }
                var khuyenMai = new KhuyenMai
                {
                    MaCode = maCode,
                    TenKhuyenMai = ten,
                    PhanTram = giamgia,
                    NgayBatDau = ngayBatDau,
                    NgayKetThuc = ngayKetThuc,
                    NoiDung = noiDung,
                    TrangThai = trangThai
                };

                // Lưu vào DB
                db.KhuyenMais.Add(khuyenMai);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult ThemPhanQuyen(string tenQuyen)
        {
            try
            {
                // Kiểm tra tên quyền có bị trùng không (nếu muốn)
                var existed = db.PhanQuyens.Any(p => p.TenQuyen.ToLower() == tenQuyen.ToLower());
                if (existed)
                {
                    return Json(new { success = false, message = "Tên quyền đã tồn tại!" });
                }

                var newQuyen = new PhanQuyen
                {
                    TenQuyen = tenQuyen
                };

                db.PhanQuyens.Add(newQuyen);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult ThemQuyenTaiKhoan(string sdt, int maquyen)
        {
            try
            {
                var user = db.TaiKhoans.FirstOrDefault(x => x.SoDienThoai == sdt);
                if (user == null)
                {
                    return Json(new { success = false, message = "Số điện thoại không tồn tại trong hệ thống." });
                }

                var existed = db.QuyenTaiKhoans.Any(x => x.SoDienThoai == sdt);
                if (existed)
                {
                    return Json(new { success = false, message = "Tài khoản này đã có quyền." });
                }

                var qtk = new QuyenTaiKhoan
                {
                    SoDienThoai = sdt,
                    MaQuyen = maquyen
                };

                db.QuyenTaiKhoans.Add(qtk);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
        [HttpPost]
        public ActionResult ThemTaiXe(HttpPostedFileBase HinhAnh, string Ten, DateTime NgayThangNamSinh, string Email, string SoDienThoai, string BienSo, string CCCD, DateTime NgayDangKy)
        {
            try
            {
                if (HinhAnh == null || HinhAnh.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Ảnh đại diện không được để trống!" });
                }

                string fileName = Path.GetFileName(HinhAnh.FileName);
                string path = Path.Combine(Server.MapPath("~/image/TaiXe"), fileName);
                HinhAnh.SaveAs(path);

                var tx = new TaiXe
                {
                    Ten = Ten,
                    NgayThangNamSinh = NgayThangNamSinh,
                    Email = Email,
                    SoDienThoai = SoDienThoai,
                    BienSo = BienSo,
                    CCCD = CCCD,
                    HinhAnh = fileName,
                    NgayDangKy = NgayDangKy,
                    DiemUyTin = 0,
                    ViTien = 0,
                    MucDoDanhGiaTB = null,
                    ViTri = " "
                };

                db.TaiXes.Add(tx);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult XoaTaiXe(int id)
        {
            var tx = db.TaiXes.FirstOrDefault(x => x.MaTX == id);
            if (tx != null)
            {
                db.TaiXes.Remove(tx);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

    }
}