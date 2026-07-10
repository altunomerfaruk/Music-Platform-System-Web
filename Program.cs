using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using Microsoft.AspNetCore.Builder;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Uygulamaya SQL Server kullanacağını ve ayarları iletiyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// ==========================================
// 2. MVC SERVİSLERİNİN SİSTEME EKLENMESİ
// ==========================================
// Projenin MVC (Model-View-Controller) mimarisinde çalışacağını söylüyoruz
builder.Services.AddControllersWithViews();

var app = builder.Build();


// ==========================================
// 3. ARA KATMANLAR (MIDDLEWARE) VE ROTA AYARLARI
// ==========================================
// Hata sayfaları ve statik dosyaların (wwwroot içindeki CSS/JS) ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot klasörünü dış dünyaya açar

app.UseRouting();
app.UseAuthorization();

// Sitenin ilk açılış sayfasını (Rota) belirliyoruz
// Tarayıcıya bir şey yazılmazsa otomatik olarak HomeController içindeki Index çalışacak
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulamayı başlatıyoruz
app.Run();