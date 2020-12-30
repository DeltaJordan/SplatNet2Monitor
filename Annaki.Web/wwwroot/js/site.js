// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

const LOCAL_STORAGE_KEY = "bootstrap-theme";

const LOCAL_META_DATA = localStorage.getItem(LOCAL_STORAGE_KEY);

const STYLE_LINK = document.getElementById("bootstrap-theme");

let themeName = LOCAL_META_DATA;

if (themeName) {
    changeTheme(themeName);
}


/**
 * Apart from toggling themes, this will also store user's theme preference in local storage.
 * So when user visits next time, we can load the same theme.
 *
 */
function changeTheme(name) {
    const themeLink = `https://bootswatch.com/4/${name}/bootstrap.min.css`;
    STYLE_LINK.setAttribute("href", themeLink);
    localStorage.setItem(LOCAL_STORAGE_KEY, name);
}
