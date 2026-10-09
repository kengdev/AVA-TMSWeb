using TMSWeb.Models.Entities.LOG;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TMSWeb.Repositories;

public class LineNotifRepository
{
    private readonly DbavalogContext db;

    // 👇 ใส่ token ตรงนี้เลย
    private static readonly string ChannelAccessToken = "IHB+FmHkgA2MTADoc4yn+aNc9RSLbvtyE+HtO1+RmoeRLa9f4SVXP0JfUJSVWgG6OPfIv4GwQmBBHDLZbGF6c4fhGrJpIx57IvJrMW1Az2MlB+Lrcb3NyvAGkSTGwfNP/qs9g4j6OwQnhWJ+OX4ssQdB04t89/1O/w1cDnyilFU=";

    // 👇 groupId ต้องได้จาก webhook ตอนบอท join group
    private static readonly string GroupId = "C83b3c58bb806276560a19e34f3856a78";

    public async Task NotifGroup(string message)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

        var payload = new
        {
            to = GroupId,
            messages = new[] { new { type = "text", text = message } }
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.line.me/v2/bot/message/push", content);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("ส่งข้อความสำเร็จ!");
        }
        else
        {
            Console.WriteLine($"ส่งข้อความล้มเหลว: {(int)response.StatusCode}");
            Console.WriteLine("รายละเอียด: " + raw);
        }
    }
}
