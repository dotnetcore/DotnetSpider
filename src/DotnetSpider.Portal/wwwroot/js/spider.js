$(function () {
    new Vue({
        el: '#app',
        data: {
            queryParam: {},
            items: [],
            title: 'Add spider',
            item: {},
            count: 0
        },
        mounted: function () {
            this.queryParam = getQueryParam();
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                http.get(`/api/v1.0/spiders?keyword=${this.queryParam.keyword}&page=${this.queryParam.page}&limit=${this.queryParam.limit}`, function (result) {
                    that.queryParam.page = result.data.page;
                    that.queryParam.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            add: function () {
                this.title = 'Add spider';
                this.item = {};
                $('#modal').modal('show')
            },
            edit: function (record) {
                this.title = 'Edit spider';
                this.item = {
                    id: record.id,
                    name: record.name,
                    image: record.image,
                    cron: record.cron,
                    environment: record.environment,
                    volume: record.volume
                };
                $('#modal').modal('show')
            },
            submit: function () {
                const that = this;
                if (this.item.id) {
                    debugger
                    http.put('/api/v1.0/spiders/' + this.item.id, this.item, function () {
                        that.load();
                    });
                } else {
                    http.post('/api/v1.0/spiders', this.item, function () {
                        that.load();
                    });
                }
                $('#modal').modal('hide')
            },
            run: function (event) {
                swal({
                    title: "Sure to run this spider?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    let id = $(event.toElement).parent().parent().parent().attr('id');
                    http.put('/api/v1.0/spiders/' + id + '/run', null, function () {
                        swal('Success', 'Send signal success', "success");
                    });
                });
            },
            enable: function (event) {
                const that = this;
                let id = $(event.toElement).parent().parent().parent().attr('id');
                http.put('/api/v1.0/spiders/' + id + '/enable', null, function () {
                    that.load();
                });
            },
            disable: function (event) {
                const that = this;
                swal({
                    title: "Sure to disable this spider?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    let id = $(event.toElement).parent().parent().parent().attr('id');
                    http.put('/api/v1.0/spiders/' + id + '/disable', null, function () {
                        that.load();
                    });
                });
            },
            remove: function (event) {
                const that = this;
                swal({
                    title: "Sure to remove this spider?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    let id = $(event.toElement).parent().parent().parent().attr('id');
                    debugger
                    http.delete('/api/v1.0/spiders/' + id, function () {
                        that.load();
                    }, function () {
                        swal('Error', 'Remove spider failed', "error");
                    });
                });
            }
        }
    });
});