var size = 1.2;
var top = 0;
var bottom = 0;
var x = 0;
var i = 0;
var pt = 0;
$('#btnPrint').click(function () {
    printFunc();
});

$('#btnPdf').click(function () {
    pdfFunc();
});

$('#btnReload').click(function () {
    window.location.reload();
});

$('#btnPND50').click(function () {
    printPND50();
});