using CoreAuthAndAuthUsingJWTToken.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SharedLIbrary;
using static SharedLIbrary.ServiceResponse;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace CoreAuthAndAuthUsingJWTToken.Repositories
{
    public class UserAccountRepositoty : IUserAccount
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;

        public UserAccountRepositoty(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
        }

        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            if (userDTO is null) return new GeneralResponse(false, "Model is empty");

            var user = await userManager.FindByEmailAsync(userDTO.Email);
            if (user is not null) return new GeneralResponse(false, "User already exists");

            var newUser = new ApplicationUser()
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                UserName = userDTO.Email
            };

            var createUser = await userManager.CreateAsync(newUser, userDTO.Password);
            if (!createUser.Succeeded)
            {
                var errors = string.Join(" | ", createUser.Errors.Select(e => e.Description));
                return new GeneralResponse(false, $"User cannot be created: {errors}");
            }

            var checkAdmin = await roleManager.RoleExistsAsync("Admin");
            if (!checkAdmin)
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await userManager.AddToRoleAsync(newUser, "Admin");
                return new GeneralResponse(true, "Account Created");
            }
            else
            {
                var checkUser = await roleManager.RoleExistsAsync("User");
                if (!checkUser)
                    await roleManager.CreateAsync(new IdentityRole() { Name = "User" });
                await userManager.AddToRoleAsync(newUser, "User");
                return new GeneralResponse(true, "Account Created");
            }
        }

        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO is null) return new LoginResponse(false, "Login container empty", null!);

            var getUser = await userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null) return new LoginResponse(false, "User not found", null!);

            bool checkUserPassword = await userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPassword) return new LoginResponse(false, "Invalid email or password", null!);

            var getUserRole = await userManager.GetRolesAsync(getUser);
            var userRole = getUserRole.FirstOrDefault() ?? "User";

            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email, userRole);
            string token = GenerateToken(userSession);
            return new LoginResponse(true, "Login successful", token!);
        }
        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClims = new[]
            {
              new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"]!,
                audience: config["Jwt:Audience"]!,
                claims: userClims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
