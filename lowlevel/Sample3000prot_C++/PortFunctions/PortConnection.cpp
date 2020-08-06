#include "PortConnection.h"


PortConnection::PortConnection()
{
    //ctor
    m_PortHandle = (HANDLE)-1;
}

PortConnection::~PortConnection()
{
    //dtor
}

bool PortConnection::OpenPort(const char* _pPort)
{
    /*CString sPortName = _pPort;
    sPortName = "\\\\.\\" + _pPort;
    LPTSTR lptstrPortName = sPortName.GetBuffer(CString::StringLength(sPortName));*/

    this->m_PortHandle = CreateFile(_pPort, // Pointer to the name of the port
                      GENERIC_READ | GENERIC_WRITE,
                                    // Access (read-write) mode
                      0,            // Share mode
                      NULL,         // Pointer to the security attribute
                      OPEN_EXISTING,// How to open the serial port
                      0,            // Port attributes
                      NULL);        // Handle to port with attribute
                                    // to copy
    if (this->m_PortHandle == (HANDLE)-1)
        return false;
    else
        return SetPortSettings(m_PortHandle);
}

bool PortConnection::SetPortSettings(HANDLE _handle)
{
    DCB			PortDCB;

    PortDCB.DCBlength = sizeof (DCB);

	// Get the default port setting information.
	GetCommState (_handle, &PortDCB);
	// Change the DCB structure settings.
	PortDCB.BaudRate = 57600;             // Current baud
	PortDCB.fBinary = TRUE;               // Binary mode; no EOF check
	PortDCB.fParity = TRUE;               // Enable parity checking
	PortDCB.fOutxCtsFlow = FALSE;         // No CTS output flow control
	PortDCB.fOutxDsrFlow = FALSE;         // No DSR output flow control
	//V1.91, DTR and RTS for power/boot control
	PortDCB.fDtrControl = DTR_CONTROL_DISABLE;
                                        // DTR flow control type
	PortDCB.fDsrSensitivity = FALSE;      // DSR sensitivity
	PortDCB.fTXContinueOnXoff = TRUE;     // XOFF continues Tx
	PortDCB.fOutX = FALSE;                // No XON/XOFF out flow control
	PortDCB.fInX = FALSE;                 // No XON/XOFF in flow control
	PortDCB.fErrorChar = FALSE;           // Disable error replacement
	PortDCB.fNull = FALSE;                // Disable null stripping
	//V1.91, DTR and RTS for power/boot control
	PortDCB.fRtsControl = RTS_CONTROL_DISABLE;
                                        // RTS flow control
	PortDCB.fAbortOnError = FALSE;        // Do not abort reads/writes on
                                        // error
	PortDCB.ByteSize = 8;                 // Number of bits/byte, 4-8
	PortDCB.Parity = NOPARITY;            // 0-4=no,odd,even,mark,space
	PortDCB.StopBits = ONESTOPBIT;        // 0,1,2 = 1, 1.5, 2
                                        // error
	// Configure the port according to the specifications of the DCB
	// structure.
	if (!SetCommState (_handle, &PortDCB))
	{
		return false;
	}

	DWORD CommErrors;
	COMSTAT CommStat;
	if (ClearCommError(m_PortHandle, &CommErrors, &CommStat))
    {
        //EscapeCommFunction(_handle, SETRTS);
        EscapeCommFunction(_handle, CLRDTR);
        Sleep(200);
        EscapeCommFunction(_handle, SETDTR);
    }

	return true;
}

bool    PortConnection::ClosePort()
{
    DWORD CommErrors;
	COMSTAT CommStat;

    if (m_PortHandle != (HANDLE)-1)
    {
        if (ClearCommError(m_PortHandle, &CommErrors, &CommStat))
        {
            EscapeCommFunction(m_PortHandle, CLRDTR);
        }
        CloseHandle(m_PortHandle);
    }
    m_PortHandle = (HANDLE)-1;
    return true;
}

int     PortConnection::CheckBytesInBuffer()
{
    COMSTAT CommStat;
	DWORD	CommErrors;

	if (ClearCommError(m_PortHandle, &CommErrors, &CommStat))
    {
        return CommStat.cbInQue;
    }
    else
    {
        return 0;
    }
}

void    PortConnection::ClearReceiveBuffer()
{
    unsigned char dummy[1] = {0};
    while(CheckBytesInBuffer() > 0)
    {
        ReadBytesFromPort(dummy, 1);
    }
}

int     PortConnection::WriteBytesToPort(LPVOID _pMessage, int _Length)
{
    DWORD numWritten;
    //LPOVERLAPPED lpOverlapped;
    if (WriteFile(m_PortHandle, _pMessage, (DWORD)_Length, &numWritten, NULL))
    {
        return (int) numWritten;
    }
    else
    {
        return 0;
    }
}

int     PortConnection::ReadBytesFromPort(LPVOID _pMessage, int _Length)
{
    DWORD numRead;
    //LPOVERLAPPED lpOverlapped;
    WaitForData(m_PortHandle, 60, (DWORD)_Length);
    if (ReadFile(m_PortHandle, _pMessage, (DWORD)_Length, &numRead, NULL))
    {
        return (int) numRead;
    }
    else
    {
        return 0;
    }
}

void    PortConnection::WaitForData(HANDLE _handle, DWORD _timeout, DWORD _minDataCount)
{
    if ((_minDataCount == 0) || (_timeout == 0))
        return;

    DWORD startTimeWait = GetTickCount();

    while (((DWORD)CheckBytesInBuffer() < _minDataCount) && (GetTickCount() <= (startTimeWait + _timeout)))
    {
        Sleep(5);
    }
}
