using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ShopThoiTrang.Models;

namespace ShopThoiTrang.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginApiController : ApiController
    {
        private ShopThoiTrangEntities db = new ShopThoiTrangEntities();

        // POST: api/Login
        [HttpPost]
        public IHttpActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return BadRequest("Username và Password là bắt buộc.");
                }

                var user = db.TaiKhoan
                    .FirstOrDefault(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password && u.TrangThai == true);

                if (user == null)
                {
                    return Unauthorized(); // Hoặc BadRequest("Tài khoản hoặc mật khẩu không đúng.");
                }

                // Trả về thông tin user (không bao gồm password)
                var result = new
                {
                    user.MaTaiKhoan,
                    user.Username,
                    user.HoTen,
                    user.Email,
                    user.SoDienThoai,
                    user.DiaChi,
                    MaQuyen = user.MaQuyen
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}