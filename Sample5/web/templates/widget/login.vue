<div v-cloak id="login">
    <div class="sample5_username">{{email}}</div>
    <button v-if="!isLoggedIn" type="button" v-on:click="show">Login</button>
    <button v-if="isLoggedIn" type="button" v-on:click="logout">Logout</button>
    <form id="logout-form" method="post" action="/account/logout"></form>
    <div v-if="showPopup" class="sample5_popup">
        <sample5-tabs :selected="selectedTab">
            <sample5-tab name="Login">
                <form id="login-form" method="post" action="/account/login">
                    <div class="sample5_field">
                        <label for="login-login-email" class="email">Email</label>
                        <input id="login-login-email" v-model="email" type="email" name="email" placeholder="me@provider.com" />
                    </div>
                    <div class="sample5_field">
                        <label for="login-login-password" class="password">Password</label>
                        <input id="login-login-password" v-model="password" type="password" name="password" />
                    </div>
                    <div class="sample5_field">
                        <label for="login-login-remember-me" class="checkbox">Remember me</label>
                        <input id="login-login-remember-me" type="checkbox" name="rememberMe" value="true" checked="checked" />
                    </div>
                    <div class="sample5_button_bar">
                        <button type="button" v-on:click="login">Login</button>
                        <button type="button" v-on:click="cancel">Cancel</button>
                    </div>
                </form>
            </sample5-tab>
            <sample5-tab name="Register">
                <form id="register-form" method="post" action="/account/register">
                    <div class="sample5_field">
                        <label for="login-register-email" class="email">Email</label>
                        <input id="login-register-email" v-model="email" type="email" name="email" placeholder="me@provider.com" />
                    </div>
                    <div class="sample5_field">
                        <label for="login-register-password" class="password">Password</label>
                        <input id="login-register-password" v-model="password" type="password" name="password" />
                    </div>
                    <div class="sample5_field">
                        <label for="login-register-confirm-password" class="password">Confirm password</label>
                        <input id="login-register-confirm-password" v-model="confirmPassword" type="password" />
                    </div>
                    <div class="sample5_field">
                        <label for="login-register-remember-me" class="checkbox">Remember me</label>
                        <input id="login-register-remember-me" type="checkbox" name="rememberMe" value="true" checked="checked" />
                    </div>
                    <div class="sample5_button_bar">
                        <button type="button" v-on:click="register">Register</button>
                        <button type="button" v-on:click="cancel">Cancel</button>
                    </div>
                </form>
            </sample5-tab>
            <sample5-tab name="Reset password">
                <form id="reset-password-form" method="post" action="/account/sendpasswordreset">
                    <div class="sample5_field">
                        <label for="login-reset-email" class="email">Email</label>
                        <input id="login-reset-email" v-model="email" type="email" placeholder="me@provider.com" />
                    </div>
                    <div class="sample5_button_bar">
                        <button type="button" v-on:click="reset">Send</button>
                        <button type="button" v-on:click="cancel">Cancel</button>
                    </div>
                </form>
            </sample5-tab>
            <div v-if="error" class="sample5_error">{{error}}</div>
        </sample5-tabs>
    </div>
</div>
