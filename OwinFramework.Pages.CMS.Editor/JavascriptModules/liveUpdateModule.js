var liveUpdateData = function() {
    var updates = [];
    var subscribers = [];

    var add = function(update) {
        updates.push(update);
        for (let i = 0; i < subscribers.length; i++) {
            subscribers[i](update);
        }
    }

    var prune = function() {
        var date = new Date();
        for (let i = 0; i < updates.length; i++) {
            age = date - new Date(updates[i].when);
            if (age > 5 / 1440) {
                updates.splice(i, 1);
                i = i - 1;
            }
        }
    }

    var subscribe = function(subscriber) {
        subscribers.push(subscriber);
    }

    return {
        updates: updates,
        add: add,
        prune: prune,
        subscribe: subscribe
    }
}();

var liveUpdateService = function () {
    var register = function () {
        alert("Register for live update");
        ns.ajax.restModule.sendGet();
        return 99;
    };

    var deregister = function (id) {
        alert("Deregister for live update " + id);
    };

    var poll = function (id) {
        var date = new Date();
        return [
            {
                id: "673267",
                when: date,
                machine: "MARTIN",
                propertyChanges: [
                {
                    elementType: "Region",
                    versionId: 12,
                    name: "Title",
                    value: "New Title"
                }]
            },
            {
                id: "8743526",
                when: date,
                machine: "CONTENT1",
                newElements: [
                    {
                        elementType: "Region",
                        id: 786
                    }
                ]
            }
        ];
    };

    return {
        register: register,
        deregister: deregister,
        poll: poll
    }
}();

var liveUpdatePoller = function () {
    var id = liveUpdateService.register();

    window.addEventListener("beforeunload", function () {
        liveUpdateService.deregister(id);
    });

    var poll = function () {
        var updates = liveUpdateService.poll(id);
        if (updates && updates.length > 0) {
            for (let i = 0; i < updates.length; i++)
                liveUpdateData.add(updates[i]);
        }
        liveUpdateData.prune();
        setTimeout(poll, 3000);
    };

    poll();

    return {
        id: id
    }
}();

exported.liveUpdateData = liveUpdateData;