function remove(id) {
    swal({
        title: "Sure to remove this spider?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.delete("/spider/" + id, function () {
            window.location.reload();
        }, function () {
            swal('Error', 'Delete failed', "error");
        });
    });
}

function run(id) {
    app.post("/spider/" + id + "/run", function () {
        window.location.reload();
    }, null, function (result) {
        swal('Error', result.message, "error");
    });
}