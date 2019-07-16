function exit(id) {
    swal({
        title: "Sure to exit this spider?",
        type: "warning",
        showCancelButton: true
    }, function () {
        app.post("/spider/" + id + "/exit", null,function () {
            swal('Success', '退出消息发送成功', "success");
        }, function () {
            swal('Error', 'Send exit message failed', "error");
        });
    });
}