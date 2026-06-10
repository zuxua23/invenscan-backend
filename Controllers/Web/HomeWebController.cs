using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Web;

[Route("")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class HomeWebController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeWebController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return View(summary);
    }
}
