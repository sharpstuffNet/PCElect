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

namespace PCElect.Lib
{
    public partial class PCElect
    {
        static object lockO = new Object();
        public void Vote(string vID, string[] vote)
        {
            using (var csp = new RSACryptoServiceProvider(2048))
            {
                csp.FromXmlString(IO.File.ReadAllText(IO.Path.Combine(_mainDir, "Key.pub")));

                using (var md = SHA512.Create())
                {
                    var hshf = IO.Path.Combine(_mainDir, "HSH.txt");
                    if (IO.File.Exists(hshf))
                    {
                        var sh = System.Convert.ToBase64String(md.ComputeHash(TX.UTF8Encoding.UTF8.GetBytes(vID)));
                        if (!IO.File.ReadAllText(hshf).Contains(sh))
                            throw new Exception("HSH not found");
                    }

                    var sb = new TX.StringBuilder();
                    sb.AppendLine(vID);
                    sb.AppendLine(DateTime.UtcNow.ToString("yyyy MMdd HH:mm:ss.FFFF"));
                    sb.AppendLine(string.Join(",", vote));

                    int idx = 0;
                    lock (lockO)
                    {
                        var fi = IO.Directory.GetFiles(_mainDir, "*.vote");
                        idx = fi.Length + 1;

                        if (idx > 1)
                        {
                            var tx = IO.File.ReadAllBytes(IO.Path.Combine(_mainDir, $"{idx - 1}.vote"));
                            sb.AppendLine(System.Convert.ToBase64String(md.ComputeHash(tx)));
                        }

                        var ba = csp.Encrypt(TX.UTF8Encoding.UTF8.GetBytes(sb.ToString()), false);
                        IO.File.WriteAllBytes(IO.Path.Combine(_mainDir, $"{idx}.vote"), ba);
                    }
                }
            }
        }
    }
}

