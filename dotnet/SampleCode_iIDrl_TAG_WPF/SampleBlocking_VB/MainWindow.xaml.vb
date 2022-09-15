Imports System.ComponentModel
Imports iIDReaderLibrary
Imports iIDReaderLibrary.Utils

Class MainWindow

    Private m_Worker As BackgroundWorker = New BackgroundWorker()
    Private m_ReaderFound As Boolean

    Private m_DocInterface As DocInterfaceControl = Nothing

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        SetUiEnabled(False, 0)
        m_Worker.WorkerReportsProgress = True
        m_Worker.WorkerSupportsCancellation = True
        AddHandler m_Worker.DoWork, AddressOf Worker_DoWork

        textBlockInitialize_DriverVersion.Text = DocInterfaceControl.LibraryVersion
        textBlock_ReaderInfo.Text = "Library version: " + textBlockInitialize_DriverVersion.Text + "  - Waiting for Initialize"
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        'Stop background worker
        m_Worker.CancelAsync()
        If m_DocInterface IsNot Nothing Then
            m_DocInterface.Terminate()
        End If
    End Sub

    Private Sub SetUiEnabled(_enabled As Boolean, _readerID As Integer)
        Dispatcher.Invoke(Sub()
                              tabItem_Std.IsEnabled = _enabled
                              tabItem_SystemMask.IsEnabled = _enabled

                              If _enabled Then
                                  If _readerID > 0 Then
                                      textBlock_ReaderInfo.Text = String.Format("ReaderID: {0}", _readerID)
                                  End If
                              Else
                                  tabControl.SelectedIndex = 0
                                  textBlock_ReaderInfo.Text = "Communication lost/not possible  - Waiting for Initialize"
                                  textBlock_Status.Text = ""
                              End If
                          End Sub)
    End Sub

    Private Sub InitUiEventHandlers()
        'SystemMask selected
        AddHandler checkBox_SystemMask_Hf_ISO15693.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_iIDL.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_iIDD.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_iIDG.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_iIDN.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ICODE_UID.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_LEGIC.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ICODE_1.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ISO14443A.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_PicoTAG.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_iIDP.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ISO14443B.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ISO15693_adressed.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_ISO15693_RWblocks.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_LEGIC_FileSystem.Checked, AddressOf CheckBox_SystemMask_Checked
        AddHandler checkBox_SystemMask_Hf_DualEcho.Checked, AddressOf CheckBox_SystemMask_Checked
        'SystemMask unselected
        AddHandler checkBox_SystemMask_Hf_ISO15693.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_iIDL.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_iIDD.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_iIDG.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_iIDN.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ICODE_UID.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_LEGIC.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ICODE_1.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ISO14443A.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_PicoTAG.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_iIDP.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ISO14443B.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ISO15693_adressed.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_ISO15693_RWblocks.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_LEGIC_FileSystem.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
        AddHandler checkBox_SystemMask_Hf_DualEcho.Unchecked, AddressOf CheckBox_SystemMask_Unchecked
    End Sub

#Region "Initialize"
    Private Sub ButtonInitialize_Click(sender As Object, e As RoutedEventArgs)
        If m_DocInterface IsNot Nothing Then
            'First dispose previous instance
            m_DocInterface.Terminate()
            m_DocInterface = Nothing
        End If
        'Get Interface parameters and initialize class
        Try
            'Port type --> Get from UI
            '   0 = Serial
            '   2 = Bluetooth
            '   4 = USB
            Dim portType As Byte = 4 'Default USB
            If radioButtonInitialize_PortSerial.IsChecked.Value Then
                portType = 0
            End If
            If radioButtonInitialize_PortBt.IsChecked.Value Then
                portType = 2
            End If

            Dim readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, textBoxInitialize_PortName.Text)
            'Interface type --> Get from UI
            '   1356 = 13.56MHz (HF)
            '   868 = 868MHz (UHF)
            Dim interfaceType As Integer = 1356
            If radioButtonInitialize_Interface1356.IsChecked.Value Then
                interfaceType = 1356
            End If
            If radioButtonInitialize_Interface868.IsChecked.Value Then
                interfaceType = 868
            End If

            'Initialize class. Then call "Initialize"
            m_DocInterface = New DocInterfaceControl(readerPortSettings, interfaceType)

            'Init CheckBoxes
            InitSystemMaskCheckBoxes(m_DocInterface.SystemMask)
            InitUiEventHandlers()

            'Initialize
            textBlockInitialize_ParamInterfaceType.Text = "InterfaceType: " + interfaceType.ToString()
            textBlockInitialize_ParamPortType.Text = "PortType: " + portType.ToString()
            textBlockInitialize_ParamPortName.Text = "PortName: " + textBoxInitialize_PortName.Text

            textBlock_ReaderInfo.Text = "Calling Initialize"
            If m_DocInterface.Initialize() Then
                'Initialize worked --> Enable UI & enable BackgroundWorker to check Reader-ID
                textBlock_ReaderInfo.Text = "Initialize Result: True"
                If Not m_Worker.IsBusy Then
                    'Start the asynchronous operation
                    m_Worker.RunWorkerAsync()
                End If
            Else
                'Initialize didn't work --> disable UI
                SetUiEnabled(False, 0)
                textBlock_ReaderInfo.Text = "Initialize Result: False"
            End If
        Catch ex As Exception
            'TODO catch exception & notify
        End Try
    End Sub

    Private Sub ButtonTerminate_Click(sender As Object, e As RoutedEventArgs)
        'Stop background worker
        m_Worker.CancelAsync()

        If m_DocInterface IsNot Nothing Then
            m_DocInterface.Terminate()
        End If
        m_ReaderFound = False
        SetUiEnabled(False, 0)
    End Sub

    Private Sub Worker_DoWork(sender As Object, e As DoWorkEventArgs)
        Dim worker As BackgroundWorker = sender
        Dim readerCheckFailCount As Integer = 0
        'Check readerID in 2 seconds interval
        Dim readerCheckSpan As TimeSpan = TimeSpan.FromSeconds(5)
        'Initialize lastChecked in the past --> when Worker started it should check for the ReaderID first of all
        Dim lastCheckedOk As DateTime = DateTime.UtcNow.AddMinutes(-1)

        'While port initialized
        '  Check reader communication Is still possible in 5 seconds interval
        '      Hint: This interval Is Not fixed! It Is recomended to check the communication with the reader when no other operation Is executed.
        '=> Most important for battery powered devices!
        While (m_DocInterface.IsInitialized)

            If (worker.CancellationPending) Then
                'Exit loop if Background worker Is cancelled
                e.Cancel = True
                Exit While
            Else

                If ((DateTime.UtcNow - lastCheckedOk) < readerCheckSpan) Then

                    'Next check time still Not reached --> just do nothing
                    Threading.Thread.Sleep(200)
                    Continue While

                Else

                    'Next check time reached --> check ReaderID

                    Dim readerInfo = m_DocInterface.ReadReaderID()
                    If readerInfo IsNot Nothing Then
                        'ReaderID check OK
                        readerCheckFailCount = 0
                        lastCheckedOk = DateTime.UtcNow
                        If Not m_ReaderFound Then
                            'Not previously found --> Enable functions
                            m_ReaderFound = True
                            SetUiEnabled(True, readerInfo.ReaderID)
                        End If
                    Else
                        'ReaderID check failed
                        readerCheckFailCount += 1
                        If readerCheckFailCount > 5 Then
                            'Reader Check failed multiple times
                            If m_ReaderFound Then
                                'Previously found --> assume reader is lost!
                                m_ReaderFound = False
                                SetUiEnabled(False, 0)
                                Return
                            End If
                        End If
                        Threading.Thread.Sleep(200)
                    End If
                End If
            End If
        End While
    End Sub
#End Region

    Private Sub Button_Identify_Click(sender As Object, e As RoutedEventArgs)
        'Identify --> Search for transponders
        comboBox_TagID.IsEnabled = False
        comboBox_TagID.Items.Clear()

        If m_DocInterface IsNot Nothing Then
            If m_DocInterface.IsInitialized Then
                Dim startTime = DateTime.UtcNow
                'This function blocks & searches for a default time of 1 second (optional parameter)
                Try
                    textBox_ThreadLog.Text += Environment.NewLine + " = Identify =" + Environment.NewLine
                    Dim identifyResult = m_DocInterface.Identify()
                    Dim processSpan = DateTime.UtcNow - startTime
                    If identifyResult IsNot Nothing Then
                        If TypeOf identifyResult Is HfScanResultInfo Then
                            Dim hfResult As HfScanResultInfo = identifyResult

                            'Update result in UI
                            comboBox_TagID.Items.Add(BitConverter.ToString(hfResult.TagID))
                            'Select first ID to be used in ReadBytes/WriteBytes
                            comboBox_TagID.SelectedIndex = 0

                            Dim toLog As String = String.Format("Result: OK. Duration: {0}", processSpan) + Environment.NewLine
                            toLog += "- HF ScanResult -" + Environment.NewLine
                            toLog += vbTab + String.Format("{0}", BitConverter.ToString(hfResult.TagID))
                            toLog += Environment.NewLine
                            textBox_ThreadLog.Text += toLog
                            textBox_ThreadLog.ScrollToEnd()
                        End If
                        If TypeOf identifyResult Is UhfScanResultInfo Then
                            Dim uhfResult As UhfScanResultInfo = identifyResult

                            'Update result in UI
                            For Each tsi As UHF_TagScanInfo In uhfResult.TagInfoList
                                comboBox_TagID.Items.Add(BitConverter.ToString(tsi.UII.UII))
                            Next
                            'Select firts UII to be used in ReadBytes/WriteBytes
                            comboBox_TagID.SelectedIndex = 0
                            If comboBox_TagID.Items.Count > 1 Then
                                comboBox_TagID.IsEnabled = True
                            End If

                            Dim toLog As String = String.Format("Result: OK. Duration: {0}", processSpan) + Environment.NewLine
                            toLog += "- UHF ScanResult -" + Environment.NewLine
                            toLog += "  Ant" + vbTab + "UII-Bytes" + Environment.NewLine
                            For Each tsi As UHF_TagScanInfo In uhfResult.TagInfoList
                                toLog += String.Format("  {0}{1}{2}", tsi.AntennaNumber, vbTab, BitConverter.ToString(tsi.UII.UII)) + Environment.NewLine
                            Next
                            toLog += Environment.NewLine
                            textBox_ThreadLog.Text += toLog
                            textBox_ThreadLog.ScrollToEnd()
                        End If
                    Else
                        'Update result in UI
                        textBox_ThreadLog.Text += String.Format("Result: FAIL. Duration: {0}", processSpan) + Environment.NewLine
                        textBox_ThreadLog.ScrollToEnd()
                    End If
                Catch ex As Exception
                    Dim processSpan = DateTime.UtcNow - startTime
                    textBox_ThreadLog.Text += String.Format("Result: Exception. Duration: {0}", processSpan) + Environment.NewLine
                    textBox_ThreadLog.ScrollToEnd()
                    System.Diagnostics.Debug.WriteLine(ex.ToString())
                End Try
            End If
        End If
    End Sub

    Private Sub Button_ReadBytes_Click(sender As Object, e As RoutedEventArgs)
        textBox_DataRead.Text = ""
        If comboBox_TagID.SelectedIndex = -1 Then
            'For ReadBytes the ID must be already read using "Identify"
            MessageBox.Show(Me, "Identifier is needed --> Call Identify first)")
            Return
        End If
        'Get parameters from Window
        Dim from As Integer = Integer.Parse(textBox_From.Text)
        Dim length As Integer = Integer.Parse(textBox_Length.Text)
        Dim page As Integer = Integer.Parse(textBox_Page.Text)
        Dim tagID = iIDReaderLibrary.Utils.HelperFunctions.HexConverter.ToByteArray(comboBox_TagID.Text.Replace("-", " "))

        If m_DocInterface IsNot Nothing Then
            If m_DocInterface.IsInitialized Then
                Dim startTime = DateTime.UtcNow

                Try
                    textBox_ThreadLog.Text += Environment.NewLine + " = ReadBytes =" + Environment.NewLine
                    Dim result = m_DocInterface.ReadBytes(tagID, page, from, length)
                    Dim processSpan = DateTime.UtcNow - startTime
                    If result IsNot Nothing Then
                        'Update result in UI
                        textBox_DataRead.Text = BitConverter.ToString(result)

                        Dim toLog = String.Format("Result: OK. Duration: {0}", processSpan) + Environment.NewLine
                        toLog += vbTab + String.Format("Page: {0}", page) + Environment.NewLine
                        toLog += vbTab + String.Format("StartByte: {0}, Length: {1}", from, length) + Environment.NewLine
                        toLog += String.Format("  DataRead: {0}", BitConverter.ToString(result)) + Environment.NewLine
                        textBox_ThreadLog.Text += toLog
                        textBox_ThreadLog.ScrollToEnd()
                    Else
                        'Update result in UI
                        textBox_ThreadLog.Text += String.Format("Result: FAIL. Duration: {0}", processSpan) + Environment.NewLine
                        textBox_ThreadLog.ScrollToEnd()
                    End If
                Catch ex As Exception
                    Dim processSpan = DateTime.UtcNow - startTime
                    textBox_ThreadLog.Text += String.Format("Result: Exception. Duration: {0}", processSpan) + Environment.NewLine
                    textBox_ThreadLog.ScrollToEnd()
                    System.Diagnostics.Debug.WriteLine(ex.ToString())
                End Try
            End If
        End If
    End Sub

    Private Sub Button_WriteBytes_Click(sender As Object, e As RoutedEventArgs)
        If comboBox_TagID.SelectedIndex = -1 Then
            'For WriteBytes the ID must be already read using "Identify"
            MessageBox.Show(Me, "Identifier is needed --> Call Identify first)")
            Return
        End If
        'Get parameters from Window
        Dim from As Integer = Integer.Parse(textBox_From.Text)
        Dim length As Integer = Integer.Parse(textBox_Length.Text)
        Dim page As Integer = Integer.Parse(textBox_Page.Text)
        Dim tagID = iIDReaderLibrary.Utils.HelperFunctions.HexConverter.ToByteArray(comboBox_TagID.Text.Replace("-", " "))
        'Initialize bytes to write => length defined in TextBox
        Dim dataToWrite As Byte() = New Byte(length - 1) {}
        'Get data to write from TextBox & convert from HEX-String
        Dim aux As String() = textBox_DataToWriteHex.Text.Split(New Char() {" ", "-"})
        If aux.Length < length Then
            MessageBox.Show(Me, "Not enough data available to write in TextBox")
            Exit Sub
        End If
        Dim j As Integer = 0
        For Each str As String In aux

            If (str.Equals(" ")) Then Continue For
            Try
                dataToWrite(j) = Convert.ToByte(str, 16)
            Catch ex As Exception
                MessageBox.Show(Me, "Error converting from HEX")
                Exit Sub
            End Try

            j = j + 1
        Next str

        If m_DocInterface IsNot Nothing Then
            If m_DocInterface.IsInitialized Then
                Dim startTime = DateTime.UtcNow

                Try
                    textBox_ThreadLog.Text += Environment.NewLine + " = WriteBytes =" + Environment.NewLine
                    Dim result = m_DocInterface.WriteBytes(tagID, page, from, dataToWrite, False)
                    Dim processSpan = DateTime.UtcNow - startTime

                    'Update result in UI
                    Dim toLog = String.Format("Result: {0}. Duration: {1}", If(result, "OK", "FAIL"), processSpan) + Environment.NewLine
                    If result Then
                        toLog += vbTab + String.Format("Page: {0}", page) + Environment.NewLine
                        toLog += vbTab + String.Format("StartByte: {0}, Length: {1}", from, length) + Environment.NewLine
                        toLog += String.Format("  DataWritten: {0}", BitConverter.ToString(dataToWrite)) + Environment.NewLine
                    End If
                    textBox_ThreadLog.Text += toLog
                    textBox_ThreadLog.ScrollToEnd()
                Catch ex As Exception
                    Dim processSpan = DateTime.UtcNow - startTime
                    textBox_ThreadLog.Text += String.Format("Result: Exception. Duration: {0}", processSpan) + Environment.NewLine
                    textBox_ThreadLog.ScrollToEnd()
                    System.Diagnostics.Debug.WriteLine(ex.ToString())
                End Try
            End If
        End If
    End Sub

#Region "SystemMask"
    ' SystemMask
    '   System mask Is used to configure which kind of transponders will be searched using "iid_functions".
    '   It consist on a series of flags that represent each of the different transponder types And extra functionalities supported
    Private Sub InitSystemMaskCheckBoxes(_systemMaskValue As UInt32)
        checkBox_SystemMask_Hf_ISO15693.IsChecked = (_systemMaskValue And &H1) <> 0
        checkBox_SystemMask_Hf_iIDL.IsChecked = (_systemMaskValue And &H2) <> 0
        checkBox_SystemMask_Hf_iIDD.IsChecked = (_systemMaskValue And &H4) <> 0
        checkBox_SystemMask_Hf_iIDG.IsChecked = (_systemMaskValue And &H8) <> 0
        checkBox_SystemMask_Hf_iIDN.IsChecked = (_systemMaskValue And &H10) <> 0
        checkBox_SystemMask_Hf_ICODE_UID.IsChecked = (_systemMaskValue And &H20) <> 0
        checkBox_SystemMask_Hf_LEGIC.IsChecked = (_systemMaskValue And &H40) <> 0
        checkBox_SystemMask_Hf_ICODE_1.IsChecked = (_systemMaskValue And &H80) <> 0
        checkBox_SystemMask_Hf_ISO14443A.IsChecked = (_systemMaskValue And &H100) <> 0
        checkBox_SystemMask_Hf_PicoTAG.IsChecked = (_systemMaskValue And &H200) <> 0
        checkBox_SystemMask_Hf_iIDP.IsChecked = (_systemMaskValue And &H400) <> 0
        checkBox_SystemMask_Hf_ISO14443B.IsChecked = (_systemMaskValue And &H800) <> 0

        checkBox_SystemMask_Hf_ISO15693_adressed.IsChecked = (_systemMaskValue And &H10000000) <> 0
        checkBox_SystemMask_Hf_ISO15693_RWblocks.IsChecked = (_systemMaskValue And &H20000000) <> 0
        checkBox_SystemMask_Hf_LEGIC_FileSystem.IsChecked = (_systemMaskValue And &H40000000) <> 0
        checkBox_SystemMask_Hf_DualEcho.IsChecked = (_systemMaskValue And &H80000000) <> 0
    End Sub

    Private Sub CheckBox_SystemMask_Checked(sender As Object, e As RoutedEventArgs)
        If m_DocInterface IsNot Nothing Then
            Dim currentSystemMask As UInt32 = m_DocInterface.SystemMask
            Dim checkBoxObj As CheckBox = sender
            Dim tagText As String = checkBoxObj.Tag
            Dim mask As UInteger = Convert.ToUInt32(tagText, 16)
            m_DocInterface.SystemMask = currentSystemMask Or mask
        End If
    End Sub

    Private Sub CheckBox_SystemMask_Unchecked(sender As Object, e As RoutedEventArgs)
        If m_DocInterface IsNot Nothing Then
            Dim currentSystemMask As UInt32 = m_DocInterface.SystemMask
            Dim checkBoxObj As CheckBox = sender
            Dim tagText As String = checkBoxObj.Tag
            Dim mask As UInteger = Convert.ToUInt32(tagText, 16)
            m_DocInterface.SystemMask = currentSystemMask Or (Not mask)
        End If
    End Sub
#End Region
End Class
