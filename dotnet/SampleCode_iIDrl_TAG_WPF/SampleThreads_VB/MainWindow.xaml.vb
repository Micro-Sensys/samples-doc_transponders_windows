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

            AddHandler m_DocInterface.InitializeCompleted, AddressOf DocInterface_InitializeCompleted
            AddHandler m_DocInterface.DocResultChanged, AddressOf DocInterface_DocResultChanged
            textBlock_ReaderInfo.Text = "Calling Initialize"
            m_DocInterface.StartInitialize()
        Catch ex As Exception
            'TODO catch exception & notify
        End Try
    End Sub

    Private Sub DocInterface_InitializeCompleted(_sender As Object, _portOpen As Boolean)
        Dispatcher.Invoke(Sub()
                              If _portOpen Then
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
                          End Sub)
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

    Dim mLastDocResultTimestamp As DateTime
    Private Sub DocInterface_DocResultChanged(sender As Object, _docResult As DocResultEventArgs)
        If _docResult IsNot Nothing Then
            Dim span = DateTime.Now - mLastDocResultTimestamp
            If _docResult.ResultInfo IsNot Nothing Then
                If TypeOf _docResult.ResultInfo Is HfScanResultInfo Then
                    'Result to Identify - InterfaceType = 1356
                    Dim hfResult As HfScanResultInfo = _docResult.ResultInfo
                    Dispatcher.Invoke(Sub()
                                          Dim tagIdStr = BitConverter.ToString(hfResult.TagID)
                                          Dim toLog As String = String.Format("{0} (Duration: {1})", _docResult.Timestamp, span) + Environment.NewLine
                                          toLog += "- HF ScanResult -" + Environment.NewLine
                                          toLog += vbTab + String.Format("{0}", tagIdStr)
                                          toLog += Environment.NewLine
                                          textBox_ThreadLog.Text += toLog
                                          textBox_ThreadLog.ScrollToEnd()

                                          Dim alreadyInComboBox = False
                                          For Each str As String In comboBox_TagID.Items
                                              If str.CompareTo(tagIdStr) = 0 Then
                                                  alreadyInComboBox = True
                                                  Exit For
                                              End If
                                          Next
                                          If Not alreadyInComboBox Then
                                              comboBox_TagID.Items.Add(tagIdStr)
                                              'Select first ID to be used in ReadBytes/WriteBytes
                                              comboBox_TagID.SelectedIndex = 0
                                              If comboBox_TagID.Items.Count > 1 Then
                                                  comboBox_TagID.IsEnabled = True
                                              End If
                                          End If
                                      End Sub)
                End If
                If TypeOf _docResult.ResultInfo Is UhfScanResultInfo Then
                    'Result to Identify - InterfaceType = 868
                    Dim uhfResult As UhfScanResultInfo = _docResult.ResultInfo
                    Dispatcher.Invoke(Sub()
                                          Dim toLog As String = String.Format("{0} (Duration: {1})", _docResult.Timestamp, span) + Environment.NewLine
                                          toLog += "- UHF ScanResult -" + Environment.NewLine
                                          toLog += "  Ant" + vbTab + "UII-Bytes" + Environment.NewLine

                                          For Each tsi As UHF_TagScanInfo In uhfResult.TagInfoList
                                              Dim tagIdStr = BitConverter.ToString(tsi.UII.UII)
                                              toLog += String.Format("  {0}{1}{2}", tsi.AntennaNumber, vbTab, tagIdStr) + Environment.NewLine
                                              Dim alreadyInComboBox = False
                                              For Each str As String In comboBox_TagID.Items
                                                  If str.CompareTo(tagIdStr) = 0 Then
                                                      alreadyInComboBox = True
                                                      Exit For
                                                  End If
                                              Next
                                              If Not alreadyInComboBox Then
                                                  comboBox_TagID.Items.Add(tagIdStr)
                                                  'Select first ID to be used in ReadBytes/WriteBytes
                                                  comboBox_TagID.SelectedIndex = 0
                                                  If comboBox_TagID.Items.Count > 1 Then
                                                      comboBox_TagID.IsEnabled = True
                                                  End If
                                              End If
                                          Next

                                          textBox_ThreadLog.Text += toLog
                                          textBox_ThreadLog.ScrollToEnd()
                                      End Sub)
                End If
                If TypeOf _docResult.ResultInfo Is ReadBytesResultInfo Then
                    'Result to ReadBytes
                    Dim readResult As ReadBytesResultInfo = _docResult.ResultInfo
                    If readResult.ReadResult IsNot Nothing Then
                        'Data read
                        Dispatcher.Invoke(Sub()
                                              Dim readBytesStr = BitConverter.ToString(readResult.ReadResult)
                                              Dim toLog As String = String.Format("{0} (Duration: {1})", _docResult.Timestamp, span) + Environment.NewLine
                                              toLog += "- ReadBytesResult -" + Environment.NewLine
                                              toLog += vbTab + String.Format("Page: {0}", readResult.FromPage) + Environment.NewLine
                                              toLog += vbTab + String.Format("StartByte: {0}, Length: {1}", readResult.FromByte, readResult.ReadResult.Length) + Environment.NewLine
                                              toLog += String.Format("  DataRead: {0}", readBytesStr) + Environment.NewLine
                                              textBox_ThreadLog.Text += toLog
                                              textBox_ThreadLog.ScrollToEnd()

                                              textBox_DataRead.Text = readBytesStr
                                          End Sub)
                    End If

                    If _docResult.ProcessFinished Then ThreadProcessFinished()
                End If
                If TypeOf _docResult.ResultInfo Is WriteBytesResultInfo Then
                    'Result to WriteBytes
                    Dim writeResult As WriteBytesResultInfo = _docResult.ResultInfo
                    If writeResult.WriteResult Then
                        'Data written
                        Dispatcher.Invoke(Sub()
                                              Dim writtenBytesStr = BitConverter.ToString(writeResult.WrittenBytes)
                                              Dim toLog As String = String.Format("{0} (Duration: {1})", _docResult.Timestamp, span) + Environment.NewLine
                                              toLog += "- WriteBytesResult -" + Environment.NewLine
                                              toLog += vbTab + String.Format("Page: {0}", writeResult.FromPage) + Environment.NewLine
                                              toLog += vbTab + String.Format("StartByte: {0}, Length: {1}", writeResult.FromByte, writeResult.WrittenBytes.Length) + Environment.NewLine
                                              toLog += String.Format("  DataWritten: {0}", writtenBytesStr) + Environment.NewLine
                                              textBox_ThreadLog.Text += toLog
                                              textBox_ThreadLog.ScrollToEnd()
                                          End Sub)
                    End If

                    If _docResult.ProcessFinished Then ThreadProcessFinished()
                End If
            Else
                If _docResult.ProcessFinished Then
                    ThreadProcessFinished()
                    Exit Sub
                End If
                Dispatcher.Invoke(Sub()
                                      textBox_ThreadLog.Text += String.Format("{0} (Duration: {1}{2} ResultInfo = NULL{2}", _docResult.Timestamp, span, Environment.NewLine)
                                      textBox_ThreadLog.ScrollToEnd()
                                  End Sub)
            End If
            If _docResult.ResultException IsNot Nothing Then
                Dispatcher.Invoke(Sub()
                                      textBox_ThreadLog.Text += String.Format("{0} (Duration: {1}{2} Result: Exception{2}", _docResult.Timestamp, span, Environment.NewLine)
                                      textBox_ThreadLog.ScrollToEnd()
                                  End Sub)
                System.Diagnostics.Debug.WriteLine(_docResult.ResultException.ToString())
            End If
        End If
    End Sub

    Private Sub ThreadProcessFinished()
        Dispatcher.Invoke(Sub()
                              progressBarThread.IsIndeterminate = False
                          End Sub)
    End Sub

    Private Sub Button_Identify_Click(sender As Object, e As RoutedEventArgs)
        'Identify --> Search for transponders
        comboBox_TagID.IsEnabled = False
        comboBox_TagID.Items.Clear()

        If m_DocInterface IsNot Nothing Then
            If m_DocInterface.IsInitialized Then
                'This function starts the "Identify" process in a new thread, and reports the result using "DocResultChanged" Event
                '
                ' Parameters:
                '  _repeatCount --> number of times "Identify" will be executed internally
                '  _delayBetweenSearchMs --> number of milliseconds to wait between internal calls to "Identify"
                '  _notifySuccessOnly --> if true, "DocResultChanged" Event will only be raised by success on internal "Identify" calls
                m_DocInterface.StartIdentify(5, 0, False)
                textBox_ThreadLog.Text += Environment.NewLine + " = StartIdentify =" + Environment.NewLine
                textBox_ThreadLog.ScrollToEnd()
                progressBarThread.IsIndeterminate = True
                mLastDocResultTimestamp = DateTime.Now
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
                'This function starts the "ReadBytes" process in a new thread, and reports the result using "DocResultChanged" Event
                m_DocInterface.StartReadBytes(tagID, page, from, length)
                textBox_ThreadLog.Text += Environment.NewLine + " = StartReadBytes =" + Environment.NewLine
                textBox_ThreadLog.ScrollToEnd()
                progressBarThread.IsIndeterminate = True
                mLastDocResultTimestamp = DateTime.Now
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
                'This functions starts the "WriteBytes" process in a new thread, and reports the result using "DocResultChanged" event
                m_DocInterface.StartWriteBytes(tagID, page, from, dataToWrite)
                textBox_ThreadLog.Text += Environment.NewLine + " = WriteBytes =" + Environment.NewLine
                textBox_ThreadLog.ScrollToEnd()
                progressBarThread.IsIndeterminate = True
                mLastDocResultTimestamp = DateTime.Now
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
