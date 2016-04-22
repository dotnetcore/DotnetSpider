var userid = queryString('userid');
var id = queryString('id');
var name = decodeURI(queryString('name')).replace('+', ' ');

function generateStatus() {
    if (userid == "" || userid == null || id == "" || id == null) {
        return;
    }
    var request = new XMLHttpRequest();
    request.open("GET", "/api/status?userid=" + encodeURI(userid) + "&id=" + encodeURI(id), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = request.responseText;
            if (response == null || response == '') {
                document.getElementById('content_inner').innerHTML = '<span>No Info</span>';
            } else {
                document.getElementById('content_inner').innerHTML = response;
            }
        }
    };
    request.send(null);

    var request1 = new XMLHttpRequest();
    request1.open("GET", "/api/monitor/" + encodeURI(userid) + "/" + encodeURI(id), true);
    request1.onreadystatechange = function () {
        if (request1.readyState == 4 && request1.status == 200) {
            var response = request1.responseText;
            if (response == null || response == '') {
            } else {
                document.getElementById('control').innerHTML = (response);
            }
        }
    };
    request1.send(null);
}

function stop() {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor/stop?userid=" + encodeURI(userid) + "&name=" + encodeURI(name), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = request.responseText;
        }
    };
    request.send(null);
}

function del() {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor/delete?userid=" + encodeURI(userid) + "&name=" + encodeURI(id), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = request.responseText;
            if (response == "ok") {
                window.location.href = 'viewtaskstatus.htm';
            }
        }
    };
    request.send(null);
}


function queryString(qs) {
    var s = urlDecode(location.search);
    s = s.replace("?", "?&").split("&");
    var re = "";
    var str = "";
    for (i = 1; i < s.length; i++)
        if (s[i].indexOf(qs + "=") == 0)
            re = s[i].replace(qs + "=", "");
    return re;
}

function urlEncode(str) { var i, temp, p, q; var result = ""; for (i = 0; i < str.length; i++) { temp = str.charCodeAt(i); if (temp >= 0x4e00) { execScript("ascCode=hex(asc(\"" + str.charAt(i) + "\"))", "vbscript"); result += ascCode.replace(/(.{ 2 })/g, "%$1"); } else { result += escape(str.charAt(i)); } } return result; }

function urlDecode(str) {
    var i, temp;
    var result = "";
    for (i = 0; i < str.length; i++) {
        if (str.charAt(i) == "%") {
            if (str.charAt(++i) == "u") {
                temp = str.charAt(i++) + str.charAt(i++) + str.charAt(i++) + str.charAt(i++) + str.charAt(i);
                result += unescape("%" + temp);
            }
            else {
                temp = str.charAt(i++) + str.charAt(i);
                if (eval("0x" + temp) <= 160) {
                    result += unescape("%" + temp);
                }
                else {
                    temp += str.charAt(++i) + str.charAt(++i) + str.charAt(++i);
                    result += Decode_unit("%" + temp);
                }
            }
        }
        else {
            result += str.charAt(i);
        }
    }
    return result.replace('+', ' ');
}
generateStatus();
document.getElementById('name').innerText = name;
setInterval(generateStatus, 5000);