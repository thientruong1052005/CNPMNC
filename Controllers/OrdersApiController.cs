using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ShopThoiTrang.Models;

namespace ShopThoiTrang.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OrdersApiController : ApiController
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: api/Orders/user/{maTaiKhoan} - Lấy đơn hàng theo user
        [HttpGet]
        [Route("api/Orders/user/{maTaiKhoan}")]
        public IHttpActionResult GetOrdersByUser(int maTaiKhoan)
        {
            try
            {
                var orders = db.DonHang
                    .Where(o => o.MaTaiKhoan == maTaiKhoan)
                    .OrderByDescending(o => o.NgayDatHang)
                    .Select(o => new
                    {
                        o.MaDonHang,
                        o.NgayDatHang,
                        o.TrangThaiDonHang,
                        o.TongTien,
                        ChiTiet = o.ChiTietDonHang.Select(ct => new
                        {
                            ct.MaSanPhamMauSacSize,
                            TenSanPham = ct.SanPham_MauSac_Size.SanPham.TenSanPham,
                            ct.SoLuong,
                            ct.Gia,
                            MauSac = ct.SanPham_MauSac_Size.MauSac.TenMau,
                            Size = ct.SanPham_MauSac_Size.Size.TenSize,
                            HinhAnh = ct.SanPham_MauSac_Size.SanPham.HinhAnh
                        }).ToList()
                    }).ToList();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Orders/{id} - Lấy chi tiết đơn hàng theo mã
        [HttpGet]
        [Route("api/Orders/{id}")]
        public IHttpActionResult GetOrder(long id)
        {
            try
            {
                var order = db.DonHang
                    .Where(o => o.MaDonHang == id)
                    .Select(o => new
                    {
                        o.MaDonHang,
                        o.NgayDatHang,
                        o.TrangThaiDonHang,
                        o.TongTien,
                        ThongTinNguoiDung = new
                        {
                            o.TaiKhoan.HoTen,
                            o.TaiKhoan.Email,
                            o.TaiKhoan.SoDienThoai,
                            o.TaiKhoan.DiaChi
                        },
                        ChiTiet = o.ChiTietDonHang.Select(ct => new
                        {
                            ct.MaSanPhamMauSacSize,
                            TenSanPham = ct.SanPham_MauSac_Size.SanPham.TenSanPham,
                            ct.SoLuong,
                            ct.Gia,
                            ThanhTien = ct.SoLuong * ct.Gia,
                            MauSac = ct.SanPham_MauSac_Size.MauSac.TenMau,
                            Size = ct.SanPham_MauSac_Size.Size.TenSize,
                            HinhAnh = ct.SanPham_MauSac_Size.SanPham.HinhAnh
                        }).ToList()
                    }).FirstOrDefault();

                if (order == null)
                {
                    return NotFound();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/Orders - Tạo đơn hàng mới
        [HttpPost]
        public IHttpActionResult PostOrder([FromBody] OrderRequest orderRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (orderRequest == null)
                {
                    return BadRequest("Dữ liệu đơn hàng không được rỗng.");
                }

                if (orderRequest.Items == null || !orderRequest.Items.Any())
                {
                    return BadRequest("Danh sách sản phẩm không được rỗng.");
                }

                // Kiểm tra user tồn tại
                var user = db.TaiKhoan.FirstOrDefault(u => u.MaTaiKhoan == orderRequest.MaTaiKhoan && u.TrangThai == true);
                if (user == null)
                {
                    return BadRequest("Tài khoản không tồn tại hoặc đã bị khóa.");
                }

                // Tạo đơn hàng mới
                var donHang = new DonHang
                {
                    MaTaiKhoan = orderRequest.MaTaiKhoan,
                    NgayDatHang = DateTime.Now,
                    TrangThaiDonHang = "Đã đặt hàng",
                    TongTien = orderRequest.Items.Sum(i => i.Gia * i.SoLuong)
                };

                // Thêm chi tiết đơn hàng và kiểm tra tồn kho
                foreach (var item in orderRequest.Items)
                {
                    // Kiểm tra sản phẩm tồn kho
                    var variant = db.SanPham_MauSac_Size
                        .FirstOrDefault(v => v.MaSanPhamMauSacSize == item.MaSanPhamMauSacSize &&
                                           v.TrangThai == true);

                    if (variant == null)
                    {
                        return BadRequest($"Sản phẩm với ID {item.MaSanPhamMauSacSize} không tồn tại hoặc đã ngừng bán.");
                    }

                    if (variant.SoLuong < item.SoLuong)
                    {
                        return BadRequest($"Sản phẩm '{variant.SanPham.TenSanPham}' không đủ số lượng tồn kho (chỉ còn {variant.SoLuong}).");
                    }

                    // Thêm chi tiết đơn hàng
                    var chiTiet = new ChiTietDonHang
                    {
                        MaSanPhamMauSacSize = item.MaSanPhamMauSacSize,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia
                    };
                    donHang.ChiTietDonHang.Add(chiTiet);

                    // Cập nhật tồn kho
                    variant.SoLuong -= item.SoLuong;
                }

                // Lưu đơn hàng vào database
                db.DonHang.Add(donHang);
                db.SaveChanges();

                // Trả về thông tin đơn hàng vừa tạo
                var result = new
                {
                    MaDonHang = donHang.MaDonHang,
                    NgayDatHang = donHang.NgayDatHang,
                    TongTien = donHang.TongTien,
                    TrangThaiDonHang = donHang.TrangThaiDonHang,
                    ChiTiet = donHang.ChiTietDonHang.Select(ct => new
                    {
                        ct.MaSanPhamMauSacSize,
                        TenSanPham = ct.SanPham_MauSac_Size.SanPham.TenSanPham,
                        ct.SoLuong,
                        ct.Gia,
                        MauSac = ct.SanPham_MauSac_Size.MauSac.TenMau,
                        Size = ct.SanPham_MauSac_Size.Size.TenSize
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/Orders/{id}/status - Cập nhật trạng thái đơn hàng
        [HttpPut]
        [Route("api/Orders/{id}/status")]
        public IHttpActionResult UpdateOrderStatus(long id, [FromBody] StatusUpdateRequest statusRequest)
        {
            try
            {
                var donHang = db.DonHang.FirstOrDefault(d => d.MaDonHang == id);
                if (donHang == null)
                {
                    return NotFound();
                }

                // Cập nhật trạng thái
                donHang.TrangThaiDonHang = statusRequest.TrangThaiMoi;
                db.SaveChanges();

                return Ok(new
                {
                    Message = "Cập nhật trạng thái thành công",
                    MaDonHang = donHang.MaDonHang,
                    TrangThaiMoi = donHang.TrangThaiDonHang
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Model để nhận dữ liệu từ app Android
    public class OrderRequest
    {
        public int MaTaiKhoan { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int MaSanPhamMauSacSize { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
    }

    public class StatusUpdateRequest
    {
        public string TrangThaiMoi { get; set; }
    }
}