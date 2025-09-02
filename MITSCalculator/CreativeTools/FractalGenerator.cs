using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MITSCalculator.CreativeTools;

public class FractalGenerator
{
    // Mandelbrot Set
    public WriteableBitmap GenerateMandelbrot(int width, int height, double centerX = -0.5, double centerY = 0, 
        double zoom = 1, int maxIterations = 100)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Avalonia.Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var buffer = (uint*)frameBuffer.Address;
            var stride = frameBuffer.RowBytes / 4;

            var scale = 4.0 / zoom;
            var xScale = scale / width;
            var yScale = scale / height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var real = centerX + (x - width / 2.0) * xScale;
                    var imag = centerY + (y - height / 2.0) * yScale;
                    
                    var iterations = CalculateMandelbrotIterations(real, imag, maxIterations);
                    var color = GetFractalColor(iterations, maxIterations);
                    
                    buffer[y * stride + x] = color;
                }
            }
        }

        return bitmap;
    }

    private int CalculateMandelbrotIterations(double real, double imag, int maxIterations)
    {
        var c = new Complex(real, imag);
        var z = Complex.Zero;
        int iterations = 0;

        while (z.Magnitude <= 2 && iterations < maxIterations)
        {
            z = z * z + c;
            iterations++;
        }

        return iterations;
    }

    // Julia Set
    public WriteableBitmap GenerateJulia(int width, int height, double juliaReal = -0.7, double juliaImag = 0.27015,
        double centerX = 0, double centerY = 0, double zoom = 1, int maxIterations = 100)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Avalonia.Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var buffer = (uint*)frameBuffer.Address;
            var stride = frameBuffer.RowBytes / 4;

            var scale = 4.0 / zoom;
            var xScale = scale / width;
            var yScale = scale / height;
            var juliaConstant = new Complex(juliaReal, juliaImag);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var real = centerX + (x - width / 2.0) * xScale;
                    var imag = centerY + (y - height / 2.0) * yScale;
                    
                    var iterations = CalculateJuliaIterations(real, imag, juliaConstant, maxIterations);
                    var color = GetFractalColor(iterations, maxIterations);
                    
                    buffer[y * stride + x] = color;
                }
            }
        }

        return bitmap;
    }

    private int CalculateJuliaIterations(double real, double imag, Complex juliaConstant, int maxIterations)
    {
        var z = new Complex(real, imag);
        int iterations = 0;

        while (z.Magnitude <= 2 && iterations < maxIterations)
        {
            z = z * z + juliaConstant;
            iterations++;
        }

        return iterations;
    }

    // Burning Ship
    public WriteableBitmap GenerateBurningShip(int width, int height, double centerX = -0.5, double centerY = -0.6,
        double zoom = 1, int maxIterations = 100)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Avalonia.Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var buffer = (uint*)frameBuffer.Address;
            var stride = frameBuffer.RowBytes / 4;

            var scale = 4.0 / zoom;
            var xScale = scale / width;
            var yScale = scale / height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var real = centerX + (x - width / 2.0) * xScale;
                    var imag = centerY + (y - height / 2.0) * yScale;
                    
                    var iterations = CalculateBurningShipIterations(real, imag, maxIterations);
                    var color = GetFractalColor(iterations, maxIterations);
                    
                    buffer[y * stride + x] = color;
                }
            }
        }

        return bitmap;
    }

    private int CalculateBurningShipIterations(double real, double imag, int maxIterations)
    {
        var c = new Complex(real, imag);
        var z = Complex.Zero;
        int iterations = 0;

        while (z.Magnitude <= 2 && iterations < maxIterations)
        {
            // Burning Ship: z = (|Re(z)| + i|Im(z)|)² + c
            z = new Complex(Math.Abs(z.Real), Math.Abs(z.Imaginary));
            z = z * z + c;
            iterations++;
        }

        return iterations;
    }

    // Newton Fractal
    public WriteableBitmap GenerateNewtonFractal(int width, int height, double centerX = 0, double centerY = 0,
        double zoom = 1, int maxIterations = 50)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Avalonia.Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var buffer = (uint*)frameBuffer.Address;
            var stride = frameBuffer.RowBytes / 4;

            // Roots of z³ - 1 = 0
            var roots = new Complex[]
            {
                new Complex(1, 0),
                new Complex(-0.5, Math.Sqrt(3) / 2),
                new Complex(-0.5, -Math.Sqrt(3) / 2)
            };

            var scale = 4.0 / zoom;
            var xScale = scale / width;
            var yScale = scale / height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var real = centerX + (x - width / 2.0) * xScale;
                    var imag = centerY + (y - height / 2.0) * yScale;
                    
                    var rootIndex = CalculateNewtonIterations(real, imag, roots, maxIterations);
                    var color = GetNewtonColor(rootIndex, maxIterations);
                    
                    buffer[y * stride + x] = color;
                }
            }
        }

        return bitmap;
    }

    private int CalculateNewtonIterations(double real, double imag, Complex[] roots, int maxIterations)
    {
        var z = new Complex(real, imag);
        const double tolerance = 1e-6;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Newton's method for z³ - 1 = 0
            // z_new = z - (z³ - 1) / (3z²)
            var z3 = z * z * z;
            var derivative = 3 * z * z;
            
            if (derivative.Magnitude < tolerance) break;
            
            z = z - (z3 - 1) / derivative;

            // Check convergence to any root
            for (int i = 0; i < roots.Length; i++)
            {
                if ((z - roots[i]).Magnitude < tolerance)
                {
                    return i; // Return which root it converged to
                }
            }
        }

        return -1; // Didn't converge
    }

    // Color generation
    private uint GetFractalColor(int iterations, int maxIterations)
    {
        if (iterations == maxIterations)
            return 0xFF000000; // Black for points in the set

        // Create a smooth color gradient
        var hue = (double)iterations / maxIterations * 360;
        var saturation = 1.0;
        var value = iterations < maxIterations ? 1.0 : 0.0;

        return HsvToRgb(hue, saturation, value);
    }

    private uint GetNewtonColor(int rootIndex, int maxIterations)
    {
        return rootIndex switch
        {
            0 => 0xFFFF0000, // Red
            1 => 0xFF00FF00, // Green
            2 => 0xFF0000FF, // Blue
            _ => 0xFF000000   // Black (didn't converge)
        };
    }

    private uint HsvToRgb(double hue, double saturation, double value)
    {
        var c = value * saturation;
        var x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        var m = value - c;

        double r, g, b;

        if (hue >= 0 && hue < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (hue >= 60 && hue < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (hue >= 120 && hue < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (hue >= 180 && hue < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (hue >= 240 && hue < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        var red = (byte)((r + m) * 255);
        var green = (byte)((g + m) * 255);
        var blue = (byte)((b + m) * 255);

        return 0xFF000000 | ((uint)red << 16) | ((uint)green << 8) | blue;
    }

    // Dragon Curve (L-System)
    public (Point[] points, string instructions) GenerateDragonCurve(int iterations, double length = 5)
    {
        var rules = new System.Collections.Generic.Dictionary<char, string>
        {
            ['F'] = "F+S",
            ['S'] = "F-S"
        };

        var axiom = "F";
        var current = axiom;

        // Generate L-system
        for (int i = 0; i < iterations; i++)
        {
            var next = "";
            foreach (char c in current)
            {
                if (rules.ContainsKey(c))
                    next += rules[c];
                else
                    next += c;
            }
            current = next;
        }

        // Convert to points
        var points = new System.Collections.Generic.List<Point>();
        var x = 0.0;
        var y = 0.0;
        var angle = 0.0; // degrees
        var angleStep = 90.0;

        points.Add(new Point(x, y));

        foreach (char c in current)
        {
            switch (c)
            {
                case 'F':
                case 'S':
                    x += length * Math.Cos(angle * Math.PI / 180);
                    y += length * Math.Sin(angle * Math.PI / 180);
                    points.Add(new Point(x, y));
                    break;
                case '+':
                    angle += angleStep;
                    break;
                case '-':
                    angle -= angleStep;
                    break;
            }
        }

        return (points.ToArray(), current);
    }

    // Sierpinski Triangle
    public Point[] GenerateSierpinskiTriangle(int iterations, Point p1, Point p2, Point p3)
    {
        var points = new System.Collections.Generic.List<Point>();
        var random = new Random();
        
        // Start at a random point
        var current = new Point(
            (p1.X + p2.X + p3.X) / 3,
            (p1.Y + p2.Y + p3.Y) / 3
        );

        var vertices = new[] { p1, p2, p3 };

        for (int i = 0; i < iterations; i++)
        {
            // Choose a random vertex
            var vertex = vertices[random.Next(3)];
            
            // Move halfway to the chosen vertex
            current = new Point(
                (current.X + vertex.X) / 2,
                (current.Y + vertex.Y) / 2
            );
            
            points.Add(current);
        }

        return points.ToArray();
    }
}