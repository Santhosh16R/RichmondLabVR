var scormAPI = null;

function findAPI(win)
{
    var attempts = 0;

    while((win.API == null) &&
          (win.parent != null) &&
          (win.parent != win))
    {
        attempts++;
        if(attempts > 10) return null;
        win = win.parent;
    }
    return win.API;
}

function ScormInitialize()
{
    scormAPI = findAPI(window);

    if(!scormAPI)
    {
        console.error("SCORM API NOT FOUND");
        return;
    }

    scormAPI.LMSInitialize("");
}

function ScormSetValue(name,value)
{
    if(scormAPI)
        scormAPI.LMSSetValue(name,value);
}

function ScormCommit()
{
    if(scormAPI)
        scormAPI.LMSCommit("");
}

function ScormComplete()
{
    ScormSetValue("cmi.core.lesson_status","completed");
    ScormCommit();
    scormAPI.LMSFinish("");
}
