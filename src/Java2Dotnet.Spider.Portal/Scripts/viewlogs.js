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
                html += "<a class=\"taska\" href=\"#\" onclick=\"searchLog('" + userid + "','" + e.name + "');\">" + e.name + " </a>&nbsp;";
            });
            html += "<br/><br/>";
            document.getElementById('tasks').innerHTML = html;
        }
    };
    request.send(null);
}

function searchLog(user, task) {
    userid = user;
    taskid = task;
    generateLogs();
}

function generateLog() {
    if (userid == "" || userid == null || taskid == "" || taskid == null) {
        return;
    }
    var request = new XMLHttpRequest();
    request.open("GET", "/api/log?userid=" + userid + "&taskid=" + taskid + "&page=1&offset=18", true);
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = request.responseText;
            if (response == null || response == '') {
                document.getElementById('content_inner').innerHTML = '<span>No Log</span>';
            } else {
                document.getElementById('content_inner').innerHTML = response;
            }
        }
    };
    request.send(null);
}
refreshtasks();
setInterval(generateLog, 2500);