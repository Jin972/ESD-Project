// Call the dataTables jQuery plugin
$(document).ready(function () {
    $('.dataTable').DataTable();
});
//Download file

function Download(fileId) {
    $("#hfFileId").val(fileId);
    $("#btnDownload")[0].click();
};

$('.alertDeplay').removeClass('hide');
$('.alertDeplay').delay(10000).slideUp(1000);

(function ($) {
    "use strict";

    // Add active state to sidbar nav links
    var path = window.location.href; // because the 'href' property of the DOM element is the absolute path
    $("#layoutSidenav_nav .sb-sidenav a.nav-link").each(function () {
        if (this.href === path) {
            $(this).addClass("active");
        }
    });

    // Toggle the side navigation
    $("#sidebarToggle").on("click", function (e) {
        e.preventDefault();
        $("body").toggleClass("sb-sidenav-toggled");
    });
})(jQuery);

$(function () {
    $.validator.methods.date = function (value, element) {
        return this.optional(element) || moment(value, "DD/MM/YYYY HH:mm:ss", true).isValid();
    }
});

$(document).ready(function () {
    $('#checkBoxAll').click(function () {
        if ($(this).is(":checked")) {
            $(".chkCheckBoxId").prop("checked", true)
        }
        else {
            $(".chkCheckBoxId").prop("checked", false)
        }
    });
});

function getValue() {
    //set the variable to the element by id
    var selectRole = document.getElementById('selectRole').value;
    var formFalculty = document.getElementById('formFalculty');
    //Check if the condition is match
    if (selectRole === 'Coordinator' || selectRole === 'Student') {
        //set CSS for the form to be displayed
        formFalculty.style.display = 'block';
    } else {
        formFalculty.style.display = 'none';
    }
};





