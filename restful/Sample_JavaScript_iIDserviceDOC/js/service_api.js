// === CONSTANTS
const API_KEY_NAME = "ApiKey";
const API_KEY_KEY  = "hL4bA4nB4yI0vI0fC8fH7eT6";

const PROTOCOL   = "http";
//const IP_ADDRESS = "172.20.10.77";
const IP_ADDRESS = "localhost";
const PORT       = "19813";

function GETversion() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/Version")
}

// === MAIN FUNCTIONS === 
function GETisConnected() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/IsConnected")
}
function POSTinitialize() {
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/Initialize")
}
function POSTterminate() {
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/Terminate")
}
function GETreaderInfo() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/ReaderInfo")
}
function GETidentify() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/Identify")
}
function GETreadBytes() {
    //Get parameters from HTML
    let TagID      = encodeURIComponent(String(document.getElementById("tagIDread").value));
    let FromByte      = parseInt(document.getElementById("readFrom").value);
    let LengthBytes = parseInt(document.getElementById("readLength").value);
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/ReadBytes?&TagID="+ TagID +'&PageNum=3&FromByte='+ FromByte +'&LengthBytes=' + LengthBytes);
}
function POSTwriteBytes() {
    //Get parameters from HTML
    let TagID      = encodeURIComponent(String(document.getElementById("tagIDwrite").value));
    let FromByte      = parseInt(document.getElementById("writeFrom").value);
    let data      = encodeURIComponent(String(document.getElementById("data").value));
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/WriteBytes?&TagID="+ TagID +'&PageNum=3&FromByte='+ FromByte +'&Data=' + data);
}
function GETsensorData() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/telidtransponder/GetSensorData")
}

/* === MAIN SSE FUNCTIONS === */
function SSEstartIdentifyCylce() {
    //Get parameters from HTML
    let interval      = parseInt(document.getElementById("timeScan").value);
    let numTries      = parseInt(document.getElementById("numScan").value);
    executeStartSSE(OnOpenIdentify, OnMessageIdentify, PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/IdentifyCycle?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&IntervalMs="+ interval + '&NumTries=' + numTries);
}
function SSEstartGetSensorCylce() {
    //Get parameters from HTML
    let interval      = parseInt(document.getElementById("timeScan").value);
    let numTries      = parseInt(document.getElementById("numScan").value);
    executeStartSSE(OnOpenGetSensor, OnMessageGetSensor, PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/telidtransponder/GetSensorDataCycle?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&IntervalMs="+ interval + '&NumTries=' + numTries);
}


/* === INTERFACE FUNCTIONS === */
function GETpossibleSettings() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/interface/PossibleSettings");
}
function GETcurrentSettings() {
    executeApiCall("GET", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/interface/CurrentSettings");
}
function POSTnewConnectionSettings() {
    //Get parameters from HTML
    let PortPath      = encodeURIComponent(String(document.getElementById("portPath").value));
    let PortType      = parseInt(document.getElementById("portType").value);
    let InterfaceType = parseInt(document.getElementById("interfaceType").value);
    executeApiCall("POST", PROTOCOL + "://" + IP_ADDRESS + ":" + PORT + "/api/iidservice/doc/interface/CurrentSettings?&PortPath="+ PortPath +'&PortType='+ PortType +'&InterfaceType=' + InterfaceType);
}



/* === HELPER FUNCTIONS === */
function executeApiCall(method, apiPath) {
    let CONN_INFO = new Object();
    CONN_INFO.METHOD = method;
    CONN_INFO.API    = apiPath;
    CONN_INFO.ASYNC  = true;

    let xhttp = createXMLHttpRequest(CONN_INFO);
    if (!xhttp) {
        alert("XMLHttpRequest not available");
        return;
    }
    xhttp.onreadystatechange = function () {
        if (xhttp.readyState === 4) { //Request ended
            try {
                let msg = JSON.parse(xhttp.responseText);
                console.log(msg);
                document.getElementById("CodeResult").innerHTML = '<b>Statuscode:&nbsp;</b>' + xhttp.status;
                document.getElementById("MsgResult").textContent = JSON.stringify(msg, undefined, 2);
            } catch (error) {
                console.error("ERROR: JSON parse");
                document.getElementById("CodeResult").innerHTML = '<b>Statuscode:&nbsp;</b> ERROR';
            }
            stopTimer();
        }
    };
    xhttp.onerror = function () {
        stopTimer();
        console.error('Request failed.');
    };
    xhttp.ontimeout = function () {
        stopTimer();
        console.error('Request timeout.', xhttp.responseURL);
    };

    document.getElementById("MsgResult").textContent = "";
    let pos = document.getElementById("ResponseTime");
    startTimer(pos);

    xhttp.send();
}

function createXMLHttpRequest(conn_info) {
    let xhttp = null;
    if (window.XMLHttpRequest) {
        try {
            xhttp = new XMLHttpRequest();
            xhttp.open(conn_info.METHOD, conn_info.API, conn_info.ASYNC);
            xhttp.setRequestHeader('Access-Control-Allow-Origin', '*');
            xhttp.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhttp.setRequestHeader(API_KEY_NAME, API_KEY_KEY);
            xhttp.timeout = 10000; // 10 seconds
        } catch (e) {
            console.log("Error XMLHttpRequest");
            xhttp = null;
        }
    }
    return xhttp;
}

let sseSourceEvent = null;
let sseMsgCounter = 0;
function executeStartSSE(onOpen, onMsg, apiPath) {

    if (sseSourceEvent != null) {
        return;
    }

    if (window.EventSource) {

        sseMsgCounter = 0;

        sseSourceEvent = new EventSource(apiPath);

        sseSourceEvent.addEventListener('open', onOpen, false);
        sseSourceEvent.addEventListener('error', OnError, false);
        sseSourceEvent.addEventListener('message', onMsg, false);
        sseSourceEvent.addEventListener('completed', OnCompleted, false);

        document.getElementById("CodeResult").innerHTML = '';
        document.getElementById("MsgResult").textContent = '';
        document.getElementById("ResponseTime").innerHTML = '';
    }
    else {
        alert("SSE not possible :(");
    }

    function OnError(e) {
        if (e.readyState == EventSource.CLOSED) {
            document.getElementById("CodeResult").innerHTML = '<b>SSE closed</b>';
            console.log("Connection was closed.");
        } else {
            console.error("EventSource ERROR");
            console.log(e);
        }
    }
    function OnCompleted(e) {
        console.log("Completed. Close connection");
        SSEclose();
    }
};

function OnOpenIdentify(e) {
    document.getElementById("CodeResult").innerHTML = '<b>SSE open</b>';
    console.log("EventSource Connection established");
}
function OnMessageIdentify(e) {
    sseMsgCounter = sseMsgCounter + 1;
    try {
        try {
            console.log(e);
            let msg = JSON.parse(e.data);
            writeJsonToOutputScreen(msg);
        } catch (error) {
            console.log("Error parse Temperature")
        }
    } catch (error) {
        console.error("ERROR: JSON parse");
    }
}
function OnOpenGetSensor(e) {
    document.getElementById("CodeResult").innerHTML = '<b>SSE open</b>';
    console.log("EventSource Connection established");
    ChartCleanDataset();
}
function OnMessageGetSensor(e) {
    sseMsgCounter = sseMsgCounter + 1;
    try {
        try {
            console.log(e);
            let msg = JSON.parse(e.data);
            writeJsonToOutputScreen(msg);
            console.log(msg);
            if (config.data.datasets[0].label != msg.description){
                config.data.datasets[0].label = msg.description;
            }
            if (msg.measurements.length > 0){
                let meas0 = msg.measurements[0];
                if (meas0.values.length > 0){
                    let value0 = meas0.values[0];
                    if (config.options.scales.yAxes[0].scaleLabel.labelString != (value0.symbol + value0.unit)){
                        config.options.scales.yAxes[0].scaleLabel.labelString = (value0.symbol + value0.unit)
                    }
                    ChartAddData(meas0.timestamp, value0.magnitude);
                }
            }
        } catch (error) {
            console.log(error)
            console.log("Error parse measurement")
        }
    } catch (error) {
        console.error("ERROR: JSON parse");
    }
}

function SSEstop() {
    SSEclose();
};

window.onclose = function () {
    console.log("EventSource close");
    SSEclose();
};

function SSEclose() {
    if (sseSourceEvent != null) {
        sseSourceEvent.close();
        document.getElementById("CodeResult").innerHTML = '<b>SSE closed</b>';

        setTimeout(() => {
            sseSourceEvent = null;
        }, 500);
    }
}

function writeJsonToOutputScreen(msg) {
    let textCounter = ("0000" + sseMsgCounter).slice(-4);
    document.getElementById("MsgResult").textContent = JSON.stringify(msg, undefined, 2);
    document.getElementById("MsgResult").textContent += "\n\n" + textCounter + " scans\n\n";
}

function ChartAddData(timestampString, magnitude) {
    if (config.data.datasets.length > 0) {

        config.data.labels.push(timestampString.substring(11,23));

        config.data.datasets.forEach(function(dataset) {
            dataset.data.push(magnitude);
        });
        window.myLine.update();
    }
}

function ChartCleanDataset() {
    config.data.datasets.forEach(function(dataset) {
        dataset.data.splice(0,dataset.data.length)
    });

    config.data.labels.splice(0,config.data.labels.length)
    
    window.myLine.update();
}   