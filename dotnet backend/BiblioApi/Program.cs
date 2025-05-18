using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BibliothequeAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 🔐 Lire la clé JWT depuis appsettings.json
// -----------------------------
var jwtKey = builder.Configuration["Jwt:Key"];



// -----------------------------
// 📌 Ajouter le DbContext
// -----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------------
// 🔐 Ajouter l’authentification JWT
// -----------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.ASCII.GetBytes(jwtKey);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// -----------------------------
// 📌 Ajouter les services Web API
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------
// 🌐 Ajouter CORS pour autoriser Angular (localhost:4200)
// -----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// -----------------------------
// 🧪 Swagger en dev
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -----------------------------
// 🔐 Middleware de sécurité
// -----------------------------
app.UseHttpsRedirection();
app.UseStaticFiles(); // Pour les images dans wwwroot/images
app.UseCors("AllowAngular");

app.UseAuthentication();    // Ajouté pour JWT
app.UseAuthorization();     // Obligatoire après UseAuthentication

app.MapControllers();
app.Run();
