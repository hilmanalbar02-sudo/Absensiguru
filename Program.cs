using Absensiguru.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Menambahkan Session
builder.Services.AddSession();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        "server=localhost;port=3306;database=db_absensi_guru;user=root;",
        ServerVersion.Parse("10.4.32-mariadb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Mengaktifkan Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();