using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Api.Products;
using ProductionManagementAI.Application.Products;
using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Api.Controllers;

/// <summary>WI-006 Product master catalog, versioned edits, and retirement.</summary>
[ApiController]
[Route("api/product-master")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public sealed class ProductMasterController(ProductMasterService service) : ControllerBase
{
    /// <summary>Lists products with bounded SKU/name search and state filtering.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductMasterItem>>> List(
        [FromQuery] ProductMasterListRequest request, CancellationToken cancellationToken) =>
        ProductMasterProblems.ToActionResult(this, await service.ListAsync(request, cancellationToken), Ok);

    /// <summary>Gets an active or retired product and its current version.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductMasterItem>> Get(Guid id, CancellationToken cancellationToken) =>
        ProductMasterProblems.ToActionResult(this, await service.GetAsync(id, cancellationToken), Ok);

    /// <summary>Creates an active product.</summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<ProductMasterItem>> Create(
        CreateProductRequest request, CancellationToken cancellationToken) =>
        ProductMasterProblems.ToActionResult(this, await service.CreateAsync(request, cancellationToken),
            product => CreatedAtAction(nameof(Get), new { id = product.Id }, product));

    /// <summary>Updates a product with an expected row version.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<ActionResult<ProductMasterItem>> Update(
        Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        ProductMasterProblems.ToActionResult(this, await service.UpdateAsync(id, request, cancellationToken), Ok);

    /// <summary>Retires a product without removing its historical orders.</summary>
    [HttpPost("{id:guid}/retire")]
    [Consumes("application/json")]
    public async Task<ActionResult<ProductMasterItem>> Retire(
        Guid id, RetireProductRequest request, CancellationToken cancellationToken) =>
        ProductMasterProblems.ToActionResult(this, await service.RetireAsync(id, request, cancellationToken), Ok);
}
