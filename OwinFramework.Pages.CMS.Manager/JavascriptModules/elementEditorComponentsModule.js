exported.buildElementEditorComponents = function() {
    Vue.component("cms-edit-website-version", {
        props: {
            websiteVersion: { type: Object },
            isNew: { type: Boolean, default: false }
        },
        template:
/*html*/`<div>
    <h2 v-if="isNew">Create New Website Version</h2>
    <h2 v-else>Edit Website Version Details</h2>
    <div class="cms_edit_panel">
        <cms-display-name-field-editor label="Display this version in CMS manager as"
                                       placeholder="My website version"
                                       v-bind:display-name="websiteVersion.displayName"
                                       @display-name-changed="websiteVersion.displayName=$event">
        </cms-display-name-field-editor>
        <cms-element-name-field-editor label="Website version name"
                                       placeholder="unique_version_name"
                                       v-bind:element-name="websiteVersion.name"
                                       @element-name-changed="websiteVersion.name=$event">
        </cms-element-name-field-editor >
        <cms-description-field-editor v-bind:description="websiteVersion.description"
                                      @description-changed="websiteVersion.description=$event" >
        </cms-description-field-editor >
    </div>
</div>`
    });

    Vue.component("cms-edit-environment", {
        props: {
            environment: { type: Object },
            isNew: { type: Boolean, default: false }
        },
        template:
/*html*/`<div>
    <h2 v-if="isNew">Create New Environment</h2>
    <h2 v-else>Edit Environment Details</h2>
    <div class="cms_edit_panel">
       <cms-display-name-field-editor label="Show this environment in CMS manager as"
                                      placeholder="Production"
                                      v-bind:display-name="environment.displayName"
                                      @display-name-changed="environment.displayName=$event">
       </cms-display-name-field-editor>
       <cms-element-name-field-editor label="Environment name"
                                      placeholder="unique_environment_name"
                                      v-bind:element-name="environment.name"
                                      @element-name-changed="environment.name=$event">
       </cms-element-name-field-editor>
       <cms-url-field-editor label="Base URL"
                             placeholder="https://environment.mycompany.com/"
                             v-bind:url="environment.baseUrl"
                             @url-changed="environment.baseUrl=$event" >
       </cms-url-field-editor>
       <cms-website-version-field-edior v-bind:website-version-id="environment.websiteVersionId"
                                        @website-version-id-changed="environment.websiteVersionId=$event">
       </cms-website-version-field-edior>
       <cms-description-field-editor v-bind:description="environment.description"
                                     @description-changed="environment.description=$event">
       </cms-description-field-editor>
    </div>
</div>`
    });

    Vue.component("cms-edit-segmentation-scenario", {
        props: {
            scenario: { type: Object },
            isNew: { type: Boolean, default: false }
        },
        template:
/*html*/`<div>
    <h2 v-if="isNew">Create New Scenario</h2>"+
    <h2 v-else>Edit Scenario Details</h2>"+
    <div class="cms_edit_panel">
       <cms-display-name-field-editor label="Show this scenario in CMS manager as"
                                       placeholder="Blue color scheme"
                                       v-bind:display-name="scenario.displayName"
                                       @display-name-changed="scenario.displayName=$event">
       </cms-display-name-field-editor>
       <cms-element-name-field-editor v-if="isNew"
                                      label="Scenario name"
                                      placeholder="unique_scenario_name"
                                      v-bind:element-name="scenario.name"
                                      @element-name-changed="scenario.name=$event">
       </cms-element-name-field-editor>
       <cms-description-field-editor v-bind:description="scenario.description"
                                       @description-changed="scenario.description=$event">
       </cms-description-field-editor>
    </div>
</div>`
    });

    Vue.component("cms-edit-page", {
        props: {
            page: { type: Object },
            pageVersion: { type: Object },
            websiteVersion: { type: Object },
            isNew: { type: Boolean, default: false }
        },
        template:
/*html*/`<div>
    <h2 v-if="isNew">Create New Page</h2>
    <h2 v-else>Edit Page Details</h2>
    <div class="cms_edit_panel">
       <div class="cms_edit_panel">
           <cms-display-name-field-editor label="Display this page in CMS manager as"
                                          placeholder="My page"
                                          v-bind:display-name="page.displayName"
                                          @display-name-changed="page.displayName=$event">
           </cms-display-name-field-editor>
           <cms-element-name-field-editor label="Page name"
                                          placeholder="unique_page_name""+
                                          v-bind:element-name="page.name"
                                          @element-name-changed="page.name=$event">
           </cms-element-name-field-editor>
           <cms-description-field-editor v-bind:description="page.description || ''"
                                         @description-changed="page.description=$event">
           </cms-description-field-editor>
       </div>
       <div v-if="isNew" class="cms_edit_panel">
           <h3>Page features</h3>
           <cms-title-field-editor label="Page title"
                                   placeholder="My new page"
                                   v-bind:title="pageVersion.title"
                                   @title-changed="pageVersion.title=$event">
           </cms-title-field-editor>
           <h3>Master page</h3>
           <cms-page-field-editor label="Master page"
                                  v-bind:page-id="pageVersion.masterPageId"
                                  v-bind:exclude="[pageVersion.elementId]"
                                  @page-id-changed="pageVersion.masterPageId=$event">
           </cms-page-field-editor>
           <h3>Page layout</h3>
           <cms-layout-field-editor label="Page layout"
                                    v-bind:layout-id="pageVersion.layoutId"
                                    v-bind:layout-name="pageVersion.layoutName"
                                    @layout-id-changed="pageVersion.layoutId=$event"
                                    @layout-name-changed="pageVersion.layoutName=$event"
                                    @inherit-changed="layoutInheritChanged($event)">
           </cms-layout-field-editor>
           <cms-layout-zones-field-editor label="Layout zone contents"
                                          v-bind:zone-nesting="zoneNesting"
                                          v-bind:layout-zones="pageVersion.layoutZones">
           </cms-layout-zones-field-editor>
       </div>
    </div>
</div>`,
        computed: {
            zoneNesting: function () {
                var nesting = null;
                if (this.pageVersion.layoutName == undefined || this.pageVersion.layoutName.length === 0) {
                    if (this.pageVersion.layoutId == undefined) {
                        // TODO: Lookup the master page layout
                        nesting = "master1,master2";
                    } else {
                        nesting = "";
                        // TODO: Lookup layout version for website version
                        exported.layoutVersionStore.retrieveRecord(
                            this.pageVersion.layoutId,
                            function (layoutVersion) {
                                nesting = layoutVersion.zoneNesting;
                            });
                    }
                }
                return nesting;
            }
        }
    });

    Vue.component("cms-edit-page-version", {
        props: {
            page: { type: Object },
            pageVersion: { type: Object },
            websiteVersion: { type: Object },
            scenario: { type: String, default: "" },
            isNew: { type: Boolean, default: false }
        },
        template:
/*html*/`<div>
   <h2 v-if="isNew">Create New Page Version</h2>
   <h2 v-else">Edit Version {{pageVersion.version}} of the {{page.displayName|cms_lowercase}} page</h2>
   <div class="cms_edit_panel">
       <cms-display-name-field-editor label="Name this version of the page"
                                       placeholder="Version 1"
                                       v-bind:display-name="pageVersion.displayName"
                                       @display-name-changed="pageVersion.displayName=$event">
       </cms-display-name-field-editor>
       <cms-description-field-editor v-bind:description="pageVersion.description || ''"
                                       @description-changed="pageVersion.description=$event">
       </cms-description-field-editor>
   </div>
   <div class="cms_edit_panel">
       <h3>Page URLs</h3>
       <cms-url-path-field-editor label="Canonical URL path"
                                   v-bind:url-path="pageVersion.canonicalUrl"
                                   @url-path-changed="pageVersion.canonicalUrl=$event">
       </cms-url-path-field-editor>
       <cms-routes-field-editor label="Routes to this page"
                                   v-bind:routes="pageVersion.routes">
       </cms-routes-field-editor>
   </div>
   <div class="cms_edit_panel">
       <h3>Master page</h3>
       <cms-page-field-editor label="Master page"
                               v-bind:page-id="pageVersion.masterPageId"
                               v-bind:exclude="[pageVersion.elementId]"
                               @page-id-changed="pageVersion.masterPageId=$event">
       </cms-page-field-editor>
       <cms-asset-deployment-field-editor label="Asset deployment"
                                           inherit-option="Inherit from master page"
                                           v-bind:asset-deployment="pageVersion.assetDeployment"
                                           v-bind:module-name="pageVersion.moduleName"
                                           @asset-deployment-changed="pageVersion.assetDeployment=$event"
                                           @module-name-changed="pageVersion.moduleName=$event">
       </cms-asset-deployment-field-editor>
   </div>
   <div class="cms_edit_panel">
       <h3>Page layout</h3>
       <cms-layout-field-editor label="Page layout"
                                   v-bind:layout-id="pageVersion.layoutId"
                                   v-bind:layout-name="pageVersion.layoutName"
                                   @layout-id-changed="pageVersion.layoutId=$event"
                                   @layout-name-changed="pageVersion.layoutName=$event"
                                   @inherit-changed="layoutInheritChanged($event)">
       </cms-layout-field-editor>
       <cms-layout-zones-field-editor label="Layout zone contents"
                                       v-bind:zone-nesting="zoneNesting"
                                       v-bind:layout-zones="pageVersion.layoutZones">
       </cms-layout-zones-field-editor>
   </div>
   <div class="cms_edit_panel">
       <h3>Page features</h3>
       <cms-title-field-editor label="Page title"
                               placeholder="My new page"
                               v-bind:title="pageVersion.title"
                               @title-changed="pageVersion.title=$event">
       </cms-title-field-editor>
       <cms-style-field-editor label="Body style"
                               v-bind:css-style="pageVersion.bodyStyle"
                               @css-style-changed="pageVersion.bodyStyle=$event">
       </cms-style-field-editor>
       <cms-permisson-field-editor label="Permission required to view this page"
                                   v-bind:permission="pageVersion.permission"
                                   v-bind:asset-path="pageVersion.assetPath"
                                   @permission-changed="pageVersion.permission=$event"
                                   @asset-path-changed="pageVersion.assetPath=$event">
       </cms-permisson-field-editor>
   </div>
   <div class="cms_edit_panel">
       <h2>Page data</h2>
       <cms-data-scopes-field-editor label="Page data scopes"
                                       v-bind:scopes="pageVersion.dataScopes">
       </cms-data-scopes-field-editor>
       <cms-data-types-field-editor label="Page data needs"
                                       v-bind:scopes="pageVersion.dataTypes">
       </cms-data-types-field-editor>
   </div>
</div>`,
        computed: {
            zoneNesting: function () {
                var nesting = null;
                if (this.pageVersion.layoutName == undefined || this.pageVersion.layoutName.length === 0) {
                    if (this.pageVersion.layoutId == undefined) {
                        // TODO: Lookup the master page layout
                        nesting = "master1,master2";
                    } else {
                        nesting = "";
                        // TODO: Find which version of the layout is used on this
                        //       version of the page in this scenario
                        /*
                        exported.listService.websitePageVersion(
                            {
                                websiteVersionId: this.websiteVersion.Id,
                                scenario: this.scenario,
                                pageId: this.page.Id
                            },
                            function (response) {
                                if (response != undefined) {
                                    var pageVersionId = response.pageVersionId;

                                    if (pageVersionId != undefined) {
                                        exported.layoutVersionStore.retrieveRecord(
                                            this.pageVersion.layoutId,
                                            function (layoutVersion) {
                                                nesting = layoutVersion.zoneNesting;
                                            });
                                    }
                                }
                                if (onFail != undefined) onFail("Failed to get page version for website version");
                            }
                        );
                        */
                    }
                }
                return nesting;
            }
        }
    });
}