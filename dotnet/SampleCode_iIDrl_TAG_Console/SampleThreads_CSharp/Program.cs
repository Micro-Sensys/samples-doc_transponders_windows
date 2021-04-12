using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using iIDReaderLibrary.Utils.Definitions;
using System;
using System.Threading;

namespace SampleThreads_CSharp
{
    class Program
    {
        /*
         * SampleThreads_CSharp
         *      SampleCode for iIDReaderLibrary.DocInterfaceControl
         *      Implemented in C#
         *      Using "Start..." functions
         *      
         * This sample demonstrates how to call the DocInterfaceControl functions that run the process in a separate new thread.
         * This is only for demo purposes. For a Console application is not efficient to work in this way.
         */

        private static byte[] m_LastTagID = null;
        static void Main(string[] args)
        {
            Console.WriteLine(".NETCore Console");
            Console.WriteLine("SampleThreads_C#");
            Console.WriteLine("--------------------");
            Console.WriteLine("Library Version: " + iIDReaderLibrary.Version.LibraryVersion);

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
                while (Console_ExecuteAndContinue(docIntControl))
                {
                    Thread.Sleep(500);
                }

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

        static volatile bool initializeCompleted = false;
        private static DocInterfaceControl Console_InitializeDocInterfaceControl()
        {
            Console.WriteLine("== Select initialize parameters ==");
            //Get PortType
            int portType = Console_InitializePortType();
            string portName = "";
            switch (portType)
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

            //Call initialize to open the communication port
            result.InitializeCompleted += DocInterfaceControl_InitializeCompleted;
            result.StartInitialize();
            Console.Write("Initializing...");
            //For demo purposes, just wait blocking execution until "Initialize" process is completed (notified using "InitializeCompleted" event)
            while (!initializeCompleted) //Alternative, call "IsInitializing"
            {
                Thread.Sleep(100);
                Console.Write(".");
            }
            Console.WriteLine("");
            if (result.IsInitialized)
            {
                Console.WriteLine("\tInitialized");
                result.DocResultChanged += DocInterfaceControl_DocResultChanged;
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

        private static void DocInterfaceControl_InitializeCompleted(object _sender, bool _portOpen)
        {
            // using "_portOpen" the result of the operation can be checked
            initializeCompleted = true;
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
            for (int i = 0; i < portNames.Length; i++)
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

        static volatile bool docOperationCompleted = false;
        private static void DocInterfaceControl_DocResultChanged(object sender, DocResultEventArgs _docResult)
        {
            // this will be called for each result
            if (_docResult != null)
            {
                Console.WriteLine(_docResult.Timestamp.ToString("HH:mm:ss,fff"));
                if (_docResult.ResultInfo != null)
                {
                    if (_docResult.ResultInfo is HfScanResultInfo)
                    {
                        //Result to Identify - InterfaceType = 1356
                        Console.WriteLine("\t" + BitConverter.ToString((_docResult.ResultInfo as HfScanResultInfo).TagID));
                        //Save UID for "ReadBytes"/"WriteBytes" process
                        m_LastTagID = (_docResult.ResultInfo as HfScanResultInfo).TagID;
                    }
                    if (_docResult.ResultInfo is UhfScanResultInfo)
                    {
                        //Result to Identify - InterfaceType = 868
                        var uhfScanResult = (_docResult.ResultInfo as UhfScanResultInfo);
                        Console.WriteLine("\t" + uhfScanResult.ToString().Replace("\n","\n\t"));
                        if (uhfScanResult.TagInfoList.Count > 0)
                            m_LastTagID = uhfScanResult.TagInfoList[0].UII.UII;
                    }
                    if (_docResult.ResultInfo is ReadBytesResultInfo)
                    {
                        //Result to ReadBytes
                        var readResult = _docResult.ResultInfo as ReadBytesResultInfo;
                        if (readResult.ReadResult != null)
                        {
                            //Data read
                            Console.WriteLine("\tRead completed:");
                            Console.WriteLine("\t\tPage: {0}", readResult.FromPage);
                            Console.WriteLine("\t\tStartByte: {0}, Length: {1}", readResult.FromByte, readResult.ReadResult.Length);
                            Console.WriteLine("\t  DataRead: {0}", BitConverter.ToString(readResult.ReadResult));
                        }

                        if (_docResult.ProcessFinished)
                        {
                            docOperationCompleted = true;
                            Console.WriteLine("\tDOC function END");
                            return;
                        }
                    }
                    if (_docResult.ResultInfo is WriteBytesResultInfo)
                    {
                        //Result to WriteBytes
                        var writeResult = (_docResult.ResultInfo as WriteBytesResultInfo);
                        if (writeResult.WriteResult)
                        {
                            //Data written
                            Console.WriteLine("\tWrite completed:");
                            Console.WriteLine("\t\tPage: {0}", writeResult.FromPage);
                            Console.WriteLine("\t\tStartByte: {0}, Length: {1}", writeResult.FromByte, writeResult.WrittenBytes.Length);
                            Console.WriteLine("\t  DataWritten: {0}", BitConverter.ToString(writeResult.WrittenBytes));
                        }

                        if (_docResult.ProcessFinished)
                        {
                            docOperationCompleted = true;
                            Console.WriteLine("\tDOC function END");
                            return;
                        }
                    }
                }
                else
                {
                    if (_docResult.ProcessFinished)
                    {
                        docOperationCompleted = true;
                        Console.WriteLine("\tDOC function END");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("\tResultInfo = NULL");
                        return;
                    }
                }
                if (_docResult.ResultException != null)
                {
                    Console.WriteLine("\tException! ");
                    System.Diagnostics.Debug.WriteLine(_docResult.ResultException.ToString());
                }
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
                        //Call ReadReaderID and show result (no "thread" function available for this call. Should not block for a long time)
                        var readerID = _docIntControl.ReadReaderID();
                        if (readerID != null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("ReaderID:");
                            Console.WriteLine(readerID.ToString());
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
                    Console.WriteLine("");
                    docOperationCompleted = false;
                    //Start "Identify" process
                    //  RepeatCount --> 5 (search for transponder 5 times)
                    //  DelayBetweenSearchs --> 0 (ms to wait between each search)
                    //  NotifySuccessOnly --> false (raise "DocResultChanged" event even if no transponder found)
                    _docIntControl.StartIdentify(5, 0, false);
                    //For demo purposes, just wait blocking execution until DOC process is completed (notified using "DocResultChanged" event, ProcessFinished = true)
                    while (!docOperationCompleted)
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("");
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
                    Console.WriteLine("");
                    Console.WriteLine("Reading 16 Bytes from position 0 (for UHF using page 3)");
                    docOperationCompleted = false;
                    //Start "ReadBytes" process
                    //  TagID --> m_LastTagID (contains TagID found in last call to Identify)
                    //  Page --> 3 (for HF tags not needed, for UHF page 3 is user-block)
                    //  From --> 0 (first byte in memory)
                    //  Length --> 16 (Bytes 0 - 15 will be read)
                    _docIntControl.StartReadBytes(m_LastTagID, 3, 0, 16);
                    //For demo purposes, just wait blocking execution until DOC process is completed (notified using "DocResultChanged" event, ProcessFinished = true)
                    while (!docOperationCompleted)
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("");
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
                    Console.WriteLine("");
                    Console.WriteLine("Writing \"00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF\" from position 0 (for UHF using page 3)");
                    byte[] toWrite = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
                    docOperationCompleted = false;
                    //Start WriteBytes
                    //  TagID --> m_LastTagID (contins first TagID found in last call to Identify)
                    //  Page --> 3 (for HF tags not needed, for UHF page 3 is user-block)
                    //  From --> 0 (first byte in memory)
                    //  Data --> "toWrite" array containing the data to be written
                    _docIntControl.StartWriteBytes(m_LastTagID, 3, 0, toWrite);
                    //For demo purposes, just wait blocking execution until DOC process is completed (notified using "DocResultChanged" event, ProcessFinished = true)
                    while (!docOperationCompleted)
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("DocInterfaceControl not initialized!");
                }
            }
        }
    }
}
