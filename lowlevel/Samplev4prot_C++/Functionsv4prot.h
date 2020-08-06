#ifndef FUNCTIONSV4PROT_H
#define FUNCTIONSV4PROT_H

#include "PortConnection.h"
#include "CRC16.h"

class Functionsv4prot
{
    public:
        Functionsv4prot();
        ~Functionsv4prot();

        bool    OpenCommunicationPort(const char* _pPort);
        bool    CloseCommunicationPort();
        unsigned char   ReadReaderID(unsigned char* _pReaderIdBytes, long& _pReaderID);
        unsigned char   Read_ISO18000_6C_UII(unsigned char* _pUiiBytes);
        unsigned char   Read_ISO18000_6C_Words(unsigned char _pageNum, unsigned int _wordAddr, unsigned char _wordCount, unsigned char* _pDataBytes);
        unsigned char   Write_ISO18000_6C_Word(unsigned char _pageNum, unsigned int _wordAddr, unsigned char* _pBlockBytes);

    protected:
        bool            ReadAndCheckReceivedAnswer(int _timeoutMs, unsigned char& _pReceivedAnswer, int& _pAnswerLength);
        unsigned char   GetNextSeqNum();

    private:
        bool                m_Connected;
        PortConnection*     m_pConnection;      //save connection class as pointer
        CRC16*              m_pCrc16;         //save crc 16 class as pointer
        unsigned char       m_SeqNum;
};

#endif // FUNCTIONSV4PROT_H
