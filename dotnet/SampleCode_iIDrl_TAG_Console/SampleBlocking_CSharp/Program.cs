using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using iIDReaderLibrary.Utils.Definitions;
using System;
using System.Threading;

namespace SampleBlocking_CSharp
{
    class Program
    {
        /*
         * SampleBlocking_CSharp
         *      SampleCode for iIDReaderLibrary.DocInterfaceControl
         *      Implemented in C#
         *      Using functions that block execution
         *      
         * This sample demonstrates how to call the DocInterfaceControl functions that run in calling thread and block the execution.
         */

        private static byte[] m_LastTagID = null;
        static void Main(string[] args)
        {
            Console.WriteLine(".NETCore Console");
            Console.WriteLine("SampleBlocking_C#");
            Console.WriteLine("--------------------");
            Console.WriteLine("Library Version: " + DocInterfaceControl.LibraryVersion);

            //Get DocInterfaceControl instance
            DocInterfaceControl docIntControl = Console_InitializeDocInterfaceControl();
            if (docIntControl != null)
            {
                //DocInterfaceControl is initialized
                Console.WriteLine("");
                Console.Write("Detecting reader..");
                while (true)
                {
                    //First of all, get the Reader Information
                    Console.Write(".");
                    var readerID = docIntControl.ReadReaderID();
                    if (readerID != null)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Detected Reader:");
                        Console.WriteLine(readerID.ToString());
                        break;
                    }
                }

                //Reader info obtained --> execute functions using menu
                Console.WriteLine("");
                while (Console_ExecuteAndContinue(docIntControl)) ;

                docIntControl.Terminate();
                Console.WriteLine("");
                Console.Write("EXITING in 5");
                Thread.Sleep(1000);
                Console.Write(", 4");
                Thread.Sleep(1000);
                Console.Write(", 3");
                Thread.Sleep(1000);
                Console.Write(", 2");
                Thread.Sleep(1000);
                Console.Write(", 1");
                Thread.Sleep(1000);
            }
        }

        private static DocInterfaceControl Console_InitializeDocInterfaceControl()
        {
            Console.WriteLine("== Select initialize parameters ==");
            //Get PortType
            int portType = Console_InitializePortType();
            string portName = "";
            switch(portType)
            {
                case 0:
                case 2:
                    //For Serial & bluetooth, PortName needed.
                    portName = Console_InitializePortName();
                    break;
            }
            //Initialize InterfaceCommunicationSettings class
            var readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, portName);
            //Get InterfaceType
            int interfaceType = Console_InitializeInterfaceType();

            //Parameters selected --> Initialize class instance
            Console.WriteLine("");
            DocInterfaceControl result = new DocInterfaceControl(readerPortSettings, interfaceType);
            Console.WriteLine(string.Format("Selected parameters: PortType: {0} | PortName: {1} | IntType: {2}", portType, portName, interfaceType));
            Console.WriteLine("Initializing...");

            //Call initialize to open the communication port
            if (result.Initialize())
            {
                Console.WriteLine("\tInitialized");
                return result;
            }
            else
            {
                //Initialization failed: Terminate class instance & try again
                Console.WriteLine("\tInitialize failed");
                result.Terminate();
                return Console_InitializeDocInterfaceControl();
            }
        }

        private static int Console_InitializePortType()
        {
            Console.WriteLine("Port Type (0 = Serial, 2 = Bluetooth, 4 = USB)");
            Console.Write("Selection (confirm with ENTER): ");
            string portTypeTxt = Console.ReadLine();
            switch (portTypeTxt)
            {
                case "0":
                    Console.WriteLine("\tSerial selected");
                    return 0;
                case "2":
                    Console.WriteLine("\tBluetooth selected");
                    return 2;
                case "4":
                    Console.WriteLine("\tUSB selected");
                    return 4;
                default:
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    return Console_InitializePortType();
            }
        }
        private static string Console_InitializePortName()
        {
            int cursorTop = Console.CursorTop;
            string[] portNames = InterfaceCommunicationSettings.GetAvailablePortNames();
            Console.WriteLine("Port Name:");
            for(int i = 0; i < portNames.Length; i++)
            {
                Console.WriteLine(string.Format("{0} - {1}", i, portNames[i]));
            }
            Console.Write("Selection (confirm with ENTER): ");
            string portNameIndexTxt = Console.ReadLine();
            if (int.TryParse(portNameIndexTxt, out int portNameIndex))
            {
                if (portNameIndex < portNames.Length)
                {
                    Console.WriteLine(string.Format("\t{0} selected", portNames[portNameIndex]));
                    return portNames[portNameIndex];
                }
            }

            //Selection failed
            Console.SetCursorPosition(0, cursorTop);
            return Console_InitializePortName();
        }
        private static int Console_InitializeInterfaceType()
        {
            Console.WriteLine("Interface Type (1356 = HF|13.56MHz, 868 = UHF|868MHz)");
            Console.Write("Selection (confirm with ENTER): ");
            string interfaceTypeTxt = Console.ReadLine();
            switch (interfaceTypeTxt)
            {
                case "1356":
                    Console.WriteLine("\tHF selected");
                    return InterfaceTypeEnum.Interface_HF;
                case "868":
                    Console.WriteLine("\tUHF selected");
                    return InterfaceTypeEnum.Interface_UHF;
                default:
                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    return Console_InitializeInterfaceType();
            }
        }

        private static bool Console_ExecuteAndContinue(DocInterfaceControl _docIntControl)
        {
            //Main Console MENU
            Console.WriteLine("");
            Console.WriteLine("--------------------");
            Console.WriteLine(" Console MENU");
            Console.WriteLine("--------------------");
            Console.WriteLine("0 - ReadReaderID");
            Console.WriteLine("1 - Identify");
            Console.WriteLine("2 - ReadBytes");
            Console.WriteLine("3 - WriteBytes");
            Console.WriteLine("X - EXIT");
            Console.Write("Selection (confirm with ENTER): ");
            string operationNumTxt = Console.ReadLine();
            switch (operationNumTxt)
            {
                case "0":
                    Console.WriteLine("\tReadReaderID");
                    Console_Execute_ReadReaderID(_docIntControl);
                    break;
                case "1":
                    Console.WriteLine("\tIdentify");
                    Console_Execute_Identify(_docIntControl);
                    break;
                case "2":
                    Console.WriteLine("\tReadBytes");
                    Console_Execute_ReadBytes(_docIntControl);
                    break;
                case "3":
                    Console.WriteLine("\tWriteBytes");
                    Console_Execute_WriteBytes(_docIntControl);
                    break;
                case "X":
                case "x":
                    return false;
                default:
                    break;
            }
            return true;
        }

        private static void Console_Execute_ReadReaderID(DocInterfaceControl _docIntControl)
        {
            //First make sure DocInterfaceControl is initialized
            if (_docIntControl != null)
            {
                if (_docIntControl.IsInitialized)
                {
                    try
                    {
                        DateTime startTime = DateTime.UtcNow;
                        //Call ReadReaderID and show result
                        var readerID = _docIntControl.ReadReaderID();
                        TimeSpan processSpan = DateTime.UtcNow - startTime;
                        if (readerID != null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("ReaderID:");
                            Console.WriteLine(readerID.ToString());
                            Console.WriteLine(string.Format("(Duration: {0})", processSpan));
                        }
                        else
                        {
                            //Update result in UI
                            Console.WriteLine(string.Format("Result: FAIL. Duration: {0}", processSpan));
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Exception");
                    }
                }
                else
                {
                    Console.WriteLine("DocInterfaceControl not initialized!");
                }
            }
        }
        private static void Console_Execute_Identify(DocInterfaceControl _docIntControl)
        {
            m_LastTagID = null;
            //First make sure DocInterfaceControl is initialized
            if (_docIntControl != null)
            {
                if (_docIntControl.IsInitialized)
                {
                    try
                    {
                        DateTime startTime = DateTime.UtcNow;
                        //Call Identify and show result
                        var tagScanInfo = _docIntControl.Identify();
                        TimeSpan processSpan = DateTime.UtcNow - startTime;
                        if (tagScanInfo != null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("IdentifyResult:");
                            Console.WriteLine(tagScanInfo.ToString());
                            Console.WriteLine(string.Format("(Duration: {0})", processSpan));
                            //TagScanInfo can be either HfScanResultInfo (for HF scan) or UhfScanResultInfo (for UHF scan).
                            //  check type to obtain TagID and save it into global variable to be used by ReadBytes/WriteBytes
                            if (tagScanInfo is HfScanResultInfo)
                            {
                                m_LastTagID = (tagScanInfo as HfScanResultInfo).TagID;
                            }
                            if (tagScanInfo is UhfScanResultInfo)
                            {
                                //For UHF more than one TAG can be found at once. For demo purposes select first TagID
                                var uhfScan = (tagScanInfo as UhfScanResultInfo);
                                if (uhfScan != null)
                                {
                                    if (uhfScan.TagInfoList.Count > 0)
                                    {
                                        m_LastTagID = uhfScan.TagInfoList[0].UII.UII;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Update result in UI
                            Console.WriteLine(string.Format("Result: FAIL. Duration: {0}", processSpan));
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Exception");
                    }
                }
                else
                {
                    Console.WriteLine("DocInterfaceControl not initialized!");
                }
            }
        }
        private static void Console_Execute_ReadBytes(DocInterfaceControl _docIntControl)
        {
            //ReadBytes function needs a Tag ID as parameter --> Obtained using "Identify"
            if (m_LastTagID == null)
            {
                Console.WriteLine("Perform \"Identify\" until a transponder is found before calling ReadBytes");
                return;
            }
            //First make sure DocInterfaceControl is initialized
            if (_docIntControl != null)
            {
                if (_docIntControl.IsInitialized)
                {
                    try
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Reading 16 Bytes from position 0 (for UHF using page 3)");
                        DateTime startTime = DateTime.UtcNow;
                        //Call ReadBytes and show result
                        //  TagID --> m_LastTagID (contains TagID found in last call to Identify)
                        //  Page --> 3 (for HF tags not needed, for UHF page 3 is user-block)
                        //  From --> 0 (first byte in memory)
                        //  Length --> 16 (Bytes 0 - 15 will be read)
                        var readResult = _docIntControl.ReadBytes(m_LastTagID, 3, 0, 16);
                        TimeSpan processSpan = DateTime.UtcNow - startTime;
                        if (readResult != null)
                        {
                            Console.Write("Data read:");
                            Console.WriteLine(BitConverter.ToString(readResult));
                            Console.WriteLine(string.Format("(Duration: {0})", processSpan));
                        }
                        else
                        {
                            //Update result in UI
                            Console.WriteLine(string.Format("Result: FAIL. Duration: {0}", processSpan));
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Exception");
                    }
                }
                else
                {
                    Console.WriteLine("DocInterfaceControl not initialized!");
                }
            }
        }
        private static void Console_Execute_WriteBytes(DocInterfaceControl _docIntControl)
        {
            //WriteBytes function needs a Tag ID as parameter --> Obtained using "Identify"
            if (m_LastTagID == null)
            {
                Console.WriteLine("Perform \"Identify\" until a transponder is found before calling WriteBytes");
                return;
            }
            //First make sure DocInterfaceControl is initialized
            if (_docIntControl != null)
            {
                if (_docIntControl.IsInitialized)
                {
                    try
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Writing \"00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF\" from position 0 (for UHF using page 3)");
                        byte[] toWrite = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
                        DateTime startTime = DateTime.UtcNow;
                        //Call WriteBytes and show result
                        //  TagID --> m_LastTagID (contins first TagID found in last call to Identify)
                        //  Page --> 3 (for HF tags not needed, for UHF page 3 is user-block)
                        //  From --> 0 (first byte in memory)
                        //  Data --> "toWrite" array containing the data to be written
                        //  Lock --> false (memory address will not be locked. Lock process is not reversible)
                        if (_docIntControl.WriteBytes(m_LastTagID, 3, 0, toWrite, false))
                        {
                            Console.WriteLine("Data written successfully");
                            Console.WriteLine(string.Format("(Duration: {0})", DateTime.UtcNow - startTime));
                        }
                        else
                        {
                            Console.WriteLine("\tNOT WRITTEN");
                            Console.WriteLine(string.Format("Result: FAIL. Duration: {0}", DateTime.UtcNow - startTime));
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Exception");
                    }
                }
                else
                {
                    Console.WriteLine("DocInterfaceControl not initialized!");
                }
            }
        }
    }
}
