window.addEventListener("load", function () {
    var a = document.querySelector("a.PostLogoutRedirectUri");
    if (a) {
        alert('signing out')
        window.location = a.href;
    }
});
