using ShopThoiTrang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity;
using System.Configuration;
using System.Diagnostics;
namespace ShopThoiTrang.Controllers
{
    public class HomeController : Controller
    {
        ShopThoiTrangEntities db = new ShopThoiTrangEntities();
        public ActionResult TrangChu()
        {
            // Lấy 6 sản phẩm cho đối tượng sản phẩm dành cho phụ nữ
            var sanPhamPhuNu = db.SanPham
                                 .Where(sp => sp.DoiTuong == 1 && sp.TrangThai && sp.DanhMuc.TrangThai==true)
                                 .OrderByDescending(sp => sp.NgayTao)
                                 .Take(6)
                                 .ToList();

            // Lấy 6 sản phẩm cho đối tượng sản phẩm dành cho đàn ông
            var sanPhamDanOng = db.SanPham
                                  .Where(sp => sp.DoiTuong == 2 && sp.TrangThai && sp.DanhMuc.TrangThai == true)
                                  .OrderByDescending(sp => sp.NgayTao)
                                  .Take(6)
                                  .ToList();

            // Truyền dữ liệu vào ViewBag
            ViewBag.SanPhamPhuNu = sanPhamPhuNu;
            ViewBag.SanPhamDanOng = sanPhamDanOng;

            return View();
        }


        public ActionResult GioiThieu()
        { 

            return View();
        }
        public ActionResult HoSo()
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }
            var tt = db.TaiKhoan.Find(user.MaTaiKhoan);
            return View(tt);
        }
        [HttpPost]
        public ActionResult HoSo(TaiKhoan updatedUser, string newPassword,string renewPassword)
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }
            var existingUser = db.TaiKhoan.Find(user.MaTaiKhoan);
            // Cập nhật thông tin người dùng
            existingUser.HoTen = updatedUser.HoTen;
            existingUser.Email = updatedUser.Email;
            existingUser.SoDienThoai = updatedUser.SoDienThoai;
            existingUser.DiaChi = updatedUser.DiaChi;
            if (newPassword != renewPassword)
            {
                ViewBag.Message = "Mật khẩu nhập lại phải trùng khớp!";
            }
            // Nếu người dùng nhập mật khẩu mới, cập nhật mật khẩu
            if (!string.IsNullOrEmpty(newPassword))
            {
      
                existingUser.Password = newPassword; // Chú ý: Nên mã hóa mật khẩu trước khi lưu
            }

            // Lưu thông tin người dùng vào cơ sở dữ liệu
            db.Entry(existingUser).State = EntityState.Modified;
            db.SaveChanges();
            // Cập nhật lại session
            Session["User"] = existingUser;

            // Quay lại trang hồ sơ với thông báo thành công
            ViewBag.Message = "Cập nhật thông tin thành công!";
            return View(user);
        }

        public ActionResult LienHe()
        {
         

            return View();
        }
        [HttpPost]
        public ActionResult LienHe(string con_name, string con_email, string con_message)
        {
            try
            {
                // Tạo email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("kienpro0987@gmail.com"); // Địa chỉ email của bạn
                mail.To.Add("kienpro0987@gmail.com"); // Gửi email tới bạn, có thể thay đổi theo yêu cầu
                mail.Subject = "Liên hệ từ: " + con_name;
                mail.Body = $"Xin chào,\n\nBạn nhận được một tin nhắn mới từ {con_name}.\nEmail: {con_email}\nNội dung tin nhắn: {con_message}";
                mail.IsBodyHtml = false;

                // Cấu hình SMTP
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587); // SMTP server, ví dụ Gmail
                smtpClient.Credentials = new NetworkCredential("kienpro0987@gmail.com", "nfcq qelz hnyl elcz"); // Đăng nhập email
                smtpClient.EnableSsl = true; // Sử dụng SSL

                // Gửi email
                smtpClient.Send(mail);

                // Thông báo gửi thành công
                ViewBag.Message = "Gửi email thành công!";
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                ViewBag.Message = "Gửi email thất bại: " + ex.Message;
            }

            return View();
        }

        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(TaiKhoan taiKhoan)
        {
            if (ModelState.IsValid)
            {
                //kiểm tra tài khoản có trong bảng TaiKhoan phù hợp với Username và Password
                var user = db.TaiKhoan.FirstOrDefault(u => u.Username == taiKhoan.Username && u.Password == taiKhoan.Password);
                if (user != null)
                { 
                    //Kiểm tra xem tài khoản có bị khóa
                    if (user.TrangThai == true)
                    {
                        // Lưu thông tin người dùng vào session hoặc cookie
                        Session["User"] = user;
                        return RedirectToAction("TrangChu", "Home"); // Chuyển hướng về trang chính
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Tài khoản bị khóa.";
                        return RedirectToAction("DangNhap", "Home");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Tài khoản hoặc mật khẩu không đúng.";
                    return RedirectToAction("DangNhap", "Home");
                }
            }
            return View(taiKhoan);
        }
        public ActionResult DangXuat()
        {
            Session.Remove("User");
            return RedirectToAction("TrangChu","Home");
        }
        // POST: Đăng Ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(TaiKhoan taiKhoan, string PasswordConfirm)
        {
            // Kiểm tra xem dữ liệu đầu vào có hợp lệ không
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên tài khoản đã tồn tại trong cơ sở dữ liệu hay chưa
                if (db.TaiKhoan.Any(u => u.Username == taiKhoan.Username))
                {
                    TempData["ErrorMessage"] = "Tài khoản đã tồn tại."; // Thêm thông báo lỗi
                    return RedirectToAction("DangNhap", "Home"); // Chuyển hướng về trang đăng nhập
                }

                // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
                if (db.TaiKhoan.Any(u => u.Email == taiKhoan.Email))
                {
                    TempData["ErrorMessage"] = "Email đã tồn tại."; // Thêm thông báo lỗi
                    return RedirectToAction("DangNhap", "Home"); // Chuyển hướng về trang đăng nhập
                }

                // Kiểm tra xem số điện thoại đã tồn tại trong cơ sở dữ liệu hay chưa
                if (db.TaiKhoan.Any(u => u.SoDienThoai == taiKhoan.SoDienThoai))
                {
                    TempData["ErrorMessage"] = "Số điện thoại đã tồn tại."; // Thêm thông báo lỗi
                    return RedirectToAction("DangNhap", "Home"); // Chuyển hướng về trang đăng nhập
                }

                // Kiểm tra mật khẩu và mật khẩu nhập lại
                if (taiKhoan.Password != PasswordConfirm)
                {
                    TempData["ErrorMessage"] = "Mật khẩu và nhập lại mật khẩu không khớp."; // Thêm thông báo lỗi
                    return RedirectToAction("DangNhap", "Home"); // Chuyển hướng về trang đăng nhập
                }
                // Nếu tất cả các kiểm tra trên đều không có lỗi, thêm tài khoản mới vào cơ sở dữ liệu
                taiKhoan.TrangThai = true; // Đặt trạng thái tài khoản (có thể điều chỉnh theo yêu cầu)
                taiKhoan.MaQuyen = 2; //set quyền là user
                db.TaiKhoan.Add(taiKhoan); // Thêm tài khoản mới vào db
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                // Thêm thông báo thành công vào TempData
                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("DangNhap","Home"); // Chuyển hướng về trang đăng nhập
            }

            // Nếu ModelState không hợp lệ, vẫn có thể redirect về trang đăng nhập
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ."; // Thêm thông báo lỗi
            return RedirectToAction("DangNhap", "Home");
        }
        public ActionResult CuaHang(int id, string searchTerm = "", decimal? minPrice = null, decimal? maxPrice = null, int? category = null, int page = 1)
        {
            var products = db.SanPham.AsQueryable().Where(u=>u.DoiTuong == id && u.TrangThai == true && u.DanhMuc.TrangThai == true);

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.TenSanPham.Contains(searchTerm));
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Gia >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Gia <= maxPrice.Value);
            }

            // Lọc theo danh mục
            if (category.HasValue)
            {
                products = products.Where(p => p.MaDanhMuc == category.Value);
            }

            // Lấy danh sách danh mục
            ViewBag.DanhMucs = db.DanhMuc.Where(sp=>sp.TrangThai == true).ToList(); // Cần đảm bảo có lớp DanhMuc trong DbContext

            // Tính toán tổng số trang
            var pageSize = 8; // Số sản phẩm hiển thị trên một trang
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Lấy sản phẩm cho trang hiện tại
            var productList = products.OrderBy(p => p.TenSanPham)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();

            // Cung cấp dữ liệu cho view
            ViewBag.CurrentId = id;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Category = category;
            return View(productList);
        }
        public ActionResult ChiTietSanPham(int id)
        {
            // Lấy thông tin sản phẩm theo masanpham
            var sanpham = db.SanPham.Find(id);
            if (sanpham == null)
            {
                return HttpNotFound();
            }

            // Lấy các sản phẩm có giá chênh lệch 100.000
            var sanphamsTuongTu = db.SanPham
                .Where(sp => sp.Gia >= sanpham.Gia - 100000 && sp.Gia <= sanpham.Gia + 100000 && sp.MaSanPham != id && sp.DoiTuong== sanpham.DoiTuong)
                .ToList();

            ViewBag.SanPhamTuongTu = sanphamsTuongTu;

            // Lấy màu sắc và kích thước tương ứng của sản phẩm
            // Lấy màu sắc và kích thước tương ứng của sản phẩm
            var mauSac = db.SanPham_MauSac_Size
                .Where(s => s.MaSanPham == id && s.TrangThai && s.MauSac.TrangThai == true) // chỉ lấy các bản ghi có trạng thái true
                .GroupBy(s => s.MaMau) // Nhóm theo mã màu
                .Select(g => g.FirstOrDefault()) // Lấy một bản ghi từ mỗi nhóm
                .ToList();

            ViewBag.MauSac = mauSac;
            // Đường dẫn đến thư mục hình ảnh
            string imagePath = Server.MapPath("~/images/" + (sanpham.DoiTuong == 1 ? "women/" : "men/") + sanpham.MaSanPham + "/");

            // Lấy danh sách hình ảnh
            var images = Directory.GetFiles(imagePath, "*.*")
                .Where(file => file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png") || file.EndsWith(".webp"))
                .Select(Path.GetFileName) // Lấy tên tệp tin
                .ToList();

            ViewBag.Images = images; // Lưu danh sách hình ảnh vào ViewBag
            return View(sanpham);
        }
        public JsonResult LaySize(int colorId, int productId)
        {
            var sizes = db.SanPham_MauSac_Size
                .Where(s => s.MaMau == colorId && s.MaSanPham == productId && s.TrangThai && s.Size.TrangThai == true)
                .Select(s => new { s.MaSize, s.Size.TenSize })
                .ToList();

            return Json(sizes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LaySoLuongTonKho(int colorId, int sizeId, int productId)
        {
            // Lấy số lượng tồn kho từ bảng SanPham_MauSac_Size
            var quantity = db.SanPham_MauSac_Size
                .Where(s => s.MaMau == colorId && s.MaSize == sizeId && s.MaSanPham == productId)
                .Select(s => s.SoLuong)
                .FirstOrDefault();

            return Json(quantity, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GioHang()
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }

            // Lấy giỏ hàng của người dùng dựa trên MaTaiKhoan
            var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);

            // Kiểm tra nếu giỏ hàng không tồn tại
            if (cart == null)
            {
                return View(new List<ChiTietGioHang>()); // Trả về view trống nếu không có giỏ hàng
            }

            // Lấy danh sách các sản phẩm trong giỏ hàng
            var cartItems = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).ToList();

            // Tính tổng tiền dựa trên giá sản phẩm và số lượng
            var totalAmount = cartItems.Sum(item => item.SanPham_MauSac_Size.SanPham.Gia * item.SoLuong);

            // Truyền tổng tiền vào ViewBag để hiển thị trên View
            ViewBag.TotalAmount = totalAmount;

            // Trả về view với danh sách các sản phẩm trong giỏ hàng
            return View(cartItems);
        }

        [HttpPost]
        public ActionResult CapNhatGioHang(int id, int quantity)
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Lấy giỏ hàng của người dùng
            var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm chi tiết giỏ hàng theo sản phẩm
            var cartItem = db.ChiTietGioHang.SingleOrDefault(ct => ct.MaGioHang == cart.MaGioHang && ct.MaSanPhamMauSacSize == id);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }
            //Kiểm tra xem số lượng có lớn hơn số lượng tồn kho không 
            var sp = db.SanPham_MauSac_Size.SingleOrDefault(u => u.MaSanPhamMauSacSize == cartItem.MaSanPhamMauSacSize);
            if (quantity >  sp.SoLuong)
            {
                return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });
            }
            // Cập nhật số lượng
            cartItem.SoLuong = quantity;
            db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

          
            return Json(new { success = true, message = "Cập nhật thành công." });
        }
        [HttpPost]
        public ActionResult XoaGioHang(int id)
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            // Lấy giỏ hàng của người dùng
            var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm chi tiết giỏ hàng theo sản phẩm
            var cartItem = db.ChiTietGioHang.SingleOrDefault(ct => ct.MaGioHang == cart.MaGioHang && ct.MaSanPhamMauSacSize == id );
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            // Xóa sản phẩm khỏi giỏ hàng
            db.ChiTietGioHang.Remove(cartItem);
            db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            return Json(new { success = true, message = "Xóa sản phẩm thành công."  });
        }


        [HttpPost]
        public ActionResult ThemVaoGioHang(int maSanPham, int maMau, int maSize, int soLuong)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (Session["User"] == null)
            {
                return Json(new { message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            var user = Session["User"] as TaiKhoan;

            // Kiểm tra xem sản phẩm có tồn tại với màu và kích cỡ hay không
            var sanPham_MauSac_Size = db.SanPham_MauSac_Size
                .FirstOrDefault(s => s.MaSanPham == maSanPham && s.MaMau == maMau && s.MaSize == maSize);

            if (sanPham_MauSac_Size == null)
            {
                return Json(new { message = "Sản phẩm không tồn tại với màu và kích cỡ này." });
            }

            // Kiểm tra số lượng tồn kho
            if (sanPham_MauSac_Size.SoLuong < soLuong)
            {
                return Json(new { message = "Số lượng không đủ trong kho." });
            }

            // Kiểm tra xem giỏ hàng đã tồn tại chưa
            var gioHang = db.GioHang.FirstOrDefault(g => g.MaTaiKhoan == user.MaTaiKhoan);
            if (gioHang == null)
            {
                // Tạo giỏ hàng mới nếu chưa có
                gioHang = new GioHang
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    NgayTao = DateTime.Now
                };
                db.GioHang.Add(gioHang);
                db.SaveChanges();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var chiTietGioHang = db.ChiTietGioHang
                .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaSanPhamMauSacSize == sanPham_MauSac_Size.MaSanPhamMauSacSize);

            if (chiTietGioHang != null)
            {
                // Cập nhật số lượng nếu sản phẩm đã có trong giỏ hàng
                chiTietGioHang.SoLuong += soLuong;
            }
            else
            {
                // Thêm sản phẩm vào giỏ hàng
                chiTietGioHang = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSanPhamMauSacSize = sanPham_MauSac_Size.MaSanPhamMauSacSize,
                    SoLuong = soLuong
                };
                db.ChiTietGioHang.Add(chiTietGioHang);
            }

            // Lưu thay đổi vào database
            db.SaveChanges();

            return Json(new { message = "Sản phẩm đã được thêm vào giỏ hàng." });
        }
        [HttpPost]
        public ActionResult ThanhToan(string paymentMethod)
        {
            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập
            }


            if (paymentMethod == "cash")
            {
                // Lấy giỏ hàng của người dùng
                var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);
                if (cart == null)
                {
                    return RedirectToAction("Index", "Home"); // Chuyển đến trang chính nếu giỏ hàng không tồn tại
                }

                // Tạo đơn hàng
                var donHang = new DonHang
                {
                    MaDonHang = DateTime.Now.Ticks,
                    MaTaiKhoan = user.MaTaiKhoan,
                    NgayDatHang = DateTime.Now,
                    TrangThaiDonHang = "Đang chờ xử lý",
                    TongTien = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).Sum(item => item.SanPham_MauSac_Size.SanPham.Gia * item.SoLuong),

                };
                db.DonHang.Add(donHang);
                db.SaveChanges();
                Session["MaDH"] = donHang.MaDonHang;
                // Lưu chi tiết đơn hàng
                var chiTietGioHang = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).ToList();
                foreach (var item in chiTietGioHang)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaSanPhamMauSacSize = item.MaSanPhamMauSacSize,
                        SoLuong = item.SoLuong,
                        Gia = item.SanPham_MauSac_Size.SanPham.Gia
                    };
                    db.ChiTietDonHang.Add(chiTietDonHang);
                }
                db.SaveChanges();

                // Xóa giỏ hàng sau khi thanh toán
                db.ChiTietGioHang.RemoveRange(chiTietGioHang);
                db.SaveChanges();
                // Nếu thanh toán bằng tiền mặt, chuyển đến trang thanh toán thành công
                return RedirectToAction("ThanhToanThanhCong","Home",new {data = "Đơn hàng đang chờ duyệt vui lòng chờ"});
            }
            else if (paymentMethod == "bank_transfer")
            {
                return RedirectToAction("ThanhToanQuaThe");
            }

            return RedirectToAction("Index", "Home"); // Nếu không có phương thức thanh toán hợp lệ
        }
        public ActionResult ThanhToanQuaThe()
        {

            // Lấy tài khoản người dùng từ Session
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập
            }
            // Lấy giỏ hàng của người dùng dựa trên MaTaiKhoan
            var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);

            // Kiểm tra nếu giỏ hàng không tồn tại
            if (cart == null)
            {
                return View(new List<ChiTietGioHang>()); // Trả về view trống nếu không có giỏ hàng
            }

            // Lấy danh sách các sản phẩm trong giỏ hàng
            var cartItems = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).ToList();

            // Tính tổng tiền dựa trên giá sản phẩm và số lượng
            decimal tongTien = cartItems.Sum(item => item.SanPham_MauSac_Size.SanPham.Gia * item.SoLuong);

            DonHang dh = new DonHang
            {
                MaDonHang = DateTime.Now.Ticks,
                MaTaiKhoan = user.MaTaiKhoan,
                NgayDatHang = DateTime.Now,
                TrangThaiDonHang = "Đã thanh toán",
                TongTien = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).Sum(item => item.SanPham_MauSac_Size.SanPham.Gia * item.SoLuong),
            };
            db.DonHang.Add(dh);
            db.SaveChanges();
            var urlPayment = "";
            Session["MaDH"] = dh.MaDonHang;

            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key


            VnPayLibrary vnpay = new VnPayLibrary();
            long amount = (long)dh.TongTien + 50000;
            Debug.WriteLine(amount);
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            long madonhang = dh.MaDonHang;
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + madonhang);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", madonhang.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(urlPayment);
        }
        public ActionResult Return()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        long code = (long)Session["MaDH"];

                        // Lấy tài khoản người dùng từ Session
                        var user = Session["User"] as TaiKhoan;

                        // Kiểm tra nếu người dùng chưa đăng nhập
                        if (user == null)
                        {
                            return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập
                        }
                        // Lấy giỏ hàng của người dùng dựa trên MaTaiKhoan
                        var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);

                        // Kiểm tra nếu giỏ hàng không tồn tại
                        if (cart == null)
                        {
                            return View(new List<ChiTietGioHang>()); // Trả về view trống nếu không có giỏ hàng
                        }

                        // Lấy danh sách các sản phẩm trong giỏ hàng
                        var cartItems = db.ChiTietGioHang.Where(ct => ct.MaGioHang == cart.MaGioHang).ToList();
                        ViewData["success"] = "Thanh toán thành công đơn hàng sẽ giao tới cho quý khách trong vòng 3-5 ngày.";
                        foreach (var gioHang in cartItems)
                        {
                            ChiTietDonHang chiTietDonHang = new ChiTietDonHang
                            {
                                MaDonHang = code,
                                MaSanPhamMauSacSize = gioHang.MaSanPhamMauSacSize,
                                SoLuong = gioHang.SoLuong,
                                Gia = gioHang.SanPham_MauSac_Size.SanPham.Gia,

                            };

                            var sp = db.SanPham_MauSac_Size.SingleOrDefault(u => u.MaSanPhamMauSacSize == gioHang.MaSanPhamMauSacSize);
                            sp.SoLuong -= gioHang.SoLuong;
                            db.ChiTietDonHang.Add(chiTietDonHang);
                            db.SaveChanges();
                        }
                        db.ChiTietGioHang.RemoveRange(cartItems);
                        db.SaveChanges();
                        return RedirectToAction("ThanhToanThanhCong", "Home",new {data = "Thanh toán thành công đơn hàng sẽ giao tới cho quý khách trong vòng 3-5 ngày." });
                    }
                    else
                    {

                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                }
            }
            long madh = (long)Session["MaDH"];
            var donhang = db.DonHang.SingleOrDefault(u=>u.MaDonHang == madh);
            donhang.TrangThaiDonHang = "Đã hủy";
            db.SaveChanges();
            return RedirectToAction("ThanhToanThatBai","Home");
        }
        public ActionResult ThanhToanThatBai()
        {
            return View();
        }
        public ActionResult ThanhToanThanhCong(string data)
        {
            // Kiểm tra nếu có thông tin đơn hàng trong Session
            if (Session["MaDH"] != null)
            {
                long code = (long)Session["MaDH"];
                var user = Session["User"] as TaiKhoan;

                // Kiểm tra nếu người dùng chưa đăng nhập
                if (user == null)
                {
                    return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập
                }

                // Lấy giỏ hàng của người dùng dựa trên MaTaiKhoan
                var cart = db.GioHang.SingleOrDefault(u => u.MaTaiKhoan == user.MaTaiKhoan);
                if (cart == null)
                {
                    return View(new List<ChiTietDonHang>()); // Trả về view trống nếu không có giỏ hàng
                }

                // Lấy danh sách các sản phẩm trong đơn hàng
                var orderDetails = db.ChiTietDonHang.Where(od => od.MaDonHang == code).ToList();

                // Truyền thông tin vào ViewBag để hiển thị
                ViewData["success"] = data;
                ViewData["orderDetails"] = orderDetails;

                return View();
            }

            // Nếu không có thông tin đơn hàng, trả về view trống
            return View(new List<ChiTietDonHang>());
        }
        public ActionResult XemDonHang(string searchTerm = "", int page = 1, int pageSize = 5)
        {
            var user = Session["User"] as TaiKhoan;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập
            }

            // Lấy danh sách đơn hàng từ database
            var donHangs = db.DonHang.AsQueryable();

            // Tìm kiếm theo tên người nhận hoặc mã đơn hàng
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                donHangs = donHangs.Where(d => d.MaDonHang.ToString().Contains(searchTerm) && d.MaTaiKhoan==user.MaTaiKhoan );
            }

            // Tính tổng số bản ghi
            var totalRecords = donHangs.Count();

            // Thực hiện phân trang
            var donHangList = donHangs.OrderBy(d => d.NgayDatHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Gửi dữ liệu đến view
            ViewBag.DonHangs = donHangList;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View();
        }
        public ActionResult ChiTietDonHang(long id)
        {
            // Lấy đơn hàng từ database dựa vào MaDonHang
            var donHang = db.DonHang.SingleOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Lấy chi tiết đơn hàng
            var chiTietDonHangs = db.ChiTietDonHang.Where(ct => ct.MaDonHang == id).ToList();

            // Gán chi tiết đơn hàng vào ViewBag hoặc một đối tượng khác nếu cần
            ViewBag.ChiTietDonHangs = chiTietDonHangs;

            return View(donHang);
        }

    }
}