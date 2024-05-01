
function toggleMobileMenu() {
    const header = document.querySelector("header");
    const menuToggler = document.querySelectorAll("#menu_toggle");
      menuToggler.forEach(toggler => {
        toggler.addEventListener("click", () => header.classList.toggle("showMenu"));
      });
}
    function scrollToContent() {
            const contentSection = document.querySelector("#content-section");
    contentSection.scrollIntoView({behavior: "smooth" });
        }
              