// Demonstrate using our own script.

// Look up a control by ID and set its value.
function myScript(ctrl, str) {
    var control = document.getElementById(ctrl);
    if (control != null)
        control.value = str;
    else
        alert("Couldn't find: " + ctrl);
}
