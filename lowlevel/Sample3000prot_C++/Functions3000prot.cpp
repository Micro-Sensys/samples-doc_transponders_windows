#include "Functions3000prot.h"

Functions3000prot::Functions3000prot()
{
    //ctor
    m_pConnection = new PortConnection;
    m_pCrc16    = new CRC16;
}

Functions3000prot::~Functions3000prot()
{
    //dtor
    delete m_pConnection;
    delete m_pCrc16;
}

bool    Functions3000prot::OpenCommunicationPort(const char* _pPort)
{
    m_Connected = m_pConnection->OpenPort(_pPort);
    return m_Connected;
}

bool    Functions3000prot::CloseCommunicationPort()
{
    if (m_Connected)
    {
        m_Connected = !m_pConnection->ClosePort();
    }
    return !m_Connected;
}


unsigned char     Functions3000prot::ReadReaderID(unsigned char* _pReaderIdBytes, long& _pReaderID)
{
    unsigned char readReaderIdCmd[5] = {0};
    readReaderIdCmd[0] = 0x01;  //SOH
    readReaderIdCmd[1] = 0xF6;  //Read Reader ID Command byte
    readReaderIdCmd[2] = 0x00;  //Parameter length = 0
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readReaderIdCmd, sizeof(readReaderIdCmd) - 2, crc);
    readReaderIdCmd[sizeof(readReaderIdCmd) - 2] = crc[0];
    readReaderIdCmd[sizeof(readReaderIdCmd) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readReaderIdAnswer[13] = {0}; // 13 bytes can be received here Maximum
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readReaderIdCmd, sizeof(readReaderIdCmd)) == sizeof(readReaderIdCmd))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readReaderIdAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readReaderIdAnswer[1] != 0x00) //if status byte with error
                return readReaderIdAnswer[1]; // if error return error

            // if answer OK save reader ID bytes in pointer array. Last byte to save from answer is: answer length - 2 (CRC)
            for (int i = 3, j = 0; i < answerLength - 2; i++, j++)
                _pReaderIdBytes[j] = readReaderIdAnswer[i];

            //calculate Reader ID
            long id = 0; // minimum 32 bit needed
            if ((readReaderIdAnswer[10] & 0x80) != 0)
                id |= (static_cast<long>(readReaderIdAnswer[5]) << 16 | static_cast<long>(readReaderIdAnswer[4]) << 8 | static_cast<long>(readReaderIdAnswer[3]));
            else
                id |= (static_cast<long>(readReaderIdAnswer[4]) << 8 | static_cast<long>(readReaderIdAnswer[3]));
            _pReaderID = id;

            return readReaderIdAnswer[1]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}
unsigned char   Functions3000prot::Read_ISO15693_UID(unsigned char* _pUidBytes)
{
    unsigned char readIso15693Uid[5] = {0};
    readIso15693Uid[0] = 0x01;  //SOH
    readIso15693Uid[1] = 0x20;  //ISO15693 Read UID Command byte
    readIso15693Uid[2] = 0x00;  //Parameter length = 0
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readIso15693Uid, sizeof(readIso15693Uid) - 2, crc);
    readIso15693Uid[sizeof(readIso15693Uid) - 2] = crc[0];
    readIso15693Uid[sizeof(readIso15693Uid) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readUidAnswer[13] = {0}; // 13 bytes can be received here Maximum
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readIso15693Uid, sizeof(readIso15693Uid)) == sizeof(readIso15693Uid))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readUidAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readUidAnswer[1] != 0x00) //if status byte with error
                return readUidAnswer[1]; // if error return error

            // if answer OK save UID bytes in pointer array. Last byte to save from answer is: answer length - 2 (CRC)
            for (int i = 3, j = 0; i < answerLength - 2; i++, j++)
                _pUidBytes[j] = readUidAnswer[i];

            return readUidAnswer[1]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}
unsigned char   Functions3000prot::Read_ISO15693_Block(unsigned char _blockNum, unsigned char* _pBlockBytes, int& _readLength)
{
    unsigned char readIso15693Block[6] = {0};
    readIso15693Block[0] = 0x01;        //SOH
    readIso15693Block[1] = 0x41;        //ISO15693 Read Block Command byte
    readIso15693Block[2] = 0x01;        //Parameter length = 1
    readIso15693Block[3] = _blockNum;   //Block number
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readIso15693Block, sizeof(readIso15693Block) - 2, crc);
    readIso15693Block[sizeof(readIso15693Block) - 2] = crc[0];
    readIso15693Block[sizeof(readIso15693Block) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readBlockAnswer[13] = {0}; // 13 bytes can be received here Maximum
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readIso15693Block, sizeof(readIso15693Block)) == sizeof(readIso15693Block))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readBlockAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readBlockAnswer[1] != 0x00) //if status byte with error
                return readBlockAnswer[1]; // if error return error

            //If answer OK, save bytes & length
            _readLength = readBlockAnswer[2];
            // if answer OK save block bytes in pointer array.
            for (int i = 3, j = 0; j < _readLength; i++, j++)
                _pBlockBytes[j] = readBlockAnswer[i];

            return readBlockAnswer[1]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}
unsigned char Functions3000prot::Write_ISO15693_Block(unsigned char _blockNum, unsigned char* _pBlockBytes)
{
    if (sizeof(_pBlockBytes) != 4)
        return 0x4F; //error code to express parameter error
    unsigned char writeIso15693Block[10] = {0};
    writeIso15693Block[0] = 0x01;            //SOH
    writeIso15693Block[1] = 0x61;            //ISO15693 Read Block Command byte
    writeIso15693Block[2] = 0x05;            //Parameter length = 1
    writeIso15693Block[3] = _blockNum;       //Block number
    writeIso15693Block[4] = _pBlockBytes[0]; //Block data
    writeIso15693Block[5] = _pBlockBytes[1]; //Block data
    writeIso15693Block[6] = _pBlockBytes[2]; //Block data
    writeIso15693Block[7] = _pBlockBytes[3]; //Block data
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(writeIso15693Block, sizeof(writeIso15693Block) - 2, crc);
    writeIso15693Block[sizeof(writeIso15693Block) - 2] = crc[0];
    writeIso15693Block[sizeof(writeIso15693Block) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char writeBlockAnswer[5] = {0}; // 5 bytes can be received here Maximum (only status)
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(writeIso15693Block, sizeof(writeIso15693Block)) == sizeof(writeIso15693Block))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *writeBlockAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            return writeBlockAnswer[1]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}

bool    Functions3000prot::ReadAndCheckReceivedAnswer(int _timeoutMs, unsigned char& _pReceivedAnswer, int& _pAnswerLength)
{
    DWORD starttime=GetTickCount();
    int availableBytes = 0;
    unsigned char bytesBuffer[64] = {0};

    //First, wait for 5 bytes:
    //  shorter answer possible is (when no data available) --> SOH, Status, length, CRC
    while((GetTickCount() - starttime) < (DWORD)_timeoutMs)
    {
        availableBytes = m_pConnection->CheckBytesInBuffer();
        if (availableBytes >= 5)
        {
            break;
        }
    }

    if (availableBytes >= 5)
    {
        //First 5 bytes available --> READ them & save
        int readBytesNum = 0;
        if (availableBytes < sizeof(bytesBuffer))
        {
            readBytesNum = m_pConnection->ReadBytesFromPort(bytesBuffer, availableBytes);
        }
        else
        {
            //In case more bytes available than fit in "bytesBuffer" --> read all that fit
            readBytesNum = m_pConnection->ReadBytesFromPort(bytesBuffer, sizeof(bytesBuffer));
        }

        //To continue, at least 3 bytes are needed
        if (readBytesNum < 3)
            return false; //Not enough bytes read to get answer length
        //Check SOH received
        if (bytesBuffer[0] != 0x01)
            return false; //SOH not received
        //Calculate total answer length: //SOH + Status + LengthByte + Data(Length provided in LengthByte) + CRC
        int answerTotalLength = 3 + bytesBuffer[2] + 2;
        // Check if current byte buffer can allocate as many bytes!
        //      Answer longer than 64 bytes are not supported in this SampleCode
        //          --> For this a change in code is needed: buffer length definition
        if (answerTotalLength > sizeof(bytesBuffer))
            return false;
        //Calculate bytes still to be read: totalLength - alreadyReadBytes
        int bytesToRead = answerTotalLength - readBytesNum;
        if (bytesToRead > 0)
        {
            //Wait for the rest of the answer
            while((GetTickCount() - starttime) < (DWORD)_timeoutMs)
            {
                availableBytes = m_pConnection->CheckBytesInBuffer();
                if (availableBytes >= bytesToRead)
                {
                    break;
                }
            }

            if (bytesToRead >= availableBytes)
            {
                //At least expected bytes available --> read them
                readBytesNum += m_pConnection->ReadBytesFromPort(&bytesBuffer[readBytesNum], bytesToRead);
            }
        }

        //Now all bytes should be read --> check length
        if (readBytesNum != answerTotalLength)
            return false; //Not all bytes are read!
        //Check CRC
        unsigned char crc[2]    = {0}; // array to save crc values
        m_pCrc16->CalculateCRC16(bytesBuffer, readBytesNum, crc);
        if (crc[0] != 0)
            return false;
        if (crc[1] != 0)
            return false;

        //CRC is OK --> set variables and return "true"
        _pAnswerLength = answerTotalLength;
        memcpy(&_pReceivedAnswer, bytesBuffer, static_cast<unsigned long>(answerTotalLength)); // copy received bytes to _pReceivedAnswer
        return true;
    }
    else
    {
        return false;
    }
}
