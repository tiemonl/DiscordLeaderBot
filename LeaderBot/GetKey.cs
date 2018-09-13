using System.IO;
using System.Text;

namespace LeaderBot {
	public class GetKey {
		private static string keyPath = "debug.token";

		public static string getKey() {
			if (File.Exists(keyPath)) {
				using (StreamReader sr = new StreamReader(keyPath, Encoding.UTF8)) {
					string key = sr.ReadToEnd();
					return key;
				}
			} else {
				return null;
			}
		}
	}
}
