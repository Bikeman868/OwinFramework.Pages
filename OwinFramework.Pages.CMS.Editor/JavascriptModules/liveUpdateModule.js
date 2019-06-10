var liveUpdateStore = function() {
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
        function (response) { liveUpdateStore.clientId = response.id });

    window.addEventListener("beforeunload", function () {
        if (liveUpdateStore.clientId != undefined) {
            ns.cmseditor.liveUpdateService.deregister({ id: liveUpdateStore.clientId });
        }
    });

    var poll = function () {
        if (liveUpdateStore.clientId == undefined) {
            setTimeout(poll, 1000);
        } else {
            ns.cmseditor.liveUpdateService.poll(
                { id: liveUpdateStore.clientId },
                function(response) {
                    if (response.messages && response.messages.length > 0) {
                        for (let i = 0; i < response.messages.length; i++)
                            liveUpdateStore.add(response.messages[i]);
                    }
                },
                function() {
                    liveUpdateStore.prune();
                    setTimeout(poll, 3000);
                }
            );
        }
    };

    poll();
}();

exported.liveUpdateStore = liveUpdateStore;