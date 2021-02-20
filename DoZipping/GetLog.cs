using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zipper {
    class GetLog {
        static Logger logger = new Logger();
        public static string Leaest() {
            return Base()[Base().Count - 2];
        }

        public static List<string> Nearest(int x) {
            List<string> logs = new List<string>();
            List<string> _logs = Base();
            for (int i = 0; i < x; i++) {
                logs.Add(_logs[i + 1]);
            }
            return logs;
        }

        static List<string> Base() {
            List<string> logs = new List<string>();
            try {

                using (StreamReader s = new StreamReader(Logger.logPath)) {
                    string _logs = s.ReadToEnd();
                    logs = _logs.Split('\n').ToList();
                }
                logs.Reverse();
            }
            catch {
                Task.Delay(2000);//二秒後再試行
                try {
                    logs = new List<string>();
                    using (StreamReader s = new StreamReader(Logger.logPath)) {
                        string _logs = s.ReadToEnd();
                        logs = _logs.Split('\n').ToList();
                    }
                    logs.Reverse();
                }
                catch {
                    throw new Exception();
                }
            }
            return logs;
        }

        public static bool isRunningOtherZipper() {
            string log = Leaest().Substring(28, Leaest().Length - 28);
            logger.Debug("leaest:" + log);
            return !(log == "Exit Process");
        }
    }
}
