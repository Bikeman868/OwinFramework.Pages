exported.buildGenericComponents = function() {
    Vue.component("cms-tabs", {
        template:
            "<div>"+
            "    <ul class=\"cms_tabs\">" +
            "      <li v-for=\"tab in tabs\" :class=\"{ 'cms_is_active': tab.isActive }\">"+
            "          <a :href=\"tab.href\" @click=\"selectTab(tab)\">{{ tab.name }}</a>"+
            "      </li>"+
            "    </ul>"+
            "    <div class=\"cms_tabs_details\">"+
            "        <slot></slot>"+
            "    </div>"+
            "</div>",
        data: function() {
            return { tabs: [] };
        },
        created: function() {
            this.tabs = this.$children;
        },
        methods: {
            selectTab: function(selectedTab) {
                this.tabs.forEach(function(tab) { tab.isActive = (tab.name === selectedTab.name); });
            }
        }
    });

    Vue.component("cms-tab", {
        template:"<div v-show=\"isActive\"><slot></slot></div>",
        props: {
            name: { required: true },
            selected: { default: false}
        },
        data: function() {
            return {
                isActive: false
            };
        },
        computed: {
            href: function() { return "#" + this.name.toLowerCase().replace(/ /g, "-"); }
        },
        mounted: function() { this.isActive = this.selected; }
    });

    Vue.component("cms-modal-dialog", {
        template:
            "<div v-if=\"visible\" class=\"cms_modal\">" +
            "  <div>" +
            "    <h1 v-if=\"title\">{{title}}</h1>" +
            "    <p v-if=\"message\">{{message}}</p>" +
            "    <div><button v-for=\"button in buttons\" @click=\"buttonClicked(button.onclick)\">{{button.caption}}</button></div>" +
            "  </div>" +
            "</div>",
        props: {
            visible: {
                required: false,
                type: Boolean,
                default: false
            },
            title: {
                required: false,
                type: String
            },
            message: {
                required: false,
                type: String
            },
            buttons: {
                required: false,
                type: Array,
                default: function () { return [{ caption: "OK", onclick: null }] }
            }
        },
        methods: {
            buttonClicked: function (onclick) {
                if (onclick != undefined) onclick();
                this.$emit("close");
            }
        }
    });
}