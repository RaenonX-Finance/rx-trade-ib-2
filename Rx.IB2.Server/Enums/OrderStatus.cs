namespace Rx.IB2.Enums; 

// ReSharper disable UnusedMember.Global
public enum OrderStatus {
    ApiPending,
    PendingSubmit,
    PendingCancel,
    PreSubmitted,
    ApiCancelled,
    Cancelled,
    Filled,
    Inactive
}
// ReSharper restore UnusedMember.Global