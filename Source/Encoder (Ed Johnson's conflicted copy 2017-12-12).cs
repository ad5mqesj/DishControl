using System;
using System.Text.RegularExpressions;
using WinfordEthIO;

namespace DishControl
{
    public class Encoder
    {
        private Eth32 device = null;
        private int outputPortNum = -1;
        private string _outPort = "";
        private int _rotations = 0;
        private uint _lastCount = 0;

        public encoderCode coding { get; set; }
        public int RevsPerRot { get; set; }
        public int EncoderBits { get; set; }
        public int StartBit { get; set; }
        public double OffsetDeg { get; set; }

        public string outputPort
        {
            get
            {
                return _outPort;
            }

            set
            {
                _outPort = value;
                string resultString = Regex.Match(_outPort, @"\d+").Value;
                Int32.TryParse(resultString, out outputPortNum);
            }
        }

        public int maxCounts
        {
            get
            {
                return ((1 << this.EncoderBits) - 1) * this.RevsPerRot;
            }
            private set { }
        }

        public Encoder()
        {
            coding = encoderCode.Binary;
            RevsPerRot = 1;
            EncoderBits = 8;
            StartBit = 0;
            OffsetDeg = 0.0;
            outputPort = "P2";
        }

        public Encoder(Eth32 dev, configModel config, bool isAz = true)
        {
            this.coding = config.coding;
            this.outputPort = config.outputPort;
            this.device = dev;

            if (isAz)
            {
                this.RevsPerRot = config.AzimuthRevsPerRot;
                this.EncoderBits = config.AzimuthEncoderBits;
                this.StartBit = config.AzimuthStartBit;
                this.OffsetDeg = config.AzimuthOffsetDeg;
            }
            else
            {
                this.RevsPerRot = config.ElevationRevsPerRot;
                this.EncoderBits = config.ElevationEncoderBits;
                this.StartBit = config.ElevationStartBit;
                this.OffsetDeg = config.ElevationOffsetDeg;
            }
            string resultString = Regex.Match(this.outputPort, @"\d+").Value;
            Int32.TryParse(resultString, out outputPortNum);
        }

        public static uint greyToBinary(uint num)
        {
            uint mask;
            for (mask = num >> 1; mask != 0; mask = mask >> 1)
            {
                num = num ^ mask;
            }
            return num;
        }

        public void SetupEncoderPorts()
        {
            if (!this.device.Connected)
                return;
            //assumes we only use first 4 ports for at most 24 bits in
            for (int i = 0; i < 4; i++)
            {
                if (i == outputPortNum)
                    continue;
                this.device.SetDirection(i, 0); //set all bits in
                this.device.OutputByte(i, 255);
            }
        }

        public uint readNormalizedEncoderBits()
        {
            if (!this.device.Connected)
                return 0;

            uint retv = 0;
            //read all bytes from input ports
            int[] portData = new int[4];

            int ixb = 0;
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i == outputPortNum)
                        continue;
                    portData[ixb++] = this.device.InputByte(i);
                }
            }
            catch (Exception ex)
            {

            }
            uint data = (uint)(portData[0] & 0xff) | (uint)(portData[1] & 0xff) << 8 | (uint)(portData[2] & 0xff) << 16;
            uint mask = ((uint)0x00ffffff >> (24 - this.EncoderBits)) << this.StartBit;
            retv = (data & mask) >> this.StartBit;

            if (this.coding == encoderCode.Grey)
            {
                retv = greyToBinary(retv);
            }
            int maxRawCounts = (1 << this.EncoderBits) - 1;
            //check for the need to increment becasue of more than 1 rev per rot
            if (this.RevsPerRot > 1)
            {
                //if we roll over 0 we are still winding up, this assumes we are 
                //reading at a rate sufficient that we dont accumulate more than 16 counts between reads
                if (this._lastCount > retv && retv < 16)
                {
                    this._rotations++;
                    if (this._rotations > this.RevsPerRot)
                        this._rotations = 0;
                }
                else if (this._lastCount < retv && retv > maxRawCounts - 16)
                {
                    this._rotations--;
                    if (this._rotations < 0)
                        this._rotations = 0;
                }
            }

            this._lastCount = retv;
            uint ret = retv + (uint)(this._rotations * maxRawCounts);
/*            if (ret >= (uint)this.OffsetCounts)
                ret = ret - (uint)this.OffsetCounts;
            else
            {
                ret = ret + (uint)maxRawCounts - (uint)this.OffsetCounts;
            }
*/            return ret;
        }

        public double countsToDegrees(uint encoderCounts)
        {
            double retv = 0.0;
            retv = (360.0 / (double)this.maxCounts) * (double)encoderCounts;
            retv -= this.OffsetDeg;
            if (retv > 360.0)
                retv -= 360.0;
            else if (retv < 0.0)
                retv += 360.0;
            return retv;

        }
    }
}
