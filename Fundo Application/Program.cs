
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Consumer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using RepositoryLayers.Context;
using RepositoryLayers.Entity;
using RepositoryLayers.Hashing;
using RepositoryLayers.Inerface;
using RepositoryLayers.Service;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Add services to the container.
builder.Services.AddDbContext<UserContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbCon"), // Connection string name
        sqlServerOptions => // SQL Server specific options
        {
            sqlServerOptions.MigrationsAssembly("RepositoryLayers"); // Assembly where migrations are located
            sqlServerOptions.EnableRetryOnFailure(); // Enable retry on failure
        });
});

//builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
//{
//    builder.AllowAnyOrigin()
//           .AllowAnyMethod()
//           .AllowAnyHeader();
//}));


//Adding  User layers in ioc container 
builder.Services.AddScoped<IUserRl,UserRlImpl>();
builder.Services.AddScoped<IUserBl, UserBl>();

//Adding Notes layer in IOC 
builder.Services.AddScoped<INotesRL, NotesRL>();
builder.Services.AddScoped<INotesBL,NotesBL>();

//Adding Lables layer in IOC
builder.Services.AddScoped<ILabelRL,LabelRL>();
builder.Services.AddScoped<ILabelBL, LabelBL>();

//Adding Collabs layer in IOC
builder.Services.AddScoped<ICollabRL,CollabRL>();
builder.Services.AddScoped<ICollabBL, CollabBL>();   

//Adding hashing to IOC Container
builder.Services.AddScoped<Password_Hash>();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

//Adding redis to IOC
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") + ",abortConnect=false";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

// Configure RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:HostName"],
    UserName = builder.Configuration["RabbitMQ:UserName"],
    Password = builder.Configuration["RabbitMQ:Password"]
});

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Authorization"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//swagger implementation
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Fundoo Swagger UI",
        Description = "Swagger UI for Implementation on FundooApp API",
    });

    //Swagger Authorization Implemantation 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type=ReferenceType.SecurityScheme,
                                 Id="Bearer"
                             }
                         },
                          new string[]{}
                     }
                 });

});
// Register RabbitMqConsumer
builder.Services.AddScoped<RabbitMqConsumer>();
// Register RabbitMqConsumerService as HostedService
builder.Services.AddHostedService<RabbitMqConsumerService>();


builder.Services.AddControllers();
var app = builder.Build();
// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseCors(
  options => options.WithOrigins("*").AllowAnyMethod().AllowAnyHeader()
      );

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Showing FundooApp API V1");
});

app.Run();