using LeavePlanner;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEntityFrameworkMySQL()
                .AddDbContext<LeavePlannerContext>(options =>
                {
                    options.UseMySQL("server=127.0.0.1;port=3306;user=root;password=;database=LeavePlanner");
                });


var app = builder.Build();

app.MapGet("/employees", async(LeavePlannerContext dbContext) => { 
    var employees=await dbContext.Employees.ToListAsync();
    return employees;

}    );

app.MapGet("/employees/{id}",async (int id, LeavePlannerContext dbContext) =>  
await dbContext.Employees.FindAsync(id)
    is Employee employee
        ? Results.Ok(employee)
        : Results.NotFound()
    );


app.MapPut("/employees/{id}", async (int id, Employee employee, LeavePlannerContext dbContext) =>
{
    var employees=await dbContext.Employees.FindAsync(id);

    if (employees is null) return Results.NotFound();

    employees.Id = employee.Id;
    employees.Name = employee.Name;

    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});



app.MapPost("/employees", async(Employee employee, LeavePlannerContext dbContext) => { 

    dbContext.Employees.Add(employee);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/employees/{employee.Id}", employee);
});


app.MapDelete("/employees/{id}", async (int id, LeavePlannerContext dbContext) =>
{
    if(await dbContext.Employees.FindAsync(id) is Employee employee)
    {
        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync();
        return Results.Ok(employee);
    }
    return Results.NotFound();
});

app.Run();