using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MITSCalculator.Core;

public class MathEngine
{
    private readonly Dictionary<string, double> _variables = new();
    private readonly Dictionary<string, double> _constants = new();

    public MathEngine()
    {
        InitializeConstants();
    }

    private void InitializeConstants()
    {
        _constants["π"] = Math.PI;
        _constants["pi"] = Math.PI;
        _constants["e"] = Math.E;
        _constants["φ"] = (1 + Math.Sqrt(5)) / 2; // Golden ratio
        _constants["c"] = 299792458; // Speed of light in m/s
        _constants["h"] = 6.62607015e-34; // Planck constant
        _constants["k"] = 1.380649e-23; // Boltzmann constant
        _constants["g"] = 9.80665; // Standard gravity
    }

    public void SetVariable(string name, double value)
    {
        _variables[name] = value;
    }

    public double GetVariable(string name)
    {
        return _variables.GetValueOrDefault(name, double.NaN);
    }

    public double GetConstant(string name)
    {
        return _constants.GetValueOrDefault(name, double.NaN);
    }

    // Advanced trigonometric functions
    public double SecantDegrees(double degrees) => 1.0 / Math.Cos(degrees * Math.PI / 180);
    public double CosecantDegrees(double degrees) => 1.0 / Math.Sin(degrees * Math.PI / 180);
    public double CotangentDegrees(double degrees) => 1.0 / Math.Tan(degrees * Math.PI / 180);

    // Hyperbolic functions
    public double SinhDegrees(double degrees) => Math.Sinh(degrees * Math.PI / 180);
    public double CoshDegrees(double degrees) => Math.Cosh(degrees * Math.PI / 180);
    public double TanhDegrees(double degrees) => Math.Tanh(degrees * Math.PI / 180);

    // Inverse trigonometric functions
    public double ArcsinDegrees(double value) => Math.Asin(value) * 180 / Math.PI;
    public double ArccosDegrees(double value) => Math.Acos(value) * 180 / Math.PI;
    public double ArctanDegrees(double value) => Math.Atan(value) * 180 / Math.PI;

    // Logarithmic functions
    public double Log(double value, double baseValue) => Math.Log(value, baseValue);
    public double Ln(double value) => Math.Log(value);
    public double Log10(double value) => Math.Log10(value);
    public double Log2(double value) => Math.Log2(value);

    // Root functions
    public double NthRoot(double value, double n) => Math.Pow(value, 1.0 / n);
    public double CubeRoot(double value) => Math.Pow(value, 1.0 / 3.0);

    // Statistical functions
    public double Factorial(double n)
    {
        if (n < 0 || n != Math.Floor(n)) throw new ArgumentException("Factorial requires non-negative integer");
        if (n == 0 || n == 1) return 1;
        double result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    public double Permutation(double n, double r)
    {
        return Factorial(n) / Factorial(n - r);
    }

    public double Combination(double n, double r)
    {
        return Factorial(n) / (Factorial(r) * Factorial(n - r));
    }

    // Complex number operations
    public Complex AddComplex(Complex a, Complex b) => a + b;
    public Complex SubtractComplex(Complex a, Complex b) => a - b;
    public Complex MultiplyComplex(Complex a, Complex b) => a * b;
    public Complex DivideComplex(Complex a, Complex b) => a / b;
    public double ComplexMagnitude(Complex c) => c.Magnitude;
    public double ComplexPhase(Complex c) => c.Phase;

    // Matrix operations
    public Matrix<double> CreateMatrix(double[][] data)
    {
        return DenseMatrix.OfRowArrays(data);
    }

    public Matrix<double> AddMatrices(Matrix<double> a, Matrix<double> b) => a + b;
    public Matrix<double> MultiplyMatrices(Matrix<double> a, Matrix<double> b) => a * b;
    public Matrix<double> InvertMatrix(Matrix<double> matrix) => matrix.Inverse();
    public double MatrixDeterminant(Matrix<double> matrix) => matrix.Determinant();

    // Financial calculations
    public double CompoundInterest(double principal, double rate, double time, int compoundFrequency)
    {
        return principal * Math.Pow(1 + rate / compoundFrequency, compoundFrequency * time);
    }

    public double PresentValue(double futureValue, double rate, double periods)
    {
        return futureValue / Math.Pow(1 + rate, periods);
    }

    public double FutureValue(double presentValue, double rate, double periods)
    {
        return presentValue * Math.Pow(1 + rate, periods);
    }

    public double MonthlyPayment(double principal, double monthlyRate, int months)
    {
        if (monthlyRate == 0) return principal / months;
        return principal * (monthlyRate * Math.Pow(1 + monthlyRate, months)) / 
               (Math.Pow(1 + monthlyRate, months) - 1);
    }

    // Unit conversions
    public double ConvertLength(double value, LengthUnit from, LengthUnit to)
    {
        // Convert to meters first, then to target unit
        double meters = from switch
        {
            LengthUnit.Millimeter => value / 1000,
            LengthUnit.Centimeter => value / 100,
            LengthUnit.Meter => value,
            LengthUnit.Kilometer => value * 1000,
            LengthUnit.Inch => value * 0.0254,
            LengthUnit.Foot => value * 0.3048,
            LengthUnit.Yard => value * 0.9144,
            LengthUnit.Mile => value * 1609.344,
            _ => value
        };

        return to switch
        {
            LengthUnit.Millimeter => meters * 1000,
            LengthUnit.Centimeter => meters * 100,
            LengthUnit.Meter => meters,
            LengthUnit.Kilometer => meters / 1000,
            LengthUnit.Inch => meters / 0.0254,
            LengthUnit.Foot => meters / 0.3048,
            LengthUnit.Yard => meters / 0.9144,
            LengthUnit.Mile => meters / 1609.344,
            _ => meters
        };
    }

    public double ConvertTemperature(double value, TemperatureUnit from, TemperatureUnit to)
    {
        // Convert to Celsius first
        double celsius = from switch
        {
            TemperatureUnit.Celsius => value,
            TemperatureUnit.Fahrenheit => (value - 32) * 5 / 9,
            TemperatureUnit.Kelvin => value - 273.15,
            TemperatureUnit.Rankine => (value - 491.67) * 5 / 9,
            _ => value
        };

        return to switch
        {
            TemperatureUnit.Celsius => celsius,
            TemperatureUnit.Fahrenheit => celsius * 9 / 5 + 32,
            TemperatureUnit.Kelvin => celsius + 273.15,
            TemperatureUnit.Rankine => (celsius + 273.15) * 9 / 5,
            _ => celsius
        };
    }

    // Programming functions
    public long BitwiseAnd(long a, long b) => a & b;
    public long BitwiseOr(long a, long b) => a | b;
    public long BitwiseXor(long a, long b) => a ^ b;
    public long BitwiseNot(long a) => ~a;
    public long LeftShift(long value, int positions) => value << positions;
    public long RightShift(long value, int positions) => value >> positions;

    public string DecimalToBinary(long value) => Convert.ToString(value, 2);
    public string DecimalToHex(long value) => Convert.ToString(value, 16).ToUpper();
    public string DecimalToOctal(long value) => Convert.ToString(value, 8);

    public long BinaryToDecimal(string binary) => Convert.ToInt64(binary, 2);
    public long HexToDecimal(string hex) => Convert.ToInt64(hex, 16);
    public long OctalToDecimal(string octal) => Convert.ToInt64(octal, 8);
}

public enum LengthUnit
{
    Millimeter, Centimeter, Meter, Kilometer,
    Inch, Foot, Yard, Mile
}

public enum TemperatureUnit
{
    Celsius, Fahrenheit, Kelvin, Rankine
}

public enum CalculatorMode
{
    Standard, Scientific, Programming, Financial, Engineering
}