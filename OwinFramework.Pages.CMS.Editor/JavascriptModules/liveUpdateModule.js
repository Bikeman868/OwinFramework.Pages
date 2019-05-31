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
        ns.ajax.restModule.getJson({
            url: "/cms/api/live-update/register",
            isSuccess: function(ajax) {
                return ajax.status === 200 && ajax.response.success;
            },
            onSuccess: function(ajax) {
                liveUpdateService.clientId = ajax.response.id;
            }
        });
    };

    var deregister = function () {
        ns.ajax.restModule.getJson({
            url: "/cms/api/live-update/deregister?id=" + liveUpdateService.clientId,
            isSuccess: function (ajax) {
                return ajax.status === 200 && ajax.response.success;
            },
            onSuccess: function (ajax) {
                liveUpdateService.clientId = undefined;
            }
        });
    };

    var poll = function (onSuccess, onDone) {
        if (liveUpdateService.clientId == undefined) {
            onDone();
        } else {
        ns.ajax.restModule.getJson({
                url: "/cms/api/live-update/poll?id=" + liveUpdateService.clientId,
                isSuccess: function(ajax) {
                    return ajax.status === 200 && ajax.response.success;
                },
                onSuccess: function(ajax) {
                    onSuccess(ajax.response.messages);
                },
                onDone: function(ajax) {
                    onDone();
                }
            });
        }
    };

    return {
        register: register,
        deregister: deregister,
        poll: poll
    }
}();

var liveUpdatePoller = function () {
    liveUpdateService.register();

    window.addEventListener("beforeunload", function () {
        liveUpdateService.deregister();
    });

    var poll = function () {
        liveUpdateService.poll(
            function(updates) {
                if (updates && updates.length > 0) {
                    for (let i = 0; i < updates.length; i++)
                        liveUpdateData.add(updates[i]);
                }
            },
            function() {
                liveUpdateData.prune();
                setTimeout(poll, 3000);
                    
            }
        );
    };

    poll();
}();

exported.liveUpdateData = liveUpdateData;