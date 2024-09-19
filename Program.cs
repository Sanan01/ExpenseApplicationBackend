using Serilog;
using System.Text;
using ExpenseApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ExpenseApplication.Data.Models;
using ExpenseApplication.Data.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<AccountantService>();
builder.Services.AddScoped<ManagerService>();

// Add services to the container.
builder.Services.AddControllers();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var validIssuer = jwtSettings["Issuer"];
var validAudience = jwtSettings["Audience"];
var validKey = jwtSettings["Key"];

var tokenValidationParamter = new TokenValidationParameters
{
	ValidateIssuer = true,
	ValidateAudience = true,
	ValidateLifetime = true,
	ValidateIssuerSigningKey = true,
	ValidIssuer = validIssuer,
	ValidAudience = validAudience,
	IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(validKey)),
	ClockSkew = TimeSpan.Zero
};

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.SaveToken = true;

	options.TokenValidationParameters = tokenValidationParamter;
});

// Register TokenValidationParameters
builder.Services.AddSingleton(provider => tokenValidationParamter);

builder.Services.AddAuthorization();


// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

// Replace the default LoggerFactory with Serilog
builder.Host.UseSerilog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpenseApplication API", Version = "v1" });

	// Configure Swagger to use JWT Bearer Token
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
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
			Array.Empty<string>()
		}
	});
});

// CORS Configuration

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAllOrigins", options =>
	{
		options.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed Roles One time setup
//ApplicationDbInitializer.SeedRoles(app).Wait();

app.Run();

Log.CloseAndFlush();
