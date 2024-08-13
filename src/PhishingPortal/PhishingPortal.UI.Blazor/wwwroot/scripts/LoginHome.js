
function toggleMobileMenu() {
    const header = document.querySelector("header");
    const menuToggler = document.querySelectorAll("#menu_toggle");
      menuToggler.forEach(toggler => {
        toggler.addEventListener("click", () => header.classList.toggle("showMenu"));
      });
}
function scrollToContent() {
    var element = document.getElementById("read-more");
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
}

