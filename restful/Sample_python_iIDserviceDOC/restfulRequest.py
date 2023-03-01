import requests
from sseclient import SSEClient

def PerformGetRequest(url):
    headers = { "ApiKey": "hL4bA4nB4yI0vI0fC8fH7eT6" }
    try:
        return requests.get(url, headers = headers)
    except Exception:
        print(" <Exception calling GET on " + url + ">")
        return None

def PerformPostRequest(url):
    headers = { "ApiKey": "hL4bA4nB4yI0vI0fC8fH7eT6" }
    try:
        return requests.post(url, headers = headers)
    except Exception:
        print(" <Exception calling POST on " + url + ">")
        return None

def PerformGetSSECycle(url, interval, numToPerform):
    url = url + '?&ApiKey=hL4bA4nB4yI0vI0fC8fH7eT6&IntervalMs=' + str(interval) + '&NumTries=' + str(numToPerform)
    messages = SSEClient(url)
    if messages != None:
        for msg in messages:
            # print(msg)
            if msg.event == 'completed':
                break
            elif msg.event == 'message':
                print (msg.data)
