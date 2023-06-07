using JetBrains.Annotations;

namespace Rx.IB2.Models.Messages;

public struct AccountListMessage {
    [UsedImplicitly]
    public required IEnumerable<string> Accounts { get; init; }
}