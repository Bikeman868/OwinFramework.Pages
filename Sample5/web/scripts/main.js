Vue.component("sample5-tabs", {
    template:"<div><ul class=\"sample5_tabs\"><li v-for=\"tab in tabs\" :class=\"{ 'sample5_is_active': tab.isActive }\"><a @click=\"selectTab(tab)\">{{ tab.name }}</a></li></ul><div class=\"sample5_tabs_details\"><slot></slot></div></div>",
    data: function () {
        return { tabs: [] };
    },
    created: function () {
        this.tabs = this.$children;
    },
    methods: {
        selectTab: function (selectedTab) {
            this.tabs.forEach(function (tab) { tab.isActive = (tab.name === selectedTab.name); });
        }
    }
});

Vue.component("sample5-tab", {
    template:"<div v-show=\"isActive\"><slot></slot></div>",
    props: {
        name: { required: true },
        selected: { default: false }
    },
    data: function () {
        return {
            isActive: false
        };
    },
    mounted: function () { this.isActive = this.selected; }
});
