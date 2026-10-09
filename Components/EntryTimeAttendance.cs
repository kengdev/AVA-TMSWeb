using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace TMSWeb.Components
{
    public class EntryTimeAttendance : ViewComponent
    {
        private readonly DashboardRepository dashboardRepo;

        public EntryTimeAttendance(DashboardRepository _dashboardRepo)
        {
            dashboardRepo = _dashboardRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var attData = await dashboardRepo.GetTimeAttendance();

            return View(attData);
        }
    }
}
