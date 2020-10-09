
from PortFunctions import PortConnection
import CRC16

import time


'''
This class provides all commands that can be used to communicate with the RFID Reader and TAG
'''
class Functionsv4pro:
    
    '''
    Constructor with private attributes
    '''
    def __init__(self):
        self.__portConnection   = PortConnection.PortConnection()  # private attribute
        self.__isConnected    = False
        self.__seqNum = 0x10
        
    def OpenCommunicationPort(self,  _portPath):
        self.__isConnected = self.__portConnection.OpenPort(_portPath)
        return self.__isConnected
    
    def CloseCommunicationPort(self):
        if self.__isConnected:
            self.__isConnected = not self.__portConnection.ClosePort()
        return not self.__isConnected
        
    def GetNextSeqNum(self):
        self.__seqNum = self.__seqNum + 1;
        if self.__seqNum > 0xFF:
            self.__seqNum = 0x10
        return bytes([self.__seqNum])
    
    def ReadReaderID(self):
        result = {
            "statusCode":0x3F, 
            "readerIdBytes":bytes([]), 
            "readerID":-1
        }
        readReaderIdCmd = bytes([])
        readReaderIdCmd = readReaderIdCmd + b'\x02' #SOH
        readReaderIdCmd = readReaderIdCmd + self.GetNextSeqNum() #SeqNum
        readReaderIdCmd = readReaderIdCmd + b'\x00\x06\x10\x10\xFF\x01' #Length, CMD Group, CMD Codes, Parameter byte
        readReaderIdCmd = readReaderIdCmd + CRC16.CalculateCRC(readReaderIdCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readReaderIdCmd) == len(readReaderIdCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[4]
                    if answerBytes[4]  != 0x00:
                        return result
                    
                    #if answer OK, save reader ID bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    readerIdBytes = answerBytes[5:len(answerBytes)-2]
                    #calculate reader ID
                    serialNumber = 0
                    if (answerBytes[12] & 0x80) != 0:
                        serialNumber |= answerBytes[7] << 16 | answerBytes[6] << 8 | answerBytes[5]
                    else:
                        serialNumber |= answerBytes[6] << 8 | answerBytes[5]
                    
                    result["readerIdBytes"] = readerIdBytes
                    result["readerID"] = serialNumber
        return result
        
    def Read_ISO18000_6C_UII(self):
        result = {
            "statusCode":0x3F, 
            "uiiBytes":bytes([]), 
        }
        readISO18000UiiCmd = bytes([])
        readISO18000UiiCmd = readISO18000UiiCmd + b'\x02' #SOH
        readISO18000UiiCmd = readISO18000UiiCmd + self.GetNextSeqNum() #SeqNum
        readISO18000UiiCmd = readISO18000UiiCmd + b'\x00\x07\x30\x10\xFF\x01\x01' #Length, CMD Group, CMD Codes, Parameter bytes
        readISO18000UiiCmd = readISO18000UiiCmd + CRC16.CalculateCRC(readISO18000UiiCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readISO18000UiiCmd) == len(readISO18000UiiCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[4]
                    if answerBytes[4]  != 0x00:
                        return result
                    
                    #if answer OK, save received bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    uiiBytes = answerBytes[5:len(answerBytes)-2]
                    
                    result["uiiBytes"] = uiiBytes
        return result
    
    def Read_ISO18000_6C_Words(self, _pageNum, _wordAddr, _wordCount):
        result = {
            "statusCode":0x3F, 
            "dataBytes":bytes([]), 
        }
        readISO18000WordsCmd = bytes([])
        readISO18000WordsCmd = readISO18000WordsCmd + b'\x02' #SOH
        readISO18000WordsCmd = readISO18000WordsCmd + self.GetNextSeqNum() #SeqNum
        readISO18000WordsCmd = readISO18000WordsCmd + b'\x00\x0B\x30\x12\xFF\x01' #Length, CMD Group, CMD Codes, Parameter bytes
        readISO18000WordsCmd = readISO18000WordsCmd + bytes([_pageNum])
        readISO18000WordsCmd = readISO18000WordsCmd + bytes([(_wordAddr & 0xFF0000) >> 16, (_wordAddr & 0xFF00) >> 8, (_wordAddr & 0xFF)])
        readISO18000WordsCmd = readISO18000WordsCmd + bytes([_wordCount])
        readISO18000WordsCmd = readISO18000WordsCmd + CRC16.CalculateCRC(readISO18000WordsCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(readISO18000WordsCmd) == len(readISO18000WordsCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[4]
                    if answerBytes[4]  != 0x00:
                        return result
                    
                    #if answer OK, save received bytes in answer. Last byte to save from answer is: answer length - 2 (CRC)
                    dataBytes = answerBytes[5:len(answerBytes)-2]
                    
                    result["dataBytes"] = dataBytes
        return result
    
    def Write_ISO18000_6C_Word(self, _pageNum, _wordAddr, _dataToWrite):
        result = {
            "statusCode":0x3F, 
        }
        if len(_dataToWrite) != 2:
            return result #Not supported! Only write 1x word possible
        writeISO18000WordsCmd = bytes([])
        writeISO18000WordsCmd = writeISO18000WordsCmd + b'\x02' #SOH
        writeISO18000WordsCmd = writeISO18000WordsCmd + self.GetNextSeqNum() #SeqNum
        writeISO18000WordsCmd = writeISO18000WordsCmd + b'\x00\x0C\x30\x14\xFF\x00' #Length, CMD Group, CMD Codes, Parameter bytes
        writeISO18000WordsCmd = writeISO18000WordsCmd + bytes([_pageNum])
        writeISO18000WordsCmd = writeISO18000WordsCmd + bytes([(_wordAddr & 0xFF0000) >> 16, (_wordAddr & 0xFF00) >> 8, (_wordAddr & 0xFF)])
        writeISO18000WordsCmd = writeISO18000WordsCmd + _dataToWrite[0:2]
        writeISO18000WordsCmd = writeISO18000WordsCmd + CRC16.CalculateCRC(writeISO18000WordsCmd)
        
        if self.__isConnected:
            self.__portConnection.ClearReceiveBuffer()
            if self.__portConnection.WriteBytesToPort(writeISO18000WordsCmd) == len(writeISO18000WordsCmd):
                answer = self.__ReadAndCheckReceivedAnswer(500)
                if answer["receiveOk"]:
                    answerBytes = answer["receivedBytes"]
                    result["statusCode"] = answerBytes[4]
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
            if availableBytes >= 7:
                break
            availableBytes = self.__portConnection.CheckBytesInBuffer()
        
        if availableBytes >= 7:
            #First 7 bytes are available --> read them & save
            bytesBuffer = self.__portConnection.ReadBytesFromPort(availableBytes)
            
            #To continue, at least 4 bytes are needed
            if len(bytesBuffer) < 4:
                return result
            #Check SOH received
            if bytesBuffer[0] != 0x02:
                return result
            #Check SeqNum received
            if bytesBuffer[1] != self.__seqNum:
                return result
            #Calculate total answer length: SOH + SeqNum + LengthBytes + Length-Provided (Status (+ Data) + CRC)
            answerTotalLength = 4 + (bytesBuffer[2] << 8) + bytesBuffer[3]; 
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
