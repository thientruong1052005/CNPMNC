using ShopThoiTrang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopThoiTrang.Controllers
{
    public class AdminController : Controller
    {
        ShopThoiTrangEntities db = new ShopThoiTrangEntities();
        // GET: Admin
        public ActionResult TrangChu()
        {
            var totalProducts = db.SanPham.Count();
            var totalAccounts = db.TaiKhoan.Count();
            var totalCategories = db.DanhMuc.Count();
            var totalRevenue = db.DonHang.Where(u=>u.TrangThaiDonHang != "Đã hủy" && u.TrangThaiDonHang != "Đang chờ xử lý").Sum(h => h.TongTien); // Tính tổng doanh thu

            // Lấy số lượng đơn hàng theo trạng thái
            var pendingOrders = db.DonHang.Count(h => h.TrangThaiDonHang == "Đang chờ xử lý");
            var approvedOrders = db.DonHang.Count(h => h.TrangThaiDonHang == "Đã duyệt");
            var paidOrders = db.DonHang.Count(h => h.TrangThaiDonHang == "Đã thanh toán");
            var deliveredOrders = db.DonHang.Count(h => h.TrangThaiDonHang == "Đã hủy");

            // Gán dữ liệu cho ViewBag
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalAccounts = totalAccounts;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.ApprovedOrders = approvedOrders;
            ViewBag.PaidOrders = paidOrders;
            ViewBag.CancelledOrders = deliveredOrders;

            return View();
        }
        public ActionResult DangNhap() { return View(); }
        // Xử lý đăng nhập
        [HttpPost]
        public ActionResult DangNhap(string tenDangNhap, string matKhau)
        {
            // Tìm kiếm tài khoản với tên đăng nhập và mật khẩu đã nhập
            var taiKhoan = db.TaiKhoan.FirstOrDefault(t => t.Username == tenDangNhap && t.Password == matKhau);

            if (taiKhoan != null)
            {
                // Kiểm tra quyền của tài khoản
                if (taiKhoan.MaQuyen == 1) // 1 là quyền admin
                {
                    // Đăng nhập thành công, lưu thông tin tài khoản vào session
                    Session["Admin"] = taiKhoan;

                    // Chuyển hướng đến trang quản lý admin
                    return RedirectToAction("TrangChu");
                }
                else
                {
                    // Nếu không phải admin
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này.";
                    return View();
                }
            }
            else
            {
                // Sai tài khoản hoặc mật khẩu
                ViewBag.ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                return View();
            }
        }

        // Đăng xuất
        public ActionResult DangXuat()
        {
            // Xóa thông tin admin khỏi session
            Session["Admin"] = null;
            return RedirectToAction("DangNhap");
        }
        // Quản lý Màu Sắc
        public ActionResult QLMauSac(string searchString, int page = 1, int pageSize = 5)
        {
            var mauSacList = db.MauSac.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                mauSacList = mauSacList.Where(m => m.TenMau.Contains(searchString));
            }

            // Tính toán để lấy dữ liệu theo trang
            var totalItems = mauSacList.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var mauSacPaged = mauSacList.OrderBy(m => m.MaMau)
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToList();

            // Truyền thông tin phân trang về view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(mauSacPaged);
        }
        public ActionResult ThemMau()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMau(MauSac mauSac)
        {
            if (ModelState.IsValid)
            {
                mauSac.TrangThai = true;
                db.MauSac.Add(mauSac);
                db.SaveChanges();
                return RedirectToAction("QLMauSac");
            }

            return View(mauSac);
        }
        public ActionResult SuaMau(int id)
        {
            var mauSac = db.MauSac.Find(id);
            if (mauSac == null)
            {
                return HttpNotFound();
            }
            return View(mauSac);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaMau(MauSac mauSac)
        {
            if (ModelState.IsValid)
            {
                var exstingmau = db.MauSac.Find(mauSac.MaMau);
                exstingmau.TenMau = mauSac.TenMau;
                db.SaveChanges();
                return RedirectToAction("QLMauSac");
            }
            return View(mauSac);
        }

        public ActionResult ThayDoiTrangThaiMau(int id)
        {
            var mauSac = db.MauSac.Find(id);
            if (mauSac != null)
            {
                mauSac.TrangThai = !mauSac.TrangThai;
                db.SaveChanges();
            }
            return RedirectToAction("QLMauSac");
        }
        // Quản lý Size
        public ActionResult QLSize(string searchString, int page = 1, int pageSize = 5)
        {
            var sizeList = db.Size.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                sizeList = sizeList.Where(s => s.TenSize.Contains(searchString));
            }

            // Tính toán để lấy dữ liệu theo trang
            var totalItems = sizeList.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var sizePaged = sizeList.OrderBy(s => s.MaSize)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            // Truyền thông tin phân trang về view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(sizePaged);
        }

        public ActionResult ThemSize()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSize(Size size)
        {
            if (ModelState.IsValid)
            {
                size.TrangThai = true;
                db.Size.Add(size);
                db.SaveChanges();
                return RedirectToAction("QLSize");
            }

            return View(size);
        }

        public ActionResult SuaSize(int id)
        {
            var size = db.Size.Find(id);
            if (size == null)
            {
                return HttpNotFound();
            }
            return View(size);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSize(Size size)
        {
            if (ModelState.IsValid)
            {
                // Tìm bản ghi hiện có trong cơ sở dữ liệu
                var existingSize = db.Size.Find(size.MaSize);
                // Cập nhật các thuộc tính của size
                existingSize.TenSize = size.TenSize;

                db.SaveChanges();
                return RedirectToAction("QLSize");
            }
            return View(size);
        }

        public ActionResult ThayDoiTrangThaiSize(int id)
        {
            var size = db.Size.Find(id);
            if (size != null)
            {
                size.TrangThai = !size.TrangThai;
                db.SaveChanges();
            }
            return RedirectToAction("QLSize");
        }
        // Action: Quản lý danh mục
        public ActionResult QLDanhMuc(string searchString, int page = 1, int pageSize = 5)
        {
            // Lấy danh sách danh mục
            var danhMucList = db.DanhMuc.AsQueryable();

            // Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                danhMucList = danhMucList.Where(d => d.TenDanhMuc.Contains(searchString));
            }

            // Sắp xếp và phân trang
            var danhMucPaged = danhMucList.OrderBy(d => d.MaDanhMuc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // Tính tổng số trang
            ViewBag.TotalPages = (int)Math.Ceiling((double)danhMucList.Count() / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;

            return View(danhMucPaged);
        }
        public ActionResult ThemDanhMuc()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhMuc(DanhMuc danhmuc)
        {
            if (ModelState.IsValid)
            {
                danhmuc.TrangThai = true;
                db.DanhMuc.Add(danhmuc);
                db.SaveChanges();
                return RedirectToAction("QLDanhMuc");
            }

            return View(danhmuc);
        }
        // Action: Sửa danh mục
        public ActionResult SuaDanhMuc(int id)
        {
            var danhMuc = db.DanhMuc.Find(id);
            if (danhMuc == null)
            {
                return HttpNotFound();
            }
            return View(danhMuc);
        }

        [HttpPost]
        public ActionResult SuaDanhMuc(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                var tontaidanhmuc = db.DanhMuc.Find(danhMuc.MaDanhMuc);
                tontaidanhmuc.TenDanhMuc = danhMuc.TenDanhMuc;
                db.SaveChanges();
                return RedirectToAction("QLDanhMuc");
            }
            return View(danhMuc);
        }

        // Action: Thay đổi trạng thái danh mục
        public ActionResult ThayDoiTrangThaiDanhMuc(int id)
        {
            var danhMuc = db.DanhMuc.Find(id);
            if (danhMuc != null)
            {
                danhMuc.TrangThai = !danhMuc.TrangThai; // Đổi trạng thái
                db.SaveChanges();
            }
            return RedirectToAction("QLDanhMuc");
        }
        public ActionResult QLTaiKhoan(string searchTerm = null, int page = 1)
        {
            int pageSize = 5; // Số lượng tài khoản mỗi trang
            var taiKhoans = db.TaiKhoan.AsQueryable().Where(u=>u.MaQuyen !=1);

            // Tìm kiếm tài khoản theo tên hoặc họ tên
            if (!string.IsNullOrEmpty(searchTerm))
            {
                taiKhoans = taiKhoans.Where(t => t.Username.Contains(searchTerm) || t.HoTen.Contains(searchTerm));
            }

            // Tính toán số lượng trang
            int totalItems = taiKhoans.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách tài khoản cho trang hiện tại
            var accounts = taiKhoans.OrderBy(t => t.MaTaiKhoan)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(accounts);
        }
    
        public ActionResult KhoaTaiKhoan(int id)
        {
            var taiKhoan = db.TaiKhoan.Find(id);
            if (taiKhoan != null)
            {
                taiKhoan.TrangThai = false; // Khóa tài khoản
                db.SaveChanges();
            }
            return RedirectToAction("QLTaiKhoan");
        }


        public ActionResult MoKhoaTaiKhoan(int id)
        {
            var taiKhoan = db.TaiKhoan.Find(id);
            if (taiKhoan != null)
            {
                taiKhoan.TrangThai = true; // Mở khóa tài khoản
                db.SaveChanges();
            }
            return RedirectToAction("QLTaiKhoan");
        }
        public ActionResult QLDonHang(string searchTerm = null, int page = 1)
        {
            int pageSize = 5; // Số lượng đơn hàng mỗi trang
            var donHangs = db.DonHang.AsQueryable();

            // Tìm kiếm đơn hàng theo trạng thái hoặc ngày đặt hàng
            if (!string.IsNullOrEmpty(searchTerm))
            {
                donHangs = donHangs.Where(d => d.TrangThaiDonHang.Contains(searchTerm));
            }

            // Tính toán số lượng trang
            int totalItems = donHangs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách đơn hàng cho trang hiện tại
            var orders = donHangs.OrderBy(d => d.MaDonHang)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }


        public ActionResult DuyetDonHang(long id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang != null)
            {
                donHang.TrangThaiDonHang = "Đã duyệt"; // Hoặc trạng thái bạn muốn
                db.SaveChanges();
            }
            return RedirectToAction("QLDonHang");
        }

     
        public ActionResult HuyDonHang(long id)
        {
            var donHang = db.DonHang.Find(id);
            if (donHang != null)
            {
                donHang.TrangThaiDonHang = "Đã hủy"; // Hoặc trạng thái hủy bạn muốn
                                                     // Lặp qua chi tiết đơn hàng để cập nhật số lượng sản phẩm trong kho
                foreach (var chiTiet in donHang.ChiTietDonHang)
                {
                    // Tìm sản phẩm trong kho tương ứng với chi tiết đơn hàng
                    var sanPham = db.SanPham_MauSac_Size.Find(chiTiet.MaSanPhamMauSacSize);
                    if (sanPham != null)
                    {
                        // Cập nhật số lượng sản phẩm trong kho
                        sanPham.SoLuong += chiTiet.SoLuong; 
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("QLDonHang");
        }

        public ActionResult ChiTietDonHang(long id)
        {
            var donHang = db.DonHang.FirstOrDefault(d => d.MaDonHang == id);
            if (donHang == null)
            {
                return HttpNotFound();
            }
            return View(donHang);
        }
        public ActionResult QLSanPham(string searchTerm = null, int page = 1)
        {
            int pageSize = 10; // Số lượng sản phẩm mỗi trang
            var sanPhams = db.SanPham_MauSac_Size.AsQueryable();

            // Tìm kiếm sản phẩm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sanPhams = sanPhams.Where(sp => sp.SanPham.TenSanPham.Contains(searchTerm) || sp.SanPham.MoTa.Contains(searchTerm));
            }

            // Tính toán tổng số trang
            int totalItems = sanPhams.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách sản phẩm cho trang hiện tại
            var products = sanPhams.OrderBy(sp => sp.MaSanPham)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            // Truyền dữ liệu qua ViewBag để hiển thị trên View
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }
        // GET: Thêm sản phẩm
        public ActionResult ThemSanPham()
        {
            ViewBag.DanhMucList = db.DanhMuc.ToList(); // Danh sách danh mục
            ViewBag.MauSacList = db.MauSac.ToList(); // Danh sách màu sắc
            ViewBag.SizeList = db.Size.ToList(); // Danh sách kích cỡ
            ViewBag.DoiTuongList = db.DoiTuong.ToList(); // Danh sách đối tượng
            return View();
        }

        // POST: Thêm sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SanPham sanPham, HttpPostedFileBase HinhAnh, int[] selectedMauSac, List<int[]> selectedSize, List<int[]> soLuong, int[] selectedDoiTuong, List<HttpPostedFileBase> HinhKemTheo)
        {
            if (selectedSize == null || !selectedSize.Any())
            {
                ViewBag.Sizes = null; // Hoặc ViewBag.Sizes = "Không có kích cỡ nào được chọn.";
            }
            else
            {
                ViewBag.Sizes = selectedSize;
                return View(sanPham);
            }
            if (ModelState.IsValid)
            {
                sanPham.NgayTao = DateTime.Now;
                sanPham.TrangThai = true; // Mặc định sản phẩm đang bán

                db.SanPham.Add(sanPham);
                db.SaveChanges();
                string doituong = "";
                if (sanPham.DoiTuong == 1)
                {
                    doituong = "women";
                }
                else if (sanPham.DoiTuong == 2)
                {
                    doituong = "men";
                }

                // Xử lý hình ảnh đại diện
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var directoryPath = Path.Combine(Server.MapPath($"~/images/{doituong}/{sanPham.MaSanPham}/"));
                    // Kiểm tra xem thư mục có tồn tại không, nếu không thì tạo nó
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    var path = Path.Combine(directoryPath, fileName);
                    HinhAnh.SaveAs(path);
                    sanPham.HinhAnh = fileName; // Lưu tên hình vào đối tượng sản phẩm
                    db.SaveChanges();
                }

                // Thêm màu sắc và kích cỡ vào bảng SanPham_MauSac_Size
                for (int i = 0; i < selectedMauSac.Length; i++)
                {
                    if (selectedMauSac[i] > 0)
                    {
                        // Tên của các trường trong form như selectedSize_1[], selectedSize_2[]...
                        var currentSize = Request.Form["selectedSize_" + selectedMauSac[i] + "[]"];
                        var currentSoLuong = Request.Form["soLuong_" + selectedMauSac[i] + "[]"];

                        // Kiểm tra xem currentSize và currentSoLuong có null không
                        if (!string.IsNullOrEmpty(currentSize) && !string.IsNullOrEmpty(currentSoLuong))
                        {
                            // Chuyển đổi thành mảng
                            var sizes = currentSize.Split(',').Select(int.Parse).ToArray();
                            var quantities = currentSoLuong.Split(',').Select(int.Parse).ToArray();

                            for (int j = 0; j < sizes.Length; j++)
                            {
                                if (sizes[j] > 0 && quantities.Length > j && quantities[j] > 0)
                                {
                                    var sanPhamMauSacSize = new SanPham_MauSac_Size
                                    {
                                        MaSanPham = sanPham.MaSanPham,
                                        MaMau = selectedMauSac[i],
                                        MaSize = sizes[j],
                                        SoLuong = quantities[j],
                                        TrangThai = true
                                    };

                                    db.SanPham_MauSac_Size.Add(sanPhamMauSacSize);
                                    db.SaveChanges();
                                }
                            }
                        }
                        }
                }

                // Xử lý hình ảnh kèm theo
                foreach (var file in HinhKemTheo)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath($"~/images/{doituong}/{sanPham.MaSanPham}/"), fileName);
                        file.SaveAs(path);

                        // Lưu tên hình vào cơ sở dữ liệu nếu cần
                        // Cần thêm mã để lưu hình ảnh vào bảng tương ứng
                    }
                }

                db.SaveChanges();
                return RedirectToAction("QLSanPham");
            }

            ViewBag.DanhMucList = db.DanhMuc.ToList();
            ViewBag.MauSacList = db.MauSac.ToList();
            ViewBag.SizeList = db.Size.ToList();
            ViewBag.DoiTuongList = db.DoiTuong.ToList();
            return View(sanPham);
        }

        public ActionResult SuaSanPham(int id)
        {
            var sanPhamMauSacSize = db.SanPham_MauSac_Size.Find(id);
            if (sanPhamMauSacSize == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách màu sắc và kích cỡ từ cơ sở dữ liệu
            ViewBag.MauSacList = new SelectList(db.MauSac.Where(u=>u.TrangThai == true).ToList(), "MaMau", "TenMau", sanPhamMauSacSize.MaMau);
            ViewBag.SizeList = new SelectList(db.Size.Where(u => u.TrangThai == true).ToList(), "MaSize", "TenSize", sanPhamMauSacSize.MaSize);

            return View(sanPhamMauSacSize);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(SanPham_MauSac_Size model)
        {
            // Nếu Model không hợp lệ, ta cần thông báo lỗi
            if (ModelState.IsValid)
            {
                // Kiểm tra xem màu và kích cỡ đã tồn tại chưa
                var existingItem = db.SanPham_MauSac_Size.FirstOrDefault(sp => sp.MaSanPham == model.MaSanPham && sp.MaMau == model.MaMau && sp.MaSize == model.MaSize);
                if (existingItem != null && existingItem.MaSanPhamMauSacSize != model.MaSanPhamMauSacSize)
                {
                    ViewBag.ErrorMessage = "Màu sắc và kích thước này đã tồn tại cho sản phẩm khác.";
                    // Truyền lại danh sách màu sắc và kích cỡ để hiển thị trong View
                    ViewBag.MauSacList = new SelectList(db.MauSac.ToList(), "MaMau", "TenMau", model.MaMau);
                    ViewBag.SizeList = new SelectList(db.Size.ToList(), "MaSize", "TenSize", model.MaSize);
                    return View(model);
                }
                var sanpham = db.SanPham_MauSac_Size.Find(model.MaSanPhamMauSacSize);
                sanpham.MaMau = model.MaMau;
                sanpham.MaSize = model.MaSize;
                sanpham.SoLuong = model.SoLuong;

                db.SaveChanges();
                return RedirectToAction("QLSanPham");
            }

            // Nếu Model không hợp lệ, truyền lại danh sách màu sắc và kích cỡ
            ViewBag.MauSacList = new SelectList(db.MauSac.ToList(), "MaMau", "TenMau", model.MaMau);
            ViewBag.SizeList = new SelectList(db.Size.ToList(), "MaSize", "TenSize", model.MaSize);
            return View(model);
        }
        public ActionResult NgungBanSanPhamMauSacSize(int id)
        {
            var sanPham = db.SanPham_MauSac_Size.Find(id);
            if (sanPham != null)
            {
                sanPham.TrangThai = false; // Đánh dấu là ngừng bán
                db.SaveChanges();
            }
            return RedirectToAction("QLSanPham");
        }

        public ActionResult BanLaiSanPhamMauSacSize(int id)
        {
            var sanPham = db.SanPham_MauSac_Size.Find(id);
            if (sanPham != null)
            {
                sanPham.TrangThai = true; // Đánh dấu là đang bán
                db.SaveChanges();
            }
            return RedirectToAction("QLSanPham");
        }


    }
}