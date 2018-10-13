using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DishControl
{
    public enum encoderCode
    {
        Grey,
        Binary
    }

    public enum driveType
    {
        CCW, 
        DirEnable,
        Both
    }
    public static class configFileHandler
    {
        public static configModel readConfig(string filename)
        {
            XmlSerializer ser = new XmlSerializer(typeof(configModel));

            configModel model = new configModel();
            using (XmlReader reader = XmlReader.Create(filename))
            {
                model = (configModel)ser.Deserialize(reader);
            }
            return model;
        }
        public static void writeConfig (string filename, configModel model)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(configModel));
            Stream fs = new FileStream(filename, FileMode.Create);
            XmlWriter writer = new XmlTextWriter(fs, Encoding.Unicode);
            serializer.Serialize(writer, model);
            writer.Close();
        }
    }

    public class presets
    {
        public presets()
        {
            Value = 0;
            Text = "";
            Az = 0.0;
            El = 0.0;
        }

        public int Value { get; set; }
        public string Text { get; set; }
        public double Az { get; set; }
        public double El { get; set; }
    }

    public class configModel
    {
        public configModel()
        {
            latitude = 0.0;
            longitude = 0.0;
            altitude = 0.0;
            coding = encoderCode.Binary;
            alpha = 0.75;
            AzimuthEncoderBits = 12;
            AzimuthRevsPerRot = 1;
            AzimuthStartBit = 0;
            AzimuthOffsetDeg = 0.0;
            ElevationEncoderBits = 12;
            ElevationRevsPerRot = 1;
            ElevationStartBit = 12;
            ElevationOffsetDeg = 0.0;
            eth32Address = "";
            outputPort = "P2";
            jogIncrement = 0.5;

            driveEnablebit = -1;
            azDriveType = driveType.Both;
            azActiveHi = true;
            azCWbit = 1;
            azCCWbit = 7;
            azEnable = 0;
            azPWMchan = 0;
            azKp = 1.0;
            azKi = 0.0;
            azKd = 0.0;
            azG = 1.0;
            azMax = 370.0;
            azMin = -10.0;
            azOutMax = 16384.0;
            azOutMin = 0.0;
            azPark = 0.0;
            azSouthPark = 180.0;
            elDriveType = driveType.Both;
            elActiveHi = true;
            elCWbit = 2;
            elCCWbit = 3;
            elEnable = -1;
            elPWMchan = 1;
            elKp = 1.0;
            elKi = 0.0;
            elKd = 0.0;
            elG = 1.0;
            elMax = 90.0;
            elMin = 0.0;
            elOutMax = 16384.0;
            elOutMin = 0.0;
            elPark = 89.99;
            elSouthPark = 89.99;

            positionFileLog = "c:\\PositionLogs\\Position.log";
            maxPosLogFiles = 5;
            maxPosLogSizeBytes = 1024 * 1024 * 10;  //10 MB

            Preset1Name = Preset2Name = Preset3Name = Preset4Name = Preset5Name = "";
            Preset1Az = Preset2Az = Preset3Az = Preset4Az = Preset5Az = 0.0;
            Preset5El = Preset4El = Preset3El = Preset2El = Preset1El = 0.0;
        }

        public List<presets> getPresetList()
        {
            List<presets> Presets = new List<presets>();
            presets none = new presets()
            {
                Value = 0,
                Text = "None",
                Az = 0.0,
                El = 0.0
            };
            Presets.Add(none);
            presets ps1 = new presets()
            {
                Value = 1,
                Text = this.Preset1Name,
                Az = this.Preset1Az,
                El = this.Preset1El
            };
            Presets.Add(ps1);
            presets ps2 = new presets()
            {
                Value = 2,
                Text = this.Preset2Name,
                Az = this.Preset2Az,
                El = this.Preset2El
            };
            Presets.Add(ps2);
            presets ps3 = new presets()
            {
                Value = 3,
                Text = this.Preset3Name,
                Az = this.Preset3Az,
                El = this.Preset3El
            };
            Presets.Add(ps3);
            presets ps4 = new presets()
            {
                Value = 4,
                Text = this.Preset4Name,
                Az = this.Preset4Az,
                El = this.Preset4El
            };
            Presets.Add(ps4);
            presets ps5 = new presets()
            {
                Value = 5,
                Text = this.Preset5Name,
                Az = this.Preset5Az,
                El = this.Preset5El
            };
            Presets.Add(ps5);
            return Presets;
        }

        public double latitude { get; set; }
        public double longitude { get; set; }
        public double altitude { get; set; }
        public encoderCode coding { get; set; }
        public double alpha { get; set; }
        public int AzimuthRevsPerRot { get; set; }
        public int AzimuthEncoderBits { get; set; }
        public int AzimuthStartBit { get; set; }
        public double AzimuthOffsetDeg { get; set; }
        public int ElevationRevsPerRot { get; set; }
        public int ElevationEncoderBits { get; set; }
        public int ElevationStartBit { get; set; }
        public double ElevationOffsetDeg { get; set; }
        public string eth32Address { get; set; }

        public string outputPort { get; set; }
        public double jogIncrement { get; set; }

        public int driveEnablebit { get; set; }
        public driveType azDriveType { get; set; }
        public bool azActiveHi { get; set; }
        public int azCWbit { get; set; }
        public int azCCWbit { get; set; }
        public int azEnable { get; set; }
        public int azPWMchan { get; set; }
        public double azKp { get; set; }
        public double azKi { get; set; }
        public double azKd { get; set; }
        public double azG { get; set; }
        public double azMax { get; set; }
        public double azMin { get; set; }
        public double azOutMax { get; set; }
        public double azOutMin { get; set; }

        public double azPark { get; set; }
        public double azSouthPark { get; set; }

        public driveType elDriveType { get; set; }
        public bool elActiveHi { get; set; }
        public int elCWbit { get; set; }
        public int elCCWbit { get; set; }
        public int elEnable { get; set; }
        public int elPWMchan { get; set; }

        public double elKp { get; set; }
        public double elKi { get; set; }
        public double elKd { get; set; }
        public double elG { get; set; }
        public double elMax { get; set; }
        public double elMin { get; set; }
        public double elOutMax { get; set; }
        public double elOutMin { get; set; }
        public double elPark { get; set; }
        public double elSouthPark { get; set; }

        public string positionFileLog { get; set; }
        public int maxPosLogSizeBytes { get; set; }
        public int maxPosLogFiles { get; set; }

        public string Preset1Name { get; set; }
        public double Preset1Az { get; set; }
        public double Preset1El { get; set; }
        public string Preset2Name { get; set; }
        public double Preset2Az { get; set; }
        public double Preset2El { get; set; }
        public string Preset3Name { get; set; }
        public double Preset3Az { get; set; }
        public double Preset3El { get; set; }
        public string Preset4Name { get; set; }
        public double Preset4Az { get; set; }
        public double Preset4El { get; set; }
        public string Preset5Name { get; set; }
        public double Preset5Az { get; set; }
        public double Preset5El { get; set; }
    }
}
