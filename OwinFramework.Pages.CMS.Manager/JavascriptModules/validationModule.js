var validation = function() {
    var namePattern = new RegExp("^[a-z][0-9a-z_]+$");
    var displayNamePattern = new RegExp("^...+$");
    var urlPattern = new RegExp("^http(s)?://[0-9a-z_-]+\.[0-9a-z_.-]+/$");
    var idPattern = new RegExp("^[0-9]+$");
    var titlePattern = new RegExp("^..+$");
    var urlPathPattern = new RegExp("^(/[0-9a-z\-_]+)+$");
    var cssPattern = new RegExp("^{( ?[a-z0-9_]+: ?[a-z0-9_\-]+; ?)*}$");
    var permissionPattern = new RegExp("^.+$");
    var pathPattern = new RegExp("^(/[0-9a-z\-_]+)+$");

    var name = function(field, name, errors) {
        if (field == undefined || field.length < 2)
            errors.push("The " + name + " field must contain at least 2 characters");
        else {
            if (!namePattern.test(field))
                errors.push("The " + name + " field can only contain lower-case letters, numbers and underscore. The first character must be a lower-case letter");
        }
        return field;
    }

    var displayName = function (field, name, errors) {
        if (field == undefined || field.length < 3)
            errors.push("The " + name + " field must contain at least 3 characters");
        return field;
    }

    var url = function (field, name, errors) {
        if (field == undefined || field.length < 10)
            errors.push("The " + name + " must contain at least 10 characters");
        else {
            if (!urlPattern.test(field))
                errors.push("The " + name + " is not valid. It should be similar to https://mycompany.com/");
        }
        return field;
    }

    var urlPath = function (field, name, errors) {
        if (field == undefined || field.length < 2)
            errors.push("The " + name + " must contain at least 2 characters");
        else {
            if (!urlPathPattern.test(field))
                errors.push("The " + name + " is not valid. It should be similar to /cart/orderhistory");
        }
        return field;
    }

    var path = function (field, name, errors) {
        if (field == undefined || field.length < 2)
            errors.push("The " + name + " must contain at least 2 characters");
        else {
            if (!pathPattern.test(field))
                errors.push("The " + name + " is not valid. It should be similar to /cart/orderhistory");
        }
        return field;
    }

    var id = function (field, name, errors) {
        if (field == undefined || field.length < 1)
            errors.push("The " + name + " must contain at least 1 number");
        else {
            if (!idPattern.test(field)) {
                errors.push("The " + name + " must be a number");
            } else {
                var id = parseInt(field);
                if (id === NaN) {
                    errors.push("The " + name + " must be a whole number");
                } else {
                    if (id < 1)
                        errors.push("The " + name + " must be greater than zero");
                    else
                        return id;
                }
            }
        }
        return field;
    }

    var title = function (field, name, errors) {
        if (field == undefined || field.length < 2)
            errors.push("The " + name + " field must contain at least 2 characters");
        return field;
    }

    var assetDeployment = function (field, name, errors) {
        if (field === "PerModule" || field === "PerPage" || field === "InPage" || field === "PerWebsite" || field === "Inherit")
            return field;
        errors.push("The " + name + " field must be a valid asset deployment method");
        return field;
    }

    var elementType = function (field, name, errors) {
        if (field === "Region" || field === "Layout" || field === "Component" || field === "Html" || field === "Template")
            return field;
        errors.push("The " + name + " field must be a valid element type");
        return field;
    }

    var css = function (field, name, errors) {
        if (field == undefined || field.length < 4)
            errors.push("The " + name + " must contain at least 4 characters");
        else {
            if (!cssPattern.test(field))
                errors.push("The " + name + " is not valid. It should be similar to {font-family: arial;}");
        }
        return field;
    }

    return {
        name: name,
        displayName: displayName,
        url: url,
        urlPath: urlPath,
        path: path,
        id: id,
        title: title,
        assetDeployment: assetDeployment,
        elementType: elementType,
        css: css,

        namePattern: namePattern,
        displayNamePattern: displayNamePattern,
        urlPattern: urlPattern,
        idPattern: idPattern,
        titlePattern: titlePattern,
        urlPathPattern: urlPathPattern,
        cssPattern: cssPattern,
        permissionPattern: permissionPattern,
        pathPattern: pathPattern
    }
}();

exported.validation = validation;
