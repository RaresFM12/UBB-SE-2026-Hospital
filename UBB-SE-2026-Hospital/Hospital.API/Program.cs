using System.Text;
using Hospital.API;
using Hospital.Data.Configuration;
using Hospital.Services.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddOpenApi();
builder.Services.AddHospitalData(builder.Configuration);
builder.Services.AddHospitalServices();

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["Key"] ?? "REPLACE-WITH-256-BIT-SECRET-DO-NOT-COMMIT";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Hospital.Data.HospitalDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed on startup: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

// Setup Minimal APIs for Users to allow Desktop login
app.MapGet("api/users/exists", async ([FromQuery] string email, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByEmailAsync(email);
    return Results.Ok(user != null);
});

app.MapGet("api/users/{id:int}/exists", async (int userId, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByIdAsync(userId);
    return Results.Ok(user != null);
});

app.MapGet("api/users/{id:int}", async (int userId, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByIdAsync(userId);
    return user != null ? Results.Ok(user) : Results.NotFound();
});

app.MapGet("api/users/by-email", async ([FromQuery] string email, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByEmailAsync(email);
    return user != null ? Results.Ok(user) : Results.NotFound();
});

app.MapPost("api/users", async ([FromBody] Hospital.Data.Models.User payload, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    await repo.CreateUserAsync(payload);
    return Results.Ok();
});

app.MapPut("api/users/{id:int}", async (int userId, [FromBody] Hospital.Data.Models.User user, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    user.Id = userId;
    await repo.UpdateUserAsync(user);
    return Results.Ok();
});

app.MapGet("api/users", async ([FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var users = await repo.GetAllUsersAsync();
    return Results.Ok(users);
});

app.MapGet("api/users/search", async ([FromQuery] string? q, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var users = await repo.GetAllUsersAsync();
    if (!string.IsNullOrWhiteSpace(q))
    {
        users = users
            .Where(user =>
                user.Email.Contains(q, StringComparison.OrdinalIgnoreCase)
                || user.Username.Contains(q, StringComparison.OrdinalIgnoreCase)
                || user.Role.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    return Results.Ok(users);
});

app.MapPost("api/users/{id:int}/promote", async (int id, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    user.IsAdmin = true;
    user.Role = "Admin";
    await repo.UpdateUserAsync(user);
    return Results.Ok();
});

app.MapPost("api/users/{id:int}/disable", async (int id, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var user = await repo.GetUserByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    user.IsDisabled = true;
    await repo.UpdateUserAsync(user);
    return Results.Ok();
});

app.MapGet("api/users/{id:int}/period-tracker", async (int userId, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var notes = await repo.GetPeriodNotesAsync(userId);
    return Results.Ok(notes != null && notes.Count > 0);
});

app.MapDesktopCompatibilityRoutes();
app.MapControllers();

app.Run();
