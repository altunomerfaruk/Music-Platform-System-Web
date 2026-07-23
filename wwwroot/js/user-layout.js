const menuButton = document.getElementById("menuButton");
const sidebar = document.getElementById("sidebar");
const sidebarOverlay = document.getElementById("sidebarOverlay");

function openSidebar() {
    sidebar?.classList.add("open");
    sidebarOverlay?.classList.add("open");
}

function closeSidebar() {
    sidebar?.classList.remove("open");
    sidebarOverlay?.classList.remove("open");
}

menuButton?.addEventListener("click", openSidebar);
sidebarOverlay?.addEventListener("click", closeSidebar);
