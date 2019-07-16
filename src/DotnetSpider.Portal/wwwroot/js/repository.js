$(function () {
    app.activeMenu('Repository');
});

function remove(id) {
    swal({
        title: "确定要删除此镜像仓储吗?",
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