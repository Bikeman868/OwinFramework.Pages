<div id="app1">
  {{ message }}
</div>

--css

#app1 { color: pink; }

--init

var app1 = new Vue({
  el: '#app1',
  data: {
  message: 'Hello Vue 1!'
  }
})

