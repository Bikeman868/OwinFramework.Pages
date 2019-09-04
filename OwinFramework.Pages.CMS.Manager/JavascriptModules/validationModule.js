exported.buildValidation = function () {
    var namePattern = new RegExp("^[a-z][0-9a-z_]+$");
    var nameRefPattern = new RegExp("^[a-z][0-9a-z_]+(:[a-z][0-9a-z_]+)?$");
    var displayNamePattern = new RegExp("^...+$");
    var urlPattern = new RegExp("^http(s)?://[0-9a-z_-]+\.[0-9a-z_.-]+/$");
    var idPattern = new RegExp("^[0-9]+$");
    var titlePattern = new RegExp("^..+$");
    var urlPathPattern = new RegExp("^(/[0-9a-z\-_]+)+$");
    var stylePattern = new RegExp("^( ?[a-z0-9_\-]+: ?[a-z0-9_\-]+; ?)*$");
    var classesPattern = new RegExp("^[0-9a-z ]*$");
    var permissionPattern = new RegExp("^.+$");
    var pathPattern = new RegExp("^(/[0-9a-z\-_]+)+$");
    var htmlPattern = new RegExp("^<.+>$");
    var tagPattern = new RegExp("^[a-z]+$");

    var name = function (value, fieldName, errors) {
        if (value == undefined || value.length < 2)
            errors.push("The " + fieldName + " field must contain at least 2 characters");
        else {
            if (!namePattern.test(value))
                errors.push("The " + fieldName + " field can only contain lower-case letters, numbers and underscore. The first character must be a lower-case letter");
        }
        return value;
    }

    var nameRef = function (value, fieldName, errors) {
        if (value == undefined || value.length < 2)
            errors.push("The " + fieldName + " field must contain at least 2 characters");
        else {
            if (!nameRefPattern.test(value))
                errors.push("The " + fieldName + " field can only contain lower-case letters, numbers and underscore. The first character must be a lower-case letter. You can slo use a a package name qualifier in front of the name separated with a colon");
        }
        return value;
    }

    var displayName = function (value, fieldName, errors) {
        if (value == undefined || value.length < 3)
            errors.push("The " + fieldName + " field must contain at least 3 characters");
        return value;
    }

    var url = function (value, fieldName, errors) {
        if (value == undefined || value.length < 10)
            errors.push("The " + fieldName + " must contain at least 10 characters");
        else {
            if (!urlPattern.test(value))
                errors.push("The " + fieldName + " is not valid. It should be similar to https://mycompany.com/");
        }
        return value;
    }

    var urlPath = function (value, fieldName, errors) {
        if (value == undefined || value.length < 2)
            errors.push("The " + fieldName + " must contain at least 2 characters");
        else {
            if (!urlPathPattern.test(value))
                errors.push("The " + fieldName + " is not valid. It should be similar to /cart/orderhistory");
        }
        return value;
    }

    var path = function (value, fieldName, errors) {
        if (value == undefined || value.length < 2)
            errors.push("The " + fieldName + " must contain at least 2 characters");
        else {
            if (!pathPattern.test(value))
                errors.push("The " + fieldName + " is not valid. It should be similar to /cart/orderhistory");
        }
        return value;
    }

    var id = function (value, fieldName, errors) {
        if (value == undefined || value.length < 1)
            errors.push("The " + fieldName + " must contain at least 1 number");
        else {
            if (!idPattern.test(value)) {
                errors.push("The " + fieldName + " must be a number");
            } else {
                var id = parseInt(value);
                if (id === NaN) {
                    errors.push("The " + fieldName + " must be a whole number");
                } else {
                    if (id < 1)
                        errors.push("The " + fieldName + " must be greater than zero");
                    else
                        return id;
                }
            }
        }
        return value;
    }

    var title = function (value, fieldName, errors) {
        if (value == undefined || value.length < 2)
            errors.push("The " + fieldName + " field must contain at least 2 characters");
        return value;
    }

    var assetDeployment = function (value, fieldName, errors) {
        if (value === "PerModule" || value === "PerPage" || value === "InPage" || value === "PerWebsite" || value === "Inherit")
            return value;
        errors.push("The " + fieldName + " field must be a valid asset deployment method");
        return value;
    }

    var elementType = function (value, fieldName, errors) {
        if (value === "Region" || value === "Layout" || value === "Component" || value === "Html" || value === "Template")
            return value;
        errors.push("The " + fieldName + " field must be a valid element type");
        return value;
    }

    var style = function (value, fieldName, errors) {
        if (value == undefined || value.length < 4)
            errors.push("The " + fieldName + " must contain at least 4 characters");
        else {
            if (!stylePattern.test(value))
                errors.push("The " + fieldName + " is not valid. It should be similar to 'font-family: arial; font-size: 12px;'");
        }
        return value;
    }

    var classes = function (value, fieldName, errors) {
        if (value == undefined || value.length < 1)
            errors.push("The " + fieldName + " can not be blank");
        else {
            if (!classesPattern.test(value))
                errors.push("The " + fieldName + " is not valid. It should be similar to 'class1 class2'");
        }
        return value;
    }

    var html = function (value, fieldName, errors) {
        if (value == undefined || value.length < 4)
            errors.push("The " + fieldName + " must contain at least 4 characters");
        return value;
    }

    var tag = function (value, fieldName, errors) {
        if (value == undefined || value.length < 1)
            errors.push("The " + fieldName + " field must can not be blank");
        else {
            if (!tagPattern.test(value))
                errors.push("The " + fieldName + " is not valid. It must be all lower case letters");
        }
        return value;
    }

    exported.validation = {
        name: name,
        nameRef: nameRef,
        displayName: displayName,
        url: url,
        urlPath: urlPath,
        path: path,
        id: id,
        title: title,
        assetDeployment: assetDeployment,
        elementType: elementType,
        style: style,
        classes: classes,
        html: html,
        tag: tag,

        namePattern: namePattern,
        nameRefPattern: nameRefPattern,
        displayNamePattern: displayNamePattern,
        urlPattern: urlPattern,
        idPattern: idPattern,
        titlePattern: titlePattern,
        urlPathPattern: urlPathPattern,
        stylePattern: stylePattern,
        classesPattern: classesPattern,
        permissionPattern: permissionPattern,
        pathPattern: pathPattern,
        htmlPattern: htmlPattern,
        tagPattern: tagPattern
    }
};
