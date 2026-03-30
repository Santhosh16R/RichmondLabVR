var scormAPI = null;

/* ==============================
   FIND SCORM API
=================================*/
function findAPI(win)
{
    var attempts = 0;

    while ((win.API == null) &&
           (win.parent != null) &&
           (win.parent != win))
    {
        attempts++;

        if (attempts > 10)
        {
            console.error("SCORM API search exceeded limit");
            return null;
        }

        win = win.parent;
    }

    return win.API;
}

/* ==============================
   INITIALIZE SCORM
=================================*/
function ScormInitialize()
{
    scormAPI = findAPI(window);

    if (!scormAPI)
    {
        console.error("SCORM API NOT FOUND");
        return false;
    }

    var result = scormAPI.LMSInitialize("");

    console.log("SCORM Initialized:", result);

    return true;
}

/* ==============================
   GET VALUE
=================================*/
function ScormGetValue(name)
{
    if (!scormAPI) return "";

    var value = scormAPI.LMSGetValue(name);
    console.log("GET:", name, value);

    return value;
}

/* ==============================
   SET VALUE
=================================*/
function ScormSetValue(name, value)
{
    if (!scormAPI) return;

    console.log("SET:", name, value);
    scormAPI.LMSSetValue(name, value);
}

/* ==============================
   COMMIT
=================================*/
function ScormCommit()
{
    if (!scormAPI) return;

    scormAPI.LMSCommit("");
}

/* ==============================
   COMPLETE COURSE
=================================*/
function ScormComplete()
{
    if (!scormAPI)
    {
        console.error("SCORM NOT INITIALIZED");
        return;
    }

    ScormSetValue("cmi.core.lesson_status", "completed");

    ScormCommit();

    scormAPI.LMSFinish("");

    console.log("SCORM Completed");
}
