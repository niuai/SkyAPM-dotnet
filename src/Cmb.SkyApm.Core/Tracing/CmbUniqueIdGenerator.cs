using SkyApm.Tracing;
using System;

namespace Cmb.SkyApm.Tracing
{
    public class CmbUniqueIdGenerator : IUniqueIdGenerator
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToLower();
        }
    }
}
