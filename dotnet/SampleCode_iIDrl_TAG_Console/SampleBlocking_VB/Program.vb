Imports System
Imports System.Threading
Imports iIDReaderLibrary
Imports iIDReaderLibrary.Utils
Imports iIDReaderLibrary.Utils.Definitions

Module Program
    '
    ' SampleBlocking_VB
    '       SampleCode for iIDReaderLibrary.DocInterfaceControl
    '       Implemented in VB
    '       Using functions that block execution
    '
    ' This sample demonstrates how to call the DocInterfaceControl functions that run in calling thread and block the execution.

    Dim m_LastTagID As Byte() = Nothing
    Sub Main(args As String())
        Console.WriteLine(".NETCore Console")
        Console.WriteLine("SampleBlocking_C#")
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
        Console.WriteLine("Initializing...")

        'Call initialize to open the communication port
        If result.Initialize() Then
            Console.WriteLine(vbTab + "Initialized")
            Return result
        Else
            'Iniitalization failed: Terminate class instance & retry
            Console.WriteLine(vbTab + "Initialize failed")
            result.Terminate()
            Return Console_InitializeDocInterfaceControl()
        End If
    End Function

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
        Thread.Sleep(500)
        Return True
    End Function

    Private Sub Console_Execute_ReadReaderID(docIntControl As DocInterfaceControl)
        'First makes ure DocInterfaceContorl is initialized
        If docIntControl IsNot Nothing Then
            If docIntControl.IsInitialized Then
                Try
                    Dim startTime As Date = Date.UtcNow
                    'Call ReadReaderID and show result
                    Dim readerID = docIntControl.ReadReaderID()
                    Dim processSpan As TimeSpan = Date.UtcNow - startTime
                    If readerID IsNot Nothing Then
                        Console.WriteLine("")
                        Console.WriteLine("ReaderID:")
                        Console.WriteLine(readerID)
                        Console.WriteLine(String.Format("(Duration: {0})", processSpan))
                    Else
                        'Update result in UI
                        Console.WriteLine(String.Format("Result: FAIL. Duration: {0}", processSpan))
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
                Try
                    Dim startTime As Date = Date.UtcNow
                    'Call Identify and show result
                    Dim tagScanInfo = docIntControl.Identify()
                    Dim processSpan As TimeSpan = Date.UtcNow - startTime
                    If tagScanInfo IsNot Nothing Then
                        Console.WriteLine("")
                        Console.WriteLine("IdentifyResult:")
                        Console.WriteLine(tagScanInfo)
                        Console.WriteLine(String.Format("(Duration: {0})", processSpan))
                        'TagScanInfo can be either HFScanResultInfo (for HF scan) or UhfScanResultInfo (for UHF scan)
                        '   check tzpe to obtain TagID and save it into global variable to be used by ReadBytes/WriteBytes
                        If TypeOf tagScanInfo Is HfScanResultInfo Then
                            Dim hfInfo As HfScanResultInfo = tagScanInfo
                            m_LastTagID = hfInfo.TagID
                        End If
                        If TypeOf tagScanInfo Is UhfScanResultInfo Then
                            Dim uhfInfo As UhfScanResultInfo = tagScanInfo
                            If uhfInfo IsNot Nothing Then
                                If uhfInfo.TagInfoList.Count > 0 Then
                                    m_LastTagID = uhfInfo.TagInfoList(0).UII.UII
                                End If
                            End If
                        End If
                    Else
                        'Update result in UI
                        Console.WriteLine(String.Format("Result: FAIL. Duration: {0}", processSpan))
                    End If
                Catch ex As Exception
                    Console.WriteLine("Exception")
                End Try
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
                Try
                    Console.WriteLine("")
                    Console.WriteLine("Reading 16 Bytes from position 0 (for UHF using page 3)")
                    Dim startTime As Date = Date.UtcNow
                    'Call ReadBytes and show result
                    '  TagID --> m_LastTagID (contains TagID found in last call to Identify)
                    '  Page --> 3 (for HF tags Not needed, for UHF page 3 Is user-block)
                    '  From --> 0 (first byte in memory)
                    '  Length --> 16 (Bytes 0 - 15 will be read)
                    Dim readResult = docIntControl.ReadBytes(m_LastTagID, 3, 0, 16)
                    Dim processSpan As TimeSpan = Date.UtcNow - startTime
                    If readResult IsNot Nothing Then
                        Console.WriteLine("Data read:")
                        Console.WriteLine(BitConverter.ToString(readResult))
                        Console.WriteLine(String.Format("(Duration: {0})", processSpan))
                    Else
                        'Update result in UI
                        Console.WriteLine(String.Format("Result: FAIL. Duration: {0}", processSpan))
                    End If
                Catch ex As Exception
                    Console.WriteLine("Exception")
                End Try
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
                Try
                    Console.WriteLine("")
                    Console.WriteLine("Writing 00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF from position 0 (for UHF using page 3)")
                    Dim toWrite As Byte() = New Byte(15) {&H0, &H11, &H22, &H33, &H44, &H55, &H66, &H77, &H88, &H99, &HAA, &HBB, &HCC, &HDD, &HEE, &HFF}
                    Dim startTime As Date = Date.UtcNow
                    'Call WriteBytes And show result
                    '  TagID --> m_LastTagID (contins first TagID found in last call to Identify)
                    '  Page --> 3 (for HF tags Not needed, for UHF page 3 Is user-block)
                    '  From --> 0 (first byte in memory)
                    '  Data --> "toWrite" array containing the data to be written
                    '  Lock --> false (memory address will Not be locked. Lock process Is Not reversible)
                    If docIntControl.WriteBytes(m_LastTagID, 3, 0, toWrite, False) Then
                        Console.WriteLine("Data written successfully")
                        Console.WriteLine(String.Format("(Duration: {0})", Date.UtcNow - startTime))
                    Else
                        Console.WriteLine(vbTab + "NOT WRITTEN")
                        Console.WriteLine(String.Format("Result: FAIL. Duration: {0}", Date.UtcNow - startTime))
                    End If
                Catch ex As Exception
                    Console.WriteLine("Exception")
                End Try
            Else
                Console.WriteLine("DocInterfaceControl not initialized!")
            End If
        End If
    End Sub
End Module
