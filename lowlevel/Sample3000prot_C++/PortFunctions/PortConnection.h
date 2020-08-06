#ifndef PORTCONNECTION_H
#define PORTCONNECTION_H

#include <windows.h>
#include <winbase.h>

class PortConnection
{
    public:
        PortConnection();
        virtual ~PortConnection();

        bool   OpenPort(const char* _pPort);
        bool   ClosePort();
        int    CheckBytesInBuffer();
        void   ClearReceiveBuffer();
        int    WriteBytesToPort(LPVOID _pMessage, int _numberBytes);
        int    ReadBytesFromPort(LPVOID _pMessage, int _numberBytes);

    protected:

    private:
        HANDLE    m_PortHandle;
        bool   SetPortSettings(HANDLE _handle);
        void    WaitForData(HANDLE _handle, DWORD _timeout, DWORD _minDataCount);
};

#endif // PORTCONNECTION_H
