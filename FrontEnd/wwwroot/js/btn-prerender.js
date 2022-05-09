console.info("btn-prerender.js loaded");

const curstate = sessionStorage.getItem('btn-prerender:clicked');

console.info(`current state ${curstate ?? "null"}`);

sessionStorage.setItem('btn-prerender:clicked', 'false');

window.onload = function (event) {
    const buttons = document.getElementsByClassName('btn-pr');
    console.info(`${buttons.length}`);

    for (const element of buttons) {
        console.info(`${element.id}`);
        element.addEventListener("click", prclick);
        element.addEventListener("unload", prunload);
    }
};

window.document.onload = function (event) {
    console.info("document onload");
};

function prclick(event) {
    const id = event.srcElement.id;
    const trigger = event.srcElement.getAttribute("data-pr-trigger");
    const trelem = document.getElementById(trigger);

    event.srcElement.hidden = true;
    trelem.hidden = false;
    sessionStorage.setItem('btn-prerender:clicked', 'true');

    console.info(`prerender clicked ${id} trigger ${trigger} found ${trelem.id}`);
}

function prunload(event) {
    const id = event.srcElement.id;

    console.info(`prerender unloaded ${id}`);
}