using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SocialMediaAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]!);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    opts.AddPolicy(
        name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        }
    );
});
builder
    .Services.AddControllers(opts =>
    {
        opts.CacheProfiles.Add("NoCache", new CacheProfile { NoStore = true });
        opts.CacheProfiles.Add(
            "Any-60",
            new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 60 }
        );
    })
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFileName));
    opts.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter token for authentication!",
            Name = "Auth",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        }
    );
});

// builder.Services.AddSwaggerGen(opts => {
//     opts.EnableAnnotations();
//
//     });

var connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SocialMediaDbContext>(opts =>
{
    opts.UseMySql(connString, ServerVersion.AutoDetect(connString));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
