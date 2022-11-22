using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace World3DMonoGame.Utils;

public static class Util
{
    public static byte[] StructureToByteArray(this RenderTarget2D[] array)
    {
        int structSize = Marshal.SizeOf(typeof(RenderTarget2D));
        int size = array.Length * structSize;
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        for (int i = 0; i < array.Length; i++ )
            Marshal.StructureToPtr(array[i], ptr+i*structSize, true);//error
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
}