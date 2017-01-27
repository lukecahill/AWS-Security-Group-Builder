using System;
using System.Windows.Forms;

namespace Security_Group_Builder {
	public partial class Form1 : Form {
		Builder builder = null;

		public Form1() {
			InitializeComponent();
			builder = new Builder(textBox1.Text);
		}

		private void startBtn_Click(object sender, EventArgs e) {
			builder.start(textBox1, textBox2);
		}

		private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show("The format for this should be NAME DESCRIPTION DIRECTION PROTOCOL PORT IPADDRESS.\n\nThese should be tab separated values.\n\nThe IPADDRESS can be either a CidrIp or a Security group name.\n\nFormatting can be done using the formatting tool of your choice.\n\nCreated by Luke Cahill\n\nSource: https://github.com/lukecahill/AWS-Security-Group-Builder", "Help", MessageBoxButtons.OK);
		}

		private void clearButton_Click(object sender, EventArgs e) {
			textBox2.Clear();
			builder.reset();
		}
	}
}
