using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore;
using MyFinancialTracker.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();

// builder.Services.AddAuthentication(opt => {
//     opt.DefaultAuthenticateScheme = JwtBearerDefault.AuthenticationScheme;
//     opt.DefaultChallengeScheme = JwtBearerDefault.AuthenticationScheme;
// }).AddJwtBearer(options => {
    
// });

// builder.Services.AddAuthentication()
//    .AddGoogle(options =>
//    {
//        IConfigurationSection googleAuthNSection =
//        config.GetSection("Authentication:Google");
//        options.ClientId = googleAuthNSection["ClientId"];
//        options.ClientSecret = googleAuthNSection["ClientSecret"];
//    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
