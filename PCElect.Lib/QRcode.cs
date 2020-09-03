using System;
using System.Collections.Generic;
using System.Text;
using Net.Codecrete.QrCodeGenerator;
using System.Drawing.Imaging;

namespace PCElect.Lib
{
    //sudo apt-get install libgdiplus

    public static class QRcode
    {       
        public static void Generate()
        {
            var qr = QrCode.EncodeText("http://zeit.de/api/v/afhfhfhfhfhfhfh?1", QrCode.Ecc.Medium);
            using (var bitmap = qr.ToBitmap(4, 10))
            {
                bitmap.Save("qr-code.png", ImageFormat.Png);
            }
        }
    }
}
//var segments = QrCode.MakeSegments("3141592653589793238462643383");
//var qr = QrCode.EncodeSegments(segments, QrCode.Ecc.High, 5, 5, 2, false);
//for (int y = 0; y < qr.Size; y++)
//{
//    for (int x = 0; x < qr.Size; x++)
//    {
//        ... paint qr.GetModule(x, y)...
//                }
//}