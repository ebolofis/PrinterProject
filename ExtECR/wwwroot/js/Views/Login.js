function loginUser(element) {
    console.log(element);
    var inputElement = document.getElementById("passwordInput");
    console.log(inputElement);
}

$(function () {
    $("#formContainer").dxForm({
        formData: {
            firstName: "John",
            lastName: "Heart"
        },
        items: [{
            dataField: "firstName",
            validationRules: [{
                type: "required",
                message: "First Name is required"
            }, {
                type: "pattern",
                pattern: "^[a-zA-Z]+$",
                message: "The name should not contain digits"
            }]
        },
            {
                dataField: "lastName",
                validationRules: [{
                    type: "required",
                    message: "Last Name is required"
                }, {
                    type: "pattern",
                    pattern: "^[a-zA-Z]+$",
                    message: "The name should not contain digits"
                }]
            }
        ]
    });
});