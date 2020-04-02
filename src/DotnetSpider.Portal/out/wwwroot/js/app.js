const app = {};
app.getUrlParam = function (name) {
    let result = location.search.match(new RegExp("[\?\&]" + name + "=([^\&]+)", "i"));
    if (result === null || result.length < 1) {
        return "";
    }
    return result[1];
};
app.getPathPart = function (url, part) {
    url = url.replace(/^http:\/\/[^/]+/, "");
    let parts = url.split('/');
    let result = '';
    if (!part) {
        result = parts[parts.length - 1];
    } else {
        part = part + 1;
        if (part < parts.length) {

            result = parts[part];
        }
    }
    return decodeURI(result);
};
app.successHandler = function (result, success, error) {
    if (result && result.code && result.code !== 200) {
        if (error) {
            error(result);
        } else {
            swal('Error', result.msg ? result.msg : 'Un-know error', "error");
        }
    } else {
        if (success) {
            success(result);
        }
    }
};
app.errorHandler = function (result, error) {
    if (error) {
        error(result.responseJSON);
    } else {
        if (swal) {
            if (result.statusText) {
                swal('Error', result.statusText, "error");
            } else {
                swal('Error', 'Un-know error', "error");
            }
        }
    }
};
app.post = function (url, data, success, error) {
    $.ajax({
        url: url,
        data: data ? JSON.stringify(data) : null,
        headers: {
            RequestVerificationToken: $('input[name$="__RequestVerificationToken"]').val()
        },
        method: 'POST',
        contentType: 'application/json',
        success: function (result) {
            app.successHandler(result, success, error);
        },
        error: function (result) {
            app.errorHandler(result, error);
        }
    });
};
app.get = function (url, success, error) {
    $.ajax({
        url: url,
        method: 'GET',
        success: function (result) {
            app.successHandler(result, success, error);
        },
        error: function (result) {
            app.errorHandler(result, error);
        }
    });
};
app.delete = function (url, success, error) {
    $.ajax({
        url: url,
        method: 'DELETE',
        success: function (result) {
            app.successHandler(result, success, error);
        },
        error: function (result) {
            app.errorHandler(result, error);
        }
    });
};
app.put = function (url, data, success, error) {
    $.ajax({
        url: url,
        data: data ? JSON.stringify(data) : null,
        method: 'PUT',
        success: function (result) {
            app.successHandler(result, success, error);
        },
        error: function (result) {
            app.errorHandler(result, error);
        }
    });
};

app.head = function (url, success, error) {
    $.ajax({
        url: url,
        method: 'HEAD',
        success: function (result) {
            if (success) success();
        },
        error: function (result) {
            if (error) error();
        }
    });
};
app.ui = {};
app.formatDate = function (time, format = 'YY-MM-DD hh:mm:ss') {
    let date = new Date(time);

    let year = date.getFullYear(),
        month = date.getMonth() + 1,//月份是从0开始的
        day = date.getDate(),
        hour = date.getHours(),
        min = date.getMinutes(),
        sec = date.getSeconds();
    let preArr = Array.apply(null, Array(10)).map(function (elem, index) {
        return '0' + index;
    });

    return format.replace(/YY/g, year)
        .replace(/MM/g, preArr[month] || month)
        .replace(/DD/g, preArr[day] || day)
        .replace(/hh/g, preArr[hour] || hour)
        .replace(/mm/g, preArr[min] || min)
        .replace(/ss/g, preArr[sec] || sec);
};
$(function () {
    $(function () {
        $('.select2').select2({
            minimumResultsForSearch: Infinity
        });
        $('input').iCheck({
            checkboxClass: 'icheckbox_square-blue',
            radioClass: 'iradio_square-blue',
            increaseArea: '20%' /* optional */
        });
    });
});
app.ui.setBusy = function () {
    $("#loading").css("display", "");
};
app.ui.clearBusy = function () {
    $("#loading").css("display", "none");
};
app.activeMenu = function (...ids) {
    $("li.menu").removeClass('active');
    ids.forEach((id) => {
        $("#Menu" + id).addClass('active');
});
};
