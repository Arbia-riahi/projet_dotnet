

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeAPI.Data;
using BibliothequeAPI.Models;
using System.Security.Claims;

namespace BibliothequeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilisateursController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public UtilisateursController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    // ✅ GET un utilisateur par ID (utilisé dans la modification)
    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOne(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }


    // ✅ GET mon profil actuel depuis le token
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // ✅ GET all
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Utilisateur>>> GetAll()
    {
        return await _context.Utilisateurs.ToListAsync();
    }


    // ✅ POST avec upload de photo
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Utilisateur user, IFormFile? photo)
    {

        var emailExiste = await _context.Utilisateurs.AnyAsync(u => u.Email == user.Email);
        if (emailExiste)
        {
            return BadRequest(new { message = "Email déjà utilisé" });
        }
        if (photo != null)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            user.PhotoUrl = $"images/{fileName}";
        }

        _context.Utilisateurs.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] Utilisateur updatedUser, IFormFile? photo)
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

        if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);

        if (photo != null)
        {
            if (!string.IsNullOrEmpty(existing.PhotoUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, existing.PhotoUrl);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            string folderPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folderPath);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            existing.PhotoUrl = $"images/{fileName}";
        }

        await _context.SaveChangesAsync();
        return Ok(existing);
    }


    // ✅ DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(user.PhotoUrl))
        {
            var fullPath = Path.Combine(_env.WebRootPath, user.PhotoUrl);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        _context.Utilisateurs.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}







   

  

   

   

