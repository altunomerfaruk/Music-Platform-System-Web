using MusicProject.Models.Concrete;

namespace MusicProject.data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            AddAtlas(context);
            AddElifKaya(context);
            AddMavi(context);

            context.SaveChanges();
        }

        private static void AddAtlas(AppDbContext context)
        {
            // DEĞİŞİKLİK:
            // Atlas daha önce eklendiyse tekrar eklenmesini engelliyoruz.
            if (context.Artists.Any(
                    artist => artist.Name == "Atlas"))
            {
                return;
            }

            var artist = new Artist
            {
                Name = "Atlas",
                Country = "Türkiye",
                DebutYear = 2018,
                UserId = null,

                Albums = new List<Album>
                {
                    new Album
                    {
                        Name = "Şehir Işıkları",

                        Description =
                            "Şehir hayatını ve gece atmosferini anlatan alternatif albüm.",

                        ReleaseDate =
                            new DateTime(2024, 4, 12),

                        Songs = new List<Song>
                        {
                            new Song
                            {
                                Title = "Son Durak",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 2450,
                                    TotalLikes = 210,
                                    PopularityScore = 3500
                                }
                            },

                            new Song
                            {
                                Title = "Kayıp Zaman",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 1380,
                                    TotalLikes = 125,
                                    PopularityScore = 2005
                                }
                            }
                        }
                    }
                }
            };

            context.Artists.Add(artist);
        }

        private static void AddElifKaya(AppDbContext context)
        {
            // DEĞİŞİKLİK:
            // Elif Kaya daha önce eklendiyse tekrar eklenmesini engelliyoruz.
            if (context.Artists.Any(
                    artist => artist.Name == "Elif Kaya"))
            {
                return;
            }

            var artist = new Artist
            {
                Name = "Elif Kaya",
                Country = "Türkiye",
                DebutYear = 2020,
                UserId = null,

                Albums = new List<Album>
                {
                    new Album
                    {
                        Name = "Gece Yolculuğu",

                        Description =
                            "Gece yolculuklarından ilham alan modern pop albümü.",

                        ReleaseDate =
                            new DateTime(2025, 2, 20),

                        Songs = new List<Song>
                        {
                            new Song
                            {
                                Title = "Gece Yolculuğu",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 4200,
                                    TotalLikes = 385,
                                    PopularityScore = 6125
                                }
                            },

                            new Song
                            {
                                Title = "Sabaha Karşı",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 3100,
                                    TotalLikes = 280,
                                    PopularityScore = 4500
                                }
                            }
                        }
                    }
                }
            };

            context.Artists.Add(artist);
        }

        private static void AddMavi(AppDbContext context)
        {
            // DEĞİŞİKLİK:
            // Mavi daha önce eklendiyse tekrar eklenmesini engelliyoruz.
            if (context.Artists.Any(
                    artist => artist.Name == "Mavi"))
            {
                return;
            }

            var artist = new Artist
            {
                Name = "Mavi",
                Country = "Türkiye",
                DebutYear = 2019,
                UserId = null,

                Albums = new List<Album>
                {
                    new Album
                    {
                        Name = "Gecenin İçinden",

                        Description =
                            "Indie ve elektronik müzik etkileri taşıyan albüm.",

                        ReleaseDate =
                            new DateTime(2023, 9, 8),

                        Songs = new List<Song>
                        {
                            new Song
                            {
                                Title = "Sessiz Şehir",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 3600,
                                    TotalLikes = 330,
                                    PopularityScore = 5250
                                }
                            },

                            new Song
                            {
                                Title = "Uzaklarda",

                                SongStat = new SongStat
                                {
                                    TotalStreams = 1850,
                                    TotalLikes = 170,
                                    PopularityScore = 2700
                                }
                            }
                        }
                    }
                }
            };

            context.Artists.Add(artist);
        }
    }
}