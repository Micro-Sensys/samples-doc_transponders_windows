﻿using iIDReaderLibrary;
using iIDReaderLibrary.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SampleThreads_CSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker m_Worker = new BackgroundWorker();
        private bool m_ReaderFound = false;

        DocInterfaceControl m_DocInterface = null;

        public MainWindow()
        {
            InitializeComponent();

            SetUiEnabled(false, 0);
            m_Worker.WorkerReportsProgress = true;
            m_Worker.WorkerSupportsCancellation = true;
            m_Worker.DoWork += Worker_DoWork;

            textBlockInitialize_DriverVersion.Text = DocInterfaceControl.LibraryVersion;
            textBlock_ReaderInfo.Text = "Library version: " + textBlockInitialize_DriverVersion.Text + "  - Waiting for Initialize";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Stop background worker
            m_Worker.CancelAsync();
            if (m_DocInterface != null)
            {
                m_DocInterface.Terminate();
            }
        }

        private void SetUiEnabled(bool _enabled, int _readerID)
        {
            Dispatcher.Invoke(() =>
            {
                tabItem_Std.IsEnabled = _enabled;
                tabItem_SystemMask.IsEnabled = _enabled;

                if (_enabled)
                {
                    if (_readerID > 0)
                        textBlock_ReaderInfo.Text = "ReaderID: " + _readerID;
                }
                else
                {
                    tabControl.SelectedIndex = 0;
                    textBlock_ReaderInfo.Text = "Communication lost/not possible  - Waiting for Initialize";
                    textBlock_Status.Text = "";
                }
            });
        }
        private void InitUiEventHandlers()
        {
            //SystemMask selected
            checkBox_SystemMask_Hf_ISO15693.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_iIDL.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_iIDD.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_iIDG.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_iIDN.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ICODE_UID.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_LEGIC.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ICODE_1.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ISO14443A.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_PicoTAG.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_iIDP.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ISO14443B.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ISO15693_adressed.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_ISO15693_RWblocks.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_LEGIC_FileSystem.Checked += CheckBox_SystemMask_Checked;
            checkBox_SystemMask_Hf_DualEcho.Checked += CheckBox_SystemMask_Checked;
            //SystemMask unselected
            checkBox_SystemMask_Hf_ISO15693.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_iIDL.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_iIDD.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_iIDG.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_iIDN.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ICODE_UID.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_LEGIC.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ICODE_1.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ISO14443A.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_PicoTAG.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_iIDP.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ISO14443B.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ISO15693_adressed.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_ISO15693_RWblocks.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_LEGIC_FileSystem.Unchecked += CheckBox_SystemMask_Unchecked;
            checkBox_SystemMask_Hf_DualEcho.Unchecked += CheckBox_SystemMask_Unchecked;
        }

        #region Initialize
        private void ButtonInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (m_DocInterface != null)
            {
                //First dispose previous instance
                m_DocInterface.Terminate();
                m_DocInterface = null;
            }
            //Get Interface parameters and initialize class
            try
            {
                //Port type --> Get from UI
                //  0 = Serial
                //  2 = Bluetooth
                //  4 = USB
                byte portType = 4; //Default USB
                if (radioButtonInitialize_PortSerial.IsChecked.Value)
                    portType = 0;
                if (radioButtonInitialize_PortBt.IsChecked.Value)
                    portType = 2;

                var readerPortSettings = InterfaceCommunicationSettings.GetForSerialDevice(portType, textBoxInitialize_PortName.Text);
                //Interface Type --> Get from UI
                //  1356 = 13.56MHz (HF)
                //  868 = 868MHz (UHF)
                int interfaceType = 1356;
                if (radioButtonInitialize_Interface1356.IsChecked.Value)
                    interfaceType = 1356;
                if (radioButtonInitialize_Interface868.IsChecked.Value)
                    interfaceType = 868;

                //Initialize class. Then call "initialize"
                m_DocInterface = new DocInterfaceControl(readerPortSettings, interfaceType);

                //Init CheckBoxes
                InitSystemMaskCheckBoxes(m_DocInterface.SystemMask);
                InitUiEventHandlers();

                //Initialize
                textBlockInitialize_ParamInterfaceType.Text = "InterfaceType: " + interfaceType;
                textBlockInitialize_ParamPortType.Text = "PortType: " + portType;
                textBlockInitialize_ParamPortName.Text = "PortName: " + textBoxInitialize_PortName.Text;

                m_DocInterface.InitializeCompleted += DocInterface_InitializeCompleted;
                m_DocInterface.DocResultChanged += DocInterface_DocResultChanged;
                textBlock_ReaderInfo.Text = "Calling Initialize";
                m_DocInterface.StartInitialize();
            }
            catch
            {
                //TODO catch exception & notify
            }
        }

        private void DocInterface_InitializeCompleted(object _sender, bool _portOpen)
        {
            Dispatcher.Invoke(() =>
            {
                if (_portOpen)
                {
                    //Initialize worked --> Enable UI & enable BackgroundWorker to check Reader-ID
                    textBlock_ReaderInfo.Text = "Initialize Result: True";
                    if (m_Worker.IsBusy != true)
                    {
                        // Start the asynchronous operation.
                        m_Worker.RunWorkerAsync();
                    }
                }
                else
                {
                    //Initialize didn't work --> disable UI
                    SetUiEnabled(false, 0);
                    textBlock_ReaderInfo.Text = "Initialize Result: False";
                }
            });
        }

        private void ButtonTerminate_Click(object sender, RoutedEventArgs e)
        {
            //Stop background worker
            m_Worker.CancelAsync();

            if (m_DocInterface != null)
            {
                m_DocInterface.Terminate();
                //m_DocInterface = null;
            }
            m_ReaderFound = false;
            SetUiEnabled(false, 0);
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int readerCheckFailCount = 0;
            //Check readerID in 5 seconds interval
            TimeSpan readerCheckSpan = TimeSpan.FromSeconds(5);
            //Initialize lastChecked in the past --> when Worker started it should check for the ReaderID first of all
            DateTime lastCheckedOk = DateTime.UtcNow.AddMinutes(-1);

            //While port initialized:
            //  Check reader communication is still possible in 5 seconds interval
            //      Hint: This interval is not fixed! It is recomended to check the communication with the reader when no other operation is executed.
            //          => Most important for battery powered devices!
            while (m_DocInterface.IsInitialized)
            {
                if (worker.CancellationPending == true)
                {
                    //Exit loop if Background worker is cancelled
                    e.Cancel = true;
                    break;
                }
                else
                {
                    if ((DateTime.UtcNow - lastCheckedOk) < readerCheckSpan)
                    {
                        //Next check time still not reached --> just do nothing
                        System.Threading.Thread.Sleep(200);
                        continue;
                    }
                    else
                    {
                        //Next check time reached --> check ReaderID

                        var readerInfo = m_DocInterface.ReadReaderID();
                        if (readerInfo != null)
                        {
                            //ReaderID check OK
                            readerCheckFailCount = 0;
                            lastCheckedOk = DateTime.UtcNow;
                            if (!m_ReaderFound)
                            {
                                //Not previously found --> Enable functions
                                m_ReaderFound = true;
                                SetUiEnabled(true, readerInfo.ReaderID); //TODO show more info? Also HW info?
                            }
                        }
                        else
                        {
                            //ReaderID check failed
                            readerCheckFailCount++;
                            if (readerCheckFailCount > 5)
                            {
                                //Reader Check failed multiple times
                                if (m_ReaderFound)
                                {
                                    //Previously found --> Asume Reader is lost!
                                    m_ReaderFound = false;
                                    SetUiEnabled(false, 0);
                                    return;
                                }
                            }
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
            }
        }
        #endregion

        DateTime mLastDocResultTimestamp;
        private void DocInterface_DocResultChanged(object sender, DocResultEventArgs _docResult)
        {
            if (_docResult != null)
            {
                TimeSpan span = DateTime.Now - mLastDocResultTimestamp;
                if (_docResult.ResultInfo != null)
                {
                    if (_docResult.ResultInfo is HfScanResultInfo)
                    {
                        //Result to Identify - InterfaceType = 1356
                        Dispatcher.Invoke(() =>
                        {
                            //Update result in UI - For demo purposes done using "Dispatcher.Invoke"
                            string tagIdStr = BitConverter.ToString((_docResult.ResultInfo as HfScanResultInfo).TagID);
                            string toLog = string.Format("{0} (Duration: {1})\n", _docResult.Timestamp, span);
                            toLog += "- HF ScanResult -\n";
                            toLog += string.Format("\t{0}", tagIdStr);
                            toLog += "\n";
                            textBox_ThreadLog.Text += toLog;
                            textBox_ThreadLog.ScrollToEnd();

                            bool alreadyInComboBox = false;
                            foreach(string str in comboBox_TagID.Items)
                            {
                                if (str.CompareTo(tagIdStr) == 0)
                                {
                                    alreadyInComboBox = true;
                                    break;
                                }
                            }
                            if (!alreadyInComboBox)
                            {
                                comboBox_TagID.Items.Add(tagIdStr);
                                //Select first ID to be used in ReadBytes/WriteBytes
                                comboBox_TagID.SelectedIndex = 0;
                                if (comboBox_TagID.Items.Count > 1)
                                    comboBox_TagID.IsEnabled = true;
                            }
                        });
                    }
                    if (_docResult.ResultInfo is UhfScanResultInfo)
                    {
                        //Result to Identify - InterfaceType = 868
                        Dispatcher.Invoke(() =>
                        {
                            //Update result in UI - For demo purposes done using "Dispatcher.Invoke"
                            string toLog = string.Format("{0} (Duration: {1})\n", _docResult.Timestamp, span);
                            toLog += "- UHF ScanResult -\n";
                            toLog += "  Ant\tUII-Bytes\n";

                            var uhfScanResult = (_docResult.ResultInfo as UhfScanResultInfo);
                            foreach (var tsi in uhfScanResult.TagInfoList)
                            {
                                string tagIdStr = BitConverter.ToString(tsi.UII.UII);
                                toLog += string.Format("  {0}\t{1}\n", tsi.AntennaNumber, tagIdStr);
                                bool alreadyInComboBox = false;
                                foreach (string str in comboBox_TagID.Items)
                                {
                                    if (str.CompareTo(tagIdStr) == 0)
                                    {
                                        alreadyInComboBox = true;
                                        break;
                                    }
                                }
                                if (!alreadyInComboBox)
                                {
                                    comboBox_TagID.Items.Add(tagIdStr);
                                    //Select first UII to be used in ReadBytes/WriteBytes
                                    comboBox_TagID.SelectedIndex = 0;
                                    if (comboBox_TagID.Items.Count > 1)
                                        comboBox_TagID.IsEnabled = true;
                                }
                            }

                            textBox_ThreadLog.Text += toLog;
                            textBox_ThreadLog.ScrollToEnd();
                        });
                    }
                    if (_docResult.ResultInfo is ReadBytesResultInfo)
                    {
                        //Result to ReadBytes
                        var readResult = (_docResult.ResultInfo as ReadBytesResultInfo);
                        if (readResult.ReadResult != null)
                        {
                            //Data read
                            Dispatcher.Invoke(() =>
                            {
                                //Update result in UI - For demo purposes done using "Dispatcher.Invoke"
                                string readBytesStr = BitConverter.ToString(readResult.ReadResult);
                                string toLog = string.Format("{0} (Duration: {1})\n", _docResult.Timestamp, span);
                                toLog += "- ReadBytesResult -\n";
                                toLog += string.Format("\tPage: {0}\n", readResult.FromPage);
                                toLog += string.Format("\tStartByte: {0}, Length: {1}\n", readResult.FromByte, readResult.ReadResult.Length);
                                toLog += string.Format("  DataRead: {0}", readBytesStr);
                                toLog += "\n";
                                textBox_ThreadLog.Text += toLog;
                                textBox_ThreadLog.ScrollToEnd();

                                textBox_DataRead.Text = readBytesStr;
                            });
                        }

                        if (_docResult.ProcessFinished)
                            ThreadProcessFinished();
                    }
                    if (_docResult.ResultInfo is WriteBytesResultInfo)
                    {
                        //Result to WriteBytes
                        var writeResult = (_docResult.ResultInfo as WriteBytesResultInfo);
                        if (writeResult.WriteResult)
                        {
                            //Data written
                            Dispatcher.Invoke(() =>
                            {
                                //Update result in UI - For demo purposes done using "Dispatcher.Invoke"
                                string writtenBytesStr = BitConverter.ToString(writeResult.WrittenBytes);
                                string toLog = string.Format("{0} (Duration: {1})\n", _docResult.Timestamp, span);
                                toLog += "- WriteBytesResult -\n";
                                toLog += "    WRITTEN\n";
                                toLog += string.Format("\tPage: {0}\n", writeResult.FromPage);
                                toLog += string.Format("\tStartByte: {0}, Length: {1}\n", writeResult.FromByte, writeResult.WrittenBytes.Length);
                                toLog += string.Format("  DataWritten: {0}", writtenBytesStr);
                                toLog += "\n";
                                textBox_ThreadLog.Text += toLog;
                                textBox_ThreadLog.ScrollToEnd();
                            });
                        }

                        if (_docResult.ProcessFinished)
                            ThreadProcessFinished();
                    }
                }
                else
                {
                    if (_docResult.ProcessFinished)
                    {
                        ThreadProcessFinished();
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        textBox_ThreadLog.Text += string.Format("{0} (Duration: {1})\n ResultInfo = NULL\n", _docResult.Timestamp, span);
                        textBox_ThreadLog.ScrollToEnd();
                    });
                }
                if (_docResult.ResultException != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        textBox_ThreadLog.Text += string.Format("{0} (Duration: {1})\n Result: Exception\n", _docResult.Timestamp, span);
                        textBox_ThreadLog.ScrollToEnd();
                    });
                    System.Diagnostics.Debug.WriteLine(_docResult.ResultException.ToString());
                }
                
                mLastDocResultTimestamp = DateTime.Now;
            }
        }

        private void ThreadProcessFinished()
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.IsIndeterminate = false;
            });
        }

        private void Button_Identify_Click(object sender, RoutedEventArgs e)
        {
            //Identify --> Search for transponders 
            comboBox_TagID.IsEnabled = false;
            comboBox_TagID.Items.Clear();

            if (m_DocInterface != null)
            {
                if (m_DocInterface.IsInitialized)
                {
                    //This function starts the "Identify" process in a new thread, and reports the result using "DocResultChanged" Event
                    /*
                     * Parameters:
                     *  _repeatCount --> number of times "Identify" will be executed internally
                     *  _delayBetweenSearchMs --> number of milliseconds to wait between internal calls to "Identify"
                     *  _notifySuccessOnly --> if true, "DocResultChanged" Event will only be raised by success on internal "Identify" calls
                     */
                    m_DocInterface.StartIdentify(5, 0, false);
                    textBox_ThreadLog.Text += "\n = StartIdentify =\n";
                    textBox_ThreadLog.ScrollToEnd();
                    progressBar.IsIndeterminate = true;
                    mLastDocResultTimestamp = DateTime.Now;
                }
            }
        }

        private void Button_ReadBytes_Click(object sender, RoutedEventArgs e)
        {
            textBox_DataRead.Text = "";
            if (comboBox_TagID.SelectedIndex == -1)
            {
                //For Readbytes the UID must be already read using "Identify"
                MessageBox.Show(this, "Identifier is needed --> Call Identify first");
                return;
            }
            //Get parameters from Window
            int from = int.Parse(textBox_From.Text);
            int length = int.Parse(textBox_Length.Text);
            int page = int.Parse(textBox_Page.Text);
            byte[] tagID = iIDReaderLibrary.Utils.HelperFunctions.HexConverter.ToByteArray(comboBox_TagID.Text.Replace("-", " "));

            if (m_DocInterface != null)
            {
                if (m_DocInterface.IsInitialized)
                {
                    //This function starts the "ReadBytes" process in a new thread, and reports the result using "DocResultChanged" Event
                    m_DocInterface.StartReadBytes(tagID, page, from, length);
                    textBox_ThreadLog.Text += "\n = StartReadBytes =\n";
                    textBox_ThreadLog.ScrollToEnd();
                    progressBar.IsIndeterminate = true;
                    mLastDocResultTimestamp = DateTime.Now;
                }
            }
        }

        private void Button_WriteBytes_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_TagID.SelectedIndex == -1)
            {
                //For Writebytes the UID must be already read using "Identify"
                MessageBox.Show(this, "Identifier is needed --> Call Identify first");
                return;
            }
            //Get parameters from Window
            int from = int.Parse(textBox_From.Text);
            int length = int.Parse(textBox_Length.Text);
            int page = int.Parse(textBox_Page.Text);
            byte[] tagID = iIDReaderLibrary.Utils.HelperFunctions.HexConverter.ToByteArray(comboBox_TagID.Text.Replace("-", " "));
            //Initialize bytes to write => length defined in TextBox
            byte[] dataToWrite = new byte[length];
            //Get data to write from TextBox & convert from HEX-String
            string[] aux = textBox_DataToWriteHex.Text.Split(new char[] { ' ', '-' });
            if (aux.Length < length)
            {
                MessageBox.Show(this, "Not enough data available to write in TextBox");
                return;
            }
            int j = 0;
            foreach (string str in aux)
            {
                if (str.Equals(" ")) continue;
                try
                {
                    dataToWrite[j] = Convert.ToByte(str, 16);
                }
                catch
                {
                    MessageBox.Show(this, "Error converting from HEX");
                    return;
                }
                j++;
            }

            if (m_DocInterface != null)
            {
                if (m_DocInterface.IsInitialized)
                {
                    //This functions starts the "WriteBytes" process in a new thread, and reports the result using "DocResultChanged" event
                    m_DocInterface.StartWriteBytes(tagID, page, from, dataToWrite);
                    textBox_ThreadLog.Text += "\n = StartWriteBytes =\n";
                    textBox_ThreadLog.ScrollToEnd();
                    progressBar.IsIndeterminate = true;
                    mLastDocResultTimestamp = DateTime.Now;
                }
            }
        }

        #region SystemMask
        /*
         * SystemMask
         *  System mask is used to configure which kind of transponders will be searched using "iid_functions".
         *  It consist on a series of flags that represent each of the different transponder types and extra functionalities supported
         */
        public void InitSystemMaskCheckBoxes(uint _systemMaskValue)
        {
            checkBox_SystemMask_Hf_ISO15693.IsChecked = (_systemMaskValue & 0x001) != 0;
            checkBox_SystemMask_Hf_iIDL.IsChecked = (_systemMaskValue & 0x002) != 0;
            checkBox_SystemMask_Hf_iIDD.IsChecked = (_systemMaskValue & 0x004) != 0;
            checkBox_SystemMask_Hf_iIDG.IsChecked = (_systemMaskValue & 0x008) != 0;
            checkBox_SystemMask_Hf_iIDN.IsChecked = (_systemMaskValue & 0x010) != 0;
            checkBox_SystemMask_Hf_ICODE_UID.IsChecked = (_systemMaskValue & 0x020) != 0;
            checkBox_SystemMask_Hf_LEGIC.IsChecked = (_systemMaskValue & 0x040) != 0;
            checkBox_SystemMask_Hf_ICODE_1.IsChecked = (_systemMaskValue & 0x080) != 0;
            checkBox_SystemMask_Hf_ISO14443A.IsChecked = (_systemMaskValue & 0x100) != 0;
            checkBox_SystemMask_Hf_PicoTAG.IsChecked = (_systemMaskValue & 0x200) != 0;
            checkBox_SystemMask_Hf_iIDP.IsChecked = (_systemMaskValue & 0x400) != 0;
            checkBox_SystemMask_Hf_ISO14443B.IsChecked = (_systemMaskValue & 0x800) != 0;

            checkBox_SystemMask_Hf_ISO15693_adressed.IsChecked = (_systemMaskValue & 0x10000000) != 0;
            checkBox_SystemMask_Hf_ISO15693_RWblocks.IsChecked = (_systemMaskValue & 0x20000000) != 0;
            checkBox_SystemMask_Hf_LEGIC_FileSystem.IsChecked = (_systemMaskValue & 0x40000000) != 0;
            checkBox_SystemMask_Hf_DualEcho.IsChecked = (_systemMaskValue & 0x80000000) != 0;
        }

        private void CheckBox_SystemMask_Checked(object sender, RoutedEventArgs e)
        {
            if (m_DocInterface != null)
            {
                uint currentSystemMask = m_DocInterface.SystemMask;
                uint maskValue = Convert.ToUInt32((string)(sender as CheckBox).Tag, 16);
                m_DocInterface.SystemMask = (currentSystemMask | maskValue);
            }
        }
        private void CheckBox_SystemMask_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m_DocInterface != null)
            {
                uint currentSystemMask = m_DocInterface.SystemMask;
                uint maskValue = Convert.ToUInt32((string)(sender as CheckBox).Tag, 16);
                m_DocInterface.SystemMask = (currentSystemMask & ~maskValue);
            }
        }
        #endregion
    }
}
