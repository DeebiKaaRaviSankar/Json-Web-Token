using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Users.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace UserController
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserAuth : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly userDbcontext _context;
        public UserAuth(IConfiguration config, userDbcontext context)
        {
            _context = context;
            _config = config;
        }
        public static UserLogin users = new UserLogin();
        [HttpGet("Users")]
        public async Task<ActionResult<IEnumerable<UserLogin>>> getUsers(){
            if(_context.Users==null){
                return NotFound();
            }
            return await _context.Users.ToListAsync();
        }
        [HttpPost("Register")]
        public async Task<ActionResult<UserLogin>> Register(UserDTO request)
        {
            var findUser = _context.Users.Where(u => u.Username.ToLower() == request.Username.ToLower());
            var f=0;
            foreach(var i in findUser){
            f=1;
            break;
            }
            if(f==0){
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            users.Username = request.Username;
            users.Password = request.Password;
            users.PasswordHash = passwordHash;
            users.PasswordSalt = passwordSalt;
            _context.Users.Add(users);
            await _context.SaveChangesAsync();
            return Ok(users);
            }
            
            return BadRequest("Username already exists!");

        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserDTO user)
        {
            // await _context.SaveChangesAsync();
            var findUser = _context.Users.Where(u => u.Username == user.Username);
            // if(findUser!=null){
            foreach (var i in findUser)
            {
                if (!verifyPassword(user.Password, i.PasswordSalt, i.PasswordHash))
                {
                    return BadRequest("Your Password is wrong");
                }
                var token = CreateToken(i);
                return Ok(token);
            }
            return BadRequest("Account doesnt exist");
        }

            private string CreateToken(UserLogin users)
        {
            List<Claim> claims = new List<Claim>(){
                new Claim(ClaimTypes.Name,users.Username)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:key").Value));
            Console.WriteLine(key);
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            Console.WriteLine(cred);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);
            // Console.WriteLine(token);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            // Console.WriteLine(jwt);
            return jwt;
        }
        private bool verifyPassword(string password, byte[] passwordsalt, byte[] passwordhash)
        {
            using (var hmac = new HMACSHA512(passwordsalt))
            {
                var computedhash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedhash.SequenceEqual(passwordhash);
            }
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }


    }
}