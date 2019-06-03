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

var liveUpdatePoller = function () {
    ns.cmseditor.liveUpdateService.register(
        null,
        function (response) { liveUpdateData.clientId = response.id });

    window.addEventListener("beforeunload", function () {
        ns.cmseditor.liveUpdateService.deregister(
            { id: liveUpdateData.clientId });
    });

    var poll = function () {
        if (liveUpdateData.clientId == undefined) {
            setTimeout(poll, 1000);
        } else {
            ns.cmseditor.liveUpdateService.poll(
                {
                    id: liveUpdateData.clientId
                },
                function(response) {
                    if (response.updates && response.updates.length > 0) {
                        for (let i = 0; i < response.updates.length; i++)
                            liveUpdateData.add(response.updates[i]);
                    }
                },
                function() {
                    liveUpdateData.prune();
                    setTimeout(poll, 3000);
                }
            );
        }
    };

    poll();
}();

exported.liveUpdateData = liveUpdateData;