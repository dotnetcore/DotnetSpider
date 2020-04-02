$(function () {
    app.activeMenu('Repository');
});

function remove(id) {
    swal({
        title: "Sure to delete this repository?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.delete("/docker-repository/" + id, function () {
            window.location.reload();
        }, function () {
            swal('Error', 'Delete failed', "error");
        });
    });
}