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
			var perH = new Dictionary<int, int>();
			DateTime? DTStart = null;

			var unknKey = 0;
			var brknBC = 0;
			var invalidVote = 0;
			var mindchanged=0;

			var VIDs = (from a in IO.File.ReadAllLines(IO.Path.Combine(_mainDir, "IDs.txt")) select HttpUtility.UrlDecode(a)).ToArray();
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
					if(VIDs[0] == ov.Key || VIDs[1] == ov.Key)
                    {
						if (!DTStart.HasValue)
							DTStart = ov.DT;
						continue;
					}
					if (!VIDs.Contains(ov.Key))
					{
						//Unknown Key
						unknKey++;
						continue;
					}
					else if (hsh != null && hsh != lines[3].Trim())
					{
						//Broken BC investigate
						brknBC++;
						Debugger.Break();
					}
					else if (ov.Vote.Length > 3)
					{
						//Ungültig
						invalidVote++;
						continue;
					}
					//Valid Vote
					if (!firstVote.ContainsKey(ov.Key))
						firstVote.Add(ov.Key, ov);
					else
						mindchanged++;

					lastVote[ov.Key] = ov;

					//Time
					var hh = (int)((ov.DT - DTStart.Value).TotalHours);
					if (!perH.ContainsKey(hh))
						perH[hh] = 1;
					else
						perH[hh]++;

					hsh = System.Convert.ToBase64String(md.ComputeHash(ba));
				}
			}
			sb.AppendLine();

			sb.AppendLine("Stats");
			sb.AppendLine($"Total Valid:\t{lastVote.Count}");
			sb.AppendLine($"Unknown VID:\t{unknKey}");
			sb.AppendLine($"Broken Chain:\t{brknBC}");
			sb.AppendLine($"Invalid Vote:\t{invalidVote}");
			sb.AppendLine($"Changed Vote:\t{mindchanged}");
			sb.AppendLine($"Total Pos:\t{lastVote.Count*3}");
			sb.AppendLine($"Total First:\t{(from a in firstVote.Values select a.Vote.Length).Sum()}");
			sb.AppendLine($"Total Last:\t{(from a in lastVote.Values select a.Vote.Length).Sum()}");

			sb.AppendLine("Times");
            foreach (var item in from h in perH.OrderBy(x=>x.Key) select h)
				sb.AppendLine($"{DTStart.Value.AddHours(item.Key).ToString("dd\tHH")}\t{item.Value}");
			sb.AppendLine();

			sb.AppendLine("First Votes");
			CreateStatistics(sb,firstVote.Values);
			sb.AppendLine();

			sb.AppendLine("Last Votes");
			CreateStatistics(sb,lastVote.Values);
			sb.AppendLine();

			IO.File.WriteAllText(IO.Path.Combine(_mainDir, $"{DateTime.Now.ToString("yyMMdd HHmm")}_Results.txt"),sb.ToString());
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
            foreach (var item in res.OrderBy(r=>r.Value))
				sb.AppendLine($"{item.Key}\t{item.Value}");

		}
	}
}
