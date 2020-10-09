#ifndef CONSOLEFLOW_H_INCLUDED
#define CONSOLEFLOW_H_INCLUDED

void PossibleFunctions()
{
    std::cout << std::endl;
    std::cout << "1 - Read ReaderID" << std::endl;
    std::cout << "2 - Read UIIs" << std::endl;
    std::cout << "3 - Read from TAG" << std::endl;
    std::cout << "4 - Write to TAG" << std::endl;
    std::cout << "# - Exit" << std::endl;
}
void PossibleMemoryBlocks()
{
    std::cout << "Please select memory block to read" << std::endl;
    std::cout << "1 - RES" << std::endl;
    std::cout << "2 - EPC" << std::endl;
    std::cout << "3 - TID" << std::endl;
    std::cout << "4 - User" << std::endl;
}


#endif // CONSOLEFLOW_H_INCLUDED
