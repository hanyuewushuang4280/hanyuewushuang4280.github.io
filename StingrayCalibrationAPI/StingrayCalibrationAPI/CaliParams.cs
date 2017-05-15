using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Raylib.IO;
using CaliSysLib.IO;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace StingrayCalibrationAPI
{
  public struct MinMaxValue
  {
    private double min;
    [Description("Min Value")]
    public double Min
    {
      get { return min; }
      set { min = value; }
    }
    private double max;
    [Description("Max Value")]
    public double Max
    {
      get { return max; }
      set { max = value; }
    }
    public double[] GetArray()
    {
      double[] d = new double[2];
      d[0] = min;
      d[1] = max;
      return d;
    }
  }
  public class MinMaxValueConver : ExpandableObjectConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(MinMaxValue))
        return true;
      return base.CanConvertTo(context, destinationType);
    }
    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string) && value is MinMaxValue)
      {
        MinMaxValue tr = (MinMaxValue)value;
        return tr.Min.ToString() + "," + tr.Max.ToString();
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (value is string)
      {
        try
        {
          string s = (string)value;
          string[] strs = s.Split(new char[] { ',' });
          MinMaxValue tr = new MinMaxValue();
          tr.Min = double.Parse(strs[0]);
          tr.Max = double.Parse(strs[1]);
          return tr;
        }
        catch
        {
          throw new Exception("Type can't be converted.");
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }

  public struct MinMaxGoalValue
  {
    private double min;
    [Description("Min Value")]
    public double Min
    {
      get { return min; }
      set { min = value; }
    }
    private double max;
    [Description("Max Value")]
    public double Max
    {
      get { return max; }
      set { max = value; }
    }
    private double goal;
    [Description("Goal Value")]
    public double Goal
    {
      get { return goal; }
      set { goal = value; }
    }
    public double[] GetArray()
    {
      double[] d = new double[3];
      d[0] = min;
      d[1] = max;
      d[2] = goal;
      return d;
    }
  }
  public class MinMaxGoalValueConver : ExpandableObjectConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(MinMaxGoalValue))
        return true;
      return base.CanConvertTo(context, destinationType);
    }
    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string) && value is MinMaxGoalValue)
      {
        MinMaxGoalValue tr = (MinMaxGoalValue)value;
        return tr.Min.ToString() + "," + tr.Max.ToString() + "," + tr.Goal.ToString();
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (value is string)
      {
        try
        {
          string s = (string)value;
          string[] strs = s.Split(new char[] { ',' });
          MinMaxGoalValue tr = new MinMaxGoalValue();
          tr.Min = double.Parse(strs[0]);
          tr.Max = double.Parse(strs[1]);
          tr.Goal = double.Parse(strs[2]);
          return tr;
        }
        catch
        {
          throw new Exception("Type can't be converted.");
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }

  public struct NoiseValue
  {
      private double ta;
      [Description("Ta")]
      public double Ta
      {
          get { return ta; }
          set { ta = value; }
      }

      private double ttca;
      [Description("Ttca")]
      public double Ttca
      {
          get { return ttca; }
          set { ttca = value; }
      }

      private double nd2_0;
      [Description("Nd2_0")]
      public double Nd2_0
      {
          get { return nd2_0; }
          set { nd2_0 = value; }
      }

      private double nd2_1;
      [Description("Nd2_1")]
      public double Nd2_1
      {
          get { return nd2_1; }
          set { nd2_1 = value; }
      }

      private double nd2_2;
      [Description("Nd2_2")]
      public double Nd2_2
      {
          get { return nd2_2; }
          set { nd2_2 = value; }
      }

      private double nd2_3;
      [Description("Nd2_3")]
      public double Nd2_3
      {
          get { return nd2_3; }
          set { nd2_3 = value; }
      }

      private double nd2_4;
      [Description("Nd2_4")]
      public double Nd2_4
      {
          get { return nd2_4; }
          set { nd2_4 = value; }
      }

        private double nIoutHigh;
        [Description("NIoutHigh")]
        public double NIoutHigh
        {
            get { return nIoutHigh; }
            set { nIoutHigh = value; }
        }
    private double nFTC1Low;
    [Description("NFTC1Low")]
    public double NFTC1Low
    {
      get { return nFTC1Low; }
      set { nFTC1Low = value; }
    }
    private double nFTC1High;
    [Description("NFTC1High")]
    public double NFTC1High
    {
      get { return nFTC1High; }
      set { nFTC1High = value; }
    }
    private double nFTC2Low;
    [Description("NFTC2Low")]
    public double NFTC2Low
    {
      get { return nFTC2Low; }
      set { nFTC2Low = value; }
    }
    private double nFTC2High;
    [Description("NFTC2High")]
    public double NFTC2High
    {
      get { return nFTC2High; }
      set { nFTC2High = value; }
    }
    private double ntcLow;
    [Description("NtcLow")]
    public double NtcLow
    {
      get { return ntcLow; }
      set { ntcLow = value; }
    }
    
    private double nIoutLow;
    [Description("NIoutLow")]
    public double NIoutLow
    {
      get { return nIoutLow; }
      set { nIoutLow = value; }
    }
    
    private double nVoutLow;
    [Description("NVoutLow")]
    public double NVoutLow
    {
      get { return nVoutLow; }
      set { nVoutLow = value; }
    }
    private double nVoutHigh;
      [Description("NVoutHigh")]
      public double NVoutHigh
    {
      get { return nVoutHigh; }
      set { nVoutHigh = value; }
    }
    private double ntcHigh;
    [Description("NtcHigh")]
    public double NtcHigh
    {
      get { return ntcHigh; }
      set { ntcHigh = value; }
    }
    
    public double[] GetArray()
    {
      double[] d = new double[17]; 
        //
      d[0] = ta;
      d[1] = ttca;
      d[2] = nd2_0;
      d[3] = nd2_1;
      d[4] = nd2_2;
      d[5] = nd2_3;
      d[6] = nd2_4;
      d[7] = nFTC1Low;
      d[8] = nFTC1High;
      d[9] = nFTC2Low;
      d[10] = nFTC2High;
      d[11] = nIoutLow;
      d[12] = nIoutHigh;
      d[13] = nVoutLow;
      d[14] = nVoutHigh;
      d[15] = ntcLow;
      d[16] = ntcHigh;
      return d;
    }
  }
  public class NoiseValueConver : ExpandableObjectConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(NoiseValue))
        return true;
      return base.CanConvertTo(context, destinationType);
    }
    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string) && value is NoiseValue)
      {
        NoiseValue tr = (NoiseValue)value;
        return tr.NIoutHigh.ToString() + "," + tr.NFTC1Low.ToString() + "," + tr.NFTC1High.ToString() + "," + tr.NFTC2Low.ToString() + "," + tr.NFTC2High.ToString() + "," +
          tr.NtcLow.ToString() + "," + tr.Nd2_1.ToString() + "," + tr.Nd2_2.ToString() + "," + tr.Nd2_3.ToString() + "," + tr.NIoutLow.ToString() + "," +
          tr.Ta.ToString() + "," + tr.NVoutLow.ToString() + "," + tr.NVoutHigh.ToString() + "," + tr.NtcHigh.ToString() + "," + tr.Ttca.ToString();
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (value is string)
      {
        try
        {
          string s = (string)value;
          string[] strs = s.Split(new char[] { ',' });
          NoiseValue tr = new NoiseValue();
          tr.NIoutHigh = double.Parse(strs[0]);
          tr.NFTC1Low = double.Parse(strs[1]);
          tr.NFTC1High = double.Parse(strs[2]);
          tr.NFTC2Low = double.Parse(strs[3]);
          tr.NFTC2High = double.Parse(strs[4]);
          tr.NtcLow = double.Parse(strs[5]);
          tr.Nd2_1 = double.Parse(strs[6]);
          tr.Nd2_2 = double.Parse(strs[7]);
          tr.Nd2_3 = double.Parse(strs[8]);
          tr.NIoutLow = double.Parse(strs[9]);
          tr.Ta = double.Parse(strs[10]);
          tr.NVoutLow = double.Parse(strs[11]);
          tr.NVoutHigh= double.Parse(strs[12]);
          tr.NtcHigh = double.Parse(strs[13]);
          tr.Ttca = double.Parse(strs[14]);
          return tr;
        }
        catch
        {
          throw new Exception("Type can't be converted.");
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }

  public class CaliParams
  {
    private string configFileName;
    public CaliParams()
    {
    }
    public CaliParams(string configFile)
    {
      try
      {
        configFileName = configFile;
        loadParams(configFile);
      }
      catch
      {
        //MessageBox.Show("Load calibration parameters config file fail!");
      }

    }
    public void SaveParams()
    {
      ExtIniFile eif = new ExtIniFile(configFileName);

      eif.WriteInt("CaliParams", "PSd2_Ppower ", pSd2_Ppower );
      eif.WriteInt("CaliParams", "SampleCount ", sampleCount);
    
      eif.WriteInt("CaliParams", "ADC1FLT_AF", af);
      eif.WriteInt("CaliParams", "ADC1FLT_SF", sf);
      eif.WriteInt("CaliParams", "ADC1FLT_Notch", notch);
      eif.WriteInt("CaliParams", "ADC1FLT_Ravg", ravg);
      eif.WriteInt("CaliParams", "ADC1FLT_Sinc4", sinc4);
      eif.WriteInt("CaliParams", "ADC1FLT_Chop", chop);

      eif.WriteInt("CaliParams", "Average_Co", averageCo);

      eif.WriteInt("CaliParams", "TargetADThresJump", targetADThresJump);
      eif.WriteInt("CaliParams", "TargetADThresStab", targetADThresStab);
      eif.WriteInt("CaliParams", "TargetADDepthInit", targetADDepthInit);
      eif.WriteInt("CaliParams", "TargetADdepthStep", targetADdepthStep);

      eif.WriteInt("CaliParams", "AmbG", AmbG);
      eif.WriteInt("CaliParams", "AmbOfs", AmbOfs);

      eif.WriteInt("CaliParams", "Alpha", alpha);
      eif.WriteInt("CaliParams", "Alpha_Low", alpha_Low);
      eif.WriteInt("CaliParams", "Alpha_PsdOfs2", alpha_PsdOfs2);

      eif.WriteInt("CaliParams", "TCAlphaJ", tcAlphaJ);
      eif.WriteInt("CaliParams", "TCOffsetJ", tcOffsetJ); 
      eif.WriteInt("CaliParams", "TCAlphaK", tcAlphaK);
      eif.WriteInt("CaliParams", "TCOffsetK", tcOffsetK);
      
      //eif.WriteInt("CaliParams", "Iout_Gain", iout_Gain);
      //eif.WriteInt("CaliParams", "Iout_Offset", iout_Offset);

      eif.WriteString("CaliParams", "TgtRangeF", targetRangeF.Min.ToString() + "," + targetRangeF.Max.ToString());
      eif.WriteString("CaliParams", "DispRangeF", displayRangeF.Min.ToString() + "," + displayRangeF.Max.ToString());
      eif.WriteString("CaliParams", "AmbRangeF", ambRangeF.Min.ToString() + "," + ambRangeF.Max.ToString());

      eif.WriteInt("CaliParams", "TP1Setting", tp1Setting);
      eif.WriteInt("CaliParams", "TP2Setting", tp2Setting);
      eif.WriteInt("CaliParams", "TP3Setting", tp3Setting);
      eif.WriteInt("CaliParams", "TP4Setting", tp4Setting);

      eif.WriteInt("CaliParams", "CaliWaitTime", caliWaitTime);
      
      eif.WriteString("CaliParams", "LinWave", lineWave.Min.ToString() + "," + lineWave.Max.ToString());
      eif.WriteString("CaliParams", "LinAmpl", lineAmplifier.Min.ToString() + "," + lineAmplifier.Max.ToString());

      eif.WriteDouble("CaliParams", "CalEmiss", caliEmiss);
      eif.WriteDouble("CaliParams", "MargF", marginF);
      eif.WriteDouble("CaliParams", "MargP", marginP);
      eif.WriteDouble("CaliParams", "MargCenterF", marginCenterF);
      eif.WriteDouble("CaliParams", "MargSclDeriv", marginScaleDerivative);
      eif.WriteInt("CaliParams", "JumpFactor", jumpFactor);
      eif.WriteInt("CaliParams", "SpanErrFudge", spanErrFudge);
      eif.WriteInt("CaliParams", "CurveFitAdj1", curveFitAdj1);
      eif.WriteInt("CaliParams", "CurveFitAdj4", curveFitAdj4);
      eif.WriteInt("CaliParams", "TableNumber", tableNumber);
      eif.WriteInt("CaliParams", "CaliOffset", calioffset);

      eif.WriteDouble("CaliParams", "TCACompFactor", tcACompensationFactor);
      eif.WriteString("CaliParams", "TaDriftF", taDriftF.Min.ToString() + "," + taDriftF.Max.ToString() + "," + taDriftF.Goal.ToString());
      eif.WriteString("CaliParams", "TwipDriftF", twipDriftF.Min.ToString() + "," + twipDriftF.Max.ToString() + "," + twipDriftF.Goal.ToString());
      eif.WriteString("CaliParams", "SpecScale1", specScale1.Min.ToString() + "," + specScale1.Max.ToString() + "," + specScale1.Goal.ToString());
      eif.WriteString("CaliParams", "SpecScale2", specScale2.Min.ToString() + "," + specScale2.Max.ToString() + "," + specScale2.Goal.ToString());
      eif.WriteString("CaliParams", "RespScale1", respScale1.Min.ToString() + "," + respScale1.Max.ToString() + "," + respScale1.Goal.ToString());
      eif.WriteString("CaliParams", "RespScale2", respScale2.Min.ToString() + "," + respScale2.Max.ToString() + "," + respScale2.Goal.ToString());
      eif.WriteString("CaliParams", "CurveFitSig", curveFitSig.Min.ToString() + "," + curveFitSig.Max.ToString() + "," + curveFitSig.Goal.ToString());
      eif.WriteString("CaliParams", "MargScale", margScale.Min.ToString() + "," + margScale.Max.ToString() + "," + margScale.Goal.ToString());
    
    }
      /*
    public void SaveAnalogParams(int ioutGain,int ioutOffset)//James 10160801
    {
        ExtIniFile eif = new ExtIniFile(configFileName);
        eif.WriteInt("CaliParams", "Iout_Gain", ioutGain);
        eif.WriteInt("CaliParams", "Iout_Offset", ioutOffset);
    }
       * */
    private void loadParams(string configFile)
    {
      try
      {
        double[] d;
        ExtIniFile eif = new ExtIniFile(configFile);

        pSd2_Ppower = eif.ReadInt("CaliParams", "PSd2_Ppower", 0);
        sampleCount = eif.ReadInt("CaliParams", "SampleCount", 0);

        sf = eif.ReadInt("CaliParams", "ADC1FLT_SF", 0);
        af = eif.ReadInt("CaliParams", "ADC1FLT_AF", 0);
        ravg = eif.ReadInt("CaliParams", "ADC1FLT_Ravg", 0);
        notch = eif.ReadInt("CaliParams", "ADC1FLT_Notch", 0);       
        sinc4 = eif.ReadInt("CaliParams", "ADC1FLT_Sinc4", 0);
        chop = eif.ReadInt("CaliParams", "ADC1FLT_Chop", 0);
        averageCo = eif.ReadInt("CaliParams", "Average_Co", 0);

        targetADThresJump = eif.ReadInt("CaliParams", "TargetADThresJump", 0);
        targetADThresStab = eif.ReadInt("CaliParams", "TargetADThresStab", 0);
        targetADDepthInit = eif.ReadInt("CaliParams", "TargetADDepthInit", 0);
        targetADdepthStep = eif.ReadInt("CaliParams", "TargetADdepthStep", 0);

        AmbG = eif.ReadInt("CaliParams", "AmbG", 0);
        AmbOfs = eif.ReadInt("CaliParams", "AmbOfs", 0);

        alpha = eif.ReadInt("CaliParams", "Alpha", 0);
        alpha_Low = eif.ReadInt("CaliParams", "Alpha_Low ", 0);
        alpha_PsdOfs2 = eif.ReadInt("CaliParams", "Alpha_PsdOfs2", 0);

        tcAlphaJ = eif.ReadInt("CaliParams", "TCAlphaJ", 0);
        tcOffsetJ = eif.ReadInt("CaliParams", "TCOffsetJ", 0);
        tcAlphaK = eif.ReadInt("CaliParams", "TCAlphaK", 0);
        tcOffsetK = eif.ReadInt("CaliParams", "TCOffsetK", 0);

        d = eif.ReadDoubleArray("CaliParams", "TgtRangeF", ",");
        targetRangeF.Min = d[0];
        targetRangeF.Max = d[1];

        d = eif.ReadDoubleArray("CaliParams", "DispRangeF", ",");
        displayRangeF.Min = d[0];
        displayRangeF.Max = d[1];

        d = eif.ReadDoubleArray("CaliParams", "AmbRangeF", ",");
        ambRangeF.Min = d[0];
        ambRangeF.Max = d[1];

        tp1Setting = eif.ReadInt("CaliParams", "TP1Setting", 0);
        tp2Setting = eif.ReadInt("CaliParams", "TP2Setting", 0);
        tp3Setting = eif.ReadInt("CaliParams", "TP3Setting", 0);
        tp4Setting = eif.ReadInt("CaliParams", "TP4Setting", 0);

        caliWaitTime = eif.ReadInt("CaliParams", "CaliWaitTime", 0);

        //iout_Gain = eif.ReadInt("CaliParams", "Iout_Gain", 0);
        //iout_Offset = eif.ReadInt("CaliParams", "Iout_Offset", 0);

        d = eif.ReadDoubleArray("CaliParams", "LinWave", ",");
        lineWave.Min = d[0];
        lineWave.Max = d[1];
        d = eif.ReadDoubleArray("CaliParams", "LinAmpl", ",");
        lineAmplifier.Min = d[0];
        lineAmplifier.Max = d[1];

        caliEmiss = eif.ReadDouble("CaliParams", "CalEmiss", 1.0);
        marginF = eif.ReadDouble("CaliParams", "MargF", 0);
        marginP = eif.ReadDouble("CaliParams", "MargP", 0);
        marginCenterF = eif.ReadDouble("CaliParams", "MargCenterF", 0);
        marginScaleDerivative = eif.ReadDouble("CaliParams", "MargSclDeriv", 0);
        jumpFactor = eif.ReadInt("CaliParams", "JumpFactor", 0);
        spanErrFudge = eif.ReadInt("CaliParams", "SpanErrFudge", 0);
        curveFitAdj1 = eif.ReadInt("CaliParams", "CurveFitAdj1", 0);
        curveFitAdj4 = eif.ReadInt("CaliParams", "CurveFitAdj4", 0);
        tableNumber = eif.ReadInt("CaliParams", "TableNumber", 0);
        calioffset = eif.ReadInt("CaliParams", "CaliOffset", 0);

        tcACompensationFactor = eif.ReadDouble("CaliParams", "TCACompFactor", 0);

        d = eif.ReadDoubleArray("CaliParams", "TaDriftF", ",");
        taDriftF.Min = d[0];
        taDriftF.Max = d[1];
        taDriftF.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "TwipDriftF", ",");
        twipDriftF.Min = d[0];
        twipDriftF.Max = d[1];
        twipDriftF.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "SpecScale1", ",");
        specScale1.Min = d[0];
        specScale1.Max = d[1];
        specScale1.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "SpecScale2", ",");
        specScale2.Min = d[0];
        specScale2.Max = d[1];
        specScale2.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "RespScale1", ",");
        respScale1.Min = d[0];
        respScale1.Max = d[1];
        respScale1.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "RespScale2", ",");
        respScale2.Min = d[0];
        respScale2.Max = d[1];
        respScale2.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "CurveFitSig", ",");
        curveFitSig.Min = d[0];
        curveFitSig.Max = d[1];
        curveFitSig.Goal = d[2];
        d = eif.ReadDoubleArray("CaliParams", "MargScale", ",");
        margScale.Min = d[0];
        margScale.Max = d[1];
        margScale.Goal = d[2];
          //
        d = eif.ReadDoubleArray("CaliParams", "Nmin", ",");
        nmin.Ta = d[0];
        nmin.Ttca = d[1];
        nmin.Nd2_0 = d[2];
        nmin.Nd2_1 = d[3];
        nmin.Nd2_2 = d[4];
        nmin.Nd2_3 = d[5];
        nmin.Nd2_4 = d[6];
        nmin.NFTC1Low = d[7];
        nmin.NFTC1High = d[8];
        nmin.NFTC2Low = d[9];
        nmin.NFTC2High = d[10];
        nmin.NIoutLow = d[11];
        nmin.NIoutHigh = d[12];
        nmin.NVoutLow = d[13];
        nmin.NVoutHigh = d[14];
        nmin.NtcLow = d[15];
        nmin.NtcHigh = d[16];
          //
        d = eif.ReadDoubleArray("CaliParams", "Nmax", ",");
        nmax.Ta = d[0];
        nmax.Ttca = d[1];
        nmax.Nd2_0 = d[2];
        nmax.Nd2_1 = d[3];
        nmax.Nd2_2 = d[4];
        nmax.Nd2_3 = d[5];
        nmax.Nd2_4 = d[6];
        nmax.NFTC1Low = d[7];
        nmax.NFTC1High = d[8];
        nmax.NFTC2Low = d[9];
        nmax.NFTC2High = d[10];
        nmax.NIoutLow = d[11];
        nmax.NIoutHigh = d[12];
        nmax.NVoutLow = d[13];
        nmax.NVoutHigh = d[14];
        nmax.NtcLow = d[15];
        nmax.NtcHigh = d[16];
       
      }
      catch
      {
        //MessageBox.Show("Load calibration parameters config file fail!");
      }
    }
    private void saveParams(string configFile)
    {

    }

    private int pSd2_Ppower ;
    [Category("CaliParams"), Description("PSd2_Ppower ")]
    public int PSd2_Ppower 
    {
        get { return pSd2_Ppower ; }
        set { pSd2_Ppower  = value; }
    }

    private int sampleCount;
    [Category("CaliParams"), Description("SampleCount ")]
    public int SampleCount
    {
        get { return sampleCount; }
        set { sampleCount = value; }
    }

    private int sf;
    [Category("CaliParams"), Description("ADC1FLT_SF  ")]
    public int ADC1FLT_SF
    {
        get { return sf; }
        set { sf = value; }
    }

    private int af;
    [Category("CaliParams"), Description("ADC1FLT_AF  ")]
    public int ADC1FLT_AF
    {
        get { return af; }
        set { af = value; }
    }

    private int ravg;
    [Category("CaliParams"), Description("ADC1FLT_Ravg  ")]
    public int ADC1FLT_Ravg
    {
        get { return ravg; }
        set { ravg = value; }
    }

    private int notch;
    [Category("CaliParams"), Description("ADC1FLT_Notch  ")]
    public int ADC1FLT_Notch
    {
        get { return notch; }
        set { notch = value; }
    }

    private int sinc4;
    [Category("CaliParams"), Description("ADC1FLT_Sinc4  ")]
    public int ADC1FLT_Sinc4
    {
        get { return sinc4; }
        set { sinc4 = value; }
    }

    private int chop;
    [Category("CaliParams"), Description("ADC1FLT_Chop  ")]
    public int ADC1FLT_Chop
    {
        get { return chop; }
        set { chop = value; }
    }

    private int averageCo;
    [Category("CaliParams"), Description("Average_Co  ")]
    public int Average_Co
    {
        get { return averageCo; }
        set { averageCo = value; }
    }


    private int targetADThresJump;
    [Category("CaliParams"), Description("TargetADThresJump  ")]
    public int TargetADThresJump
    {
        get { return targetADThresJump; }
        set { targetADThresJump = value; }
    }

    private int targetADThresStab;
    [Category("CaliParams"), Description("TargetADThresStab  ")]
    public int TargetADThresStab
    {
        get { return targetADThresStab; }
        set { targetADThresStab = value; }
    }

    private int targetADDepthInit;
    [Category("CaliParams"), Description("TargetADDepthInit    ")]
    public int TargetADDepthInit  
    {
        get { return targetADDepthInit; }
        set { targetADDepthInit = value; }
    }

    private int targetADdepthStep;
    [Category("CaliParams"), Description("TargetADdepthStep   ")]
    public int TargetADdepthStep 
    {
        get { return targetADdepthStep; }
        set { targetADdepthStep = value; }
    }

    private int alpha;
    [Category("CaliParams"), Description("Alpha")]
    public int Alpha
    {
        get { return alpha; }
        set { alpha = value; }
    }

    private int alpha_Low;
    [Category("CaliParams"), Description("Alpha_Low   ")]
    public int Alpha_Low
    {
        get { return alpha_Low; }
        set { alpha_Low = value; }
    }

    private int alpha_PsdOfs2;
    [Category("CaliParams"), Description("Alpha_PsdOfs2   ")]
    public int Alpha_PsdOfs2
    {
        get { return alpha_PsdOfs2; }
        set { alpha_PsdOfs2 = value; }
    }

    private int tcAlphaK;
    [Category("CaliParams"), Description("TCAlphaK")]
    public int TCAlphaK
    {
        get { return tcAlphaK; }
        set { tcAlphaK = value; }
    }

    private int tcAlphaJ;
    [Category("CaliParams"), Description("TCAlphaJ")]
    public int TCAlphaJ
    {
        get { return tcAlphaJ; }
        set { tcAlphaJ = value; }
    }

    private int tcOffsetK;
    [Category("CaliParams"), Description("TCOffsetK")]
    public int TCOffsetK
    {
        get { return tcOffsetK; }
        set { tcOffsetK = value; }
    }

    private int tcOffsetJ;
    [Category("CaliParams"), Description("TCOffsetJ")]
    public int TCOffsetJ
    {
        get { return tcOffsetJ; }
        set { tcOffsetJ = value; }
    }

    private int ambG;
    [Category("CaliParams"), Description("AmbG   ")]
    public int AmbG 
    {
        get { return ambG; }
        set { ambG = value; }
    }

    private int ambOfs;
    [Category("CaliParams"), Description("AmbOfs  ")]
    public int AmbOfs
    {
        get { return ambOfs; }
        set { ambOfs = value; }
    }

    private MinMaxValue targetRangeF;
    [Category("CaliParams"), Description("Target Temperature Range (F)"), TypeConverter(typeof(MinMaxValueConver))]
    public MinMaxValue TgtRangeF
    {
        get { return targetRangeF; }
        set { targetRangeF = value; }
    }

    private MinMaxValue displayRangeF;
    [Category("CaliParams"), Description("Display Temperature Range (F)"), TypeConverter(typeof(MinMaxValueConver))]
    public MinMaxValue DispRangeF
    {
        get { return displayRangeF; }
        set { displayRangeF = value; }
    }

    private MinMaxValue ambRangeF;
    [Category("CaliParams"), Description("Ambient Temperature Range (F)"), TypeConverter(typeof(MinMaxValueConver))]
    public MinMaxValue AmbRangeF
    {
        get { return ambRangeF; }
        set { ambRangeF = value; }
    }

    private int tp1Setting;
    [Category("CaliParams"), Description("TP1Setting  ")]
    public int TP1Setting
    {
        get { return tp1Setting; }
        set { tp1Setting = value; }
    }

    private int tp2Setting;
    [Category("CaliParams"), Description("TP2Setting  ")]
    public int TP2Setting
    {
        get { return tp2Setting; }
        set { tp2Setting = value; }
    }

    private int tp3Setting;
    [Category("CaliParams"), Description("TP3Setting  ")]
    public int TP3Setting
    {
        get { return tp3Setting; }
        set { tp3Setting = value; }
    }

    private int tp4Setting;
    [Category("CaliParams"), Description("TP4Setting  ")]
    public int TP4Setting
    {
        get { return tp4Setting; }
        set { tp4Setting = value; }
    }

    private int caliWaitTime;
    [Category("CaliParams"), Description("Calibration Waiting Time between point to point")]
    public int CaliWaitTime
    {
        get { return caliWaitTime; }
        set { caliWaitTime = value; }
    }
/*
    private int iout_Gain;
    [Category("CaliParams"), Description("Iout_Gain    ")]
    public int Iout_Gain 
    {
        get { return iout_Gain; }
        set { iout_Gain = value; }
    }

    private int iout_Offset;
    [Category("CaliParams"), Description("Iout_Offset   ")]
    public int Iout_Offset 
    {
        get { return iout_Offset; }
        set { iout_Offset = value; }
    }
      */
    private MinMaxValue lineWave;
    [Category("CaliParams"), Description("Line Wave"), TypeConverter(typeof(MinMaxValueConver))]
    public MinMaxValue LineWave
    {
      get { return lineWave; }
      set { lineWave = value; }
    }

    private MinMaxValue lineAmplifier;
    [Category("CaliParams"), Description("Line Amplifier"), TypeConverter(typeof(MinMaxValueConver))]
    public MinMaxValue LineAmpl
    {
      get { return lineAmplifier; }
      set { lineAmplifier = value; }
    }

    private double caliEmiss;
    [Category("CaliParams"), Description("Calibration Emissivity")]
    public double CalEmiss
    {
        get { return caliEmiss; }
        set { caliEmiss = value; }
    }

    private double marginF;
    [Category("CaliParams"), Description("Linearity Error Margins (F)")]
    public double MarginF
    {
        get { return marginF; }
        set { marginF = value; }
    }

    private double marginP;
    [Category("CaliParams"), Description("Linearity Error Margins (Percent)")]
    public double MarginP
    {
        get { return marginP; }
        set { marginP = value; }
    }

    private double marginCenterF;
    [Category("CaliParams"), Description("F	Center of Percent Error Margin")]
    public double MargCenterF
    {
        get { return marginCenterF; }
        set { marginCenterF = value; }
    }

    private double marginScaleDerivative;
    [Category("CaliParams"), Description("Error scale derivative")]
    public double MargSclDeriv
    {
        get { return marginScaleDerivative; }
        set { marginScaleDerivative = value; }
    }

    private int jumpFactor;
    [Category("CaliParams"), Description("Determines energy lookup jump size")]
    public int JumpFactor
    {
        get { return jumpFactor; }
        set { jumpFactor = value; }
    }

    private int spanErrFudge;
    [Category("CaliParams"), Description("Negative error feedback amplifier")]
    public int SpanErrFudge
    {
        get { return spanErrFudge; }
        set { spanErrFudge = value; }
    }

    private int curveFitAdj1;
    [Category("CaliParams"), Description("Curve Fit Adjust Parameter 1")]
    public int CurveFitAdj1
    {
        get { return curveFitAdj1; }
        set { curveFitAdj1 = value; }
    }

    private int curveFitAdj4;
    [Category("CaliParams"), Description("Curve Fit Adjust Parameter 4")]
    public int CurveFitAdj4
    {
        get { return curveFitAdj4; }
        set { curveFitAdj4 = value; }
    }

    private int tableNumber;
    [Category("CaliParams"), Description("TableNumber")]
    public int TableNumber
    {
        get { return tableNumber; }
        set { tableNumber = value; }
    }

    private int calioffset;
    [Category("CaliParams"), Description("CaliOffset")]
    public int CaliOffset
    {
        get { return calioffset; }
        set { calioffset = value; }
    }

    private double tcACompensationFactor;
    [Category("CaliParams"), Description("TC terminal temperature compensation factor ( TtcaComp = Ttca + (Twip - Ttca) * TCACompFactor)")]
    public double TCACompFactor
    {
        get { return tcACompensationFactor; }
        set { tcACompensationFactor = value; }
    }

    private MinMaxGoalValue taDriftF;
    [Category("CaliParams"), Description("Thermistor temperature drift limits (F)"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue TaDriftF
    {
      get { return taDriftF; }
      set { taDriftF = value; }
    }

    private MinMaxGoalValue twipDriftF;
    [Category("CaliParams"), Description("WIP container temperature drift limits (F)"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue TwipDriftF
    {
      get { return twipDriftF; }
      set { twipDriftF = value; }
    }

    private MinMaxGoalValue specScale1;
    [Category("CaliParams"), Description("Spectral multiplier"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue SpecScale1
    {
      get { return specScale1; }
      set { specScale1 = value; }
    }

    private MinMaxGoalValue specScale2;
    [Category("CaliParams"), Description("Spectral multiplier"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue SpecScale2
    {
      get { return specScale2; }
      set { specScale2 = value; }
    }

    private MinMaxGoalValue respScale1;
    [Category("CaliParams"), Description("Responsivity multiplier"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue RespScale1
    {
      get { return respScale1; }
      set { respScale1 = value; }
    }

    private MinMaxGoalValue respScale2;
    [Category("CaliParams"), Description("Responsivity multiplier"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue RespScale2
    {
      get { return respScale2; }
      set { respScale2 = value; }
    }

    private MinMaxGoalValue curveFitSig;
    [Category("CaliParams"), Description("Curve fit standard deviation (sigma)"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue CurveFitSig
    {
      get { return curveFitSig; }
      set { curveFitSig = value; }
    }

    private MinMaxGoalValue margScale;
    [Category("CaliParams"), Description("Linearity Error boundaries multiplier"), TypeConverter(typeof(MinMaxGoalValueConver))]
    public MinMaxGoalValue MargScale
    {
      get { return margScale; }
      set { margScale = value; }
    }

    private NoiseValue nmin;
    [Category("CaliParams"), Description("Nmin"), TypeConverter(typeof(NoiseValueConver))]
    public NoiseValue Nmin
    {
        get { return nmin; }
        set { nmin = value; }
    }

    private NoiseValue nmax;
    [Category("CaliParams"), Description("Nmax"), TypeConverter(typeof(NoiseValueConver))]
    public NoiseValue Nmax
    {
        get { return nmax; }
        set { nmax = value; }
    }
  }

  [Serializable]
  public class AnchovyParams
  {
    // device
    public int IRAmbCount;
    public int TCAmbCount;
    public int VoutGain;
    public int VoutOffset;
    public int PSdOfs2;
    public int Alpha;
    public int CaliTemp;
    public int OutputMode;
    public int[] BPTable;
    public int[] PSd2Table;

    // Calibration
    public float AdjGain;
    public float AdjOffset;
    public int AmbTempValue;
    public float TP1Temp;
    public int TP1Count;
    public float TP2Temp;
    public int TP2Count;
    public float TP3Temp;
    public int TP3Count;
    public int mVLowCount;
    public float mVLowOut;
    public int mVHighCount;
    public float mVHighOut;
    public int TCLowCount;
    public float TCLowOut;
    public int TCHighCount;
    public float TCHighOut;
    //James added for stingray
      public int PSd2_Ppower ;
      public int AdcFilterCoef;
      public int AdcThreshold;
      public int TargetADThresJump;
      public int TargetADThresStab;
      public int TargetADDepthInit;
      public int TargetADdepthStep;
      public int AmbG;
      public int AmbOfs;
      public int Iout_Gain;
      public int Iout_Offset;
      public int Alpha_Low;

    // System Info
    public DateTime BackupDate;
    public string FirmwareVersion;
    public string ModelName;
    public int ModelNum;
    public string Remark;
    public float Emissivity;
    public string SerialNum;
    public string TempUnit;
    public float UpperLimit;
    public float LowLimit;
    public float IRAmbTemp;
    public float TCAmbTemp;


    public AnchovyParams()
    {
      BPTable = new int[32];
      PSd2Table = new int[32];
    }

    public void Load(string file)
    {
      MemoryStream ms = Streams.FileToMemoryStream(file);
      BinaryFormatter bf = new BinaryFormatter();
      object obj = bf.Deserialize(ms);
      if (obj != null)
      {
        AnchovyParams temp = (AnchovyParams)obj;
          //James added for stingray
        this.PSd2_Ppower  = temp.PSd2_Ppower ;
        this.AdcFilterCoef = temp.AdcFilterCoef;
        this.AdcThreshold = temp.AdcThreshold;
        this.TargetADThresJump = temp.TargetADThresJump;
        this.TargetADThresStab = temp.TargetADThresStab;
        this.TargetADDepthInit = temp.TargetADDepthInit;
        this.TargetADdepthStep = temp.TargetADdepthStep;
        this.Alpha_Low = temp.Alpha_Low;
        this.AmbG = temp.AmbG;
        this.AmbOfs = temp.AmbOfs;
        this.Iout_Gain = temp.Iout_Gain;
        this.Iout_Offset = temp.Iout_Offset;

          //end

        this.IRAmbCount = temp.IRAmbCount;
        this.TCAmbCount = temp.TCAmbCount;
        this.VoutGain = temp.VoutGain;
        this.VoutOffset = temp.VoutOffset;
        this.PSdOfs2 = temp.PSdOfs2;
        this.Alpha = temp.Alpha;
        this.CaliTemp = temp.CaliTemp;
        this.OutputMode = temp.OutputMode;
        this.BPTable = temp.BPTable;
        this.PSd2Table = temp.PSd2Table;


        this.AdjGain = temp.AdjGain;
        this.AdjOffset = temp.AdjOffset;
        this.AmbTempValue = temp.AmbTempValue;
        this.TP1Temp = temp.TP1Temp;
        this.TP1Count = temp.TP1Count;
        this.TP2Temp = temp.TP2Temp;
        this.TP2Count = temp.TP2Count;
        this.TP3Temp = temp.TP3Temp;
        this.TP3Count = temp.TP3Count;
        this.mVLowCount = temp.mVLowCount;
        this.mVLowOut = temp.mVLowOut;
        this.mVHighCount = temp.mVHighCount;
        this.mVHighOut = temp.mVHighOut;
        this.TCLowCount = temp.TCLowCount;
        this.TCLowOut = temp.TCLowOut;
        this.TCHighCount = temp.TCHighCount;
        this.TCHighOut = temp.TCHighOut;

        this.BackupDate = temp.BackupDate;
        this.FirmwareVersion = temp.FirmwareVersion;
        this.ModelName = temp.ModelName;
        this.ModelNum = temp.ModelNum;
        this.Remark = temp.Remark;
        this.Emissivity = temp.Emissivity;
        this.SerialNum = temp.SerialNum;
        this.TempUnit = temp.TempUnit;
        this.UpperLimit = temp.UpperLimit;
        this.LowLimit = temp.LowLimit;
        this.IRAmbTemp = temp.IRAmbTemp;
        this.TCAmbTemp = temp.TCAmbTemp;
      }
    }
    public void Save(string file)
    {
      BackupDate = System.DateTime.Now;
      MemoryStream ms = new MemoryStream();
      BinaryFormatter bf = new BinaryFormatter();
      object clone = this.MemberwiseClone();
      bf.Serialize(ms, clone);
      Streams.MemoryStreamToFile(ms, file);
    }
  }
}
