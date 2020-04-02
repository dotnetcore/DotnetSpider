$(function () {
    app.activeMenu('DownloaderAgent');
});

function exit(id) {
    swal({
        title: "Sure to stop this agent?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.post('/agent/' + id + '/exit', null, function () {
            swal('Success', 'Send signal success', "success");
        }, function () {
            swal('Error', 'Stop agent failed', "error");
        });
    });
}

function deleteAgent(id) {
    swal({
        title: "Sure to delete this agent?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.delete('/agent/' + id, function () {
            window.location.reload();
        }, function () {
            swal('Error', 'Delete agent failed', "error");
        });
    });
}