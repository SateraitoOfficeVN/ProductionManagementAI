using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Application.Dashboard;
using ProductionManagementAI.Application.Health;

namespace ProductionManagementAI.Api.Controllers;

/// <summary>DD-003-API §1. Takes no parameters: nothing is bound from the request, so a query string has no effect.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public class DashboardController(DashboardService service) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await service.GetSnapshotAsync(cancellationToken));
}

/// <summary>DD-003-API §2. The cookie handler never renews the session for this path (WI-004 DEC-019).</summary>
[ApiController]
[Route("api/system")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public class SystemController(SystemHealthService service) : ControllerBase
{
    [HttpGet("health")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<SystemHealthResponse>> Health(CancellationToken cancellationToken) =>
        Ok(await service.CheckAsync(cancellationToken));
}
