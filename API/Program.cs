using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Application.Common.Behaviors;
using LeavePlanner.Application.Employees;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Configuration;
using LeavePlanner.Domain;
using LeavePlanner.Infrastructure.Calendar;
using LeavePlanner.Infrastructure.Email;
using LeavePlanner.Infrastructure.Persistence;
using LeavePlanner.Infrastructure.Time;
using System.ComponentModel.DataAnnotations;
using LeavePlanner.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Settings are validated at startup so a missing key fails the boot with a message
// naming it, rather than surfacing as a 500 later. See README.md > Configuration.
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(AppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<Auth0Options>()
    .Bind(builder.Configuration.GetSection(Auth0Options.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<GoogleCalendarOptions>()
    .Bind(builder.Configuration.GetSection(GoogleCalendarOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("LeavePlannerDB");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'LeavePlannerDB' is not configured. See README.md > Configuration.");
}

// CORS and authentication are configured before the container is built, which is earlier
// than ValidateOnStart runs, so these two sections are bound and validated eagerly. The
// same DataAnnotations are reused, keeping one set of rules per option.
var appSettings = BindAndValidate<AppOptions>(builder.Configuration, AppOptions.SectionName);
var auth0 = BindAndValidate<Auth0Options>(builder.Configuration, Auth0Options.SectionName);

static T BindAndValidate<T>(IConfiguration configuration, string sectionName) where T : class, new()
{
    var section = configuration.GetSection(sectionName);
    if (!section.Exists())
    {
        throw new InvalidOperationException(
            $"The '{sectionName}' configuration section is missing. See README.md > Configuration.");
    }

    var bound = section.Get<T>() ?? new T();
    Validator.ValidateObject(bound, new ValidationContext(bound), validateAllProperties: true);
    return bound;
}

builder.Services.AddControllers();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddScoped<IClock, SystemClock>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton(_ => new HttpClient(new SocketsHttpHandler
{
	PooledConnectionLifetime = TimeSpan.FromMinutes(15)
}));
builder.Services.AddSingleton<IPublicHolidayCalendar, GooglePublicHolidayCalendar>();
builder.Services.AddScoped<LeaveEvaluator>();
builder.Services.AddScoped<EmployeeHierarchy>();
builder.Services.AddScoped<PublicHolidayGenerator>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccessChecker, AccessChecker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeavePlanner API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
builder.Services.AddEntityFrameworkMySQL()
                .AddDbContext<LeavePlannerContext>(options =>
                {
                    options.UseMySQL(connectionString);
                });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(appSettings.FrontendUrl)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0.Authority;
        options.Audience = auth0.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "email"
        };
    });

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LeavePlanner API V1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
