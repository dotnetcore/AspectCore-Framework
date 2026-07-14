using Xunit;

namespace AspectCore.E2E.Tests;

/// <summary>
/// Collection definition that ensures all test classes sharing the static
/// <see cref="Fixtures.InterceptorLog"/> run sequentially (no parallelization),
/// preventing cross-test contamination of the shared log.
/// </summary>
[CollectionDefinition("InterceptorLog")]
public sealed class InterceptorLogCollection
{
}
