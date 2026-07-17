using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using Microsoft.AspNetCore.Authentication.Cookies;

// Servis ve Repository klasörlerinin yollarını (namespace) sisteme tanıtıyoruz
using MusicProject.Repositories.Interface;
using MusicProject.Repositories.Concrete;
using MusicProject.Services.Interface;
using MusicProject.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ==========================================
// 1. VERİTABANI BAĞLANTISI
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// ==========================================
// 2. DEPENDENCY INJECTION (BAĞIMLILIKLARIN KAYDI)
// ==========================================
// Generic Repository Kaydı (Eğer projende kullanıyorsan açık kalabilir)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Repository Kayıtları (Veritabanı depolarını sisteme tanıtıyoruz)
builder.Services.AddScoped<IArtistRepository, ArtistRepository>(); // Az önce eksik olan ve sistemi çökerten satır!
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Service (Manager) Kayıtları (İş kurallarını sisteme tanıtıyoruz)
builder.Services.AddScoped<ISongService, SongManager>();
builder.Services.AddScoped<IArtistService, ArtistManager>();
builder.Services.AddScoped<IUserService, UserManager>();

// ==========================================
// 3. MVC VE GÜVENLİK SERVİSLERİ
// ==========================================
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Yetkisiz kullanıcıların yönlendirileceği giriş sayfası
        options.LoginPath = "/Auth/Login";
        // Tarayıcıda saklanacak şifreli çerezin adı
        options.Cookie.Name = "MusicProjectCookie";
        // Oturumun açık kalma süresi (7 Gün)
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build(); // Bütün parçalar eksiksiz olduğu için artık burası hatasız geçecek!


// ==========================================
// 4. ARA KATMANLAR (MIDDLEWARE) VE ROTA AYARLARI
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot klasöründeki CSS ve JS dosyalarını yükler

app.UseRouting();

// GÜVENLİK KONTROLLERİ (Sıralama hayati önem taşır!)
app.UseAuthentication(); // 1. Önce "Sen kimsin?" diye sorar (Kimlik Doğrulama)
app.UseAuthorization();  // 2. Sonra "Buraya girmeye yetkin var mı?" diye kontrol eder (Yetkilendirme)

// ROTA (Sitenin varsayılan açılış sayfası)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run(); // Uygulamayı başlatır