using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    public partial class BackfillArtistCountryIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Yeni: Türkiye kaydı seed işlemi henüz çalışmamış olsa bile migration içinde garanti edilir.
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM Countries
                    WHERE IsoCode = 'TR'
                )
                BEGIN
                    INSERT INTO Countries
                    (
                        Name,
                        IsoCode,
                        IsDeleted,
                        CreatedAt
                    )
                    VALUES
                    (
                        N'Türkiye',
                        'TR',
                        0,
                        SYSUTCDATETIME()
                    );
                END;
            """);

            // Yeni: Eski metin ülke alanı Türkiye olan sanatçıları TR kaydına bağlar.
            migrationBuilder.Sql("""
                UPDATE Artists
                SET CountryId = (
                    SELECT TOP 1 Id
                    FROM Countries
                    WHERE IsoCode = 'TR'
                    ORDER BY Id
                )
                WHERE CountryId IS NULL
                  AND Country IS NOT NULL
                  AND LOWER(LTRIM(RTRIM(Country))) IN
                  (
                      N'türkiye',
                      N'turkiye',
                      N'turkey',
                      N'türkiye cumhuriyeti'
                  );
            """);

            // Yeni: Diğer ülke adlarını Countries tablosundaki adlarla eşleştirir.
            migrationBuilder.Sql("""
                UPDATE Artists
                SET CountryId = Countries.Id
                FROM Artists
                INNER JOIN Countries
                    ON LOWER(LTRIM(RTRIM(Artists.Country)))
                     = LOWER(LTRIM(RTRIM(Countries.Name)))
                WHERE Artists.CountryId IS NULL
                  AND Artists.Country IS NOT NULL;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma sırasında eski metin alanı korunduğu için yalnızca ilişki temizlenir.
            migrationBuilder.Sql("""
                UPDATE Artists
                SET CountryId = NULL
                WHERE CountryId = (
                    SELECT TOP 1 Id
                    FROM Countries
                    WHERE IsoCode = 'TR'
                    ORDER BY Id
                );
            """);
        }
    }
}