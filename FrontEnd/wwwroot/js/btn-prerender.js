console.info("btn-prerender.js: loaded");

const curstate = sessionStorage.getItem('btn-prerender:clicked');

console.info(`btn-prerender.js: current state ${curstate ?? "null"}`);

sessionStorage.setItem('btn-prerender:clicked', 'false');

window.onload = function (event) {
    const buttons = document.getElementsByClassName('btn-prerender');
    for (const element of buttons) {
        element.addEventListener("click", prclick);
        const test_id = element.getAttribute("data-test-id");
        console.info(`btn-prerender.js: listening on ${test_id}`);
    }
};

function prclick(event) {
    console.info(`btn-prerender.js: clicked`);

    const source_element = event.srcElement;
    const trigger_id = source_element.getAttribute("data-prerender-trigger");
    const trigger_element = document.getElementById(trigger_id);

    source_element.hidden = true;
    trigger_element.hidden = false;

    console.info(`btn-prerender.js: triggered #${trigger_id}`);

    sessionStorage.setItem('btn-prerender:clicked', 'true');
}
