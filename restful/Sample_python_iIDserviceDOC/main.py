from ConsoleFlow import PrintMainConsoleFunctions, PrintDocFunctions
from connectionSettingsFunctions import ModifyConnectionSettings
from restfulRequest import PerformGetRequest, PerformPostRequest, PerformGetSSECycle
from Constants import __errorPrintHeaderStr, __hostname
import json
# print(json.dumps(json.loads(answer.text), indent=4))


'''
Little Console Application without GUI
'''
if __name__ == "__main__":
    print("== Sample code for iIDservice ==")
    print("Console application on Python")
    print("--> iIDservice running on: " + __hostname)

    while True:
        PrintMainConsoleFunctions()
        selValue = input() # read console input
        if len(selValue) > 1:
            selValue = selValue(0)
        
        if (selValue == "1"):
            # ConnectionSettings
            print ("\t1 - ConnectionSettings")
            ModifyConnectionSettings(__hostname)
        elif selValue == "2":
            # DOC Functions
            print ("\t2 - DOC functions")
            # Get is connected DOC
            print("Get connection state...")
            answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/IsConnected')
            if answer != None:
                if answer.status_code == 200:
                    print("Connected: " + answer.text)
                    if answer.text == "false":
                        # CONNECT DOC
                        print("Connecting...")
                        answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/doc/Initialize')
                        if answer != None:
                            if answer.status_code == 200:
                                print(answer.text)
                                if answer.text == "false":
                                    continue
                            else:
                                print(__errorPrintHeaderStr + str(answer.status_code))
                                continue
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
                    continue
            
            tagIDhex = None
            while True:
                PrintDocFunctions()
                '''
                    print("1 - GET IsConnected")
                    print("2 - GET ReaderInfo")
                    print("3 - GET Identify")
                    print("4 - POST ReadBytes")
                    print("5 - POST WriteBytes")
                    print("6 - GET GetSensorData")
                    print("7 - SSE IdentifyCycle")
                    print("8 - SSE GetSensorDataCycle")
                    print("A - GET Timeout")
                    print("B - POST Timeout")
                '''
                selValue = input() # read console input2
                if len(selValue) > 1:
                    continue
                    # selValue = selValue(0)
                
                if selValue == "1":
                    print("\t1 - GET IsConnected")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/IsConnected')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "2":
                    print("\t2 - GET ReaderInfo")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/ReaderInfo')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "3":
                    print("\t3 - GET Identify")
                    tagIDhex = None
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/Identify')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                            answerScan = json.loads(answer.text)
                            scanResult = answerScan["scanResult"]
                            if (answerScan["interfaceType"] == 1356):
                                tagIDhex = scanResult["TagID_Hex"]
                            else:
                                firstTag = scanResult[0]
                                uiiInfo = firstTag["TagID"]
                                tagIDhex = uiiInfo["UII"] # TODO implement UHF!
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "4":
                    print("\t4 - GET ReadBytes")
                    if tagIDhex == None:
                        print("Preform Identify first")
                        continue
                    # For demo purposes read first 16 bytes of user memory (Page 3)
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/ReadBytes?&TagID='+tagIDhex+'&PageNum=3&FromByte=0&LengthBytes=16')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "5":
                    print("\t5 - POST WriteBytes")
                    if tagIDhex == None:
                        print("Preform Identify first")
                        continue
                    # For demo purposes read first 16 bytes of user memory (Page 3)
                    answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/doc/WriteBytes?&TagID='+tagIDhex+'&PageNum=3&FromByte=0&Data=30313233')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "6":
                    print("\t6 - GET GetSensorData")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/telidtransponder/GetSensorData')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "7":
                    print("\t7 - SSE IdentifyCycle")
                    answer = PerformGetSSECycle('http://' + __hostname + ':19813/api/iidservice/doc/IdentifyCycle', 1000, 10)
                    if answer != None:
                        if answer.status_code == 200:
                            print("\t\t=> OK")
                        else:
                            print("\t\t=> ERROR")
                            print(answer.text)
                elif selValue == "8":
                    print("\t8 - SSE SensorDataCycle")
                    answer = PerformGetSSECycle('http://' + __hostname + ':19813/api/iidservice/doc/telidtransponder/GetSensorDataCycle', 1000, 10)
                    if answer != None:
                        if answer.status_code == 200:
                            print("\t\t=> OK")
                        else:
                            print("\t\t=> ERROR")
                            print(answer.text)
                elif selValue == "A" or selValue == "a":
                    print("\tA - GET Timeout")
                    answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/doc/Timeout')
                    if answer != None:
                        if answer.status_code == 200:
                            print(answer.text)
                        else:
                            print(__errorPrintHeaderStr + str(answer.status_code))
                elif selValue == "B" or selValue == "b":
                    print("\tA - POST Timeout")
                    print("New timeout in ms: ", end="")
                    timeoutValue = input() # read console 
                    answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/doc/Timeout?&Value='+timeoutValue)
                    if answer != None:
                        if answer.status_code == 200:
                            print("\t\t=> OK")
                        else:
                            print("\t\t=> ERROR")
                            print(answer.text)
                elif selValue == "0":
                    break
            # DISCONNECT DOC
            print("Disconnecting...")
            answer = PerformPostRequest('http://' + __hostname + ':19813/api/iidservice/doc/Terminate')
            if answer != None:
                if answer.status_code == 200:
                    print("Disconnect: " + answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "h" or selValue == "H":
            print("\tH - Change RESTful destination")
            print("New destination: ", end="")
            __hostname = input() # read console
            print("--> iIDservice running on: " + __hostname)
        elif selValue == "v" or selValue == "V":
            print ("\tV - Version information")
            answer = PerformGetRequest('http://' + __hostname + ':19813/api/iidservice/Version')
            if answer != None:
                if answer.status_code == 200:
                    print(answer.text)
                else:
                    print(__errorPrintHeaderStr + str(answer.status_code))
        elif selValue == "0":
            break
