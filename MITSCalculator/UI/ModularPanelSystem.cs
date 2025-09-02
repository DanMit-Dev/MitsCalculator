using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using MITSCalculator.Core;
using MITSCalculator.Views;
using MITSCalculator.Statistics;
using MITSCalculator.CreativeTools;

namespace MITSCalculator.UI;

public class ModularPanelSystem : UserControl
{
    private readonly Dictionary<string, ICalculatorPanel> _panels;
    private readonly TabControl _tabView;
    private readonly MathEngine _mathEngine;
    private readonly StatisticalEngine _statisticalEngine;
    private string _activeWorkspace = "Default";

    public event EventHandler<string>? WorkspaceChanged;
    public event EventHandler<string>? PanelAdded;
    public event EventHandler<string>? PanelRemoved;

    public ModularPanelSystem(MathEngine mathEngine, StatisticalEngine statisticalEngine)
    {
        _mathEngine = mathEngine;
        _statisticalEngine = statisticalEngine;
        _panels = new Dictionary<string, ICalculatorPanel>();
        _tabView = new TabControl();
        
        InitializeComponent();
        LoadDefaultPanels();
    }

    private void InitializeComponent()
    {
        Content = _tabView;
    }

    private void LoadDefaultPanels()
    {
        // Core calculator panels
        AddPanel("Calculator", new BasicCalculatorPanel(_mathEngine));
        AddPanel("Statistics", new StatisticsPanel(_statisticalEngine));
        AddPanel("Fractals", new FractalPanel());
        AddPanel("Audio", new AudioPanel());
        AddPanel("STEM Tools", new STEMPanel());
    }

    public void AddPanel(string name, ICalculatorPanel panel)
    {
        if (_panels.ContainsKey(name)) return;

        _panels[name] = panel;
        
        var tabItem = new TabItem
        {
            Header = name,
            Content = panel.GetContent()
        };

        _tabView.Items.Add(tabItem);
        PanelAdded?.Invoke(this, name);
    }

    public void RemovePanel(string name)
    {
        if (!_panels.ContainsKey(name) || IsSystemPanel(name)) return;

        _panels.Remove(name);
        
        var tabToRemove = _tabView.Items
            .OfType<TabItem>()
            .FirstOrDefault(tab => tab.Header?.ToString() == name);
        
        if (tabToRemove != null)
        {
            _tabView.Items.Remove(tabToRemove);
        }
        
        PanelRemoved?.Invoke(this, name);
    }

    public void SelectPanel(string name)
    {
        var tab = _tabView.Items
            .OfType<TabItem>()
            .FirstOrDefault(tab => tab.Header?.ToString() == name);
        
        if (tab != null)
        {
            _tabView.SelectedItem = tab;
        }
    }

    public ICalculatorPanel? GetPanel(string name)
    {
        return _panels.GetValueOrDefault(name);
    }

    public IEnumerable<string> GetPanelNames()
    {
        return _panels.Keys;
    }

    private bool IsSystemPanel(string name)
    {
        var systemPanels = new[] { "Calculator", "Statistics" };
        return systemPanels.Contains(name);
    }

    private string GetActivePanel()
    {
        var selectedTab = _tabView.SelectedItem as TabItem;
        return selectedTab?.Header?.ToString() ?? "Calculator";
    }

    public string ActiveWorkspace => _activeWorkspace;
}

public interface ICalculatorPanel
{
    string Name { get; }
    Control GetContent();
    void Initialize();
    void Cleanup();
}

public class BasicCalculatorPanel : UserControl, ICalculatorPanel
{
    private readonly MathEngine _mathEngine;
    
    public new string Name => "Basic Calculator";

    public BasicCalculatorPanel(MathEngine mathEngine)
    {
        _mathEngine = mathEngine;
        Initialize();
    }

    public Control GetContent() => this;

    public void Initialize()
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Avalonia.Thickness(10)
        };

        // Display
        var display = new TextBox
        {
            Text = "0",
            IsReadOnly = true,
            FontSize = 24,
            TextAlignment = Avalonia.Media.TextAlignment.Right,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(display, 0);
        grid.Children.Add(display);

        // Button grid
        var buttonGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            RowDefinitions = new RowDefinitions("*,*,*,*,*")
        };

        // Add calculator buttons
        var buttons = new[,]
        {
            {"C", "CE", "⌫", "÷"},
            {"7", "8", "9", "×"},
            {"4", "5", "6", "-"},
            {"1", "2", "3", "+"},
            {"±", "0", ".", "="}
        };

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var button = new Button
                {
                    Content = buttons[row, col],
                    Margin = new Avalonia.Thickness(2),
                    FontSize = 16
                };
                
                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                buttonGrid.Children.Add(button);
            }
        }

        Grid.SetRow(buttonGrid, 1);
        grid.Children.Add(buttonGrid);
        
        Content = grid;
    }

    public void Cleanup()
    {
        // Cleanup resources if needed
    }
}

public class StatisticsPanel : UserControl, ICalculatorPanel
{
    private readonly StatisticalEngine _statisticalEngine;
    
    public new string Name => "Statistics";

    public StatisticsPanel(StatisticalEngine statisticalEngine)
    {
        _statisticalEngine = statisticalEngine;
        Initialize();
    }

    public Control GetContent() => this;

    public void Initialize()
    {
        var stackPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Avalonia.Thickness(10)
        };

        stackPanel.Children.Add(new TextBlock { Text = "Statistical Analysis", FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18 });

        // Data input
        var dataInput = new TextBox
        {
            Watermark = "Enter data values separated by commas",
            Height = 100,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        stackPanel.Children.Add(new TextBlock { Text = "Data Input:" });
        stackPanel.Children.Add(dataInput);

        // Analysis buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        var resultsArea = new ScrollViewer
        {
            Height = 300,
            Content = new TextBlock { Text = "Results will appear here..." }
        };

        var calcStatsButton = new Button { Content = "Calculate Stats" };
        calcStatsButton.Click += (s, e) =>
        {
            try
            {
                var dataText = dataInput.Text;
                if (string.IsNullOrWhiteSpace(dataText)) return;

                var values = dataText.Split(',')
                    .Select(x => double.Parse(x.Trim()))
                    .ToArray();

                var stats = _statisticalEngine.CalculateDescriptiveStatistics(values);
                
                var result = $"Mean: {stats.Mean:F4}\n" +
                           $"Median: {stats.Median:F4}\n" +
                           $"Standard Deviation: {stats.StandardDeviation:F4}\n" +
                           $"Variance: {stats.Variance:F4}\n" +
                           $"Min: {stats.Min:F4}\n" +
                           $"Max: {stats.Max:F4}\n" +
                           $"Count: {stats.Count}";

                resultsArea.Content = new TextBlock { Text = result };
            }
            catch (Exception ex)
            {
                resultsArea.Content = new TextBlock { Text = $"Error: {ex.Message}" };
            }
        };

        buttonPanel.Children.Add(calcStatsButton);
        stackPanel.Children.Add(buttonPanel);
        stackPanel.Children.Add(new TextBlock { Text = "Results:" });
        stackPanel.Children.Add(resultsArea);

        Content = stackPanel;
    }

    public void Cleanup()
    {
        // Cleanup resources if needed
    }
}

public class FractalPanel : UserControl, ICalculatorPanel
{
    public new string Name => "Fractals";

    public FractalPanel()
    {
        Initialize();
    }

    public Control GetContent() => this;

    public void Initialize()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("250,*"),
            Margin = new Avalonia.Thickness(10)
        };

        // Controls panel
        var controlsPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };

        controlsPanel.Children.Add(new TextBlock { Text = "Fractal Controls", FontWeight = Avalonia.Media.FontWeight.Bold });

        // Fractal type selector
        var typeSelector = new ComboBox();
        foreach (var type in new[] { "Mandelbrot", "Julia", "Burning Ship", "Newton" })
            typeSelector.Items.Add(type);
        typeSelector.SelectedIndex = 0;
        
        controlsPanel.Children.Add(new TextBlock { Text = "Fractal Type:" });
        controlsPanel.Children.Add(typeSelector);

        // Parameters
        controlsPanel.Children.Add(new TextBlock { Text = "Zoom:" });
        controlsPanel.Children.Add(new Slider { Minimum = 0.1, Maximum = 100, Value = 1 });

        controlsPanel.Children.Add(new TextBlock { Text = "Iterations:" });
        controlsPanel.Children.Add(new NumericUpDown { Minimum = 10, Maximum = 1000, Value = 100 });

        controlsPanel.Children.Add(new Button { Content = "Generate Fractal" });

        Grid.SetColumn(controlsPanel, 0);
        grid.Children.Add(controlsPanel);

        // Fractal display area
        var displayArea = new Border
        {
            Background = Avalonia.Media.Brushes.Black,
            Child = new TextBlock 
            { 
                Text = "Fractal will be displayed here",
                Foreground = Avalonia.Media.Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };

        Grid.SetColumn(displayArea, 1);
        grid.Children.Add(displayArea);

        Content = grid;
    }

    public void Cleanup()
    {
        // Cleanup resources if needed
    }
}

public class AudioPanel : UserControl, ICalculatorPanel
{
    public new string Name => "Audio Synthesis";

    public AudioPanel()
    {
        Initialize();
    }

    public Control GetContent() => this;

    public void Initialize()
    {
        var stackPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Avalonia.Thickness(10)
        };

        stackPanel.Children.Add(new TextBlock { Text = "Mathematical Audio Synthesis", FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18 });

        // Waveform controls
        var waveformPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        waveformPanel.Children.Add(new TextBlock { Text = "Waveform:", VerticalAlignment = VerticalAlignment.Center });
        
        var waveformSelector = new ComboBox();
        foreach (var wave in new[] { "Sine", "Square", "Sawtooth", "Triangle", "Noise" })
            waveformSelector.Items.Add(wave);
        waveformSelector.SelectedIndex = 0;
        waveformPanel.Children.Add(waveformSelector);
        
        stackPanel.Children.Add(waveformPanel);

        // Frequency control
        var freqPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        freqPanel.Children.Add(new TextBlock { Text = "Frequency (Hz):", VerticalAlignment = VerticalAlignment.Center });
        freqPanel.Children.Add(new NumericUpDown { Minimum = 20, Maximum = 20000, Value = 440 });
        stackPanel.Children.Add(freqPanel);

        // Duration control
        var durationPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        durationPanel.Children.Add(new TextBlock { Text = "Duration (s):", VerticalAlignment = VerticalAlignment.Center });
        durationPanel.Children.Add(new NumericUpDown { Minimum = 0.1m, Maximum = 10m, Value = 1m, Increment = 0.1m });
        stackPanel.Children.Add(durationPanel);

        // Mathematical function input
        stackPanel.Children.Add(new TextBlock { Text = "Custom Function f(t):" });
        stackPanel.Children.Add(new TextBox { Watermark = "Enter mathematical function, e.g., sin(440*2*pi*t)" });

        // Control buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttonPanel.Children.Add(new Button { Content = "Generate" });
        buttonPanel.Children.Add(new Button { Content = "Play" });
        buttonPanel.Children.Add(new Button { Content = "Stop" });
        buttonPanel.Children.Add(new Button { Content = "Export WAV" });
        stackPanel.Children.Add(buttonPanel);

        Content = stackPanel;
    }

    public void Cleanup()
    {
        // Cleanup resources if needed
    }
}

public class STEMPanel : UserControl, ICalculatorPanel
{
    public new string Name => "STEM Tools";

    public STEMPanel()
    {
        Initialize();
    }

    public Control GetContent() => this;

    public void Initialize()
    {
        Content = new STEMSimulatorView();
    }

    public void Cleanup()
    {
        // Cleanup resources if needed
    }
}