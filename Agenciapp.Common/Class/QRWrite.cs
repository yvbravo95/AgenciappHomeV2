
using ThoughtWorks.QRCode.Codec;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Drawing.Imaging;

namespace Agenciapp.Common.Class
{
    public class QRWrite
    {
        private IWebHostEnvironment _env;
        private QRCodeEncoder qrCodeEncoder;

        public QRWrite(IWebHostEnvironment env)
        {
            _env = env;
            qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            int scale = Convert.ToInt32("5");
            qrCodeEncoder.QRCodeScale = scale;
        }

        public string ShowQR(string filename, string text)
        {
            string sWebRootFolder = _env.WebRootPath;
            string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "UserFiles" + Path.DirectorySeparatorChar + "QR";

            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

            filePath = Path.Combine(filePath, filename);

            if (File.Exists(filePath))
                File.Delete(filePath);

            Image image;
            image = qrCodeEncoder.Encode(text);
            image.Save(filePath, ImageFormat.Jpeg);

            return filePath;
        }
    }
}
