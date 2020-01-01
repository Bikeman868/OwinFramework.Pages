exported.buildDisplayOnlyComponents = function() {
    Vue.component("cms-page-preview", {
        props: {
            pageVersion: {
                required: true,
                type: Object
            }
        },
        template:
/*html*/`<pre class="cms_preview cms_html__page_preview">
&lt;html>
  &lt;head>
    &lt;title>{{pageVersion.title}}&lt;/title>
  &lt;/head>
  &lt;body>
  &lt;/body>
&lt;/html>
</pre>`
    // TODO: Other page version properties
    // TODO: Undefined properties revert to master page
    });

    Vue.component("cms-layout-preview", {
        props: {
            layoutVersion: {
                required: true,
                type: Object
            }
        },
        template:
            `<pre class="cms_preview cms_html__layout_preview">{{html}}</pre>`,
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
            `<pre class="cms_preview cms_html__region_preview">{{html}}</pre>`,
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
/*html*/`<tr class="cms_history_summary">
  <td class="cms_history__when">{{summary.when|cms_formatDate}}</td>
  <td class="cms_history__identity">{{summary.identity|cms_formatUserUrn}}</td>
  <td v-if="events==undefined" class="cms_history__changes">
    {{summary.changes}}
    <button @click="retrieveEvents">...</button>
  </td>
  <td v-else class="cms_history__changes">
    <table class="cms_history_events">
      <tr class="cms_history_events" v-for="event in events">
        <td class="cms_history__when">{{event.when|cms_formatTime}}</td>
        <td class="cms_history__changes">
          <div v-for="change in event.changes">
            <span v-if="change.changeType==='Created'">Created this {{event.recordType|cms_lowercase}}</span>
            <span v-if="change.changeType==='Deleted'">Deleted this {{event.recordType|cms_lowercase}}</span>
            <span v-if="change.changeType==='Modified'">Changed the {{change.field}} from "{{change.oldValue}}" to "{{change.newValue}}"</span>
            <span v-if="change.changeType==='ChildAdded'">Added {{change.childType}} with ID={{change.childId}}<span v-if="change.scenario"> in the {{change.scenario}} scenario</span></span>
            <span v-if="change.changeType==='ChildRemoved'">Removed {{change.childType}} with ID={{change.childId}}<span v-if="change.scenario"> in the {{change.scenario}} scenario</span></span>
          </div>
        </td>
      </tr>
    </table>
  </td>
</tr>`,
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
/*html*/`<div>
  <h2>{{label}}</h2>
  <table class="cms_history_period">
    <tr class="cms_history_summary">
      <th>When</th><th>Who</th><th>What</th>
    </tr>
    <cms-history-summary v-for="summary in summaries" :key="summary.id" :summary="summary">
    </cms-history-summary>
  </table>
</div>`,
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

    Vue.component("cms-view-page-detail", {
        props: {
            page: { type: Object }
        },
       template:
/*html*/`<div>
  <h2>{{page.displayName}} page</h2>
  <div class="cms_field">
    <label>Name</label>
    <p>{{page.name}}</p>
  </div>
  <div v-if="page.description" class="cms_field">
    <label>Description</label>
    <p>{{page.description}}</p>
  </div>
  <div class="cms_field">
    <label>Created</label>
    <p>{{page.createdWhen|cms_formatDateTime}} by {{page.createdBy|cms_formatUserUrn}}</p>
  </div>
</div>`
    });

    Vue.component("cms-view-page-version-detail", {
        props: {
            page: { type: Object },
            pageVersion: { type: Object }
        },
        template:
/*html*/`<div>
  <h2>Version {{pageVersion.version}} of the {{page.displayName}} page</h2>
  <div class="cms_field" v-if="pageVersion.displayName">
      <label>Version name</label>
      <p>{{ pageVersion.displayName }}</p>
  </div>
  <div class="cms_field" v-if="pageVersion.title">
      <label>Page title</label>
      <p>{{ pageVersion.title }}</p>
  </div>
  <div class="cms_field" v-if="pageVersion.canonicalUrl">
      <label>Canonical URL</label>
      <p class="cms_field">{{ pageVersion.canonicalUrl }}</p>
  </div>
  <div class="cms_field" v-if="pageVersion.routes && pageVersion.routes.length > 0">
      <label>Routes to this page</label>
      <p v-for="route in pageVersion.routes">{{ route.path }}</p>
  </div>
  <div class="cms_field">
      <label>Assets</label>
      <p>{{ pageVersion.assetDeployment }}<span v-if="pageVersion.assetDeployment==='PerModule'"> in the {{ pageVersion.moduleName }} module</span></p>
  </div>
  <div class="cms_field" v-if="pageVersion.masterPageId">
      <label>Inherits from master page</label>
      <p>{{ pageVersion.masterPageId | cms_lookupPageId }}</p>
  </div>
  <div class="cms_field">
      <label>Page layout</label>
      <p v-if="pageVersion.layoutName" class="cms_field">{{ pageVersion.layoutName }}</p>
      <p v-else class="cms_field">{{ pageVersion.layoutId | cms_lookupLayoutId }}</p>
      <p v-for="layoutZone in pageVersion.layoutZones" class="cms_field">
          The {{ layoutZone.zone }} layout zone contains
          <span v-if="layoutZone.regionId">the {{ layoutZone.regionId | cms_lookupRegionId }} region</span>
          <span v-else-if="layoutZone.layoutId">the {{ layoutZone.layoutId | cms_lookupLayoutId }} layout</span>
          <span v-else-if="layoutZone.layoutId">the {{ layoutZone.componentId | cms_lookupComponentId }} component</span>
          <span v-else-if="layoutZone.contentType === 'Html'">HTML localizable as '{{ layoutZone.contentName }}'</span>
          <span v-else-if="layoutZone.contentType === 'Template'">the {{ layoutZone.contentName }} template</span>
          <span v-else-if="layoutZone.contentType">the '{{ layoutZone.contentName }}' {{ layoutZone.contentType | cms_lowercase }}</span>
          <span v-else>the default content defined by the layout</span>
      </p>
  </div>
  <div class="cms_field" v-if="pageVersion.bodyStyle">
      <label>Body CSS style</label>
      <p>{{ pageVersion.bodyStyle }}</p>
  </div>
  <div class="cms_field" v-if="pageVersion.permission">
      <label>Requires permission</label>
      <p>{{ pageVersion.permission }}<span v-if="pageVersion.assetPath"> on {{ pageVersion.assetPath }}</span></p>
  </div>
  <div class="cms_field" v-if="pageVersion.components && pageVersion.components.length > 0">
      <label>Dependant components</label>
      <ul><li v-for="component in pageVersion.components">{{ component.componentId | cms_lookupComponentId }}</li></ul>
  </div>
  <div class="cms_field" v-if="pageVersion.dataScopes && pageVersion.dataScopes.length > 0">
      <label>Data scopes</label>
      <ul><li v-for="scope in pageVersion.dataScopes">{{ scope.dataScopeId | cms_lookupDataScopeId }}</li></ul>
  </div>
  <div class="cms_field" v-if="pageVersion.dataTypes && pageVersion.dataTypes.length > 0">
      <label>Data needs</label>
      <ul><li v-for="type in pageVersion.dataTypes">{{ type.dataTypeId | cms_lookupDataTypeId }}</li></ul>
  </div>
  <div class="cms_field" v-if="pageVersion.description">
      <label>Description</label>
      <p>{{ pageVersion.description }}</p>
  </div>
  <div class="cms_field"> 
      <label>Created</label>
      <p>{{ pageVersion.createdWhen | cms_formatDateTime }} by {{ pageVersion.createdBy | cms_formatUserUrn }}</p>
  </div>
</div>`
    });

    Vue.component("cms-view-layout-detail", {
        props: {
            layout: { type: Object }
        },
        template:
/*html*/`<div>
  <h2>{{layout.displayName}} layout</h2>
  <div class="cms_field">
    <label>Name</label>
    <p>{{layout.name}}</p> 
  </div>
  <div v-if="layout.description" class="cms_field">
    <label>Description</label>
    <p>{{layout.description}}</p>
  </div>
  <div class="cms_field">
    <label>Created</label>
    <p>{{layout.createdWhen|cms_formatDateTime}} by {{layout.createdBy|cms_formatUserUrn}}</p> 
  </div>
</div>`
    });

    Vue.component("cms-view-layout-version-detail", {
        props: {
            layout: { type: Object },
            layoutVersion: { type: Object }
        },
        template: 
/*html*/`<div>
    <h2>Version {{ layoutVersion.version }} of the {{ layout.displayName }} layout</h2>
    <div class="cms_field" v-if="layoutVersion.displayName">
        <label>Version name</label>
        <p>{{ layoutVersion.displayName }}</p>
    </div>
    <div class="cms_field">
        <label>Assets</label>
        <p>{{ layoutVersion.assetDeployment }}<span v-if="layoutVersion.assetDeployment==='PerModule'"> in the {{ layoutVersion.moduleName }} module</span></p>
    </div>
    <div class="cms_field">
        <label>Zone nesting</label>
        <p>{{ layoutVersion.zoneNesting }}</p>
    </div>
    <div class="cms_field">
        <label>Zone contents</label>
        <p v-for="zone in layoutVersion.zones" class="cms_field">
            <span>The {{ zone.zone }} zone </span>
            <span v-if="zone.regionId">contains the {{ zone.regionId | cms_lookupRegionId }} region</span>
            <span v-else-if="zone.layoutId">contains the {{ zone.layoutId | cms_lookupLayoutId }} layout</span>
            <span v-else-if="zone.layoutId">contains the {{ zone.componentId | cms_lookupComponentId }} component</span>
            <span v-else-if="zone.contentType === 'Html'">contains HTML localizable as '{{ zone.contentName }}'</span>
            <span v-else-if="zone.contentType === 'Template'">contains the {{ zone.contentName }} template</span>
            <span v-else-if="zone.contentType">contains the '{{ zone.contentName }}' {{ zone.contentType | cms_lowercase }}</span>
            <span v-else> is empty</span>
        </p>
    </div>
    <div class="cms_field" v-if="layoutVersion.tag">
        <label>Html tag enclosing layout</label>
        <p>{{ layoutVersion.tag }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.style">
        <label>Html custom style</label>
        <p>{{ layoutVersion.style }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.classes">
        <label>Additional CSS classes</label>
        <p>{{ layoutVersion.classes }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.nestingTag">
        <label>Nested zone html tag</label>
        <p>{{ layoutVersion.nestingTag }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.nestingStyle">
        <label>Nested zone custom style</label>
        <p>{{ layoutVersion.nestingStyle }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.nestingClasses">
        <label>Nested zone additional classes</label>
        <p>{{ layoutVersion.nestingClasses }}</p>
    </div>
    <div class="cms_field" v-if="layoutVersion.components && layoutVersion.components.length > 0">
        <label>Dependant components</label>
        <ul><li v-for="component in layoutVersion.components">{{ component.componentId | cms_lookupComponentId }}</li></ul>
    </div>
    <div class="cms_field" v-if="layoutVersion.dataScopes && layoutVersion.dataScopes.length > 0">
        <label>Data scopes</label>
        <ul><li v-for="scope in layoutVersion.dataScopes">{{ scope.dataScopeId | cms_lookupDataScopeId }}</li></ul>
    </div>
    <div class="cms_field" v-if="layoutVersion.dataTypes && layoutVersion.dataTypes.length > 0">
        <label>Data needs</label>
        <ul><li v-for="type in layoutVersion.dataTypes">{{ type.dataTypeId | cms_lookupDataTypeId }}</li></ul>
    </div>
    <div class="cms_field" v-if="layoutVersion.description">
        <label>Description</label>
        <p>{{ layoutVersion.description }}</p>
    </div>
    <div class="cms_field">
        <label>Created</label>
        <p>{{ layoutVersion.createdWhen | cms_formatDateTime }} by {{ layoutVersion.createdBy | cms_formatUserUrn }}</p>
    </div>
</div>`
    });

    Vue.component("cms-view-region-detail", {
        props: {
            region: { type: Object }
        },
        template:
/*html*/`<div>
  <h2>{{region.displayName}} region</h2>
  <div class="cms_field">
    <label>Name</label>
    <p>{{region.name}}</p> 
  </div>
  <div v-if="region.description" class="cms_field">
    <label>Description</label>
    <p>{{region.description}}</p>
  </div>
  <div class="cms_field">
    <label>Created</label>
    <p>{{region.createdWhen|cms_formatDateTime}} by {{region.createdBy|cms_formatUserUrn}}</p> 
  </div>
</div>`
    });

    Vue.component("cms-view-region-version-detail", {
        props: {
            region: { type: Object },
            regionVersion: { type: Object }
        },
        template:
/*html*/`<div>
    <h2>Version {{ regionVersion.version }} of the {{ region.displayName }} region</h2>
    <div class="cms_field" v-if="regionVersion.displayName">
        <label>Version name</label>
        <p>{{ regionVersion.displayName }}</p>
    </div>
    <div class="cms_field" v-if="regionVersion.routes && regionVersion.routes.length > 0">
        <label>Routes to this region</label>
        <p v-for="route in regionVersion.routes">{{ route.path }}</p>
    </div>
    <div class="cms_field">
        <label>Assets</label>
        <p>{{ regionVersion.assetDeployment }}<span v-if="regionVersion.assetDeployment==='PerModule'"> in the {{ regionVersion.moduleName }} module</span></p>
    </div>
    <div class="cms_field" v-if="regionVersion.layoutName || regionVersion.layoutId">
        <label>Region layout</label>
        <p v-if="regionVersion.layoutName" class="cms_field">{{ regionVersion.layoutName }}</p>
        <p v-else class="cms_field">{{ regionVersion.layoutId | cms_lookupLayoutId }}</p>
        <p v-for="layoutZone in regionVersion.layoutZones" class="cms_field">
            The {{ layoutZone.zone }} layout zone contains
            <span v-if="layoutZone.regionId">the {{ layoutZone.regionId | cms_lookupRegionId }} region</span>
            <span v-else-if="layoutZone.layoutId">the {{ layoutZone.layoutId | cms_lookupLayoutId }} layout</span>
            <span v-else-if="layoutZone.layoutId">the {{ layoutZone.componentId | cms_lookupComponentId }} component</span>
            <span v-else-if="layoutZone.contentType === 'Html'">HTML localizable as '{{ layoutZone.contentName }}'</span>
            <span v-else-if="layoutZone.contentType === 'Template'">the {{ layoutZone.contentName }} template</span>
            <span v-else-if="layoutZone.contentType">the '{{ layoutZone.contentName }}' {{ layoutZone.contentType | cms_lowercase }}</span>
            <span v-else>the default content defined by the layout</span>
        </p>
    </div>
    <div class="cms_field" v-if="regionVersion.componentName || regionVersion.componentId">
        <label>Region component</label>
        <p v-if="regionVersion.componentName" class="cms_field">{{ regionVersion.componentName }}</p>
        <p v-else class="cms_field">{{ regionVersion.componentId | cms_lookupComponentId }}</p>
    </div>
    <div class="cms_field" v-if="regionVersion.assetName">
        <label>Region HTML</label>
        <p class="cms_field">{{ regionVersion.assetName }}</p>
        <p class="cms_field">{{ regionVersion.assetValue }}</p>
    </div>
    <div class="cms_field" v-if="regionVersion.regionTemplates && regionVersion.regionTemplates.length > 0">
        <label>Region templates</label>
        <p v-for="template in regionVersion.regionTemplates" class="cms_field">
            {{ template.pageArea }} area of the page contains the {{ template.templatePath }} template
        </p>
    </div>
    <div class="cms_field" v-if="regionVersion.repeatDataTypeId">
        <h3>Repeat the contents of this region</h3>
        <div class="cms_field">
            <label>Repeat for each</label>
            <p>
                {{ regionVersion.repeatDataTypeId | cms_lookupDataTypeId }}
                <span v-if="regionVersion.repeatDataScopeId">
                    in {{ regionVersion.repeatDataScopeId | cms_lookupDataScopeId }} scope
                </span>
                <span v-else-if="regionVersion.repeatDataScoeName">
                    in {{ regionVersion.repeatDataScopeName }} scope
                </span>
            </p>
        </div>
        <div class="cms_field" v-if="regionVersion.listElementTag">
            <label>List item Html tag</label>
            <p class="cms_field">{{ regionVersion.listElementTag }}</p>
        </div>
        <div class="cms_field" v-if="regionVersion.listElementStyle">
            <label>List item custom style</label>
            <p class="cms_field">{{ regionVersion.listElementStyle }}</p>
        </div>
        <div class="cms_field" v-if="regionVersion.listElementClasses">
            <label>List item additional CSS classes</label>
            <p class="cms_field">{{ regionVersion.listElementClasses }}</p>
        </div>
        <div v-if="regionVersion.listDataScopeId || regionVersion.listDataScopeName">
            <label>Resolve list item data</label>
            <span v-if="regionVersion.repeatDataScopeId">
                Using {{ regionVersion.listDataScopeId | cms_lookupDataScopeId }} scope
            </span>
            <span v-else-if="regionVersion.repeatDataScoeName">
                Using {{ regionVersion.listDataScopeName }} scope
            </span>
        </div>
    </div>
    <div class="cms_field" v-if="regionVersion.components && regionVersion.components.length > 0">
        <label>Dependant components</label>
        <ul><li v-for="component in regionVersion.components">{{ component.componentId | cms_lookupComponentId }}</li></ul>
    </div>
    <div class="cms_field" v-if="regionVersion.dataScopes && regionVersion.dataScopes.length > 0">
        <label>Data scopes</label>
        <ul><li v-for="scope in regionVersion.dataScopes">{{ scope.dataScopeId | cms_lookupDataScopeId }}</li></ul>
    </div>
    <div class="cms_field" v-if="regionVersion.dataTypes && regionVersion.dataTypes.length > 0">
        <label>Data needs</label>
        <ul><li v-for="type in regionVersion.dataTypes">{{ type.dataTypeId | cms_lookupDataTypeId }}</li></ul>
    </div>
    <div class="cms_field" v-if="regionVersion.description">
        <label>Description</label>
        <p>{{ regionVersion.description }}</p>
    </div>
    <div class="cms_field">
        <label>Created</label>
        <p>{{ regionVersion.createdWhen | cms_formatDateTime }} by {{ regionVersion.createdBy | cms_formatUserUrn }}</p>
    </div>
</div>`
    });

    Vue.component("cms-view-component-detail", {
        props: {
            component: { type: Object }
        },
        template:
/*html*/`<div>
  <h2>{{component.displayName}} component</h2>
  <div class="cms_field">
    <label>Name</label>
    <p>{{component.name}}</p> 
  </div>
  <div v-if="component.description" class="cms_field">
    <label>Description</label>
    <p>{{component.description}}</p>
  </div>
  <div class="cms_field">
    <label>Created</label>
    <p>{{component.createdWhen|cms_formatDateTime}} by {{component.createdBy|cms_formatUserUrn}}</p> 
  </div>
</div>`
    });

    Vue.component("cms-view-component-version-detail", {
        props: {
            component: { type: Object },
            componentVersion: { type: Object }
        },
        template:
/*html*/`<div>
    <h2>Version {{ componentVersion.version }} of the {{ component.displayName }} component</h2>
    <div class="cms_field" v-if="componentVersion.displayName">
        <label>Version name</label>
        <p>{{ componentVersion.displayName }}</p>
    </div>
    <div class="cms_field">
        <label>Assets</label>
        <p>{{ componentVersion.assetDeployment }}<span v-if="componentVersion.assetDeployment==='PerModule'"> in the {{ componentVersion.moduleName }} module</span></p>
    </div>
    <div class="cms_field" v-if="componentVersion.description">
        <label>Description</label>
        <p>{{ componentVersion.description }}</p>
    </div>
    <div class="cms_field">
        <label>Created</label>
        <p>{{ componentVersion.createdWhen | cms_formatDateTime }} by {{ componentVersion.createdBy | cms_formatUserUrn }}</p>
    </div>
</div>`
    });

    Vue.component("cms-view-environment-detail", {
        props: {
            environment: { type: Object }
        },
        template:
/*html*/`<div>
    <h2>{{ environment.displayName }} environment</h2>
    <div class="cms_field"><label>Name</label><p>{{ environment.name }}</p></div>
    <div class="cms_field"><label>Base URL</label><p>{{ environment.baseUrl }}</p></div>
    <div class="cms_field"><label>Website version</label><p>{{ environment.websiteVersionId|cms_lookupWebsiteVersionId }}</p></div>
    <div class="cms_field"><label>Description</label><p>{{ environment.description }}</p></div>
    <div class="cms_field"><label>Created</label><p>{{ environment.createdWhen|cms_formatDateTime }} by {{ environment.createdBy|cms_formatUserUrn }}</p></div>
</div>`
    });

    Vue.component("cms-view-website-version-detail", {
        props: {
            websiteVersion: { type: Object }
        },
        template: 
/*html*/`<div>
    <h2>{{ websiteVersion.displayName }} version of the website</h2>
    <div class="cms_field"><label>Name</label><p>{{ websiteVersion.name }}</p></div>
    <div class="cms_field"><label>Description</label><p>{{ websiteVersion.description }}</p></div>
    <div class="cms_field"><label>Created</label><p>{{ websiteVersion.createdWhen|cms_formatDateTime }} by {{ websiteVersion.createdBy|cms_formatUserUrn }}</p></div>
</div>`
    });

    Vue.component("cms-view-segmentation-scenario-detail", {
        props: {
            scenario: { type: Object }
        },
        template:
/*html*/`<div>
    <h2>{{ scenario.displayName }} a/b testing scenario</h2>
    <div class="cms_field"><label>Name</label><p>{{ scenario.name }}</p></div>
    <div class="cms_field"><label>Description</label><p>{{ scenario.description }}</p></div>
</div>`
    });
}