using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Scatterer
{
    internal static class BurstUtil
    {
        public static bool IsBurstCompiled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                bool burst = true;
                IsBurstCompiledInner(ref burst);
                return burst;
                
            }
        }
        
        [BurstDiscard]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void IsBurstCompiledInner(ref bool burst)
        {
            burst = false;
        }
    }
}
