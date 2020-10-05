using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using TX = System.Text;
using System.Web;
using IO = System.IO;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PCElect.Lib
{
	public partial class PCElect
	{
		struct OneVote
		{
			public string Key { get; set; }
			public DateTime DT { get; set; }
			public string[] Vote { get; set; }
		}

		public void Results()
		{
			var sb = new StringBuilder();
			var firstVote = new Dictionary<string, OneVote>();
			var lastVote = new Dictionary<string, OneVote>();

			var VIDs = IO.File.ReadAllLines(IO.Path.Combine(_mainDir, "IDs.txt"));
			using (var md = SHA512.Create())
			using (var csp = new RSACryptoServiceProvider(2048))
			{
				csp.FromXmlString(IO.File.ReadAllText(IO.Path.Combine(_mainDir, "Key.priv")));

				string hsh = null;

				for (int i = 1; ; i++)
				{
					var fn = IO.Path.Combine(_mainDir, $"{i}.vote");
					if (!IO.File.Exists(fn))
						break;

					var ba = IO.File.ReadAllBytes(fn);
					var bad = csp.Decrypt(ba, false);
					var bs = TX.UTF8Encoding.UTF8.GetString(bad);
					var lines = bs.Replace("\n", "\r").Replace("\r\r", "\r").Split('\r');
					var ov = new OneVote()
					{
						Key = lines[0].Trim(),
						DT = DateTime.ParseExact(lines[1].Trim(), "yyyy MMdd HH:mm:ss.FFFF", CultureInfo.InvariantCulture),
						Vote = lines[2].Trim().Split(',')
					};
					if (!VIDs.Contains(ov.Key) && !VIDs.Contains(HttpUtility.UrlEncode(ov.Key)))
					{
						//Unknown Key

						continue;
					}
					else if (hsh != null && hsh != lines[3].Trim())
					{
						//Broken BC investigate
						Debugger.Break();
					}
					else if (ov.Vote.Length > 3)
					{
						//Ungültig

						continue;
					}
					if (!firstVote.ContainsKey(ov.Key))
						firstVote.Add(ov.Key, ov);
					lastVote[ov.Key] = ov;

					hsh = System.Convert.ToBase64String(md.ComputeHash(ba));
				}
			}

			
			CreateStatistics(sb,firstVote.Values);
			CreateStatistics(sb,lastVote.Values);

			IO.File.WriteAllText(IO.Path.Combine(_mainDir, "Results.txt"),sb.ToString());
		}

		private void CreateStatistics(StringBuilder sb, IEnumerable<OneVote> votes)
		{
			var res = new Dictionary<string, int>();
            foreach (var item in votes)
            {
                foreach (var vv in item.Vote)
                {
					if (!res.ContainsKey(vv))
						res.Add(vv, 1);
					else
						res[vv]++;
                }
            }
		}
	}
}
