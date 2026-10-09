$('.datetimepicker').datetimepicker({
    datepicker: true,
    timepicker: true,
    lang: 'th',
    step: 30,
    format: 'Y-m-d H:i',
    inline: false
});

$('#txtCostCode').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('../AutoComplete/AutoCompleteCustomerCost', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        setCustomerBox(code);
        return item;
    }
});

$('#container_kind').change(function () {
    var val = $(this).val();
    if (val == '99') {
        $('#txtOther').prop('disabled', false);
        $('#txtOther').focus();
    }
    else {
        $('#txtOther').val('');
        $('#txtOther').prop('disabled', true);
    }
});

function searchPlaceFunc(action) {
    var url = '../search/place';
    var setting = 'width=1100,height=700';
    $('#valueChange').val(action)

    window.open(url, 'search place', setting);
}

function searchCustomerFunc() {
    var url = '../search/customer';
    var setting = 'width=1100,height=700';
    window.open(url, 'search customer', setting);
}

function setPlaceBox(code) {
    var action = $('#valueChange').val();
    $.post("../order/GetPlace",
        {
            code: code
        }, function (result) {
            var code = result.data.place_code;
            var name = result.data.place_name;
            var address = result.data.place_address;
            if (action === 'S') {
                $('#txtStartCode').val(code);
                $('#txtDisStartName').val(name);
            }
            else if (action === 'M') {
                $('#txtMissionCode').val(code);
                $('#txtDisMissionName').val(name);
            }
            else if (action === 'F') {
                $('#txtFinishCode').val(code);
                $('#txtDisFinishName').val(name);
            }
        });
}


function setCustomerBox(code) {
    $.post("../Order/GetCustomerCost",
        {
            code: code
        }, function (result) {
            $('#txtCostCode').val(result.data.cost_code);
            $('#txtCustomerCode').val(result.data.customer_code);
            $('#txtCustomerName').val(result.data.customer_name_th);
            $('#txtMissionCode').val(result.data.mission_code);
            $('#txtDisMissionName').val(result.data.mission_name);
        });
    $('#txtSearchCustomer').val('');
}