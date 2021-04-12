Imports System
Imports System.Threading
Imports iIDReaderLibrary
Imports iIDReaderLibrary.Utils
Imports iIDReaderLibrary.Utils.Definitions

Module Program
    '
    ' SampleThreads_VB
    '       SampleCode for iIDReaderLibrary.DocInterfaceControl
    '       Implemented in VB
    '       Using "Start..." functions
    '
    ' This sample demonstrates how to call the DocInterfaceControl functions that run the process in a separate new thread.
    ' This is only for demo purposes. For a Console application is not efficient to work in this way.

    Dim m_LastTagID As Byte() = Nothing
    Sub Main(args As String())
        Console.WriteLine(".NETCore Console")
        Console.WriteLine("SampleThreads_VB")
        Console.WriteLine("--------------------")
        Console.WriteLine("Library Version: " + iIDReaderLibrary.Version.LibraryVersion)

        'Get DocInterfaceControl instance
        Dim docIntControl As DocInterfaceControl = Console_InitializeDocInterfaceControl()
        If docIntControl IsNot Nothing Then
            'DocInterfaceControl is initialized
            Console.WriteLine("")
            Console.Write("Detecting reader..")
            While True
                'First of all, get the Reader Information
                Console.Write(".")
                Dim readerID = docIntControl.ReadReaderID()
                If readerID IsNot Nothing Then
                    Console.WriteLine("")
                    Console.WriteLine("Detected Reader:")
                    Console.WriteLine(readerID.ToString())
                    Exit While
                End If
            End While

            'Reader info obtained --> execute functions using menu
            Console.WriteLine("")
            While Console_ExecuteAndContinue(docIntControl)
                Thread.Sleep(500)
            End While

            docIntControl.Terminate()
            Console.WriteLine("")
            Console.Write("EXITING in 5")
            Thread.Sleep(1000)
            Console.Write(", 4")
            Thread.Sleep(1000)
            Console.Write(", 3")
            Thread.Sleep(1000)
            Console.Write(", 2")
            Thread.Sleep(1000)
            Console.Write(", 1")
            Thread.Sleep(1000)
        End If
    End Sub

    Dim initializeCompleted As Boolean = False
    Private Function Console_InitializeDocInterfaceControl() As DocInterfaceControl
        Console.WriteLine("== Select initialize parameters ==")
        'Get PortType
        Dim portType As Integer = Console_InitializePortType()
        Dim portName = ""
        Select Case portType
            Case 0, 2
                'For serial & Bluetooth, PortName needed.
                portName = Console_InitializePortName()
        End Select
        'Initialize InterfaceCommunicationSettings class
        Dim readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, portName)
        'Get InterfaceType
        Dim interfaceType As Integer = Console_InitializeInterfaceType()

        'Parameters selected --> Initialize class instance
        Console.WriteLine("")
        Dim result = New DocInterfaceControl(readerPortSettings, interfaceType)
        Console.WriteLine(String.Format("Selected parameters: PortType: {0} | PortName: {1} | IntType: {2}", portType, portName, interfaceType))

        'Call initialize to open the communication port
        AddHandler result.InitializeCompleted, AddressOf DocInterfaceControl_InitializeCompleted
        result.StartInitialize()
        Console.Write("Initializing...")
        'For demo purposes, just wait blocking execution until "Initialize" process is completed (notified using "InitializeCompleted" event
        While Not initializeCompleted
            Thread.Sleep(100)
            Console.Write(".")
        End While
        Console.WriteLine("")
        If result.IsInitialized Then
            Console.WriteLine(vbTab + "Initialized")
            AddHandler result.DocResultChanged, AddressOf DocInterfaceControl_DocResultChanged
            Return result
        Else
            'Iniitalization failed: Terminate class instance & retry
            Console.WriteLine(vbTab + "Initialize failed")
            result.Terminate()
            Return Console_InitializeDocInterfaceControl()
        End If
    End Function

    Private Sub DocInterfaceControl_InitializeCompleted(_sender As Object, _portOpen As Boolean)
        ' using "_portOpen" the result of the operation can be checked
        initializeCompleted = True
    End Sub

    Private Function Console_InitializePortType() As Integer
        Console.WriteLine("Port Type (0 = Serial, 2 = Bluetooth, 4 = USB)")
        Console.Write("Selection (confirm with ENTER): ")
        Dim portTypeTet = Console.ReadLine()
        Select Case portTypeTet
            Case "0"
                Console.WriteLine(vbTab + "Serial selected")
                Return 0
            Case "2"
                Console.WriteLine(vbTab + "Bluetooth selected")
                Return 2
            Case "4"
                Console.WriteLine(vbTab + "USB selected")
                Return 4
            Case Else
                Console.SetCursorPosition(0, Console.CursorTop - 2)
                Return Console_InitializePortType()
        End Select
    End Function
    Private Function Console_InitializePortName() As String
        Dim cursorTop = Console.CursorTop
        Dim portNames As String() = InterfaceCommunicationSettings.GetAvailablePortNames()
        Console.WriteLine("Port Name:")
        For i = 0 To portNames.Length - 1
            Console.WriteLine(String.Format("{0} - {1}", i, portNames(i)))
        Next
        Console.Write("Selection (confirm with ENTER): ")
        Dim portNameIndexTxt = Console.ReadLine()
        Dim portNameIndex As Integer
        If Integer.TryParse(portNameIndexTxt, portNameIndex) Then
            If portNameIndex < portNames.Length Then
                Console.WriteLine(vbTab + String.Format("{0} selected", portNames(portNameIndex)))
                Return portNames(portNameIndex)
            End If
        End If

        'Selection failed
        Console.SetCursorPosition(0, cursorTop)
        Return Console_InitializePortName()
    End Function
    Private Function Console_InitializeInterfaceType() As Integer
        Console.WriteLine("Interface Type (1356 = HF|13.56MHz, 868 = UHF|868MHz)")
        Console.Write("Selection (confirm with ENTER): ")
        Dim interfaceTypeTxt = Console.ReadLine()
        Select Case interfaceTypeTxt
            Case "1356"
                Console.WriteLine(vbTab + "HF selected")
                Return InterfaceTypeEnum.Interface_HF
            Case "868"
                Console.WriteLine(vbTab + "UHF selected")
                Return InterfaceTypeEnum.Interface_UHF
            Case Else
                Console.SetCursorPosition(0, Console.CursorTop - 2)
                Return Console_InitializeInterfaceType()
        End Select
    End Function

    Dim docOperationCompleted As Boolean = True
    Private Sub DocInterfaceControl_DocResultChanged(sender As Object, _docResult As DocResultEventArgs)
        ' this will be called for each result
        If _docResult IsNot Nothing Then
            Console.WriteLine(_docResult.Timestamp.ToString("HH:mm:ss,fff"))
            If _docResult.ResultInfo IsNot Nothing Then
                If TypeOf _docResult.ResultInfo Is HfScanResultInfo Then
                    'Result to Identify - InterfaceType = 1356
                    Dim hfResult As HfScanResultInfo = _docResult.ResultInfo
                    Console.WriteLine(vbTab + BitConverter.ToString(hfResult.TagID))
                    'Save UID for "ReadBytes"/"WriteBytes" process
                    m_LastTagID = hfResult.TagID
                End If
                If TypeOf _docResult.ResultInfo Is UhfScanResultInfo Then
                    'Result to Identify - InterfaceType = 868
                    Dim uhfResult As UhfScanResultInfo = _docResult.ResultInfo
                    Console.WriteLine(vbTab + uhfResult.ToString().Replace(Environment.NewLine, Environment.NewLine + vbTab))
                    If uhfResult.TagInfoList.Count > 0 Then
                        m_LastTagID = uhfResult.TagInfoList(0).UII.UII
                    End If
                End If
                If TypeOf _docResult.ResultInfo Is ReadBytesResultInfo Then
                    'Result to ReadBytes
                    Dim readResult As ReadBytesResultInfo = _docResult.ResultInfo
                    If readResult.ReadResult IsNot Nothing Then
                        'Data read
                        Console.WriteLine(vbTab + "Read completed:")
                        Console.WriteLine(vbTab + vbTab + "Page: {0}", readResult.FromPage)
                        Console.WriteLine(vbTab + vbTab + "StartByte: {0}, Length: {1}", readResult.FromByte, readResult.ReadResult.Length)
                        Console.WriteLine(vbTab + "  DataRead: {0}", BitConverter.ToString(readResult.ReadResult))
                    End If

                    If _docResult.ProcessFinished Then
                        docOperationCompleted = True
                        Console.WriteLine(vbTab + "function END")
                        Return
                    End If
                End If
                If TypeOf _docResult.ResultInfo Is WriteBytesResultInfo Then
                    'Result to WriteBytes
                    Dim writeResult As WriteBytesResultInfo = _docResult.ResultInfo
                    If writeResult.WriteResult Then
                        'Data written
                        Console.WriteLine(vbTab + "Write completed:")
                        Console.WriteLine(vbTab + vbTab + "Page: {0}", writeResult.FromPage)
                        Console.WriteLine(vbTab + vbTab + "StartByte: {0}, Length: {1}", writeResult.FromByte, writeResult.WrittenBytes.Length)
                        Console.WriteLine(vbTab + "  DataWritten: {0}", BitConverter.ToString(writeResult.WrittenBytes))
                    End If

                    If _docResult.ProcessFinished Then
                        docOperationCompleted = True
                        Console.WriteLine(vbTab + "function END")
                        Return
                    End If
                End If
            Else
                If _docResult.ProcessFinished Then
                    docOperationCompleted = True
                    Console.WriteLine(vbTab + "function END")
                    Return
                Else
                    Console.WriteLine(vbTab + "ResultInfo = NULL")
                    Return
                End If
            End If
            If _docResult.ResultException IsNot Nothing Then
                Console.WriteLine(vbTab + "Exception!")
                System.Diagnostics.Debug.WriteLine(_docResult.ResultInfo.ToString())
            End If
        End If
    End Sub
    Private Function Console_ExecuteAndContinue(docIntControl As DocInterfaceControl) As Boolean
        'Main Console MENU
        Console.WriteLine("")
        Console.WriteLine("--------------------")
        Console.WriteLine(" Console MENU")
        Console.WriteLine("--------------------")
        Console.WriteLine("0 - ReadReaderID")
        Console.WriteLine("1 - Identify")
        Console.WriteLine("2 - ReadBytes")
        Console.WriteLine("3 - WriteBytes")
        Console.WriteLine("X - EXIT")
        Console.Write("Selection (confirm with ENTER): ")
        Dim operationNumTxt = Console.ReadLine()
        Select Case operationNumTxt
            Case "0"
                Console.WriteLine(vbTab + "ReadReaderID")
                Console_Execute_ReadReaderID(docIntControl)
            Case "1"
                Console.WriteLine(vbTab + "Identfy")
                Console_Exceute_Identify(docIntControl)
            Case "2"
                Console.WriteLine(vbTab + "ReadBytes")
                Console_Execute_ReadBytes(docIntControl)
            Case "3"
                Console.WriteLine(vbTab + "WriteBytes")
                Console_Execute_WriteBytes(docIntControl)
            Case "x", "X"
                Return False
        End Select
        Return True
    End Function

    Private Sub Console_Execute_ReadReaderID(docIntControl As DocInterfaceControl)
        'First makes ure DocInterfaceContorl is initialized
        If docIntControl IsNot Nothing Then
            If docIntControl.IsInitialized Then
                Try
                    'Call ReadReaderID and show result (no "thread" function available for this call. Should not block for a long time)
                    Dim readerID = docIntControl.ReadReaderID()
                    If readerID IsNot Nothing Then
                        Console.WriteLine("")
                        Console.WriteLine("ReaderID:")
                        Console.WriteLine(readerID)
                    End If
                Catch ex As Exception
                    Console.WriteLine("Exception")
                End Try
            Else
                Console.WriteLine("DocInterfaceControl not initialized!")
            End If
        End If
    End Sub

    Private Sub Console_Exceute_Identify(docIntControl As DocInterfaceControl)
        m_LastTagID = Nothing
        'First makes ure DocInterfaceContorl is initialized
        If docIntControl IsNot Nothing Then
            If docIntControl.IsInitialized Then
                Console.WriteLine("")
                docOperationCompleted = False
                'Start "Identify" process
                '  RepeatCount --> 5 (search for transponder 5 times)
                '  DelayBetweenSearchs --> 0 (ms to wait between each search)
                '  NotifySuccessOnly --> false (raise "DocResultChanged" event even if no transponder found)
                docIntControl.StartIdentify(5, 0, False)
                'For demo purposes, just wait blocking execution until DOC process is completed (notified using "DocResultChanged" event, ProcessFinished = true)
                While Not docOperationCompleted
                    Thread.Sleep(100)
                End While
                Console.WriteLine("")
            Else
                Console.WriteLine("DocInterfaceControl not initialized!")
            End If
        End If
    End Sub

    Private Sub Console_Execute_ReadBytes(docIntControl As DocInterfaceControl)
        'ReadBytes function needs a Tag ID as parameter --> Obtained using "Identify"
        If m_LastTagID Is Nothing Then
            Console.WriteLine("Perform Identify until a transponder is found before calling ReadBytes")
            Return
        End If
        'First makes ure DocInterfaceContorl is initialized
        If docIntControl IsNot Nothing Then
            If docIntControl.IsInitialized Then
                Console.WriteLine("")
                Console.WriteLine("Reading 16 Bytes from position 0 (for UHF using page 3)")
                docOperationCompleted = False
                'Start ReadBytes and show result
                '  TagID --> m_LastTagID (contains TagID found in last call to Identify)
                '  Page --> 3 (for HF tags Not needed, for UHF page 3 Is user-block)
                '  From --> 0 (first byte in memory)
                '  Length --> 16 (Bytes 0 - 15 will be read)
                docIntControl.StartReadBytes(m_LastTagID, 3, 0, 16)
                'For demo purposes, just wait blocking execution until DOC process is completed (notified using "DocResultChanged" event, ProcessFinished = true)
                While Not docOperationCompleted
                    Thread.Sleep(100)
                End While
                Console.WriteLine("")
            Else
                Console.WriteLine("DocInterfaceControl not initialized!")
            End If
        End If
    End Sub

    Private Sub Console_Execute_WriteBytes(docIntControl As DocInterfaceControl)
        'WriteBytes function needs a Tag ID as parameter --> Obtained using "Identify"
        If m_LastTagID Is Nothing Then
            Console.WriteLine("Perform Identify until a transponder is found before calling WriteBytes")
            Return
        End If
        'First makes ure DocInterfaceContorl is initialized
        If docIntControl IsNot Nothing Then
            If docIntControl.IsInitialized Then
                Console.WriteLine("")
                Console.WriteLine("Writing 00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF from position 0 (for UHF using page 3)")
                Dim toWrite As Byte() = New Byte(15) {&H0, &H11, &H22, &H33, &H44, &H55, &H66, &H77, &H88, &H99, &HAA, &HBB, &HCC, &HDD, &HEE, &HFF}
                'Start WriteBytes And show result
                '  TagID --> m_LastTagID (contins first TagID found in last call to Identify)
                '  Page --> 3 (for HF tags Not needed, for UHF page 3 Is user-block)
                '  From --> 0 (first byte in memory)
                '  Data --> "toWrite" array containing the data to be written
                '  Lock --> false (memory address will Not be locked. Lock process Is Not reversible)
                docIntControl.StartWriteBytes(m_LastTagID, 3, 0, toWrite)
            Else
                Console.WriteLine("DocInterfaceControl not initialized!")
            End If
        End If
    End Sub
End Module
