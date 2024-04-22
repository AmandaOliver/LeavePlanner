using LeavePlanner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEntityFrameworkMySQL()
                .AddDbContext<LeavePlannerContext>(options =>
                {
                    options.UseMySQL(builder.Configuration.GetConnectionString("LeavePlannerDB"));
                });


var app = builder.Build();

System.Diagnostics.Debug.WriteLine(" hello world ");
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

app.Run();