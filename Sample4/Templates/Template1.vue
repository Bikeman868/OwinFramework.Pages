<div id="app">
  {{ message }}
</div>

--css

#app { color: pink; }

--init

var app = new Vue({
  el: '#app',
  data: {
  message: 'Hello Vue!'
  }
})

