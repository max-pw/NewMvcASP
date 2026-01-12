using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using MyStoreMVC.Data;
using MyStoreMVC.DTOs;
using MyStoreMVC.Models;

namespace MyStoreMVC.Services
{
    public class AuthService
    {
        private readonly MyStoreDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(MyStoreDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterDto registerDto)
        {
            // Verificar si el email ya existe
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, "Email already registered", null);
            }

            // Verificar si el username ya existe
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return (false, "Username already taken", null);
            }

            // Crear nuevo usuario
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generar token
            var token = _jwtService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };

            return (true, "Registration successful", response);
        }

        public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto loginDto)
        {
            // Buscar usuario por email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return (false, "Invalid email or password", null);
            }

            // Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password", null);
            }

            // Generar token
            var token = _jwtService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };

            return (true, "Login successful", response);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}