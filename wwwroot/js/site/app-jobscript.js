setdefault()
var oTable = $('#detail-table').DataTable({
    "scrollY": 300,
    searching: false,
    info: false,
    deferRender: !0,
    scroller: !0,
    deferRender: !0,
    scrollX: !0,
    scrollCollapse: !0,
    stateSave: !0,
    ordering: false,
    lengthMenu: [[10, 15, 20, -1], [10, 15, 20, "All"]], pageLength: 10
});

$('.datetimepicker').datetimepicker({
    datepicker: true,
    timepicker: true,
    lang: 'th',
    step: 30,
    format: 'Y-m-d H:i',
    inline: false
});

$('#btnAddDetail').click(function () {
    var res = validateAddDetail()
    if (res) {
        overLay();
        var txtTruck = $('#truck_license').val();
        var txtTrailer = $('#txtTrailer').val();
        var txtDriverCode = $('#txtDriver').val();
        var txtDriverName = $('#txtDriverName').val();
        var txtContainerNo = $('#txtContainerNo').val();
        var txtSealNo = $('#txtSealNo').val();
        var txtStartCode = $('#txtStartCode').val();
        var txtMissionCode = $('#txtMissionCode').val();
        var txtStartName = $('#txtStartName').val();
        var txtMissionName = $('#txtMissionName').val();
        var rdoTrip = $('#rdoTrip:checked').val();
        var txtFinishCode = $('#txtFinishCode').val();
        var txtFinishName = $('#txtFinishName').val();
        var customer_acronym = $('#customer_acronym').val();
        $.post("/app/Job/EntryJobDetail",
            {
                truck_code: txtTruck,
                trailer_license: txtTrailer,
                driver_code: txtDriverCode,
                driver_name: txtDriverName,
                container_no: txtContainerNo,
                seal_no: txtSealNo,
                start_code: txtStartCode,
                start_name: txtStartName,
                mission_code: txtMissionCode,
                mission_name: txtMissionName,
                finish_code: txtFinishCode,
                finish_name: txtFinishName,
                customer_acronym: customer_acronym,
                trip: rdoTrip
            }, function (result) {
                unoverLay();
                $('#detail-table').DataTable().row.add($(result)).draw();
                setdefault();
            });
    }
});

$('#chkContainerBack').click(function () {
    var check = $(this).is(':checked');
    if (check) {
        $('#container-back-detail').prop('disabled', false);
    } else {
        $('#container-back-detail').prop('disabled', true);
    }
});

$('#txtTrailer').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompleteTrailer', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        $('#txtTrailer').val(code)
        return item;
    }
});

$('#txtFinishCode').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompletePlace', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        $('#valueChange').val('F');
        setPlaceBox(code);
        return item;
    }
});

$('#txtMissionCode').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompletePlace', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        $('#valueChange').val('M');
        setPlaceBox(code);
        return item;
    }
});

$('#txtStartCode').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompletePlace', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        $('#valueChange').val('S');
        setPlaceBox(code);
        return item;
    }
});

$('#txtDriver').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompleteDriver', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        var code = map[item].value;
        setDriverBoxByCode(code);
        return item;
    }
});

$('#txtCostCode').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompleteCustomerCost', { term: query }, function (data) {
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

$('#txtBookingNo').typeahead({
    minLength: 1,
    source: function (query, process) {
        var list = [];
        map = {};
        return $.post('/app/AutoComplete/AutoCompleteBooking', { term: query }, function (data) {
            $.each(data, function (i, item) {
                map[item.title] = item;
                list.push(item.title);
            });
            process(list);
        });
    },
    updater: function (item) {
        return item;
    }
});

$("#truck_license").chosen();

$('#txtCostCode').change(function () {
    var code = $(this).val();
    setCustomerBox(code)
});

$('#txtDriverCode').change(function () {
    var code = $(this).val();
    setDriverBoxByCode(code)
});


$('#txtMissionCode').change(function () {
    $('#valueChange').val('M');
    var code = $(this).val();
    setPlaceBox(code)
});

$('#txtStartCode').change(function () {
    $('#valueChange').val('S');
    var code = $(this).val();
    setPlaceBox(code)
});

$('#txtFinishCode').change(function () {
    $('#valueChange').val('F');
    var code = $(this).val();
    setPlaceBox(code)
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

$('#rdoKindBack').change(function () {
    var val = $(this).val();
    if (val == '99') {
        $('#txtOtherBack').prop('disabled', false);
        $('#txtOtherBack').focus();
    }
    else {
        $('#txtOtherBack').val('');
        $('#txtOtherBack').prop('disabled', true);
    }
});


function setTrailerBox(license) {
    $.post("/app/trailer/gettrailerbytruck",
				{
				    truck_license: license
				}, function (result) {
				    $('#txtTrailer').val(result.data.trailer_license);
				});
}

function removeFuncRow(id) {
    $('#detail-table').DataTable().row($(id)).remove().draw();
}

function searchPlaceFunc(action) {
    var url = '/app/place/_searchplace';
    var setting = 'width=1100,height=700';
    $('#valueChange').val(action)

    window.open(url, 'search place', setting);
}

function searchCustomerFunc() {
    var url = '/app/cost/_searchcost';
    var setting = 'width=1100,height=700';
    window.open(url, 'search customer', setting);
}

function searchDriverFunc() {
    var url = '/app/driver/_searchdriver';
    var setting = 'width=1100,height=700';
    window.open(url, 'search Driver', setting);
}

function setPlaceBox(code) {
    var action = $('#valueChange').val();
    $.post("/app/place/getplacedatabycode",
				{
				    code: code
				}, function (result) {
				    var code = result.data.place_code;
				    var name = result.data.place_name;
				    var address = result.data.place_address;
				    if (action === 'S') {
				        $('#txtStartCode').val(code);
				        $('#txtStartName').val(name);
				        $('#txtDisStartName').val(name);
				    }
				    else if (action === 'M') {
				        $('#txtMissionCode').val(code);
				        $('#txtMissionName').val(name);
				        $('#txtDisMissionName').val(name);
				    }
				    else if (action === 'F') {
				        $('#txtFinishCode').val(code);
				        $('#txtFinishName').val(name);
				        $('#txtDisFinishName').val(name);
				    }
				});
}

function setCustomerBox(code) {
    $.post("/app/job/getcustomercost",
				{
				    code: code
				}, function (result) {
				    $('#txtCostCode').val(result.data.cost_code);
				    $('#txtCustomerCode').val(result.data.customer_code);
				    $('#txtCustomerName').val(result.data.customer_name_th);
				    $('#txtCostName').val(result.data.cost_name);
				    $('#txtCost').val(result.data.cost_value);
				    $('#txtCostCode').val(result.data.cost_code);
				    $('#txtCostValue').val(result.data.cost_value);
				    $('#txtRemark').val(result.data.cost_remark);
				    $('#txtCustomerAddress1').val(result.data.customer_address_1);
				    $('#txtCustomerAddress2').val(result.data.customer_address_2);
				    $('#txtMissionCode').val(result.data.mission_code);
				    $('#txtMissionName').val(result.data.mission_name);
				    $('#txtDisMissionName').val(result.data.mission_name);
				    $('#txtCalculateType').val(result.data.calculate_type)
				    $('#txtCalculateValue').val(result.data.calculate_value)
				    $('#txtSubcontract').val(result.data.cost_subcontact);
				    $('#txtCostSubcontact').val(result.data.cost_subcontact);
                    $('#txtCostCalculate').val(result.data.calculate_value);
                    $('#customer_acronym').val(result.data.customer_acronym);

				    if (result.data.calculate_type == "A") {
				        $('#txtCalculateName').val('คิดเป็นยอดเงิน')
				    }
				    else {
				        $('#txtCalculateName').val('คิดเป็นเปอร์เซ็นต์')
				    }
				});
    $('#txtSearchCustomer').val('');
}

function setDriverBoxByCode(code) {
    $.post("/app/driver/getdriverdatabycode",
				{
				    code: code
				}, function (result) {
				    $('#txtDriver').val(result.data.driver_code);
				    $('#txtDisDriverName').val(result.data.fullname);
				    $('#txtDriverName').val(result.data.fullname);
				    $('#truck_license').val(result.data.truck_code);
				    $('#truck_license').trigger("chosen:updated");
				    $('#txtTrailer').val(result.data.trailer_license);
				});
}

function setdefault() {
    $('#txtContainerNo').val('');
    $('#truck_license').val('');
    $('#truck_license').trigger("chosen:updated");
    $('#txtTrailer').val('');
    $('#txtSealNo').val('');

    $('#txtDriver').val('');
    $('#txtDriverName').val('');
    $('#txtDisDriverName').val('');
    

    /*$('#txtStartCode').val('');
    $('#txtStartName').val('');
    $('#txtDisStartName').val('');

    $('#txtMissionCode').val('');
    $('#txtMissionName').val('');
    $('#txtDisMissionName').val('');

    $('#txtFinishCode').val('');
    $('#txtFinishName').val('');
    $('#txtDisFinishName').val('');*/

    $('#chkContainerBack').prop('checked', false);
    $('.rdoTypeBack').attr('checked', false);
    $('.rdoKindBack').attr('checked', false);
    $('#txtOtherBack').val('');
    $('#txtOtherBack').prop('disabled', true);
    $('#txtContainerBackNo').val('');
    $('#container-back-detail').prop('disabled', true);

    $('#txtDriver').focus();
}

function changeDateTime(str_datetime) {
    if (str_datetime !== "") {
        var arr_date = str_datetime.split("-");
        var str_year = arr_date[2];
        var str_month = arr_date[1];
        var str_day = arr_date[0];

        var full_datetime = str_year + "-" + str_month + "-" + str_day;
        return full_datetime;
    }
    else {
        return "";
    }
}

function validateAddDetail() {
    var fn = $('#frmAdd').myfunction({
        rules: {
            txtDriver:{
                text_button: {
                    value: true,
                    message: "กรุณาพิมพ์รหัสพนักงานขับรถ"
                }
            },
            txtStartCode: {
                text_button: {
                    value: true,
                    message: "กรุณาพิมพ์รหัสสถานที่รับตู้"
                }
            },
            txtMissionCode: {
                text_button: {
                    value: true,
                    message: "กรุณาพิมพ์รหัสสถานที่เปิด/บรรจุ/ลงท่า"
                }
            },
            truck_license:{
                filter_selected: {
                    value: true,
                    message: "กรุณาเลือกข้อมูล"
                }
            },
            txtTrailer: {
                require: {
                    value: true,
                    message: "กรุณาพิมพ์ทะเบียนหางลาก หากไม่มีพิมพ์ -"
                }
            },
            txtContainerNo: {
                require: {
                    value: true,
                    message: "กรุณาพิมพ์เบอร์ตู้ หากไม่มีพิมพ์ -"
                },
                container_code: {
                    message: "เบอร์ตู้ไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง"
                }
            },
            txtSealNo: {
                require: {
                    value: true,
                    message: "กรุณาพิมพ์เบอร์ซีล หากไม่มีพิมพ์ -"
                }
            }
        }
    })

    return fn;
}
