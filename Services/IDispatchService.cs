using TMSWeb.Models;
namespace TMSWeb.Services;
public interface IDispatchService
{
    DispatchSnapshot Snapshot();
    DispatchResult Assign(AssignRequest request);
    DispatchResult Move(MoveRequest request);
    DispatchResult Cancel(string assignmentId);
    DispatchResult ConfirmPartner(string vehicleId);
}
