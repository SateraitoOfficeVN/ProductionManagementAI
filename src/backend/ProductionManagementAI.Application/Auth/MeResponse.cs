namespace ProductionManagementAI.Application.Auth;

public record MeResponse(Guid Id, string UserName, string DisplayName, IReadOnlyList<string> Roles);
