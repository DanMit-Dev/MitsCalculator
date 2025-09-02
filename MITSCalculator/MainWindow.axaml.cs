using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Globalization;
using MathNet.Numerics;
using NCalc2;
using MITSCalculator.Core;
using MITSCalculator.Views;
using MITSCalculator.UI;
using MITSCalculator.Data;
using MITSCalculator.Statistics;
using MITSCalculator.Plugins;

namespace MITSCalculator;

public partial class MainWindow : Avalonia.Controls.Window
{
    private string _currentInput = "0";
    private string _previousInput = "";
    private string _operation = "";
    private bool _waitingForOperand = false;
    private string _expression = "";
    private readonly MathEngine _mathEngine;
    private readonly ExpressionParser _parser;
    private readonly StatisticalEngine _statisticalEngine;
    private readonly DatabaseManager _databaseManager;
    private readonly ThemeManager _themeManager;
    private readonly PluginManager _pluginManager;
    private CalculatorMode _currentMode = CalculatorMode.Standard;
    private ModularPanelSystem _panelSystem;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize core components
        _mathEngine = new MathEngine();
        _parser = new ExpressionParser(_mathEngine);
        _statisticalEngine = new StatisticalEngine();
        _databaseManager = new DatabaseManager();
        _themeManager = new ThemeManager();
        _pluginManager = new PluginManager();
        
        // Initialize modular panel system
        _panelSystem = new ModularPanelSystem(_mathEngine, _statisticalEngine);
        
        // Set up the UI
        SetupMainInterface();
        
        // Apply theme
        _themeManager.ApplyTheme("Dark");
        
        // Load plugins
        _pluginManager.LoadAllPlugins();
        
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
                        result = Math.Sin(value * Math.PI / 180);
                        break;
                    case "cos":
                        result = Math.Cos(value * Math.PI / 180);
                        break;
                    case "tan":
                        result = Math.Tan(value * Math.PI / 180);
                        break;
                    case "log":
                        result = _mathEngine.Log10(value);
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
                    _currentInput = _mathEngine.GetConstant("π").ToString("G", CultureInfo.InvariantCulture);
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

    private void SetupMainInterface()
    {
        // Replace the existing content with the modular panel system
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        // Menu bar
        var menuBar = CreateMenuBar();
        Grid.SetRow(menuBar, 0);
        mainGrid.Children.Add(menuBar);

        // Panel system
        Grid.SetRow(_panelSystem, 1);
        mainGrid.Children.Add(_panelSystem);

        Content = mainGrid;
    }

    private Menu CreateMenuBar()
    {
        var menuBar = new Menu();

        // File menu
        var fileMenu = new MenuItem { Header = "File" };
        fileMenu.Items.Add(new MenuItem { Header = "New Workspace" });
        fileMenu.Items.Add(new MenuItem { Header = "Save Workspace" });
        fileMenu.Items.Add(new MenuItem { Header = "Load Workspace" });
        fileMenu.Items.Add(new Separator());
        fileMenu.Items.Add(new MenuItem { Header = "Exit" });

        // View menu
        var viewMenu = new MenuItem { Header = "View" };
        var themeSubmenu = new MenuItem { Header = "Theme" };
        themeSubmenu.Items.Add(new MenuItem { Header = "Light" });
        themeSubmenu.Items.Add(new MenuItem { Header = "Dark" });
        viewMenu.Items.Add(themeSubmenu);

        // Tools menu
        var toolsMenu = new MenuItem { Header = "Tools" };
        toolsMenu.Items.Add(new MenuItem { Header = "Physics Simulator" });
        toolsMenu.Items.Add(new MenuItem { Header = "Chemistry Tools" });
        toolsMenu.Items.Add(new MenuItem { Header = "Plugin Manager" });

        // Help menu
        var helpMenu = new MenuItem { Header = "Help" };
        helpMenu.Items.Add(new MenuItem { Header = "About" });

        menuBar.Items.Add(fileMenu);
        menuBar.Items.Add(viewMenu);
        menuBar.Items.Add(toolsMenu);
        menuBar.Items.Add(helpMenu);

        return menuBar;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Cleanup resources
        _pluginManager?.Dispose();
        _databaseManager?.Dispose();
        base.OnClosing(e);
    }
}