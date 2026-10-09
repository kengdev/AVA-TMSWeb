$.fn.extend(
    {
        myfunction: function (data) {
            var rules = data.rules;
            var y = 0;
            $.each(rules, function (i, item) {
                var c = 0;
                $.each(item, function (key, val) {
                    var x = key.toLowerCase();
                    if (c == 0) {
                        switch (x) {
                            case "require":
                                var element = ('#' + i)
                                if ($(element).val() == '') {
                                    setError(element, val);
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "duplicate":
                                var element = ('#' + i)
                                if (val.value) {
                                    setError(element, val);
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "minlength":
                                var element = ('#' + i);
                                if ($(element).val().length < val.value) {
                                    setError(element, val);
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "maxlength":
                                var element = ('#' + i);
                                if ($(element).val().length > val.value) {
                                    setError(element, val);
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "selected":
                                var element = ('#' + i);
                                if ($(element).val() == '') {
                                    setError(element, val)
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "filter_selected":
                                var element = ('#' + i);
                                if ($(element).val() == '') {
                                    $(element).parent().find('.chosen-single').addClass('has-error-filter');
                                    $(element).next().next("span.help-block").html(val.message);
                                    y++; c++;
                                }
                                else {
                                    $(element).parent().find('.chosen-single').removeClass('has-error-filter');
                                    $(element).next().next("span.help-block").html('');
                                }
                                break;
                            case "checked":
                                var element = ('.' + i);
                                if ($(element + ':checked').length == 0) {
                                    $(element).closest('div').parent("div.form-group").addClass("has-error");
                                    $(element).closest('div').next("span.help-block").html(val.message);
                                    y++; c++;
                                }
                                else {
                                    $(element).parent().next("span.help-block").html('');
                                    setBlank(element, val);
                                }
                                break;
                            case "radio":
                                var element = ('.' + i);
                                if ($(element + ':checked').length == 0) {
                                    $(element).closest('div').parent("div.form-group").addClass("has-error");
                                    $(element).closest('div').next("span.help-block").html(val.message);
                                    y++; c++;
                                }
                                else {
                                    $(element).closest('div').next("span.help-block").html('');
                                    setBlank(element, val);
                                }
                                break;
                            case "text_button":
                                var element = ('#' + i);
                                if ($(element).val() == '') {
                                    $(element).parent().parent().parent().addClass("has-error");
                                    $(element).parent().next("span.help-block").html(val.message);
                                    y++; c++;
                                }
                                else {
                                    $(element).parent().parent().parent().removeClass("has-error");
                                    $(element).parent().next("span.help-block").html('');
                                    setBlank(element, val);
                                }
                                break;
                            case "text_other":
                                var parent = $('#' + val.parent).val();
                                var element = ('#' + i);
                                if (parent == val.parent_val) {
                                    if ($(element).val() == '') {
                                        setError(element, val)
                                        y++; c++;
                                    }
                                    else {
                                        setBlank(element, val);
                                    }
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "confirm":
                                var password = $('#' + val.value);
                                var element = $('#' + i);
                                if (password.val() != element.val()) {
                                    setError(element, val)
                                    setError(password, '')
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }

                                break;
                            case "input-group":
                                var element = $('#' + i);
                                if (val.require) {
                                    if (element.val() == '') {
                                        setInputGroupError(element, val)
                                        y++; c++;
                                    }
                                    else {
                                        setBlank(element, val);
                                    }
                                }

                                break;
                            case "container_format":
                                var element = $('#' + i);
                                var check = validateContainer(element.val());
                                if (!check) {
                                    setError(element, val)
                                    y++; c++;
                                }
                                else {
                                    setBlank(element, val);
                                }
                                break;
                            case "file":
                                var element = $('#' + i);
                                if (element.val() == '') {
                                    setErrorButton(element, val);
                                    y++; c++;
                                }

                                break;
                        }

                    }
                });
            });

            if (y > 0) {
                return false;
            }
            else {
                return true;
            }
        }
    });

function setError(element, val) {
    if (val.type == 'group') {
        $(element).closest("div.form-group").addClass("has-error");
        $(element).closest("div.form-group").find('label').css("color", "black");
        $(element).closest('div.form-group').find('span.help-block').html(val.message);
    }
    else {
        $(element).closest("div.form-group").addClass("has-error");
        $(element).closest("div.form-group").find('label').css("color", "black");
        $(element).next(".help-block").html(val.message);
    }
}

function setErrorButton(element, val) {
    if (val.type == 'button-circle') {
        $(element).closest("label.btn-file").find('button.btn').removeClass('yellow');
        $(element).closest("label.btn-file").find('button.btn').addClass('red');
        $(element).closest("label.btn-file").find('span.help-block').html(val.message);
    }
}

function setBlankErrorButton(element, val) {
    if (val.type == 'button-circle') {
        $(element).closest("label.btn-file").find('button.btn').removeClass('red');
        $(element).closest("label.btn-file").find('button.btn').addClass('yellow');
    }
}

function setBlank(element, val) {
    if (val.type == 'group') {
        $(element).closest("div.form-group").removeClass("has-error");
        $(element).closest('div.form-group').find('span.help-block').html('');
    }
    else {
        $(element).closest("div.form-group").removeClass("has-error");
        $(element).next(".help-block").html('');
    }
}

function validateContainer(str) {
    var value = [];
    var con = 0;
    var container = str.toUpperCase();
    var checking_number = container.charAt(10);
    if (container.charAt(3) == "U" || str.length == 11) {
        for (var i = 0; i <= 10; i++) {
            if (i < 4) {
                value.push(getNumberFromDigit(container.charAt(i)));
            }
            else {
                value.push(container.charAt(i));
            }
        }

        for (var i = 0; i < value.length; i++) {
            var x = 0;
            switch (i) {
                case 0:
                    x = value[i] * 1;
                    break;
                case 1:
                    x = value[i] * 2;
                    break;
                case 2:
                    x = value[i] * 4;
                    break;
                case 3:
                    x = value[i] * 8;
                    break;
                case 4:
                    x = value[i] * 16;
                    break;
                case 5:
                    x = value[i] * 32;
                    break;
                case 6:
                    x = value[i] * 64;
                    break;
                case 7:
                    x = value[i] * 128;
                    break;
                case 8:
                    x = value[i] * 256;
                    break;
                case 9:
                    x = value[i] * 512;
                    break;
            }
            con = con + x;
        }

        var deg = con / 11 | 0;
        var meg = deg * 11;

        var final = con - meg;

        if (final == checking_number || final == 10) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}

function getNumberFromDigit(digit) {
    var num = 0;
    switch (digit) {
        case "A":
            num = 10;
            break;
        case "B":
            num = 12;
            break;
        case "C":
            num = 13;
            break;
        case "D":
            num = 14;
            break;
        case "E":
            num = 15;
            break;
        case "F":
            num = 16;
            break;
        case "G":
            num = 17;
            break;
        case "H":
            num = 18;
            break;
        case "I":
            num = 19;
            break;
        case "J":
            num = 20;
            break;
        case "K":
            num = 21;
            break;
        case "L":
            num = 23;
            break;
        case "M":
            num = 24;
            break;
        case "N":
            num = 25;
            break;
        case "O":
            num = 26;
            break;
        case "P":
            num = 27;
            break;
        case "Q":
            num = 28;
            break;
        case "R":
            num = 29;
            break;
        case "S":
            num = 30;
            break;
        case "T":
            num = 31;
            break;
        case "U":
            num = 32;
            break;
        case "V":
            num = 34;
            break;
        case "W":
            num = 35;
            break;
        case "X":
            num = 36;
            break;
        case "Y":
            num = 37;
            break;
        case "Z":
            num = 38;
            break;
    }

    return num;
}