using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeAPI.Data;
using BibliothequeAPI.DTOs;
using BibliothequeAPI.Models;
using BCrypt.Net;
using BibliothequeAPI.Helpers;



namespace BibliothequeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration)
    {
        _context = context;
        _env = env;
        _configuration = configuration;
    }




    // login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (utilisateur == null)
            return Unauthorized("Email incorrect");

        // 🔐 Authentification différente selon le rôle
        if (utilisateur.Role == "admin")
        {
            if (dto.Password != utilisateur.PasswordHash)
                return Unauthorized("Mot de passe incorrect (admin)");
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, utilisateur.PasswordHash))
                return Unauthorized("Mot de passe incorrect (user)");
        }

        var token = JwtHelper.GenerateJwt(utilisateur, _configuration);

        return Ok(new
        {
            token,
            utilisateur.Id,
            utilisateur.Nom,
            utilisateur.Email,
            utilisateur.Role,
            utilisateur.PhotoUrl
        });
    }



    // ✅ REGISTER pour créer un utilisateur (admin ou user)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterDTO dto, IFormFile? photo)
    {
        if (await _context.Utilisateurs.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email déjà utilisé" });

        var utilisateur = new Utilisateur
        {
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Role = "user", // Forcé ici
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        if (photo != null)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            utilisateur.PhotoUrl = $"images/{fileName}";
        }

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Utilisateur créé avec succès",
            utilisateur = new
            {
                utilisateur.Id,
                utilisateur.Nom,
                utilisateur.Email,
                utilisateur.Role
            }
        });
    }



    // ✅ MODIFIER PROFIL
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromForm] Utilisateur updatedUser, IFormFile? photo)
    {
        var existing = await _context.Utilisateurs.FindAsync(id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(updatedUser.Nom))
            existing.Nom = updatedUser.Nom;

        if (!string.IsNullOrWhiteSpace(updatedUser.Prenom))
            existing.Prenom = updatedUser.Prenom;

        if (!string.IsNullOrWhiteSpace(updatedUser.Email))
            existing.Email = updatedUser.Email;

        if (!string.IsNullOrWhiteSpace(updatedUser.Telephone))
            existing.Telephone = updatedUser.Telephone;

        // 📸 Nouvelle photo ?
        if (photo != null)
        {
            if (!string.IsNullOrEmpty(existing.PhotoUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, existing.PhotoUrl);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            string folder = Path.Combine(_env.WebRootPath, "images");
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            string path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await photo.CopyToAsync(stream);

            existing.PhotoUrl = $"images/{fileName}";
        }

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // ✅ GET /api/auth/{id} → afficher les infos du profil
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUtilisateur(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(user);
    }
}
