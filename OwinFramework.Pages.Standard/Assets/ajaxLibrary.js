exported.restModule = function () {
    // The passed in request object can have the following properties:
    // method = GET, POST, PUT or DELETE
    // url = The URL of the endpoint to call
    // contentType = Mime type of the content in the body of your POST or PUT
    // accept = The Accept header value to send with the request
    // body = The content to send. If 
    var buildAjax = function (request) {
        if (request.accept) request.accept = "application/json";

        var ajax = new XMLHttpRequest();
        ajax.open(request.method, request.url, true);
        ajax.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        ajax.withCredentials = true;

        if (request.contentType != undefined)
            ajax.setRequestHeader("Content-Type", request.contentType);

        if (request.responseType != undefined)
            ajax.responseType = request.responseType;
        else
            ajax.responseType = "json";

        if (request.accept != undefined)
            ajax.setRequestHeader("Accept", accept);
        else
            ajax.setRequestHeader("Accept", "application/json");

        return ajax;
    }

    // The ajax parameter must be an instance of XMLHttpRequest.
    // The request parameter is an object with these parameters:
    // isSuccess = a function that takes the XMLHttpRequest object 
    //             and returns true if the request is considered 
    //             successful.
    var sendAjax = function (ajax, request) {
        ajax.onload = function () {
            if (ajax.readyState === ajax.DONE) {
                if (request.isSessionExpired != undefined && request.isSessionExpired(ajax)) {
                    if (request.onRenewSession != undefined) request.onRenewSession(ajax);
                    ajax = buildAjax(request);
                    sendAjax(ajax, request);
                } else {
                    if (request.isSuccess == undefined || request.isSuccess(ajax)) {
                        if (request.onSuccess != undefined) request.onSuccess(ajax);
                    } else {
                        if (request.onFail != undefined) request.onFail(ajax);
                    }
                }
            if (request.onCompletion != undefined) request.onCompletion(ajax);
            }
        };

        ajax.onerror = function () {
            if (request.onFail != undefined) request.onFail(ajax);
            if (request.onCompletion != undefined) request.onCompletion(ajax);
        }

        if (request.contentType === "application/json") {
            if (request.body == undefined)
                request.body = JSON.stringify(null);
            else
                request.body = JSON.stringify(body);
        } else {
            if (request.body == undefined) request.body = "";
        }

        try {
            ajax.send(request.body);
        } catch (e) {
            console.log(e);
        }
    }

    // The passed in request object can have the following properties:
    // url = The URL of the endpoint to call
    var sendGet = function (request) {
        request.method = "GET";
        request.responsetype = "json";
        var ajax = buildAjax(request);

        request.onSuccess = function(ajax) {

        };
        sendAjax(ajax, request);

        alert("GET");
    }

    var sendPut = function () {
        alert("PUT");
    }

    var sendPost = function () {
        alert("POST");
    }

    var sendDelete = function () {
        alert("DELETE");
    }

    return {
        sendGet: sendGet,
        sendPut: sendPut,
        sendPost: sendPost,
        sendDelete: sendDelete
    }
}();
