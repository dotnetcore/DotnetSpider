// Write your Javascript code.
function loadContent(event, controler, action, query) {
    $('#content-title').empty().append($(event).attr("title"));

    $('ul.sidebar-menu li.active').attr('class', '');
    $(event).parent().attr('class', 'active');
    $.get("/" + controler + "/" + action + "?" + query,
        function (data) {
            var content = $('#content');
            content.empty().append(data);
        });
}

function stopTask(id) {
    $.get("/taskstatus/stop/?identity=" + id,
        function (data) {
            if (data === "OK") {
                layer.alert("停止成功", { icon: 1 });
            } else {
                layer.alert("停止失败", { icon: 2 });
            }
        });
}



