using System.Text;
using CarbonWise.API.Configuration.Extensions;
using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using CarbonWise.BuildingBlocks.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CarbonWise.BuildingBlocks.Application.Users.Commands;
using CarbonWise.BuildingBlocks.Application.Users.Queries;
using CarbonWise.BuildingBlocks.Infrastructure.Users;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using CarbonWise.BuildingBlocks.Infrastructure.SchoolInfos;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Infrastructure.Electrics;
using CarbonWise.BuildingBlocks.Infrastructure.NaturalGases;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Infrastructure.Papers;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Infrastructure.Waters;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints;
using CarbonWise.BuildingBlocks.Application.Services.LLMService;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Infrastructure.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Application.Services.Reports;
using CarbonWise.BuildingBlocks.Application.Services.ExternalAPIs;
using Microsoft.Extensions.Configuration;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.ConfigureSwaggerOptions());

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Configure JWT authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

// Register services
// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ISchoolInfoRepository, SchoolInfoRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IElectricRepository, ElectricRepository>();
builder.Services.AddScoped<ICarbonFootprintService, CarbonFootprintService>();
// Register handlers
builder.Services.AddScoped<INaturalGasRepository, NaturalGasRepository>();
builder.Services.AddScoped<IPaperRepository,PaperRepository>();
builder.Services.AddScoped<IWaterRepository, WaterRepository>();
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<GetUserQueryHandler>();

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection("LlmSettings"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ILlmService, LlmService>();

builder.Services.AddScoped<ICarbonFootprintTestRepository, CarbonFootprintTestRepository>();
builder.Services.AddScoped<ITestQuestionRepository, TestQuestionRepository>();
builder.Services.AddScoped<ICarbonFootprintTestService, CarbonFootprintTestService>();

builder.Services.Configure<ExternalAPIsSettings>(builder.Configuration.GetSection("ExternalAPIs"));
builder.Services.AddHttpClient<IExternalAPIsService, ExternalAPIsService>();

builder.Services.AddScoped<CarbonWise.BuildingBlocks.Application.Services.Consumption.IConsumptionDataService,
                          CarbonWise.BuildingBlocks.Infrastructure.Services.Consumption.ConsumptionDataService>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CarbonWise.BuildingBlocks.Application.Features.Electrics.ElectricDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CarbonWise.BuildingBlocks.Application.Features.NaturalGases.NaturalGasDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CarbonWise.BuildingBlocks.Application.Features.Papers.PaperDto).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CarbonWise.BuildingBlocks.Application.Features.Waters.WaterDto).Assembly);

});
// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();

app.MapControllers();

app.Run();