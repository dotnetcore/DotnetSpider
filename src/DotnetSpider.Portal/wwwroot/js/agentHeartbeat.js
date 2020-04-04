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
                debugger
                let pagedQuery = getPagedQuery();
                http.get(`/api/v1.0/agents/${id}/heartbeats?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            }
        }
    });
});