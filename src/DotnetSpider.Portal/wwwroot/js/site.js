// Write your Javascript code.
function loadContent(event, controler, action, query) {
    $('#content-title').empty().append($(event).attr("title"));
    $.get("/" + controler + "/" + action + "?" + query,
        function (data) {
            var content = $('#content');
            content.empty().append(data);
        });
}

