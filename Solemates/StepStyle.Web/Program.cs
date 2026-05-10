using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Repositories.Interfaces;
using StepStyle.Web.Services.ExternalApi;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(StepStyle.Web.Repositories.GenericRepository<>));
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, StepStyle.Web.Services.EmailSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
}

builder.Services.AddHttpClient("NbuClient").AddPolicyHandler(GetRetryPolicy());
builder.Services.AddHttpClient("CountryClient").AddPolicyHandler(GetRetryPolicy());
builder.Services.AddHttpClient("PexelsClient").AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<IExchangeRateService, NbuExchangeRateService>();
builder.Services.AddScoped<ICountryService, RestCountryService>();
builder.Services.AddScoped<IPexelsService, PexelsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Brand}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedAsync(context);
    await DbSeeder.SeedRolesAndAdminAsync(services);
}

app.Run();