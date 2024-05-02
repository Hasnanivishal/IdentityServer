using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyIdentityServer.Authorization;
using MyIdentityServer.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Set up the database service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// set up the identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add Authentication Service
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.Name = "MyIdpServerCookie";
        options.Events = new CookieAuthenticationEvents()
        {
            OnValidatePrincipal = async ctx =>
            {
                return;
            }
        };
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:Secret")!))
        };
        options.Events = new JwtBearerEvents()
        {
            OnTokenValidated = async ctx =>
            {
                return;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IndiaUserOnly", policy =>
    {
        policy.RequireRole("User");
        // Custom Requirement
        policy.AddRequirements(new LocationRequirement("India"));
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(ClaimTypes.Role, "Admin");
    });
});

// Custom Requirement Handler
builder.Services.AddSingleton<IAuthorizationHandler, LocationAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeAuthorizationHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddPolicy("AllowedClients", options =>
    {
        options.AllowAnyOrigin()
        // options.WithOrigins("https://localhost:30288")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "Bearer",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"Middlewar is hit by request....{context.Request.Host}/{context.Request.Path}");

    await next.Invoke(context);
});

app.UseHttpsRedirection();

app.UseCors("AllowedClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
