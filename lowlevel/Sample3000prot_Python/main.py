from ConsoleFlow import OutputPossibleFunctions
import Functions3000pro

'''
Little Console Application without GUI
'''
if __name__ == "__main__":

    rfidFunctions = Functions3000pro.Functions3000pro()
    readerBytes = []
    print("Connect with Reader?")
    print("1 - yes / # - no")

    inputValue = input() # read console input

    cancelConnect = False
    portOpen = False

    if not inputValue == "1":
        print("reader not connected")
    else:
        while not cancelConnect and not portOpen:
            print("Input Port Path or # - Cancel")
            print("Possible Path")
            print("Linux: /dev/ttyUSB0")
            print("Windows: COM4")
            print("macOS: /dev/tty.usbserial-00000001")

            path = str(input()) # read console input

            if path == "#":
                cancelConnect = True
            else:
                print("Trying to open port..." + path)
                portOpen = rfidFunctions.OpenCommunicationPort(path)
                if portOpen:
                    print("Port open!")
                else:
                    print("Could not open port")
    
    if portOpen: # successfull connection
        exitConsole = False
        
        while not exitConsole:
            print("Trying to communicate with reader...")
            readerIdResult = rfidFunctions.ReadReaderID()
            if readerIdResult["statusCode"] == 0x00:
                print("Reader: " + str(readerIdResult["readerID"]) + " connected")
                break
            else:
                print("No Reader ID")
                print("A - Abort, other letter to try again")
                selection = str(input()) #read console input
                if selection == "A":
                    exitConsole = True
        
        while not exitConsole:
            # show all possible functions
            OutputPossibleFunctions()

            # save console input value in variable
            select = str(input())

            if select == "1":
                readerIdResult = rfidFunctions.ReadReaderID()
                if readerIdResult["statusCode"] == 0x00:
                    print("Reader: " + str(readerIdResult["readerID"]) + " connected")
                else:
                    print("Error: " + hex(readerIdResult["statusCode"]) + " reading Reader ID")
            elif select == "2":
                readUidResult = rfidFunctions.Read_ISO15693_UID()
                if readUidResult["statusCode"] == 0x00:
                    print("UID read: " + readUidResult["uidBytes"].hex())
                else:
                    print("Error: " + hex(readUidResult["statusCode"]) + " reading ISO15693 UID")
            elif select == "3":
                print("Please enter block number to read")
                blockNum = int(input())
                readBlockResult = rfidFunctions.Read_ISO15693_Block(blockNum)
                if readBlockResult["statusCode"] == 0x00:
                    print("Data read: " + readBlockResult["dataBytes"].hex())
                else:
                    print("Error: " + hex(readBlockResult["statusCode"]) + " reading ISO15693 Block")
            elif select == '4':
                dataToWrite = b'\x00\x00\x00\x00'
                print("1 - 0x31323334")
                print("2 - 0xAABBCCDD")
                print("# - 0x00000000")
                select = str(input())
                if select == "1":
                    dataToWrite = b'\x31\x32\x33\x34'
                if select == "2":
                    dataToWrite = b'\xAA\xBB\xCC\xDD'
                print("Please enter block number to write")
                blockNum = int(input())
                writeBlockResult = rfidFunctions.Write_ISO15693_Block(blockNum,  dataToWrite)
                if writeBlockResult["statusCode"] == 0x00:
                    print("Block: " + str(blockNum) + " written")
                else:
                    print("Error: " + hex(writeBlockResult["statusCode"]) + " writing ISO15693 Block")
            else:
                exitConsole = True

        # disconnect reader from serial port
        isConnClosed = rfidFunctions.CloseCommunicationPort()
        if isConnClosed:
            print("Connection successfully closed")
        else:
            print("Connection not closed")
