exported.templateLibrary = function () {
    var templateServiceUrl = "/services/templates/template?path=";
    var cache = true;
    var templates = {};

    var init = function(params) {
        if (params.templateServiceUrl != undefined)
            templateServiceUrl = params.templateServiceUrl;
        if (params.cache != undefined)
            cache = params.cache;
    }

    var getTemplate = function (parent, templatePath, onNew, onExisting) {
        var template = templates[templatePath];
        if (template != undefined) {
            if (onExisting != undefined) onExisting(template);
            return;
        }

        var request = {
            isSuccess: function (ajax) { return ajax.status === 200; },
            onSuccess: function (ajax) {
                template = ajax.responseText;
                if (cache) templates[templatePath] = template;
                parent.insertAdjacentHTML("beforeend", template);
                if (onNew != undefined)
                    setTimeout(onNew(template), 10);
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