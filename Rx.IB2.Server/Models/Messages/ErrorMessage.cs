using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct ErrorMessage {
    [UsedImplicitly]
    public required string Message { get; init; }
}