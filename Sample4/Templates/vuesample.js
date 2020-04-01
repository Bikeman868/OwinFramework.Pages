new Vue({
    el: "#sample4_vuesample",
    data: {
        paragraph1: /*mustache*/`"{{Sample4.ViewModels.CustomerViewModel:Name}}"`,
        paragraph2: /*html*/`<h3>Contains "quotes"</h3>`
    }
});