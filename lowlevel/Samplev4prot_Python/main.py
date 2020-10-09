from ConsoleFlow import OutputPossibleFunctions, OutputPossibleMemoryBlocks
import Functionsv4pro

'''
Little Console Application without GUI
'''
if __name__ == "__main__":

    rfidFunctions = Functionsv4pro.Functionsv4pro()
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
                readUiiResult = rfidFunctions.Read_ISO18000_6C_UII()
                if readUiiResult["statusCode"] == 0x00:
                    print("UID read: " + str(readUiiResult["uiiBytes"]))
                    readUiiBytes = readUiiResult["uiiBytes"]
                    numUiisFound = readUiiBytes[0]
                    bytePointer = 1
                    for i in range(numUiisFound):
                        if bytePointer >= len(readUiiBytes):
                            break
                        uiiLength = readUiiBytes[bytePointer]
                        bytePointer += 1
                        if bytePointer+uiiLength > len(readUiiBytes):
                            break
                        uiiBytes = readUiiBytes[bytePointer:bytePointer+uiiLength]
                        #print("UII[" + str(i) + "]: " + str(uiiBytes))
                        print("UII[" + str(i) + "]: " + uiiBytes.hex())
                        bytePointer += uiiLength
                else:
                    print("Error: " + hex(readUiiResult["statusCode"]) + " reading ISO18000-6C UIIs")
            elif select == "3":
                pageNum = 3
                wordNum = 0
                OutputPossibleMemoryBlocks()
                select = str(input())
                if select == "1":
                    pageNum = 0
                if select == "2":
                    pageNum = 1
                if select == "3":
                    pageNum = 2
                print("Please enter word number to read")
                wordNum = int(input())
                readBlockResult = rfidFunctions.Read_ISO18000_6C_Words(pageNum,  wordNum,  4) #read 4 words
                if readBlockResult["statusCode"] == 0x00:
                    print("Data read: " + readBlockResult["dataBytes"].hex())
                else:
                    print("Error: " + hex(readBlockResult["statusCode"]) + " reading ISO18000-6C Words")
            elif select == '4':
                dataToWrite = b'\x00\x00'
                print("1 - 0x3132")
                print("2 - 0xAABB")
                print("# - 0x0000")
                select = str(input())
                if select == "1":
                    dataToWrite = b'\x31\x32'
                if select == "2":
                    dataToWrite = b'\xAA\xBB'
                pageNum = 3
                wordNum = 0
                OutputPossibleMemoryBlocks()
                select = str(input())
                if select == "1":
                    pageNum = 0
                if select == "2":
                    pageNum = 1
                if select == "3":
                    pageNum = 2
                print("Please enter word number to read")
                wordNum = int(input())
                writeBlockResult = rfidFunctions.Write_ISO18000_6C_Word(pageNum,  wordNum,  dataToWrite)
                if writeBlockResult["statusCode"] == 0x00:
                    print("Block: " + str(wordNum) + " written")
                else:
                    print("Error: " + hex(writeBlockResult["statusCode"]) + " writing ISO18000-6C Word")
            else:
                exitConsole = True

        # disconnect reader from serial port
        isConnClosed = rfidFunctions.CloseCommunicationPort()
        if isConnClosed:
            print("Connection successfully closed")
        else:
            print("Connection not closed")
