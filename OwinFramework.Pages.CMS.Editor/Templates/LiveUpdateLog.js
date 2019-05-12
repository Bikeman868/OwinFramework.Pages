var live_update_log = new Vue({
    el: "#cms_live_update_log",
    data: {
        updates: [
            {
                when: "12s ago",
                from: "SHVANMHALLIDAY",
                properties: [{elementType:"Region", elementVersion:23, property:"DisplayName", value:"New name"}],
                versions: [{ websiteVersion: 1, elementType: "Region", elementId: 5, fromVersion: 19, toVersion:20 }],
                added: [{ elementType: "Region", elementId: 5 }],
                deleted: [{ elementType: "Layout", elementId: 2 }],
                newVersions: [{ elementType: "Layout", elementId: 2, elementVersion: 3 }],
                deletedVersions: [{ elementType: "Layout", elementId: 2, elementVersion: 1 }]
            }
        ]
    }
})