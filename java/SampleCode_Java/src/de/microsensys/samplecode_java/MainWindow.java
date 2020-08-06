package de.microsensys.samplecode_java;

import java.awt.EventQueue;
import java.awt.GridLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.WindowEvent;
import java.awt.event.WindowListener;
import java.util.Enumeration;

import javax.swing.AbstractButton;
import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.SpringLayout;
import javax.swing.SwingUtilities;
import javax.swing.border.EmptyBorder;
import javax.swing.text.DefaultCaret;

import de.microsensys.exceptions.MssException;
import de.microsensys.functions.RFIDFunctions;
import de.microsensys.utils.InterfaceTypeEnum;
import de.microsensys.utils.PortTypeEnum;
import de.microsensys.utils.ProtocolTypeEnum;
import de.microsensys.utils.ReaderIDInfo;
import de.microsensys.utils.SystemMaskEnum;

public class MainWindow extends JFrame implements ActionListener {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1167251094000250767L;
	
	private final String actionClear = "clearText";
	private final String actionDisconnect = "disconnect";
	private final String actionConnect = "connect";
	private final String actionReaderId = "readReaderId";
	private final String actionIdentify = "identify";
	private final String actionReadBytes = "readBytes";
	private final String actionWriteBytes = "writeBytes";
	
	private JPanel contentPane;
	
	private JButton buttonClearText;
	private JButton buttonDisconnect;
	private ButtonGroup radioButtonGroupPort;
	private JRadioButton radioButtonSer;
	private JRadioButton radioButtonBt;
	private ButtonGroup radioButtonGroupFreq;
	private JRadioButton radioButtonHf;
	private JRadioButton radioButtonUhf;
	private JRadioButton radioButtonLegic;
	private JButton buttonConnect;
	private JTextField textFieldReaderName; //TODO Loaded with example name in initialization
	private JButton buttonReaderID;
	private JButton buttonIdentify;
	private JButton buttonReadBytes;
	private JButton buttonWriteBytes;
	private JCheckBox checkBoxLegicFs;
	private JLabel labelPageTitle;
	private JTextField textFieldPageNum;
	private JTextArea textAreaResults;
	
	private CheckConnectingReader mCheckThread;
	
	RFIDFunctions reader;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					MainWindow frame = new MainWindow();
					frame.addWindowListener(new WindowListener() {

						@Override
						public void windowOpened(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowClosing(WindowEvent e) {
							frame.closeCommunication();
						}

						@Override
						public void windowClosed(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowIconified(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowDeiconified(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowActivated(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}

						@Override
						public void windowDeactivated(WindowEvent e) {
							// TODO Auto-generated method stub
							
						}
						
					});
					frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the frame.
	 */
	public MainWindow() {
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setBounds(100, 100, 450, 500);
		
		SpringLayout layout;
		
		layout = new SpringLayout();
		contentPane = new JPanel();
		contentPane.setBorder(new EmptyBorder(5, 5, 5, 5));
		contentPane.setLayout(layout);
		setContentPane(contentPane);
		
		buttonClearText = new JButton("CLEAR TEXT");
		buttonClearText.setActionCommand(actionClear);
		buttonClearText.addActionListener(this);
		buttonDisconnect = new JButton("DISCONNECT");
		buttonDisconnect.setActionCommand(actionDisconnect);
		buttonDisconnect.addActionListener(this);
		radioButtonSer = new JRadioButton("Serial");
		radioButtonBt = new JRadioButton("Bluetooth");
		radioButtonGroupPort = new ButtonGroup();
		radioButtonGroupPort.add(radioButtonSer);
		radioButtonGroupPort.add(radioButtonBt);
		radioButtonSer.setSelected(true);
		radioButtonHf = new JRadioButton("HF");
		radioButtonUhf = new JRadioButton("UHF");
		radioButtonLegic = new JRadioButton("LEGIC");
		radioButtonGroupFreq = new ButtonGroup();
		radioButtonGroupFreq.add(radioButtonHf);
		radioButtonGroupFreq.add(radioButtonUhf);
		radioButtonGroupFreq.add(radioButtonLegic);
		radioButtonHf.setSelected(true);
		buttonConnect = new JButton("CONNECT");
		buttonConnect.setActionCommand(actionConnect);
		buttonConnect.addActionListener(this);
		textFieldReaderName = new JTextField("COM12"); //TODO Example for Windows
		//textFieldReaderName = new JTextField("dev/ttyUSB0"); //TODO Example for Linux
		//textFieldReaderName = new JTextField("/dev/tty.usbserial-00000001"); //TODO Example for macOS
		buttonReaderID = new JButton("READER ID");
		buttonReaderID.setActionCommand(actionReaderId);
		buttonReaderID.addActionListener(this);
		buttonIdentify = new JButton("IDENTIFY");
		buttonIdentify.setActionCommand(actionIdentify);
		buttonIdentify.addActionListener(this);
		buttonReadBytes = new JButton("READ BYTES");
		buttonReadBytes.setActionCommand(actionReadBytes);
		buttonReadBytes.addActionListener(this);
		buttonWriteBytes = new JButton("WRITE BYTES");
		buttonWriteBytes.setActionCommand(actionWriteBytes);
		buttonWriteBytes.addActionListener(this);
		checkBoxLegicFs = new JCheckBox("LEGIC FS");
		labelPageTitle = new JLabel("Seg./Page:");
		textFieldPageNum = new JTextField("3");
		textAreaResults = new JTextArea();
		JScrollPane scrollPane = new JScrollPane(textAreaResults);
		scrollPane.setVerticalScrollBarPolicy(JScrollPane.VERTICAL_SCROLLBAR_ALWAYS);
		DefaultCaret caret = (DefaultCaret)textAreaResults.getCaret();
		caret.setUpdatePolicy(DefaultCaret.ALWAYS_UPDATE);
		textAreaResults.setEditable(false);
		textAreaResults.setLineWrap(true);
		textAreaResults.setWrapStyleWord(true);
		
		JPanel topPanel = new JPanel();
		topPanel.setLayout(new GridLayout(0,2));
		topPanel.add(buttonClearText);
		topPanel.add(buttonDisconnect);
		contentPane.add(topPanel);
		layout.putConstraint(SpringLayout.NORTH, topPanel, 5, SpringLayout.NORTH, contentPane);
		layout.putConstraint(SpringLayout.WEST, topPanel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, topPanel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top2Panel = new JPanel();
		top2Panel.setLayout(new GridLayout(0,2));
		top2Panel.add(radioButtonSer);
		top2Panel.add(radioButtonBt);
		contentPane.add(top2Panel);
		layout.putConstraint(SpringLayout.NORTH, top2Panel, 5, SpringLayout.SOUTH, topPanel);
		layout.putConstraint(SpringLayout.WEST, top2Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top2Panel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top3Panel = new JPanel();
		top3Panel.setLayout(new GridLayout(0,3));
		top3Panel.add(radioButtonHf);
		top3Panel.add(radioButtonUhf);
		top3Panel.add(radioButtonLegic);
		contentPane.add(top3Panel);
		layout.putConstraint(SpringLayout.NORTH, top3Panel, 5, SpringLayout.SOUTH, top2Panel);
		layout.putConstraint(SpringLayout.WEST, top3Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top3Panel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top4Panel = new JPanel();
		top4Panel.setLayout(new GridLayout(0,2));
		top4Panel.add(buttonConnect);
		top4Panel.add(textFieldReaderName);
		contentPane.add(top4Panel);
		layout.putConstraint(SpringLayout.NORTH, top4Panel, 5, SpringLayout.SOUTH, top3Panel);
		layout.putConstraint(SpringLayout.WEST, top4Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top4Panel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top5Panel = new JPanel();
		top5Panel.setLayout(new GridLayout(0,2));
		top5Panel.add(buttonReaderID);
		top5Panel.add(buttonIdentify);
		contentPane.add(top5Panel);
		layout.putConstraint(SpringLayout.NORTH, top5Panel, 5, SpringLayout.SOUTH, top4Panel);
		layout.putConstraint(SpringLayout.WEST, top5Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top5Panel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top6Panel = new JPanel();
		top6Panel.setLayout(new GridLayout(0,2));
		top6Panel.add(buttonReadBytes);
		top6Panel.add(buttonWriteBytes);
		contentPane.add(top6Panel);
		layout.putConstraint(SpringLayout.NORTH, top6Panel, 5, SpringLayout.SOUTH, top5Panel);
		layout.putConstraint(SpringLayout.WEST, top6Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top6Panel, -5, SpringLayout.EAST, contentPane);
		
		JPanel top7Panel = new JPanel();
		top7Panel.setLayout(new GridLayout(0,3));
		top7Panel.add(checkBoxLegicFs);
		top7Panel.add(labelPageTitle);
		top7Panel.add(textFieldPageNum);
		contentPane.add(top7Panel);
		layout.putConstraint(SpringLayout.NORTH, top7Panel, 5, SpringLayout.SOUTH, top6Panel);
		layout.putConstraint(SpringLayout.WEST, top7Panel, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, top7Panel, -5, SpringLayout.EAST, contentPane);
		
		contentPane.add(scrollPane);
		layout.putConstraint(SpringLayout.NORTH, scrollPane, 5, SpringLayout.SOUTH, top7Panel);
		layout.putConstraint(SpringLayout.WEST, scrollPane, 5, SpringLayout.WEST, contentPane);
		layout.putConstraint(SpringLayout.EAST, scrollPane, -5, SpringLayout.EAST, contentPane);
		layout.putConstraint(SpringLayout.SOUTH, scrollPane, -5, SpringLayout.SOUTH, contentPane);
	}
	
	protected void closeCommunication() {
        if (reader != null){
            if (reader.isConnected())
                reader.terminate();
        }
	}
	
	void setEnabledRadioButtons(ButtonGroup _bg, boolean _enabled) {
		Enumeration<AbstractButton> en = _bg.getElements();
		while(en.hasMoreElements()) {
			((JRadioButton)en.nextElement()).setEnabled(_enabled);
		}
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		switch(e.getActionCommand()) {
		case actionClear:
			textAreaResults.setText("");
			break;
		case actionDisconnect:
			disconnect();
			break;
		case actionConnect:
			connect();
			break;
		case actionReaderId:
			readerId();
			break;
		case actionIdentify:
			identify();
			break;
		case actionReadBytes:
			readBytes();
			break;
		case actionWriteBytes:
			writeBytes();
			break;
		}
	}
	
	private void connect() {
		textAreaResults.setText("");
		
		//Check if the reader was already connected
		if (reader != null) {
			if (reader.isConnected()) {
				//Already connected --> call "disconnect" first
				textAreaResults.append("Disconnect first.\n");
				return;
			}
		}
		
		//Initialize RFIDFunctions
		int portType = PortTypeEnum.Bluetooth;
		if (radioButtonSer.isSelected())
			portType = PortTypeEnum.Serial;
		reader = new RFIDFunctions(portType);
		//Set the COM-Port name
		reader.setPortName(textFieldReaderName.getText().toString());
		
		//Set Protocol Type & Interface Type (according to selection in UI)
		if (radioButtonHf.isSelected()) {
			//Default Protocol type for HF is "3000"
			reader.setProtocolType(ProtocolTypeEnum.Protocol_3000);
			reader.setInterfaceType(InterfaceTypeEnum.HF);
		}
		if (radioButtonUhf.isSelected()) {
			//Default Protocol type for UHF is "v4"
			reader.setProtocolType(ProtocolTypeEnum.Protocol_v4);
			reader.setInterfaceType(InterfaceTypeEnum.UHF);
		}
		if (radioButtonLegic.isSelected()) {
			//Default Protocol type for LEGIC is "LEGIC"
			reader.setProtocolType(ProtocolTypeEnum.Protocol_LEGIC);
			reader.setInterfaceType(InterfaceTypeEnum.HF);
			
			//LEGIC FS only supported for LEGIC Protocol
			checkBoxLegicFs.setEnabled(true);
		}
		else {
			checkBoxLegicFs.setEnabled(false);
			checkBoxLegicFs.setSelected(false);
		}
		
		//Update UI accordingly
		buttonDisconnect.setEnabled(true);
		buttonConnect.setEnabled(false);
		setEnabledRadioButtons(radioButtonGroupFreq, false);
		setEnabledRadioButtons(radioButtonGroupPort, false);
		textFieldReaderName.setEnabled(false);
		textAreaResults.append("Connecting...");
		
		try {
			//Once the instance is configured, call "initialize" to connect / open the communication port
			reader.initialize();
			
			//"initialize" just starts the process. Used Thread to check if the connection procedure has finished, and the result
			startCheckConnectingThread();
		} catch (MssException e) {
			//Exception thrown by "initialize" if something was wrong
			e.printStackTrace();
			textAreaResults.append("Initialize Exception: " + e.toString());
		}
	}

	private void disconnect() {
		//Check the instance is initialized
		if (reader != null){
            if (reader.isConnected()){
            	//If it is connected --> call "terminate" to close the communication
                textAreaResults.append("Disconnecting. \n");
                reader.terminate();
            }
            else textAreaResults.append("Not connected. \n");
        }
        else textAreaResults.append("Error initializing variable \"reader\" \n");

		//Update UI accordingly
        buttonDisconnect.setEnabled(false);
        buttonConnect.setEnabled(true);
        setEnabledRadioButtons(radioButtonGroupFreq, true);
		setEnabledRadioButtons(radioButtonGroupPort, true);
        textFieldReaderName.setEnabled(true);
	}
	
	private void readerId() {
		//Check the instance is initialized
		if (reader!=null){
            if (reader.isConnected()){
            	//Reader connected
            	textAreaResults.append("readerID \n");
            	
            	//Reader the Reader ID
                ReaderIDInfo result = reader.readReaderID();
                if (result!=null){
                	//Reader ID is successfully read
                	textAreaResults.append(result.toHexString() + "\n");
                	textAreaResults.append(result.toString() + "\n");
                }
                else textAreaResults.append("Error reading \"Reader ID\" \n");
            }
            else textAreaResults.append("Not connected. \n");
        }
        else textAreaResults.append("Error initializing variable \"reader\"");
	}
	
	private void identify() {
		textAreaResults.append("\n\n/----------/\n");
		//Check the instance is initialized
        if (reader!=null){
            if (reader.isConnected()){
            	//Reader connected
            	textAreaResults.append("Identify \n");
                byte[] UID;
                try {
                	//Scan for a transponder identifier
                    UID = reader.identify();
                    if (UID!=null){
                    	//Transponder found --> Show hexadecimal representation of identifier
                    	textAreaResults.append("UID found... (Hexadecimal):\n  ");
                        for (byte aUID : UID) {                //For each byte in the byte array
                        	textAreaResults.append(String.format("%X", aUID) + " "); //Convert bytes into a Hexadecimal String and write them.
                        }
                        textAreaResults.append("\n");
                    }
                    else textAreaResults.append("No TAG found. \n");
                } catch (Exception e) {
                    e.printStackTrace();
                    textAreaResults.append(e.toString() + "\n");
                }
            }
            else textAreaResults.append("Not connected. \n");
        }
        else textAreaResults.append("Error initializing variable \"reader\" \n");
	}

	private void readBytes() {
		textAreaResults.append("\n\n/----------/\n");
		//Check the instance is initialized
        if (reader!=null){
            if (reader.isConnected()){
            	//Reader connected

            	//Set / clear File System Mask depending of state of CheckBox
            	if (checkBoxLegicFs.isSelected())
                    reader.setSystemMask(reader.getSystemMask() | SystemMaskEnum.GROUP_LEGIC_FS);
                else
                    reader.setSystemMask(reader.getSystemMask() & ~SystemMaskEnum.GROUP_LEGIC_FS);
            	//Set Page Number (Only supported for LEGIC FS and UHF)
                reader.setPage(Integer.parseInt(textFieldPageNum.getText().toString()));

                textAreaResults.append("Read 16 bytes:\n");
                byte[] UID;
                try {
                	//Scan for a transponder identifier
                    UID = reader.identify();
                    if (UID!=null){
                    	//Transponder found
                        textAreaResults.append("UID found... (Hexadecimal):\n  ");
                        for (byte aUID : UID) {
                            textAreaResults.append(String.format("%X", aUID) + " "); //Print TAG UID
                        }
                        textAreaResults.append("\n");
                        
                        //Read data from the transponder memory (for example Bytes 0-15)
                        byte[] data = reader.readBytes(UID, 0, 16);
                        if (data!=null){
                        	//Data read from transponder --> Show Hexadecimal representation
                            textAreaResults.append("/--/\n16 bytes of data read... (Hexadecimal):\n  ");
                            for (byte aData : data) {
                                textAreaResults.append(String.format("%X", aData) + " ");
                            }
                            textAreaResults.append("\n");
                        }
                        else textAreaResults.append("Error reading.\n");
                    }
                    else textAreaResults.append("No TAG near the Reader. \n");
                } catch (Exception e1) {
                    e1.printStackTrace();
                    textAreaResults.append(e1.toString() + "\n");
                }

            }
            else textAreaResults.append("Not connected. \n");
        }
        else textAreaResults.append("Error initializing variable \"reader\" \n");
	}
	
	private void writeBytes() {
		textAreaResults.append("\n\n/----------/\n");
		//Check the instance is initialized
        if (reader!=null){
            if (reader.isConnected()){
            	//Reader connected
            	//Set / clear File System Mask
            	if (checkBoxLegicFs.isSelected())
                    reader.setSystemMask(reader.getSystemMask() | SystemMaskEnum.GROUP_LEGIC_FS);
                else
                    reader.setSystemMask(reader.getSystemMask() & ~SystemMaskEnum.GROUP_LEGIC_FS);
                //Set Page Number (Only supported for LEGIC FS and UHF)
                reader.setPage(Integer.parseInt(textFieldPageNum.getText().toString()));
                textAreaResults.append("Trying to write 16 bytes: 1234567890123456\n");

                byte[] UID;
                try {
                	//Scan for a transponder identifier
                    UID = reader.identify();
                    if (UID!=null){
                        textAreaResults.append("UID found... (Hexadecimal):\n  ");
                        for (byte aUID : UID) {
                            textAreaResults.append(String.format("%X", aUID) + " "); //Print TAG UID
                        }
                        textAreaResults.append("\n");

                        //Prepare data to write
                        String auxstr = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
                        byte[] dataaux = auxstr.getBytes(); 		//Get bytes from the String
                        byte[] data = new byte[16]; 				//Create the byte array where the data will be saved
                        for (int i=0;i<data.length;i++){ 			//Save the data in the byte array
                            if (i<dataaux.length) data[i]=dataaux[i];
                            else data[i]=0x00; 						//if there is not enough bytes, fill byte array with zeros
                        }
                        textAreaResults.append("Data to write: ");
                        for (byte aData : data) {    //For each byte in the byte array
                            textAreaResults.append(String.format("%X", aData) + " ");//Convert bytes into a Hexadecimal String and write them.
                        }
                        textAreaResults.append("\n");
                        
                        //Try to write from Byte 0, 16 bytes into a TAG memory
                        if (reader.writeBytes(UID, 0, data, false)){
                            textAreaResults.append("Data written succesfully.\n");
                        }
                        else textAreaResults.append("Error writing. \n");
                    }
                    else textAreaResults.append("No TAG near the Reader. \n");
                } catch (Exception e1) {
                    e1.printStackTrace();
                    textAreaResults.append(e1.toString() + "\n");
                }
            }
            else textAreaResults.append("Not connected. \n");
        }
        else textAreaResults.append("Error initializing variable \"reader\" \n");
	}
	
	public void startCheckConnectingThread(){
        if (mCheckThread!=null){
            mCheckThread.cancel();
            mCheckThread=null;
        }
        mCheckThread = new CheckConnectingReader();
        mCheckThread.start();
    }
    private class CheckConnectingReader extends Thread {
        private boolean loop;

        CheckConnectingReader(){
            loop = true;
        }

        @Override
        public void run() {
            while (loop){
                if (reader.isConnecting()){
                    //Still trying to connect -> Wait and continue
                    try {
                        Thread.sleep(200);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                    SwingUtilities.invokeLater(new Runnable() {
                    	public void run() {
                    		textAreaResults.append(".");
                    	}
                    });
                    continue;
                }
                //Connecting process finished! Check if connected or not connected
                if (reader.isConnected()) {
                	//Connection established
                	SwingUtilities.invokeLater(new Runnable() {
                    	public void run() {
                    		textAreaResults.append("\n CONNECTED \n");
                    	}
                    });
                }
                else {
                	//Connect failed, couldn't connect to RFID reader
                	SwingUtilities.invokeLater(new Runnable() {
                    	public void run() {
                    		textAreaResults.append("\n Reader NOT connected \n  --> PRESS DISCONNECT BUTTON");
                    	}
                    });
                }

                //Stop this thread
                cancel();
            }
        }

        void cancel(){
            loop = false;
        }
    }
}
