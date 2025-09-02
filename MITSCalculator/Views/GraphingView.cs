using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Avalonia;
using MITSCalculator.Core;

namespace MITSCalculator.Views;

public class GraphingView : UserControl
{
    private readonly MathEngine _mathEngine;
    private readonly ExpressionParser _parser;
    private PlotView _plotView = null!;
    private TextBox _functionInput = null!;
    private TextBox _xMinInput = null!;
    private TextBox _xMaxInput = null!;
    private ComboBox _plotTypeCombo = null!;

    public GraphingView(MathEngine mathEngine, ExpressionParser parser)
    {
        _mathEngine = mathEngine;
        _parser = parser;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Avalonia.Thickness(10)
        };

        // Function input controls
        var inputPanel = CreateInputPanel();
        mainPanel.Children.Add(inputPanel);

        // Plot view
        _plotView = new PlotView
        {
            Height = 400,
            Background = Avalonia.Media.Brushes.White
        };
        mainPanel.Children.Add(_plotView);

        // Control buttons
        var buttonPanel = CreateButtonPanel();
        mainPanel.Children.Add(buttonPanel);

        Content = mainPanel;
        
        // Initialize with a sample function
        InitializeSamplePlot();
    }

    private StackPanel CreateInputPanel()
    {
        var panel = new StackPanel { Spacing = 10 };

        // Function input
        var funcPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        funcPanel.Children.Add(new TextBlock { Text = "f(x) =", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        _functionInput = new TextBox 
        { 
            Width = 200, 
            Text = "sin(x)",
            Watermark = "Enter function of x"
        };
        funcPanel.Children.Add(_functionInput);
        panel.Children.Add(funcPanel);

        // Range inputs
        var rangePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        rangePanel.Children.Add(new TextBlock { Text = "Range:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        
        _xMinInput = new TextBox { Width = 80, Text = "-10" };
        rangePanel.Children.Add(_xMinInput);
        
        rangePanel.Children.Add(new TextBlock { Text = "≤ x ≤", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        
        _xMaxInput = new TextBox { Width = 80, Text = "10" };
        rangePanel.Children.Add(_xMaxInput);
        
        panel.Children.Add(rangePanel);

        // Plot type selector
        var typePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        typePanel.Children.Add(new TextBlock { Text = "Type:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        
        _plotTypeCombo = new ComboBox
        {
            SelectedIndex = 0,
            Width = 100
        };
        foreach (var item in new[] { "Line", "Scatter", "Area", "Bar" })
            _plotTypeCombo.Items.Add(item);
        typePanel.Children.Add(_plotTypeCombo);
        
        panel.Children.Add(typePanel);

        return panel;
    }

    private StackPanel CreateButtonPanel()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

        var plotButton = new Button { Content = "Plot Function" };
        plotButton.Click += PlotButton_Click;
        panel.Children.Add(plotButton);

        var clearButton = new Button { Content = "Clear Plot" };
        clearButton.Click += ClearButton_Click;
        panel.Children.Add(clearButton);

        var addButton = new Button { Content = "Add Series" };
        addButton.Click += AddSeriesButton_Click;
        panel.Children.Add(addButton);

        var exportButton = new Button { Content = "Export PNG" };
        exportButton.Click += ExportButton_Click;
        panel.Children.Add(exportButton);

        return panel;
    }

    private void InitializeSamplePlot()
    {
        var plotModel = new PlotModel 
        { 
            Title = "Function Plotter",
            Background = OxyColors.White
        };

        // Add axes
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis 
        { 
            Position = AxisPosition.Bottom, 
            Title = "x",
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineColor = OxyColors.LightGray
        });
        
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis 
        { 
            Position = AxisPosition.Left, 
            Title = "f(x)",
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineColor = OxyColors.LightGray
        });

        _plotView.Model = plotModel;
        
        // Plot the initial function
        PlotFunction("sin(x)", -10, 10, OxyColors.Blue);
    }

    private void PlotButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var function = _functionInput.Text ?? "x";
            var xMin = double.Parse(_xMinInput.Text ?? "-10");
            var xMax = double.Parse(_xMaxInput.Text ?? "10");

            _plotView.Model.Series.Clear();
            PlotFunction(function, xMin, xMax, OxyColors.Blue);
            _plotView.InvalidatePlot();
        }
        catch (Exception ex)
        {
            // Handle error - could show a message box or status message
            Console.WriteLine($"Error plotting function: {ex.Message}");
        }
    }

    private void ClearButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _plotView.Model.Series.Clear();
        _plotView.InvalidatePlot();
    }

    private void AddSeriesButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var function = _functionInput.Text ?? "x";
            var xMin = double.Parse(_xMinInput.Text ?? "-10");
            var xMax = double.Parse(_xMaxInput.Text ?? "10");

            // Add as new series with different color
            var colors = new[] { OxyColors.Red, OxyColors.Green, OxyColors.Orange, OxyColors.Purple, OxyColors.Brown };
            var colorIndex = _plotView.Model.Series.Count % colors.Length;
            
            PlotFunction(function, xMin, xMax, colors[colorIndex]);
            _plotView.InvalidatePlot();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding series: {ex.Message}");
        }
    }

    private void ExportButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            // Export functionality would go here
            // For now, just show that it's working
            Console.WriteLine("Export functionality would be implemented here");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting: {ex.Message}");
        }
    }

    private void PlotFunction(string functionExpression, double xMin, double xMax, OxyColor color)
    {
        try
        {
            var points = new List<DataPoint>();
            var numPoints = 1000;
            var dx = (xMax - xMin) / numPoints;

            for (int i = 0; i <= numPoints; i++)
            {
                var x = xMin + i * dx;
                try
                {
                    // Set x as a variable for the expression
                    _mathEngine.SetVariable("x", x);
                    
                    // Replace x in the expression and evaluate
                    var expression = functionExpression.Replace("x", x.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    var y = _parser.Evaluate(expression);
                    
                    if (!double.IsNaN(y) && !double.IsInfinity(y))
                    {
                        points.Add(new DataPoint(x, y));
                    }
                }
                catch
                {
                    // Skip invalid points
                    continue;
                }
            }

            if (points.Count > 0)
            {
                var series = new OxyPlot.Series.LineSeries
                {
                    Title = functionExpression,
                    Color = color,
                    StrokeThickness = 2,
                    ItemsSource = points
                };

                _plotView.Model.Series.Add(series);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error plotting function '{functionExpression}': {ex.Message}");
        }
    }

    public void PlotMultipleFunctions(string[] functions, double xMin, double xMax)
    {
        _plotView.Model.Series.Clear();
        
        var colors = new[] { OxyColors.Blue, OxyColors.Red, OxyColors.Green, OxyColors.Orange, OxyColors.Purple };
        
        for (int i = 0; i < functions.Length && i < colors.Length; i++)
        {
            PlotFunction(functions[i], xMin, xMax, colors[i]);
        }
        
        _plotView.InvalidatePlot();
    }

    public void SetFunction(string function)
    {
        _functionInput.Text = function;
    }

    public void SetRange(double xMin, double xMax)
    {
        _xMinInput.Text = xMin.ToString();
        _xMaxInput.Text = xMax.ToString();
    }
}