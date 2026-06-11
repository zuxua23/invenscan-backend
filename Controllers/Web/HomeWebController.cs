using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        var chartData = await _dashboardService.GetChartDataAsync();
        ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(chartData,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        return View(summary);
    }
}
