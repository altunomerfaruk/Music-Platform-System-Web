using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Repositories.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Concrete;
using MusicProject.Services.Interface;


var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    );

// ==========================================
// 1. VERİTABANI BAĞLANTISI
// ==========================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// ==========================================
// 2. DEPENDENCY INJECTION
// ==========================================

// Generic Repository kaydı
builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>)
);

// ==========================================
// REPOSITORY KAYITLARI
// ==========================================

builder.Services.AddScoped<
    IArtistRepository,
    ArtistRepository
>();

builder.Services.AddScoped<
    ISongRepository,
    SongRepository
>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();

builder.Services.AddScoped<
    ILikedSongRepository,
    LikedSongRepository
>();

builder.Services.AddScoped<
    ISongStatRepository,
    SongStatRepository
>();

builder.Services.AddScoped<
    IFollowedArtistRepository,
    FollowedArtistRepository
>();

// ==========================================
// SERVICE KAYITLARI
// ==========================================

builder.Services.AddScoped<
    ISongService,
    SongManager
>();

builder.Services.AddScoped<
    IArtistService,
    ArtistManager
>();

builder.Services.AddScoped<
    IUserService,
    UserManager
>();

builder.Services.AddScoped<
    ILikedSongService,
    LikedSongService
>();

builder.Services.AddScoped<
    ISongStatService,
    SongStatService
>();

builder.Services.AddScoped<
    IFollowedArtistService,
    FollowedArtistService
>();

// ==========================================
// 3. MVC VE GÜVENLİK SERVİSLERİ
// ==========================================

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        // Yetkisiz kullanıcıların yönlendirileceği sayfa
        options.LoginPath = "/Auth/Login";

        // Tarayıcıda tutulacak cookie adı
        options.Cookie.Name = "MusicProjectCookie";

        // Oturumun açık kalma süresi
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

// ==========================================
// 4. ÖRNEK VERİLERİ EKLEME
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context =
        serviceProvider.GetRequiredService<AppDbContext>();

    SeedData.Initialize(context);
}

// ==========================================
// 5. HATA YÖNETİMİ
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ==========================================
// 6. MIDDLEWARE
// ==========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Önce kullanıcının kimliği kontrol edilir.
app.UseAuthentication();

// Sonra yetkisi kontrol edilir.
app.UseAuthorization();

// ==========================================
// 7. ROTA AYARLARI
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();