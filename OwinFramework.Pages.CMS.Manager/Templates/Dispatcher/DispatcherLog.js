﻿exported.dispatcher_log_vm = function (eId) {
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
                vm._dispatcherUnsubscribe = exported.dispatcher.subscribe(function (message) {
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
                            updateData.changes += change.recordType + " #" + change.id + " " + change.name + "=\"" + change.value + "\" ";
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
                    if (message.childListChanges) {
                        for (let i = 0; i < message.childListChanges.length; i++) {
                            var childListChange = message.childListChanges[i];
                            updateData.changes += childListChange.recordType + " #" + childListChange.id + " " + childListChange.childRecordType + "s changed ";
                        }
                    }
                    vm.messages.unshift(updateData);
                });
                vm.visible = true;
            },
            hide: function() {
                var vm = this;
                if (vm._dispatcherUnsubscribe != undefined) {
                    vm._dispatcherUnsubscribe();
                    vm._dispatcherUnsubscribe = null;
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