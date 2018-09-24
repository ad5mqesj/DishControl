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
            azMax = 370.0;
            azMin = -10.0;
            azOutMax = 16384.0;
            azOutMin = 0.0;
            azPark = 0.0;
            elDriveType = driveType.Both;
            elActiveHi = true;
            elCWbit = 2;
            elCCWbit = 3;
            elEnable = -1;
            elPWMchan = 1;
            elKp = 1.0;
            elKi = 0.0;
            elKd = 0.0;
            elMax = 90.0;
            elMin = 0.0;
            elOutMax = 16384.0;
            elOutMin = 0.0;
            elPark = 89.99;
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
        public double azMax { get; set; }
        public double azMin { get; set; }
        public double azOutMax { get; set; }
        public double azOutMin { get; set; }

        public double azPark { get; set; }

        public driveType elDriveType { get; set; }
        public bool elActiveHi { get; set; }
        public int elCWbit { get; set; }
        public int elCCWbit { get; set; }
        public int elEnable { get; set; }
        public int elPWMchan { get; set; }

        public double elKp { get; set; }
        public double elKi { get; set; }
        public double elKd { get; set; }
        public double elMax { get; set; }
        public double elMin { get; set; }
        public double elOutMax { get; set; }
        public double elOutMin { get; set; }
        public double elPark { get; set; }
    }
}
