exported.templateLibrary = function () {
    var templateServiceUrl = "/services/templates/template?path=";
    var templates = {};

    var init = function(params) {
        if (params.templateServiceUrl != undefined)
            templateServiceUrl = params.templateServiceUrl;
    }

    var getTemplate = function (parent, templatePath, onExisting, onNew) {
        var template = templates[templatePath];
        if (template != undefined) {
            if (onExisting != undefined) onExisting(template);
            return;
        }

        var request = {
            isSuccess: function (ajax) { return ajax.status === 200; },
            onSuccess: function (ajax) {
                template = ajax.responseText;
                templates[templatePath] = template;
                parent.insertAdjacentHTML("beforeend", template);
                if (onNew != undefined)
                    setTimeout(onNew(template), 100);
            },
            url: templateServiceUrl + encodeURIComponent(templatePath),
        };
        ns.ajax.restModule.getHtml(request);
    }

    return {
        init: init,
        getTemplate: getTemplate
    };
}();