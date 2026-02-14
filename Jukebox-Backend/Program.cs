using Jukebox_Backend.Data;
using Jukebox_Backend.DbHelpers;
using Jukebox_Backend.Exceptions;
using Jukebox_Backend.Models.Entities;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Information("Registering services...");

builder.Host.UseSerilog();
// db connection
try
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new ConfigurationException("Connection string not found");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
catch (ConfigurationException ex)
{
    Log.Fatal(ex.Message);
    await Log.CloseAndFlushAsync();
    Environment.Exit(1);
}

// identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager();

// jwt auth
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new ConfigurationException("JWT Key not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// cors 
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// automapper
builder.Services.AddAutoMapper(typeof(Program));

// the audio db
builder.Services.AddHttpClient<TheAudioDBService>();

// register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<MusicBrainzService>();
builder.Services.AddScoped<TheAudioDBService>();
builder.Services.AddSingleton<YouTubeApiService>();
builder.Services.AddScoped<PlaylistGenerationService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<ArtistService>();
builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserQuizHistoryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// swagger with jwt
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

Log.Information("Services registered successfully");
/////////////////////////////////////////////////////
var app = builder.Build();

// global error handling
app.UseMiddleware<Jukebox_Backend.Middleware.ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        await DbHelper.InitializeDatabaseAsync<ApplicationDbContext>(app);
        Log.Information("Database initialized successfully");
    }
    catch (DbInitializationException ex)
    {
        Log.Fatal(ex.Message);
        await Log.CloseAndFlushAsync();
        Environment.Exit(1);
    }
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//middleware to check if user is active on every request, altrimenti se diasattivo utente ed e ancora loggato può avere ancora accesso.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(
                    new { message = "Account deactivated" });
                return;
            }
        }
    }
    await next();
});

// serve static files from wwwroot (default)
app.UseStaticFiles();

app.MapControllers();

Log.Information("Application started successfully");

app.Run();