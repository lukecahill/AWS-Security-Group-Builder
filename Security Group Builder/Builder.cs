using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Security_Group_Builder {
	public class Builder {
		List<SecurityGroup> securityGroupList = new List<SecurityGroup>();
		StringBuilder sb = new StringBuilder();
		ArrayList groupNames = new ArrayList();
		string first = "";
		public void start(TextBox textBox1, TextBox textBox2) {
			var tab = '\t';
			var newLine = '\n';
			var split = textBox2.Text.Trim().Replace("\r", "").Split(newLine);

			foreach (var item in split) {
				if (item.Equals("")) { continue; }
				var t = item.Split(tab);

				if (t.Length != 6) {
					MessageBox.Show("There should be a maximum of 6 items on each line. See the help button for the correct format.");
					return;
				}
				securityGroupList.Add(new SecurityGroup {
					Name = t[0],
					Description = t[1],
					Direction = t[2],
					Protocol = t[3],
					Port = t[4],
					IpAddress = t[5]
				});
			}

			buildString(textBox1);
		}

		public void buildString(TextBox textBox1) {
			var i = 0;
			generateSecurityGroup(securityGroupList[0]);

			foreach (var group in securityGroupList) {
				i++;
				var description = group.Description.Replace(" ", "");

				if (!groupNames.Contains(group.Name)) {
					groupNames.Add(group.Name);
				}

				if (groupNames.Contains(group.Name)) {
					group.Name += "" + i;
				}

				sb.Append("\"" + group.Name + description + i);

				if (group.Direction == "Ingress") {
					// yes I know it would be easier building using JSON.Net - but I had some problems with this. 
					sb.Append("\": { \"Type\" : \"AWS::EC2::SecurityGroupIngress\", \"Properties\" : ");
				} else if (group.Direction == "Egress") {
					if (group.IpAddress == "all") {
						continue;   // skip the rest of this loop. The egress is default is all if left blank so this is not needed.
					}
					sb.Append("\": { \"Type\" : \"AWS::EC2::SecurityGroupEgress\", \"Properties\" : ");
				}

				var ports = splitPorts(group);

				determineSecurityGroup(group, ports);

				sb.Append("}");
				if (i != securityGroupList.Count) {
					sb.Append(",");
				}
			}

			if (checkValid()) {
				sb.ToString().Replace("\\", "");
				writeToFile(textBox1);
			} else {
				MessageBox.Show("Something went wrong when formatting code.\n\nPlease check the input.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool checkValid() {
			try {
				var tmpString = new StringBuilder();
				tmpString.Append("{");
				tmpString.Append(sb.ToString());
				tmpString.Append("}");
				var tmp = JsonValue.Parse(tmpString.ToString());
				return true;

			} catch (FormatException fex) {
				Debug.WriteLine(fex.Message);
				return false;

			} catch (Exception ex) {
				Debug.WriteLine(ex.Message);
				return false;
			}
		}

		private void writeToFile(TextBox textBox1) {
			var filename = "security-groups.txt";
			if (!String.IsNullOrWhiteSpace(textBox1.Text)) {
				if (textBox1.Text.EndsWith(".txt")) {
					filename = textBox1.Text;
				} else {
					filename = $"{textBox1.Text}.txt";
				}

			}
			try {
				using (var writer = new StreamWriter(filename)) {
					writer.Write(sb.ToString());
					writer.Flush();
				}
				MessageBox.Show("Completed", "", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
			} catch (IOException ex) {
				MessageBox.Show($"Could not write to file. Error message: {ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (Exception e) {
				MessageBox.Show($"Could not write to file. Error message: {e.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				sb.Clear();
			}
		}

		private void generateSecurityGroup(SecurityGroup group) {
			sb.Append("\"" + group.Name);
			first = group.Name;
			sb.Append("\": { \"Type\" : \"AWS::EC2::SecurityGroup\", \"Properties\" : { ");
			sb.Append("\"VpcId\" : \"\" } },");
		}

		private void determineSecurityGroup(SecurityGroup group, string[] ports) {

			if (Regex.IsMatch(group.IpAddress, "[a-zA-Z]")) {
				sb.Append("{ \"FromPort\" : \"" + ports[0] + "\",");
				sb.Append("\"ToPort\" : \"" + ports[1] + "\",");
				sb.Append("\"SourceSecurityGroupId\" : { \"Ref\" : \"" + group.IpAddress + "\" },");
				sb.Append("\"GroupId\" : { \"Ref\" : \"" + first + "\" }}");

			} else {
				sb.Append("{ \"FromPort\" : \"" + ports[0] + "\",");
				sb.Append("\"ToPort\" : \"" + ports[1] + "\",");
				sb.Append("\"CidrIp\" : \"" + group.IpAddress + "\",");
				sb.Append("\"GroupId\" : { \"Ref\" : \"" + first + "\" }}");
			}
		}

		public string[] splitPorts(SecurityGroup group) {
			string[] ports = new string[2];
			if (group.Port.IndexOf('-') > -1) {
				ports = group.Port.Split('-');
			} else {
				ports[0] = group.Port;
				ports[1] = group.Port;
			}

			return ports;
		}
	}
}
