function collapse(content) {
    content.style.maxHeight = 0;
}

function expand(content) {
    content.style.maxHeight = "none";
}

function setCollapsible(collapsible) {
    var content = collapsible.nextElementSibling;
    if (collapsible.classList.contains("active")) {
        collapsible.innerHTML = "-";
        expand(content);
    }
    else {
        collapsible.innerHTML = "+";
        collapse(content);
    }
}
