using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace testLogger {
    public partial class Form1 :Form {
        public Form1() {
            Logger logger = new Logger("test",".\\logs\\test.log",3);
            Thread.Sleep(3000);
            logger.Info("  1");
            logger.Info("  2");
            logger.Info("  3");
            logger.Info("  4");
            logger.Info("  5");
            logger.Info("  6");
            logger.Info("  7");
            logger.Info("  8");
            logger.Info("  9");
            Thread.Sleep(3000);
            Logger logger2 = new Logger("test2",".\\logs\\test.log", 3);
            Thread.Sleep(3000);
            logger2.Info(" 1");
            logger2.Info(" 2");
            logger2.Info(" 3");
            logger2.Info(" 4");
            logger2.Info(" 5");
            logger2.Info(" 6");
            logger2.Info(" 7");
            logger2.Info(" 8");
            logger2.Info(" 9");
        }
    }
}
