var scormAPI = null;

function findAPI(win) {
    while (win) {
        if (win.API) return win.API;
        win = win.parent;
    }
    return null;
}

function ScormInitialize() {
    scormAPI = findAPI(window);
    if (scormAPI) {
        scormAPI.LMSInitialize("");
    }
}

function ScormGetValue(name) {
    if (scormAPI)
        return scormAPI.LMSGetValue(name);
    return "";
}

function ScormSetValue(name, value) {
    if (scormAPI)
        scormAPI.LMSSetValue(name, value);
}

function ScormComplete() {

    ScormSetValue("cmi.core.lesson_status", "completed");

    scormAPI.LMSCommit("");
    scormAPI.LMSFinish("");
}
