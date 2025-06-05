using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeAPI.Data;
using BibliothequeAPI.Models;

namespace BibliothequeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivresController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public LivresController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    //  GET: Tous les utilisateurs peuvent consulter les livres
    [Authorize(Roles = "admin,user")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Livre>>> GetAll()
    {
        return await _context.Livres.ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
   
    public async Task<ActionResult<Livre>> GetOne(int id)
    {
        var livre = await _context.Livres.FindAsync(id);
        if (livre == null)
            return NotFound();

        return livre;
    }





    //  POST: Seul l’admin peut créer un livre
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Livre livre, IFormFile? image)
    {
        if (image != null)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            livre.ImageUrl = $"images/{fileName}";
        }

        _context.Livres.Add(livre);
        await _context.SaveChangesAsync();
        return Ok(livre);
    }


    //  PUT: Seul l’admin peut modifier un livre
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] Livre livre, IFormFile? image)
    {
        var existing = await _context.Livres.FindAsync(id);
        if (existing == null) return NotFound();

        var form = Request.Form;

        //  Mise à jour seulement si le champ est présent dans la requête
        if (form.ContainsKey("titre") && !string.IsNullOrWhiteSpace(form["titre"]))
            existing.Titre = form["titre"];

        if (form.ContainsKey("auteur") && !string.IsNullOrWhiteSpace(form["auteur"]))
            existing.Auteur = form["auteur"];

        if (form.ContainsKey("genre") && !string.IsNullOrWhiteSpace(form["genre"]))
            existing.Genre = form["genre"];

        if (form.ContainsKey("annee") && int.TryParse(form["annee"], out int annee))
            existing.Annee = annee;

        if (form.ContainsKey("prix") && decimal.TryParse(form["prix"], out decimal prix))
            existing.Prix = prix;

        // Nouvelle image ?
        if (image != null)
        {
            if (!string.IsNullOrEmpty(existing.ImageUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, existing.ImageUrl);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            string folderPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folderPath);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            existing.ImageUrl = $"images/{fileName}";
        }

        await _context.SaveChangesAsync();
        return Ok(existing);
    }


    // DELETE: Seul l’admin peut supprimer un livre
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var livre = await _context.Livres.FindAsync(id);
        if (livre == null) return NotFound();

        if (!string.IsNullOrEmpty(livre.ImageUrl))
        {
            var fullPath = Path.Combine(_env.WebRootPath, livre.ImageUrl);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        _context.Livres.Remove(livre);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
