using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Globalization;
using MathNet.Numerics;
using NCalc;

namespace MITSCalculator;

public partial class MainWindow : Avalonia.Controls.Window
{
    private string _currentInput = "0";
    private string _previousInput = "";
    private string _operation = "";
    private bool _waitingForOperand = false;
    private string _expression = "";

    public MainWindow()
    {
        InitializeComponent();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        MainDisplay.Text = _currentInput;
        HistoryDisplay.Text = _expression;
    }

    private void Number_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string number = button.Content?.ToString() ?? "";
            
            if (_waitingForOperand || _currentInput == "0")
            {
                _currentInput = number;
                _waitingForOperand = false;
            }
            else
            {
                _currentInput += number;
            }
            
            UpdateDisplay();
        }
    }

    private void Operator_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string op = button.Content?.ToString() ?? "";
            
            if (!_waitingForOperand && _operation != "")
            {
                Calculate();
            }
            
            _previousInput = _currentInput;
            _operation = op;
            _expression = $"{_currentInput} {op}";
            _waitingForOperand = true;
            
            UpdateDisplay();
        }
    }

    private void Function_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string function = button.Content?.ToString() ?? "";
            
            try
            {
                double value = double.Parse(_currentInput, CultureInfo.InvariantCulture);
                double result = 0;
                
                switch (function)
                {
                    case "sin":
                        result = Math.Sin(value * Math.PI / 180); // Convert to radians
                        break;
                    case "cos":
                        result = Math.Cos(value * Math.PI / 180); // Convert to radians
                        break;
                    case "tan":
                        result = Math.Tan(value * Math.PI / 180); // Convert to radians
                        break;
                    case "log":
                        result = Math.Log10(value);
                        break;
                    case "√":
                        result = Math.Sqrt(value);
                        break;
                    case "x²":
                        result = Math.Pow(value, 2);
                        break;
                }
                
                _currentInput = result.ToString("G", CultureInfo.InvariantCulture);
                _expression = $"{function}({value})";
                _waitingForOperand = true;
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                _currentInput = "Error";
                _expression = ex.Message;
                UpdateDisplay();
            }
        }
    }

    private void Constant_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string constant = button.Content?.ToString() ?? "";
            
            switch (constant)
            {
                case "π":
                    _currentInput = Math.PI.ToString("G", CultureInfo.InvariantCulture);
                    break;
            }
            
            _expression = constant;
            _waitingForOperand = true;
            UpdateDisplay();
        }
    }

    private void Decimal_Click(object sender, RoutedEventArgs e)
    {
        if (_waitingForOperand)
        {
            _currentInput = "0.";
            _waitingForOperand = false;
        }
        else if (!_currentInput.Contains("."))
        {
            _currentInput += ".";
        }
        
        UpdateDisplay();
    }

    private void Equals_Click(object sender, RoutedEventArgs e)
    {
        Calculate();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _currentInput = "0";
        _previousInput = "";
        _operation = "";
        _expression = "";
        _waitingForOperand = false;
        UpdateDisplay();
    }

    private void ClearEntry_Click(object sender, RoutedEventArgs e)
    {
        _currentInput = "0";
        UpdateDisplay();
    }

    private void Backspace_Click(object sender, RoutedEventArgs e)
    {
        if (_currentInput.Length > 1)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        }
        else
        {
            _currentInput = "0";
        }
        
        UpdateDisplay();
    }

    private void Calculate()
    {
        if (_operation == "" || _waitingForOperand) return;
        
        try
        {
            double prev = double.Parse(_previousInput, CultureInfo.InvariantCulture);
            double current = double.Parse(_currentInput, CultureInfo.InvariantCulture);
            double result = 0;
            
            switch (_operation)
            {
                case "+":
                    result = prev + current;
                    break;
                case "-":
                    result = prev - current;
                    break;
                case "×":
                    result = prev * current;
                    break;
                case "÷":
                    if (current != 0)
                        result = prev / current;
                    else
                        throw new DivideByZeroException("Cannot divide by zero");
                    break;
                case "x^y":
                    result = Math.Pow(prev, current);
                    break;
            }
            
            _currentInput = result.ToString("G", CultureInfo.InvariantCulture);
            _expression = $"{_previousInput} {_operation} {current} =";
            _operation = "";
            _previousInput = "";
            _waitingForOperand = true;
            UpdateDisplay();
        }
        catch (Exception ex)
        {
            _currentInput = "Error";
            _expression = ex.Message;
            UpdateDisplay();
        }
    }
}