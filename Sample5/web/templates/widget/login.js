new Vue({
    el: "#login",
    data: {
        showPopup: false,
        isLoggedIn: false,
        email: "",
        password: "",
        confirmPassword: "",
        error: ""
    },
    watch: {
        email: function () { this.error = undefined; },
        password: function () { this.error = undefined; },
        confirmPassword: function () { this.error = undefined; },
    },
    methods: {
        show: function () {
            document.getElementById("pageMask").style.visibility = "visible";
            this.showPopup = true;
        },
        cancel: function () {
            this.error = undefined;
            this.password = undefined;
            this.confirmPassword = undefined;
            this.showPopup = false;
            document.getElementById("pageMask").style.visibility = "hidden";
        },
        login: function () {
            if (this.isValidEmail()) {
                this.error = "Login is not implemented yet";
            }
            if (!this.error) this.cancel();
        },
        register: function () {
            if (this.isValidEmail() && this.isValidPassword()) {
                if (this.password != this.confirmPassword)
                    this.error = "Your password and confirmation password do not match, please try again."
                else {
                    this.error = "Registration is not implemented yet";
                }
            }
            if (!this.error) this.cancel();
        },
        reset: function () {
            if (this.isValidEmail()) {
                this.error = "Resetting your password not implemented yet";
            }
            if (!this.error) this.cancel();
        },
        isValidEmail() {
            var isValid = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(this.email);
            if (!isValid) this.error = "Please enter a valid email address";
            return isValid;
        },
        isValidPassword() {
            var isValid = this.password && this.password.length > 7;
            if (!isValid) this.error = "Please choose a password that is at least 8 characters. Thanks you.";
            return isValid;
        }
    }
})