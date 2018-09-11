using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderBot
{
	/// <summary>
	/// This is used to alleviate boilerplate code
	/// </summary>
    class SupportingMethods
    {
		public bool stringEquals(string a, string b) {
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
