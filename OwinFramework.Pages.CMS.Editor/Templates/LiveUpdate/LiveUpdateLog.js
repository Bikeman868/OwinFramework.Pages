var live_update_log = new Vue({
    el: "#cms_live_update_log",
    data: {
        updates: [
            {
                when: "2019-05-31",
                from: "SHVANMHALLIDAY",
                changes: "Description of changes"
            }
        ]
    },
    methods: {
        updateTimes: function () {
            var date = new Date();
            for (let i = 0; i < this.updates.length; i++) {
                var updateDate = new Date(this.updates[i].when);
                var secondsAgo = Math.trunc((date - updateDate) / 1000);
                if (isNaN(secondsAgo) && secondsAgo > 1200) {
                    this.updates.splice(i, 1);
                    i = i - 1;
                } else {
                    this.updates[i].elapsed = secondsAgo + "s ago";
                }
            }
            setTimeout(this.updateTimes, 1000);
        }
    },
    created: function () {
        var vm = this;
        this.updates = [];
        liveUpdateData.subscribe(function (update) {
            var updateData =
            {
                when: update.when,
                from: update.machine,
                changes: ""
            };
            if (update.propertyChanges && update.propertyChanges.length > 0) {
                updateData.changes += "Property changes. ";
            }
            if (update.websiteVersionChanges && update.websiteVersionChanges.length > 0) {
                updateData.changes += "Website version changes. ";
            }
            if (update.newElements && update.newElements.length > 0) {
                updateData.changes += "New elements. ";
            }
            if (update.deletedElements && update.deletedElements.length > 0) {
                updateData.changes += "Deleted elements. ";
            }
            if (update.newVersions && update.newVersions.length > 0) {
                updateData.changes += "New element versions. ";
            }
            if (update.deletedElementVersions && update.deletedElementVersions.length > 0) {
                updateData.changes += "Deleted element versions. ";
            }
            vm.updates.unshift(updateData);
        });
        this.updateTimes();
    }
})