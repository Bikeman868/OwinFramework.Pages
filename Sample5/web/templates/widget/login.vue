<div v-cloak id="login">
    <button v-if="!isLoggedIn" type="button" v-on:click="show">Login</button>
    <button v-if="isLoggedIn" type="button" v-on:click="logout">Logout</button>
    <div v-if="showPopup" class="sample5_popup">
        <sample5-tabs :selected="selectedTab">
            <sample5-tab name="Login">
                <form id="signin-form" method="POST" action="/formid/signin">
                    <div class="sample5_field">
                        <label>Email</label>
                        <input v-model="email" type="email" name="email" placeholder="me@provider.com" />
                    </div>
                    <div class="sample5_field">
                        <label>Password</label>
                        <input v-model="password" type="password" name="password" />
                    </div>
                    <div class="sample5_field">
                        <label>Remember me</label>
                        <input type="checkbox" name="rememberMe" value="true" checked="checked" />
                    </div>
                    <div class="sample5_button_bar">
                        <button type="button" v-on:click="login">Login</button>
                        <button type="button" v-on:click="cancel">Cancel</button>
                    </div>
                </form>
            </sample5-tab>
            <sample5-tab name="Register">
                <form id="signup-form" method="POST" action="/formid/signup">
                    <div class="sample5_field">
                        <label>Email</label>
                        <input v-model="email" type="email" name="email" placeholder="me@provider.com" />
                    </div>
                    <div class="sample5_field">
                        <label>Password</label>
                        <input v-model="password" type="password" name="password" />
                    </div>
                    <div class="sample5_field">
                        <label>Confirm password</label>
                        <input v-model="confirmPassword" type="password" />
                    </div>
                    <div class="sample5_field">
                        <label>Remember me</label>
                        <input type="checkbox" name="rememberMe" value="true" checked="checked" />
                    </div>
                    <div class="sample5_button_bar">
                        <button type="button" v-on:click="register">Register</button>
                        <button type="button" v-on:click="cancel">Cancel</button>
                    </div>
                </form>
            </sample5-tab>
            <sample5-tab name="Reset password">
                <form id="reset-password-form" method="POST" action="/formid/sendpasswordreset">
                    <div class="sample5_field">
                        <label>Email</label>
                        <input v-model="email" type="email" placeholder="me@provider.com" />
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
