exported.dispatcher = function() {
    var messages = [];
    var subscribers = [];
    var clientId = null;

    var add = function(message) {
        messages.push(message);
        for (let i = 0; i < subscribers.length; i++) {
            subscribers[i](message);
        }
    }

    var prune = function() {
        var date = new Date();
        for (let i = 0; i < messages.length; i++) {
            age = date - new Date(messages[i].when);
            if (age > 5 / 1440) {
                messages.splice(i, 1);
                i = i - 1;
            }
        }
    }

    ns.cmsmanager.liveUpdateService.register(
        null,
        function (response) { clientId = response.id });

    window.addEventListener("beforeunload", function () {
        if (clientId != undefined) {
            /* Due to a weird bug where beforeunload is called multiple times 
               even though the page was not reloaded, this line is being commented
               out. The session will expire on the server side anyway so this
               housekeeping is a nice to have */
            //ns.cmsmanager.liveUpdateService.deregister({ id: clientId });
        }
    });

    var poll = function () {
        if (clientId == undefined) {
            setTimeout(poll, 1000);
        } else {
            ns.cmsmanager.liveUpdateService.poll(
                { id: clientId },
                function (response) {
                    if (response.messages && response.messages.length > 0) {
                        for (let i = 0; i < response.messages.length; i++)
                            add(response.messages[i]);
                    }
                },
                function () {
                    prune();
                    setTimeout(poll, 3000);
                }
            );
        }
    };

    var subscribe = function (subscriber) {
        subscribers.push(subscriber);
        return function() {
            subscribers = subscribers.filter(function(e) { return e !== subscriber; });
        }
    }

    poll();

    return {
        subscribe: subscribe
    }
}();
