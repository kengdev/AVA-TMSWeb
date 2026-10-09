using TMSWeb.Models;
using TMSWeb.Services;
using Microsoft.AspNetCore.Mvc;
namespace TMSWeb.Controllers;
[AutoValidateAntiforgeryToken]
public sealed class DispatchController(IDispatchService service) : Controller
{
    public IActionResult Index() => View();
    [HttpGet] public IActionResult Data() => Json(service.Snapshot());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Assign([FromBody] AssignRequest request) => ModelState.IsValid ? Result(service.Assign(request)) : BadRequest(new DispatchResult(false,"ข้อมูลวันเวลาไม่ถูกต้อง"));
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Move([FromBody] MoveRequest request) => ModelState.IsValid ? Result(service.Move(request)) : BadRequest(new DispatchResult(false,"ข้อมูลวันเวลาไม่ถูกต้อง"));
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancel([FromBody] string assignmentId) => Result(service.Cancel(assignmentId));
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ConfirmPartner([FromBody] string vehicleId) => Result(service.ConfirmPartner(vehicleId));
    private IActionResult Result(DispatchResult result) => result.Success ? Ok(result) : Conflict(result);
}
