exported.templateLibrary = function () {
    var templates = {};

    var getTemplate = function (parent, templatePath, onSuccess) {
        var template = templates[templatePath];
        if (template == undefined) {
            exported.templatesService.getTemplate(
                {
                    path: templatePath,
                    accept: "text/html",
                    responseType: "text"
                },
                function (html) {
                    template = html;
                    templates[templatePath] = template;
                    parent.insertAdjacentHTML('beforeend', html);
                    if (onSuccess != undefined) onSuccess();
                });
        }
        return template;
    }

    return {
        getTemplate: getTemplate
    };
}();