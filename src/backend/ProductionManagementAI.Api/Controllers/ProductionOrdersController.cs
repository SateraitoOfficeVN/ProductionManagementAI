using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Api.ProductionOrders;
using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Api.Controllers;

/// <summary>001_DD-API §2–§4. Business checks live in <see cref="ProductionOrderService"/> (001_DD-FN).</summary>
[ApiController]
[Route("api/production-orders")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public class ProductionOrdersController(ProductionOrderService service) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")] // DEC-020: JSON-only bodies; anything else is 415
    public async Task<ActionResult<ProductionOrderResponse>> Create(
        CreateProductionOrderRequest request, CancellationToken cancellationToken) =>
        ProductionOrderProblems.ToActionResult(this, await service.CreateAsync(request, cancellationToken),
            order => CreatedAtAction(nameof(Get), new { id = order.Id }, order));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductionOrderResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        ProductionOrderProblems.ToActionResult(this, await service.GetAsync(id, cancellationToken), Ok);

    /// <summary>002_DD-API §1. No request body, so no [Consumes]; the query string is validated into a typed query.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductionOrderListItem>>> List(
        [FromQuery] ProductionOrderListRequest request, CancellationToken cancellationToken)
    {
        var query = ProductionOrderListQuery.TryCreate(request);
        if (query is not Result<ProductionOrderListQuery>.Ok valid)
        {
            // Only Invalid is reachable here; ToActionResult renders it as the 400 Problem Details body.
            return ProductionOrderProblems.ToActionResult(this, query, _ => Ok());
        }

        return ProductionOrderProblems.ToActionResult(
            this, await service.ListAsync(valid.Value, cancellationToken), Ok);
    }

    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<ProductionOrderResponse>> Update(
        Guid id, UpdateProductionOrderRequest request, CancellationToken cancellationToken) =>
        ProductionOrderProblems.ToActionResult(this, await service.UpdateAsync(id, request, cancellationToken), Ok);
}

/// <summary>001_DD-API §1.</summary>
[ApiController]
[Route("api/products")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public class ProductsController(ProductionOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await service.ListProductsAsync(cancellationToken));
}

public static class AuthorizationPolicies
{
    /// <summary>Admin or Operator may create and edit production orders (DEC-001).</summary>
    public const string ProductionOrderEditor = "ProductionOrderEditor";
}
