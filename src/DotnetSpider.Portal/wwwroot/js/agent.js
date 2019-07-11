$(function () {
    app.activeMenu('DownloaderAgent');
});

function exit(id) {
    swal({
        title: "确定要停止此下载代理器吗?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.post('/downloader-agent/' + id + '/exit', null, function () {
            swal('Success', '退出消息发送成功', "success");
        }, function () {
            swal('Error', 'Exit failed', "error");
        });
    });
}

function deleteAgent(id) {
    swal({
        title: "确定要删除此下载代理器吗?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.delete('/downloader-agent/' + id, function () {
            window.location.reload();
        }, function () {
            swal('Error', 'Exit failed', "error");
        });
    });
}