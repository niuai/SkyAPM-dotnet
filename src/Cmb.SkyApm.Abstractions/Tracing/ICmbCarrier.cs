using SkyApm.Tracing;

namespace Cmb.SkyApm.Tracing
{
    public interface ICmbCarrier : ICarrier
    {
        string BusinessId { get; set; }

        string CmbParentSpanId { get; set; }

        string CmbSpanId { get; set; }

        string CmbTimeStamp { get; set; }

        string CmbSampled { get; set; }

        string CmbDebug { get; set; }
    }
}
