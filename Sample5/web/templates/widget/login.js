new Vue({
    el: "#login",
    data: {
        showPopup: false,
        isLoggedIn: false,
        email: "",
        password: "",
        confirmPassword: ""
    },
    methods: {
        show: function () {
            document.getElementById("pageMask").style.visibility = "visible";
            this.showPopup = true;
        },
        cancel: function () {
            this.showPopup = false;
            document.getElementById("pageMask").style.visibility = "hidden";
        },
        login: function () {
            this.cancel();
        },
        register: function () {
            this.cancel();
        },
        reset: function () {
            this.cancel();
        }
    }
})