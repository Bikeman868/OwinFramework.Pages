exported.update_notifier_log_vm = function (eId) {
    return new Vue({
        el: "#" + eId,
        data: {
            visible: true,
            messages: []
        },
        methods: {
            updateTimes: function() {
                var vm = this;
                var date = new Date();
                for (let i = 0; i < vm.messages.length; i++) {
                    var message = vm.messages[i];
                    var updateDate = new Date(message.when);
                    var secondsAgo = Math.trunc((date - updateDate) / 1000);
                    if (isNaN(secondsAgo) && secondsAgo > 600) {
                        vm.messages.splice(i, 1);
                        i = i - 1;
                    } else {
                        message.elapsed = secondsAgo + "s ago";
                    }
                }
                setTimeout(vm.updateTimes, 1000);
            },
            show: function (context, managerContext) {
                var vm = this;
                vm._updateNotifierUnsubscribe = exported.updateNotifier.subscribe(function (message) {
                    var updateData =
                    {
                        when: message.when,
                        elapsed: "",
                        from: message.machine,
                        changes: ""
                    };
                    if (message.propertyChanges) {
                        for (let i = 0; i < message.propertyChanges.length; i++) {
                            var change = message.propertyChanges[i];
                            updateData.changes += change.recordType + " #" + change.id + " " + change.name;
                            if (change.index != undefined) updateData.changes += "[" + change.index + "]";
                            updateData.changes += " = ";
                            if (change.objectValue != undefined)
                                updateData.changes += JSON.stringify(change.objectValue);
                            else if (change.arrayValue != undefined)
                                updateData.changes += JSON.stringify(change.arrayValue);
                            else if (change.changedProperties != null)
                                updateData.changes += "{}";
                            else updateData.changes += "\"" + change.value + "\"";
                            updateData.changes += " ";
                        }
                    }
                    if (message.websiteVersionChanges && message.websiteVersionChanges.length > 0) {
                        updateData.changes += "Website version changes. ";
                    }
                    if (message.newRecords) {
                        for (let i = 0; i < message.newRecords.length; i++) {
                            var newRecord = message.newRecords[i];
                            updateData.changes += "New " + newRecord.recordType + " #" + newRecord.id + " ";
                        }
                    }
                    if (message.deletedRecords) {
                        for (let i = 0; i < message.deletedRecords.length; i++) {
                            var deletedRecord = message.deletedRecords[i];
                            updateData.changes += deletedRecord.recordType + " #" + deletedRecord.id + " deleted ";
                        }
                    }
                    vm.messages.unshift(updateData);
                });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                if (vm._updateNotifierUnsubscribe != undefined) {
                    vm._updateNotifierUnsubscribe();
                    vm._updateNotifierUnsubscribe = null;
                }
                vm.visible = false;
            }
        },
        created: function() {
            var vm = this;
            vm.messages = [];
            this.updateTimes();
        }
    });
}