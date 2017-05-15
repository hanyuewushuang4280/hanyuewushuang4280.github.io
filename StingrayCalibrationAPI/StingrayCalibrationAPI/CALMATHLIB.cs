using System;
using NationalInstruments;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Math;
using System.IO;


namespace StingrayCalibrationAPI
{
    //Datapool start
    public enum CAL_COMMAND : int
    {
        MEAS_N_IR_TAR1 = 0x00007777,
        MEAS_N_IR_TAR2 = 0x00006666,
        MEAS_N_IR_AMB = 0x00008888,
        MEAS_N_REF1 = 0x00005555,
        MEAS_N_REF2 = 0x00004444,
        MEAS_N_BAT = 0x00009999,
        MEAS_N_TC_TAR = 0x00001111,
        MEAS_N_TC_OFS = 0x00002222,
        MEAS_N_TC_AMB = 0x00003333,
        MEAS_MCLK = 0x0000aaaa,
        MEAS_IICTEST = 0x0000bbbb
    };

    public enum CAL_UNITCOUNT : int
    {
        d1_0 = 0, d1_1, d1_2, d1_3, d1_4, d2_0, d2_1, d2_2, d2_3, d2_4, a, bat, tc0, tc1, tca
    };

    public enum CAL_AICHANNEL : int
    {
        vcc = 0, ref1, ref2, bat, tc, tca, wip
    };

    public struct DATA_POOL
    {
        // ***** Cali Data *****
        public double[] Volt, VoltStdev;				// Voltage, Rms Noise (V)
        public double[] UN, UNStdev, UNr1, UNr2;		// Average counts, Standard deviation, Ref 1 counts, Ref 2 counts
        public double[] TcalF;							// Cal points @ Cal Emissivity (F)
        public double MCLK_ms;
        public byte[] UnitCode;

        // ***** Calculate Arguments *****
        #region Genaral constants

        public const int SIZEUUTID = 6;			// Array with the UUTID
        public const int STRINGUUTID = 16;			// Size of the UUTID string

        public const double PSd1_P = 1 << 18;

        //public const double PSd2_P  = 1 << 20;         //James  chang to 20 from 24
        // public const double PSd2_P = 1 << 16;         //James  chang to 16 from 24
        //   public const double PSd2_P = 1 << 24;         //24 BIT FOR LTH7 20160804 P7,P3,LT,5,LTP,LTH5
        public double PSd2_P;         //24 BIT FOR LTH7 20160804 P7,P3,LT,5,LTP,LTH5
        // public const double PSd2_P = 1 << 18;         //18 BIT FOR G7 MT 
        public const double PSa_P = 1 << 16;

        public const double PSbat_P = 1 << 16;
        public const double PStc_P = 1 << 21;
        public const double PStca_P = 1 << 16;
        public const double PSCO = 1 << 16;

        #endregion

        public double[] UPOut;  // 0 1 2 PsOut1(Low gain)  3 4 5 PsOut2 (High gain)
        public double SpecScale2, RespScale2, Ceng2, Pamb2;	// Spectral shift (%), system responsivity
        public double SpecScale1, RespScale1, Ceng1, Pamb1;	// Spectral shift (%), system responsivity
        public double CalAmb;
        public double TambK2, TambK1, TambS2, TambS1, PdOfs2, PdOfs1;
        public long FTC1Gain, FTC1Ofs, FTC2Gain, FTC2Ofs, VGain, VOfs, IGain, IOfs, TCGain, TCOffset;//James change the double into long for sending to the unit 20161026
        public double VoutValue, ADGain, ADOffset;
        public double AmbG, AmbOfs, TcAG, TcAOfs;		// LM34 Gain and Offset, Ambient and TC ambient

        public double CurveFitSigma1, CurveFitSigma2;	// Sigma of Energy Curve Fitting
        public double TaF;								// Detector ambient temperature (F) 
        public double TtcaF;							// TC terminal ambient temperature (F)
        public double TwipF;							// WIP container temperature (F)
        public double TtcaCompF;						// TC terminal ambient temperature compensated with Twip(F)
        public double PtcOfs;							// Offset between calibrated TC curve and given data
        public double MargScale;						// Scale factor of margin spec.
        public double SpanErrFudge;						// When <0, SpanErr -= this to amplify feedback and kick it positive
        public double[] Terr1F, Terr2F;					// The difference between energy curve and cali temperature

        // ***** Unit Defaults *****
        public double PercMargCenterF;					// Center of percent error margin (F)
        public double InitMargScale;					// Initial MargScale
        public double InitDegMargF, InitPercMarg;		// Initial error margins (F), (%)
        public double MargScaleMin, MargScaleMax;		// Min and max values for ErrScale
        public double MargScaleDeriv;					// SpanErr sensitivity to ErrScale: smaller = slower convergency
        public double JumpFactor;						// Determines jump size when choosing a divisor:
        // should be <= (shortest BP span) / StodErr / 10
        // (to ensure at least 10 jumps per span)
        public int PSd2_Ppower;
        
        public double InitRespScale1, InitSpecScale1;
        public double RespScaleMin1, RespScaleMax1;		// Min and max system Responsivity
        public double SpecScaleMin1, SpecScaleMax1;		// Wavelength shift limit (%)
        public double InitRespScale2, InitSpecScale2;
        public double RespScaleMin2, RespScaleMax2;		// Min and max system Responsivity
        public double SpecScaleMin2, SpecScaleMax2;		// Wavelength shift limit (%)
        public double CurveFitAdj1, CurveFitAdj4;
        // public double Iout_Gain ,Iout_Offset;
        public long ModelNum;
        public string ModelName;
        public long AmbTempS;
        public int  AdcFilterCoef, AdcThreshold, TargetADThresJump, TargetADThresStab, TargetADDepthInit, TargetADdepthStep;
        public int Alpha, Alpha_Low, Alpha_High;
        public int SampleCount;
        public int SF, AF, Ravg, Notch, Chop, Sinc4;
        public int AverageCo;
        // end
        public double[] LineWave, LineAmpl;				// Lines wavelength and amplitude arrays
        public double CurveFitSigmaMax;					// Sigma of Energy Curve Fitting
        public double CalEmiss, DCIEmiss;				// Calibration & DCI emissivity
        public double VtcaOfs, VtcaGn;					// Offset voltage and gain of LM34 on cal station (V)
        public double VwipOfs, VwipGn;					// Offset voltage and gain of LM34 on WIP container (V)
        public double TCACompFactor;					// TC terminal temperature compensation factor 
        public double[] TDefF;							// Default cal temperature
        public double[] TaDefF, PaDef;					// Detector Ambient Thermistor Data
        public double[] TtcJDefS, VtcJDef;				// Thermocouple Type J Data
        public double[] TtcKDefS, VtcKDef;				// Thermocouple Type K Data
        public double[] TtcaDefF, PtcaDef;				// Thermocouple Ambient Thermistor Data

        public int PSdTblNum;							// Number of Thermopile Energy Points
        public int PSaTblNum;							// Number of Detector Ambient Thermistor Energy points
        public int PStcTblNum;							// Number of Thermocouple Energy points
        public int PStcaTblNum;							// Number of TC Ambient Thermistor Energy points

        public int P3CaliOffset;

        public double TLowF, THighF;					// Temperature range margins (F)
        public double DLowF, DHighF;
        public double TaLowF, TaHighF;					// Unit ambient range limits (F)
        public double TaDriftMinF, TaDriftMaxF, TaDriftGoalF;			// Thermistor min and max drift during cal
        public double TwipDriftMinF, TwipDriftMaxF,TwipDriftGoalF;		// WIP container temperature min and max drift

        public double[] UNMin, UNmax, UNStdevMax;		// Counts and RMS Noise Limits: d1_0,d1_1,d1_2,d1_3,d1_4,d2_0,d2_1,d2_2,d2_3,d2_4,a,bat,tc0,tc1,tca

        public bool FuseBlow;

        // ***** Cali Results *****
        public long[] LowPSd2Tbl;							// -13 -32 122 212F
        public long[] LowPSd2P3Tbl;							// -13 -32 50 68F for P3
        public long[] LowBPTbl;							    // -13 -32 122 212F
        public long[] LowBPP3Tbl;							// -13 -32 50 68F for P3
        public int PSCO1, PSCO2;
        public double PSbatTh;
        public long[] PSd1Tbl;								// Energy table of detector 1th channel - results will be stored here
        public long[] PSd2Tbl;								// Energy table of detector 2nd channel - results will be stored here
        public long PSdOfs1, PSdOfs2;
        public long[] BPTbl;								// Break point of energy table (0.1S)

        //Add for P3
        public long[] P3BPTbl;
        public long[] P3PSd1Tbl;							// Energy table of detector 1th channel - results will be stored here
        public long[] P3PSd2Tbl;							// Energy table of detector 2nd channel - results will be stored here
        public long P3PSdOfs1, P3PSdOfs2;
        //
        //public int[] PSaTbl;								// Ambient thermistor table
        //public int[] PStcJTbl;							// Thermocouple Type J table
        //public int[] BPtcTbl;								// Break point of thermocouple table (0.1S)
        //public int[] PStcKTbl;							// Thermocouple Tyep K table
        public long TempSHigh, TempSLow;					// Temperature range high/low limits (0.1S)
        public long PSd1Max, PSd2Max, PStcMax;				// To prevent the interpolation of LPF firmware from overflow
        //public int PSbatThDead;							// Battery dead threshold
        public int SerNumber;								// Serial Number
        public int[] UUTID;									// Unit ID

        // ***** Cal Station Info *****
        public int StationSN;

        // ***** Cal Waiting Time Between Point to Point *****
        public int CalibratingWaitTime;

        // ***** DCI calibration point *****
        public int CaliTP1;
        public int CaliTP2;
        public int CaliTP3;
        public int CaliTP4;

        // ***** Cal Process Data *****
        public double FTC1Low;
        public double FTC1LowCounts;
        public double FTC1High;
        public double FTC1HighCounts;
        public double FTC2Low;
        public double FTC2LowCounts;
        public double FTC2High;
        public double FTC2HighCounts;
        public double VoutLow;
        public double VoutLowCounts;
        public double VoutHigh;
        public double VoutHighCounts;
        public double IoutLow;
        public double IoutLowCounts;
        public double IoutHigh;
        public double IoutHighCounts;
        public double TCout;
        public double TCLow;
        public double TCLowCounts;
        public double TCHigh;
        public double TCHighCounts;
        public double TP1Value;
        public int TP1Counts;
        public double TP2Value;
        public int TP2Counts;
        public double TP3Value;
        public int TP3Counts;
        public double TP4Value;
        public int TP4Counts;
        public double AmbTemp;
        public double Emiss;

        // ******** Amb Temp Cali ************
        public double AmbIRCounts;
        public double AmbTCCounts;
    }

    /// <summary>
    /// UNIT 的摘要说明。
    /// </summary>
    public class UNITLIB
    {
        //		private static bool LPFUnitInit = false;

        public static DATA_POOL Dtp = new DATA_POOL();

        public static string Hex2Str(double data)
        {
            string str1, str2;
            int d;
            d = Convert.ToInt32(data);
            str1 = d.ToString("X8");
            str2 = str1.Substring(6, 2) + " " + str1.Substring(4, 2) + " " + str1.Substring(2, 2) + " " + str1.Substring(0, 2);
            return str2;
        }


        public static string[] Hex2StrArray(double data)
        {
            return Hex2StrArray(Convert.ToInt32(data));
        }


        public static string[] Hex2StrArray(int data)
        {
            string str1;
            string[] str2 = new string[4];
            str1 = data.ToString("X8");
            str2[0] = str1.Substring(6, 2);
            str2[1] = str1.Substring(4, 2);
            str2[2] = str1.Substring(2, 2);
            str2[3] = str1.Substring(0, 2);
            return str2;
        }


        public static uint String2UInt(string str)
        {
            char[] hexchar = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int i, j;
            uint d;
            char[] ch;

            d = 0;
            str.Replace(" ", "");
            ch = str.ToCharArray();
            for (i = 0; i < ch.Length; i++)
            {
                for (j = 0; j < 16; j++)
                {
                    if (ch[ch.Length - i - 1].Equals(hexchar[j])) break;
                }
                if (j != 16) d += ((uint)j << (i * 4));
            }
            return d;
        }

        
        public static void RenewCodeFile()
        {
            int i, j;
            string str1;
            string[] str2 = new string[1 * 2 + 4 * 5 + 6 * 112 + 2 * 2 + 4 * 4];
            string[] str3;
            StreamReader sr;
            StreamWriter sw;

            if (File.Exists("TMPCODE.TXT"))
                File.Delete("TMPCODE.TXT");

            j = 2;

            str3 = Hex2StrArray(Dtp.PSCO1);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            str3 = Hex2StrArray(Dtp.PSCO2);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            str3 = Hex2StrArray(Dtp.PSbatTh);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            for (i = 0; i < 28; i++)
            {
                str3 = Hex2StrArray(Dtp.PSd1Tbl[i]);
                str2[j++] = str3[0];
                str2[j++] = str3[1];
                str2[j++] = str3[2];
                str2[j++] = str3[3];
            }

            for (i = 0; i < 28; i++)
            {
                str3 = Hex2StrArray(Dtp.PSd2Tbl[i]);
                str2[j++] = str3[0];
                str2[j++] = str3[1];
                str2[j++] = str3[2];
                str2[j++] = str3[3];
            }

            for (i = 0; i < 28; i++)
            {
                str3 = Hex2StrArray(Dtp.BPTbl[i]);
                str2[j++] = str3[0];
                str2[j++] = str3[1];
                str2[j++] = str3[2];
                str2[j++] = str3[3];
            }

 
            str3 = Hex2StrArray(Dtp.TempSHigh);
            str2[j++] = str3[0];
            str2[j++] = str3[1];

            str3 = Hex2StrArray(Dtp.TempSLow);
            str2[j++] = str3[0];
            str2[j++] = str3[1];

            str3 = Hex2StrArray(Dtp.PSd1Max);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            str3 = Hex2StrArray(Dtp.PSd2Max);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            str3 = Hex2StrArray(Dtp.PStcMax);
            str2[j++] = str3[0];
            str2[j++] = str3[1];
            str2[j++] = str3[2];
            str2[j++] = str3[3];

            /*			str3 = Hex2StrArray(Dtp.PSbatThDead);
                        str2[j++] = str3[0];
                        str2[j++] = str3[1];
                        str2[j++] = str3[2];
                        str2[j++] = str3[3];
            */
            sr = new StreamReader("LPF_FIN.TXT");
            sw = new StreamWriter("TMPCODE.TXT");

            do
            {
                str1 = sr.ReadLine();
                sw.WriteLine(str1);
            }
            while (!str1.StartsWith("@FC80"));

            str1 = sr.ReadLine();
            str2[0] = str1.Substring(0, 2);
            str2[1] = str1.Substring(3, 2);
            i = 0;
            j = 0;
            for (; str1 != null; )
            {
                for (i = 0; i < 16; i++)
                {
                    if (j == str2.Length) break;
                    str1 = str1.Insert(i * 3, str2[j++]);
                    str1 = str1.Remove(i * 3 + 2, 2);
                }
                sw.WriteLine(str1);
                str1 = sr.ReadLine();
            }
            sr.Close();
            sw.Close();
        }


    }


    //Datapool end James
	public delegate void MathErrorEventHandler(object sender, string errorstr);
	/// <summary>
	/// CALMATHLIB 的摘要说明。
	/// </summary>
	public class CALMATHLIB
	{
		private const double C0 = 273.15;		// Kelvin scale offset, K
		private const double C1 = 3.7415e-16;	// Blackbody radiation constant, watt*m*m
		private const double C2 = 1.4388e4;		// Blackbody radiation constant, um*K


		private const int NTANGENT = 4;			// Number of points in TDataF[] to calculate tangent line
		private const int NRUNS    = 25;		// Number of FullSet runs to choose BestErrScale from

		private static double[] LineWave = new double[4];
		private static double[] LineAmpl = new double[4];
		private static double Emiss;
		private static double DegMargS, PercMarg;
		private static double CurFitAdj1, CurFitAdj4; 

		public event MathErrorEventHandler MathError;

        //Meng add 20170418
        private const double TemLowerLimit = -200;//Lower limit -40 t0 -200

		private void OnError(string errorstr)
		{
			if ( MathError != null )
				MathError(this,errorstr);
		}

		public CALMATHLIB()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}


		#region public static methods for temperature unit convert
		// Fahrenheit <-> Stod
        //
        
		public static double FtoS(double f)    
		{
            return (f - ((TemLowerLimit) * 1.8 + 32)) * 5.0;
		}
		public static double StoF(double s)
		{
            return s / 5.0 + ((TemLowerLimit) * 1.8 + 32);
		}

		// Fahrenheit <-> Kelven
		public static double FtoK(double f)
		{
			return (f - 32.0) / 1.8 + 273.15;
		}
		public static double KtoF(double k)
		{
			return (k - 273.15) * 1.8 + 32.0;
		}

		// Fahrenheit <-> Celsius
		public static double FtoC(double f)
		{
			return (f - 32.0) / 1.8;
		}
		public static double CtoF(double c)
		{
			return (c * 1.8) + 32.0;
		}

		// Celsius <-> Stod
		public static double CtoS(double c)
		{
            return (c - TemLowerLimit) * 9.0;
		}
		public static double StoC(double s)
		{
            return s / 9.0 + TemLowerLimit;
		}

		// Celsius <-> Kelven
		public static double CtoK(double c)
		{
			return c + 273.15;
		}
		public static double KtoC(double k)
		{
			return k - 273.15;
		}

		// Kelven <-> Stod
		public static double KtoS(double k)
		{
            //return (k -273.15-( -40) * 9.0;//TemLowerLimit=40
            return (k - 273.15 - TemLowerLimit) * 9.0;
		}
		public static double StoK(double s)
		{
            //	return s / 9.0 +273.15-40;
            return s / 9.0 + 273.15 + TemLowerLimit;

		}

		#endregion

		#region Linear interpolation method
		/* linterp -----------------------------------------------------------

		Performs linear interpolation on a data array. The argument X array must be ascending. 
		If supplied X value is outside the X data array, the function performs extrapolation 
		by two last data points.

		Input:	ndata >= 2		size of X & Y data arrays
				xarray[ndata]	>>ascending<< X data array
				yarray[ndata]	Y data array

		Return:	Y value		

		*/
		public static double linterp(int[] xarray, double[] yarray, double xvalue)
		{
			int i;
			double[] xa = new double[xarray.Length];
			for( i = 0; i < xarray.Length; i++)
				xa[i] = (double)xarray[i];
			return linterp(xa,yarray,xvalue);
		}

		public static double linterp(long[] xarray, long[] yarray, double xvalue)
		{
			int i;
			double[] xa = new double[xarray.Length];
			double[] ya = new double[yarray.Length];

			for( i = 0; i < xarray.Length; i++)
				xa[i] = (double)xarray[i];
			for( i = 0; i < yarray.Length; i++)
				ya[i] = (double)yarray[i];

			return linterp(xa,ya,xvalue);
		}

		public static double linterp(double[] xarray, int[] yarray, double xvalue)
		{
			int i;
			double[] ya = new double[xarray.Length];
			for( i = 0; i < xarray.Length; i++)
				ya[i] = (double)yarray[i];
			return linterp(xarray,ya,xvalue);
		}

		public static double linterp(int[] xarray, int[] yarray, double xvalue)
		{
			int i;
			double[] xa = new double[xarray.Length];
			double[] ya = new double[xarray.Length];
			for( i = 0; i < xarray.Length; i++)
			{
				xa[i] = (double)xarray[i];
				ya[i] = (double)yarray[i];
			}
			return linterp(xa,ya,xvalue);
		}

		public static double linterp(double[] xarray, double[] yarray, double xvalue)
		{
			double	yvalue, slope;
			int	ispan;

			if(xarray.Length < 2) return yarray[0];	// on illegal call, return first element in Y array

			for(ispan = xarray.Length - 2; ispan > 0; ispan--)
				if(xvalue > xarray[ispan]) break;
   
			if(ispan == 0)		// interpolate in the first span or extrapolate below the data array
			{
				slope = (yarray[1] - yarray[0]) / (xarray[1] - xarray[0]);
				yvalue = yarray[1] - slope * (xarray[1] - xvalue);
			}
			else				// interpolate in the particular span or extrapolate above the data array
			{
				slope = (yarray[ispan + 1] - yarray[ispan]) / (xarray[ispan + 1] - xarray[ispan]);
				yvalue = yarray[ispan] + slope * (xvalue - xarray[ispan]);
			}
 
			return yvalue;
		}
		#endregion







		// Calculate the gain(G2) and offset(Ofs2) of the 2nd IR target channel, then calculate UPdOut2[4]
		private static void CalcuUPdOut2(ref DATA_POOL dtp)
		{
			double[] X = new double[3];
			double[] Y = new double[3];
			double G2, Ofs2, mserr;

			Y[0] = dtp.UPOut[(int)CAL_UNITCOUNT.d2_1] - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			Y[1] = dtp.UPOut[(int)CAL_UNITCOUNT.d2_2] - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			Y[2] = dtp.UPOut[(int)CAL_UNITCOUNT.d2_3] - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			X[0] = dtp.UPOut[(int)CAL_UNITCOUNT.d1_1] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			X[1] = dtp.UPOut[(int)CAL_UNITCOUNT.d1_2] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			X[2] = dtp.UPOut[(int)CAL_UNITCOUNT.d1_3] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			CurveFit.LinearFit(X, Y, FitMethod.LeastSquare,out G2, out Ofs2, out mserr);
			dtp.UPOut[(int)CAL_UNITCOUNT.d2_4] = (dtp.UPOut[(int)CAL_UNITCOUNT.d1_4] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0])
				* G2 + Ofs2 + dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
		}


		/* EnergyFitFunc -----------------------------------------------------------

		To be used as a Model Function in NonLinearFit. 
		This puts a strict requirement on the function format.

		Calculates unit response on radiated energy by Planck equation.

		Input:	Target Temperature [TK (Kelvin)],
				Equation Coefficients [	Responsivity (1/Volt),
										Ambient Offset ,
										Spectral multiplier	],
				Emissivity (emiss)
		Return:	Calculated Detector Signal		

		*/
		private static double EnergyFitFuncK(double TK, double[] coef)
		{
			int	i;
			double	engy, wave, C2byTK;

			if(TK < 0.01) TK = 0.01;							// Prevent out-of-range error
			C2byTK = C2 / TK;

			for(i = 0, engy = -coef[1]; i < LineAmpl.Length; i++)	// Calc energy for each line 
			{
				wave = coef[2] * LineWave[i];
				engy += coef[0] * LineAmpl[i] * 1000 / (wave * wave * wave * wave * wave)
					/ (Math.Exp(C2byTK / wave) - 1.0);
			}

			return engy * Emiss * (CurFitAdj1 + (CurFitAdj1 - CurFitAdj4) * (C0 - TK) / 461.667);
		}

		/* FitCalData -----------------------------------------------------------

		Fits Cal Data Points to an equation and calculates Ambient Offset correction.
		Stores results in EqCoef[] and sets global CalAOFF for the given cal cycle.

		Input:	Reads data from CalPntS[], CalPntMC[], and ThermC

		Output:	writes to array EqCoef[] {Offset/MVOFF, Responsivity/MRESP, Wavelength(micron)}
				writes to CalAOFF (Stod)
		
		Return:	0 on success, others on error		

		*/
		private static void FitCalData(ref DATA_POOL dtp)
		{
			int i;
			ModelFunctionCallback pEnergyFitFuncK = new ModelFunctionCallback(EnergyFitFuncK);
			double msqerr;										// Error code
			double[] TcalK = new double[dtp.TcalF.Length];	// Cal points @ Cal Emissivity (K)
			double[] UPd1 = new double[dtp.TcalF.Length];	// Unit reading - NonLinearFit needs this to be double
			double[] UPd2 = new double[dtp.TcalF.Length];	// Unit reading - NonLinearFit needs this to be double
			double[] Coef = new double[3];

			// Prepare cal data:
			if(LineWave.Length != dtp.LineWave.Length) LineWave = new double[dtp.LineWave.Length];
			if(LineAmpl.Length != dtp.LineAmpl.Length) LineAmpl = new double[dtp.LineAmpl.Length];
			for (i = 0; i < LineWave.Length; i++)
			{
				LineWave[i] = dtp.LineWave[i];
				LineAmpl[i] = dtp.LineAmpl[i];
			}
			for (i = 0; i < TcalK.Length; i++)
			{
				TcalK[i] = FtoK(dtp.TcalF[i]);
			}
			CurFitAdj1 = dtp.CurveFitAdj1;
			CurFitAdj4 = dtp.CurveFitAdj4;
			//CalcuUPdOut2(ref dtp);
			UPd1[0] = dtp.UPOut[0]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			UPd1[1] = dtp.UPOut[1]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			UPd1[2] = dtp.UPOut[2]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			//UPd1[3] = dtp.UPOut[(int)CAL_UNITCOUNT.d1_4] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0];
			UPd2[0] = dtp.UPOut[3]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			UPd2[1] = dtp.UPOut[4]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			UPd2[2] = dtp.UPOut[5]; // - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			//UPd2[3] = dtp.UPOut[(int)CAL_UNITCOUNT.d2_4] - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0];
			for (i = 0; i < UPd1.Length; i++)
			{
				UPd1[i] *= (CurFitAdj1 + (CurFitAdj1 - CurFitAdj4) * (C0 - TcalK[i]) / 461.667);
				UPd2[i] *= (CurFitAdj1 + (CurFitAdj1 - CurFitAdj4) * (C0 - TcalK[i]) / 461.667);
			}
			Emiss = dtp.CalEmiss;

			try
			{
				// the 1st IR target channel
				Coef[0] = dtp.InitRespScale1 * 20;					// Load initial guess values
				Coef[1] = 0;
				Coef[2] = dtp.InitSpecScale1;

				CurveFit.NonLinearFit(TcalK, UPd1,pEnergyFitFuncK, Coef, out msqerr);
				
				dtp.CurveFitSigma1 = msqerr;
				dtp.RespScale1 = Coef[0];
				dtp.Ceng1 = Coef[1];
				dtp.SpecScale1 = Coef[2];
				
				// the 2nd IR target channel
				Coef[0] = dtp.InitRespScale2 * 20;					// Load initial guess values
				Coef[1] = 0;
				Coef[2] = dtp.InitSpecScale2;
				
				CurveFit.NonLinearFit(TcalK, UPd2,pEnergyFitFuncK, Coef, out msqerr);
				
				dtp.CurveFitSigma2 = msqerr;
				dtp.RespScale2 = Coef[0];
				dtp.Ceng2 = Coef[1];
				dtp.SpecScale2 = Coef[2];

//				CalTaS = dtp.ThermC - EnergyFunS(dtp.ThermC) / EnergyFunDerivS(dtp.ThermC);
//				ThermOsc = linterp(dtp.Table0BP.Length, dtp.Table0BP, TOscs, dtp.ThermC + dtp.ROFudge);
//				ErrThermOsc = linterp(dtp.Table0BP.Length, dtp.Table0BP, TOscs, CalTaS) - ThermOsc;
//				dtp.ErrAOFF =  (int)Math.Round(CalTaS - dtp.ThermC + dtp.ROFudge);
//				dtp.SpErr = (Coef[2] - 1.0) * 100.0;
//				dtp.Responsivity = (Coef[1] - 1) * 100.0;
//				dtp.FitCurveOFF = Coef[0];
//				if(Math.Abs(dtp.SpErr) > dtp.SpectralMax) return (int)CAL_ERROR.BADSPECTRUM;	// Spectral shift too large
//				if(Math.Abs(dtp.Responsivity) > dtp.ResponsivityMax) return (int)CAL_ERROR.LOWSIGNAL;// Too low detector signal
//				if(Math.Abs(dtp.ErrAOFF) > dtp.MaxErrAOFF) return (int)CAL_ERROR.BADOFFSET;	// AOFF adjusted too much
//				if(dtp.Sigma > dtp.SigmaMax) return (int)CAL_ERROR.BADSIGMA;		// Curve fit error too large (with 3 cal points should never happen)
//				return (int)CAL_ERROR.NOERR;
			}
			catch(Exception ex)
			{
				throw(new ApplicationException("Curve Fit Error!!!\r\n" + ex.Message));
//				OnError(ex.Message);
//				return (int)CAL_ERROR.BADCALDATA;
			}

		}



		private static void CalOffset2(ref DATA_POOL dtp)
		{
			double TS;

			TS = KtoS(dtp.TambK2);
			dtp.Pamb2 = Pd2S(TS, ref dtp);
			dtp.PdOfs2 = (dtp.Pamb2 -dtp.Ceng2)*Emiss;
			dtp.PSdOfs2 =(long)(dtp.PSd2_P * dtp.PdOfs2);
		}

		private static void CalOffset1(ref DATA_POOL dtp)
		{
			double TS;

			TS = KtoS(dtp.TambK1);
			dtp.Pamb1 = Pd1S(TS, ref dtp);
			dtp.PdOfs1 = (dtp.Pamb1 -dtp.Ceng1)*Emiss;
            dtp.PSdOfs1 = (long)(DATA_POOL.PSd1_P * dtp.PdOfs1);
		}

		/* EnergyFunc -----------------------------------------------------------

		A neat version of EnergyFitFunc - to be used as a main Enegry function.

		Calculates unit response on radiated energy by Planck equation.
		Ambient temperature equivalent is subtracted.

		Input:	Target Temperature (stod),
				Reference to DATA_POOL object
		
		Return:	Calculated Detector Signal

		*/
		private static double PSd1S(double TS, ref DATA_POOL dtp)
		{
			int	i;
			double	engy, wave, C2byTK;

			C2byTK = C2 / StoK(TS);

			for(i = 0, engy = 0; i < dtp.LineAmpl.Length; i++)	// Calc energy for each line 
			{
				wave = dtp.SpecScale1 * dtp.LineWave[i];
				engy += dtp.RespScale1 * dtp.LineAmpl[i] * 1000/ (wave * wave * wave * wave * wave)
					/ (Math.Exp(C2byTK / wave) - 1.0);
			}

			return engy * DATA_POOL.PSd1_P;
		}


		private static double Pd1S(double TS, ref DATA_POOL dtp)
		{
			int	i;
			double	engy, wave, C2byTK;

			C2byTK = C2 / StoK(TS);

			for(i = 0, engy = 0; i < dtp.LineAmpl.Length; i++)	// Calc energy for each line 
			{
				wave = dtp.SpecScale1 * dtp.LineWave[i];
				engy += dtp.RespScale1 * dtp.LineAmpl[i] * 1000/ (wave * wave * wave * wave * wave)
					/ (Math.Exp(C2byTK / wave) - 1.0);
			}

			return engy;
		}


		private static double PSd2S(double TS, ref DATA_POOL dtp)
		{
			int	i;
			double	engy, wave, C2byTK;

			C2byTK = C2 / StoK(TS);

			for(i = 0, engy = 0; i < dtp.LineAmpl.Length; i++)	// Calc energy for each line 
			{
				wave = dtp.SpecScale2 * dtp.LineWave[i];
				engy += dtp.RespScale2 * dtp.LineAmpl[i] *1000 / (wave * wave * wave * wave * wave)
					/ (Math.Exp(C2byTK / wave) - 1.0);
			}

			return engy * dtp.PSd2_P;
		}

		private static double Pd2S(double TS, ref DATA_POOL dtp)
		{
			int	i;
			double	engy, wave, C2byTK;

			C2byTK = C2 / StoK(TS);

			for(i = 0, engy = 0; i < dtp.LineAmpl.Length; i++)	// Calc energy for each line 
			{
				wave = dtp.SpecScale2 * dtp.LineWave[i];
				engy += dtp.RespScale2 * dtp.LineAmpl[i] *1000 / (wave * wave * wave * wave * wave)
					/ (Math.Exp(C2byTK / wave) - 1.0);
			}

			return engy ;
		}


		// Energy Function Derivative (input in Stod)
		private static double PSd1DerivS(double TS, ref DATA_POOL dtp)
		{
			return (PSd1S(TS + 1, ref dtp) - PSd1S(TS - 1, ref dtp)) / 2;
		}

		private static double PSd2DerivS(double TS, ref DATA_POOL dtp)
		{
			return (PSd2S(TS + 1, ref dtp) - PSd2S(TS - 1, ref dtp)) / 2;
		}

		// Energy Error Margin (input in Stod)
		private static double PSd1MargS(double TS, ref DATA_POOL dtp)
		{
			double THighS = FtoS(dtp.THighF);
			return dtp.MargScale * Math.Max(DegMargS, Math.Abs((TS > THighS ? THighS : TS)
				- FtoS(dtp.PercMargCenterF)) * PercMarg) * PSd1DerivS(TS, ref dtp);
		}

		private static double PSd2MargS(double TS, ref DATA_POOL dtp)
		{
			double THighS = FtoS(dtp.THighF);
			return dtp.MargScale * Math.Max(DegMargS, Math.Abs((TS > THighS ? THighS : TS)
				- FtoS(dtp.PercMargCenterF)) * PercMarg) * PSd2DerivS(TS, ref dtp);
		}

		/* NextBP -----------------------------------------------------------

		Calculates next PSd/Breakpoint pair up from starting Breakpoint.

		The routine finds a line that crosses the zone between upper and lower error margins from
		EnergyFunc(). The line {y = div * s + lof} passes through lower margin at starting BP and as
		close as possible to being tangent to the upper margin.

		To verify that upper margin is not crossed, the routine moves in jumps of about 15 Stod
		for speed. This ensures more than 10 check points per BP span, - sufficient accuracy to not 
		significantly exceed error margin.

		When counting further to determine the next BP (where the line crosses lower margin again),
		the routine rushes in jumps until it crosses it, then returns back in single Stodarts.


		Input:	Divisor Index (int)
				Starting Breakpoint (Stod):			reads from CalBP[DivIndex]

		Output:	writes to PSd2Tbl[TblIndex + 1]
				writes to BPTbl[TblIndex + 1] (Stod)
		
		Return:	0 on success, -1 on error		

		*/
		private static void	NextBP2(int TblIndex, ref DATA_POOL dtp)
		{
			int	hit;
			//int	div, jump, pivot ;
            long	div, jump, pivot ;//James 
			long s, bp, lof;
			double	diff, newdiff;
 
			try
			{
				hit = 0;												// Reset flags
	 
				jump = (int)Math.Round(dtp.JumpFactor * DegMargS * dtp.MargScale);// Determine jump size
				bp = dtp.BPTbl[TblIndex];								// Read starting breakpoint
				pivot = (int)Math.Round(PSd2S(bp,ref dtp) - PSd2MargS(bp,ref dtp));		// Pivot point of the line at bp
				div = (int)Math.Round(PSd2DerivS(bp,ref dtp));			// Start with the divisor ~= tangent

				if(div <= 1) throw(new ApplicationException("div <= 1"));			// Value out of bounds

               
				for(s = bp + jump; hit == 0; div++)						// Increment divisor until the line hits upper margin
				{
					s = bp + jump;
					for(diff = 2.0 * PSd2MargS(bp,ref dtp); diff >= 0.0; s += jump)
					{
						newdiff = PSd2S(s,ref dtp) + PSd2MargS(s,ref dtp) - (double)(div * (s - bp) + pivot);
						// Calc difference between line and upper margin
						if(newdiff >= diff) break;			// Passed proximity point, line will not hit upper
						diff = newdiff;
	   
						if(diff <= 0.0) hit = 1;			// Line crossed upper, set hit flag before loop ends
					}
				}											// We arrive here with the line having crossed upper

				div -= 2;									// Take previous divisor that did not hit, correct overrun of 1
				lof = pivot - (div * bp);					// Calc our chosen line offset

				for(; div * s + lof > Math.Round(PSd2S(s,ref dtp) - PSd2MargS(s,ref dtp)); s += jump);
				// Rush in jumps until line crosses under lower
				for(; div * s + lof < Math.Round(PSd2S(s,ref dtp) - PSd2MargS(s,ref dtp)); s--);
				// Go back in single stodarts until crossing
				dtp.BPTbl[TblIndex + 1] = s;				// Write results into BPTbl & PSd2Tbl arrays
				dtp.PSd2Tbl[TblIndex + 1] = div * s + lof;

			}
			catch(Exception ex)
			{
				throw(new ApplicationException("Search next BP error!!!" + "\r\nCurrent Index = " + TblIndex.ToString()
					+ "\r\n" + ex.Message));
			}
		}

		/* FullSet2 -----------------------------------------------------------
		Calculates a full PSd2/Breakpoint set, using consecutive calls to NextBP2.
		Result depends on current Coef[] and error margins set by MargScale.
		*/

		private static void FullSet2(ref DATA_POOL dtp)
		{
			int	i;
			double TLowS;

			TLowS = FtoS(dtp.TLowF);

			// Set lowest breakppoint
			dtp.BPTbl[0] = (int)TLowS;
			dtp.PSd2Tbl[0] = (int)Math.Ceiling(PSd2S(TLowS,ref dtp) - PSd2MargS(TLowS,ref dtp));


			for(i = 0; i < dtp.BPTbl.Length - 1; i++)
			{
				NextBP2(i, ref dtp);						// Fill BPTbl[] & PSd2Tbl[]
			}
		}

		/* GenPSdTable -----------------------------------------------------------

		Generates PSd1/PSd2/BP table, best fitting the equation with current Coef[], starting at TLowS.
		Runs NRUNS iterations, trying to minimize MargScale while keeping the table span from TLowS to THighS.
		Choose the best result and sets PSd1Tbl[], PSd2Tbl[] and BPTbl[] to these values.

		Input:	Reads data from Coef[], initial margin values, and other globals

		Output:	writes to CalDiv[] (Osc/Stod) and CalBP[] (Stod)
		
		Return:	0 on success, -1 on error		

		*/
		private static void GenPSdTable(ref DATA_POOL dtp)
		{
			int	n;
			double NomSpan, ActSpan, SpanErr, BestErrScale;
			double TLowS, THighS;

 
			try
			{
				TLowS = FtoS(dtp.TLowF);
				THighS = FtoS(dtp.THighF);
				PercMarg = dtp.InitPercMarg * 0.01;		// Set error margins   
				DegMargS = dtp.InitDegMargF * 5;
	 
				dtp.MargScale = dtp.InitMargScale;
				BestErrScale = dtp.MargScaleMax;

				NomSpan = THighS - TLowS;				// Set nominal temp. span

	 
				for(n=0; n<NRUNS; n++)
				{
					FullSet2(ref dtp);					// Ignore error, if any, on first runs
	 
					ActSpan = (double)(dtp.BPTbl[dtp.BPTbl.Length - 1] - dtp.BPTbl[0]);	// Get actual temp. span
					SpanErr = ActSpan / NomSpan - 1;

					if(SpanErr < 0.0) SpanErr -= dtp.SpanErrFudge;	// Amplify feedback on small negative error
					else if(n > 2 && dtp.MargScale < BestErrScale)	// Seek the smallest MargScale after 3 iterations
					{
						BestErrScale = dtp.MargScale;
					}

					dtp.MargScale *= (1 - dtp.MargScaleDeriv * SpanErr);	// Correct MargScale with negative feedback on ActSpan
	  
					if(dtp.MargScale < dtp.MargScaleMin)			// Prevent MargScale from going ridiculously low or negative
					{
						dtp.MargScale = dtp.MargScaleMin;
					}

				}
	 
				dtp.MargScale = BestErrScale;						// Repeat the table set that gave best error margins
				FullSet2(ref dtp);
	 
				// Signal level / working range problem; or, can't construct table set with given error margins
				if((dtp.MargScale <= dtp.MargScaleMin) || (dtp.MargScale >= dtp.MargScaleMax)) 
					throw(new ApplicationException("MargScale is out of range"));
				
				// generate PSd1Tble
				for(n = 0; n < dtp.BPTbl.Length; n++)
					dtp.PSd1Tbl[n] = (int)Math.Ceiling(PSd1S((double)dtp.BPTbl[n], ref dtp)
						- PSd1MargS((double)dtp.BPTbl[n], ref dtp));

				// Calculate PSdOfs1 and PSdOfs2
				//dtp.PSdOfs1 = (int)(DATA_POOL.PSd1_P * dtp.UPOut[(int)CAL_UNITCOUNT.d1_0]);
				//dtp.PSdOfs2 = (int)(dtp.PSd2_P * dtp.UPOut[(int)CAL_UNITCOUNT.d2_0]);
			}
			catch(Exception ex)
			{
				throw(new ApplicationException("Generate energy table error!!!\r\n" + ex.Message));
			}
		}

/*		// Calculate ambient temperature, using the energy curve fit result - Pamb and energy function - PSd
		private static void CalcuTamb(ref DATA_POOL dtp)
		{
			double TaS2, PSamb2;

			PSamb2 = dtp.Pamb2 * dtp.PSd2_P;
			TaS2 = linterp(dtp.PSd2Tbl, dtp.BPTbl, PSamb2);
			TaS2 += (PSamb2 - PSd2S(TaS2, ref dtp)) / PSd2DerivS(TaS2, ref dtp);
			dtp.TaF = StoF(TaS2);
		}

		// Generate PSa table
		private static void GenPSaTable(ref DATA_POOL dtp)
		{
			int i;
			double PaOfs, UPaOut, PaDefOut;

			CalcuTamb(ref dtp);
			UPaOut = dtp.UPOut[(int)CAL_UNITCOUNT.a];
			PaDefOut = linterp(dtp.TaDefF, dtp.PaDef, dtp.TaF);
			PaOfs = UPaOut - PaDefOut;
			for ( i = 0; i < dtp.BPTbl.Length; i++)
				dtp.PSaTbl[i] = (int)(DATA_POOL.PSa_P
					* ( 1 - PaOfs - linterp(dtp.TaDefF, dtp.PaDef, StoF(dtp.BPTbl[i]))));
		}


		// Generate PStc table
		
		private static void GenPStcTable(ref DATA_POOL dtp)
		{
			int i;
			double UPtcOfs, Gtc, Vcc; //UPtcOut, Vcc, Vtc1;

			//UPtcOfs = dtp.UPOut[(int)CAL_UNITCOUNT.tc0];
			//UPtcOut = dtp.UPOut[(int)CAL_UNITCOUNT.tc1];
			//Vcc = dtp.Volt[(int)CAL_AICHANNEL.vcc];
			//Vtc1 = dtp.Volt[(int)CAL_AICHANNEL.tc];
			//Gtc = Vcc / Vtc1 * (UPtcOut - UPtcOfs);

			Vcc = 5; // to be define
			Gtc = dtp.TCGain;
			UPtcOfs = dtp.TCOfs;

			for ( i = 0; i < dtp.BPtcTbl.Length; i++)
			{
				dtp.BPtcTbl[i] = (int)dtp.TtcJDefS[i];
				dtp.PStcJTbl[i] = (int)(DATA_POOL.PStc_P * (Gtc * dtp.VtcJDef[i] / Vcc + UPtcOfs));
			}

			for ( i = 0; i < dtp.BPtcTbl.Length; i++)
			{
				dtp.BPtcTbl[i] = (int)dtp.TtcKDefS[i];
				dtp.PStcKTbl[i] = (int)(DATA_POOL.PStc_P * (Gtc * dtp.VtcKDef[i] / Vcc + UPtcOfs));
			}

		}


		// Generate PStca table 
		private static void GenPStcaTable(ref DATA_POOL dtp)
		{
			int i;
			double Vtca, Vwip, UPtcaOut, UPtcaOfs, PtcaDefOut;

			Vtca = dtp.Volt[(int)CAL_AICHANNEL.tca];
			Vwip = dtp.Volt[(int)CAL_AICHANNEL.wip];
			dtp.TtcaF = (Vtca - dtp.VtcaOfs) / dtp.VtcaGn;
			dtp.TwipF = (Vwip - dtp.VwipOfs) / dtp.VwipGn;
			dtp.TtcaCompF = dtp.TtcaF + (dtp.TwipF - dtp.TtcaF) * dtp.TCACompFactor;
			UPtcaOut = dtp.UPOut[(int)CAL_UNITCOUNT.tca];
			PtcaDefOut = linterp(dtp.TtcaDefF, dtp.PtcaDef, dtp.TtcaCompF);
			UPtcaOfs = UPtcaOut - PtcaDefOut;
			for ( i = 0; i < dtp.BPtcTbl.Length; i++)
				dtp.PStcaTbl[i] = (int)(DATA_POOL.PStca_P
					* ( 1 - UPtcaOfs - linterp(dtp.TtcaDefF, dtp.PtcaDef, StoF(dtp.BPtcTbl[i]))));
		}
*/


		private static void CalcuTerrF(ref DATA_POOL dtp)
		{
			int i;
			double[] Perr1, Perr2;

			Perr1 = new double[dtp.TcalF.Length];
			Perr2 = new double[dtp.TcalF.Length];
			Perr1[0] = PSd1S(FtoS(dtp.TcalF[0]), ref dtp) - (dtp.Pamb1 + (dtp.UPOut[0] - dtp.PdOfs1) / dtp.CalEmiss) * DATA_POOL.PSd1_P;
			Perr2[0] = PSd2S(FtoS(dtp.TcalF[0]), ref dtp) - (dtp.Pamb2 + (dtp.UPOut[3] - dtp.PdOfs2) / dtp.CalEmiss) * dtp.PSd2_P;
			Perr1[1] = PSd1S(FtoS(dtp.TcalF[1]), ref dtp) - (dtp.Pamb1 + (dtp.UPOut[1] - dtp.PdOfs1) / dtp.CalEmiss) * DATA_POOL.PSd1_P;
			Perr2[1] = PSd2S(FtoS(dtp.TcalF[1]), ref dtp) - (dtp.Pamb2 + (dtp.UPOut[4] - dtp.PdOfs2) / dtp.CalEmiss) * dtp.PSd2_P;
			Perr1[2] = PSd1S(FtoS(dtp.TcalF[2]), ref dtp) - (dtp.Pamb1 + (dtp.UPOut[2] - dtp.PdOfs1) / dtp.CalEmiss) * DATA_POOL.PSd1_P;
			Perr2[2] = PSd2S(FtoS(dtp.TcalF[2]), ref dtp) - (dtp.Pamb2 + (dtp.UPOut[5] - dtp.PdOfs2) / dtp.CalEmiss) * dtp.PSd2_P;
			//Perr1[3] = PSd1S(FtoS(dtp.TcalF[3]), ref dtp) - (dtp.Pamb1 + (dtp.UPOut[(int)CAL_UNITCOUNT.d1_4] - dtp.UPOut[(int)CAL_UNITCOUNT.d1_0]) / dtp.CalEmiss) * DATA_POOL.PSd1_P;
			//Perr2[3] = PSd2S(FtoS(dtp.TcalF[3]), ref dtp) - (dtp.Pamb2 + (dtp.UPOut[(int)CAL_UNITCOUNT.d2_4] - dtp.UPOut[(int)CAL_UNITCOUNT.d2_0]) / dtp.CalEmiss) * dtp.PSd2_P;
			for (i = 0; i < dtp.TcalF.Length; i++)
			{
				dtp.Terr1F[i] = (Perr1[i] / PSd1DerivS(FtoS(dtp.TcalF[i]), ref dtp)) / 5;
				dtp.Terr2F[i] = (Perr2[i] / PSd2DerivS(FtoS(dtp.TcalF[i]), ref dtp)) / 5;
			}
		}
        //James
        /// <summary>
        /// type: 1---MV; 2---TC;

        public static void AnalogCalmathLib(ref DATA_POOL dtp)
        {
            double gain, offset;
            
           //James 20160729 FOR model mA
        
                gain = (dtp.IoutHighCounts - dtp.IoutLowCounts) / (dtp.IoutHigh - dtp.IoutLow);
                offset = dtp.IoutLowCounts - gain * dtp.IoutLow;
                dtp.IGain = (int)(gain * 65536);
                dtp.IOfs = (int)(offset* 1024);
    


      
           //James 20160729 for 12 wires model 0-10V
          
                gain = (dtp.VoutHighCounts - dtp.VoutLowCounts) / (dtp.VoutHigh - dtp.VoutLow);
                offset = dtp.VoutLowCounts - gain*dtp.VoutLow;
               dtp.VGain = (int)(gain * 65536);
               dtp.VOfs = (int)(offset * 1024);//add 65536 to scale 20161011
          
            //James 20161021 for Tc
            
                gain = (dtp.TCHighCounts - dtp.TCLowCounts) / (dtp.TCHigh - dtp.TCLow);
                offset = dtp.TCLowCounts - gain *  dtp.TCLow;
                dtp.TCGain =(long) (gain * 65536);
                dtp.TCOffset = (long)(offset * 1024);
          
           //James 20161021 for FTC1

                gain = (dtp.FTC1HighCounts - dtp.FTC1LowCounts) / (dtp.FTC1High - dtp.FTC1Low);
                offset = dtp.FTC1LowCounts - gain * dtp.FTC1Low;
                dtp.FTC1Gain = (long)(gain * 65536);
                dtp.FTC1Ofs = (long)(offset * 1024);
        
           //James 20161021 for FTC2

                gain = (dtp.FTC2HighCounts - dtp.FTC2LowCounts) / (dtp.FTC2High - dtp.FTC2Low);
                offset = dtp.FTC2LowCounts - gain * dtp.FTC2Low;
                dtp.FTC2Gain = (long)(gain * 65536);
                dtp.FTC2Ofs = (long)(offset * 1024);
        
        }
        
        //James 
		/* DoCALMATHLIB -----------------------------------------------------------
		Combining FitCalData(), GenTable() and GenTDiv() functions and  
		exporting this function to call it from Visual Basic

		Input:	FitCalData(), GenTable() and GenTDiv() functions;

		Output:	Calibration results

		Return:	0 on success, error code otherwise
		*/
		public static void IrCalmathLib(ref DATA_POOL dtp)
		{
            int i;
            int Scale;
         //   double[] UPOut = new double[15];
         //   long[] LowBPTbl = new long[4];
         //   long[] LowBPP3Tbl = new long[4];
         //   long[] LowPSd2Tbl = new long[4];
         //   long[] LowPSd2P3Tbl = new long[4];
            dtp.AmbTempS =((long) CALMATHLIB.CtoS(dtp.AmbTemp))*4;
            dtp.UPOut[0] = dtp.TP1Value / 131072;
            dtp.UPOut[3] = dtp.TP1Value / 131072;
            dtp.UPOut[1] = dtp.TP2Value / 131072;
            dtp.UPOut[4] = dtp.TP2Value / 131072;
            dtp.UPOut[2] = dtp.TP3Value / 131072;
            dtp.UPOut[5] = dtp.TP3Value / 131072;


            dtp.PSd2_P = Math.Pow(2, dtp.PSd2_Ppower);
            Scale = dtp.PSd2_Ppower -17;//James added  20161021

            FitCalData(ref dtp);

            CalOffset1(ref dtp);
            CalOffset2(ref dtp);

            //CalcuTerrF(ref dtp);

            GenPSdTable(ref dtp);
            /*

                        GenPStcTable(ref dtp);
			
            */

            for (i = 0; i < dtp.BPTbl.Length; i++) dtp.BPTbl[i] *= (int)4;
            //			for( i = 0; i < dtp.BPtcTbl.Length; i++) dtp.BPtcTbl[i] *= (int)10;
            for (i = 0; i < dtp.PSd1Tbl.Length; i++) dtp.PSd1Tbl[i] = dtp.PSd1Tbl[i] >> Scale;
            for (i = 0; i < dtp.PSd2Tbl.Length; i++) dtp.PSd2Tbl[i] = dtp.PSd2Tbl[i] >> Scale;

           
                //
            dtp.LowBPTbl[0] = (long)(CALMATHLIB.CtoS(-25)) * 4;
            dtp.LowBPTbl[1] = (long)(CALMATHLIB.CtoS(0)) * 4;
            dtp.LowBPTbl[2] = (long)(CALMATHLIB.CtoS(50)) * 4;
            dtp.LowBPTbl[3] = (long)(CALMATHLIB.CtoS(100)) * 4;
                //
            dtp.LowPSd2Tbl[0] = ((long)PSd2S(CtoS(-25), ref dtp)) >> Scale;//-25C 
            dtp.LowPSd2Tbl[1] = ((long)PSd2S(CtoS(0), ref dtp)) >> Scale; // 0C
            dtp.LowPSd2Tbl[2] = ((long)PSd2S(CtoS(50), ref dtp)) >> Scale;// 50C
            dtp.LowPSd2Tbl[3] = ((long)PSd2S(CtoS(100), ref dtp)) >> Scale;//100C

            //Jame added for P3
            dtp.LowBPP3Tbl[0] = (long)(CALMATHLIB.CtoS(-25)) * 4;
            dtp.LowBPP3Tbl[1] = (long)(CALMATHLIB.CtoS(0)) * 4;
            dtp.LowBPP3Tbl[2] = (long)(CALMATHLIB.CtoS(5)) * 4;
            dtp.LowBPP3Tbl[3] = (long)(CALMATHLIB.CtoS(15)) * 4;
            //
            dtp.LowPSd2P3Tbl[0] = ((long)PSd2S(CtoS(-25), ref dtp)) >> Scale;//-25C 
            dtp.LowPSd2P3Tbl[1] = ((long)PSd2S(CtoS(0), ref dtp)) >> Scale; // 0C
            dtp.LowPSd2P3Tbl[2] = ((long)PSd2S(CtoS(5), ref dtp)) >> Scale;// 5C
            dtp.LowPSd2P3Tbl[3] = ((long)PSd2S(CtoS(15), ref dtp)) >> Scale;//15C

            dtp.PSdOfs1 = dtp.PSdOfs1 >> Scale;
            dtp.PSdOfs2 = dtp.PSdOfs2 >> Scale;

            dtp.TempSHigh = (int)FtoS(dtp.DHighF) * (int)4;		// set temperature range for unit display on LCD 
            dtp.TempSLow = (int)FtoS(dtp.DLowF) * (int)4;		// this item different energy table range in P7 module

            i = (int)((double)((long)1 << 32) * 0.99 / (dtp.BPTbl[dtp.BPTbl.Length - 1] - dtp.BPTbl[dtp.BPTbl.Length - 2]));
            dtp.PSd1Max = dtp.PSd1Tbl[dtp.PSd1Tbl.Length - 2] + i;
            dtp.PSd2Max = dtp.PSd2Tbl[dtp.PSd2Tbl.Length - 2] + i;

            //			i = (int)((double)((long)1 << 32) * 0.99 / (dtp.BPtcTbl[dtp.BPtcTbl.Length - 1] - dtp.BPtcTbl[dtp.BPtcTbl.Length - 2]));
            //			dtp.PStcMax = dtp.PStcTbl[dtp.PStcTbl.Length - 2] + i;

		}

		/* LinearErrS -----------------------------------------------------------

		Calculate the linear error of the generated tables at a certain unit count.

		Input:	Temperature counts (F)
		
		Output: Error (F),
				Margin at this point (F).
				
		*/
		public static void LinearErrF(ref DATA_POOL dtp, double TF, out double ErrF, out double MargF)
		{
			double energy, t_s;
			double TS = FtoS(TF);
			double THighS = FtoS(dtp.THighF);
			MargF = dtp.MargScale * Math.Max(DegMargS, Math.Abs((TS > THighS ? THighS : TS)
				- FtoS(dtp.PercMargCenterF)) * PercMarg) / 5;
			energy = PSd2S(TS,ref dtp);
			t_s = linterp(dtp.PSd2Tbl, dtp.BPTbl,energy);
			ErrF = (t_s - TS) / 5;
		}
	}

}
