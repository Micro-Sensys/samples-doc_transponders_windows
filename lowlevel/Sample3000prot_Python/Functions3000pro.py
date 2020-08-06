
from PortFunctions import PortConnection
import CRC16

import time


'''
This class provides all commands that can be used to communicate with the RFID Reader and TAG
'''
class Functions3000pro:
    
    '''
    Constructor with private attributes
    '''
    def __init__(self):
        self.__portConnection   = PortConnection.PortConnection()  # private attribute
        self.__isConnected    = False
    
    def OpenCommunicationPort(self,  _portPath):
        self.__isConnected = self.__portConnection.OpenPort(_portPath)
        return self.__isConnected
    
    def CloseCommunicationPort(self):
        if self.__isConnected:
            self.__isConnected = not self.__portConnection.ClosePort()
        return not self.__isConnected
    
    def ReadReaderID(self):
        result = {
            "statusCode":0x3F, 
            "readerIdBytes":bytes([]), 
            "readerID":-1
        }
        readReaderIdCmd       = b'\x01\xF6\x00' #SOH, ReaderID Command byte, Parameter length = 0
        readReaderIdCmd = readReaderIdCmd + CRC16.CalculateCRC(readReaderIdCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readReaderIdCmd) == len(readReaderIdCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[1]
                    if answerBytes[1]  != 0x00:
                        return result
                    
                    #if answer OK, save reader ID bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    readerIdBytes = answerBytes[3:len(answerBytes)-2]
                    #calculate reader ID
                    serialNumber = 0
                    if (answerBytes[10] & 0x80) != 0:
                        serialNumber |= answerBytes[5] << 16 | answerBytes[4] << 8 | answerBytes[3]
                    else:
                        serialNumber |= answerBytes[4] << 8 | answerBytes[3]
                    
                    result["readerIdBytes"] = readerIdBytes
                    result["readerID"] = serialNumber
        return result
    
    def Read_ISO15693_UID(self):
        result = {
            "statusCode":0x3F, 
            "uidBytes":bytes([])
        }
        readIso15693UidCmd       = b'\x01\x20\x00' #SOH, ISO15693 UID Command byte, Parameter length = 0
        readIso15693UidCmd = readIso15693UidCmd + CRC16.CalculateCRC(readIso15693UidCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readIso15693UidCmd) == len(readIso15693UidCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[1]
                    if answerBytes[1]  != 0x00:
                        return result
                    
                    #if answer OK, save reader ID bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    uidBytes = answerBytes[3:len(answerBytes)-2]
                    result["uidBytes"] = uidBytes
        return result
    
    def Read_ISO15693_Block(self, _blockNum):
        result = {
            "statusCode":0x3F, 
            "dataBytes":bytes([])
        }
        readIso15693BlockCmd       = b'\x01\x41\x01' #SOH, ISO15693 Read Block Command byte, Parameter length = 1
        readIso15693BlockCmd = readIso15693BlockCmd + bytes([_blockNum])
        readIso15693BlockCmd = readIso15693BlockCmd + CRC16.CalculateCRC(readIso15693BlockCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readIso15693BlockCmd) == len(readIso15693BlockCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[1]
                    if answerBytes[1]  != 0x00:
                        return result
                    
                    #if answer OK, save reader ID bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    uidBytes = answerBytes[3:len(answerBytes)-2]
                    result["dataBytes"] = uidBytes
        return result
        
    def Write_ISO15693_Block(self, _blockNum, _dataToWrite):
        result = {
            "statusCode":0x3F, 
        }
        if len(_dataToWrite) != 4:
            return result #Not supported by this sample code
        writeIso15693BlockCmd       = b'\x01\x61\x05' #SOH, ISO15693 Write Block Command byte, Parameter length = 5
        writeIso15693BlockCmd = writeIso15693BlockCmd + bytes([_blockNum])
        writeIso15693BlockCmd = writeIso15693BlockCmd + _dataToWrite[0:4]
        writeIso15693BlockCmd = writeIso15693BlockCmd + CRC16.CalculateCRC(writeIso15693BlockCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(writeIso15693BlockCmd) == len(writeIso15693BlockCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[1]
        return result
    
    def __ReadAndCheckReceivedAnswer(self, _timeoutMs):
        result = {
            "receiveOk":False, 
            "receivedBytes":bytes([]), 
            "receivedBytesLength":0
        }
        
        startTime = time.perf_counter()
        availableBytes = 0
        bytesBuffer = bytes([])
        
        availableBytes = self.__portConnection.CheckBytesInBuffer()
        while (time.perf_counter() - startTime) < (_timeoutMs / 1000):
            if availableBytes >= 5:
                break
            availableBytes = self.__portConnection.CheckBytesInBuffer()
        
        if availableBytes >= 5:
            #First 5 bytes are available --> read them & save
            bytesBuffer = self.__portConnection.ReadBytesFromPort(availableBytes)
            
            #To continue, at least 3 bytes are needed
            if len(bytesBuffer) < 3:
                return result
            #Check SOH received
            if bytesBuffer[0] != 0x01:
                return result
            #Calculate total answer length: SOH + status + length byte + Data(length provided in LengthByte) + CRC
            answerTotalLength = 3 + bytesBuffer[2] + 2; 
            bytesToRead = answerTotalLength - len(bytesBuffer)
            if bytesToRead > 0:
                #Wait for the rest of the answer
                availableBytes = self.__portConnection.CheckBytesInBuffer()
                while (time.perf_counter() - startTime) < (_timeoutMs / 1000):
                    if availableBytes >= bytesToRead:
                        break
                    availableBytes = self.__portConnection.CheckBytesInBuffer()
                
                if bytesToRead >= availableBytes:
                    #At least expected bytes available --> read them
                    bytesBuffer = bytesBuffer + self.__portConnection.ReadBytesFromPort(bytesToRead)
            
            #Now all bytes should be read --> check length
            if len(bytesBuffer) != answerTotalLength:
                return result
            #Check CRC
            crc = CRC16.CalculateCRC(bytesBuffer)
            if crc[0] != 0:
                return result
            if crc[1] != 0:
                return result
            
            #CRC is OK --> set variables and return
            result["receivedBytes"] = bytesBuffer
            result["receivedBytesLength"] = len(bytesBuffer)
            result["receiveOk"] = True
            return result
        else:
            return result
