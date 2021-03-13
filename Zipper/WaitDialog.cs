using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zipper {
    public partial class WaitDialog :Form {
        private static Logger logger = new Logger("Zipper");
        private int processCount = 0;
        private FlowLayoutPanel basePanel;
        private Label processingContentLabel;
        private TextBox processingContent;
        private string[] argsProperty;
        public WaitDialog(string[] args) {
            argsProperty = args;
            Console.WriteLine("WaitDialogを表示しました");
            basePanel = new FlowLayoutPanel() {
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                Size = new Size(400, 300),
            };
            processingContentLabel = new Label() {
                Text = "現在の処理内容 ",
                AutoSize = true,
            };
            processingContent = new TextBox() {
                Multiline = true,
                Size = new Size(370, 260),
                Anchor = (AnchorStyles.Right) | (AnchorStyles.Bottom),
                ScrollBars = ScrollBars.Vertical,
                BackColor = ColorTranslator.FromHtml("0x090909"),
                ForeColor = ColorTranslator.FromHtml("0xf3f3f3"),
            };
            basePanel.Controls.AddRange(new Control[] { processingContentLabel, processingContent });
            Controls.Add(basePanel);
            ClientSize = new Size(400, 300);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer_Tick);
            Shown += WaitDialog_Shown;
        }

        public void timer_Tick(object sender, EventArgs e) {
            processCount++;
            this.processingContent.Text = string.Join("\r\n", Program.logs);
            string addString;
            switch ((processCount % 10 < 5 ? processCount % 10 : 4)) {
                case 0:
                    addString = "■理中";
                    break;
                case 1:
                    addString = "処■中";
                    break;
                case 2:
                    addString = "処理■";
                    break;
                case 3:
                    addString = "処■中";
                    break;
                case 4:
                    addString = "処理中";
                    break;
                default:
                    addString = " ";
                    break;
            }
            this.processingContentLabel.Text = "現在の処理内容 " + addString;
            //Logger.Debug($"processingContent text:{this.processingContent.Text}");

        }

        public async void WaitDialog_Shown(object sender, EventArgs e) {
            string[] args = ((WaitDialog)sender).argsProperty;
            Task<int> t = await Task.Run(async () => Process.MainProcess(args));
            logger.Info("処理が完了しました");
            MessageBox.Show("処理が完了しました","Minecraft Auto Backup",MessageBoxButtons.OK);
            this.Close();
        }
    }
}
