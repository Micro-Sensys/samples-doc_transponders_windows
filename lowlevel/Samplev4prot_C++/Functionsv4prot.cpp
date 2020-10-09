#include "Functionsv4prot.h"

Functionsv4prot::Functionsv4prot()
{
    //ctor
    m_pConnection = new PortConnection;
    m_pCrc16    = new CRC16;
    m_SeqNum    = 0x10;
}

Functionsv4prot::~Functionsv4prot()
{
    //dtor
    delete m_pConnection;
    delete m_pCrc16;
}

bool    Functionsv4prot::OpenCommunicationPort(const char* _pPort)
{
    m_Connected = m_pConnection->OpenPort(_pPort);
    return m_Connected;
}

bool    Functionsv4prot::CloseCommunicationPort()
{
    if (m_Connected)
    {
        m_Connected = !m_pConnection->ClosePort();
    }
    return !m_Connected;
}

unsigned char   Functionsv4prot::GetNextSeqNum()
{
    m_SeqNum += 1;
    return m_SeqNum;
}

unsigned char   Functionsv4prot::ReadReaderID(unsigned char* _pReaderIdBytes, long& _pReaderID)
{
    unsigned char readReaderIdCmd[10] = {0};
    readReaderIdCmd[0] = 0x02;              //SOH
    readReaderIdCmd[1] = GetNextSeqNum();   //SeqNum
    readReaderIdCmd[2] = 0x00;              //Length High
    readReaderIdCmd[3] = 0x06;              //Length Low
    readReaderIdCmd[4] = 0x10;              //Command Group
    readReaderIdCmd[5] = 0x10;              //Command byte 1
    readReaderIdCmd[6] = 0xFF;              //Command byte 2
    readReaderIdCmd[7] = 0x01;              //Parameter byte
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readReaderIdCmd, sizeof(readReaderIdCmd) - 2, crc);
    readReaderIdCmd[sizeof(readReaderIdCmd) - 2] = crc[0];
    readReaderIdCmd[sizeof(readReaderIdCmd) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readReaderIdAnswer[15] = {0}; // 15 bytes can be received here Maximum
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readReaderIdCmd, sizeof(readReaderIdCmd)) == sizeof(readReaderIdCmd))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readReaderIdAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readReaderIdAnswer[4] != 0x00) //if status byte with error
                return readReaderIdAnswer[4]; // if error return error

            // if answer OK save reader ID bytes in pointer array. Last byte to save from answer is: answer length - 2 (CRC)
            for (int i = 5, j = 0; i < answerLength - 2; i++, j++)
                _pReaderIdBytes[j] = readReaderIdAnswer[i];

            //calculate Reader ID
            long id = 0; // minimum 32 bit needed
            if ((readReaderIdAnswer[12] & 0x80) != 0)
                id |= (static_cast<long>(readReaderIdAnswer[7]) << 16 | static_cast<long>(readReaderIdAnswer[6]) << 8 | static_cast<long>(readReaderIdAnswer[5]));
            else
                id |= (static_cast<long>(readReaderIdAnswer[6]) << 8 | static_cast<long>(readReaderIdAnswer[5]));
            _pReaderID = id;

            return readReaderIdAnswer[4]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}

unsigned char   Functionsv4prot::Read_ISO18000_6C_UII(unsigned char* _pUiiBytes)
{
    unsigned char readISO18000UiiCmd[11] = {0};
    readISO18000UiiCmd[0] = 0x02;              //SOH
    readISO18000UiiCmd[1] = GetNextSeqNum();   //SeqNum
    readISO18000UiiCmd[2] = 0x00;              //Length High
    readISO18000UiiCmd[3] = 0x07;              //Length Low
    readISO18000UiiCmd[4] = 0x30;              //Command Group
    readISO18000UiiCmd[5] = 0x10;              //Command byte 1
    readISO18000UiiCmd[6] = 0xFF;              //Command byte 2
    readISO18000UiiCmd[7] = 0x01;              //Parameter byte
    readISO18000UiiCmd[8] = 0x01;              //Parameter byte
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readISO18000UiiCmd, sizeof(readISO18000UiiCmd) - 2, crc);
    readISO18000UiiCmd[sizeof(readISO18000UiiCmd) - 2] = crc[0];
    readISO18000UiiCmd[sizeof(readISO18000UiiCmd) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readISO1800UiiAnswer[64] = {0}; // Answer can be of various length. For DEMO purposes used 64 bytes
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readISO18000UiiCmd, sizeof(readISO18000UiiCmd)) == sizeof(readISO18000UiiCmd))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readISO1800UiiAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readISO1800UiiAnswer[4] != 0x00) //if status byte with error
                return readISO1800UiiAnswer[4]; // if error return error

            // if answer OK save reader ID bytes in pointer array. Last byte to save from answer is: answer length - 2 (CRC)
            for (int i = 5, j = 0; i < answerLength - 2; i++, j++)
                _pUiiBytes[j] = readISO1800UiiAnswer[i];

            return readISO1800UiiAnswer[4]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}

unsigned char   Functionsv4prot::Read_ISO18000_6C_Words(unsigned char _pageNum, unsigned int _wordAddr, unsigned char _wordCount, unsigned char* _pDataBytes)
{
    unsigned char readISO18000WordsCmd[15] = {0};
    readISO18000WordsCmd[0] = 0x02;              //SOH
    readISO18000WordsCmd[1] = GetNextSeqNum();   //SeqNum
    readISO18000WordsCmd[2] = 0x00;              //Length High
    readISO18000WordsCmd[3] = 0x0B;              //Length Low
    readISO18000WordsCmd[4] = 0x30;              //Command Group
    readISO18000WordsCmd[5] = 0x12;              //Command byte 1
    readISO18000WordsCmd[6] = 0xFF;              //Command byte 2
    readISO18000WordsCmd[7] = 0x01;              //Parameter byte
    readISO18000WordsCmd[8] = _pageNum;          //Memory bank
    readISO18000WordsCmd[9] = static_cast<unsigned char>((_wordAddr & 0xFF0000) >> 16);  //Word Address High
    readISO18000WordsCmd[10] = static_cast<unsigned char>((_wordAddr & 0xFF00) >> 8);     //Word Address middle
    readISO18000WordsCmd[11] = static_cast<unsigned char>((_wordAddr & 0xFF));            //Word Address low
    readISO18000WordsCmd[12] = _wordCount;
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(readISO18000WordsCmd, sizeof(readISO18000WordsCmd) - 2, crc);
    readISO18000WordsCmd[sizeof(readISO18000WordsCmd) - 2] = crc[0];
    readISO18000WordsCmd[sizeof(readISO18000WordsCmd) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char readISO1800WordsAnswer[_wordCount * 2 + 8] = {0}; // Answer can be of various length. Calculate length using the number of words to read
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(readISO18000WordsCmd, sizeof(readISO18000WordsCmd)) == sizeof(readISO18000WordsCmd))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *readISO1800WordsAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            if (readISO1800WordsAnswer[4] != 0x00) //if status byte with error
                return readISO1800WordsAnswer[4]; // if error return error

            // if answer OK save reader ID bytes in pointer array. Last byte to save from answer is: answer length - 2 (CRC)
            for (int i = 5, j = 0; i < answerLength - 2; i++, j++)
                _pDataBytes[j] = readISO1800WordsAnswer[i];

            return readISO1800WordsAnswer[4]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}

unsigned char Functionsv4prot::Write_ISO18000_6C_Word(unsigned char _pageNum, unsigned int _wordAddr, unsigned char* _pBlockBytes)
{
    unsigned char writeISO18000WordsCmd[16] = {0};
    writeISO18000WordsCmd[0] = 0x02;              //SOH
    writeISO18000WordsCmd[1] = GetNextSeqNum();   //SeqNum
    writeISO18000WordsCmd[2] = 0x00;              //Length High
    writeISO18000WordsCmd[3] = 0x0C;              //Length Low
    writeISO18000WordsCmd[4] = 0x30;              //Command Group
    writeISO18000WordsCmd[5] = 0x14;              //Command byte 1
    writeISO18000WordsCmd[6] = 0xFF;              //Command byte 2
    writeISO18000WordsCmd[7] = 0x00;              //Parameter byte
    writeISO18000WordsCmd[8] = _pageNum;          //Memory bank
    writeISO18000WordsCmd[9] = static_cast<unsigned char>((_wordAddr & 0xFF0000) >> 16);  //Word Address High
    writeISO18000WordsCmd[10] = static_cast<unsigned char>((_wordAddr & 0xFF00) >> 8);     //Word Address middle
    writeISO18000WordsCmd[11] = static_cast<unsigned char>((_wordAddr & 0xFF));            //Word Address low
    writeISO18000WordsCmd[12] = _pBlockBytes[0];     //Data to write
    writeISO18000WordsCmd[13] = _pBlockBytes[1];     //Data to write
    unsigned char crc[2]    = {0}; // array to save crc values
    m_pCrc16->CalculateCRC16(writeISO18000WordsCmd, sizeof(writeISO18000WordsCmd) - 2, crc);
    writeISO18000WordsCmd[sizeof(writeISO18000WordsCmd) - 2] = crc[0];
    writeISO18000WordsCmd[sizeof(writeISO18000WordsCmd) - 1] = crc[1];

    m_pConnection->ClearReceiveBuffer();
    unsigned char writeISO1800WordsAnswer[9] = {0}; // Answer can be maximum 9 bytes
    int answerLength = 0;
    if (m_pConnection->WriteBytesToPort(writeISO18000WordsCmd, sizeof(writeISO18000WordsCmd)) == sizeof(writeISO18000WordsCmd))
    {
        bool answerOK = ReadAndCheckReceivedAnswer(500, *writeISO1800WordsAnswer, answerLength); //check received bytes and save in array
        if (answerOK)
        {
            return writeISO1800WordsAnswer[4]; //return status byte from received bytes
        }
    }
    return 0x3F; //Return error code (Ex. 0x3F) as "Communication error" code
}

bool    Functionsv4prot::ReadAndCheckReceivedAnswer(int _timeoutMs, unsigned char& _pReceivedAnswer, int& _pAnswerLength)
{
    DWORD starttime=GetTickCount();
    int availableBytes = 0;
    unsigned char bytesBuffer[64] = {0};

    //First, wait for 7 bytes:
    //  shorter answer possible is (when no data available) --> SOH, SeqNum, length, Status, CRC
    availableBytes = m_pConnection->CheckBytesInBuffer();
    while((GetTickCount() - starttime) < (DWORD)_timeoutMs)
    {
        if (availableBytes >= 7)
        {
            break;
        }
        availableBytes = m_pConnection->CheckBytesInBuffer();
    }

    if (availableBytes >= 7)
    {
        //First 7 bytes available --> READ them & save
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

        //To continue, at least 4 bytes are needed
        if (readBytesNum < 4)
            return false; //Not enough bytes read to get answer length
        //Check SOH received
        if (bytesBuffer[0] != 0x02)
            return false; //SOH not received
        //Check SeqNum received
        if (bytesBuffer[1] != m_SeqNum)
            return false; //SeqNum received is different --> answer probably from different request?
        //Calculate total answer length: //SOH + SeqNum + LengthBytes + Length-Provided (Status (+ Data) + CRC)
        int answerTotalLength = 4 + (static_cast<int>(bytesBuffer[2]) << 8 | static_cast<int>(bytesBuffer[3]));
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
            availableBytes = m_pConnection->CheckBytesInBuffer();
            while((GetTickCount() - starttime) < (DWORD)_timeoutMs)
            {
                if (availableBytes >= bytesToRead)
                {
                    break;
                }
                availableBytes = m_pConnection->CheckBytesInBuffer();
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
