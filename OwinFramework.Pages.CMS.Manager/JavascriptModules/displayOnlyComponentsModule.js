exported.buildDisplayOnlyComponents = function() {
    Vue.component("cms-page-preview", {
        props: {
            pageVersion: {
                required: true,
                type: Object
            }
        },
        template:
            "<pre class=\"cms_preview cms_html__page_preview\">{{html}}</pre>",
        computed: {
            html: function () {
                var result = "";
                result += "<div>\n";
                result += "</div>";
                return result;
            }
        }
    });

    Vue.component("cms-layout-preview", {
        props: {
            layoutVersion: {
                required: true,
                type: Object
            }
        },
        template:
            "<pre class=\"cms_preview cms_html__layout_preview\">{{html}}</pre>",
        computed: {
            html: function () {
                var result = "";
                var tag = this.layoutVersion.tag;
                var indent = "";
                if (tag) {
                    result += "<" + tag;
                    if (this.layoutVersion.style) result += " style=\"" + this.layoutVersion.style + "\"";
                    if (this.layoutVersion.classes) result += " class=\"" + this.layoutVersion.classes + "\"";
                    result += ">\n";
                    indent = "  ";
                }
                var nesting = this.layoutVersion.zoneNesting || "";
                var nestingTag = this.layoutVersion.nestingTag || "div";
                var zone = "";
                var addZone = function () {
                    if (zone.length > 0) result += indent + zone + "\n";
                    zone = "";
                };
                for (let i = 0; i < nesting.length; i++) {
                    var c = nesting.charAt(i);
                    if (c == " ") continue;
                    if (c == "(") {
                        addZone();
                        result += indent + "<" + nestingTag;
                        if (this.layoutVersion.nestingStyle) result += " style=\"" + this.layoutVersion.nestingStyle + "\"";
                        if (this.layoutVersion.nestingClasses) result += " class=\"" + this.layoutVersion.nestingClasses + "\"";
                        result += ">\n";
                        indent += "  ";
                    } else if (c == ")") {
                        addZone();
                        indent = indent.substring(2);
                        result += indent + "</" + nestingTag + ">\n";
                    } else if (c == ",") {
                        addZone();
                    } else {
                        zone += c;
                    }
                }
                addZone();
                if (tag) result += "</" + tag + ">";
                return result;
            }
        }
    });

    Vue.component("cms-region-preview", {
        props: {
            regionVersion: {
                required: true,
                type: Object
            }
        },
        template:
            "<pre class=\"cms_preview cms_html__region_preview\">{{html}}</pre>",
        computed: {
            html: function () {
                var result = "";
                result += "<div>\n";
                result += "</div>";
                return result;
            }
        }
    });

    Vue.component("cms-history-summary", {
        props: {
            summary: {
                required: true,
                type: Object
            }
        },
        watch: {
            summary: "clearEvents"
        },
        template:
            "<tr class=\"cms_history_summary\">" +
            "  <td class=\"cms_history__when\">{{summary.when|cms_formatDate}}</td>" +
            "  <td class=\"cms_history__identity\">{{summary.identity|cms_formatUserUrn}}</td>" +
            "  <td v-if=\"events==undefined\" class=\"cms_history__changes\">" +
            "    {{summary.changes}}" +
            "    <button @click=\"retrieveEvents\">...</button>" +
            "  </td>" +
            "  <td v-else class=\"cms_history__changes\">" +
            "    <table class=\"cms_history_events\">" +
            "      <tr class=\"cms_history_events\" v-for=\"event in events\">" +
            "        <td class=\"cms_history__when\">{{event.when|cms_formatTime}}</td>" +
            "        <td class=\"cms_history__changes\">" +
            "          <div v-for=\"change in event.changes\">"+
            "            <span v-if=\"change.changeType==='Created'\">Created this {{event.recordType|cms_lowercase}}</span>" +
            "            <span v-if=\"change.changeType==='Deleted'\">Deleted this {{event.recordType|cms_lowercase}}</span>" +
            "            <span v-if=\"change.changeType==='Modified'\">Changed the {{change.field}} from \"{{change.oldValue}}\" to \"{{change.newValue}}\"</span>" +
            "            <span v-if=\"change.changeType==='ChildAdded'\">Added {{change.childType}} with ID={{change.childId}}<span v-if=\"change.scenario\"> in the {{change.scenario}} scenario</span></span>" +
            "            <span v-if=\"change.changeType==='ChildRemoved'\">Removed {{change.childType}} with ID={{change.childId}}<span v-if=\"change.scenario\"> in the {{change.scenario}} scenario</span></span>" +
            "          </div>" +
            "        </td>" +
            "      </tr>" +
            "    </table>" +
            "  </td>" +
            "</tr>",
        data: function () {
            return {
                events: null
            }
        },
        methods: {
            retrieveEvents: function () {
                var vm = this;
                exported.historyService.summary(
                { id: vm.summary.id },
                function(response) {
                    if (response != undefined) {
                        response.forEach(function(e) {
                            e.changes = JSON.parse(e.changeDetails);
                        });
                    }
                    vm.events = response;
                });
            },
            clearEvents: function() {
                this.events = null;
            }
        }
    });

    Vue.component("cms-history-period", {
        props: {
            label: {
                required: false,
                type: String,
                default: "History"
            },
            recordType: {
                required: true,
                type: String
            },
            recordId: {
                required: true,
                type: Number
            }
        },
        watch: {
            recordType: "loadData",
            recordId: "loadData"
        },
        template:
            "<div>" +
            "  <h2>{{label}}</h2>" +
            "  <table class=\"cms_history_period\">" +
            "    <tr class=\"cms_history_summary\">" +
            "      <th>When</th><th>Who</th><th>What</th>" +
            "    </tr>" +
            "    <cms-history-summary v-for=\"summary in summaries\" :key=\"summary.id\" :summary=\"summary\">" +
            "    </cms-history-summary>" +
            "  </table>" +
            "</div>",
        data: function () {
            return {
                summaries: []
            }
        },
        created: function() {
            this.loadData();
        },
        methods: {
            loadData: function () {
                var vm = this;
                vm.summaries = null;
                exported.historyService.period(
                    { type: vm.recordType, id: vm.recordId },
                    function(result) {
                        vm.summaries = result.summaries;
                    });
            }
        }
    });
}