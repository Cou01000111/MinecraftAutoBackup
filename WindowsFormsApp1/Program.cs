using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1 {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    class Form1 :Form {
        Label a;
        public Form1() {
            a = new Label();
            this.Controls.Add(a);
            Form2 f = new Form2();
            f.Owner = this;
            f.Show();
        }
    }

    class Form2 :Form {
        public Form2() {
            this.Owner
        }
    }
}
