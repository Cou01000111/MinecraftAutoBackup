﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zipper {
    public partial class WaitDialog :Form {
        Label processingContentLabel;
        Label processingContent;
        ProgressBar progressBar;
        public WaitDialog() {
            processingContentLabel = new Label() {
                Text = "現在の処理内容"
            };
            processingContent = new Label();
            progressBar = new ProgressBar() { };
            Controls.AddRange(new Control[] { processingContentLabel,processingContent,progressBar});
            Timer timer = new Timer();
            timer.Interval = 5000;
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer_Tick);
        }

        public void timer_Tick(object sender,EventArgs e) {
            Logger.Info("処理中");
        }
    }
}
