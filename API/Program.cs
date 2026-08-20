using LeavePlanner.Configuration;
using System.ComponentModel.DataAnnotations;
using LeavePlanner.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// --- Configuration -----------------------------------------------------------------
// Every setting below is bound from configuration and validated at startup, so a missing
// or malformed value fails the boot with a message naming the key rather than surfacing
// as a confusing 500 later. Secrets never live in appsettings.json: use user-secrets
// locally and environment variables (Section__Key) when deployed. See README.md.

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

var connectionString = builder.Configuration.GetConnectionString("LeavePlannerDB");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'LeavePlannerDB' is not configured. Set it with " +
        "'dotnet user-secrets set \"ConnectionStrings:LeavePlannerDB\" \"...\"' for local " +
        "development, or the ConnectionStrings__LeavePlannerDB environment variable when " +
        "deployed. See README.md > Configuration.");
}

// CORS and authentication must be configured before the container is built, which is
// earlier than ValidateOnStart runs. Bind and validate those two sections eagerly so a
// missing key still fails the boot with a readable message rather than a null reference.
// BindAndValidate reuses the same DataAnnotations as the IOptions<T> registrations above,
// so there is one set of rules per option, not two.
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

// --- Services ----------------------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddScoped<OrganizationsService>();
builder.Services.AddScoped<LeavesService>();
builder.Services.AddScoped<RequestsService>();
builder.Services.AddScoped<EmployeesService>();
builder.Services.AddScoped<CountriesService>();
builder.Services.AddHttpClient<CountriesService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeavePlanner API", Version = "v1" });

    // Define the security scheme for JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add the security requirement
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
// Add CORS policy
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
