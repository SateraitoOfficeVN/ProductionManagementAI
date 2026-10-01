using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Api.ProductionLines;
using ProductionManagementAI.Application.ProductionLines;

namespace ProductionManagementAI.Api.Controllers;

/// <summary>Exposes the seven authorized Production lines operations with strict feature boundaries.</summary>
[ApiController]
[Route("api/production-lines")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public sealed class ProductionLinesController(ProductionLineService service) : ControllerBase
{
    /// <summary>Lists lines using bounded literal filters and snapshot paging.</summary>
    [HttpGet]
    public async Task<ActionResult> List(CancellationToken ct)
    {
        if (!LineRequestReader.Query(Request, "q", "state", "page")) return Invalid();
        return LineProblems.Render(this, await service.ListAsync(new(Request.Query["q"].FirstOrDefault(),
            Request.Query["state"].FirstOrDefault(), Request.Query["page"].FirstOrDefault()), ct), Ok);
    }
    /// <summary>Reads a line and a snapshot page of active or retired associations.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(string id, CancellationToken ct)
    {
        if (!LineRequestReader.Id(id, out var lineId)) return Invalid("id");
        if (!LineRequestReader.Query(Request, "pairsPage")) return Invalid();
        return LineProblems.Render(this, await service.GetAsync(lineId, Request.Query["pairsPage"].FirstOrDefault(), ct), Ok);
    }
    /// <summary>Reads active products excluding all persisted associations for an editing line.</summary>
    [HttpGet("product-choices")]
    public async Task<ActionResult> ProductChoices(CancellationToken ct)
    {
        if (!LineRequestReader.Query(Request, "q", "page", "lineId")) return Invalid();
        Guid? lineId = null;
        if (Request.Query.ContainsKey("lineId"))
        {
            if (!LineRequestReader.Id(Request.Query["lineId"].FirstOrDefault(), out var parsed)) return Invalid("lineId");
            lineId = parsed;
        }
        return LineProblems.Render(this, await service.ProductChoicesAsync(Request.Query["q"].FirstOrDefault(), Request.Query["page"].FirstOrDefault(), lineId, ct), Ok);
    }
    /// <summary>Reads currently eligible lines for one product observation.</summary>
    [HttpGet("eligible")]
    public async Task<ActionResult> Eligible(CancellationToken ct)
    {
        if (!LineRequestReader.Query(Request, "productId", "q", "page")) return Invalid();
        if (!LineRequestReader.Id(Request.Query["productId"].FirstOrDefault(), out var productId)) return Invalid("productId");
        return LineProblems.Render(this, await service.EligibleAsync(productId, Request.Query["q"].FirstOrDefault(), Request.Query["page"].FirstOrDefault(), ct), Ok);
    }
    /// <summary>Creates a line and its explicitly confirmed product timing rows atomically.</summary>
    [HttpPost]
    public async Task<ActionResult> Create(CancellationToken ct)
    {
        if (!LineRequestReader.Query(Request)) return Invalid();
        var body = await LineRequestReader.Body(Request, ct);
        if (body.Problem is { } failure) return LineProblems.Problem(HttpContext, failure);
        var json = body.Value;
        if (!LineRequestReader.Shape(json, "code", "name", "workingHoursPerDay", "products") ||
            !LineRequestReader.Text(json, "code", out var code) || !LineRequestReader.Text(json, "name", out var name) ||
            !LineRequestReader.Text(json, "workingHoursPerDay", out var hours) || !LineRequestReader.Inputs(json, "products", true, out var products)) return Invalid();
        return LineProblems.Render(this, await service.CreateAsync(new(code, name, hours, products), ct),
            line => CreatedAtAction(nameof(Get), new { id = line.Id }, line));
    }
    /// <summary>Edits a versioned aggregate without changing omitted associations.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, CancellationToken ct)
    {
        if (!LineRequestReader.Id(id, out var lineId)) return Invalid("id");
        if (!LineRequestReader.Query(Request)) return Invalid();
        var body = await LineRequestReader.Body(Request, ct);
        if (body.Problem is { } failure) return LineProblems.Problem(HttpContext, failure);
        var json = body.Value;
        if (!LineRequestReader.Shape(json, "name", "workingHoursPerDay", "version", "productChanges") ||
            !LineRequestReader.Text(json, "name", out var name) || !LineRequestReader.Text(json, "workingHoursPerDay", out var hours) ||
            !LineRequestReader.Text(json, "version", out var version) || !LineRequestReader.Inputs(json, "productChanges", false, out var changes)) return Invalid();
        return LineProblems.Render(this, await service.UpdateAsync(lineId, new(name, hours, version, changes), ct), Ok);
    }
    /// <summary>Retires a current-version line while preserving its associations and history.</summary>
    [HttpPost("{id}/retire")]
    public async Task<ActionResult> Retire(string id, CancellationToken ct)
    {
        if (!LineRequestReader.Id(id, out var lineId)) return Invalid("id");
        if (!LineRequestReader.Query(Request)) return Invalid();
        var body = await LineRequestReader.Body(Request, ct);
        if (body.Problem is { } failure) return LineProblems.Problem(HttpContext, failure);
        if (!LineRequestReader.Shape(body.Value, "version") || !LineRequestReader.Text(body.Value, "version", out var version)) return Invalid();
        return LineProblems.Render(this, await service.RetireAsync(lineId, version, ct), Ok);
    }
    private ActionResult Invalid(string field = "body") => LineProblems.Problem(HttpContext,
        new(400, "VALIDATION", new Dictionary<string, string[]> { [field] = ["VALIDATION"] }));
}
