new Vue({
    el: "#login",
    data: {
        isLoggedIn: false,
        email: "",
        isEmailVerified: false,
        showPopup: false,
        password: "",
        confirmPassword: "",
        error: "",
        selectedTab: ""
    },
    watch: {
        email: function () { this.error = undefined; },
        password: function () { this.error = undefined; },
        confirmPassword: function () { this.error = undefined; },
    },
    mounted: function () {
        this.selectedTab = "Login";
    },
    methods: {
        show: function () {
            document.getElementById("pageMask").style.visibility = "visible";
            this.showPopup = true;
        },
        hide: function () {
            this.showPopup = false;
            document.getElementById("pageMask").style.visibility = "hidden";
        },
        cancel: function () {
            this.error = undefined;
            this.password = undefined;
            this.confirmPassword = undefined;
            this.hide();
        },
        login: function () {
            if (this.isValidEmail()) {
                document.forms["login-form"].submit();
            }
            this.showPopup = false;
        },
        register: function () {
            if (this.isValidEmail() && this.isValidPassword()) {
                if (this.password != this.confirmPassword)
                    this.error = "Your password and confirmation password do not match, please try again."
                else {
                    this.hide();
                    document.forms["register-form"].submit();
                }
            }
        },
        logout: function () {
            this.hide();
            this.isLoggedIn = false;
            document.forms["logout-form"].submit();
        },
        reset: function () {
            if (this.isValidEmail()) {
                this.hide();
                document.forms["reset-password-form"].submit();
            }
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
}
