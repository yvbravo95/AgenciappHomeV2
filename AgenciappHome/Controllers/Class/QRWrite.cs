using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using ThoughtWorks.QRCode.Codec;

namespace AgenciappHome.Controllers.Class
{
    public class QRWrite
    {
        private QRCodeEncoder qrCodeEncoder;
        private IWebHostEnvironment _env;
        
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

            if(!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

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