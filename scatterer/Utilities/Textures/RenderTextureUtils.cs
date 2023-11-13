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

        public static void ResizeFlipFlopRT(ref FlipFlop<RenderTexture> flipFlop, int newWidth, int newHeight, bool copyContents, int newDepth = 0)
        {
            bool is3d = flipFlop[false].volumeDepth > 0;

            // Create new RenderTextures with the specified dimensions
            var newFlip = CreateRenderTexture(newWidth, newHeight, flipFlop[false].format, flipFlop[false].useMipMap, flipFlop[false].filterMode, flipFlop[false].dimension, is3d && newDepth > 0 ? newDepth : flipFlop[false].volumeDepth, flipFlop[false].enableRandomWrite);
            var newFlop = CreateRenderTexture(newWidth, newHeight, flipFlop[true].format,  flipFlop[true].useMipMap,  flipFlop[true].filterMode,  flipFlop[true].dimension,  is3d && newDepth > 0 ? newDepth : flipFlop[true].volumeDepth,  flipFlop[true].enableRandomWrite);

            if (copyContents)
            {
                if (is3d)
                {
                    // TODO
                }
                else
                {
                    // Copy the contents from the old RenderTextures to the new ones
                    Graphics.Blit(flipFlop[false], newFlip);
                    Graphics.Blit(flipFlop[true], newFlop);
                }
            }

            // Release the old RenderTextures
            ReleaseFlipFlopRT(ref flipFlop);

            // Update the FlipFlop with the new RenderTextures
            flipFlop = new FlipFlop<RenderTexture>(newFlip, newFlop);
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
