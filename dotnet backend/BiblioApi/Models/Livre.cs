namespace BibliothequeAPI.Models;

public class Livre
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public int Annee { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public decimal Prix { get; set; } // ✅ Ajout du prix
}
