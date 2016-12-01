// A place to put PortableRazor-related scripts.

// Wrap Ajax.

/*
// JavaScriptInterop only supported on Android.
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
*/

// Warning: This implementation of Ajax does not support overlapping calls.

// Save completion callback.
var ajaxComplete;

function PortableAjax(obj) {
    try {
        var jsonStr = JSON.stringify(obj);
        var uri = "ajax:" + jsonStr;
        ajaxComplete = obj.complete;
        window.location = uri;
    }
    catch (e) {
        alert("PortableAjax exception: " + e);
    }
}

function PortableAjaxReturn(jsonStr, jsonReturn) {
    var returnValue = "";
    if (jsonReturn != null) {
        try {
            jsonReturn = jsonReturn.replace(/\r/g, "\\r");
            jsonReturn = jsonReturn.replace(/\n/g, "\\n");
            returnValue = JSON.parse(jsonReturn);
        }
        catch (e) {
            alert("PortableAjaxReturn exception: " + e + "\njsonReturn: " + jsonReturn);
        }
        if (ajaxComplete != null)
            ajaxComplete(returnValue);
    }
    else {
        var obj = null;
        try {
            jsonStr = jsonStr.replace(/\r/g, "\\r");
            jsonStr = jsonStr.replace(/\n/g, "\\n");
            obj = JSON.parse(jsonStr);
        }
        catch (e) {
            alert("PortableAjaxReturn exception: " + e + "\njsonStr: " + jsonStr);
        }
        alert("Ajax call failed: " + obj.url);
    }
}

function PortableNativeCall(functionName, argumentsArray, returnCallback) {
    //alert("PortableNativeCall: functionName: " + functionName + "\nargumentsArray: " + argumentsArray + "\nreturnFunction: " + returnFunction);
    try {
        var arguments = "";
        if ((argumentsArray != null) && (argumentsArray != [])) {
            arguments += "?";
            var count = argumentsArray.length;
            var index;
            for (index = 0; index < count; index += 2) {
                if (index != 0)
                    arguments += "&";
                arguments += argumentsArray[index].toString() + "=" + argumentsArray[index + 1].toString();
            }
        }
        if (returnCallback != null) {
            if (arguments == "")
                arguments += "?";
            else
                arguments += "&";
            arguments += "returnCallback=" + returnCallback;
        }
        var uri = "call:" + functionName + arguments;
        window.location = uri;
    }
    catch (e) {
        alert("PortableNativeCall exception: " + e);
    }
}