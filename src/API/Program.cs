using API.Authorization;
using API.Middleware;
using API.Services;
using Application;
using Application.Interfaces;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = API.Common.CustomProblemDetails.Create(
            StatusCodes.Status400BadRequest,
            "Bad Request",
            "One or more validation errors occurred.",
            context.HttpContext.Request.Path,
            errors,
            context.HttpContext.TraceIdentifier);

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAdmin, policy => 
        policy.RequireRole(Roles.Admin));
        
    options.AddPolicy(Policies.RequireUser, policy => 
        policy.RequireRole(Roles.User));
        
    options.AddPolicy(Policies.RequireUserOrAdmin, policy => 
        policy.RequireRole(Roles.Admin, Roles.User));

    options.AddPolicy(Policies.CanManageProject, policy =>
        policy.Requirements.Add(new ResourceOwnerOrAdminRequirement()));

    options.AddPolicy(Policies.CanManageTask, policy =>
        policy.Requirements.Add(new ResourceOwnerOrAdminRequirement()));

    options.AddPolicy(Policies.CanManageComment, policy =>
        policy.Requirements.Add(new ResourceOwnerOrAdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, ProjectAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TaskAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CommentAuthorizationHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clean Architecture Task Management API",
        Version = "v1",
        Description = "Clean Architecture solution with ASP.NET Core Identity, JWT, Refresh Tokens, Roles, Policies, and Task Workflow."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token: Bearer {token}"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

try
{
    await IdentityDataSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetService<ILogger<Program>>();
    logger?.LogError(ex, "An error occurred while seeding Identity data.");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clean Architecture API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
