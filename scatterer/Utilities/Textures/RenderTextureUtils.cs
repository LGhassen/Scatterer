using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public struct FlipFlop<T>
    {
        public FlipFlop(T flip, T flop)
        {
            this.flip = flip;
            this.flop = flop;
        }

        public T this[bool useFlip]
        {
            get => useFlip ? flip : flop;
            set
            {
                if (useFlip) flip = value;
                else flop = value;
            }
        }

        T flip;
        T flop;
    }

    public static class RenderTextureUtils
	{
        public static FlipFlop<RenderTexture> CreateFlipFlopRT(int width, int height, RenderTextureFormat format, FilterMode filterMode, TextureDimension dimension = TextureDimension.Tex2D, int depth = 0, bool randomReadWrite = false)
        {
            return new FlipFlop<RenderTexture>(
                CreateRenderTexture(width, height, format, false, filterMode, dimension, depth, randomReadWrite),
                CreateRenderTexture(width, height, format, false, filterMode, dimension, depth, randomReadWrite));
        }

        public static void ReleaseFlipFlopRT(ref FlipFlop<RenderTexture> flipFlop)
        {
            RenderTexture rt;

            rt = flipFlop[false];
            if (rt != null) rt.Release();
            rt = flipFlop[true];
            if (rt != null) rt.Release();

            flipFlop = new FlipFlop<RenderTexture>(null, null);
        }

        public static void ResizeRT(RenderTexture rt, int newWidth, int newHeight)
        {
            if (rt != null)
            {
                rt.Release();
                rt.width = newWidth;
                rt.height = newHeight;
                rt.Create();
            }
        }

        public static void ResizeFlipFlopRT(ref FlipFlop<RenderTexture> flipFlop, int newWidth, int newHeight, int newDepth = 0)
        {
            RenderTextureUtils.ResizeRT(flipFlop[false], newWidth, newHeight);
            RenderTextureUtils.ResizeRT(flipFlop[true], newWidth, newHeight);
        }

        public static RenderTexture CreateRenderTexture(int width, int height, RenderTextureFormat format, bool useMips, FilterMode filterMode, TextureDimension dimension = TextureDimension.Tex2D, int depth = 0, bool randomReadWrite = false)
        {
            var rt = new RenderTexture(width, height, 0, format);
            rt.anisoLevel = 1;
            rt.antiAliasing = 1;
            rt.dimension = dimension;
            rt.volumeDepth = depth;
            rt.useMipMap = useMips;
            rt.autoGenerateMips = useMips;
            rt.filterMode = filterMode;
            rt.enableRandomWrite = randomReadWrite;
            rt.Create();

            return rt;
        }
    }
}
