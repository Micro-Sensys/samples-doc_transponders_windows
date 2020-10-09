#include <iostream>

#include "Functionsv4prot.h"
#include "ConsoleFlow.h"

using namespace std;

int main()
{
    bool portIsOpen  = false;
    Functionsv4prot rfidFunctions;

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
                        unsigned char uiiBytes[60] = {0}; //Answer bytes contain structure with (probably) multiple Transponders
                        result = rfidFunctions.Read_ISO18000_6C_UII(uiiBytes);
                        if (result == 0)
                        {
                            int numUiisFound = (static_cast<int>(uiiBytes[0]));
                            std::cout << "UII read OK. " << std::dec << numUiisFound << " UIIs found" << std::endl;
                            unsigned char uiiLength = 0;
                            unsigned char bytePointer = 1;
                            for(int i = 0; i < numUiisFound; i++)
                            {
                                if (bytePointer >= sizeof(uiiBytes))
                                    break; //Make sure we don't address non existing byte!
                                uiiLength = uiiBytes[bytePointer];
                                bytePointer++;
                                std::cout << "PC+UII[" << std::dec << i <<"]: ";
                                for (int j = 0; j < uiiLength; j++)
                                {
                                    if (bytePointer >= sizeof(uiiBytes))
                                        break; //Make sure we don't address non existing byte!
                                    std::cout << " " << std::hex << (int)uiiBytes[bytePointer];
                                    bytePointer++;
                                }
                                std::cout << std::endl;
                            }
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " reading ISO18000-6C UIIs" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '3':
                    {
                        int pageNumber = 0;
                        PossibleMemoryBlocks();
                        char inputMemBlock = '#';
                        std::cin >> inputMemBlock;
                        switch(inputMemBlock)
                        {
                            case '1':
                                {
                                    pageNumber = 0x00;
                                    break;
                                }
                            case '2':
                                {
                                    pageNumber = 0x01;
                                    break;
                                }
                            case '3':
                                {
                                    pageNumber = 0x02;
                                    break;
                                }
                            default:
                            case '4':
                                {
                                    pageNumber = 0x03;
                                    break;
                                }
                        }
                        int wordNumber = 0;
                        std::cout << "Please enter word number to read" << std::endl << std::dec;
                        std::cin >> wordNumber; // input block number to be read
                        unsigned char dataBytes[8] = {0}; //Read 4 words
                        result = rfidFunctions.Read_ISO18000_6C_Words(pageNumber, wordNumber, 4, dataBytes);
                        if (result == 0)
                        {
                            std::cout << "Block read:";
                            for(int i = 0; i < sizeof(dataBytes); i++)
                            {
                                std::cout << " " << std::hex << (int)dataBytes[i];
                            }
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " reading ISO18000-6C Words" << std::endl;
                        }
                        std::cout << std::endl;
                    }
                    break;
                case '4':
                    {
                        //For demo purposes, a couple of options are provided to be written in memory
                        std::cout << "Select which data to write" << std::endl;
                        std::cout << "1 - 0x3132" << std::endl;
                        std::cout << "2 - 0xAABB" << std::endl;
                        std::cout << "# - 0x0000" << std::endl;
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
                        int pageNumber = 0;
                        PossibleMemoryBlocks();
                        char inputMemBlock = '#';
                        std::cin >> inputMemBlock;
                        switch(inputMemBlock)
                        {
                            case '1':
                                {
                                    pageNumber = 0x00;
                                    break;
                                }
                            case '2':
                                {
                                    pageNumber = 0x01;
                                    break;
                                }
                            case '3':
                                {
                                    pageNumber = 0x02;
                                    break;
                                }
                            default:
                            case '4':
                                {
                                    pageNumber = 0x03;
                                    break;
                                }
                        }
                        int wordNumber = 0;
                        std::cout << "Please enter word number to read" << std::endl << std::dec;
                        std::cin >> wordNumber; // input block number to be read
                        //unsigned char _pageNum, unsigned int _wordAddr, unsigned char* _pBlockBytes
                        result = rfidFunctions.Write_ISO18000_6C_Word(pageNumber, wordNumber, dataToWrite);
                        if (result == 0)
                        {
                            std::cout << "Page " << pageNumber << " word " << wordNumber << " written";
                            std::cout << std::endl;
                        }
                        else
                        {
                            std::cout << "Error 0x" << std::hex << (int)result << " writing ISO18000-6C Word" << std::endl;
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
