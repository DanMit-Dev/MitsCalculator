using System;
using Avalonia.Controls;
using Avalonia.Layout;
using MITSCalculator.STEM;

namespace MITSCalculator.Views;

public class STEMSimulatorView : UserControl
{
    private readonly PhysicsSimulator _physicsSimulator;
    private readonly ChemistrySimulator _chemistrySimulator;

    public STEMSimulatorView()
    {
        _physicsSimulator = new PhysicsSimulator();
        _chemistrySimulator = new ChemistrySimulator();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        var mainPanel = new StackPanel
        {
            Spacing = 20,
            Margin = new Avalonia.Thickness(10)
        };

        // Physics Section
        var physicsSection = CreatePhysicsSection();
        mainPanel.Children.Add(physicsSection);

        // Chemistry Section  
        var chemistrySection = CreateChemistrySection();
        mainPanel.Children.Add(chemistrySection);

        Content = new ScrollViewer { Content = mainPanel };
    }

    private Control CreatePhysicsSection()
    {
        var section = new StackPanel { Spacing = 10 };
        
        section.Children.Add(new TextBlock 
        { 
            Text = "Physics Simulator", 
            FontSize = 18, 
            FontWeight = Avalonia.Media.FontWeight.Bold 
        });

        // Projectile Motion
        var projectileGroup = new StackPanel { Spacing = 5 };
        projectileGroup.Children.Add(new TextBlock { Text = "Projectile Motion:", FontWeight = Avalonia.Media.FontWeight.SemiBold });
        
        var projectileInputs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        projectileInputs.Children.Add(new TextBlock { Text = "Velocity:" });
        var velocityInput = new TextBox { Width = 80, Text = "50" };
        projectileInputs.Children.Add(velocityInput);
        
        projectileInputs.Children.Add(new TextBlock { Text = "Angle:" });
        var angleInput = new TextBox { Width = 80, Text = "45" };
        projectileInputs.Children.Add(angleInput);
        
        var projectileButton = new Button { Content = "Calculate" };
        projectileInputs.Children.Add(projectileButton);
        
        var projectileResult = new TextBlock { Text = "Results will appear here..." };
        
        projectileButton.Click += (s, e) =>
        {
            try
            {
                var velocity = double.Parse(velocityInput.Text ?? "0");
                var angle = double.Parse(angleInput.Text ?? "0");
                var result = PhysicsSimulator.ProjectileMotion.CalculateTrajectory(velocity, angle);
                
                projectileResult.Text = $"Range: {result.Range:F2}m, Max Height: {result.MaxHeight:F2}m, Time: {result.TimeOfFlight:F2}s";
            }
            catch (Exception ex)
            {
                projectileResult.Text = $"Error: {ex.Message}";
            }
        };
        
        projectileGroup.Children.Add(projectileInputs);
        projectileGroup.Children.Add(projectileResult);
        section.Children.Add(projectileGroup);

        // Wave Physics
        var waveGroup = new StackPanel { Spacing = 5 };
        waveGroup.Children.Add(new TextBlock { Text = "Wave Physics:", FontWeight = Avalonia.Media.FontWeight.SemiBold });
        
        var waveInputs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        waveInputs.Children.Add(new TextBlock { Text = "Frequency:" });
        var freqInput = new TextBox { Width = 80, Text = "440" };
        waveInputs.Children.Add(freqInput);
        
        waveInputs.Children.Add(new TextBlock { Text = "Wavelength:" });
        var wavelengthInput = new TextBox { Width = 80, Text = "0.77" };
        waveInputs.Children.Add(wavelengthInput);
        
        var waveButton = new Button { Content = "Calculate Speed" };
        waveInputs.Children.Add(waveButton);
        
        var waveResult = new TextBlock { Text = "Wave speed will appear here..." };
        
        waveButton.Click += (s, e) =>
        {
            try
            {
                var frequency = double.Parse(freqInput.Text ?? "0");
                var wavelength = double.Parse(wavelengthInput.Text ?? "0");
                var speed = PhysicsSimulator.WavePhysics.CalculateWaveSpeed(frequency, wavelength);
                
                waveResult.Text = $"Wave Speed: {speed:F2} m/s";
            }
            catch (Exception ex)
            {
                waveResult.Text = $"Error: {ex.Message}";
            }
        };
        
        waveGroup.Children.Add(waveInputs);
        waveGroup.Children.Add(waveResult);
        section.Children.Add(waveGroup);

        return section;
    }

    private Control CreateChemistrySection()
    {
        var section = new StackPanel { Spacing = 10 };
        
        section.Children.Add(new TextBlock 
        { 
            Text = "Chemistry Simulator", 
            FontSize = 18, 
            FontWeight = Avalonia.Media.FontWeight.Bold 
        });

        // Ideal Gas Law
        var gasGroup = new StackPanel { Spacing = 5 };
        gasGroup.Children.Add(new TextBlock { Text = "Ideal Gas Law (PV = nRT):", FontWeight = Avalonia.Media.FontWeight.SemiBold });
        
        var gasInputs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        gasInputs.Children.Add(new TextBlock { Text = "P:" });
        var pressureInput = new TextBox { Width = 80, Text = "1" };
        gasInputs.Children.Add(pressureInput);
        
        gasInputs.Children.Add(new TextBlock { Text = "V:" });
        var volumeInput = new TextBox { Width = 80, Text = "22.4" };
        gasInputs.Children.Add(volumeInput);
        
        gasInputs.Children.Add(new TextBlock { Text = "n:" });
        var molesInput = new TextBox { Width = 80, Text = "1" };
        gasInputs.Children.Add(molesInput);
        
        gasInputs.Children.Add(new TextBlock { Text = "T:" });
        var tempInput = new TextBox { Width = 80, Text = "" };
        gasInputs.Children.Add(tempInput);
        
        var gasButton = new Button { Content = "Calculate T" };
        gasInputs.Children.Add(gasButton);
        
        var gasResult = new TextBlock { Text = "Temperature will appear here..." };
        
        gasButton.Click += (s, e) =>
        {
            try
            {
                var pressure = double.Parse(pressureInput.Text ?? "0");
                var volume = double.Parse(volumeInput.Text ?? "0");
                var moles = double.Parse(molesInput.Text ?? "0");
                var temperature = ChemistrySimulator.GasLaws.IdealGasLaw(pressure, volume, moles);
                
                gasResult.Text = $"Temperature: {temperature:F2} K ({temperature - 273.15:F2} °C)";
            }
            catch (Exception ex)
            {
                gasResult.Text = $"Error: {ex.Message}";
            }
        };
        
        gasGroup.Children.Add(gasInputs);
        gasGroup.Children.Add(gasResult);
        section.Children.Add(gasGroup);

        // pH Calculator
        var pHGroup = new StackPanel { Spacing = 5 };
        pHGroup.Children.Add(new TextBlock { Text = "pH Calculator:", FontWeight = Avalonia.Media.FontWeight.SemiBold });
        
        var pHInputs = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        pHInputs.Children.Add(new TextBlock { Text = "[H+]:" });
        var hConcentration = new TextBox { Width = 120, Text = "1e-7" };
        pHInputs.Children.Add(hConcentration);
        
        var pHButton = new Button { Content = "Calculate pH" };
        pHInputs.Children.Add(pHButton);
        
        var pHResult = new TextBlock { Text = "pH will appear here..." };
        
        pHButton.Click += (s, e) =>
        {
            try
            {
                var concentration = double.Parse(hConcentration.Text ?? "0");
                var pH = ChemistrySimulator.AcidsAndBases.CalculatePH(concentration);
                
                var classification = pH < 7 ? "Acidic" : pH > 7 ? "Basic" : "Neutral";
                pHResult.Text = $"pH: {pH:F2} ({classification})";
            }
            catch (Exception ex)
            {
                pHResult.Text = $"Error: {ex.Message}";
            }
        };
        
        pHGroup.Children.Add(pHInputs);
        pHGroup.Children.Add(pHResult);
        section.Children.Add(pHGroup);

        return section;
    }
}