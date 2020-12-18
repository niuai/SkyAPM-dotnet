using SkyApm.Tracing;

namespace Cmb.SkyApm.Tracing
{
    public interface ICmbCarrier : ICarrier
    {
        string BusinessId { get; set; }
    }
}
