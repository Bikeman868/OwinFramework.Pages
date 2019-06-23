var validation = function() {
    var namePattern = new RegExp("^[a-z][0-9a-z_]+$");

    var name = function(field, errors) {
        if (field == undefined || field.length < 2)
            errors.push("The name field must contain at least 2 characters");
        else {
            if (!namePattern.test(field))
                errors.push("The name field can only contain lower-case letters, numbers and underscore. The first character must be a lower-case letter");
        }
    }

    var displayName = function (field, errors) {
        if (field == undefined || field.length < 3)
            errors.push("The display name field must contain at least 3 characters");
    }

    return {
        name: name,
        displayName: displayName,

        namePattern: namePattern,
        displayNamePattern: "^...+$"
    }
}();

exported.validation = validation;
