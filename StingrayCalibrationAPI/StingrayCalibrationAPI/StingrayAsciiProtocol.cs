using Raytek.AsciiTalk.components.protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StingrayCalibrationAPI
{
    public class StingrayAsciiProtocol : AsciiProtocol
    {
        StringBuilder sb = new StringBuilder();
        private const string CALIBRATION_COMMAND = "%";

        public StingrayAsciiProtocol() : base()
        {
            
        }

        /// <summary>
        /// Setup the AsciiProtocol object with provided parameters.
        /// </summary>
        /// <param name="MultidropID">the multidrop id to be used in the string construction</param>
        /// <param name="HeadID">the head id to be used in the string construction</param>
        public StingrayAsciiProtocol(int MultidropID, int HeadID) : base(MultidropID, HeadID)
        {
            
        }

        //TODO reroute all calls of getter and check for %
        public override string GetCommand(string ascii_command)
        {
            if(ascii_command.Contains(CALIBRATION_COMMAND))
            {
                return ascii_command;
            }
            else
                return pollPrefix + ascii_command;
        }

        public override string GetCommand(int headID, string ascii_command)
        {
            if (ascii_command.Contains(CALIBRATION_COMMAND))
            {
                return headID + ascii_command;
            }
            return pollPrefix + headID + ascii_command;
        }

        public override string PollString(string ascii_command)
        {
            sb.Clear();
            AddMultidropId();
            if (!ascii_command.Contains(CALIBRATION_COMMAND))
            {
                sb.Append(pollPrefix);
            }
            AddHeadID();
            sb.Append(ascii_command);
            return sb.ToString();
        }

        public override string SetString(string ascii_command, string value)
        {
            return this.SetCommand(ascii_command, value);
           
        }

        public override string SetCommand(string ascii_command, string value)
        {
 
            if (ascii_command.Contains("XU"))
            {
                return base.SetCommand("%DI", "A" + value);
            }
            else if (ascii_command.Contains("%UID"))
            {
                return base.SetCommand("%DI", "A" + value);
            }
            else
            {
                return base.SetCommand(ascii_command,value);
            }
        }

        public override bool ExtractResult(ref string value, string cmdString)
        {
            if (cmdString.Contains(" "))
            {
                //do your own parse here. 
                //return true;
            }
            return base.ExtractResult(ref value, cmdString);
        } 

    }
}
