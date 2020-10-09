#ifndef FUNCTIONS3000PROT_H
#define FUNCTIONS3000PROT_H

#include "PortConnection.h"
#include "CRC16.h"

class Functions3000prot
{
    public:
        Functions3000prot();
        ~Functions3000prot();

        bool    OpenCommunicationPort(const char* _pPort);
        bool    CloseCommunicationPort();
        unsigned char   ReadReaderID(unsigned char* _pReaderIdBytes, long& _pReaderID);
        unsigned char   Read_ISO15693_UID(unsigned char* _pUidBytes);
        unsigned char   Read_ISO15693_Block(unsigned char _blockNum, unsigned char* _pBlockBytes, int& _readLength);
        unsigned char   Write_ISO15693_Block(unsigned char _blockNum, unsigned char* _pBlockBytes);

    protected:
        bool    ReadAndCheckReceivedAnswer(int _timeoutMs, unsigned char& _pReceivedAnswer, int& _pAnswerLength);

    private:
        bool                m_Connected;
        PortConnection*     m_pConnection;      //save connection class as pointer
        CRC16*              m_pCrc16;         //save crc 16 class as pointer
};

#endif // FUNCTIONS3000PROT_H
