$(function () {
    let el = '#select2-template';
    Vue.component('select2', {
        props: ['options', 'value'],
        template: el,
        mounted: function () {
            let vm = this;
            $(this.$el)
                // init select2
                .select2({
                    data: this.options,
                    minimumResultsForSearch: Infinity
                })
                .val(this.value)
                .trigger('change')
                // emit event on change.
                .on('change', function () {
                    vm.$emit('input', this.value)
                })
        },
        watch: {
            value: function (value) {
                // update value
                $(this.$el)
                    .val(value)
                    .trigger('change')
            },
            options: function (options) {
                // update options
                $(this.$el).empty().select2({data: options})
            }
        },
        destroyed: function () {
            $(this.$el).off().select2('destroy')
        }
    });
});

