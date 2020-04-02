function exit(id) {
    swal({
        title: "Sure to exit this spider?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.post("/spider/" + id + "/exit", null,function () {
            swal('Success', 'Send signal success', "success");
        }, function () {
            swal('Error', 'Send exit message failed', "error");
        });
    });
}