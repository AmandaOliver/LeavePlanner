using LeavePlanner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;


var builder = WebApplication.CreateBuilder(args);
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:3000") 
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); 
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkMySQL()
                .AddDbContext<LeavePlannerContext>(options =>
                {
                    options.UseMySQL(builder.Configuration.GetConnectionString("LeavePlannerDB"));
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
app.UseAuthorization();
app.MapControllers();
// https://www.googleapis.com/calendar/v3/calendars/en.italian%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c

app.MapGet("/countries", async (LeavePlannerContext dbContext) => {
    try
    {
        return Results.Ok(await dbContext.Countries.ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);  // Sends a 500 Internal Server Error with the exception message
    }
});
app.MapGet("/api/employee/check-employee", async (string email, LeavePlannerContext context) =>
{
    if (string.IsNullOrEmpty(email))
    {
        return Results.BadRequest("Email is required.");
    }

    var employee = await context.Employees
                                .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());

    if (employee != null)
    {
        return Results.Ok("User is an employee.");
    }
    else
    {
        return Results.NotFound("User is not an employee.");
    }
});

app.Run();