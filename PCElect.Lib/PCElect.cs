using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using TX=System.Text;
using System.Web;
using IO = System.IO;
using System.Linq;

namespace PCElect.Lib
{
    public partial class PCElect
    {
        string _mainDir;
        IConfiguration _config;
        ILogger _logger;

        public PCElect(string dir, IConfiguration config, ILogger logger)
        {
            _mainDir = dir;
            _config = config;
            _logger = logger;
        }

        public void Init()
        {
            using (var csp = new RSACryptoServiceProvider(2048))
            {
                IO.File.WriteAllBytes(IO.Path.Combine(_mainDir, "Key.blob"), csp.ExportCspBlob(true));
                IO.File.WriteAllText(IO.Path.Combine(_mainDir, "Key.priv"), csp.ToXmlString(true));
                IO.File.WriteAllText(IO.Path.Combine(_mainDir, "Key.pub"), csp.ToXmlString(false));
            }
        }

        public void Results()
        {
            using (var csp = new RSACryptoServiceProvider(2048))
            {
                csp.FromXmlString(IO.File.ReadAllText(IO.Path.Combine(_mainDir, "Key.priv")));

                foreach (var item in IO.Directory.GetFiles(_mainDir, "*.vote"))
                {
                    var ba= csp.Decrypt(IO.File.ReadAllBytes(item), false);
                    var bs= TX.UTF8Encoding.UTF8.GetString(ba);
                    IO.File.WriteAllText(IO.Path.ChangeExtension(item, ".vote.txt"),bs);
                }
            }
        }
            public void AddVotes(int v)
        {
            
            var tmpl = IO.File.ReadAllText(IO.Path.Combine(_mainDir, "Template.html"));
            var j = tmpl.IndexOf("<!--Start-->");
            var k = tmpl.IndexOf("<!--End-->");
            var mid = tmpl.Substring(j+12, k-j-12);
            var idf = IO.Path.Combine(_mainDir, "IDs.txt");

            var sb = new TX.StringBuilder(tmpl.Substring(0, j));

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

            var hshf = IO.Path.Combine(_mainDir, "HSH.txt");
            using (var md = SHA512.Create())
            {
                var ids = IO.File.ReadAllLines(IO.Path.Combine(_mainDir, "IDs.txt"));
                var hsh = from a in ids select System.Convert.ToBase64String(md.ComputeHash(TX.UTF8Encoding.UTF8.GetBytes(a)));
                if (IO.File.Exists(hshf))
                    IO.File.Delete(hshf);
                IO.File.WriteAllLines(hshf, hsh);
            }
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
