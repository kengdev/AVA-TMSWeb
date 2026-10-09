$("#frmCashing").ajaxForm(function (data) {
    killOverlayForm();
    if (data.result == true) {
        $.alert.open('info', data.resultMessage, function (r) {
            window.location.reload();
            window.opener.reloadRow();
        });
    } else {
        $.alert.open('error', data.resultMessage);
    }
});

var oTable = $('#list-cash').DataTable({
    scrollY: 310,
    searching: true,
    info: false,
    deferRender: !0,
    scroller: true,
    ordering: false,
    lengthMenu: [[10, 15, 20, -1], [10, 15, 20, "All"]],
    pageLength: 1000
});

$("#check-all").change(function () {
    $("input:checkbox").prop('checked', $(this).prop("checked"));
    $(".total-val").prop('disabled', !$(this).prop("checked"));
    setNetTotal();
});

$('#filterDate').on('keyup', function () {
    $('#list-cash').DataTable().columns(1).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});
$('#filterCustomer').on('keyup', function () {
    $('#list-cash').DataTable().columns(2).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});
$('#filterPlace').on('keyup', function () {
    $('#list-cash').DataTable().columns(3).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});
$('#filterBooking').on('keyup', function () {
    $('#list-cash').DataTable().columns(4).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});
$('#filterContainer').on('keyup', function () {
    $('#list-cash').DataTable().columns(5).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});
$('#filterLicense').on('keyup', function () {
    $('#list-cash').DataTable().columns(6).search(this.value).draw();
    $('input:checkbox').prop('checked', false);
});

function checkboxFunc(e) {
    var _tot = $(".item-checked").length
    var _tot_checked = $(".item-checked:checked").length;
    var x = $(e).closest('td').find('.total-val');
    if($(e).is(':checked')){
        x.prop('disabled', false);
    }else{
        x.prop('disabled', true);
    }

    if (_tot != _tot_checked) {
        $("#check-all").prop('checked', false);
    }
    else if (_tot == _tot_checked) {
        $("#check-all").prop('checked', true);
    }

    setNetTotal();
}

function setNetTotal() {
    var total = 0;
    var amount = $('.item-checked:checked').length;
    $('.item-checked:checked').each(function () {
        total = parseFloat($(this).closest('td').find('.total-val').val()) + total;
    });

    $('#amount-total').html(amount);
    $('#amount-total-val').val(amount);
    $('#net-total').html(number_format(total));
    $('#net-total-val').val(total.toFixed(2));
}

function number_format(number) {
    var decimals = 2;
    var dec_point = '.';
    var thousands_sep = ',';
    number = number.toFixed(decimals);
    var nstr = number.toString();
    nstr += '';
    x = nstr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? dec_point + x[1] : '';
    var rgx = /(\d+)(\d{3})/;

    while (rgx.test(x1))
        x1 = x1.replace(rgx, '$1' + thousands_sep + '$2');

    return x1 + x2;
}

function OnValidate() {
    var _tot_checked = $(".item-checked:checked").length;
    if (_tot_checked != 0) {
        overLayForm();
        return true;
    }
    else {
        $.alert.open('warning', "ต้องเลือกข้อมูลอย่างน้อย 1 รายการ");
        return false;
    }
}