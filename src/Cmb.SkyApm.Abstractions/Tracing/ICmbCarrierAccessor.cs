namespace Cmb.SkyApm.Tracing
{
    public interface ICmbCarrierAccessor
    {
        ICmbCarrier Context { get; set; }
    }
}
