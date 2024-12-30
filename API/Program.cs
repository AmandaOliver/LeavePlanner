using LeavePlanner.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<OrganizationsService>();
builder.Services.AddScoped<LeavesService>();
builder.Services.AddScoped<RequestsService>();
builder.Services.AddScoped<EmployeesService>();
builder.Services.AddScoped<CountriesService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CountriesController>();
builder.Services.AddScoped<EmployeesController>();
builder.Services.AddScoped<OrganizationsController>();
builder.Services.AddScoped<LeavesController>();
builder.Services.AddScoped<RequestsController>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

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
                    options.UseMySQL(builder.Configuration.GetConnectionString("LeavePlannerDB"));
                });
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetConnectionString("LeavePlannerUrl"))
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://dev-pcb54t1svzog7cdm.us.auth0.com/";
        options.Audience = "https://leaveplanner.com"; ;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };
    });

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOrganizationEndpoints();
app.MapEmployeesEndpoints();
app.MapLeavesEndpoints();
app.MapRequestsEndpoints();
app.MapCountriesEndpoints();
app.Run();