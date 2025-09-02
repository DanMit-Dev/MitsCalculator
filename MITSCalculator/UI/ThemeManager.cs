using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace MITSCalculator.UI;

public class ThemeManager
{
    private readonly Dictionary<string, ThemeVariant> _themes;
    private string _currentTheme = "Dark";

    public event EventHandler<string>? ThemeChanged;

    public ThemeManager()
    {
        _themes = new Dictionary<string, ThemeVariant>
        {
            ["Light"] = ThemeVariant.Light,
            ["Dark"] = ThemeVariant.Dark
        };
    }

    public string CurrentTheme 
    { 
        get => _currentTheme;
        set
        {
            if (_currentTheme != value && _themes.ContainsKey(value))
            {
                _currentTheme = value;
                ApplyTheme(value);
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public IEnumerable<string> AvailableThemes => _themes.Keys;

    public void ApplyTheme(string themeName)
    {
        if (_themes.TryGetValue(themeName, out var theme))
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = theme;
            }
        }
    }

    public ThemeColors GetCurrentThemeColors()
    {
        return _currentTheme switch
        {
            "Light" => new ThemeColors
            {
                Primary = Color.FromRgb(0, 120, 215),
                Secondary = Color.FromRgb(100, 100, 100),
                Background = Color.FromRgb(255, 255, 255),
                Surface = Color.FromRgb(248, 248, 248),
                OnPrimary = Color.FromRgb(255, 255, 255),
                OnSecondary = Color.FromRgb(255, 255, 255),
                OnBackground = Color.FromRgb(0, 0, 0),
                OnSurface = Color.FromRgb(0, 0, 0),
                Border = Color.FromRgb(200, 200, 200),
                Accent = Color.FromRgb(0, 120, 215)
            },
            "Dark" => new ThemeColors
            {
                Primary = Color.FromRgb(0, 120, 215),
                Secondary = Color.FromRgb(150, 150, 150),
                Background = Color.FromRgb(32, 32, 32),
                Surface = Color.FromRgb(48, 48, 48),
                OnPrimary = Color.FromRgb(255, 255, 255),
                OnSecondary = Color.FromRgb(255, 255, 255),
                OnBackground = Color.FromRgb(255, 255, 255),
                OnSurface = Color.FromRgb(255, 255, 255),
                Border = Color.FromRgb(80, 80, 80),
                Accent = Color.FromRgb(100, 180, 255)
            },
            _ => new ThemeColors() // Default colors
        };
    }
}

public class ThemeColors
{
    public Color Primary { get; set; }
    public Color Secondary { get; set; }
    public Color Background { get; set; }
    public Color Surface { get; set; }
    public Color OnPrimary { get; set; }
    public Color OnSecondary { get; set; }
    public Color OnBackground { get; set; }
    public Color OnSurface { get; set; }
    public Color Border { get; set; }
    public Color Accent { get; set; }
}