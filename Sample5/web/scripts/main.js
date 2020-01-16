Vue.component("sample5-tabs", {
    template: "<div class=\"sample5_tabs\"><ul class=\"sample5_tabs\"><li v-for=\"tab in tabs\" :class=\"{ 'sample5_is_selected': tab.selected }\"><a @click=\"selectTab(tab.name)\">{{ tab.name }}</a></li></ul><div class=\"sample5_tabs_details\"><slot></slot></div></div>",
    props: {
        selected: { type: String }
    },
    watch: {
        selected: "selectTab"
    },
    data: function () {
        return { tabs: [] };
    },
    mounted: function () {
        this.tabs = this.$children;
        this.selectTab(this.selected);
    },
    methods: {
        selectTab: function (tabName) {
            this.tabs.forEach(function (tab) { tab.selected = (tab.name === tabName); });
        }
    }
});

Vue.component("sample5-tab", {
    template:"<div v-show=\"selected\"><slot></slot></div>",
    props: {
        name: { required: true },
        selected: { default: false }
    },
});
