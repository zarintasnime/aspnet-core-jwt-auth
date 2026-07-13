//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.Filters;
//using System.Text;
//using CoreAuthAndAuthUsingJWTToken.Models;
//using CoreAuthAndAuthUsingJWTToken.Repositories;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers()
//    .AddJsonOptions(options => {
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//        options.JsonSerializerOptions.WriteIndented = true;
//    });
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("con")
//    ?? throw new InvalidOperationException("Connection string 'con' not found.")));

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddSignInManager()
//    .AddRoles<IdentityRole>();

//builder.Services.AddAuthorization();
//builder.Services.AddAuthentication(options => {
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options => {
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateIssuerSigningKey = true,
//        ValidateLifetime = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
//    };
//});

//// Swagger with JWT "Authorize" button support so bearer tokens can be tested from the UI
//builder.Services.AddSwaggerGen(options => {
//    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreAuthAndAuthUsingJWTToken", Version = "v1" });
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
//        Description = "Standard Authorization header using the Bearer scheme (\"Bearer {token}\")",
//        In = ParameterLocation.Header,
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey
//    });
//    options.OperationFilter<SecurityRequirementsOperationFilter>();
//});

//builder.Services.AddScoped<IUserAccount, UserAccountRepositoty>();

//builder.Services.AddCors(options => {
//    options.AddPolicy("EnableCORS", policy => {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseCors("EnableCORS");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CoreAuthAndAuthUsingJWTToken.Models;
using CoreAuthAndAuthUsingJWTToken.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ১. Controllers & JSON Options (Reference Cycle Handling)
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// ২. Database Context Setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("con")
    ?? throw new InvalidOperationException("Connection string 'con' not found.")));

// ৩. Identity Middleware Setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddRoles<IdentityRole>();

// ৪. Authentication & JWT Validation Configuration
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreAuthAndAuthUsingJWTToken", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Please paste the generated JWT token here directly (no need to write 'Bearer ' in the box).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddScoped<IUserAccount, UserAccountRepositoty>();


builder.Services.AddCors(options => {
    options.AddPolicy("EnableCORS", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("EnableCORS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();