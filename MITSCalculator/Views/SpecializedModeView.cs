using Avalonia.Controls;
using Avalonia.Layout;
using MITSCalculator.Core;
using System;
using System.Collections.Generic;

namespace MITSCalculator.Views;

public class SpecializedModeView : UserControl
{
    private readonly MathEngine _mathEngine;
    private CalculatorMode _currentMode = CalculatorMode.Standard;
    private StackPanel _modePanel = null!;

    public CalculatorMode CurrentMode
    {
        get => _currentMode;
        set
        {
            _currentMode = value;
            UpdateModePanel();
        }
    }

    public SpecializedModeView(MathEngine mathEngine)
    {
        _mathEngine = mathEngine;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        _modePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        Content = new ScrollViewer
        {
            Content = _modePanel
        };

        UpdateModePanel();
    }

    private void UpdateModePanel()
    {
        _modePanel.Children.Clear();

        switch (_currentMode)
        {
            case CalculatorMode.Scientific:
                AddScientificControls();
                break;
            case CalculatorMode.Programming:
                AddProgrammingControls();
                break;
            case CalculatorMode.Financial:
                AddFinancialControls();
                break;
            case CalculatorMode.Engineering:
                AddEngineeringControls();
                break;
        }
    }

    private void AddScientificControls()
    {
        _modePanel.Children.Add(new TextBlock { Text = "Scientific Mode", FontWeight = Avalonia.Media.FontWeight.Bold });

        // Angle unit selector
        var anglePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        anglePanel.Children.Add(new TextBlock { Text = "Angle Unit:" });
        
        var degreeRadio = new RadioButton { Content = "Degrees", IsChecked = true };
        var radianRadio = new RadioButton { Content = "Radians" };
        
        anglePanel.Children.Add(degreeRadio);
        anglePanel.Children.Add(radianRadio);
        _modePanel.Children.Add(anglePanel);

        // Advanced functions grid
        var functionsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto")
        };

        // Row 0: Inverse trig functions
        AddButtonToGrid(functionsGrid, "asin", 0, 0);
        AddButtonToGrid(functionsGrid, "acos", 1, 0);
        AddButtonToGrid(functionsGrid, "atan", 2, 0);
        AddButtonToGrid(functionsGrid, "atan2", 3, 0);

        // Row 1: Hyperbolic functions
        AddButtonToGrid(functionsGrid, "sinh", 0, 1);
        AddButtonToGrid(functionsGrid, "cosh", 1, 1);
        AddButtonToGrid(functionsGrid, "tanh", 2, 1);
        AddButtonToGrid(functionsGrid, "ln", 3, 1);

        // Row 2: Other functions
        AddButtonToGrid(functionsGrid, "log₂", 0, 2);
        AddButtonToGrid(functionsGrid, "logₓ", 1, 2);
        AddButtonToGrid(functionsGrid, "n!", 2, 2);
        AddButtonToGrid(functionsGrid, "ⁿPᵣ", 3, 2);

        _modePanel.Children.Add(functionsGrid);
    }

    private void AddProgrammingControls()
    {
        _modePanel.Children.Add(new TextBlock { Text = "Programming Mode", FontWeight = Avalonia.Media.FontWeight.Bold });

        // Number base selector
        var basePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        basePanel.Children.Add(new TextBlock { Text = "Base:" });
        
        var hexRadio = new RadioButton { Content = "HEX" };
        var decRadio = new RadioButton { Content = "DEC", IsChecked = true };
        var octRadio = new RadioButton { Content = "OCT" };
        var binRadio = new RadioButton { Content = "BIN" };
        
        basePanel.Children.Add(hexRadio);
        basePanel.Children.Add(decRadio);
        basePanel.Children.Add(octRadio);
        basePanel.Children.Add(binRadio);
        _modePanel.Children.Add(basePanel);

        // Bitwise operations
        var bitwiseGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddButtonToGrid(bitwiseGrid, "AND", 0, 0);
        AddButtonToGrid(bitwiseGrid, "OR", 1, 0);
        AddButtonToGrid(bitwiseGrid, "XOR", 2, 0);
        AddButtonToGrid(bitwiseGrid, "NOT", 3, 0);

        AddButtonToGrid(bitwiseGrid, "LSH", 0, 1);
        AddButtonToGrid(bitwiseGrid, "RSH", 1, 1);
        AddButtonToGrid(bitwiseGrid, "MOD", 2, 1);
        AddButtonToGrid(bitwiseGrid, "DIV", 3, 1);

        _modePanel.Children.Add(bitwiseGrid);

        // Hex digits
        var hexGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddButtonToGrid(hexGrid, "A", 0, 0);
        AddButtonToGrid(hexGrid, "B", 1, 0);
        AddButtonToGrid(hexGrid, "C", 2, 0);
        AddButtonToGrid(hexGrid, "D", 0, 1);
        AddButtonToGrid(hexGrid, "E", 1, 1);
        AddButtonToGrid(hexGrid, "F", 2, 1);

        _modePanel.Children.Add(hexGrid);
    }

    private void AddFinancialControls()
    {
        _modePanel.Children.Add(new TextBlock { Text = "Financial Mode", FontWeight = Avalonia.Media.FontWeight.Bold });

        // Time value of money
        var tvm = CreateFieldSet("Time Value of Money");
        tvm.Children.Add(CreateInputField("Present Value (PV)", "pv"));
        tvm.Children.Add(CreateInputField("Future Value (FV)", "fv"));
        tvm.Children.Add(CreateInputField("Interest Rate (%)", "rate"));
        tvm.Children.Add(CreateInputField("Number of Periods", "n"));
        tvm.Children.Add(CreateInputField("Payment (PMT)", "pmt"));
        _modePanel.Children.Add(tvm);

        // Loan calculations
        var loan = CreateFieldSet("Loan Calculations");
        loan.Children.Add(CreateInputField("Principal", "principal"));
        loan.Children.Add(CreateInputField("Annual Rate (%)", "annual_rate"));
        loan.Children.Add(CreateInputField("Loan Term (years)", "term"));
        loan.Children.Add(CreateButtonPanel(new[] { "Monthly Payment", "Total Interest", "Amortization" }));
        _modePanel.Children.Add(loan);

        // Investment calculations
        var investment = CreateFieldSet("Investment Calculations");
        investment.Children.Add(CreateInputField("Initial Investment", "initial"));
        investment.Children.Add(CreateInputField("Monthly Contribution", "monthly"));
        investment.Children.Add(CreateInputField("Return Rate (%)", "return_rate"));
        investment.Children.Add(CreateInputField("Years", "years"));
        investment.Children.Add(CreateButtonPanel(new[] { "Calculate", "Chart Growth" }));
        _modePanel.Children.Add(investment);
    }

    private void AddEngineeringControls()
    {
        _modePanel.Children.Add(new TextBlock { Text = "Engineering Mode", FontWeight = Avalonia.Media.FontWeight.Bold });

        // Unit conversions
        var units = CreateFieldSet("Unit Conversions");
        
        // Length conversion
        var lengthPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        lengthPanel.Children.Add(new TextBox { Width = 100, Watermark = "Value" });
        var fromCombo = new ComboBox { SelectedIndex = 2 };
        foreach (var item in new[] { "mm", "cm", "m", "km", "in", "ft", "yd", "mi" })
            fromCombo.Items.Add(item);
        lengthPanel.Children.Add(fromCombo);
        lengthPanel.Children.Add(new TextBlock { Text = "→" });
        var toCombo = new ComboBox { SelectedIndex = 0 };
        foreach (var item in new[] { "mm", "cm", "m", "km", "in", "ft", "yd", "mi" })
            toCombo.Items.Add(item);
        lengthPanel.Children.Add(toCombo);
        lengthPanel.Children.Add(new Button { Content = "Convert" });
        units.Children.Add(lengthPanel);

        // Temperature conversion
        var tempPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        tempPanel.Children.Add(new TextBox { Width = 100, Watermark = "Value" });
        var fromTempCombo = new ComboBox { SelectedIndex = 0 };
        foreach (var item in new[] { "°C", "°F", "K", "°R" })
            fromTempCombo.Items.Add(item);
        tempPanel.Children.Add(fromTempCombo);
        tempPanel.Children.Add(new TextBlock { Text = "→" });
        var toTempCombo = new ComboBox { SelectedIndex = 1 };
        foreach (var item in new[] { "°C", "°F", "K", "°R" })
            toTempCombo.Items.Add(item);
        tempPanel.Children.Add(toTempCombo);
        tempPanel.Children.Add(new Button { Content = "Convert" });
        units.Children.Add(tempPanel);

        _modePanel.Children.Add(units);

        // Constants
        var constants = CreateFieldSet("Engineering Constants");
        var constantsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto")
        };

        AddButtonToGrid(constantsGrid, "c (light)", 0, 0);
        AddButtonToGrid(constantsGrid, "h (Planck)", 1, 0);
        AddButtonToGrid(constantsGrid, "k (Boltzmann)", 0, 1);
        AddButtonToGrid(constantsGrid, "g (gravity)", 1, 1);
        AddButtonToGrid(constantsGrid, "R (gas)", 0, 2);
        AddButtonToGrid(constantsGrid, "N_A (Avogadro)", 1, 2);

        constants.Children.Add(constantsGrid);
        _modePanel.Children.Add(constants);
    }

    private StackPanel CreateFieldSet(string title)
    {
        var panel = new StackPanel
        {
            Spacing = 5,
            Margin = new Avalonia.Thickness(0, 10)
        };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = title, 
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        });
        
        return panel;
    }

    private StackPanel CreateInputField(string label, string name)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = label, Width = 150 });
        panel.Children.Add(new TextBox { Name = name, Width = 150 });
        return panel;
    }

    private StackPanel CreateButtonPanel(string[] buttonTexts)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        foreach (var text in buttonTexts)
        {
            panel.Children.Add(new Button { Content = text });
        }
        return panel;
    }

    private void AddButtonToGrid(Grid grid, string content, int column, int row)
    {
        var button = new Button
        {
            Content = content,
            Margin = new Avalonia.Thickness(2)
        };
        Grid.SetColumn(button, column);
        Grid.SetRow(button, row);
        grid.Children.Add(button);
    }
}