<div id="login">
    <button>Login</button>
    <div class="popup form">
        <p><a>Login</a> or <a>signup</a></p>
        <div>
            <label for="login-email">Email</label>
            <input id="login-email" type="text" />
        </div>
        <div v-if="showPassword1">
            <label for="login-password">Password</label>
            <input id="login-password" type="password" />
        </div>
        <div v-if="showPassword2">
            <label for="login-confirm-password">Confirm password</label>
            <input id="login-confirm-password" type="password" />
        </div>
        <p><a>Password reset</a></p>
    </div>
</div>
