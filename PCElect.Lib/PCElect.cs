using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using IO = System.IO;

namespace PCElect.Lib
{
    public class PCElect
    {
        string _mainDir;
        IConfigurationRoot _config;
        ILogger _logger;

        public PCElect(string dir, IConfigurationRoot config, ILogger logger)
        {
            _mainDir = dir;
            _config = config;
            _logger = logger;
        }

        public void Init()
        {

        }

        public void Results(string v)
        {

        }

        public void AddVotes(int v)
        {
            
            var tmpl = IO.File.ReadAllText(IO.Path.Combine(_mainDir, "Template.html"));
            var j = tmpl.IndexOf("<!--Start-->");
            var k = tmpl.IndexOf("<!--End-->");
            //var st = tmpl.Substring(0, j);
            //var en = tmpl.Substring(k+10);
            var mid = tmpl.Substring(j+12, k-j-12);
            var idf = IO.Path.Combine(_mainDir, "IDs.txt");

            var sb = new StringBuilder(tmpl.Substring(0, j));

            using (var random = RandomNumberGenerator.Create())
            {
                if (!IO.File.Exists(idf))
                {
                    //STartID
                    var id = GetID(random);
                    IO.File.AppendAllText(idf, $"{id}{Environment.NewLine}");
                    sb.Append(GetFor(mid,id, -1, "Start"));

                    //EndID
                    id = GetID(random);
                    IO.File.AppendAllText(idf, $"{id}{Environment.NewLine}");
                    sb.Append(GetFor(mid,id, -1, "End"));
                }
                

                for (int i = 0; i < v; i++)
                {
                    var id = GetID(random);
                    IO.File.AppendAllText(idf, $"{id}{Environment.NewLine}");
                    sb.Append(GetFor(mid,id, i, ""));
                }
            }
            sb.Append(tmpl.Substring(k + 10));
            IO.File.WriteAllText(IO.Path.Combine(_mainDir, "IDs.html"), sb.ToString());
        }

        private string GetFor(string html, string id, int idx, string head)
        {
            var url = @"https://kita.dewr.me/?VID=";

            var qr = QrCode.EncodeText($"{url}{id}", QrCode.Ecc.Medium);
            using (var bitmap = qr.ToBitmap(4, 10))
            {
                bitmap.Save(IO.Path.Combine(_mainDir,"img", $"qr{idx}.png"), ImageFormat.Png);
            }

            var s = html.Replace("-BBBBB-", head);
            s = s.Replace("-UUUUU-", $"{url}{id}");
            s = s.Replace("-QQQQQ-.png", $"qr{idx}.png");
            return s;
        }

        private static string GetID(RandomNumberGenerator random)
        {
            var ba = new byte[16];
            random.GetNonZeroBytes(ba);
            var str = System.Convert.ToBase64String(ba);
            return HttpUtility.UrlEncode(str.Substring(0,str.Length-2));
        }
    }
}
