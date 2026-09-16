using ProductionManagementAI.Application.Auth;
using Xunit;

namespace ProductionManagementAI.Application.Tests.Auth;

public class MeResponseTests
{
    [Fact]
    public void ExposesConstructorArguments_AsGiven()
    {
        var id = Guid.NewGuid();
        var roles = new List<string> { "Admin", "Operator" };

        var response = new MeResponse(id, "alice", "Alice A.", roles);

        Assert.Equal(id, response.Id);
        Assert.Equal("alice", response.UserName);
        Assert.Equal("Alice A.", response.DisplayName);
        Assert.Equal(roles, response.Roles);
    }

    [Fact]
    public void Roles_CanBeEmpty_ForAUserWithNoRoleAssignments()
    {
        var response = new MeResponse(Guid.NewGuid(), "alice", "Alice A.", []);

        Assert.Empty(response.Roles);
    }
}
