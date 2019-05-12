exported.liveUpdatePoller = function () {
    var poll = function() {
        alert("Live update poll");
        setTimeout(poll, 10000);
    };
    poll();
}();
