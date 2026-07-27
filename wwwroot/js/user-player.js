document.addEventListener("DOMContentLoaded", function () {
    const playSongTokenForm = document.getElementById("playSongTokenForm");

    if (!playSongTokenForm) {
        console.error("PlaySong token formu bulunamadı.");
        return;
    }

    const playSongUrl = playSongTokenForm.dataset.playUrl;

    const tokenInput = playSongTokenForm.querySelector(
        "input[name='__RequestVerificationToken']"
    );

    if (!playSongUrl) {
        console.error("PlaySong action adresi bulunamadı.");
        return;
    }

    if (!tokenInput) {
        console.error("Antiforgery token bulunamadı.");
        return;
    }

    document.addEventListener("click", async function (event) {
        const clickedElement = event.target;

        if (!(clickedElement instanceof Element)) {
            return;
        }

        const playButton = clickedElement.closest("[data-song-id]");

        if (!playButton) {
            return;
        }

        const songId = playButton.dataset.songId;

        if (!songId) {
            console.error("Oynat butonunda data-song-id bulunamadı.");
            return;
        }

        const songInfo = getSongInfoFromButton(playButton);

        await addListeningHistory(
            songId,
            playButton,
            playSongUrl,
            tokenInput.value,
            songInfo
        );
    });
});

async function addListeningHistory(
    songId,
    playButton,
    playSongUrl,
    antiforgeryToken,
    songInfo
) {
    if (playButton.dataset.requestRunning === "true") {
        return;
    }

    const oldButtonText = playButton.textContent;
    const oldButtonTitle = playButton.title;

    playButton.dataset.requestRunning = "true";
    playButton.disabled = true;

    try {
        const formData = new FormData();

        formData.append("songId", songId);
        formData.append(
            "__RequestVerificationToken",
            antiforgeryToken
        );

        const response = await fetch(playSongUrl, {
            method: "POST",
            body: formData,
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        const result = await readJsonResponse(response);

        if (!response.ok) {
            throw new Error(
                result?.message ??
                "Dinleme kaydı oluşturulamadı."
            );
        }

        if (!result?.success) {
            throw new Error(
                result?.message ??
                "Dinleme işlemi başarısız oldu."
            );
        }

        // DEĞİŞİKLİK:
        // Önce bütün şarkı butonları tekrar oynat simgesine çevrilir.
        resetAllSongPlayButtons();

        // Yalnızca basılan şarkının butonu duraklat simgesi olur.
        setCurrentSongButton(playButton);

        // Alt müzik oynatıcısı güncellenir.
        updateMusicPlayer(songInfo);

        // Ana sayfadaki toplam dinleme sayısı anlık artırılır.
        increaseListeningCounter();

        console.log(result.message);
    }
    catch (error) {
        console.error(error);

        playButton.textContent = oldButtonText;
        playButton.title = oldButtonTitle;

        showPlayerError(
            error instanceof Error
                ? error.message
                : "Dinleme sırasında bir hata oluştu."
        );
    }
    finally {
        playButton.disabled = false;
        playButton.dataset.requestRunning = "false";
    }
}

// DEĞİŞİKLİK:
// Sayfadaki bütün şarkı oynat butonlarını başlangıç hâline getirir.
function resetAllSongPlayButtons() {
    const songPlayButtons = document.querySelectorAll("[data-song-id]");

    songPlayButtons.forEach(function (button) {
        button.textContent = "▶";
        button.disabled = false;
        button.classList.remove("currently-playing");

        const songName = getSongInfoFromButton(button).name;

        button.title = `${songName} şarkısını oynat`;
    });
}

// DEĞİŞİKLİK:
// Yalnızca o anda seçilen şarkının butonunu aktif hâle getirir.
function setCurrentSongButton(playButton) {
    playButton.textContent = "Ⅱ";
    playButton.title = "Dinleniyor";
    playButton.classList.add("currently-playing");
}

function getSongInfoFromButton(playButton) {
    const songCard = playButton.closest(".song-card");

    if (songCard) {
        const songName = songCard
            .querySelector(".song-name")
            ?.textContent
            ?.trim();

        const songSubtitle = songCard
            .querySelector(".song-artist")
            ?.textContent
            ?.trim();

        return {
            name: songName || "Şarkı",
            artist: songSubtitle || "Sanatçı bilgisi yok"
        };
    }

    const searchSongRow = playButton.closest(".search-song-row");

    if (searchSongRow) {
        const songName = searchSongRow
            .querySelector(".search-song-name")
            ?.textContent
            ?.trim();

        const songSubtitle = searchSongRow
            .querySelector(".search-song-album")
            ?.textContent
            ?.trim();

        return {
            name: songName || "Şarkı",
            artist: songSubtitle || "Sanatçı bilgisi yok"
        };
    }

    const tableRow = playButton.closest("tr");

    if (tableRow) {
        const songName = tableRow
            .querySelector(
                ".song-title, .cell-title, .history-song-title"
            )
            ?.textContent
            ?.trim();

        const songSubtitle = tableRow
            .querySelector(
                ".song-album, .cell-subtitle, .history-subtitle"
            )
            ?.textContent
            ?.trim();

        return {
            name: songName || "Şarkı",
            artist: songSubtitle || "Sanatçı bilgisi yok"
        };
    }

    const pageSongName = document
        .querySelector(
            ".song-details-title, .song-title, .detail-title"
        )
        ?.textContent
        ?.trim();

    const pageSongArtist = document
        .querySelector(
            ".song-details-artist, .song-artist, .detail-subtitle"
        )
        ?.textContent
        ?.trim();

    return {
        name: pageSongName || "Seçilen şarkı",
        artist: pageSongArtist || "Sanatçı bilgisi yok"
    };
}

function updateMusicPlayer(songInfo) {
    const playingSongName =
        document.getElementById("playingSongName");

    const playingSongArtist =
        document.getElementById("playingSongArtist");

    const mainPlayButton =
        document.getElementById("mainPlayButton");

    if (playingSongName) {
        playingSongName.textContent = songInfo.name;
    }

    if (playingSongArtist) {
        playingSongArtist.textContent = songInfo.artist;
    }

    if (mainPlayButton) {
        mainPlayButton.textContent = "Ⅱ";
        mainPlayButton.title = "Duraklat";
    }
}

function increaseListeningCounter() {
    const totalListeningCount =
        document.getElementById("totalListeningCount");

    if (!totalListeningCount) {
        return;
    }

    const currentCount = Number.parseInt(
        totalListeningCount.textContent.trim(),
        10
    );

    if (Number.isNaN(currentCount)) {
        totalListeningCount.textContent = "1";
        return;
    }

    totalListeningCount.textContent =
        String(currentCount + 1);
}

async function readJsonResponse(response) {
    const contentType = response.headers.get("content-type");

    if (!contentType?.includes("application/json")) {
        return null;
    }

    return await response.json();
}

function showPlayerError(message) {
    let errorBox = document.getElementById("playerErrorBox");

    if (!errorBox) {
        errorBox = document.createElement("div");
        errorBox.id = "playerErrorBox";

        Object.assign(errorBox.style, {
            position: "fixed",
            right: "22px",
            bottom: "105px",
            zIndex: "5000",
            maxWidth: "340px",
            padding: "13px 17px",
            color: "#ffffff",
            backgroundColor: "#541c24",
            border: "1px solid rgba(255, 255, 255, 0.12)",
            borderRadius: "12px",
            boxShadow: "0 16px 38px rgba(0, 0, 0, 0.40)",
            fontSize: "12px",
            lineHeight: "1.5"
        });

        document.body.appendChild(errorBox);
    }

    errorBox.textContent = message;
    errorBox.style.display = "block";

    window.clearTimeout(errorBox.hideTimer);

    errorBox.hideTimer = window.setTimeout(function () {
        errorBox.style.display = "none";
    }, 3500);
}