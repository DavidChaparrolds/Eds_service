using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDS.tools
{
    internal class Logger
    {

        private static string logDirectory = Path.Combine(System.Environment.SystemDirectory.Substring(0, 3), "LineaDatascan\\Logs");
        private static long _logSizeLimit = 51200000;
        public static void EscribirLog(string mensaje)
        {
            if (!Directory.Exists(Logger.logDirectory))
                Directory.CreateDirectory(Logger.logDirectory);
            string str = Path.Combine(Logger.logDirectory, "EDS.txt");
            long num = 0;
            lock (str)
            {
                StreamWriter streamWriter = (StreamWriter)null;
                try
                {
                    streamWriter = new StreamWriter((Stream)new FileStream(str, FileMode.Append, FileAccess.Write, FileShare.Read));
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(string.Format("----->{0}", (object)DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss fff")));
                    streamWriter.WriteLine(mensaje);
                    streamWriter.WriteLine("----------------------");
                    num = streamWriter.BaseStream.Length;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    streamWriter?.Close();
                }
                if (num <= Logger._logSizeLimit)
                    return;
                string destFileName = Path.Combine(Logger.logDirectory, string.Format("EDS{0}.txt", (object)DateTime.Now.ToString("yyyyMMddHHmmss")));
                File.Move(str, destFileName);
            }
        }

        public static void EscribirLog(Exception ex)
        {
            if (!Directory.Exists(Logger.logDirectory))
                Directory.CreateDirectory(Logger.logDirectory);
            string str = Path.Combine(Logger.logDirectory, "EDS.txt");
            long num = 0;
            lock (str)
            {
                StreamWriter streamWriter = (StreamWriter)null;
                try
                {
                    streamWriter = new StreamWriter((Stream)new FileStream(str, FileMode.Append, FileAccess.Write, FileShare.Read));
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(string.Format("----->{0}", (object)DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss fff")));
                    streamWriter.WriteLine(ex.Message);
                    streamWriter.WriteLine("---------STACK TRACE-------------");
                    streamWriter.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        streamWriter.WriteLine("---------INNER EXCEPTION-------------");
                        streamWriter.WriteLine(ex.InnerException.Message);
                        streamWriter.WriteLine("---------INNER EXCEPTION STACK TRACE-------------");
                        streamWriter.WriteLine(ex.InnerException.StackTrace);
                    }
                    num = streamWriter.BaseStream.Length;
                }
                catch
                {
                }
                finally
                {
                    streamWriter?.Close();
                }
                if (num <= Logger._logSizeLimit)
                    return;
                string destFileName = Path.Combine(Logger.logDirectory, string.Format("EDS{0}.txt", (object)DateTime.Now.ToString("yyyyMMddHHmmss")));
                File.Move(str, destFileName);
            }
        }

    }
}
