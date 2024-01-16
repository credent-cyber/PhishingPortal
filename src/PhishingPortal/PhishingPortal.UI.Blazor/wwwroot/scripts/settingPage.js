function menu() {
    let arrow = document.querySelectorAll(".arrow");
    for (var i = 0; i < arrow.length; i++) {
        arrow[i].addEventListener("click", (e) => {
            let arrowParent = e.target.parentElement.parentElement;
        });
    }
    let sidebar = document.querySelector(".setting_sidebar");
    let sidebarBtn = document.querySelector(".bx-menu");
 
    sidebarBtn.addEventListener("click", () => {
        sidebar.classList.toggle("close");
       
    });

   
}