
import serial
import time

'''
This class is responsible for the connection between the application and the serial interface.
'''
class PortConnection:
    
    '''
    Example Ports: 
    Windows:    COM4
    Linux:      /dev/ttyUSB0
    macOS:      /dev/tty.usbserial-00000001
    
    return: true if port open else false
    '''
    def OpenPort(self, portPath):
        try:
            self.__ser = serial.Serial( port=portPath,
                                        baudrate=57600,
                                        parity=serial.PARITY_NONE,
                                        stopbits=serial.STOPBITS_ONE,
                                        bytesize=serial.EIGHTBITS)
                                        #timeout=1, #max waiting time for read
                                        #write_timeout=1, #max waiting time for write
                                        #)
        except:
            return False
        self.__ser.dtr = False;
        time.sleep(0.25)
        self.__ser.dtr = True;
        return self.__ser.is_open
    
    '''
    close serial port
    
    return: true if port successfully closed else false
    '''
    def ClosePort(self):
        self.__ser.close()
        time.sleep(0.25)
        return not self.__ser.is_open
    
    '''
    gets the number of bytes available in buffer
    
    return: number of bytes available to read
    '''
    def CheckBytesInBuffer(self):
        return self.__ser.in_waiting
        
    '''
    clears the input buffer
    
    '''
    def ClearReceiveBuffer(self):
        self.__ser.reset_input_buffer()
        self.__ser.reset_output_buffer()
        
    '''
    set new waiting time for read values over serial
    '''
    def SetNewTimeoutForReadBytes(self, timeoutTime):
        self.__ser.timeout=timeoutTime

    '''
    write message to serial port
    
    parameter: _byteMessage Message to be sent
    return: number of bytes written
    '''
    def WriteBytesToPort(self, _byteMessage):
        return self.__ser.write(_byteMessage)

    '''
    read message from serial port
    
    parameter: number of bytes to be read
    return: bytes which were read
    '''
    def ReadBytesFromPort(self, _numberBytes):
        self.WaitForData(60,  _numberBytes)
        return self.__ser.read(_numberBytes)
        
    '''
    waits for the defined time for the number of expected bytes
    
    parameter: _numMilliseconds number of ms to wait
    parameter: _minNumBytes number of bytes to expect
    '''
    def WaitForData(self, _numMilliseconds, _minNumBytes):
        if _numMilliseconds < 0:
            return
        if _minNumBytes < 0:
            return
        
        waited = 0
        while (self.CheckBytesInBuffer() < _minNumBytes) and (waited < _numMilliseconds):
            time.sleep(0.005)
            waited = waited + 5
