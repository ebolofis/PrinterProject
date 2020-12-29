
$(document).ready(function () {

});

// configuration variables
var itemsPerPage = 3;

// reference to keep track of current page
var currentPage = 1;

// data, could be json from api
var cards = [
    { card: " <h5>  Knight of the Widget </h5>" },
    { card: "Oddly Uneven" },
    { card: "Midlife Upgrade" },
    { card: "Old Guard" },
    { card: "Ordinary Pony" },
    { card: "Rhino-" },
    { card: "Riveting Rigger" },
    { card: "Rules Lawyer" },
    { card: "Sacrifice Play" },
    { card: "Shaggy Camel" }
]

// reference to total pages
var pages = numPages(cards)

function numPages(cardsArray) {
    // returns the number of pages
    return Math.ceil(cardsArray.length / itemsPerPage)
}

function changePage(page) {
    // reference to output containing element
    var output = document.getElementById("output")

    // make sure page is in bounds 
    if (page < 1) page = 1
    if (page > pages) page = pages

    // clear output containing element
    output.innerHTML = ""

    // for each item within the range of the current page
    for (var i = (page - 1) * itemsPerPage; i < (page * itemsPerPage) && i < cards.length; i++) {
        // append the html to the output containing element
        output.innerHTML += `<li>${cards[i].card}</li>`
    }
}

function nextPage() {
    // if not on last page, goto next page
    if (currentPage < pages) changePage(++currentPage)
}

function prevPage() {
    // if not on the first page, goto previous page
    if (currentPage > 1) changePage(--currentPage)
}

// directly access a page by number
function gotoPage(page) {
    // sets the current page to the selected page
    currentPage = page
    // changes the page to the selected page
    changePage(page)
}

// creates individual page navigation
function addPages() {
    // grab reference to containing element
    var el = document.getElementById('pages')
    // for each page
    for (var i = 1; i < pages + 1; i++) {
        // append a link with the respective page number
        el.innerHTML += `<li><a href="javascript:gotoPage(${i})">${i}</a></li>`
    }
}

window.onload = function () {
    changePage(1) // set default page
    addPages() // generate page navigation
};
