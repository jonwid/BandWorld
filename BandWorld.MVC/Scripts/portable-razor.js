// A place to put PortableRazor-related scripts.

// Wrap Ajax.
function PortableAjax(obj) {
    var jsonStr = JSON.stringify(obj);
    var jsonReturn = JavaScriptInterop.Ajax(jsonStr);
    if (jsonReturn != null) {
        var returnValue = JSON.parse(jsonReturn);
        if (obj.complete != null)
            obj.complete(returnValue);
    }
    else
        alert("Ajax call failed: " + obj.url);
}
