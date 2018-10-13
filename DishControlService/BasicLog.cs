using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DishControl
{
    public class BasicLog
    {
        public static void writeLog(string msg)
        {
            try
            {
                string path = "c:\\windows\\temp";
#if _TEST
                string enableLog = "true";
#else
                string enableLog = "false";
#endif
                if (enableLog.ToLower().Equals("true"))
                {
                    File.AppendAllText(path + "\\DishControlLog.txt", DateTime.Now.ToString() + " : " + msg + "\r\n");
                }
                PerformFileTrim(path + "\\DishControlLog.txt");
            }
            catch (Exception) { }//if logging fails ignore
        }
        private static void PerformFileTrim(string filename)
        {
            var fileSize = (new System.IO.FileInfo(filename)).Length;

            if (fileSize > 5000000)
            {
                var text = File.ReadAllText(filename);
                var amountToCull = (int)(text.Length * 0.33);
                amountToCull = text.IndexOf('\n', amountToCull);
                var trimmedText = text.Substring(amountToCull + 1);
                File.WriteAllText(filename, trimmedText);
            }
        }

    }

    public class RollingLogger
    {
        static string LOG_FILE = @"c:\temp\logfile.log";
        static int MaxRolledLogCount = 3;
        static int MaxLogSize = 1024; // 1 * 1024 * 1024; <- small value for testing that it works, you can try yourself, and then use a reasonable size, like 1M-10M

        public static void setupRollingLogger(string file, int maxSize, int numFiles)
        {
            LOG_FILE = file;
            MaxRolledLogCount = numFiles;
            MaxLogSize = maxSize;
        }

        public static void LogMessage(string msg)
        {
            lock (LOG_FILE) // lock is optional, but.. should this ever be called by multiple threads, it is safer
            {
                RollLogFile(LOG_FILE);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                string timeStamp = String.Format("{0:G}  \t",DateTime.UtcNow);
                try
                {
                    File.AppendAllText(LOG_FILE, timeStamp + msg + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception)
                { }
            }
        }

        private static void RollLogFile(string logFilePath)
        {
            try
            {
                var length = new FileInfo(logFilePath).Length;

                if (length > MaxLogSize)
                {
                    var path = Path.GetDirectoryName(logFilePath);
                    var wildLogName = Path.GetFileNameWithoutExtension(logFilePath) + "*" + Path.GetExtension(logFilePath);
                    var bareLogFilePath = Path.Combine(path, Path.GetFileNameWithoutExtension(logFilePath));
                    string[] logFileList = Directory.GetFiles(path, wildLogName, SearchOption.TopDirectoryOnly);
                    if (logFileList.Length > 0)
                    {
                        // only take files like logfilename.log and logfilename.0.log, so there also can be a maximum of 10 additional rolled files (0..9)
                        var rolledLogFileList = logFileList.Where(fileName => fileName.Length == (logFilePath.Length + 2)).ToArray();
                        Array.Sort(rolledLogFileList, 0, rolledLogFileList.Length);
                        if (rolledLogFileList.Length >= MaxRolledLogCount)
                        {
                            File.Delete(rolledLogFileList[MaxRolledLogCount - 1]);
                            var list = rolledLogFileList.ToList();
                            list.RemoveAt(MaxRolledLogCount - 1);
                            rolledLogFileList = list.ToArray();
                        }
                        // move remaining rolled files
                        for (int i = rolledLogFileList.Length; i > 0; --i)
                            File.Move(rolledLogFileList[i - 1], bareLogFilePath + "." + i + Path.GetExtension(logFilePath));
                        var targetPath = bareLogFilePath + ".0" + Path.GetExtension(logFilePath);
                        // move original file
                        File.Move(logFilePath, targetPath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
