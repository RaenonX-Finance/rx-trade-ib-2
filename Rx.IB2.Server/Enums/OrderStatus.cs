namespace Rx.IB2.Enums; 

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