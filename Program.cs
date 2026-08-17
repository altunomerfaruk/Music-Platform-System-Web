using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Repositories.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Concrete;
using MusicProject.Services.Interface;
using Hangfire;
using MusicProject.Services.Background;
using Microsoft.AspNetCore.Identity;
using MusicProject.Models.Concrete;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    );
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);
builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>)
);
builder.Services.AddScoped< IArtistRepository,ArtistRepository>();
builder.Services.AddScoped<ISongRepository,SongRepository>();
builder.Services.AddScoped< IUserRepository,UserRepository>();
builder.Services.AddScoped<ILikedSongRepository,LikedSongRepository>();
builder.Services.AddScoped<ISongStatRepository,SongStatRepository>();
builder.Services.AddScoped<IFollowedArtistRepository,FollowedArtistRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IAdminContentModerationRepository, AdminContentModerationRepository>();
builder.Services.AddScoped<IAdminContentModerationService, AdminContentModerationManager>();
builder.Services.AddScoped<ISongService, SongManager>();
builder.Services.AddScoped<IArtistService,ArtistManager>();
builder.Services.AddScoped<IUserService,UserManager>();
builder.Services.AddScoped<ILikedSongService,LikedSongManager>();
builder.Services.AddScoped<ISongStatService,SongStatManager>();
builder.Services.AddScoped<IFollowedArtistService,FollowedArtistManager>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardManager>();
builder.Services.AddScoped<IAlbumService, AlbumManager>();
builder.Services.AddScoped<IPublicationService, PublicationManager>();
builder.Services.AddScoped<IListeningHistoryRepository, ListeningHistoryRepository>();
builder.Services.AddScoped<IListeningHistoryService, ListeningHistoryManager>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IGenreService, GenreManager>();
builder.Services.AddScoped<ICountryService, CountryManager>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPublicationJobScheduler, PublicationJobScheduler>();
builder.Services.AddScoped<IAudioStorageService, LocalAudioStorageManager>();
builder.Services.AddScoped<IArtistSongWorkflowService, ArtistSongWorkflowManager>();
builder.Services.AddScoped<IArtistAlbumWorkflowService, ArtistAlbumWorkflowManager>();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<PublicationJob>();

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";

        options.Cookie.Name = "MusicProjectCookie";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context =
        serviceProvider.GetRequiredService<AppDbContext>();

    SeedData.Initialize(context);
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireDashboardAuthorizationFilter()
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
