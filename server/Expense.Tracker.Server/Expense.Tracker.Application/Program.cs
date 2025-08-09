using Expense.Tracker.Application.Extensions;
using Expense.Tracker.Application.Middleware;
using Expense.Tracker.Services.Abstractions.Enums;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection") 
    ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

// Add services to the container using extension methods
builder.Services.AddExpenseTrackerServices(connectionString);
builder.Services.AddExpenseTrackerAuthentication(builder.Configuration);
builder.Services.AddExpenseTrackerControllers();
builder.Services.AddExpenseTrackerSwagger();
builder.Services.AddExpenseTrackerCors();
builder.Services.AddExpenseTrackerHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
var corsPolicy = app.Environment.IsDevelopment() 
    ? CorsServiceExtensions.ExpenseTrackerCorsPolicyName 
    : CorsServiceExtensions.ExpenseTrackerCorsProductionPolicyName;
app.UseCors(corsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();

        c.InjectJavascript("/js/health-check.js");
        c.InjectStylesheet("/css/health-check.css");
    });
}

app.MapExpenseTrackerHealthChecks();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCookieJwt();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
