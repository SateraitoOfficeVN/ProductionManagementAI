using ProductionManagementAI.Application.Auth;
using Xunit;

namespace ProductionManagementAI.Application.Tests.Auth;

public class LoginRequestTests
{
    [Fact]
    public void Records_WithSameValues_AreEqual()
    {
        var a = new LoginRequest("alice", "secret");
        var b = new LoginRequest("alice", "secret");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Records_WithDifferentValues_AreNotEqual()
    {
        var a = new LoginRequest("alice", "secret");
        var b = new LoginRequest("bob", "secret");

        Assert.NotEqual(a, b);
    }
}
