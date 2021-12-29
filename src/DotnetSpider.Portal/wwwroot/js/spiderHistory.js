$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 15,
            count: 0,
            items: []
        },
        mounted: function () {
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let data = window.location.href.split('/');
                let id = data[data.length - 2];
                let pagedQuery = getPagedQuery();
                http.get(`/api/v1.0/spiders/${id}/histories?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            exit: function (event) {
                swal({
                    title: "Sure to stop this spider?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    let id = $(event.toElement).parent().parent().parent().attr('id');
                    debugger
                    http.put('/api/v1.0/spiders/' + id + '/exit', null, function () {
                        swal('Success', 'Send signal success', "success");
                    }, function () {
                        swal('Error', 'Stop spider failed', "error");
                    });
                });
            },
        }
    });
});