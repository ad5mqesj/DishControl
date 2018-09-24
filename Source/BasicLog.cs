using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
					System.IO.File.AppendAllText(path + "\\DishControlLog.txt", DateTime.Now.ToString() + " : " + msg + "\r\n");
				}
			}
			catch (Exception) { }//if logging fails ignore
		}
	}
}
