using System;
using Security_Group_Builder;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SGBUnitTests {
	[TestClass]
	public class UnitTest1 {
		Builder builder = new Builder();

		[TestMethod]
		public void splitPorts() {
			var sg = new SecurityGroup {
				Port = "997"
			};

			var result = builder.splitPorts(sg);
			var expected = new string[] { "997", "997" };
			Assert.IsTrue(result.SequenceEqual(expected));
		}

		public void writeToFile() { }
	}
}
