using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Raylib.Calibrations;
//using StingrayClassLibrary;
using SerialPortCommLib;
using CaliSysLib.Utility;
//
using Raytek.AsciiTalk.engine;
//
using Raytek.AsciiTalk.components;
using Raytek.AsciiTalk.model;
using Raytek.AsciiTalk.tools;
using Raylib.Calibrations.interfaces;

//
namespace StingrayCalibrationAPI
{
    public class StingrayCalibration : CalibrationBase
    {
    #region Variables

        private CaliParams StingrayCaliParams;
        //James added the define ,
        private SerialPortCommLib.SerialPortComm multimetercomm, calibrationboxcomm;
        /*StingraySerialPort Stingray = new StingraySerialPort(new SerialPortConfiguration()
        {
            BaudRate = 9600,
            PortName = "COM1",
            ReadTimeOut = 1000, // ms
            WriteTimeOut = 1000
        });*/
        //
        //  protected SerialPort multiMeter;
        //  protected SerialPort caliBox;

        public CalibrationResultDataPool StingrayCaliResults;
        private int FlagHighTempModel = 0, FlagP3Model = 0;//the limit range over 25C,such as ht ,mt models
        public int CalibrationprogressCounter = 0;
        //public int CalibrationWaitingTime = 0;
        public long StingraySerialNumber;
        public long[] BPSendTalbe = new long[32], PSd2SendTable = new long[32];//James Stingray
        int ADC1FilterConfig, ADC1FLT, ADC1FLTNotch, ADC1FLTRavg, ADC1SF, ADC1FLTSinc4, ADC1FLTChop; 

        private float MultimeterValue;
        //just give them readable names or at least explain
        const int TwoWiresLowCount = 468;
        const int TwoWiresHighCount = 15448;
        const int SixWiresIoutLowCount = 3106;
        const int SixWiresIoutHighCount = 14531;
        const int SixWiresVoutLowCount = 1638;
        const int SixWiresVoutHighCount = 14745;
        const int SixWiresTcLowCount = 1130;
        const int SixWiresTcHighCount = 9830;
        const int TwelveWiresFTCLowCount = 1400;
        const int TwelveWiresFTCHighCount = 14745;
        public struct CalibrationResultDataPool
        {
            //CONFIG
            public long UniqueID;
            public long SerialNumber;
            public String AnalogBoardFirmwareVersion, DignalBoardFirmwareVersion;
            public long ModelNumber;
            public int CalibrationWaitingTime;
            public string ModelName;
            public string TerminalType;
            //IR
            public double CalEmissivity;
            public double AmbTempCunit;
            public long AmbTempSunit;
            public long Alpha;
            public long AlphaLow;
            public double[] AdValueFromUnit, BlackBodyRealTemperature;
            public long[] BreakPointEnergryTable;
            public long[] DetectorEnergryTable;
            public long DetectorEnergryOffset;
            public double RespsibilityScale;
            public double SpectrumScale;
            public long TemperatureHighLimitSunit;
            public long TemperatureLowLimitSunit;
            // Analog 
            public long MAOutputLowCount;
            public long MAOutputHighCount;
            public double MAOutputLow;
            public double MAOutputHigh;
            public long MAOutputGain;
            public long MAOutputOffset;
            public long MVOutputLowCount;
            public long MVOutputHighCount;
            public double MVOutputLow;
            public double MVOutputHigh;
            public double MVOutputGain;
            public double MVOutputOffset;
            public double ThermalCoupleOutputLowCount;
            public double ThermalCoupleOutputHighCount;
            public double ThermalCoupleOutputLow;
            public double ThermalCoupleOutputHigh;
            public long ThermalCoupleOutputGain;
            public long ThermalCoupleOutputOffset;
            public long FTC1InputLowCount;
            public long FTC1InputHighCount;
            public double FTC1InputLow;
            public double FTC1InputHigh;
            public long FTC1InputGain;
            public long FTC1InputOffset;
            public long FTC2InputLowCount;
            public long FTC2InputHighCount;
            public double FTC2InputLow;
            public double FTC2InputHigh;
            public long FTC2InputGain;
            public long FTC2InputOffset;
            // Power Consumption 
            public double PowermA;
            public double PowermV;
            public double PowermW;

        }
        public DATA_POOL StingrayDataPool = new DATA_POOL();
        //      public StingrayClassLibrary.DATA_POOL StingrayDataPool = new StingrayClassLibrary.DATA_POOL();//call the lib
        public int AdFromUnit { get; set; }
        public double AmbFromUnit { get; set; }
        public int FTCLowCount { get; set; }
        public int FTCHighCount { get; set; }
        /// <summary>
        /// Comments
        /// </summary>
        public double SomeData2 { get; set; }
        public double SomeData3 { get; set; }
        public string ERRORMessage;
        CancellationToken cancellationToken = new CancellationToken();
    #endregion

        //  public StingrayCalibration(string name, string focus, string wiring, ICalibrationDevice communicationInterface, String MultimeterPortName, String CaliBoxPortName)
        //      : base(name, communicationInterface)
        //James change the ICalibrationDevice into Stin.graySerialPort from build errors,20161227
        public StingrayCalibration(string name, string focus, string wiring, ICalibrationDevice communicationInterface, String MultimeterPortName, String CaliBoxPortName)
            : base(name, communicationInterface)
        {

            //!!!check if device is supported!!!
            //if device is not supported throw new NotSupportedException() !!!!!
            //  Stingray= communicationInterface;
            if (communicationInterface is GenericDeviceInterface)
                (communicationInterface as GenericDeviceInterface).Protocol = new StingrayAsciiProtocol();
            communicationInterface.Connect();
            if (communicationInterface.IsOpen)
            {

            }
            else
            {
                Log.Error("can not connect the Stingray unit");
            }
            InitializeMultimeter(MultimeterPortName);//Initialize Multimeter Serial 
            InitializeCalibrationBox(CaliBoxPortName);//Initialize Calibration box Serial 
            SwitchDeviceIntoCalibrationMode();
            // TestCommports();
            // this.SupportedSteps  set this. 
            //SupportedSteps = CalibrationStep.CALIBRATION_DATA_WRITTEN_TO_DEVICE |
            // CalibrationStep.CALIPOINT1 |
            //  CalibrationStep.CALIPOINT2 |
            //   CalibrationStep.CALIPOINT3 |
            // Stingray.Connect();//jA
            lock (CommunicationSyncronisation) //means i want exclusive access to the communicationInterface
            {
                InitializeUnit(name, focus, wiring);

                //       var CalibrationModel = SwitchDeviceIntoCalibrationMode();
                //dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(CalibrationprogressCounter++, this));
            }
        }

    #region Switch ModelName to ModelNumber
        public long ModelNumber(string name, string focus, string wiring)
        {
            long ModelNum;
            int Detectortype = 0, FocusType = 0, TerminalType = 0;

            switch (name)
            {
                case "LTP":
                    Detectortype = 0;
                    FlagHighTempModel = 0;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "LT":
                    Detectortype = 1;
                    FlagHighTempModel = 0;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "LTH5":
                    Detectortype = 2;
                    FlagHighTempModel = 0;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "LTH7":
                    Detectortype = 3;
                    FlagHighTempModel = 0;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "G5":
                    Detectortype = 4;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 20000;//20000ms
                    break;
                case "G5H":
                    Detectortype = 5;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 20000;//20000ms
                    break;
                case "P7":
                    Detectortype = 6;
                    FlagHighTempModel = 0;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "G7":
                    Detectortype = 7;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "MT":
                    Detectortype = 8;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 20000;//20000ms
                    break;
                case "MTH":
                    Detectortype = 9;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 20000;//20000ms
                    break;
                case "MTFL":
                    Detectortype = 10;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 20000;//20000ms
                    break;
                case "HT":
                    Detectortype = 11;

                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "P3":
                    Detectortype = 12;
                    FlagHighTempModel = 1;
                    FlagP3Model = 1;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "1ML":
                    Detectortype = 13;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "1MH":
                    Detectortype = 14;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "2M":
                    Detectortype = 15;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                case "3M":
                    Detectortype = 16;
                    FlagHighTempModel = 1;
                    FlagP3Model = 0;
                    //CalibrationWaitingTime = 7000;//7000ms
                    break;
                default:
                    break;
            }
            switch (wiring)
            {
                case "S":
                    TerminalType = 0;
                    break;
                case "T":
                    TerminalType = 1;
                    break;
                case "M16":
                    TerminalType = 2;
                    break;
                case "E":
                    TerminalType = 2;
                    break;
                default:
                    break;
            }
            switch (focus)
            {
                case "SF":
                    FocusType = 0;
                    break;
                case "SF2":
                    FocusType = 1;
                    break;
                case "SF4":
                    FocusType = 2;
                    break;
                case "CF":
                    FocusType = 3;
                    break;
                case "CF1":
                    FocusType = 4;
                    break;
                case "CF2":
                    FocusType = 5;
                    break;
                default:
                    break;
            }
            ModelNum = (Detectortype << 8) + (FocusType << 4) + TerminalType;
            return ModelNum;
        }
    #endregion

        public void InitializeUnit(string name, string focus, string wiring)
        {
            StingrayCaliResults.TerminalType = wiring;//get the terminalType 
            StingrayDataPool.ModelName = "T" + name + wiring + focus;//get the combine name 
            StingrayCaliResults.ModelName = StingrayDataPool.ModelName;
            StingrayDataPool.ModelNum = ModelNumber(name, focus, wiring);//get Stingray unit ModelNumber from UI input
            StingrayCaliResults.ModelNumber = StingrayDataPool.ModelNum;
            //loade the configuration files into CaliParams
            StingrayCaliParams = new CaliParams(Environment.CurrentDirectory + "\\" + name + ".CAL");
            //load data into StingayDataPool for calibration
            InitStingrayDataPool(StingrayCaliParams);

            SwitchDeviceIntoCalibrationMode();
            //Write model name and model number
            CofigUnitBeforeAdGather();
            //Write the AD filter parameters
            getCaliParams();
            //Quit calibrating
            SwitchDeviceOutCalibrationMode();
            //ready for the AD gathering
        }

        public void InitializeMultimeter(string MultimeterPortName)
        {
            //8808A

            SerialPortCommLib.CommConfig Multimeter_cfg = new SerialPortCommLib.CommConfig();

            Multimeter_cfg.BaudRate = 9600;
            Multimeter_cfg.CommPort = MultimeterPortName;
            Multimeter_cfg.DataBit = 8;
            Multimeter_cfg.StopBit = StopBits.One;
            Multimeter_cfg.SynWaitTime = 150;
            Multimeter_cfg.ReadBufferSize = 128;
            Multimeter_cfg.WriteBufferSize = 128;
            Multimeter_cfg.ReadTimeout = 2000;
            Multimeter_cfg.WriteTimeout = 2000;
            Multimeter_cfg.ReceivedWatchInterval = 2000;
            multimetercomm = new SerialPortCommLib.SerialPortComm(Multimeter_cfg);
            multimetercomm.IsSynchronized = true;


        }
        public void InitializeCalibrationBox(string CaliBoxPortName)
        {

            //Relay 20161031 for com and Gnd ,mAout and Vout

            SerialPortCommLib.CommConfig Relay_cfg = new SerialPortCommLib.CommConfig();
            //   Relay_cfg.BaudRate = extIniFile.ReadInt("RelayCommunicationSettings", "BaudRate");
            Relay_cfg.BaudRate = 9600;
            Relay_cfg.CommPort = CaliBoxPortName;
            Relay_cfg.DataBit = 8;
            Relay_cfg.StopBit = StopBits.One;
            Relay_cfg.SynWaitTime = 150;
            Relay_cfg.ReadBufferSize = 128;
            Relay_cfg.WriteBufferSize = 128;
            Relay_cfg.ReadTimeout = 2000;
            Relay_cfg.WriteTimeout = 2000;
            Relay_cfg.ReceivedWatchInterval = 2000;
            calibrationboxcomm = new SerialPortCommLib.SerialPortComm(Relay_cfg);
            calibrationboxcomm.IsSynchronized = true;


        }
        public void TestCommports()
        {
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            var result = communicationInterface.Set("E", "1.0"); //
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "E") && result == "1.000")
            {

            }
            else
            {//didnt work

            }
            calibrationboxcomm.Connect();
            multimetercomm.Connect();
            calibrationboxcomm.SendCommand("[OPEN001]", 1);//Connect Gnd and Com of 8808A
            Thread.Sleep(1500);  //delay 1.5s
            calibrationboxcomm.SendCommand("[CLOSE001]", 1);//Disconnect Gnd and Com of 8808A
            Thread.Sleep(1500);
            calibrationboxcomm.SendCommand("[OPEN002]", 1);//Connect Gnd and Com of 8808A
            Thread.Sleep(1500);
            calibrationboxcomm.SendCommand("[CLOSE002]", 1);//Disconnect Gnd and Com of 8808A
            Thread.Sleep(1500);
            //multimetercomm.SendCommand("ADC", 1);//Connect Gnd and Com of 8808A
            //Thread.Sleep(500);
            multimetercomm.SendCommand("*IDN?", 1);
            ERRORMessage = (string)multimetercomm.ReceiveSyn();
            Thread.Sleep(1500);
            calibrationboxcomm.Disconnect();
            multimetercomm.Disconnect();

        }

        //Those all parameters in 'name'.CAL who were fixed.
        public void InitStingrayDataPool(CaliParams cp) 
        {
            //Stingray using
            StingrayDataPool.PSd2_Ppower = cp.PSd2_Ppower;

            StingrayDataPool.SampleCount = cp.SampleCount;

            StingrayDataPool.AF = cp.ADC1FLT_AF;
            StingrayDataPool.SF = cp.ADC1FLT_SF;
            StingrayDataPool.Ravg = cp.ADC1FLT_Ravg;
            StingrayDataPool.Notch = cp.ADC1FLT_Notch;
            StingrayDataPool.Chop = cp.ADC1FLT_Chop;
            StingrayDataPool.Sinc4 = cp.ADC1FLT_Sinc4;

            StingrayDataPool.AverageCo = cp.Average_Co;

            StingrayDataPool.TargetADThresJump = cp.TargetADThresJump;
            StingrayDataPool.TargetADThresStab = cp.TargetADThresStab;
            StingrayDataPool.TargetADDepthInit = cp.TargetADDepthInit;
            StingrayDataPool.TargetADdepthStep = cp.TargetADdepthStep;

            StingrayDataPool.AmbG = cp.AmbG;
            StingrayDataPool.AmbOfs = cp.AmbOfs;

            //StingrayDataPool.IGain = cp.Iout_Gain;
            //StingrayDataPool.IOfs = cp.Iout_Offset;
            
            StingrayDataPool.Alpha = cp.Alpha;
            StingrayDataPool.Alpha_Low = cp.Alpha_Low;
            StingrayDataPool.Alpha_High = cp.Alpha_PsdOfs2;

            StingrayDataPool.CalibratingWaitTime = cp.CaliWaitTime;

            StingrayDataPool.TLowF = cp.TgtRangeF.Min;
            StingrayDataPool.THighF = cp.TgtRangeF.Max;
            StingrayDataPool.DLowF = cp.DispRangeF.Min;
            StingrayDataPool.DHighF = cp.DispRangeF.Max;
            StingrayDataPool.TaLowF = cp.AmbRangeF.Min;
            StingrayDataPool.TaHighF = cp.AmbRangeF.Max;

            StingrayDataPool.CaliTP1 = cp.TP1Setting;
            StingrayDataPool.CaliTP2 = cp.TP2Setting;
            StingrayDataPool.CaliTP3 = cp.TP3Setting;
            StingrayDataPool.CaliTP4 = cp.TP4Setting;
           
            StingrayDataPool.LineWave = cp.LineWave.GetArray();
            StingrayDataPool.LineAmpl = cp.LineAmpl.GetArray();

            StingrayDataPool.CalEmiss = cp.CalEmiss;
            StingrayDataPool.P3CaliOffset = cp.CaliOffset;

            StingrayDataPool.InitPercMarg = cp.MarginP;
            StingrayDataPool.InitDegMargF = cp.MarginF;
            StingrayDataPool.PercMargCenterF = cp.MargCenterF;
            StingrayDataPool.MargScaleMin = cp.MargScale.Min;
            StingrayDataPool.MargScaleMax = cp.MargScale.Max;
            StingrayDataPool.InitMargScale = cp.MargScale.Goal;
            StingrayDataPool.MargScaleDeriv = cp.MargSclDeriv;
            StingrayDataPool.JumpFactor = cp.JumpFactor;
            StingrayDataPool.SpanErrFudge = cp.SpanErrFudge;
            StingrayDataPool.CurveFitAdj1 = cp.CurveFitAdj1;
            StingrayDataPool.CurveFitAdj4 = cp.CurveFitAdj4;
            StingrayDataPool.SpecScaleMin1 = cp.SpecScale1.Min;
            StingrayDataPool.SpecScaleMax1 = cp.SpecScale1.Max;
            StingrayDataPool.InitSpecScale1 = cp.SpecScale1.Goal;
            StingrayDataPool.SpecScaleMin2 = cp.SpecScale2.Min;
            StingrayDataPool.SpecScaleMax2 = cp.SpecScale2.Max;
            StingrayDataPool.InitSpecScale2 = cp.SpecScale2.Goal;
            StingrayDataPool.RespScaleMin1 = cp.RespScale1.Min;
            StingrayDataPool.RespScaleMax1 = cp.RespScale1.Max;
            StingrayDataPool.InitRespScale1 = cp.RespScale1.Goal;
            StingrayDataPool.RespScaleMin2 = cp.RespScale2.Min;
            StingrayDataPool.RespScaleMax2 = cp.RespScale2.Max;
            StingrayDataPool.InitRespScale2 = cp.RespScale2.Goal;

            StingrayDataPool.CurveFitSigmaMax = cp.CurveFitSig.Max;
            
            StingrayDataPool.PSdTblNum = cp.TableNumber;
            StingrayDataPool.PSaTblNum = cp.TableNumber;
            StingrayDataPool.PStcTblNum = cp.TableNumber;
            StingrayDataPool.PStcaTblNum = cp.TableNumber;

            StingrayDataPool.UNMin = cp.Nmin.GetArray();
            StingrayDataPool.UNmax = cp.Nmax.GetArray();
            
             
            StingrayDataPool.UPOut = new double[StingrayDataPool.UNMin.Length];
            StingrayDataPool.PSd1Tbl = new long[StingrayDataPool.PSdTblNum];
            StingrayDataPool.PSd2Tbl = new long[StingrayDataPool.PSdTblNum];
            StingrayDataPool.LowPSd2Tbl = new long[4];
            StingrayDataPool.LowPSd2P3Tbl = new long[4];
            StingrayDataPool.LowBPTbl = new long[4];
            StingrayDataPool.LowBPP3Tbl = new long[4];
            StingrayDataPool.BPTbl = new long[StingrayDataPool.PSdTblNum];
            StingrayDataPool.TcalF = new double[4];
            //
            StingrayDataPool.P3BPTbl = new long[StingrayDataPool.PSdTblNum];
            StingrayDataPool.P3PSd1Tbl = new long[StingrayDataPool.PSdTblNum];
            StingrayDataPool.P3PSd2Tbl = new long[StingrayDataPool.PSdTblNum];

            //Init for StingrayCaliResults
            StingrayCaliResults.BreakPointEnergryTable = new long[32];
            StingrayCaliResults.BreakPointEnergryTable = new long[32];
            StingrayCaliResults.BlackBodyRealTemperature = new double[4];
            StingrayCaliResults.AdValueFromUnit = new double[4];
            //
        }
        public bool CheckmultiMeter()
        {
            return true;
        }
        public bool CheckCalibrationBox()
        {
            return true;
        }
        public void OpenMultimeter()
        {

            try
            {
                multimetercomm.Connect();
            }
            catch
            {

            }
            if (multimetercomm.IsOpen)
            {
            }
            else
            {
            }
        }
        public void CloseMultimeter()
        {

            try
            {
                multimetercomm.Disconnect();
            }
            catch
            {

            }
            if (!multimetercomm.IsOpen)
            {
            }
            else
            {
            }
        }
        public void OpenCalibrationBox()
        {

            try
            {

                calibrationboxcomm.Connect();
            }
            catch
            {

            }
            if (calibrationboxcomm.IsOpen)
            {
            }
            else
            {
            }
        }
        public void CloseCalibrationBox()
        {

            try
            {
                calibrationboxcomm.Disconnect();
            }
            catch
            {

            }
            if (!calibrationboxcomm.IsOpen)
            {
            }
            else
            {
            }
        }

    #region Do analog calibration
        public void CalibrationAnalogOutput(string wiring)
        {
            OpenMultimeter();
            OpenCalibrationBox();
            switch (wiring)
            {
                case "S":
                    Cali2WiresAnalogOutput();
                    break;
                case "T":
                    Cali6WiresAnalogOutput();
                    break;
                case "M16":
                    Cali12WiresAnalogInputOutput();
                    break;
                case "E":

                    break;
                default:
                    break;
            }
            System.Threading.Thread.Sleep(100);
            CloseMultimeter();
            CloseCalibrationBox();
        }
    #endregion

    #region Power consumption check
        private void PowerConsumptionCheck(string wiring)
        {
            string result = "";
            double TempCur1, TempVolt1, TempCur2, TempVolt2;
            
            OpenMultimeter();
            OpenCalibrationBox();
            SwitchDeviceIntoCalibrationMode();

            switch (wiring)
            {
                case "S":
                    //
                    calibrationboxcomm.SendCommand("[OPEN001]", 1);
                    multimetercomm.SendCommand("ADC", 1);
                    System.Threading.Thread.Sleep(100);

                    result = communicationInterface.Set("%OA", "16383"); // Output max current
                    if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
                    {
                        //   return true;
                    }
                    else
                    {
                        //   return false;
                    }
                    System.Threading.Thread.Sleep(2000); //delay for waiting multimeter stable

                    multimetercomm.SendCommand("MEAS1?", 1);
                    System.Threading.Thread.Sleep(2000);
                    result = (string)multimetercomm.ReceiveSyn();

                    if (!processResult("MEAS1?", result, ref MultimeterValue))
                    {
                        AnalogCaliResult(0);
                        return;
                    }
                    else
                    {

                    }
                    MultimeterValue = MultimeterValue * 1000 * 1000;//unit A to uA
                    StingrayCaliResults.PowermA = (double)MultimeterValue/1000;//unit mA
                    calibrationboxcomm.SendCommand("[CLOSE001]", 1);
                    break;
                case "T":
                case "M16":
                    calibrationboxcomm.SendCommand("[OPEN002]", 1);
                    calibrationboxcomm.SendCommand("[OPEN003]", 1);
                    calibrationboxcomm.SendCommand("[OPEN004]", 1);
                    
                //Status 1: Output max current                  
                    result = communicationInterface.Set("%OA", "16383"); // Output max current
                    if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
                    {
                        //   return true;
                    }
                    else
                    {
                        //   return false;
                    }
                    multimetercomm.SendCommand("VDC", 1);
                    System.Threading.Thread.Sleep(2000); //delay for waiting multimeter stable
                    multimetercomm.SendCommand("MEAS1?", 1);
                    System.Threading.Thread.Sleep(2000);
                    result = (string)multimetercomm.ReceiveSyn();
                    if (!processResult("MEAS1?", result, ref MultimeterValue))
                    {
                        AnalogCaliResult(0);
                        return;
                    }
                    else
                    {

                    }
                    MultimeterValue = MultimeterValue * 1000;//unit V to mV
                    TempVolt1 = (double)MultimeterValue;//unit mV                    
                    TempCur1 = TempVolt1/50*1000; //unit uA
                    //StingrayCaliResults.PowermA = (double)MultimeterValue;//unit uA
                   

                //Status 2: Output max voltage
                    System.Threading.Thread.Sleep(500);
                    result = communicationInterface.Set("%OV", "16383"); // Output max current
                    if (communicationInterface.Protocol.ExtractResult(ref result, "%OV") && result == "T")
                    {
                        //   return true;
                    }
                    else
                    {
                        //   return false;
                    }

                    multimetercomm.SendCommand("VDC", 1);                   
                    System.Threading.Thread.Sleep(2000); //delay for waiting multimeter stable
                    multimetercomm.SendCommand("MEAS1?", 1);
                    System.Threading.Thread.Sleep(2000);
                    result = (string)multimetercomm.ReceiveSyn();
                    if (!processResult("MEAS1?", result, ref MultimeterValue))
                    {
                        AnalogCaliResult(0);
                        return;
                    }
                    else
                    {

                    }
                    MultimeterValue = MultimeterValue * 1000;//unit V to mV
                    TempVolt2 = (double)MultimeterValue;//unit mV                   
                    TempCur2 = TempVolt2/50*1000;  //unit uA
                    //StingrayCaliResults.PowermA = (double)MultimeterValue;//unit uA

                //Compare the value between different status
                    if ((TempVolt1 / 1000) * (TempCur1 / 1000 / 1000) > (TempVolt2 / 1000) * (TempCur2 / 1000 / 1000))
                    {
                        StingrayCaliResults.PowermV = TempVolt1;
                        StingrayCaliResults.PowermA = TempCur1/1000;
                    }
                    else
                    {
                        StingrayCaliResults.PowermV = TempVolt2;
                        StingrayCaliResults.PowermA = TempCur2/1000;
                    }
                    StingrayCaliResults.PowermW = StingrayCaliResults.PowermV / 1000 * StingrayCaliResults.PowermA;

                    calibrationboxcomm.SendCommand("[CLOSE002]", 1);
                    calibrationboxcomm.SendCommand("[CLOSE003]", 1);
                    calibrationboxcomm.SendCommand("[CLOSE004]", 1);

                    break;
                
                case "E":

                    break;
                default:
                    break;
            }

            System.Threading.Thread.Sleep(100);
            SwitchDeviceOutCalibrationMode();
            CloseMultimeter();
            CloseCalibrationBox();
        }
    #endregion

        public void CalibrationAnalogInput(string wiring)
        {

        }

    #region All specific methods about Analog calibration
        private void Cali2WiresAnalogOutput()
        {
            string result = "";
            //SwitchDeviceIntoCalibrationMode();
            //config calibrationbox
            calibrationboxcomm.SendCommand("[OPEN001]", 1);//Connect Gnd and Com of 8808A
            System.Threading.Thread.Sleep(100);
            //calibrationboxcomm.SendCommand("[OPEN002]", 1);//Connect Vout and mA Gear of 8808A
            System.Threading.Thread.Sleep(100);
            //config multimeter
            multimetercomm.SendCommand("ADC", 1);

            //start  unit analog output calibration
            SwitchDeviceIntoCalibrationMode();
            //output first calibration point current 
            result = communicationInterface.Set("%OA", TwoWiresLowCount.ToString()); // sends %OA=TwoWiresLowCount

            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter


            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }
            MultimeterValue = MultimeterValue * 1000 * 1000;//unit A to uA
            StingrayCaliResults.MAOutputLow = (double)MultimeterValue;//unit uA
            //
            //Add range check function
            //

            result = communicationInterface.Set("%OA", TwoWiresHighCount.ToString()); // sends %OA=TwoWiresLowCount
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter

            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }
            MultimeterValue = MultimeterValue * 1000 * 1000;//unit A to uA
            StingrayCaliResults.MAOutputHigh = (double)MultimeterValue;

            //
            CaliAnalogOuputAlgorithm(StingrayCaliResults.MAOutputLow, TwoWiresLowCount, StingrayCaliResults.MAOutputHigh, TwoWiresHighCount, 3);
            //write the results into unit
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            result = communicationInterface.Set("%D", "M" + StingrayCaliResults.MAOutputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DM") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            result = communicationInterface.Set("%D", "N" + StingrayCaliResults.MAOutputOffset.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DN") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            SwitchDeviceOutCalibrationMode();
            RestoreFactorySet();
            calibrationboxcomm.SendCommand("[CLOSE001]", 1);//Disconnect Gnd and Com of 8808A
            multimetercomm.SendCommand("VDC", 1);
            //
        }
        private void Cali6WiresAnalogOutput()
        {
            SwitchDeviceIntoCalibrationMode();
            //StartTransferTable();
            CaliAnalogOutputMAMV();
            Cali6WiresAnalogOutputTC();
            //EndTransferTable();
            SwitchDeviceOutCalibrationMode();
            RestoreFactorySet();
        }
        private void Cali12WiresAnalogInputOutput()
        {
            SwitchDeviceIntoCalibrationMode();
            //For reducing power
            communicationInterface.Set("%DI", "B" + StingrayCaliResults.ModelNumber.ToString()); // sends %DI=BModelNumber
            System.Threading.Thread.Sleep(100);
            //StartTransferTable();
            CaliAnalogOutputMAMV();
            Cali12WiresAnalogFTCINPUT();
            //EndTransferTable();
            SwitchDeviceOutCalibrationMode();
            RestoreFactorySet();
        }
        private void Cali6WiresAnalogOutputTC()
        {
            string result = "";
            //start  unit analog output calibration

            //output first calibration point current 
            result = communicationInterface.Set("%OTC", SixWiresTcLowCount.ToString());

            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OTC") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter


            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }
            MultimeterValue = MultimeterValue * 1000;//unit V to mV
            StingrayCaliResults.ThermalCoupleOutputLow = (double)MultimeterValue;//unit mV

            result = communicationInterface.Set("%OTC", SixWiresTcHighCount.ToString());
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OTC") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter
            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {
                AnalogCaliResult(0);
                return;
            }

            else
            {

            }

            MultimeterValue = MultimeterValue * 1000;//unit V to mV
            StingrayCaliResults.ThermalCoupleOutputHigh = (double)MultimeterValue;

            CaliAnalogOuputAlgorithm(StingrayCaliResults.ThermalCoupleOutputLow, SixWiresTcLowCount, StingrayCaliResults.ThermalCoupleOutputHigh, SixWiresTcHighCount, 6);
            
            //write the results into unit with %DS and %DY commands
            result = communicationInterface.Set("%D", "S" + StingrayCaliResults.ThermalCoupleOutputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DS") && result == "T")
            {
                //    return true;
            }
            else
            {
                //   return false;
            }

            result = communicationInterface.Set("%D", "Y" + StingrayCaliResults.ThermalCoupleOutputOffset.ToString()); // 
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DY") && result == "T")
            {
                //    return true;
            }
            else
            {
                //   return false;
            }
        }
        private void Cali12WiresAnalogFTCINPUT()
        {
            string result = "";
            //Start Calibrate the FTC1 and FTC2 input

            //output first calibration point current 
            result = communicationInterface.Set("%OV", TwelveWiresFTCLowCount.ToString());

            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OV") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(2000);//delay for waiting multimeter stable

            //get value from multimeter
            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(2000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {
                AnalogCaliResult(0);
                return;
            }
            else
            {

            }

            MultimeterValue = MultimeterValue * 1000;//unit V to mV

            StingrayCaliResults.FTC1InputLow = (double)MultimeterValue;//unit mV
            StingrayCaliResults.FTC2InputLow = (double)MultimeterValue;//unit mV
            //GET AD with %F1V and %F2V comand           
            FTCLowCount = communicationInterface.GetIntFromDevice("FTC1 Input", "%F1V");
            StingrayCaliResults.FTC1InputLowCount = FTCLowCount;
            System.Threading.Thread.Sleep(1000);
            FTCLowCount = communicationInterface.GetIntFromDevice("FTC2 Input", "%F2V");
            StingrayCaliResults.FTC2InputLowCount = FTCLowCount;
            System.Threading.Thread.Sleep(1000);
            //var result = communicationInterface.Set("%F1V", String.Empty); //only sends %TTE

            //
            //
            result = communicationInterface.Set("%OV", TwelveWiresFTCHighCount.ToString());
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OV") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter

            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }


            MultimeterValue = MultimeterValue * 1000;//unit V to mV

            StingrayCaliResults.FTC1InputHigh = (double)MultimeterValue;//unit mV
            StingrayCaliResults.FTC2InputHigh = (double)MultimeterValue;//unit mV
            //GET AD with %F1V and %F2V comand
            StingrayCaliResults.FTC1InputHighCount = communicationInterface.GetIntFromDevice("FTC1 Input", "%F1V");
            StingrayCaliResults.FTC2InputHighCount = communicationInterface.GetIntFromDevice("FTC2 Input", "%F2V");

            CaliAnalogInputAlgorithm(StingrayCaliResults.FTC1InputLow, StingrayCaliResults.FTC1InputLowCount, StingrayCaliResults.FTC1InputHigh, StingrayCaliResults.FTC1InputHighCount, 1);
            CaliAnalogInputAlgorithm(StingrayCaliResults.FTC2InputLow, StingrayCaliResults.FTC2InputLowCount, StingrayCaliResults.FTC2InputHigh, StingrayCaliResults.FTC2InputHighCount, 2);
            //write the results into unit
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            result = communicationInterface.Set("%D", "A" + StingrayCaliResults.FTC1InputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DA") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            result = communicationInterface.Set("%D", "B" + StingrayCaliResults.FTC1InputOffset.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DB") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }

            //write the results into unit
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            result = communicationInterface.Set("%D", "C" + StingrayCaliResults.FTC2InputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DC") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            result = communicationInterface.Set("%D", "D" + StingrayCaliResults.FTC2InputOffset.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DD") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }

        }
        private void CaliAnalogOutputMAMV()
        {
            string result = "";
            //config calibrationbox
            calibrationboxcomm.SendCommand("[CLOSE001]", 1);//Disconnect Gnd and Com of 8808A
            System.Threading.Thread.Sleep(100);
            calibrationboxcomm.SendCommand("[OPEN002]", 1);//Connect Vout and mA Gear of 8808A
            System.Threading.Thread.Sleep(100);
            //config multimeter
            multimetercomm.SendCommand("ADC", 1);

            //start  unit analog output calibration

            //output first calibration point current 
            result = communicationInterface.Set("%OA", SixWiresIoutLowCount.ToString()); // sends %OA=SixWiresIoutLowCount

            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter


            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }
            MultimeterValue = MultimeterValue * 1000 * 1000;//unit A to uA
            StingrayCaliResults.MAOutputLow = (double)MultimeterValue;//unit uA
            //
            //
            result = communicationInterface.Set("%OA", SixWiresIoutHighCount.ToString()); // sends %OA=SixWiresIoutHighCount
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OA") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter
            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }


            MultimeterValue = MultimeterValue * 1000 * 1000;//unit A to uA

            StingrayCaliResults.MAOutputHigh = (double)MultimeterValue;
            CaliAnalogOuputAlgorithm(StingrayCaliResults.MAOutputLow, SixWiresIoutLowCount, StingrayCaliResults.MAOutputHigh, SixWiresIoutHighCount, 4);
            //write the results into unit with %DM and %DN commands

            //communicationInterface.set("E", "0.95"); //sends E=0.95
            result = communicationInterface.Set("%D", "M" + StingrayCaliResults.MAOutputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DM") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            result = communicationInterface.Set("%D", "N" + StingrayCaliResults.MAOutputOffset.ToString()); // 
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DN") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }

            //Start calibrate mV output
            //config calibrationbox
            calibrationboxcomm.SendCommand("[CLOSE001]", 1);//Disconnect Gnd and Com of 8808A
            System.Threading.Thread.Sleep(100);
            calibrationboxcomm.SendCommand("[CLOSE002]", 1);//Disconnect Vout and mA Gear of 8808A
            System.Threading.Thread.Sleep(100);
            //config multimeter
            multimetercomm.SendCommand("VDC", 1);

            //start  unit analog output calibration
            //output first calibration point current 
            result = communicationInterface.Set("%OV", SixWiresVoutLowCount.ToString());

            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OV") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter


            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {
                AnalogCaliResult(0);
                return;
            }

            else
            {

            }

            MultimeterValue = MultimeterValue * 1000;//unit V to mV
            StingrayCaliResults.MVOutputLow = (double)MultimeterValue;//unit mV
            //
            //
            result = communicationInterface.Set("%OV", SixWiresVoutHighCount.ToString());
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%OV") && result == "T")
            {
                //   return true;
            }
            else
            {
                //   return false;
            }
            System.Threading.Thread.Sleep(4000);//delay for waiting multimeter stable
            //get value from multimeter
            multimetercomm.SendCommand("MEAS1?", 1);
            System.Threading.Thread.Sleep(1000);
            result = (string)multimetercomm.ReceiveSyn();

            if (!processResult("MEAS1?", result, ref MultimeterValue))
            {

                AnalogCaliResult(0);
                return;
            }

            else
            {

            }

            MultimeterValue = MultimeterValue * 1000;//unit V to mV
            StingrayCaliResults.MVOutputHigh = (double)MultimeterValue;

            CaliAnalogOuputAlgorithm(StingrayCaliResults.MVOutputLow, SixWiresVoutLowCount, StingrayCaliResults.MVOutputHigh, SixWiresVoutHighCount, 5);
            //write the results into unit with %DK and %DL commands

            //communicationInterface.set("E", "0.95"); //sends E=0.95
            result = communicationInterface.Set("%D", "K" + StingrayCaliResults.MVOutputGain.ToString());
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DK") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
            result = communicationInterface.Set("%D", "L" + StingrayCaliResults.MVOutputOffset.ToString()); // 
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DL") && result == "T")
            {
                //    return true;
            }
            else
            {//didnt work
                //   return false;
            }
        }
        private void CaliAnalogOuputAlgorithm(double low, long lowcount, double high, long highcount, int type)
        {
            double gain, offset;

            if (type == 3)//James 20160729 FOR two wires model
            {
                gain = (highcount - lowcount) / (high - low);
                offset = lowcount - gain * low;
                StingrayCaliResults.MAOutputLowCount = lowcount;
                StingrayCaliResults.MAOutputHighCount = highcount;
                StingrayCaliResults.MAOutputGain = (long)(gain * 65536);
                // StingrayDataPool.IOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.MAOutputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }

            if (type == 4)//James 20160729 for 12 wires model 4-20mA
            {
                gain = (highcount - lowcount) / (high - low);
                offset = lowcount - gain * low;
                StingrayCaliResults.MAOutputLowCount = lowcount;
                StingrayCaliResults.MAOutputHighCount = highcount;
                StingrayCaliResults.MAOutputGain = (long)(gain * 65536);
                // StingrayDataPool.IOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.MAOutputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }
            if (type == 5)//James 20160729 for 12 wires model 0-10V
            {
                gain = (highcount - lowcount) / (high - low);
                offset = lowcount - gain * low;
                StingrayCaliResults.MVOutputLowCount = lowcount;
                StingrayCaliResults.MVOutputHighCount = highcount;
                StingrayCaliResults.MVOutputGain = (long)(gain * 65536);
                // StingrayDataPool.VOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.MVOutputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }

            if (type == 6)//James 20160729 for 6 wires model TCout
            {
                //TC UNIT is uV ,not same with Voltage output Unit mV
                low = low * 1000;
                high = high * 1000;
                //TC UNIT is uV ,not same with Voltage output Unit mV
                StingrayCaliResults.ThermalCoupleOutputLowCount = lowcount;
                StingrayCaliResults.ThermalCoupleOutputHighCount = highcount;
                gain = (highcount - lowcount) / (high - low);
                offset = lowcount - gain * low;
                StingrayCaliResults.ThermalCoupleOutputGain = (long)(gain * 65536);
                // StingrayDataPool.VOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.ThermalCoupleOutputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }
        }
        private void CaliAnalogInputAlgorithm(double low, long lowcount, double high, long highcount, int type)
        {
            double gain, offset;
            if (type == 1)//James 20160729 for12 wires model FTC1
            {
                //  low = low / 10;//Just for debug
                //  high = high / 10;//Just for debug 
                //TC UNIT is uV ,not same with Voltage output Unit mV

                gain = (high - low) / (highcount - lowcount);
                offset = low - gain * lowcount;
                StingrayCaliResults.FTC1InputLowCount = lowcount;
                StingrayCaliResults.FTC1InputHighCount = highcount;
                StingrayCaliResults.FTC1InputGain = (long)(gain * 65536);
                // StingrayDataPool.VOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.FTC1InputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }
            if (type == 2)//James 20160729 for 12 wires model FTC2
            {
                //  low = low / 10;//Just for debug
                //  high = high / 10;//Just for debug 
                //TC UNIT is uV ,not same with Voltage output Unit mV

                gain = (high - low) / (highcount - lowcount);
                offset = low - gain * lowcount;
                StingrayCaliResults.FTC2InputLowCount = lowcount;
                StingrayCaliResults.FTC2InputHighCount = highcount;

                StingrayCaliResults.FTC2InputGain = (long)(gain * 65536);
                // StingrayDataPool.VOfs = (long)(offset * 65536);//add 65536 to scale 20161011
                StingrayCaliResults.FTC2InputOffset = (long)(offset * 1024);//change  65536 to  1024 
            }

        }
    #endregion

        public bool CofigUnitBeforeAdGather()
        {
            string result = "";
            
            result = communicationInterface.Set("%DI", "B" + StingrayCaliResults.ModelNumber.ToString()); // sends %DI=BModelNumber
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                
            }
            else
            {
                
            }
            result = communicationInterface.Set("%DI", "C" + StingrayCaliResults.ModelName.ToString()); // sends %DI=CModelName
            System.Threading.Thread.Sleep(100);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// 
        /// 
        /// 
        /// <summary>
        /// A unique identifier for the mainboard
        /// </summary>
        /// <returns>a 24 digit code</returns>
        public override string GetUniqueID()
        {
            throw new NotImplementedException();
            //endruance uses 24 alphanumeric code fixed inside the microcontroller unit defined by stm microelectronics
            //either there is one or we use serial number
            //%UID = 
        }
        
        public override bool IsAmbientStable()
        {
            lock (CommunicationSyncronisation)
            {
                //this needs progress report
                var notDone = true;
               // var progressCounter = 0;
                while (notDone)
                {
                    if (cancellationToken.IsCancellationRequested) return false; //cancellation for longer running tasks
                    //measure and check
                    //if done
                    notDone = false;
                    //if something is wrong throw exception maybe
                }
                //do something maybe
                return false;
            }
        }

        //Check the unit and device work well after assembled and before calibration
        public override bool RunDiagnostics()
        {
            lock (CommunicationSyncronisation) //means i want exclusive access to the communicationInterface
            {
                var notDone = true;
                //var progressCounter = 0;
                while (notDone && !cancellationToken.IsCancellationRequested) //instead of IsCancellationToken/// cancellationToken.ThrowIfCancellationRequested();
                {
                    IsAmbientStable();
                    //do measure get value from the unit to check the unit 
                    var adcValue = communicationInterface.GetIntFromDevice(Strings.IHV_COMMAND_DESCRIPTION, CaliCmd.ADC_IHV);
                    var Tamb = communicationInterface.GetDoubleFromDevice(Strings.IHV_COMMAND_DESCRIPTION, CaliCmd.T_INT);
                    //progress counter has to be between 0 and 100;
                   // dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));
                    //do something with your ADCValue
                    //check the device 8808A
                    var MeterStatus = CheckmultiMeter();
                   // dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));
                    //check the calibration box 
                    var CalbirationBoxStatus = CheckCalibrationBox();
                   // dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));


                }
            }
            return true;
        }

        
        protected void GatheringIrAdValue(int Count)
        {
            double sumAd = 0, sumAmb = 0;
            int ambs;
            string ambhex;
            
            SwitchDeviceIntoCalibrationMode();

            var result = communicationInterface.Set("E", "1.0"); // Set E=1
            if (communicationInterface.Protocol.ExtractResult(ref result, "E") && result == "1.000")
            {

            }
            else
            {
                CustomException EmissSetFail = new CustomException("Emissivity set fail!");
                throw EmissSetFail;
            }
            //
            for (int i = 0; i < Count; i++)
            {
                // AmbFromUnit = communicationInterface.GetIntFromDevice(Strings.TINT_COMMAND_DESCRIPTION, "%AMBV");
                ambhex = communicationInterface.GetStringFromDevice(Strings.TINT_COMMAND_DESCRIPTION, "%AMBV");
                ambs = Convert.ToInt32(ambhex, 16);
                AmbFromUnit = (double)ambs / 4;//get the S value
                AmbFromUnit = CALMATHLIB.StoC(AmbFromUnit);

                System.Threading.Thread.Sleep(20);
                AdFromUnit = communicationInterface.GetIntFromDevice("Gather AD form unit %IHV", "%IHV");
                System.Threading.Thread.Sleep(20);

                sumAd = sumAd + AdFromUnit;
                sumAmb = sumAmb + AmbFromUnit;
                
            }

            AdFromUnit = (int)sumAd / Count;
            AmbFromUnit = sumAmb / Count;
        }

        public void WriteCalibrationDataToDevice()
        {
            var result = communicationInterface.Set("%TTS", String.Empty); //only sends %CE
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%TTS") && result == "T")
            {
                
            }
            else
            {
                
            }

            if (FlagHighTempModel == 0)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (i < StingrayDataPool.PSdTblNum)
                    {
                        BPSendTalbe[i] = StingrayDataPool.BPTbl[i];
                        result = communicationInterface.Set("%D", "T" + i.ToString("D2") + (StingrayDataPool.BPTbl[i]).ToString()); // sends %D=T 

                        //its good to check the result
                        if (communicationInterface.Protocol.ExtractResult(ref result, "%DT") && result == "T")
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        BPSendTalbe[i] = 4294967295;
                        result = communicationInterface.Set("%D", "T" + i.ToString("D2") + "4294967295"); // sends %D=T 

                        //its good to check the result
                        if (communicationInterface.Protocol.ExtractResult(ref result, "%DT") && result == "T")
                        {
                            //  return true;
                        }
                        else
                        {
                            //  return false;
                        }
                    }
                    System.Threading.Thread.Sleep(100);//James20160817
                }
            }
            else
            {
                for (int i = 0; i < 28; i++)
                {
                    BPSendTalbe[i + 4] = StingrayDataPool.BPTbl[i];

                }
                if (FlagP3Model == 1)
                {
                    BPSendTalbe[0] = StingrayDataPool.LowBPP3Tbl[0];// (long)(CALMATHLIB.CtoS(-25)) * 4;
                    BPSendTalbe[1] = StingrayDataPool.LowBPP3Tbl[1];//(long)(CALMATHLIB.CtoS(0)) * 4;
                    BPSendTalbe[2] = StingrayDataPool.LowBPP3Tbl[2];//(long)(CALMATHLIB.CtoS(5)) * 4;
                    BPSendTalbe[3] = StingrayDataPool.LowBPP3Tbl[3];//(long)(CALMATHLIB.CtoS(15)) * 4;
                }

                else
                {
                    BPSendTalbe[0] = StingrayDataPool.LowBPTbl[0];// (long)(CALMATHLIB.CtoS(-25)) * 4;
                    BPSendTalbe[1] = StingrayDataPool.LowBPTbl[1]; //(long)(CALMATHLIB.CtoS(0)) * 4;
                    BPSendTalbe[2] = StingrayDataPool.LowBPTbl[2]; //(long)(CALMATHLIB.CtoS(50)) * 4;
                    BPSendTalbe[3] = StingrayDataPool.LowBPTbl[3]; //(long)(CALMATHLIB.CtoS(100)) * 4;
                }

                for (int i = 0; i < 32; i++)
                {
                    result = communicationInterface.Set("%D", "T" + i.ToString("D2") + BPSendTalbe[i].ToString()); // sends %D=T 

                    //its good to check the result
                    if (communicationInterface.Protocol.ExtractResult(ref result, "%DT") && result == "T")
                    {
                        //  return true;
                    }
                    else
                    {
                        // return false;
                    }
                    System.Threading.Thread.Sleep(100);//James20160817

                }
            }

            if (FlagHighTempModel == 0)
            {

                for (int i = 0; i < 32; i++)
                {
                    if (i < StingrayDataPool.PSdTblNum)
                    {
                        PSd2SendTable[i] = StingrayDataPool.PSd2Tbl[i];
                        result = communicationInterface.Set("%D", "V" + i.ToString("D2") + (StingrayDataPool.PSd2Tbl[i]).ToString()); // sends %D=T 

                        //its good to check the result
                        if (communicationInterface.Protocol.ExtractResult(ref result, "%DV") && result == "T")
                        {
                            //    return true;
                        }
                        else
                        {
                            // return false;
                        }
                    }
                    else
                    {
                        PSd2SendTable[i] = 2147483647;
                        result = communicationInterface.Set("%D", "V" + i.ToString("D2") + "2147483647"); // sends %D=T 

                        //its good to check the result
                        if (communicationInterface.Protocol.ExtractResult(ref result, "%DV") && result == "T")
                        {
                            //    return true;
                        }
                        else
                        {
                            //  return false;
                        }
                    }
                    System.Threading.Thread.Sleep(100);//James20160817
                }
            }
            else
            {
                for (int i = 0; i < 28; i++)
                {
                    PSd2SendTable[i + 4] = StingrayDataPool.PSd2Tbl[i];

                }
                if (FlagP3Model == 1)
                {
                    PSd2SendTable[0] = StingrayDataPool.LowPSd2P3Tbl[0];
                    PSd2SendTable[1] = StingrayDataPool.LowPSd2P3Tbl[1];
                    PSd2SendTable[2] = StingrayDataPool.LowPSd2P3Tbl[2];
                    PSd2SendTable[3] = StingrayDataPool.LowPSd2P3Tbl[3];
                }
                else
                {
                    PSd2SendTable[0] = StingrayDataPool.LowPSd2Tbl[0];
                    PSd2SendTable[1] = StingrayDataPool.LowPSd2Tbl[1];
                    PSd2SendTable[2] = StingrayDataPool.LowPSd2Tbl[2];
                    PSd2SendTable[3] = StingrayDataPool.LowPSd2Tbl[3];
                }

                for (int i = 0; i < 32; i++)
                {
                    result = communicationInterface.Set("%D", "V" + i.ToString("D2") + PSd2SendTable[i].ToString()); // sends %D=T 

                    //its good to check the result
                    if (communicationInterface.Protocol.ExtractResult(ref result, "%DV") && result == "T")
                    {
                        //   return true;
                    }
                    else
                    {
                        // return false;
                    }
                    System.Threading.Thread.Sleep(100);//James20160817
                }
            }

            //Convert.ToInt32(StingrayDataPool.THighF).ToString()
            result = communicationInterface.Set("%D", "O" + (CALMATHLIB.FtoS(StingrayDataPool.DHighF)*4).ToString()); //James  
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DO") && result == "T")
            {
                
            }
            else
            {
                
            }

            result = communicationInterface.Set("%D", "P" + (CALMATHLIB.FtoS(StingrayDataPool.DLowF) * 4).ToString()); //James  
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DP") && result == "T")
            {
                
            }
            else
            {
               
            }

            result = communicationInterface.Set("%D", "F" + ((int)StingrayDataPool.PSdOfs2).ToString()); //James  
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DF") && result == "T")
            {
                
            }
            else
            {
                
            }

            result = communicationInterface.Set("%D", "R" + StingrayDataPool.AmbTempS.ToString()); //James  
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DR") && result == "T")
            {
                
            }
            else
            {
                
            }

            //James added the G,H,M,N,Y,Z commands

            result = communicationInterface.Set("%D", "G" + StingrayDataPool.AmbG.ToString());   
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DG") && result == "T")
            {
                
            }
            else
            {
                
            }

            result = communicationInterface.Set("%D", "H" + StingrayDataPool.AmbOfs.ToString());   
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DH") && result == "T")
            {
                
            }
            else
            {
                
            }

            result = communicationInterface.Set("%D", "M" + StingrayDataPool.IGain.ToString());  
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DM") && result == "T")
            {
                
            }
            else
            {
                
            }

            result = communicationInterface.Set("%D", "N" + StingrayDataPool.IOfs.ToString()); 
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DN") && result == "T")
            {
                
            }
            else
            {
                
            }

            //Write DetAlpha/DetAlpha_low/DetAlpha_high
            result = communicationInterface.Set("%D", "Q" + StingrayDataPool.Alpha.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%D") && result == "T")
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {

            }
            result = communicationInterface.Set("%D", "W" + StingrayDataPool.Alpha_Low.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%D") && result == "T")
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {

            }

            result = communicationInterface.Set("%D", "Z" + StingrayDataPool.Alpha_High.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {

            }
           

            // end
            if (StingraySerialNumber != 0)
            {
                result = communicationInterface.Set("%DI", "A" + StingrayDataPool.SerNumber.ToString("d9")); //James  

                //its good to check the result
                if (communicationInterface.Protocol.ExtractResult(ref result, "%DIA") && result == "T")
                {
                    //  return true;
                }
                else
                {
                    //   return false;
                }
            }


            result = communicationInterface.Set("%TTE", String.Empty); //only sends %CE
            if (communicationInterface.Protocol.ExtractResult(ref result, "%TTE") && result == "T")
            {
                
            }
            else
            {
               
            }

            System.Threading.Thread.Sleep(100);
            SwitchDeviceOutCalibrationMode();
            System.Threading.Thread.Sleep(500);

            result = communicationInterface.Set("XF", String.Empty); // XF DELAY 1500ms
            System.Threading.Thread.Sleep(1500);
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "XF") && result == "T")
            {
                
            }
            else
            {
               
            }
            //

            result = communicationInterface.Set("E", "1.0"); //
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "E") && result == "T")
            {
                
            }
            else
            {
                
            }
        }

        public override bool SwitchDeviceIntoCalibrationMode()
        {
            //Example
            //communicationInterface.set("E", "0.95"); //sends E=0.95          
            var result = communicationInterface.Set("%CS", String.Empty); //only sends %CS
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%CS") && result == "T")
            {
                return true;
            }
            else
            {
                CustomException InCaliFail = new CustomException("Enter into calibration mode fail!");
                throw InCaliFail;
            }
                     
        }
        public bool SwitchDeviceOutCalibrationMode()
        {
            //Example
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            var result = communicationInterface.Set("%CE", String.Empty); //only sends %CE
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%CE") && result == "T")
            {
                return true;
            }
            else
            {
                CustomException OutCaliFail = new CustomException("Quit calibration mode fail!");
                throw OutCaliFail;
            }
        }

        public bool StartTransferTable()
        {
            //Example
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            var result = communicationInterface.Set("%TTS", String.Empty); //only sends %TTS
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%TTS") && result == "T")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool EndTransferTable()
        {
            //Example
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            var result = communicationInterface.Set("%TTE", String.Empty); //only sends %TTE
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "%TTE") && result == "T")
            {
                return true;
            }
            else
            {//didnt work
                return false;
            }
        }
        public void RestoreFactorySet()
        {
            //Example
            //communicationInterface.set("E", "0.95"); //sends E=0.95
            var result = communicationInterface.Set("XF", String.Empty); //only sends %XF
            //its good to check the result
            if (communicationInterface.Protocol.ExtractResult(ref result, "XF") && result == "T")
            {
                //   return true;
            }
            else
            {//didnt work
                //  return false;
            }
        }
        //
        public override bool Run(CalibrationAction step)
        {
            throw new NotSupportedException();
        }

        public override bool Run(CalibrationAction step, double blackBodyRealTemperature, CalibrationActionOptions calActOption)
        {           
            switch (step)
            {
                case CalibrationAction.ANALOG_OUTPUT:
                    CalibrationAnalogOutput(StingrayCaliResults.TerminalType);
                    break;

                case CalibrationAction.ANALOG_INPUT:
                    CalibrationAnalogInput(StingrayCaliResults.TerminalType);
                    break;

                case CalibrationAction.DETECTOR_OFFSET_1:
                        
                    break;

                case CalibrationAction.DETECTOR_OFFSET_2:

                    break;

                case CalibrationAction.CALIPOINT1:
                    System.Threading.Thread.Sleep(StingrayDataPool.CalibratingWaitTime);//delay time ,waiting the AD is stable
                    GatheringIrAdValue(StingrayDataPool.SampleCount);
                    //AdFromUnit = 40137;//just for debug simulate
                    //AmbFromUnit = 27.9;//jusr for debug simulate
                    StingrayDataPool.TP1Value = AdFromUnit;//save AD into Datapool
                    StingrayDataPool.AmbTemp = AmbFromUnit;//save Amb into Datapool
                    StingrayDataPool.TcalF[0] = CALMATHLIB.CtoF(blackBodyRealTemperature);//save DCI Value into Datapool 
                    StingrayDataPool.TambK1 = CALMATHLIB.CtoK(AmbFromUnit);
                    StingrayDataPool.TambK2 = CALMATHLIB.CtoK(AmbFromUnit);
                    //dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));
                    break;

                case CalibrationAction.CALIPOINT2:
                    System.Threading.Thread.Sleep(StingrayDataPool.CalibratingWaitTime);//delay time ,waiting the AD is stable
                    GatheringIrAdValue(StingrayDataPool.SampleCount);//gether AD from unit，10 means to get 10 Ad counts then get average value
                    //AdFromUnit = 40958;//just for debug simulate                    
                    StingrayDataPool.TP2Value = AdFromUnit;
                    StingrayDataPool.TcalF[1] = CALMATHLIB.CtoF(blackBodyRealTemperature);//save DCI Value into Datapool 
                    //  dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));
                    break;

                case CalibrationAction.CALIPOINT3:
                    System.Threading.Thread.Sleep(StingrayDataPool.CalibratingWaitTime);//delay time ,waiting the AD is stable
                    GatheringIrAdValue(StingrayDataPool.SampleCount);//gether AD from unit，10 means to get 10 Ad counts then get average value
                    //AdFromUnit = 44218;//just for debug simulate
                    StingrayDataPool.TP3Value = AdFromUnit;
                    StingrayDataPool.TcalF[2] = CALMATHLIB.CtoF(blackBodyRealTemperature);//save DCI Value into Datapool 

                    //finish Ad gather then start to curve except P3 model   
                    if (FlagP3Model == 0)
                    {
                        CALMATHLIB.IrCalmathLib(ref StingrayDataPool);
                    }
                    else
                    {

                    }

                    //dispatchProgressChangedEvent(new System.ComponentModel.ProgressChangedEventArgs(progressCounter++, this));
                    break;

                case CalibrationAction.CALIPOINT4:
                    if (FlagP3Model == 1)
                    {
                        double tpv1 = 0, tpv2 = 0, tpv3 = 0, tpv4 = 0;
                        double tcf1 = 0, tcf2 = 0, tcf3 = 0, tcf4 = 0;
                        long[] LowPSdTemp = new long[4];
                        float tptemp = 0f;
                        int cnttemp = 0;

                        System.Threading.Thread.Sleep(StingrayDataPool.CalibratingWaitTime);//delay time ,waiting the AD is stable
                        GatheringIrAdValue(StingrayDataPool.SampleCount);
                        //AdFromUnit = 138278;
                        //Get the 4th point value
                        StingrayDataPool.TP4Value = AdFromUnit;
                        StingrayDataPool.TcalF[3] = CALMATHLIB.CtoF(blackBodyRealTemperature);

                        tpv1 = StingrayDataPool.TP1Value;
                        tpv2 = StingrayDataPool.TP2Value;
                        tpv3 = StingrayDataPool.TP3Value;
                        tpv4 = StingrayDataPool.TP4Value;

                        tcf1 = StingrayDataPool.TcalF[0];
                        tcf2 = StingrayDataPool.TcalF[1];
                        tcf3 = StingrayDataPool.TcalF[2];
                        tcf4 = StingrayDataPool.TcalF[3];

                        /*Step1: Do high 3 point (25 75 150) IR calmath
                            * */
                        //Update 1~3 point value and DCI value
                        StingrayDataPool.TP1Value = tpv2;
                        StingrayDataPool.TP2Value = tpv3;
                        StingrayDataPool.TP3Value = tpv4;
                        //
                        StingrayDataPool.TcalF[0] = tcf2;
                        StingrayDataPool.TcalF[1] = tcf3;
                        StingrayDataPool.TcalF[2] = tcf4;

                        CALMATHLIB.IrCalmathLib(ref StingrayDataPool);
                        StingrayDataPool.P3PSdOfs1 = StingrayDataPool.PSdOfs1;
                        StingrayDataPool.P3PSdOfs2 = StingrayDataPool.PSdOfs2;
                        for (int i = 0; i < StingrayDataPool.PSdTblNum; i++)
                        {
                            StingrayDataPool.P3BPTbl[i] = StingrayDataPool.BPTbl[i];
                            StingrayDataPool.P3PSd1Tbl[i] = StingrayDataPool.PSd1Tbl[i];
                            StingrayDataPool.P3PSd2Tbl[i] = StingrayDataPool.PSd2Tbl[i];
                        }

                        /*Step2: Do high 3 point (75 150 400) IR calmath
                            * */
                        //Update 1~3 point value and DCI value
                        StingrayDataPool.TP1Value = tpv1;
                        StingrayDataPool.TP2Value = tpv2;
                        StingrayDataPool.TP3Value = tpv3;
                        //
                        StingrayDataPool.TcalF[0] = tcf1;
                        StingrayDataPool.TcalF[1] = tcf2;
                        StingrayDataPool.TcalF[2] = tcf3;
                        CALMATHLIB.IrCalmathLib(ref StingrayDataPool);
                        //use the high bp and psd cover the low bp and psd for 16th points
                        for (int i = 16; i < StingrayDataPool.PSdTblNum; i++)
                        {
                            StingrayDataPool.BPTbl[i] = StingrayDataPool.P3BPTbl[i];
                            StingrayDataPool.PSd1Tbl[i] = StingrayDataPool.P3PSd1Tbl[i] + (StingrayDataPool.P3PSdOfs1 - StingrayDataPool.PSdOfs1);
                            StingrayDataPool.PSd2Tbl[i] = StingrayDataPool.P3PSd2Tbl[i] + (StingrayDataPool.P3PSdOfs2 - StingrayDataPool.PSdOfs2);

                        }
                        // get the real P3 BP and PsdTab;
                        StingrayDataPool.P3PSdOfs1 = StingrayDataPool.PSdOfs1;
                        StingrayDataPool.P3PSdOfs2 = StingrayDataPool.PSdOfs2;

                        for (int i = 0; i < 4; i++)
                        {
                            LowPSdTemp[i] = StingrayDataPool.LowPSd2P3Tbl[i];
                        }

                        for (int i = 0; i < StingrayDataPool.PSdTblNum; i++)
                        {
                            StingrayDataPool.P3BPTbl[i] = StingrayDataPool.BPTbl[i];
                            StingrayDataPool.P3PSd1Tbl[i] = StingrayDataPool.PSd1Tbl[i];
                            StingrayDataPool.P3PSd2Tbl[i] = StingrayDataPool.PSd2Tbl[i];
                        }

                        tptemp = StingrayDataPool.P3CaliOffset + (StingrayDataPool.P3BPTbl[21] / 36 - 200);
                        cnttemp = (int)(StingrayDataPool.P3PSdOfs2 + StingrayDataPool.PSd2Tbl[21]);

                        /*Setp3:
                            * */
                        //Update 1~3 point value and DCI value
                        StingrayDataPool.TP1Value = tpv3;
                        StingrayDataPool.TP2Value = cnttemp;// tptemp;
                        StingrayDataPool.TP3Value = tpv4;
                        //
                        StingrayDataPool.TcalF[0] = tcf3;
                        StingrayDataPool.TcalF[1] = tptemp;// StingrayDataPool.P3BPTbl[21] / 36 - 200;
                        StingrayDataPool.TcalF[2] = tcf4;
                        //StingrayDataPool.TcalF[2] = StingrayDataPool.TcalF[3];
                        CALMATHLIB.IrCalmathLib(ref StingrayDataPool);

                        //  UNIT.Dtp.PSdOfs1 = UNIT.Dtp.P3PSdOfs1;
                        //  UNIT.Dtp.PSdOfs2 = UNIT.Dtp.P3PSdOfs2; 

                        for (int i = 16; i < StingrayDataPool.PSdTblNum; i++)
                        {
                            StingrayDataPool.P3BPTbl[i] = StingrayDataPool.BPTbl[i];
                            StingrayDataPool.P3PSd1Tbl[i] = StingrayDataPool.PSd1Tbl[i] + (StingrayDataPool.PSdOfs1 - StingrayDataPool.P3PSdOfs1);
                            StingrayDataPool.P3PSd2Tbl[i] = StingrayDataPool.PSd2Tbl[i] + (StingrayDataPool.PSdOfs2 - StingrayDataPool.P3PSdOfs2);

                        }
                        //
                        StingrayDataPool.PSdOfs1 = StingrayDataPool.P3PSdOfs1;
                        StingrayDataPool.PSdOfs2 = StingrayDataPool.P3PSdOfs2;
                        for (int i = 0; i < StingrayDataPool.PSdTblNum; i++)
                        {
                            StingrayDataPool.BPTbl[i] = StingrayDataPool.P3BPTbl[i];
                            StingrayDataPool.PSd1Tbl[i] = StingrayDataPool.P3PSd1Tbl[i];
                            StingrayDataPool.PSd2Tbl[i] = StingrayDataPool.P3PSd2Tbl[i];

                        }
                        for (int i = 0; i < 4; i++)
                        {
                            StingrayDataPool.LowPSd2P3Tbl[i] = LowPSdTemp[i];
                        }
                        // 
                        for (int i = 9; i < 15; i++)
                        {
                            StingrayDataPool.BPTbl[i] = StingrayDataPool.BPTbl[i] + 60; //compate 1.8C for curve
                        }
                        //
                    }
                    else
                    {

                    }
                    break;
                case CalibrationAction.WRITE_CALIBRATION_DATA_TO_DEVICE:
                    System.Threading.Thread.Sleep(1000);  //delay 1s
                    WriteCalibrationDataToDevice();
                    SaveIrCalibrationResultIntoResultDatapool();
                    break;

                case CalibrationAction.POWER_CALCULATION:
                    System.Threading.Thread.Sleep(1000);  //delay 1s
                    PowerConsumptionCheck(StingrayCaliResults.TerminalType);
                    break;
                case CalibrationAction.TC_CALIBRATION:

                    break;
                case CalibrationAction.TC_OUTPUT:

                    break;

                default:
                    throw new NotSupportedException();

            }
            
            return true;
            //throw new NotSupportedException();
        }
        public void SaveIrCalibrationResultIntoResultDatapool()
        {
            StingrayCaliResults.CalibrationWaitingTime = StingrayDataPool.CalibratingWaitTime;
            StingrayCaliResults.AmbTempSunit = StingrayDataPool.AmbTempS;
            StingrayCaliResults.AmbTempCunit = StingrayDataPool.AmbTemp;
            StingrayCaliResults.CalEmissivity = StingrayDataPool.CalEmiss;
            StingrayCaliResults.Alpha = StingrayDataPool.Alpha;
            StingrayCaliResults.AlphaLow = StingrayDataPool.Alpha_Low;
            StingrayCaliResults.AdValueFromUnit[0] = StingrayDataPool.TP1Value;
            StingrayCaliResults.AdValueFromUnit[1] = StingrayDataPool.TP2Value;
            StingrayCaliResults.AdValueFromUnit[2] = StingrayDataPool.TP3Value;
            StingrayCaliResults.AdValueFromUnit[3] = StingrayDataPool.TP4Value;
            StingrayCaliResults.BlackBodyRealTemperature = StingrayDataPool.TcalF;
            StingrayCaliResults.BreakPointEnergryTable = BPSendTalbe;
            StingrayCaliResults.DetectorEnergryTable = PSd2SendTable;
            StingrayCaliResults.DetectorEnergryOffset = StingrayDataPool.PSdOfs2;
        }

        #region Custom exception handling application
        public class CustomException : ApplicationException
        {
            public CustomException(string message)
                : base(message)
            {
            }
        }
        public class CustomExceptionCode
        {            
            public enum ErrorCode
            {
                NONE = 0,
                PowerTooHigh = 1,
                TempratureHigh = 1,
                TempratureLow = 2,
                //.....
                ALL = 185,
            }

            //Calculate calibration result
            //.....
           

        }
        #endregion

        /*    public override bool Verify(CalibrationAction step, double blackBodyRealTemperature)
        {
            switch (step)
            {
                case CalibrationAction.VALIDATE_POINT1:
                    //for some stuff like validation it is nice to see what the device reads currently
                    var objectTemp = communicationInterface.GetIntFromDevice("ObjectTemperature", "T");
                    var Tint = communicationInterface.GetIntFromDevice(Strings.IHV_COMMAND_DESCRIPTION, CaliCmd.T_INT);
                    //dispatchDeviceReadingEvent(new DeviceReadingEventArgs(objectTemp, Tint)); //send an event here
                    break;
                case CalibrationAction.VALIDATE_POINT2:
                    break;
                case CalibrationAction.VALIDATE_POINT3:
                    break;
                default:
                    break;
            }
            throw new NotSupportedException();
        }*/
        //
        private void AnalogCaliResult(int status)
        {
            switch (status)
            {
                case 0:

                    calibrationboxcomm.SendCommand("[CLOSE001]", 1);
                    calibrationboxcomm.SendCommand("[CLOSE002]", 1);
                    System.Threading.Thread.Sleep(200);

                    calibrationboxcomm.Disconnect();
                    multimetercomm.Disconnect();


                    break;
                case 1:

                    calibrationboxcomm.SendCommand("[CLOSE001]", 1);
                    calibrationboxcomm.SendCommand("[CLOSE002]", 1);
                    System.Threading.Thread.Sleep(200);

                    calibrationboxcomm.Disconnect();
                    multimetercomm.Disconnect();


                    break;

            }
        }
        
        //In initialization, to write config default parameters
        private void getCaliParams()
        {          
            string result = "";           

            if (!communicationInterface.IsOpen)
            {
                communicationInterface.Connect();
            }
            else 
            {

            }
                
            //James Write the J,K,L,M
            result = communicationInterface.Set("%DI", "J" + StingrayDataPool.TargetADThresJump.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                // return true;
                System.Threading.Thread.Sleep(100);
            }
            else
            {//didnt work
                // return false;
            }

            result = communicationInterface.Set("%DI", "K" + StingrayDataPool.TargetADThresStab.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                // return true;
                System.Threading.Thread.Sleep(100);
            }
            else
            {//didnt work
                // return false;
            }

            result = communicationInterface.Set("%DI", "L" + StingrayDataPool.TargetADDepthInit.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                // return true;
                System.Threading.Thread.Sleep(100);
            }
            else
            {//didnt work
                // return false;
            }

            result = communicationInterface.Set("%DI", "M" + StingrayDataPool.TargetADdepthStep.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {               
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                
            }

            //Config the ADC1
            if (StingrayDataPool.Ravg == 1)  
            {
                ADC1FLTRavg = 0x4000;
            }
            else
            {
                ADC1FLTRavg = 0;
            }

            if (StingrayDataPool.Notch == 1)
            {
                ADC1FLTNotch = 0x0080;
            }
            else
            {
                ADC1FLTNotch = 0;
            }

            if (StingrayDataPool.Sinc4 == 1)
            {
                ADC1FLTSinc4 = 0x1000;
            }
            else
            {
                ADC1FLTSinc4 = 0;
            }

            if (StingrayDataPool.Chop == 1)
            {
                ADC1FLTChop = 0x8000;
            }
            else
            {
                ADC1FLTChop = 0;
            }

            ADC1FilterConfig = ADC1FLTRavg | ADC1FLTNotch | ADC1FLTSinc4 | ADC1FLTChop;
            ADC1FilterConfig = ADC1FilterConfig << 16;
            ADC1FLT = StingrayDataPool.AF << 8;
            ADC1SF = StingrayDataPool.SF;
            ADC1FilterConfig = ADC1FilterConfig + ADC1FLT + ADC1SF;
            //Write config parameter for ADC1
            result = communicationInterface.Set("%DI", "N" + ADC1FilterConfig.ToString());            
            
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {

            }

            //Write AverageCo for Post-progressing
            result = communicationInterface.Set("%DI", "O" + StingrayDataPool.AverageCo.ToString());
            if (communicationInterface.Protocol.ExtractResult(ref result, "%DI") && result == "T")
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {

            }



        }

    #region Do result parsing
        private bool processResult(string command, string result, ref float val)
        {
            //int i;
            string v;
            string c;
            //string d, e;
            //float f, j;


            bool rval = false;
            if (result == null)
                return false;
            switch (command)
            {
                case "%DR":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(3, v.Length - 3);
                    val = float.Parse(v);
                    rval = true;
                    break;
                //James added the %IHV command to get AD 
                case "%IHV":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = float.Parse(v);
                    break;
                case "%ILV":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = float.Parse(v);
                    break;
                //James added the %F1V command to get AD 
                case "%F1V":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = float.Parse(v);
                    break;
                //James added the %F1V command to get AD 
                case "%F2V":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = float.Parse(v);
                    break;
                case "MEAS1?":
                    if (!result.StartsWith("+" + "F"))
                        rval = true;
                    else
                    {
                        rval = false;
                        break;
                    }
                   
                    ///		result:	"+3.9403E-3 ADC\r\n=>\r\n"	
                    ///             "=>\r\n-27.836E-3 ADC\r\n=>\r\n"
                    Match match = Regex.Match(result, @"[+-]\b[0-9]\S*");
                    if(match.Success)
                    {
                        string matchStr = match.Groups[0].Value.ToString(); //Matched string: +3.9403E-3
                        
                        //displayInfo("Info_Show", float.Parse(matchStr).ToString());
                        val = float.Parse(matchStr);
                    }
                    
                    /*c = result.Substring(1, (result.Length - 14));//
                    i = result.Length;
                    d = result.Substring(1, (result.Length - 11));//d=4.0059
                    i = c.Length;
                    e = d.Substring((i + 1), (d.Length - i - 1));
                    f = float.Parse(c);
                    i = (int.Parse(e));

                    j = (float)Math.Pow(10, i);//unit mA to A,mV to V,
                    val = f * j;*/
                    //val = 1000000*(float)val /(float) Resistance;//for i to V convert to calibrate I 20160919
                    // val = 1000 * (float)val;//uA
                    break;

                case "?Q":
                case "?T":
                    c = command.Replace("?", "!");
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(c))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(2, v.Length - 2);
                    if (v == "<<<<<<")
                        v = "0000";

                    val = float.Parse(v);
                    rval = true;
                    break;
                case "?I":
                    c = command.Replace("?", "!");
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(c))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(2, v.Length - 2);
                    val = float.Parse(v);
                    rval = true;
                    break;
                case "?XR":
                case "?XJ":
                    c = command.Replace("?", "!");
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(c))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(3, v.Length - 3);
                    val = float.Parse(v);
                    rval = true;
                    break;

            }
            return rval;
        }

        private bool processResult(string command, string result, ref int val)
        {
            string v;
            string c;
            bool rval = false;
            if (result == null)
                return false;
            switch (command)
            {
                case "%DR":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(3, v.Length - 3);
                    val = int.Parse(v);
                    rval = true;
                    break;
                //James added the %IHV command to get AD 
                case "%IHV":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = int.Parse(v);
                    rval = true;
                    break;
                case "%ILV":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = int.Parse(v);
                    rval = true;
                    break;
                case "%F1V":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = int.Parse(v);
                    rval = true;
                    break;
                case "%F2V":
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(command))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(4, v.Length - 4);
                    val = int.Parse(v);
                    rval = true;
                    break;
                case "?Q":
                    c = command.Replace("?", "!");
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(c))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(2, v.Length - 2);
                    val = int.Parse(v);
                    rval = true;
                    break;


                case "?XR":
                case "?XJ":
                    c = command.Replace("?", "!");
                    v = result.Substring(0, result.IndexOf("\r"));
                    if (!v.StartsWith(c))
                    {
                        rval = false;
                        break;
                    }
                    v = v.Substring(3, v.Length - 3);
                    val = int.Parse(v);
                    rval = true;
                    break;


            }
            return rval;
        }

        private bool processResult(string command, string result)
        {
            bool rval = false;
            if (result == null)
                return false;
            switch (command)
            {
                case "%ACS":
                case "%AVHS":
                case "%AIRS":
                case "%ATCS":
                case "%ACE":
                case "%CS":
                case "%CE":
                case "%OV":
                case "%OA":
                case "%OTC":
                case "%OT":
                case "%RO":
                case "%RC":
                case "%TTS":
                case "%TTE":
                case "%DI":
                case "%D":
                    if (result.StartsWith(command + "T"))
                        rval = true;
                    else
                        rval = false;
                    break;
                case "O":
                    if (result.StartsWith("!" + command))
                        rval = true;
                    else
                        rval = false;
                    break;
                case "XO":
                    if (result.StartsWith("!" + command))
                        rval = true;
                    else
                        rval = false;
                    break;
                case "%XFW":
                    if (result.StartsWith(command + "T"))
                        rval = true;
                    else
                        rval = false;
                    break;
            }
            return rval;
        }

        private bool processResult(string command, string result, int type)
        {
            bool rval = false;
            // Cali Command
            if (type == 0)
            {
                if (result == null)
                    return false;
                switch (command)
                {
                    case "%ACS":
                    case "%AVHS":
                    case "%AIRS":
                    case "%ATCS":
                    case "%ACE":
                    case "%CS":
                    case "%CE":
                    case "%OV":
                    case "%OA":
                    case "%OTC":
                    case "%OT":
                    case "%RO":
                    case "%RC":
                    case "%TTS":
                    case "%TTE":
                    case "%VG":
                    case "%VO":
                    case "%DI":
                    case "%D":
                        if (result.StartsWith(command + "T"))
                            rval = true;
                        else
                            rval = false;
                        break;
                    case "%XFW":
                        if (result.StartsWith(command + "T"))
                            rval = true;
                        else
                            rval = false;
                        break;
                }
                return rval;
            }
            // User Command
            if (type == 1)
            {
                if (result == null)
                    return false;
                switch (command)
                {
                    case "?T":
                    case "?Q":
                    case "?I":
                    case "?E":
                    case "?XV":
                    case "?XR":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        break;
                    case "?XRA":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        break;
                    case "*IDN?"://added by James for Stingray
                        if (result.StartsWith("FLUKE"))
                            rval = true;
                        else
                            rval = false;
                        break;
                    case "U":
                    case "E":
                    case "DG":
                    case "DO":
                    case "XF":  // added by jimmy for v1.1.0
                        if (result.StartsWith("!" + command))
                            rval = true;
                        else
                            rval = false;
                        break;
                }
                return rval;
            }
            return rval;
        }

        private bool processResult(string command, string result, int type, ref object val, object flag)
        {
            bool rval = false;
            string s = "";
            // Cali Command
            if (type == 0)
            {
                if (result == null)
                    return false;
                switch (command)
                {
                    case "MEAS1?"://James added the command for 8808A
                        if (!result.StartsWith("+" + "F"))
                            rval = true;
                        else
                        {
                            rval = false;
                            break;
                        }
                        s = result.Substring(1, result.Length - 14);
                        val = double.Parse(s);

                        break;

                    case "%DR":
                        if (!result.StartsWith(command + "F"))
                            rval = true;
                        else
                        {
                            rval = false;
                            break;
                        }
                        s = result.Substring(4, result.Length - 6);
                        val = (int)Utility.String2UInt(s);
                        break;

                    case "%DIR":
                        if (!result.StartsWith(command + "F"))
                            rval = true;
                        else
                        {
                            rval = false;
                            break;
                        }
                        s = result.Substring(5, result.Length - 7);
                        if (flag.ToString() == "String")
                            val = s;
                        else
                            val = (int)Utility.String2UInt(s);
                        break;
                }
                return rval;
            }
            // User Command
            if (type == 1)
            {
                if (result == null)
                    return false;
                switch (command)
                {
                    case "?T":
                    case "?I":
                    case "?E":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(2, result.Length - 4);
                        val = float.Parse(s);
                        break;
                    case "?Q":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(2, result.Length - 4);
                        val = int.Parse(s);
                        break;
                    case "?U":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(2, result.Length - 4);
                        val = s;
                        break;
                    case "?XV":
                    case "?XR":
                    case "?XU":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        val = result.Substring(3, result.Length - 5);
                        break;
                    case "?XB":
                    case "?XH":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(3, result.Length - 5);
                        val = float.Parse(s);
                        break;
                    case "?DG":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(3, result.Length - 5);
                        val = float.Parse(s);
                        break;
                    case "?DO":
                        if (result.StartsWith("!" + command.Substring(1, command.Length - 1)))
                            rval = true;
                        else
                            rval = false;
                        s = result.Substring(3, result.Length - 5);
                        val = float.Parse(s);
                        break;

                }
                return rval;
            }
            return rval;
        }
    #endregion
    }
}
