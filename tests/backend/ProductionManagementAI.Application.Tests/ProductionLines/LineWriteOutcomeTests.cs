using ProductionManagementAI.Application.ProductionLines;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.ProductionLines;

/// <summary>Verifies that ambiguous commits and failed cleanup never become replayable busy outcomes.</summary>
public sealed class LineWriteOutcomeTests
{
    [Theory]
    [InlineData(false, true, 503)]
    [InlineData(false, false, 500)]
    [InlineData(true, true, 500)]
    [InlineData(true, false, 500)]
    public async Task OnlyPreCommitProviderFailureWithConfirmedRollbackIsBusy(bool commit, bool rollback, int expected)
    {
        var session = new Session(commit, rollback);
        var service = new ProductionLineService(new Repository(session), TimeProvider.System);
        var result = await service.CreateAsync(new("L", "Name", "8", []), CancellationToken.None);
        Assert.Equal(expected, result.Problem?.Status);
        Assert.Equal(expected == 503 ? "LINE_BUSY" : "UNEXPECTED", result.Problem?.Code);
        Assert.Equal(1, session.SaveCalls);
        Assert.Equal(commit ? 1 : 0, session.CommitCalls);
        Assert.Equal(1, session.RollbackCalls);
        Assert.False(session.CleanupWasAlreadyCancelled);
    }

    [Fact]
    public async Task CancelledRequestUsesFreshCleanupAndDoesNotReplay()
    {
        using var cancellation = new CancellationTokenSource();
        var session = new Session(false, true, cancellation);
        var service = new ProductionLineService(new Repository(session), TimeProvider.System);
        var result = await service.CreateAsync(new("L", "Name", "8", []), cancellation.Token);
        Assert.Equal(500, result.Problem?.Status);
        Assert.Equal(1, session.SaveCalls); Assert.Equal(0, session.CommitCalls); Assert.Equal(1, session.RollbackCalls);
        Assert.False(session.CleanupWasAlreadyCancelled);
    }

    [Fact]
    public async Task PostCommitReadFailureIsUnknownAndSharesTheOriginalDeadlineToken()
    {
        var session = new Session(true, true, commitSuccess: true);
        var repository = new Repository(session);
        var result = await new ProductionLineService(repository, TimeProvider.System).CreateAsync(new("L", "Name", "8", []), CancellationToken.None);
        Assert.Equal(500, result.Problem?.Status); Assert.Equal("UNEXPECTED", result.Problem?.Code);
        Assert.Equal(repository.BeginToken, repository.ReadToken);
        Assert.Equal(1, session.SaveCalls); Assert.Equal(1, session.CommitCalls);
    }

    [Fact]
    public async Task UseCaseCancelsAtFifteenSecondsWithoutRestartingItsBudget()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var result = await new ProductionLineService(new DeadlineRepository(), TimeProvider.System).ListAsync(new(null, null, null), CancellationToken.None);
        Assert.Equal(500, result.Problem?.Status);
        Assert.InRange(watch.Elapsed.TotalSeconds, 14, 18);
    }
    private sealed class DeadlineRepository : IProductionLineRepository
    {
        public async Task<LinePage<LineSummary>> ListAsync(LineQuery query, CancellationToken ct) { await Task.Delay(Timeout.InfiniteTimeSpan, ct); throw new InvalidOperationException(); }
        public Task<ILineWriteSession> BeginWriteAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<LineDetail?> GetAsync(Guid id, int page, CancellationToken ct) => throw new NotSupportedException();
        public Task<LinePage<LineProductChoice>?> ProductChoicesAsync(LineQuery query, CancellationToken ct) => throw new NotSupportedException();
        public Task<EligibleLinePage?> EligibleAsync(LineQuery query, CancellationToken ct) => throw new NotSupportedException();
    }
    private sealed class Repository(Session session) : IProductionLineRepository
    {
        internal CancellationToken BeginToken; internal CancellationToken ReadToken;
        public Task<ILineWriteSession> BeginWriteAsync(CancellationToken ct) { BeginToken = ct; return Task.FromResult<ILineWriteSession>(session); }
        public Task<LinePage<LineSummary>> ListAsync(LineQuery query, CancellationToken ct) => throw new NotSupportedException();
        public Task<LineDetail?> GetAsync(Guid id, int page, CancellationToken ct) { ReadToken = ct; throw new LineTransientException(new InvalidOperationException()); }
        public Task<LinePage<LineProductChoice>?> ProductChoicesAsync(LineQuery query, CancellationToken ct) => throw new NotSupportedException();
        public Task<EligibleLinePage?> EligibleAsync(LineQuery query, CancellationToken ct) => throw new NotSupportedException();
    }
    private sealed class Session(bool failCommit, bool confirmRollback, CancellationTokenSource? cancelRequest = null, bool commitSuccess = false) : ILineWriteSession
    {
        internal int SaveCalls; internal int CommitCalls; internal int RollbackCalls; internal bool CleanupWasAlreadyCancelled;
        public Task<IReadOnlyDictionary<Guid, Product>> LockProductsAsync(IEnumerable<Guid> ids, CancellationToken ct) =>
            Task.FromResult<IReadOnlyDictionary<Guid, Product>>(new Dictionary<Guid, Product>());
        public Task<ProductionLine?> LockLineAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<Guid, ProductionLineProduct>> LockPairsAsync(Guid id, IEnumerable<Guid> ids, CancellationToken ct) => throw new NotSupportedException();
        public void Add(ProductionLine line) { }
        public void Add(ProductionLineProduct pair) { }
        public void Touch(ProductionLine line, DateTimeOffset now) { }
        public Task SaveAsync(CancellationToken ct)
        {
            SaveCalls++;
            if (cancelRequest is not null) { cancelRequest.Cancel(); return Task.FromCanceled(ct); }
            return failCommit ? Task.CompletedTask : Task.FromException(new LineTransientException(new InvalidOperationException()));
        }
        public Task CommitAsync(CancellationToken ct) { CommitCalls++; return commitSuccess ? Task.CompletedTask : Task.FromException(new LineTransientException(new InvalidOperationException())); }
        public Task<bool> RollbackAsync(CancellationToken ct) { RollbackCalls++; CleanupWasAlreadyCancelled = ct.IsCancellationRequested; return Task.FromResult(confirmRollback); }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
