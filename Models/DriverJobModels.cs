namespace TMSWeb.Models;
public class UpdateDriverJobModel
{
    public string job_detail_code { get; set; }
    public string job_type { get; set; }
    public string driver_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public byte[] container_pic { get; set; }
    public string trailer_license { get; set; }
    public byte[] seal_pic { get; set; }
    public decimal tare_weight { get; set; }
    public decimal yard { get; set; }
    public int fuel { get; set; }
    public decimal port { get; set; }
    public decimal express { get; set; }
    public decimal lift1 { get; set; }
    public decimal lift2 { get; set; }
    public decimal lift3 { get; set; }
    public string Lat { get; set; }
    public string Lon { get; set; }
    public List<byte[]> floor_images { get; set; }
}