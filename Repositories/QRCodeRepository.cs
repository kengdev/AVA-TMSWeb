using QRCoder;
using Rotativa.AspNetCore.Options;
using System.Drawing;
using System.Drawing.Imaging;

namespace TMSWeb.Repositories;

public class QRCodeRepository
{
    public string JobCodeURL(string code)
    {
        string url =
            "http://avagloballogistics.com/driverjob/floorinvoice?code=" + code;

        using var ms = new MemoryStream();
        using var generator = new QRCodeGenerator();
        using var data =
            generator.CreateQrCode(
                url,
                QRCodeGenerator.ECCLevel.H);

        using var qrCode = new QRCode(data);
        using Bitmap bitmap = qrCode.GetGraphic(50);

        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        return "data:image/png;base64," +
               Convert.ToBase64String(ms.ToArray());
    }
}
