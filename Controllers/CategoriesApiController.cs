using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ShopThoiTrang.Models;

namespace ShopThoiTrang.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CategoriesApiController : ApiController
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // GET: api/Categories - Lấy danh sách danh mục
        [HttpGet]
        public IHttpActionResult GetCategories()
        {
            try
            {
                var categories = db.DanhMuc
                    .Where(c => c.TrangThai == true)
                    .Select(c => new
                    {
                        c.MaDanhMuc,
                        c.TenDanhMuc
                    }).ToList();

                return Ok(categories);
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