using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace MITSCalculator.Core;

public class ExpressionParser
{
    private readonly MathEngine _mathEngine;
    private readonly Dictionary<string, Func<double[], double>> _functions;

    public ExpressionParser(MathEngine mathEngine)
    {
        _mathEngine = mathEngine;
        _functions = new Dictionary<string, Func<double[], double>>
        {
            // Basic functions
            ["sin"] = args => Math.Sin(args[0] * Math.PI / 180),
            ["cos"] = args => Math.Cos(args[0] * Math.PI / 180),
            ["tan"] = args => Math.Tan(args[0] * Math.PI / 180),
            ["asin"] = args => _mathEngine.ArcsinDegrees(args[0]),
            ["acos"] = args => _mathEngine.ArccosDegrees(args[0]),
            ["atan"] = args => _mathEngine.ArctanDegrees(args[0]),
            
            // Hyperbolic functions
            ["sinh"] = args => _mathEngine.SinhDegrees(args[0]),
            ["cosh"] = args => _mathEngine.CoshDegrees(args[0]),
            ["tanh"] = args => _mathEngine.TanhDegrees(args[0]),
            
            // Logarithmic functions
            ["log"] = args => args.Length == 1 ? Math.Log10(args[0]) : _mathEngine.Log(args[0], args[1]),
            ["ln"] = args => _mathEngine.Ln(args[0]),
            ["log2"] = args => _mathEngine.Log2(args[0]),
            
            // Root functions
            ["sqrt"] = args => Math.Sqrt(args[0]),
            ["cbrt"] = args => _mathEngine.CubeRoot(args[0]),
            ["root"] = args => _mathEngine.NthRoot(args[0], args[1]),
            
            // Power functions
            ["pow"] = args => Math.Pow(args[0], args[1]),
            ["exp"] = args => Math.Exp(args[0]),
            
            // Other functions
            ["abs"] = args => Math.Abs(args[0]),
            ["floor"] = args => Math.Floor(args[0]),
            ["ceil"] = args => Math.Ceiling(args[0]),
            ["round"] = args => Math.Round(args[0]),
            ["factorial"] = args => _mathEngine.Factorial(args[0]),
            ["rand"] = args => new Random().NextDouble(),
            
            // Statistical functions
            ["min"] = args => args.Min(),
            ["max"] = args => args.Max(),
            ["sum"] = args => args.Sum(),
            ["avg"] = args => args.Average(),
            
            // Programming functions
            ["mod"] = args => args[0] % args[1],
            ["and"] = args => _mathEngine.BitwiseAnd((long)args[0], (long)args[1]),
            ["or"] = args => _mathEngine.BitwiseOr((long)args[0], (long)args[1]),
            ["xor"] = args => _mathEngine.BitwiseXor((long)args[0], (long)args[1]),
            ["not"] = args => _mathEngine.BitwiseNot((long)args[0]),
        };
    }

    public double Evaluate(string expression)
    {
        try
        {
            // Handle empty or null expressions
            if (string.IsNullOrWhiteSpace(expression))
                return 0;

            // Replace constants
            expression = ReplaceConstants(expression);
            
            // Replace variables
            expression = ReplaceVariables(expression);
            
            // Parse and evaluate
            return EvaluateExpression(expression);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error evaluating expression: {ex.Message}", ex);
        }
    }

    private string ReplaceConstants(string expression)
    {
        // Replace mathematical constants
        expression = expression.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture));
        expression = expression.Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture));
        expression = expression.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));
        
        return expression;
    }

    private string ReplaceVariables(string expression)
    {
        // This is a simplified variable replacement
        // In a full implementation, you'd want to parse variable names more carefully
        var variablePattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b";
        return Regex.Replace(expression, variablePattern, match =>
        {
            var varName = match.Value;
            if (_functions.ContainsKey(varName.ToLower()))
                return varName; // Keep function names
            
            var value = _mathEngine.GetVariable(varName);
            if (!double.IsNaN(value))
                return value.ToString(CultureInfo.InvariantCulture);
            
            var constant = _mathEngine.GetConstant(varName);
            if (!double.IsNaN(constant))
                return constant.ToString(CultureInfo.InvariantCulture);
                
            return varName; // Keep unchanged if not found
        });
    }

    private double EvaluateExpression(string expression)
    {
        // This is a simplified expression evaluator
        // For a full implementation, you'd want to use a proper parser like NCalc
        // or implement a more robust parsing algorithm
        
        try
        {
            // Handle function calls first
            expression = ProcessFunctions(expression);
            
            // Simple arithmetic evaluation using NCalc
            var evaluator = new NCalc.Expression(expression);
            var result = evaluator.Evaluate();
            
            if (result is double d)
                return d;
            if (result is int i)
                return i;
            if (result is float f)
                return f;
            if (result is decimal dec)
                return (double)dec;
                
            return Convert.ToDouble(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid expression: {expression}", ex);
        }
    }

    private string ProcessFunctions(string expression)
    {
        // Process function calls like sin(30), log(100, 10), etc.
        var functionPattern = @"([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]+)\)";
        
        return Regex.Replace(expression, functionPattern, match =>
        {
            var functionName = match.Groups[1].Value.ToLower();
            var argumentsString = match.Groups[2].Value;
            
            if (!_functions.ContainsKey(functionName))
                return match.Value; // Keep unchanged if function not found
            
            // Parse arguments
            var arguments = argumentsString.Split(',')
                .Select(arg => EvaluateSimple(arg.Trim()))
                .ToArray();
            
            try
            {
                var result = _functions[functionName](arguments);
                return result.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return match.Value; // Keep unchanged if evaluation fails
            }
        });
    }

    private double EvaluateSimple(string expression)
    {
        // Simple evaluation for function arguments
        if (double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return value;
        
        // Try to evaluate as a simple expression
        try
        {
            var evaluator = new NCalc.Expression(expression);
            var result = evaluator.Evaluate();
            return Convert.ToDouble(result);
        }
        catch
        {
            throw new ArgumentException($"Invalid argument: {expression}");
        }
    }

    public void AddCustomFunction(string name, Func<double[], double> function)
    {
        _functions[name.ToLower()] = function;
    }
}