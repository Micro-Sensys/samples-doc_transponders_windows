#include <iostream>

#include "Functions3000prot.h"
#include "ConsoleFlow.h"

using namespace std;

int main()
{
    /*int x = 30;
    unsigned char y = 30;
    std::cout << std::hex << x << std::endl;
    std::cout << std::hex << y << std::endl;
    return 0;*/

    bool portIsOpen  = false;
    Functions3000prot rfidFunctions;

    // output message if you want connect with reader
    std::cout << "Try to open port with Reader ?" << std::endl;
    std::cout << "1 - yes / # - no" << std::endl;

    // save console input value in variable
    char input = '0';
    std::cin >> input;

    // check console input
    if (input == '1')
    {
        char port[25] = {0}; 

        bool exitConsole = false;
        // loop until cancel connection
        while (!exitConsole)
        {
            std::cout << "Trying to open port... COM3" << std::endl;
            portIsOpen = rfidFunctions.OpenCommunicationPort("COM3");
            if (portIsOpen)
            {
                std::cout << "Port open!" << std::endl;
                break;
            }
            else
            {
                std::cout << "A - Abort" << std::endl;
                std::cin >> port;
                if (port[0] == 'A')
                {
                    exitConsole = true;
                    break;
                }
            }
        }
        if (portIsOpen)
        {
            long serialNumberReader = 0;
            unsigned char readerIdBytes[8] = {0};
            while (!exitConsole)
            {
                std::cout << "Trying communicate with reader..." << std::endl;
                if (rfidFunctions.ReadReaderID(readerIdBytes, serialNumberReader) == 0x00)
                {
                    std::cout << "Reader: " << serialNumberReader << " Connected" << std::endl; // output reader id
                    break;
                }
                else
                {
                    std::cout << "No Reader ID" << std::endl;
                    std::cout << "A - Abort, other letter to try again" << std::endl;
                    std::cin >> port;
                    if (port[0] == 'A')
                    {
                        exitConsole = true;
                        break;
                    }
                }
            }

            while (!exitConsole)
            {
                PossibleFunctions();
                // save console input value in variable
                char input = '#';
                unsigned char result;
                std::cin >> input;
                switch(input)
                {
                case '1':
                    {
                        result = rfidFunctions.ReadReaderID(readerIdBytes, serialNumberReader);
                        if (result == 0x00)
                        {
                            std::cout << "Reader: " << std::dec << serialNumberReader << " Connected" << std::endl; // output reader id
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " reading Reader ID" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '2':
                    {
                        unsigned char uidBytes[8] = {0}; //ISO15693 UID is always 8 byte long
                        result = rfidFunctions.Read_ISO15693_UID(uidBytes);
                        if (result == 0)
                        {
                            std::cout << "UID read:";
                            for(int i = 0; i < sizeof(uidBytes); i++)
                            {
                                std::cout << " " << std::hex << (int)uidBytes[i];
                            }
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " reading ISO15693 UID" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '3':
                    {
                        std::cout << "Please enter block number to read" << std::endl << std::dec;
                        int blockNumber = 0;
                        int blockLength = 0;
                        unsigned char blockBytes[8] = {0}; //ISO15693 data can be 8 byte long maximum
                        std::cin >> blockNumber; // input block number to be read
                        result = rfidFunctions.Read_ISO15693_Block((unsigned char)blockNumber, blockBytes, blockLength);
                        if (result == 0)
                        {
                            std::cout << "Block read:";
                            for(int i = 0; i < blockLength; i++)
                            {
                                std::cout << " " << std::hex << (int)blockBytes[i];
                            }
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " reading ISO15693 data" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '4':
                    {
                        //For demo purposes, two options are provided to be written in memory
                        std::cout << "Select which data to write" << std::endl;
                        std::cout << "1 - 0x31323334" << std::endl;
                        std::cout << "2 - 0xAABBCCDD" << std::endl;
                        std::cout << "# - 0x00000000" << std::endl;
                        std::cin >> input;
                        unsigned char dataToWrite[4] = {0}; //In this sample only writing 4 bytes is supported
                        switch(input)
                        {
                        case '1':
                            dataToWrite[0] = 0x31;
                            dataToWrite[1] = 0x32;
                            dataToWrite[2] = 0x33;
                            dataToWrite[3] = 0x34;
                            break;
                        case '2':
                            dataToWrite[0] = 0xAA;
                            dataToWrite[1] = 0xBB;
                            dataToWrite[2] = 0xCC;
                            dataToWrite[3] = 0xDD;
                            break;
                        }
                        std::cout << "Please enter block number to write" << std::endl << std::dec;
                        int blockNumber = 0;
                        std::cin >> blockNumber; // input block number to be written
                        result = rfidFunctions.Write_ISO15693_Block(blockNumber, dataToWrite);
                        if (result == 0)
                        {
                            std::cout << "Block " << blockNumber << " written";
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " writing ISO15693 data" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '#':
                    exitConsole = true;
                    break;
                }

            }

            rfidFunctions.CloseCommunicationPort();
        }
    }
    return 0;
}
