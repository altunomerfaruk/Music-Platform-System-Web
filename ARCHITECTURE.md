# MusicProject — Proje Haritası

ASP.NET Core 10 MVC. Katmanlı yapı: **Controller → Service → Repository → DbContext**.
Controller'lar EF'e doğrudan dokunmaz; repository'ler `AppDbContext` dışına çıkmaz.

---

## 1. Klasör yapısı

```
MusicProject/
├── Program.cs                  Uygulama girişi, DI kayıtları, middleware, Hangfire
│
├── Models/                     Veri modeli — dış bağımlılığı yok
│   ├── Concrete/               EF entity'leri (Song, Album, Artist, User, ...)
│   ├── Core/                   BaseEntities (Id, IsDeleted, CreatedAt)
│   └── Enums/                  UserRole, PublicationStatus, *Result enum'ları
│
├── Data/                       Veritabanı erişim altyapısı
│   ├── AppDbContext.cs         DbSet'ler, OnModelCreating, global query filter'lar
│   ├── SeedData.cs             Tür + örnek sanatçı seed'i
│   └── CountrySeedData.cs      Ülke tablosunu ISO kodlarından doldurur
│
├── Repositories/               Veri erişimi — LINQ/EF burada kalır
│   ├── Interface/              I*Repository sözleşmeleri
│   └── Concrete/               EF implementasyonları
│
├── Services/                   İş kuralları — controller ile repository arası
│   ├── Interface/              I*Service sözleşmeleri
│   ├── Concrete/               *Manager / *Service implementasyonları
│   └── Background/             Hangfire job'ları ve dashboard yetkilendirmesi
│
├── Contracts/                  Katmanlar arası veri taşıyıcıları
│   ├── Responses/              Okuma modelleri (*Dto), tüketen alana göre bölünür
│   │   ├── AdminDashboard/
│   │   ├── ArtistDashboard/
│   │   └── UserDashboard/
│   └── Requests/               Controller'dan servise giden yazma modelleri
│
├── ViewModels/                 Razor sayfalarına bağlanan modeller
│   ├── Auth/                   Login, Register
│   ├── UserDashboard/          Dinleyici sayfaları (+ UserLayoutViewModel)
│   ├── ArtistDashboard/        Sanatçı paneli (+ ArtistLayoutViewModel)
│   └── AdminDashboard/         Yönetim paneli (+ AdminLayoutViewModel)
│
├── Controllers/                Klasör adları view klasörleriyle birebir eşleşir
│   ├── Base/                   DashboardControllerBase — ortak kullanıcı yardımcıları
│   ├── Home/  Auth/
│   ├── UserDashboard/          UserDashboardController — partial, konu bazlı 7 dosya
│   ├── ArtistDashboard/        ArtistDashboardController — partial, 4 dosya
│   └── AdminDashboard/         AdminDashboardController — partial, 5 dosya
│
├── Views/                      Klasör adı controller adıyla eşleşir
│   └── Shared/                 _UserLayout, _ArtistLayout, _AdminLayout + sidebar'lar
│
├── Migrations/                 EF Core migration geçmişi
│
├── Storage/                    Kullanıcı yüklemeleri — wwwroot DIŞINDA
│   └── Audio/                  Yüklenen mp3'ler (guid'li dosya adları)
│
└── wwwroot/                    Yalnızca statik site varlıkları (css, js, lib)
```

### Yüklenen mp3'ler neden wwwroot dışında?

```
Storage/
└── Audio/
    └── 3f2a91c4e5b64d0f8a17c2be9d40517e.mp3
```

Yüklenen ses dosyaları **static file olarak sunulmuyor**. `wwwroot` altında
olsalardı dosya URL'si bilen herkes doğrudan indirebilirdi.

Kullanıcı mp3'e yalnızca `UserDashboardController.StreamSong` endpoint'i
üzerinden erişir. Endpoint stream'i açmadan önce şu kontrolleri yapar
(`ISongService.GetSongForListening` → `SongRepository`, SQL tarafında):

- şarkı `PublicationStatus == Published` mi
- şarkı `IsAdminHidden` değil mi
- şarkı bir albüme bağlıysa, albüm de `Published` ve `IsAdminHidden` değil mi

Üçünden biri sağlanmazsa `NotFound` döner. Böylece admin moderasyonu
doğrudan dosya URL'siyle **bypass edilemez**; gizlenen içeriğin sesi de anında
erişilemez hale gelir.

Dosya okuma `IAudioStorageService.OpenRead` üzerinden yapılır ve yanıt
`enableRangeProcessing: true` ile döner (tarayıcıda ileri/geri sarma için şart).

### Namespace kuralı

Namespace klasör yolunu izler: `Services/Concrete/` → `MusicProject.Services.Concrete`.

**Tek istisna:** `Controllers/` altındaki tüm dosyalar `MusicProject.Controllers`
namespace'inde kalır. Bunun sebebi `UserDashboardController` ve
`ArtistDashboardController`'ın `partial class` olması — partial parçaların aynı
namespace'te olması zorunlu. Klasörler sadece dosyaları gruplamak için.

---

## 2. İstek akışı

```
Tarayıcı
   │
   ▼
Controller ── ViewModel doldurur ──► View (.cshtml)
   │
   ▼
Service ──── iş kuralı, doğrulama
   │
   ▼
Repository ── EF sorgusu (Include, filtre, sıralama)
   │
   ▼
AppDbContext ──► SQL Server
```

Kural: bir katman yalnızca bir altındakini çağırır. Controller repository çağırmaz.

**Filtreleme ve sıralama repository'de yapılır.** Liste sayfalarının filtreleri
`Contracts/Requests` altındaki bir arama nesnesiyle (`SongSearchRequest`,
`ArtistSearchRequest`) repository'ye kadar iner ve `IQueryable` üzerinde SQL'e
çevrilir. Controller'da `.ToList()` alıp bellekte `Where` yazmak yasak — veri
büyüdükçe tüm tabloyu belleğe çeker.

Not: metin araması `string.Contains(...)` ile yapılır ve `LIKE '%...%'`'e çevrilir;
büyük/küçük harf duyarsızlığı veritabanı collation'ından gelir, `StringComparison`
parametresi SQL'e çevrilemediği için kullanılmaz.

Aynı kural sayma ve sınırlama için de geçerli: `GetVisibleSongCount()` →
`COUNT(*)`, `SearchSongsByText(query, 4)` → `TOP 4`, `GetFeaturedArtists(6)` →
`TOP 6`. Listeyi çekip `.Count` veya `.Take(n)` yapmak aynı hatadır.

`GetAllArtists()` / `GetAllAlbums()` yalnızca filtre `<select>` kutularının
seçeneklerini doldurmak için kullanılır — orada zaten tam liste gerekir.

**Entity → DTO dönüşümü nerede yapılır?**

- Servis bir okuma modelinin *sahibiyse* DTO'yu kendisi kurar
  (`ArtistManager.GetArtistDetails` → `ArtistDetailsDto`).
- Controller entity listesi üzerinde filtreleme/sıralama yapıyorsa dönüşüm
  controller'da, `private static ToXListItem(...)` yardımcılarıyla yapılır
  (`ToSongListItem`, `ToArtistListItem`, `ToArtistSongListItem`,
  `ToArtistAlbumListItem`).

İkincisinin sebebi: `GetSongsSortedByAlphabet` gibi metotlar hem UserDashboard
hem AdminDashboard tarafından kullanılıyor ve farklı alanlara ihtiyaç duyuyorlar.
Servisi tek bir DTO şekline sabitlemek her çağıranı kısıtlardı.

Değişmeyen kural: **entity view'a ulaşmaz.**

---

## 3. Controller haritası

### HomeController → `Views/Home/`

| Verb | Aksiyon | View |
|---|---|---|
| GET | `Index` | `Index.cshtml` |

### AuthController → `Views/Auth/`

| Verb | Aksiyon | View |
|---|---|---|
| GET/POST | `Login` | `Login.cshtml` |
| GET/POST | `Register` | `Register.cshtml` |
| GET | `Logout` | — (Login'e yönlendirir) |

### UserDashboardController → `Views/UserDashboard/`

Rol: `User, Artist`. Partial class, 7 dosya.

| Dosya | Verb | Aksiyon | View |
|---|---|---|---|
| `.cs` | GET | `Index` | `Index.cshtml` |
| `.Songs.cs` | GET | `SongDetails` | `SongDetails.cshtml` |
| | GET | `LikedSongs` | `LikedSongs.cshtml` |
| | GET | `AllSongs` | `AllSongs.cshtml` |
| | POST | `ToggleLike` | — |
| | POST | `PlaySong` | — (JSON) |
| | GET | `StreamSong` | — (mp3 stream) |
| `.Artists.cs` | GET | `ArtistDetails` | `ArtistDetails.cshtml` |
| | GET | `FollowedArtists` | `FollowedArtists.cshtml` |
| | GET | `AllArtists` | `AllArtists.cshtml` |
| | POST | `ToggleFollow` | — |
| `.Albums.cs` | GET | `AlbumDetails` | `AlbumDetails.cshtml` |
| `.Genres.cs` | GET | `AllGenres` | `AllGenres.cshtml` |
| | GET | `GenreDetails` | `GenreDetails.cshtml` |
| `.Search.cs` | GET | `Search` | `SearchResults.cshtml` |
| | GET | `SearchSuggestions` | — (JSON) |
| `.Account.cs` | GET/POST | `UserSettings` | `UserSettings.cshtml` |
| | GET | `ListeningHistory` | `ListeningHistory.cshtml` |

Ortak yardımcılar `.cs` dosyasında: `FillLayoutData` (layout verileri),
`RedirectBack` (returnUrl ile güvenli geri dönüş).

### ArtistDashboardController → `Views/ArtistDashboard/`

Rol: `Artist`. Partial class, 4 dosya.

| Dosya | Verb | Aksiyon | View |
|---|---|---|---|
| `.cs` | GET | `Index` | `Index.cshtml` |
| `.Songs.cs` | GET | `MySongs` | `MySongs.cshtml` |
| | GET/POST | `CreateSong` | `CreateSong.cshtml` |
| | GET/POST | `EditSong` | `EditSong.cshtml` |
| | POST | `DeleteSong` | — |
| `.Albums.cs` | GET | `MyAlbums` | `MyAlbums.cshtml` |
| | GET | `AlbumDetails` | `AlbumDetails.cshtml` |
| | GET/POST | `CreateAlbum` | `CreateAlbum.cshtml` |
| | GET/POST | `EditAlbum` | `EditAlbum.cshtml` |
| | POST | `DeleteAlbum` | — |
| `.Profile.cs` | GET/POST | `ProfileSettings` | `ProfileSettings.cshtml` |

Ortak yardımcılar `.cs` dosyasında: `TryGetDashboard` (sanatçı profili yoksa
`ArtistProfileNotFound.cshtml` döner), `FillArtistLayoutData`.

### AdminDashboardController → `Views/AdminDashboard/`

Rol: `Admin`.

| Verb | Aksiyon | View |
|---|---|---|
| GET | `Index` | `Index.cshtml` |
| GET | `Users` | `Users.cshtml` |
| POST | `SetUserActiveStatus` | — |
| GET | `Artists` | `Artists.cshtml` |
| GET | `Albums` | `Albums.cshtml` |
| POST | `SetAlbumAdminHiddenStatus` | — |
| GET | `Songs` | `Songs.cshtml` |
| POST | `SetSongAdminHiddenStatus` | — |

Dosya dağılımı: `.cs` (ctor, `Index`, ortak yardımcılar), `.Users.cs`,
`.Artists.cs`, `.Albums.cs`, `.Songs.cs`.

Kullanıcılar sayfasında admin yalnızca hesap **aktif/pasif** durumunu yönetir;
kendi hesabının durumunu değiştiremez (`CanChangeStatus`). Admin panelinden
kullanıcıyı sanatçı hesabına yükseltme diye bir akış **yoktur**.

---

## 4. Servis ve repository haritası

| Servis | Kullandığı repository | Sorumluluk |
|---|---|---|
| `UserManager` | `IUserRepository` | Kimlik doğrulama, kayıt, hesap ayarları |
| `ArtistManager` | `IArtistRepository`, `ISongRepository`, `ICountryRepository` | Sanatçı detay/pano, profil güncelleme |
| `AlbumManager` | `IAlbumRepository` | Albüm CRUD, yayın durumu |
| `SongManager` | `ISongRepository` | Şarkı CRUD, tür/sanatçı ilişkileri |
| `GenreManager` | `IGenreRepository` | Tür listeleme ve detay |
| `CountryManager` | `ICountryRepository` | Ülke listesi, varlık kontrolü |
| `LikedSongService` | `ILikedSongRepository` | Beğeni aç/kapa |
| `FollowedArtistService` | `IFollowedArtistRepository` | Takip aç/kapa |
| `ListeningHistoryManager` | `IListeningHistoryRepository` | Dinleme kaydı |
| `SongStatService` | `ISongStatRepository` | Şarkı istatistikleri |
| `AdminDashboardManager` | `IAdminDashboardRepository` | Yönetim listeleri |
| `AdminContentModerationManager` | `IAdminContentModerationRepository` | İçerik gizleme |
| `PublicationManager` | — | Yayın zamanı doğrulama, UTC/TR dönüşümü |
| `PublicationJobScheduler` | — | Hangfire job planlama |
| `LocalAudioStorageManager` | — | mp3 kaydetme/silme |
| `ArtistSongWorkflowManager` | — (servisleri birleştirir) | Şarkı ekleme/düzenleme/silme iş akışı |
| `ArtistAlbumWorkflowManager` | — (servisleri birleştirir) | Albüm ekleme/düzenleme/silme iş akışı |

Tüm arayüzler `Program.cs`'te `AddScoped` ile kayıtlı.

**Adlandırma:** arayüzler `I*Service`, implementasyonlar `*Manager`.
(`PublicationJobScheduler` istisna — zamanlayıcı, iş kuralı servisi değil.)

### İş akışı servisleri

Bir işlem birden fazla servisi sırayla kullanıyor ve arada hata olursa geri alma
gerekiyorsa, bu orkestrasyon controller'da değil ayrı bir *workflow* servisinde durur.

`ArtistSongWorkflowManager` örneği — şarkı eklerken sırayla:
albüm sahipliğini doğrula → yayın durumunu hesapla (albüme bağlıysa albümden
devral) → zamanlamayı doğrula → mp3'ü diske yaz → kaydı oluştur → Hangfire
job'ı planla. Herhangi bir adım patlarsa oluşturulan kayıt geri alınır
(soft-delete), planlanan job iptal edilir, yazılan mp3 silinir — yarım kalmış
şarkı bırakılmaz.

Sonuç `ArtistSongWorkflowResult` ile döner: `Succeeded`, `SuccessMessage`,
`ErrorMessage` ve hatanın hangi alana ait olduğunu söyleyen `ArtistSongWorkflowField`
(`None`, `Title`, `AudioFile`, `ScheduledPublishAt`, `AlbumId`).
Servis ViewModel property adlarını bilmez; controller enum'u `nameof(model.X)` ile
eşleştirip `ModelState`'e yazar.

### DB yazması sınırı: rollback mi, cleanup mi?

Güncelleme akışında mp3 ve Hangfire job'ı DB yazmasından **önce** ayrılır.
Ayrım noktası DB yazmasının başarısıdır:

| Aşama | Hata olursa |
|---|---|
| DB yazmasından **önce** | Yeni job iptal, yeni mp3 sil. Eski mp3/job'a dokunma. Kullanıcıya hata dön. |
| DB yazmasından **sonra** | Eski job iptali ve eski mp3 temizliği *post-success cleanup*'tır. Patlarsa yalnızca loglanır (`ILogger<ArtistSongWorkflowManager>`); yeni kaynaklar **geri alınmaz** ve kullanıcıya **başarılı** sonuç döner. |

Sebep: DB yazması bittikten sonra kayıt zaten yeni mp3/job değerlerini gösterir.
Temizlik hatası yüzünden bunları silmek, veritabanının işaret ettiği dosyayı yok
eder. Bu durumda tutarsızlık, "artık kullanılmayan bir dosya/job diskte kaldı"
seviyesinde kalır — ki bu güvenli taraftır.

Controller'da kalanlar: form doğrulama (`ModelState`), dropdown doldurma, ViewModel
kurma ve yönlendirme.

Albüm tarafında `ArtistAlbumWorkflowManager` aynı kalıbı izler
(`ArtistAlbumWorkflowResult` + `ArtistAlbumWorkflowField`). İki sonuç sınıfı
bilerek ayrı tutuldu; ortak bir generic tip, hata alanları farklı olduğu için
okunabilirlikten kaybettirirdi.

**Planlanmış yayının yeniden zamanlanması:** yeni Hangfire job planlandıktan
sonra kayıt güncellenir, ancak *ondan sonra* eski job iptal edilir. Sıra
önemlidir — kayıt güncellenmezse yeni job iptal edilir ve eskisi geçerli kalır.

**Admin gizlemesi ve albüm bağlantısı:** admin tarafından gizlenmiş bir albümdeki
şarkının albüm bağlantısı değiştirilemez; aksi halde sanatçı şarkıyı albümden
çıkararak moderasyonu aşabilirdi. Kontrol iki yerde durur: workflow bunu
`UpdateArtistSong` çağrısından önce yakalar ve hatayı `AlbumId` alanına düşürür,
`SongManager` içindeki aynı kontrol defense-in-depth olarak kalır.

---

## 5. Contracts vs ViewModels

İkisi karıştırılmamalı:

- **`Contracts/Responses/*Dto`** — servisin controller'a döndürdüğü okuma modeli.
  Entity'yi doğrudan view'a sızdırmamak için var. Örn. `ArtistDetailsDto.Country`
  alanı `Artist.CountryEntity.Name`'den doldurulur.
- **`Contracts/Requests/*Request`** — controller'dan servise giden yazma modeli.
  Örn. `UpdateAlbumRequest`.
- **`ViewModels/**`** — Razor sayfasının ihtiyacı olan her şey. Genellikle bir
  layout view model'inden türer (`UserLayoutViewModel`, `ArtistLayoutViewModel`,
  `AdminLayoutViewModel`) ki sidebar/başlık verisi hazır gelsin.

### Ters yönlü bağımlılık — tamamlanan geçiş

Eskiden üç servis arayüzü ViewModel döndürüyordu, yani servis katmanı Web
katmanına bağımlıydı. Bu geçiş tamamlandı; artık hepsi `*Dto` döndürüyor:

| Arayüz | Dönen tip |
|---|---|
| `IAdminDashboardService` | `AdminDashboardDto`, `Admin*ListItemDto`, `AdminLayoutTotalsDto` |
| `IUserService` | `UserSettingsDto` (girdi: `UpdateUserSettingsRequest`) |
| `IArtistService` | `ArtistDashboardDto` |

Bağımlılık yönü artık tek taraflı: `Services/` ve `Repositories/` altında
`MusicProject.ViewModels` geçmez. ViewModel'i controller kurar.

**Sunum alanları controller'da:** `SearchTerm`, `Initial`, `RoleName`,
`CanChangeStatus`, grafik `BarHeightPercent`/`DayLabel` hesabı ve
"Single" / "Bağımsız" / "Belirtilmedi" gibi fallback metinleri servisin işi
değildir. Servis ham veriyi (bilinmiyorsa boş string) döndürür, gösterim
metnini controller koyar. `AdminDashboardController` ve
`ArtistDashboardController.FillArtistLayoutData` buna örnektir.

`Admin*ListItemDto` tiplerinde hangi alanı kimin doldurduğu yorum satırıyla
ayrılmıştır (`--- servis doldurur ---` / `--- controller doldurur ---`).

---

## 6. Ülke alanı — tamamlanan geçiş

`Artist.Country` (string) kaldırıldı. Artık:

```
Artist.CountryId  ──FK──►  Country.Id
Artist.CountryEntity.Name        okuma için
```

Ülke adı okuyan her sorgu `Include(a => a.CountryEntity)` içermeli.
`FollowedArtistRepository` bunu `ThenInclude` ile yapar.

---

## 7. Bilinen davranış — doğrulama hatasında boş kalan alanlar

`UserSettings` POST'u doğrulama hatasıyla dönerse `RoleName` ve `IsPremium`
boş gelir; form bu alanları göndermiyor ve controller hata yolunda yeniden
doldurmuyor. Görsel bir eksiklik, veri kaybı değil.
