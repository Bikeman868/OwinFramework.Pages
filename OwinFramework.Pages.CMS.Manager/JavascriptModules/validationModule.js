var validation = function() {
    var namePattern = new RegExp("^[a-z][0-9a-z_]+$");
    var urlPattern = new RegExp("^http(s)?://[0-9a-z_-]+\.[0-9a-z_.-]+/$");
    var idPattern = new RegExp("^[0-9]+$");

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
    }

    return {
        name: name,
        displayName: displayName,
        url: url,
        id: id,

        namePattern: namePattern,
        displayNamePattern: "^...+$",
        urlPattern: urlPattern,
        idPattern: idPattern
    }
}();

exported.validation = validation;
