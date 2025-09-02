using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Avalonia.Controls;

namespace MITSCalculator.Plugins;

public interface ICalculatorPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    string Author { get; }
    
    void Initialize(IPluginHost host);
    void Shutdown();
    Control? GetUI();
    Dictionary<string, Func<double[], double>> GetFunctions();
}

public interface IPluginHost
{
    void RegisterFunction(string name, Func<double[], double> function);
    void UnregisterFunction(string name);
    void ShowMessage(string message);
    void AddPanel(string name, Control panel);
    void RemovePanel(string name);
    T? GetService<T>() where T : class;
}

public class PluginManager : IPluginHost
{
    private readonly Dictionary<string, ICalculatorPlugin> _loadedPlugins;
    private readonly Dictionary<string, Func<double[], double>> _pluginFunctions;
    private readonly Dictionary<string, Control> _pluginPanels;
    private readonly List<AssemblyLoadContext> _loadContexts;
    private readonly string _pluginDirectory;

    public event EventHandler<PluginEventArgs>? PluginLoaded;
    public event EventHandler<PluginEventArgs>? PluginUnloaded;
    public event EventHandler<string>? MessageReceived;

    public PluginManager()
    {
        _loadedPlugins = new Dictionary<string, ICalculatorPlugin>();
        _pluginFunctions = new Dictionary<string, Func<double[], double>>();
        _pluginPanels = new Dictionary<string, Control>();
        _loadContexts = new List<AssemblyLoadContext>();
        
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MITSCalculator");
        _pluginDirectory = Path.Combine(appDataPath, "Plugins");
        Directory.CreateDirectory(_pluginDirectory);
    }

    public IEnumerable<string> LoadedPlugins => _loadedPlugins.Keys;
    public IEnumerable<string> AvailableFunctions => _pluginFunctions.Keys;
    public IEnumerable<string> AvailablePanels => _pluginPanels.Keys;

    public void LoadAllPlugins()
    {
        var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories);
        
        foreach (var file in pluginFiles)
        {
            try
            {
                LoadPlugin(file);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to load plugin {Path.GetFileName(file)}: {ex.Message}");
            }
        }
    }

    public bool LoadPlugin(string assemblyPath)
    {
        try
        {
            if (!File.Exists(assemblyPath))
            {
                ShowMessage($"Plugin file not found: {assemblyPath}");
                return false;
            }

            // Create isolated load context
            var loadContext = new AssemblyLoadContext($"Plugin_{Path.GetFileNameWithoutExtension(assemblyPath)}", true);
            _loadContexts.Add(loadContext);

            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(ICalculatorPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var pluginType in pluginTypes)
            {
                var plugin = Activator.CreateInstance(pluginType) as ICalculatorPlugin;
                if (plugin == null) continue;

                // Check for conflicts
                if (_loadedPlugins.ContainsKey(plugin.Name))
                {
                    ShowMessage($"Plugin with name '{plugin.Name}' is already loaded");
                    continue;
                }

                // Initialize plugin
                plugin.Initialize(this);
                _loadedPlugins[plugin.Name] = plugin;

                // Register plugin functions
                var functions = plugin.GetFunctions();
                if (functions != null)
                {
                    foreach (var func in functions)
                    {
                        RegisterFunction($"{plugin.Name}.{func.Key}", func.Value);
                    }
                }

                // Add plugin UI if available
                var ui = plugin.GetUI();
                if (ui != null)
                {
                    AddPanel(plugin.Name, ui);
                }

                ShowMessage($"Plugin '{plugin.Name}' v{plugin.Version} loaded successfully");
                PluginLoaded?.Invoke(this, new PluginEventArgs(plugin));
            }

            return true;
        }
        catch (Exception ex)
        {
            ShowMessage($"Error loading plugin: {ex.Message}");
            return false;
        }
    }

    public bool UnloadPlugin(string pluginName)
    {
        if (!_loadedPlugins.TryGetValue(pluginName, out var plugin))
        {
            ShowMessage($"Plugin '{pluginName}' is not loaded");
            return false;
        }

        try
        {
            // Shutdown plugin
            plugin.Shutdown();

            // Remove plugin functions
            var functionsToRemove = _pluginFunctions.Keys
                .Where(key => key.StartsWith($"{pluginName}."))
                .ToList();
            
            foreach (var funcName in functionsToRemove)
            {
                _pluginFunctions.Remove(funcName);
            }

            // Remove plugin panel
            if (_pluginPanels.ContainsKey(pluginName))
            {
                _pluginPanels.Remove(pluginName);
            }

            _loadedPlugins.Remove(pluginName);
            
            ShowMessage($"Plugin '{pluginName}' unloaded successfully");
            PluginUnloaded?.Invoke(this, new PluginEventArgs(plugin));
            
            return true;
        }
        catch (Exception ex)
        {
            ShowMessage($"Error unloading plugin '{pluginName}': {ex.Message}");
            return false;
        }
    }

    public void UnloadAllPlugins()
    {
        var pluginNames = _loadedPlugins.Keys.ToList();
        foreach (var name in pluginNames)
        {
            UnloadPlugin(name);
        }

        // Unload assembly contexts
        foreach (var context in _loadContexts)
        {
            try
            {
                context.Unload();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error unloading assembly context: {ex.Message}");
            }
        }
        _loadContexts.Clear();
    }

    public ICalculatorPlugin? GetPlugin(string name)
    {
        return _loadedPlugins.GetValueOrDefault(name);
    }

    public PluginInfo[] GetPluginInfo()
    {
        return _loadedPlugins.Values.Select(p => new PluginInfo
        {
            Name = p.Name,
            Version = p.Version,
            Description = p.Description,
            Author = p.Author,
            IsLoaded = true
        }).ToArray();
    }

    public Func<double[], double>? GetFunction(string functionName)
    {
        return _pluginFunctions.GetValueOrDefault(functionName);
    }

    public Control? GetPanel(string panelName)
    {
        return _pluginPanels.GetValueOrDefault(panelName);
    }

    // IPluginHost implementation
    public void RegisterFunction(string name, Func<double[], double> function)
    {
        if (_pluginFunctions.ContainsKey(name))
        {
            ShowMessage($"Function '{name}' is already registered");
            return;
        }

        _pluginFunctions[name] = function;
    }

    public void UnregisterFunction(string name)
    {
        _pluginFunctions.Remove(name);
    }

    public void ShowMessage(string message)
    {
        MessageReceived?.Invoke(this, message);
        Console.WriteLine($"[PluginManager] {message}");
    }

    public void AddPanel(string name, Control panel)
    {
        _pluginPanels[name] = panel;
    }

    public void RemovePanel(string name)
    {
        _pluginPanels.Remove(name);
    }

    public T? GetService<T>() where T : class
    {
        // This could be expanded to provide access to calculator services
        return null;
    }

    public void Dispose()
    {
        UnloadAllPlugins();
    }
}

// Plugin creation helper
public abstract class CalculatorPluginBase : ICalculatorPlugin
{
    protected IPluginHost? Host;

    public abstract string Name { get; }
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract string Author { get; }

    public virtual void Initialize(IPluginHost host)
    {
        Host = host;
    }

    public virtual void Shutdown()
    {
        Host = null;
    }

    public virtual Control? GetUI()
    {
        return null;
    }

    public virtual Dictionary<string, Func<double[], double>> GetFunctions()
    {
        return new Dictionary<string, Func<double[], double>>();
    }

    protected void ShowMessage(string message)
    {
        Host?.ShowMessage($"[{Name}] {message}");
    }
}

// Sample plugin for demonstration
public class SampleMathPlugin : CalculatorPluginBase
{
    public override string Name => "Sample Math Plugin";
    public override string Version => "1.0.0";
    public override string Description => "Demonstrates plugin functionality with additional math functions";
    public override string Author => "MITS Calculator Team";

    public override Dictionary<string, Func<double[], double>> GetFunctions()
    {
        return new Dictionary<string, Func<double[], double>>
        {
            ["fibonacci"] = args => Fibonacci((int)args[0]),
            ["prime"] = args => IsPrime((int)args[0]) ? 1 : 0,
            ["gcd"] = args => GCD((int)args[0], (int)args[1]),
            ["lcm"] = args => LCM((int)args[0], (int)args[1])
        };
    }

    public override Control GetUI()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = "Sample Math Plugin", FontWeight = Avalonia.Media.FontWeight.Bold });
        panel.Children.Add(new TextBlock { Text = "Available functions: fibonacci(n), prime(n), gcd(a,b), lcm(a,b)" });
        
        var testButton = new Button { Content = "Test Functions" };
        testButton.Click += (s, e) =>
        {
            ShowMessage("Testing fibonacci(10) = " + Fibonacci(10));
            ShowMessage("Testing prime(17) = " + (IsPrime(17) ? "true" : "false"));
            ShowMessage("Testing gcd(48,18) = " + GCD(48, 18));
        };
        panel.Children.Add(testButton);
        
        return panel;
    }

    private double Fibonacci(int n)
    {
        if (n <= 1) return n;
        double a = 0, b = 1;
        for (int i = 2; i <= n; i++)
        {
            var temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }

    private bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    private double GCD(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private double LCM(int a, int b)
    {
        return (a * b) / GCD(a, b);
    }
}

public class PluginEventArgs : EventArgs
{
    public ICalculatorPlugin Plugin { get; }

    public PluginEventArgs(ICalculatorPlugin plugin)
    {
        Plugin = plugin;
    }
}

public class PluginInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string Author { get; set; } = "";
    public bool IsLoaded { get; set; }
}