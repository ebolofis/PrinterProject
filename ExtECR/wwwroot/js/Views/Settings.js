
var currentFiscalName = "";



$(document).ready(function () {

    var display1 = document.getElementById("electronicScaleDiv");
    var display2 = document.getElementById("terminalDiv");
    var display3 = document.getElementById("customerDisplayDiv");
    display1.style.display = "none";
    display2.style.display = "none";
    display3.style.display = "none";
    
    ko.applyBindings();
    const tabs = document.querySelectorAll('[data-tab-target]')
    const tabContents = document.querySelectorAll('[data-tab-content]')

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const target = document.querySelector(tab.dataset.tabTarget)
            tabContents.forEach(tabContent => {
                tabContent.classList.remove('active')
            })
            tabs.forEach(tab => {
                tab.classList.remove('active')
            })
            tab.classList.add('active')
            target.classList.add('active')
        })
    })

    $('.options li').click(function () {
        var click_val = $(this).attr('data-id');
        var val = $(this).text();
        $('.options li').removeClass('active');
        $(this).addClass('active');
        $('#btn').text(val);
        $('#head-wrap, #form-area').removeAttr('class');
        $('#head-wrap').addClass('header-headings ' + click_val);
        $('#form-area').addClass('account-form-fields ' + click_val);
    });

    $("#progress-button").click(function () {
        $("#progress-menu").toggleClass('hidden-xs');
    });
});

function toggleCustomerDisplay() {
    var x = document.getElementById("customerDisplayDiv");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function toggleElectronicScale() {
    var x = document.getElementById("electronicScaleDiv");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function toggleTerminal() {
    var x = document.getElementById("terminalDiv");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}

function selectedPrinter(option) {
    var selectPrinterBox = document.getElementById("selectedPrinter");
    var selectedOption = selectPrinterBox.options[selectPrinterBox.options.selectedIndex];
    currentFiscalName = selectedOption.outerText;
    CurrentExtecr.FiscalName(selectedOption.outerText);
    debugger;
}


function ExtecrModel() {
    var self = this;
    self.FiscalName = ko.observable(null);
    GetData();
  
}



function GetData() {
    $.ajax({
        url: "/Data/GetInstallationMasterData",
        cache: false,
        type: "GET",
        crossdomain: false,
        dataType: "json",
        ContentType: "application/json; charset=utf-8",
        success: function (response) {
            if (response !== undefined && response !== null) {
                console.log(response);
            } else {
                console.log("Unable to get hotelLogo with id: " + localStorage.ConfigHotelName + "!");
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
};