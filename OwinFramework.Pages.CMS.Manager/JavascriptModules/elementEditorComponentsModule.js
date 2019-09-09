exported.buildElementEditorComponents = function() {
    Vue.component("cms-edit-website-version", {
        props: {
            websiteVersion: { type: Object }
        },
        template:
            "<div>" +
            "    <div class=\"cms_edit_panel\">" +
            "        <cms-display-name-field-editor label=\"Display this version in CMS manager as\"" +
            "                                       placeholder=\"My website version\"" +
            "                                       v-bind: display-name=\"websiteVersion.displayName\"" +
            "                                       @display-name-changed=\"websiteVersion.displayName=$event\">" +
            "        </cms-display-name-field-editor>" +
            "        <cms-element-name-field-editor label=\"Website version name\"" +
            "                                       placeholder=\"unique_version_name\"" +
            "                                       v-bind: element-name=\"websiteVersion.name\"" +
            "                                       @element-name - changed=\"websiteVersion.name=$event\">" +
            "        </cms-element-name-field-editor >" +
            "        <cms-description-field-editor v-bind: description=\"websiteVersion.description\"" +
            "                                      @description-changed=\"websiteVersion.description=$event\" >" +
            "        </cms-description-field-editor >" +
            "    </div>" +
            "</div>"
    });
}