exported.restModule = function () {
    // You can replace this default implementation with your application
    // specific logic by assiging new values to these variables
    var isSessionExpired = function () { return false; }
    var isSuccess = function () { return true; }
    var onRenewSession = function (ajax) { return false; }
    var onSuccess = function (ajax) { return null; }
    var onFail = function (ajax) { console.log("Failed ajax request: " + ajax); }
    var onDone = function (ajax) { }

    var init = function (params) {
        if (params.isSessionExpired != undefined)
            isSessionExpired = params.isSessionExpired;

        if (params.isSuccess != undefined)
            isSuccess = params.isSuccess;

        if (params.onRenewSession != undefined)
            onRenewSession = params.onRenewSession;

        if (params.onSuccess != undefined)
            onSuccess = params.onSuccess;

        if (params.onFail != undefined)
            onFail = params.onFail;

        if (params.onDone != undefined)
            onDone = params.onDone;
    }

    // The passed in request object can have the following properties:
    // method = GET, POST, PUT or DELETE
    // url = The URL of the endpoint to call
    // contentType = Mime type of the content in the body of your POST or PUT
    // accept = The mime type of the data you want the service to return
    // responseType - The Javascript type to return. Can be "text", "json", "document", "blob", "arrayBuffer"
    var buildAjax = function (request) {
        if (request.accept == undefined) request.accept = "application/json";
        if (request.isSessionExpired == undefined) request.isSessionExpired = isSessionExpired;
        if (request.isSuccess == undefined) request.isSuccess = isSuccess;
        if (request.onRenewSession == undefined) request.onRenewSession = onRenewSession;
        if (request.onSuccess == undefined) request.onSuccess = onSuccess;
        if (request.onFail == undefined) request.onFail = onFail;
        if (request.onDone == undefined) request.onDone = onDone;

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
            ajax.setRequestHeader("Accept", request.accept);
        else
            ajax.setRequestHeader("Accept", "application/json");

        if (request.headers != undefined) {
            for (var i = 0; i < request.headers.length; i++) {
                ajax.setRequestHeader(request.headers[i].name, request.headers[i].value);
            }
        }

        return ajax;
    }

    // The ajax parameter must be an instance of XMLHttpRequest.
    // The request parameter is an object which must have these required properties:
    //   url = The url of the service to call.
    //   method = The http method. GET, PUT, POST etc.
    // The request object can optionally have these additional properties:
    //   body = The content to send. If the contentType is json then
    //       this is assumed to be a JavaScript object and gets stringified
    //       otherwise the body should be a string.
    //   isSessionExpired = a function that checks to see if the response
    //       indicates that the user's session has expired.
    //   onRenewSession = a function that renews the user's session
    //   isSuccess = a function that tests the response to see if the request
    //       was successful.
    //   onSuccess = a function that is called when the request succeeds
    //   onFail = a function to call is the request fails
    var sendAjax = function (ajax, request) {
        ajax.onload = function () {
            if (ajax.readyState === ajax.DONE) {
                if (request.isSessionExpired(ajax)) {
                    if (request.onRenewSession(ajax)) {
                        var retry = Object.assign({}, request);
                        retry.isSessionExpired = function () { return false; }
                        retry.onRenewSession = function() { return false; }
                        ajax = buildAjax(retry);
                        sendAjax(ajax, retry);
                    }
                } else {
                    if (request.isSuccess(ajax)) {
                        request.onSuccess(ajax);
                    } else {
                        request.onFail(ajax);
                    }
                }
                request.onDone(ajax);
            }
        };

        ajax.onerror = function () {
            request.onFail(ajax);
            request.onDone(ajax);
        }

        if (request.contentType === "application/json") {
            if (typeof(request.body) !== "string") {
                if (request.body == undefined)
                    request.body = JSON.stringify(null);
                else
                    request.body = JSON.stringify(request.body);
            }
        } else {
            if (request.body == undefined) request.body = "";
        }

        try {
            ajax.send(request.body);
        } catch (e) {
            console.log(e);
            request.onFail(ajax);
            request.onDone(ajax);
        }
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // onSuccess = a function to call if the request succeeds
    var getJson = function (request) {
        request.method = "GET";
        request.responsetype = "json";
        request.accept = "application/json";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // onSuccess = a function to call if the request succeeds
    var getDocument = function (request) {
        request.method = "GET";
        request.responsetype = "document";
        request.accept = "text/html";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // onSuccess = a function to call if the request succeeds
    var getXml = function (request) {
        request.method = "GET";
        request.responsetype = "document";
        request.accept = "application/xml";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // body = The JSON object to send with the request
    var putJson = function (request) {
        request.method = "PUT";
        request.responsetype = "json";
        request.accept = "application/json";
        request.contentType = "application/json";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // body = The form fields to send with the request in the format
    //        name=value&name=value&name=value
    var putForm = function (request) {
        request.method = "PUT";
        request.responsetype = "json";
        request.accept = "application/json";
        request.contentType = "application/x-www-form-urlencoded";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // body = The JSON object to send with the request
    var postJson = function (request) {
        request.method = "POST";
        request.responsetype = "json";
        request.accept = "application/json";
        request.contentType = "application/json";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    // body = The form fields to send with the request in the format
    //        name=value&name=value&name=value
    var postForm = function (request) {
        request.method = "POST";
        request.responsetype = "json";
        request.accept = "application/json";
        request.contentType = "application/x-www-form-urlencoded";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    // The passed in request object must have the following properties:
    // url = The URL of the endpoint to call
    var sendDelete = function (request) {
        request.method = "DELETE";
        var ajax = buildAjax(request);

        sendAjax(ajax, request);
    }

    return {
        init: init, // Optional initialization sets various hooks
        buildAjax: buildAjax, // Low level method not required in most applications
        sendAjax: sendAjax, // Low level method not required in most applications
        getJson: getJson, // Performs a GET request and returns the results as a JavaScript object
        getDocument: getDocument, // Performs a GET request and returns the results as a Document object
        getXml: getXml, // Performs a GET request and returns the results as a XMLDocument object
        putJson: putJson, // Performs a PUT request passing a JavaScript object
        postJson: postJson, // Performs a PUT request passing a JavaScript object
        putForm: putForm, // Performs a PUT request passing a url encoded form
        postForm: postForm, // Performs a PUT request passing a url encoded form
        sendDelete: sendDelete // Performs a DELETE request
    }
}();
