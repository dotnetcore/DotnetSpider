$(function () {
    let el = '#pagination-template';
    Vue.component('pagination', {
        template: el,
        props: {
            perPages: {
                type: Number,
                default: 9
            },
            page: {
                type: Number,
                default: 1
            },
            limit: {
                type: Number,
                default: 10
            },
            count: {
                type: Number,
                default: 1
            }
        },
        methods: {
            prev: function () {
                if (this.page > 1) {
                    this.go(this.page - 1)
                }
            },
            next: function () {
                if (this.page < this.totalPage) {
                    this.go(this.page + 1)
                }
            },
            first: function () {
                if (this.page !== 1) {
                    this.go(1)
                }
            },
            last: function () {
                if (this.page !== this.totalPage) {
                    this.go(this.totalPage)
                }
            },
            go: function (page) {
                if (this.page !== page) {
                    let url = new URL(window.location.href);

                    url.searchParams.set('limit', this.limit);
                    url.searchParams.set('page', page);

                    window.location.href = url.toString();
                }
            }
        },
        computed: {
            totalPage: function () {
                return Math.ceil(this.count / this.limit)
            },
            disablePre: function () {
                return this.page <= 1;
            },
            disableFirst: function () {
                return this.page === 1;
            },
            disableLast: function () {
                return this.page === this.totalPage;
            },
            disableNext: function () {
                return this.page >= this.totalPage;
            },
            pagers: function () {
                const array = [];

                const pages = this.totalPage;
                let page = this.page;

                const half = this.perPages % 2 === 0 ? this.perPages / 2 : Math.ceil(this.perPages / 2) + 1;
                const offset = {
                    start: page - half,
                    end: page + half
                };

                if (offset.start < 1) {
                    offset.end = offset.end + (1 - offset.start);
                    offset.start = 1
                }
                if (offset.end > pages) {
                    offset.start = offset.start - (offset.end - pages);
                    offset.end = pages
                }
                if (offset.start < 1) offset.start = 1;

                for (let i = offset.start; i <= offset.end; i++) {
                    array.push(i)
                }
                return array
            }
        }
    });
});