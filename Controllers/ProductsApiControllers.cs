using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ShopThoiTrang.Models;

namespace ShopThoiTrang.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ProductsApiController : ApiController
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: api/Products - Lấy danh sách sản phẩm
        [HttpGet]
        public IHttpActionResult GetProducts()
        {
            try
            {
                var products = db.SanPham
                    .Where(p => p.TrangThai == true)
                    .Select(p => new
                    {
                        p.MaSanPham,
                        p.TenSanPham,
                        p.MoTa,
                        p.Gia,
                        p.HinhAnh,
                        DanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : "Không xác định",
                        DoiTuong = p.DoiTuong1 != null ? p.DoiTuong1.TenDoiTuong : "Tất cả",
                        Variants = p.SanPham_MauSac_Size
                            .Where(v => v.TrangThai == true)
                            .Select(v => new
                            {
                                v.MaSanPhamMauSacSize,
                                MauSac = v.MauSac != null ? v.MauSac.TenMau : "Không xác định",
                                Size = v.Size != null ? v.Size.TenSize : "Không xác định",
                                v.SoLuong
                            }).ToList()
                    }).ToList();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Products/5 - Lấy chi tiết sản phẩm
        [HttpGet]
        public IHttpActionResult GetProduct(int id)
        {
            try
            {
                var product = db.SanPham
                    .Where(p => p.MaSanPham == id && p.TrangThai == true)
                    .Select(p => new
                    {
                        p.MaSanPham,
                        p.TenSanPham,
                        p.MoTa,
                        p.Gia,
                        p.HinhAnh,
                        p.NgayTao,
                        DanhMuc = p.DanhMuc != null ? p.DanhMuc.TenDanhMuc : "Không xác định",
                        DoiTuong = p.DoiTuong1 != null ? p.DoiTuong1.TenDoiTuong : "Tất cả",
                        Variants = p.SanPham_MauSac_Size
                            .Where(v => v.TrangThai == true)
                            .Select(v => new
                            {
                                v.MaSanPhamMauSacSize,
                                MauSac = v.MauSac != null ? v.MauSac.TenMau : "Không xác định",
                                Size = v.Size != null ? v.Size.TenSize : "Không xác định",
                                v.SoLuong
                            }).ToList()
                    }).FirstOrDefault();

                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
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
}