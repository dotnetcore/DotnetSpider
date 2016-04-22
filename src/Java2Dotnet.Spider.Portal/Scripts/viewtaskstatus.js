var userid = "";
var taskid = "";

function refreshtasks() {
    userid = document.getElementById('userid').value;
    var request = new XMLHttpRequest();
    request.open("GET", "/api/task?userid=" + encodeURI(userid));
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = request.responseText;
            var result = JSON.parse(response);
            var html = "<br/><br/>";
            result.forEach(function (e) {
                html += "<a class=\"taska\" href=\"#\" onclick=\"searchStatus('" + userid + "','" + e.name + "');\">" + e.name + " </a>&nbsp;";
            });
            html += "<br/><br/>";
            document.getElementById('tasks').innerHTML = html;
        }
    };
    request.send(null);
}
function searchStatus(user, task) {
    userid = user;
    taskid = task;
    generateStatus();
}

function generateStatus() {
    if (userid == "" || userid == null || taskid == "" || taskid == null) {
        return;
    }
    var request = new XMLHttpRequest();
    request.open("GET", "/api/status?userid=" + encodeURI(userid) + "&taskid=" + encodeURI(taskid) + "&page=1&offset=18", true);
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
}

function stop(uid,name) {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor?op=stop&userid=" + encodeURI(uid) + "&name=" + encodeURI(name), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            generateStatus();
        }
    };
    request.send(null);
}

function del(uid,id) {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor?op=delete&userid=" + encodeURI(uid) + "&name=" + encodeURI(id), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            generateStatus();
        }
    };
    request.send(null);
}

function start(uid, name) {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor?op=start&userid=" + encodeURI(uid) + "&name=" + encodeURI(name), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            generateStatus();
        }
    };
    request.send(null);
}

function exit(uid, name) {
    var request = new XMLHttpRequest();
    request.open("GET", "/api/monitor?op=exit&userid=" + encodeURI(uid) + "&name=" + encodeURI(name), true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            generateStatus();
        }
    };
    request.send(null);
}

refreshtasks();
setInterval(generateStatus, 2500);