// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function handleSuccess(result, success, error) {
    if (result && result.code && result.code !== 0) {
        if (error) {
            error(result);
        } else {
            let msg = result.msg ? result.msg : 'Unknown error';
            swal('Error', msg, "error");
        }
    } else {
        if (success) {
            success(result);
        }
    }
}

function handleError(result, error) {
    if (error) {
        if (result.responseJSON) {
            error(result.responseJSON);
        } else if (result.statusText) {
            error(result.statusText);
        } else {
            error(result);
        }
    } else {
        if (result.responseJSON) {
            if (!result.responseJSON.errors) {
                swal('Error', result.responseJSON.msg ? result.responseJSON.msg : 'Unknown error', "error");
            } else {
                let errors = '\n';
                result.responseJSON.errors.forEach(x => {
                    errors += x.error + '\n';
                });
                swal(result.responseJSON.msg ? result.responseJSON.msg : 'Error', errors, "error");
            }
        } else if (result.statusText) {
            swal('Error', result.statusText ? result.statusText : 'Unknown error', "error");
        } else {
            swal('Error', 'Unknown error', "error");
        }
    }
}

const http = {};
http.get = function (url, success, error) {
    $.ajax({
        url: url,
        method: 'GET',
        success: function (result) {
            handleSuccess(result, success, error);
        },
        error: function (result) {
            handleError(result, error);
        }
    });
};
http.delete = function (url, success, error) {
    $.ajax({
        url: url,
        method: 'DELETE',
        success: function (result) {
            handleSuccess(result, success, error);
        },
        error: function (result) {
            handleError(result, error);
        }
    });
};
http.post = function (url, data, success, error) {
    $.ajax({
        url: url,
        data: data ? JSON.stringify(data) : null,
        method: 'POST',
        contentType: 'application/json',
        success: function (result) {
            handleSuccess(result, success, error);
        },
        error: function (result) {
            handleError(result, error);
        }
    });
};
http.put = function (url, data, success, error) {
    $.ajax({
        url: url,
        data: data ? JSON.stringify(data) : null,
        method: 'PUT',
        contentType: 'application/json',
        success: function (result) {
            handleSuccess(result, success, error);
        },
        error: function (result) {
            handleError(result, error);
        }
    });
};

function getQueryArgument(name) {
    let url = new URL(window.location.href);
    return url.searchParams.get(name);
}

function uuid() {
    let s = [];
    let hexDigits = "0123456789abcdef";
    for (let i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
    s[8] = s[13] = s[18] = s[23] = "-";

    return s.join("");
}

function formatDate(time, format = 'YY-MM-DD hh:mm:ss') {
    if (!time) {
        return '';
    }
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
}

function getPagedQuery() {
    let page = getQueryArgument('page');
    page = page === null ? 1 : page;
    let limit = getQueryArgument('limit');
    limit = limit === null ? 15 : limit;
    let keyword = getQueryArgument('keyword');
    keyword = keyword === null ? '' : keyword;
    return `page=${page}&limit=${limit}&keyword=${keyword}`;
}

function getQueryParam() {
    let page = getQueryArgument('page');
    page = page === null ? 1 : page;
    let limit = getQueryArgument('limit');
    limit = limit === null ? 15 : limit;
    let keyword = getQueryArgument('keyword');
    keyword = keyword === null ? '' : keyword;
    return {
        page: page,
        limit: limit,
        keyword: keyword,
    };
}