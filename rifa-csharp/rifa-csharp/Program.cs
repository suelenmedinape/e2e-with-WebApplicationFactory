using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rifa_csharp.Data;
using rifa_csharp.Entities;
using rifa_csharp.Interface;
using rifa_csharp.Jwt.Service;
using rifa_csharp.JwtSecurity.Interface;
using rifa_csharp.JwtSecurity.Service;
using rifa_csharp.Logging;
using rifa_csharp.Repositories;
using rifa_csharp.Service;
using rifa_csharp.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseLazyLoadingProxies().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Raffle API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
    
    c.OperationFilter<SwashbuckleFormFileOperationFilter>();
});

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SupabaseService>(); 

builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<UnitOfService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

var secreteKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("Invalid secret key");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Em produção isso fica como true
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreteKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADMIN", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("OPERATOR", policy => policy.RequireRole("OPERATOR"));
    options.AddPolicy("ADMIN_OPERATOR", policy => policy.RequireRole("OPERATOR", "ADMIN"));
});

builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Error,
}));

var app = builder.Build();

// IMPORTANTE: Seed das roles ao iniciar a aplicação
/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}*/

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "Raffle API";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Raffle API");
    });
}

app.UseCors("AllowAngularDev");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
