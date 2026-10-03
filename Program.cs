using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

var envFile = Path.Combine(builder.Environment.ContentRootPath, ".env");
//Reads Environment File and Sets Environment Variables
//Requires SA_PASSWORD to work
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex < 0) continue;
        var key = trimmed[..separatorIndex].Trim();
        var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}
//Required
var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
builder.Services.AddAuthorization();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//Set connection string and replaces placeholder with .env variable
var connectionString = builder.Configuration.GetConnectionString("Default")!
    .Replace("${SA_PASSWORD}", Environment.GetEnvironmentVariable("SA_PASSWORD"));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", async (LoginRequest body, AppDbContext db, HttpContext ctx) =>
{
    // Authenticate request based on content type.
    var person = await db.People.FirstOrDefaultAsync(p => p.Username == body.Username);
    if (person is null || !BCrypt.Net.BCrypt.Verify(body.Password, person.Password_Hash))
    {
        return Results.Unauthorized();
    }
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
        new Claim(ClaimTypes.Name, person.Username)
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { Token = tokenString });
});

app.MapPost("/CreateUser", async (Person body, AppDbContext db) =>
{
    Console.WriteLine($"Creating user: {body.Username}");
    db.People.Add(body);
    await db.SaveChangesAsync();
    return Results.Ok(body);
}).RequireAuthorization();

app.MapPost("/createticket", async (CreateTicketRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    var personIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (personIdClaim is null)
        return Results.Unauthorized();
    var person = await db.People.FindAsync(int.Parse(personIdClaim));
    var ticket = new Ticket(body.type, person!, body.details);
    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();
    return Results.Created($"/tickets/{ticket.TicketId}", ticket);
}).RequireAuthorization();
app.MapGet("/tickets", async (AppDbContext db, string? status, string? priority, string? type, string? statusfilter) =>
{
    var query = db.Tickets.AsQueryable();
    if (status is not null && Enum.TryParse<TicketStatus>(status, out var ticketStatus))
    {
        query = query.Where(t => t.Status == ticketStatus);
    }
    if (statusfilter is not null && Enum.TryParse<TicketStatus>(statusfilter, out var filteredStatus))
    {
        query = query.Where(t => t.Status != filteredStatus);
    }
    if (priority is not null && Enum.TryParse<TicketPriority>(priority, out var ticketPriority))
    {
        query = query.Where(t => t.Priority == ticketPriority);
    }
    if (type is not null && Enum.TryParse<TicketType>(type, out var ticketType))
    {
        query = query.Where(t => t.Type == ticketType);
    }
    var tickets = await query.ToListAsync();
    return Results.Ok(tickets);
}).RequireAuthorization();
app.MapGet("/tickets/{id}", async (int id, AppDbContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null)
        return Results.NotFound();
    return Results.Ok(ticket);
}).RequireAuthorization();

app.MapPost("/updateticket", async (UpdateTicketRequest body, ClaimsPrincipal user, AppDbContext db) =>
{
    var personIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if(personIdClaim is null)
        return Results.Unauthorized();
    var person = await db.People.FindAsync(int.Parse(personIdClaim));
    if (person is null || person.Role != "IT")
        return Results.Unauthorized();
    var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.TicketId == body.ticketId);
    if (ticket is null)
        return Results.NotFound();
    ticket.Status = body.status;
    if (body.priority is not null)
        ticket.Priority = (TicketPriority)body.priority;
    if (body.type is not null)
        ticket.Type = (TicketType)body.type;
    ticket.UpdatedBy = person;
    var update = new TicketUpdate(person!, ticket, body.note) {Ticket = ticket};
    db.Tickets.Update(ticket);
    db.Updates.Add(update);
    await db.SaveChangesAsync();
    return Results.Ok(ticket);
}).RequireAuthorization();
app.Run();
