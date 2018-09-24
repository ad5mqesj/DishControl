using DishControl;
using System;
using System.Runtime.Serialization;

namespace InterfaceFake
{
    public enum Eth32PwmClock
    {
        Disabled = 0,
        Enabled = 1
    }

    public enum Eth32PwmChannel
    {
        Disabled = 0,
        Normal = 1,
        Inverted = 2
    }
    public enum EthError
    {
        NotConnected = -1001,
        AlreadyConnected = -1000,
        Timeout = -201,
        InvalidIndex = -117,
        InvalidNetmask = -116,
        InvalidIp = -115,
        InvalidValue = -114,
        InvalidOther = -113,
        InvalidPointer = -112,
        InvalidChannel = -111,
        InvalidBit = -109,
        InvalidPort = -104,
        InvalidHandle = -101,
        BufSize = -27,
        Plugin = -26,
        LoadLib = -25,
        ConfigReject = -24,
        ConfigNoAck = -23,
        ReuseOpt = -22,
        BcastOpt = -21,
        WrongMode = -20,
        NetworkIntr = -19,
        Winsock = -18,
        Windows = -17,
        Malloc = -16,
        Ethread = -15,
        Rthread = -14,
        Pipe = -13,
        NotSupported = -12,
        Thread = -11,
        Network = -10,
        Closing = -2,
        General = -1,
        None = 0
    }

public class Eth32Exception : Exception
    {
        public Eth32Exception(EthError errorcode)
        {

        }
        public Eth32Exception(string message, EthError errorcode) { }
        public Eth32Exception(SerializationInfo info, StreamingContext context) { }
        public Eth32Exception(string message, EthError errorcode, Exception inner) { }

        public EthError ErrorCode { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    public class DigPort
    {
        public DigPort()
        {
            data = 0;
            portDir = 0;
        }
        public byte data { get; set; }
        public byte portDir { get; set; }
    }

    public class PwmChannelState
    {
        public PwmChannelState()
        {
            state = Eth32PwmChannel.Disabled;
            freq = 0.0;
            duty = 0.0;
        }

        public int channel { get; set; }
        public Eth32PwmChannel state { get; set; }
        public double freq { get; set; }
        public double duty { get; set; }
    }

    public class Eth32
    {
        T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }

            return array;
        }
        public const int DefaultPort = 7152;
        public const int DirInput = 0;
        public const int DirOutput = 255;

        public Eth32()
        {
            this.Ports = InitializeArray<DigPort>(5);
            this.PwmChans = InitializeArray<PwmChannelState>(2);
            this.error = "";
            this.Connected = false;
        }

        private DigPort[] Ports;

        PwmChannelState[] PwmChans;

        public string error { get; set; }
        public int SerialUnit { get; }
        public bool Connected { get; set; }
        public int Timeout { get; set; }
        public Eth32PwmClock PwmClockState { get; set; }
        public int ProductID { get; }
        public int PwmBasePeriod { get; set; }

        public event EventHandler HardwareEvent;

        public static string ErrorString(int errorcode)
        {
            return "Not Implememnted";
        }
        public void Connect(string address, int port, int timeout) { this.Connected = true; }
        public void Connect(string address, int port) { this.Connected = true; }
        public void Connect(string address) { this.Connected = true; }

        //        public Eth32ConnectionFlag ConnectionFlags(int reset) { }
        //        public void DisableEvent(Eth32EventType eventtype, int port, int bit) { }
        //        public void EnableEvent(Eth32EventType eventtype, int port, int bit, int id) { }
        //        public void GetAnalogEventDef(int bank, int channel, out int lomark, out int himark);
        //        public int InputAnalog(int channel);
        //        public ushort InputAnalogUShort(int channel);
        //        public byte InputByteByte(int port)
        //        public int InputSuccessive(int port, int maxcount, out int status);
        //        public byte InputSuccessiveByte(int port, int maxcount, out int status);
        //        public void SetAnalogEventDef(int bank, int channel, int lomark, int himark, Eth32AnalogEvtDef defaultval);
        //        public void PulseBit(int port, int bit, Eth32PulseEdge edge, int count);
        //        public byte[] GetEeprom(int address, int length)
        //        public void SetEeprom(int address, int length, byte[] buffer);
        //        protected virtual void Cleanup();
        //        public void VerifyConnection();

        public void ResetDevice()
        {
            this.Ports = InitializeArray<DigPort>(5);
            this.PwmChans = InitializeArray<PwmChannelState>(2);
            this.error = "";
        }

        public void Disconnect()
        {
            this.Connected = false;
        }
        public void Dispose() { }
        public void EmptyEventQueue() { }
        public int GetDirection(int port)
        {
            if (port < 0 || port > 4)
            {
                error = "port out of range";
                return 0;
            }
            return (int)Ports[port].portDir;
        }
        public int GetDirectionBit(int port, int bit)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return 0;
            }
            byte dir = Ports[port].portDir;
            return (dir & (1 >> bit)) << bit;
        }
        public bool GetDirectionBitBool(int port, int bit)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return false;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return false;
            }
            byte dir = Ports[port].portDir;
            return (dir & (1 >> bit)) > 0;
        }
        public byte GetDirectionByte(int port)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            return Ports[port].portDir;
        }

        public void GetPwmParameters(int channel, out Eth32PwmChannel state, out double freq, out double duty)
        {
            state = Eth32PwmChannel.Disabled;
            freq = 0.0;
            duty = 0.0;
            if (channel < 0 || channel > 1)
            {
                error = "Channel out of range";
            }
            state = PwmChans[channel].state;
            freq = PwmChans[channel].freq;
            duty = PwmChans[channel].duty;
        }

        public int InputBit(int port, int bit)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return 0;
            }
            byte data = Ports[port].data;
            return (data & (1 >> bit)) << bit;

        }
        public bool InputBitBool(int port, int bit)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return false;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return false;
            }
            byte data = Ports[port].data;
            return (data & (1 >> bit)) > 0;
        }
        public int InputByte(int port)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            return (int)Ports[port].data;
        }
        public void OutputBit(int port, int bit, int val)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return;
            }
            BasicLog.writeLog(String.Format ("OutputBit(port: {0}, bit: {1}, val: {2})", port, bit, val));
            byte data = (byte)((val & 0x01) >> bit);
            if (data > 0)
            {
                this.Ports[port].data |= data;
            }
            else
            {
                this.Ports[port].data &= (byte)(~data);
            }
        }
        public void OutputByte(int port, int val)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return;
            }
            BasicLog.writeLog(String.Format("OutputByte(port: {0}, val: {1})", port, val));
            this.Ports[port].data = (byte)val;
        }
        public int Readback(int port)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            return (int)this.Ports[port].data;
        }
        public byte ReadbackByte(int port)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return 0;
            }
            return this.Ports[port].data;
        }

        public void SetDirection(int port, int direction)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return;
            }
            BasicLog.writeLog(String.Format("SetDirection(port: {0}, val: {1})", port, direction));
            this.Ports[port].portDir = (byte)direction;
        }
        public void SetDirectionBit(int port, int bit, bool direction)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return;
            }
            BasicLog.writeLog(String.Format("SetDirectionBit(port: {0}, bit: {1}, direction: {2})", port, bit, direction.ToString()));
            byte dir = (byte)((direction?0x01:0) >> bit);
            if (dir > 0)
            {
                this.Ports[port].portDir |= dir;
            }
            else
            {
                this.Ports[port].portDir &= (byte)(~dir);
            }

        }
        public void SetDirectionBit(int port, int bit, int direction)
        {
            if (port < 0 || port > 5)
            {
                error = "port out of range";
                return;
            }
            if (bit < 0 || bit > 7)
            {
                error = "bit out of range";
                return;
            }
            BasicLog.writeLog(String.Format("SetDirectionBit(port: {0}, bit: {1}, direction: {2})", port, bit, direction));
            byte dir = (byte)((direction&0x01) >> bit);
            if (dir > 0)
            {
                this.Ports[port].portDir |= dir;
            }
            else
            {
                this.Ports[port].portDir &= (byte)(~dir);
            }
        }

        public void SetPwmParameters(int channel, Eth32PwmChannel state, double freq, double duty)
        {
            if (channel < 0 || channel > 1)
            {
                error = "Channel out of range";
            }
            PwmChans[channel].state = state;
            PwmChans[channel].freq = freq;
            PwmChans[channel].duty = duty;
        }
    }
}
