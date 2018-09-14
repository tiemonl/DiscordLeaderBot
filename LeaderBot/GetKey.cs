using System.IO;
using System.Text;

namespace LeaderBot {
	public class GetKey {

		public static string getKey(string bot) {
			string keyPath = Path.Combine("Tokens", $"{bot}.token");
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
