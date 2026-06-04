using System.Text;
using Hospital.Data.Configuration;
using Hospital.Services.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

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

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();
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

app.MapGet("api/users/{id:int}/period-tracker", async (int userId, [FromServices] Hospital.Data.Repositories.IUsersRepository repo) =>
{
    var notes = await repo.GetPeriodNotesAsync(userId);
    return Results.Ok(notes != null && notes.Count > 0);
});

app.MapControllers();

app.Run();
