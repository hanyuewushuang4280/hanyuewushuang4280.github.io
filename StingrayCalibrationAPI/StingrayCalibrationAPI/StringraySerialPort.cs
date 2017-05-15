using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib.Calibrations;
using Raytek.AsciiTalk.components;
using Raylib.Calibrations.interfaces;

namespace StingrayCalibrationAPI
{
    public class StingraySerialPort : GenericDeviceInterface,ICalibrationDevice
    {
        public StingraySerialPort(Raytek.AsciiTalk.model.SerialPortConfiguration configuration) : base(configuration,false)
        {
        
        }

        public double GetDoubleFromDevice(string commandInfoText, string cmd)
        {
            return this.GetDoubleFromDevice(cmd);
        }

        public double GetDoubleFromDevice(string commandInfoText, Raytek.AsciiTalk.model.CommandName cmd)
        {
            return this.GetDoubleFromDevice(cmd);
        }

        public string GetStringFromDevice(string commandInfoText, string cmd)
        {
            return this.GetStringFromDevice(cmd);
        }
    }
}
