using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace colordiff;

internal class Program
{
    static float[] frgb = new float[6];
    static float[] fok = new float[6];

    static void Main(string[] args)
    {
        if(args.Length < 6 || args.Length > 7)
        {
            Console.WriteLine("Usage:\ncolordiff R1 G1 B1 R2 G2 B2 [loops]\nSpecify the number of loops to compare cube-root vs Halley-solver performance.");
            return;
        }

        var ichannel = new int[6];
        for(int i = 0; i < 6; i++)
        {
            ichannel[i] = int.MinValue;
            if (!int.TryParse(args[i], out ichannel[i]) || ichannel[i] < 0 || ichannel[i] > 255)
            {
                Console.WriteLine($"Element {i + 1} invalid, 0-255 only.");
                return;
            }
            frgb[i] = ichannel[i] / 255.0f;
        }

        int loops = 0;
        if(args.Length == 7) 
        {
            if(!int.TryParse(args[6], out loops) || loops < 1)
            {
                Console.WriteLine("Invalid loop count");
                return;
            }
        }

        if(loops == 0)
        {
            Console.WriteLine($"Distance: {Solve(false)}");
            Console.WriteLine($"  color1 Lab ({fok[0]:0.0000000000}, {fok[1]:0.0000000000}, {fok[2]:0.0000000000})");
            Console.WriteLine($"  color1 RGB ({frgb[0]:0.0000000000}, {frgb[1]:0.0000000000}, {frgb[2]:0.0000000000})");
            Console.WriteLine($"  color2 Lab ({fok[3]:0.0000000000}, {fok[4]:0.0000000000}, {fok[5]:0.0000000000})");
            Console.WriteLine($"  color2 RGB ({frgb[3]:0.0000000000}, {frgb[4]:0.0000000000}, {frgb[5]:0.0000000000})");
            return;
        }

        Stopwatch timing = new();
        
        timing.Start();
        for(int i = 0; i < loops; i++) Solve(false);
        timing.Stop();
        long real_ms = timing.ElapsedMilliseconds;

        timing.Restart();
        for (int i = 0; i < loops; i++) Solve(true);
        timing.Stop();
        long halley_ms = timing.ElapsedMilliseconds;

        Console.WriteLine($"Elapsed for real cube-root  {real_ms}ms");
        Console.WriteLine($"Elapsed for Halley iterator {halley_ms}ms");
    }

    private static float Solve(bool useHalley)
    {
        rgb2oklab(0, useHalley); // color 1 RGB
        rgb2oklab(3, useHalley); // color 2 RGB
        float squaredist = 
              Math.Abs(fok[0] - fok[3]) * Math.Abs(fok[0] - fok[3]) 
            + Math.Abs(fok[1] - fok[4]) * Math.Abs(fok[1] - fok[4]) 
            + Math.Abs(fok[2] - fok[5]) * Math.Abs(fok[2] - fok[5]);
        float dist = (float)Math.Sqrt(squaredist);
        return dist;
    }

    private static void rgb2oklab(int offset, bool useHalley)
    {
        var r = frgb[offset];
        var g = frgb[offset + 1];
        var b = frgb[offset + 2];

        float ol = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
        float oa = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
        float ob = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

        float lr = (!useHalley) ? (float)Math.Cbrt(ol) : halley_cube_root(ol);
        float ar = (!useHalley) ? (float)Math.Cbrt(oa) : halley_cube_root(oa);
        float br = (!useHalley) ? (float)Math.Cbrt(ob) : halley_cube_root(ob);

        fok[offset] =     0.2104542553f * lr + 0.7936177850f * ar - 0.0040720468f * br;
        fok[offset + 1] = 1.9779984951f * lr - 2.4285922050f * ar + 0.4505937099f * br;
        fok[offset + 2] = 0.0259040371f * lr + 0.7827717662f * ar - 0.8086757660f * br;
    }

    private static float halley_cube_root(float x)
    {
        float y = Math.Sign(x) * (float)((uint)(Math.Abs(x)) / 3u + 0x2a514067u);
        for (int i = 0; i < 2; ++i)
        {
            float y3 = y * y * y;
            y *= (y3 + 2.0f * x) / (2.0f * y3 + x);

            // Newton's method looks like this, but requires about 2X iterations
            // for the same error range, and ends up being approximately equivalent
            // to the .NET Math.Cbrt call.
            // y = (2.0f * y + x / (y * y)) * 0.333333333f;
        }
        return y;
    }
}
