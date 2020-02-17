Vue.component("tab", {
    template:
/*html*/`
<div v-show="selected">
  <slot></slot>
</div>`,
    props: {
        name: { required: true },
        selected: { default: false }
    },
});
