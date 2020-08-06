
'''
function to get CRC Bytes 

parameter:  byte array where CBL and CBH should be calculated
return:     bytearray with CBL - Check byte low, CBH - Check byte high
'''
def CalculateCRC(_byteArray):

    preset     = 65535 # 0xFFFF
    polynomial = 33800 # 0x8408

    for b in _byteArray:
        preset ^= b
        for _ in range(8):
            if (preset & 1) > 0:
                preset = preset >> 1 ^ polynomial
            else:
                preset >>= 1
    
    CBL = preset & 255
    CBH = preset >> 8 & 255
    checksum = bytearray([CBL, CBH])
    return checksum
