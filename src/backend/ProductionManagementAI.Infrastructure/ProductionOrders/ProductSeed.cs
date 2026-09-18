using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

/// <summary>
/// The 30 illustrative demo products from DB-002 (DEC-019). Fixed IDs and timestamp keep the migration's
/// seed data reproducible.
/// </summary>
internal static class ProductSeed
{
    private static readonly DateTimeOffset SeededAt = new(2026, 9, 18, 0, 0, 0, TimeSpan.Zero);

    public static readonly Product[] All =
    [
        P("0197e4a0-0000-7000-8000-000000001001", "P-1001", "Steel bracket"),
        P("0197e4a0-0000-7000-8000-000000001002", "P-1002", "Aluminium housing"),
        P("0197e4a0-0000-7000-8000-000000001003", "P-1003", "Control panel assembly"),
        P("0197e4a0-0000-7000-8000-000000001004", "P-1004", "Drive shaft"),
        P("0197e4a0-0000-7000-8000-000000001005", "P-1005", "Hydraulic valve"),
        P("0197e4a0-0000-7000-8000-000000001006", "P-1006", "Gearbox assembly"),
        P("0197e4a0-0000-7000-8000-000000001007", "P-1007", "Bearing housing"),
        P("0197e4a0-0000-7000-8000-000000001008", "P-1008", "Pump impeller"),
        P("0197e4a0-0000-7000-8000-000000001009", "P-1009", "Motor mount plate"),
        P("0197e4a0-0000-7000-8000-000000001010", "P-1010", "Conveyor roller"),
        P("0197e4a0-0000-7000-8000-000000001011", "P-1011", "Spur gear 40T"),
        P("0197e4a0-0000-7000-8000-000000001012", "P-1012", "Coupling flange"),
        P("0197e4a0-0000-7000-8000-000000001013", "P-1013", "Pneumatic cylinder"),
        P("0197e4a0-0000-7000-8000-000000001014", "P-1014", "Sensor bracket"),
        P("0197e4a0-0000-7000-8000-000000001015", "P-1015", "Cable harness A"),
        P("0197e4a0-0000-7000-8000-000000001016", "P-1016", "Cable harness B"),
        P("0197e4a0-0000-7000-8000-000000001017", "P-1017", "Terminal block unit"),
        P("0197e4a0-0000-7000-8000-000000001018", "P-1018", "Relay module"),
        P("0197e4a0-0000-7000-8000-000000001019", "P-1019", "Power supply unit"),
        P("0197e4a0-0000-7000-8000-000000001020", "P-1020", "PLC enclosure"),
        P("0197e4a0-0000-7000-8000-000000001021", "P-1021", "Heat sink"),
        P("0197e4a0-0000-7000-8000-000000001022", "P-1022", "Cooling fan assembly"),
        P("0197e4a0-0000-7000-8000-000000001023", "P-1023", "Filter housing"),
        P("0197e4a0-0000-7000-8000-000000001024", "P-1024", "Valve body"),
        P("0197e4a0-0000-7000-8000-000000001025", "P-1025", "Piston rod"),
        P("0197e4a0-0000-7000-8000-000000001026", "P-1026", "Spring assembly"),
        P("0197e4a0-0000-7000-8000-000000001027", "P-1027", "Welded frame"),
        P("0197e4a0-0000-7000-8000-000000001028", "P-1028", "Guard panel"),
        P("0197e4a0-0000-7000-8000-000000001029", "P-1029", "Hinge set"),
        P("0197e4a0-0000-7000-8000-000000001030", "P-1030", "Fastener kit"),
    ];

    private static Product P(string id, string sku, string name) =>
        new() { Id = Guid.Parse(id), Sku = sku, Name = name, CreatedAtUtc = SeededAt };
}
