using Rx.IB2.Enums;

namespace Rx.IB2.Extensions; 

public static class NumberExtensions {
    private const double MaxFloatErrorAllowance = 1E-5;

    public static PxTick ToPxTick(this int pxTickInt) {
        return (PxTick)pxTickInt;
    }

    public static double MaxValueAsZero(this double value) {
        return Math.Abs(value - double.MaxValue) < MaxFloatErrorAllowance ? 0 : value;
    }

    public static decimal MaxValueAsZero(this decimal value) {
        return Math.Abs(value - decimal.MaxValue) < (decimal)MaxFloatErrorAllowance ? 0 : value;
    }

    public static decimal? MaxValueAsNull(this decimal value) {
        return Math.Abs(value - decimal.MaxValue) < (decimal)MaxFloatErrorAllowance ? null : value;
    }
}