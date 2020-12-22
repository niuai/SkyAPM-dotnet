using System.Threading;

namespace Cmb.SkyApm.Tracing
{
    public class CmbCarrierAccessor : ICmbCarrierAccessor
    {
        private readonly AsyncLocal<CmbCarrier> _carrierContext = new AsyncLocal<CmbCarrier>();

        public ICmbCarrier Context
        {
            get => _carrierContext.Value;
            set => _carrierContext.Value = (CmbCarrier)value;
        }
    }
}
