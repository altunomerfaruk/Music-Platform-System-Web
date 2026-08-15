document.addEventListener("DOMContentLoaded", function () {
    const playSongTokenForm =
        document.getElementById("playSongTokenForm");

    const audioPlayer =
        document.getElementById("globalAudioPlayer");

    if (!playSongTokenForm || !audioPlayer) {
        console.error("Müzik oynatıcı başlatılamadı.");
        return;
    }

    const progressRange =
        document.getElementById("playerProgress");

    const currentTimeLabel =
        document.getElementById("playerCurrentTime");

    const totalTimeLabel =
        document.getElementById("playerTotalTime");

    let isSeeking = false;

    function getPlayerDuration() {
        const duration = audioPlayer.duration;

        return Number.isFinite(duration) && duration > 0
            ? duration
            : 0;
    }

    function paintProgressFill(percent) {
        if (!progressRange) {
            return;
        }

        const safePercent =
            Math.min(100, Math.max(0, percent));

        progressRange.style.backgroundImage =
            `linear-gradient(to right, var(--primary) ${safePercent}%, transparent ${safePercent}%)`;
    }

    function resetProgressUi() {
        if (progressRange) {
            progressRange.max = "0";
            progressRange.value = "0";
            progressRange.setAttribute("aria-valuetext", "0:00");
        }

        if (currentTimeLabel) {
            currentTimeLabel.textContent = "0:00";
        }

        if (totalTimeLabel) {
            totalTimeLabel.textContent = "0:00";
        }

        paintProgressFill(0);
    }

    function syncDuration() {
        const duration = getPlayerDuration();

        if (progressRange) {
            progressRange.max = duration > 0 ? String(duration) : "0";
        }

        if (totalTimeLabel) {
            totalTimeLabel.textContent = formatPlayerTime(duration);
        }
    }

    function syncProgressFromAudio() {
        if (isSeeking) {
            return;
        }

        const duration = getPlayerDuration();
        const currentTime = audioPlayer.currentTime || 0;

        if (progressRange && duration > 0) {
            progressRange.value =
                String(Math.min(currentTime, duration));
        }

        if (currentTimeLabel) {
            currentTimeLabel.textContent =
                formatPlayerTime(currentTime);
        }

        const percent =
            duration > 0 ? (currentTime / duration) * 100 : 0;

        paintProgressFill(percent);

        if (progressRange) {
            progressRange.setAttribute(
                "aria-valuetext",
                `${formatPlayerTime(currentTime)} / ${formatPlayerTime(duration)}`
            );
        }
    }

    if (progressRange) {
        audioPlayer.addEventListener("loadedmetadata", syncDuration);
        audioPlayer.addEventListener("durationchange", syncDuration);
        audioPlayer.addEventListener("timeupdate", syncProgressFromAudio);

        progressRange.addEventListener("input", function () {
            isSeeking = true;

            const duration = getPlayerDuration();
            const target = Number(progressRange.value) || 0;

            if (currentTimeLabel) {
                currentTimeLabel.textContent = formatPlayerTime(target);
            }

            const percent =
                duration > 0 ? (target / duration) * 100 : 0;

            paintProgressFill(percent);
        });

        progressRange.addEventListener("change", function () {
            const duration = getPlayerDuration();

            if (duration > 0) {
                const target =
                    Math.min(Number(progressRange.value) || 0, duration);

                // Seek yalnizca currentTime'i gunceller; yeni byte araligi mevcut
                // StreamSong (range) endpoint'inden gelir. PlaySong cagrilmaz, bu
                // yuzden ListeningHistory / dinlenme sayaci tekrar artmaz.
                audioPlayer.currentTime = target;
            }

            isSeeking = false;

            syncProgressFromAudio();
        });
    }

    const playSongUrl =
        playSongTokenForm.dataset.playUrl;

    const tokenInput =
        playSongTokenForm.querySelector(
            "input[name='__RequestVerificationToken']"
        );

    if (!playSongUrl || !tokenInput) {
        console.error("PlaySong bilgileri bulunamadı.");
        return;
    }

    let currentSongId = null;
    let currentSongInfo = null;

    document.addEventListener("click", async function (event) {
        const clickedElement = event.target;

        if (!(clickedElement instanceof Element)) {
            return;
        }

        const playButton =
            clickedElement.closest("[data-song-id]");

        if (!playButton) {
            return;
        }

        const songId =
            playButton.dataset.songId;

        if (!songId) {
            return;
        }

        if (currentSongId === songId &&
            audioPlayer.src &&
            !audioPlayer.ended) {

            if (audioPlayer.paused) {
                await resumeCurrentSong(audioPlayer);
            }
            else {
                audioPlayer.pause();
            }

            return;
        }

        const songInfo =
            getSongInfoFromButton(playButton);

        await startSong(
            songId,
            songInfo,
            playButton,
            playSongUrl,
            tokenInput.value,
            audioPlayer
        );

        if (audioPlayer.src) {
            currentSongId = songId;
            currentSongInfo = songInfo;
        }
    });

    const mainPlayButton =
        document.getElementById("mainPlayButton");

    if (mainPlayButton) {
        mainPlayButton.addEventListener("click", async function () {
            if (!currentSongId || !audioPlayer.src) {
                return;
            }

            if (audioPlayer.ended) {
                const result = await requestPlayback(
                    currentSongId,
                    playSongUrl,
                    tokenInput.value
                );

                if (!result) {
                    return;
                }

                audioPlayer.src = result.streamUrl;
                audioPlayer.load();

                resetProgressUi();

                await resumeCurrentSong(audioPlayer);

                increaseListeningCounter();

                return;
            }

            if (audioPlayer.paused) {
                await resumeCurrentSong(audioPlayer);
            }
            else {
                audioPlayer.pause();
            }
        });
    }

    audioPlayer.addEventListener("play", function () {
        if (!currentSongId) {
            return;
        }

        setSongButtonsPlaying(currentSongId);
        setMainPlayerPlaying(true);
    });

    audioPlayer.addEventListener("pause", function () {
        if (!currentSongId || audioPlayer.ended) {
            return;
        }

        setSongButtonsPaused(currentSongId);
        setMainPlayerPlaying(false);
    });

    audioPlayer.addEventListener("ended", function () {
        if (!currentSongId) {
            return;
        }

        setSongButtonsPaused(currentSongId);
        setMainPlayerPlaying(false);
    });

    audioPlayer.addEventListener("error", function () {
        if (!audioPlayer.src) {
            return;
        }

        if (currentSongId) {
            setSongButtonsPaused(currentSongId);
        }

        setMainPlayerPlaying(false);

        showPlayerError(
            "Şarkının ses dosyası yüklenemedi."
        );
    });

    async function startSong(
        songId,
        songInfo,
        playButton,
        url,
        antiforgeryToken,
        player
    ) {
        if (playButton.dataset.requestRunning === "true") {
            return;
        }

        playButton.dataset.requestRunning = "true";
        playButton.disabled = true;

        try {
            const result = await requestPlayback(
                songId,
                url,
                antiforgeryToken
            );

            if (!result) {
                return;
            }

            if (currentSongId) {
                resetSongButtons(currentSongId);
            }

            currentSongId = songId;
            currentSongInfo = songInfo;

            player.pause();
            player.src = result.streamUrl;
            player.load();

            resetProgressUi();

            updateMusicPlayer(songInfo);

            await player.play();

            increaseListeningCounter();
        }
        catch (error) {
            console.error(error);

            showPlayerError(
                error instanceof Error
                    ? error.message
                    : "Şarkı çalınırken bir hata oluştu."
            );
        }
        finally {
            playButton.disabled = false;
            playButton.dataset.requestRunning = "false";
        }
    }
});

async function requestPlayback(
    songId,
    playSongUrl,
    antiforgeryToken
) {
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

    const result =
        await readJsonResponse(response);

    if (!response.ok) {
        throw new Error(
            result?.message ??
            "Şarkı oynatılamadı."
        );
    }

    if (!result?.success ||
        !result?.streamUrl) {
        throw new Error(
            result?.message ??
            "Şarkının ses adresi alınamadı."
        );
    }

    return result;
}

async function resumeCurrentSong(audioPlayer) {
    try {
        await audioPlayer.play();
    }
    catch (error) {
        console.error(error);

        showPlayerError(
            "Şarkı oynatılamadı."
        );
    }
}

function resetSongButtons(songId) {
    const buttons =
        document.querySelectorAll(
            `[data-song-id="${CSS.escape(songId)}"]`
        );

    buttons.forEach(function (button) {
        button.textContent = "▶";
        button.disabled = false;
        button.classList.remove("currently-playing");

        const songName =
            getSongInfoFromButton(button).name;

        button.title =
            `${songName} şarkısını oynat`;
    });
}

function setSongButtonsPlaying(songId) {
    const buttons =
        document.querySelectorAll(
            `[data-song-id="${CSS.escape(songId)}"]`
        );

    buttons.forEach(function (button) {
        button.textContent = "Ⅱ";
        button.title = "Duraklat";
        button.classList.add("currently-playing");
    });
}

function setSongButtonsPaused(songId) {
    const buttons =
        document.querySelectorAll(
            `[data-song-id="${CSS.escape(songId)}"]`
        );

    buttons.forEach(function (button) {
        button.textContent = "▶";
        button.title = "Devam et";
        button.classList.remove("currently-playing");
    });
}

function setMainPlayerPlaying(isPlaying) {
    const mainPlayButton =
        document.getElementById("mainPlayButton");

    if (!mainPlayButton) {
        return;
    }

    mainPlayButton.textContent =
        isPlaying ? "Ⅱ" : "▶";

    mainPlayButton.title =
        isPlaying ? "Duraklat" : "Oynat";
}

function getSongInfoFromButton(playButton) {
    const songCard =
        playButton.closest(".song-card");

    if (songCard) {
        const songName =
            songCard
                .querySelector(".song-name")
                ?.textContent
                ?.trim();

        const songSubtitle =
            songCard
                .querySelector(".song-artist")
                ?.textContent
                ?.trim();

        return {
            name: songName || "Şarkı",
            artist: songSubtitle || "Sanatçı bilgisi yok"
        };
    }

    const searchSongRow =
        playButton.closest(".search-song-row");

    if (searchSongRow) {
        const songName =
            searchSongRow
                .querySelector(".search-song-name")
                ?.textContent
                ?.trim();

        const songSubtitle =
            searchSongRow
                .querySelector(".search-song-album")
                ?.textContent
                ?.trim();

        return {
            name: songName || "Şarkı",
            artist: songSubtitle || "Sanatçı bilgisi yok"
        };
    }

    const tableRow =
        playButton.closest("tr");

    if (tableRow) {
        const songName =
            tableRow
                .querySelector(
                    ".song-title, .cell-title, .history-song-title"
                )
                ?.textContent
                ?.trim();

        const songSubtitle =
            tableRow
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

    const pageSongName =
        document
            .querySelector(
                ".song-details-title, .song-title, .detail-title"
            )
            ?.textContent
            ?.trim();

    const pageSongArtist =
        document
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

    if (playingSongName) {
        playingSongName.textContent =
            songInfo.name;
    }

    if (playingSongArtist) {
        playingSongArtist.textContent =
            songInfo.artist;
    }
}

function increaseListeningCounter() {
    const totalListeningCount =
        document.getElementById("totalListeningCount");

    if (!totalListeningCount) {
        return;
    }

    const currentCount =
        Number.parseInt(
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
    const contentType =
        response.headers.get("content-type");

    if (!contentType?.includes("application/json")) {
        return null;
    }

    return await response.json();
}

function showPlayerError(message) {
    let errorBox =
        document.getElementById("playerErrorBox");

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

    errorBox.hideTimer =
        window.setTimeout(function () {
            errorBox.style.display = "none";
        }, 3500);
}

function formatPlayerTime(totalSeconds) {
    if (!Number.isFinite(totalSeconds) || totalSeconds < 0) {
        return "0:00";
    }

    const wholeSeconds = Math.floor(totalSeconds);
    const minutes = Math.floor(wholeSeconds / 60);
    const seconds = wholeSeconds % 60;

    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}