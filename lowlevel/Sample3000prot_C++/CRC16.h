#ifndef CRC16_H
#define CRC16_H

class CRC16
{

public:

    /*
     * calculates the crc 16 checksum over the given bytes
     * parameter: _pBytes:      array with bytes over which the checksum is to be calculated
     *            _NumberBytes: number of bytes in array
     *
     *             _pChecksum:  stores the determined checksum in an array which can be accessed
     */
    static void CalculateCRC16(unsigned char* _pBytes, int _NumberBytes, unsigned char* _pChecksum)
    {
        int preset     = 65535; //0xFFFF
        int polynomial = 33800; //0x8408

        for (int i = 0; i < _NumberBytes; i++)
        {
            preset ^= static_cast<int>(_pBytes[i]);
            for (int j = 0; j < 8; j++)
            {
                if ((preset & 1) > 0)
                    preset = preset >> 1 ^ polynomial;
                else
                    preset >>= 1;
            }
        }
        _pChecksum[0] = static_cast<unsigned char>((preset & 255));      //CBL
        _pChecksum[1] = static_cast<unsigned char>((preset >> 8 & 255)); //CBH
    }

};

#endif // CRC16_H
