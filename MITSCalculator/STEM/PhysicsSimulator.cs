using System;
using System.Collections.Generic;
using System.Numerics;

namespace MITSCalculator.STEM;

public class PhysicsSimulator
{
    // Classical Mechanics
    public class ProjectileMotion
    {
        public static ProjectileResult CalculateTrajectory(double initialVelocity, double angle, 
            double initialHeight = 0, double gravity = 9.81)
        {
            var angleRad = angle * Math.PI / 180;
            var vx = initialVelocity * Math.Cos(angleRad);
            var vy = initialVelocity * Math.Sin(angleRad);

            // Time of flight
            var timeOfFlight = (vy + Math.Sqrt(vy * vy + 2 * gravity * initialHeight)) / gravity;
            
            // Maximum height
            var maxHeight = initialHeight + (vy * vy) / (2 * gravity);
            
            // Range
            var range = vx * timeOfFlight;
            
            // Generate trajectory points
            var trajectoryPoints = new List<(double x, double y)>();
            var timeStep = timeOfFlight / 100;
            
            for (double t = 0; t <= timeOfFlight; t += timeStep)
            {
                var x = vx * t;
                var y = initialHeight + vy * t - 0.5 * gravity * t * t;
                if (y >= 0) trajectoryPoints.Add((x, y));
            }

            return new ProjectileResult
            {
                TimeOfFlight = timeOfFlight,
                MaxHeight = maxHeight,
                Range = range,
                TrajectoryPoints = trajectoryPoints.ToArray()
            };
        }
    }

    public class PendulumSimulation
    {
        public static PendulumResult SimplePendulum(double length, double amplitude, double gravity = 9.81)
        {
            var period = 2 * Math.PI * Math.Sqrt(length / gravity);
            var frequency = 1 / period;
            var angularFrequency = 2 * Math.PI * frequency;

            return new PendulumResult
            {
                Period = period,
                Frequency = frequency,
                AngularFrequency = angularFrequency,
                MaxVelocity = amplitude * angularFrequency,
                MaxAcceleration = amplitude * angularFrequency * angularFrequency
            };
        }

        public static (double angle, double velocity)[] SimulateMotion(double length, double initialAngle, 
            double timeStep = 0.01, double duration = 10, double gravity = 9.81)
        {
            var results = new List<(double, double)>();
            var angle = initialAngle * Math.PI / 180; // Convert to radians
            var velocity = 0.0;
            var angularFrequency = Math.Sqrt(gravity / length);

            for (double t = 0; t <= duration; t += timeStep)
            {
                results.Add((angle * 180 / Math.PI, velocity)); // Convert back to degrees
                
                // Small angle approximation: θ'' = -ω²θ
                var acceleration = -angularFrequency * angularFrequency * angle;
                velocity += acceleration * timeStep;
                angle += velocity * timeStep;
            }

            return results.ToArray();
        }
    }

    public class WavePhysics
    {
        public static double[] GenerateWaveform(WaveType type, double amplitude, double frequency, 
            double phase, double duration, int sampleRate = 1000)
        {
            var samples = (int)(duration * sampleRate);
            var waveform = new double[samples];
            var angularFreq = 2 * Math.PI * frequency;

            for (int i = 0; i < samples; i++)
            {
                var time = (double)i / sampleRate;
                var argument = angularFreq * time + phase;

                waveform[i] = type switch
                {
                    WaveType.Sine => amplitude * Math.Sin(argument),
                    WaveType.Cosine => amplitude * Math.Cos(argument),
                    WaveType.Square => amplitude * Math.Sign(Math.Sin(argument)),
                    WaveType.Triangle => amplitude * (2 / Math.PI) * Math.Asin(Math.Sin(argument)),
                    WaveType.Sawtooth => amplitude * (2 / Math.PI) * (argument % (2 * Math.PI) - Math.PI),
                    _ => 0
                };
            }

            return waveform;
        }

        public static double CalculateWaveSpeed(double frequency, double wavelength)
        {
            return frequency * wavelength;
        }

        public static double CalculateResonantFrequency(double length, double waveSpeed, int harmonicNumber = 1)
        {
            return harmonicNumber * waveSpeed / (2 * length);
        }
    }

    // Thermodynamics
    public class Thermodynamics
    {
        public static double IdealGasLaw(double pressure = double.NaN, double volume = double.NaN, 
            double moles = double.NaN, double temperature = double.NaN, double gasConstant = 8.314)
        {
            // PV = nRT, solve for the missing variable
            if (double.IsNaN(pressure))
                return moles * gasConstant * temperature / volume;
            if (double.IsNaN(volume))
                return moles * gasConstant * temperature / pressure;
            if (double.IsNaN(moles))
                return pressure * volume / (gasConstant * temperature);
            if (double.IsNaN(temperature))
                return pressure * volume / (moles * gasConstant);
            
            return double.NaN; // All variables provided
        }

        public static double CarnoeEfficiency(double hotTemperature, double coldTemperature)
        {
            return 1 - coldTemperature / hotTemperature;
        }

        public static double HeatCapacity(double mass, double specificHeat, double temperatureChange)
        {
            return mass * specificHeat * temperatureChange;
        }
    }

    // Electromagnetism
    public class Electromagnetism
    {
        public static double CoulombsLaw(double charge1, double charge2, double distance, 
            double permittivity = 8.854e-12)
        {
            var k = 1 / (4 * Math.PI * permittivity);
            return k * Math.Abs(charge1 * charge2) / (distance * distance);
        }

        public static double OhmsLaw(double voltage = double.NaN, double current = double.NaN, 
            double resistance = double.NaN)
        {
            if (double.IsNaN(voltage))
                return current * resistance;
            if (double.IsNaN(current))
                return voltage / resistance;
            if (double.IsNaN(resistance))
                return voltage / current;
            
            return double.NaN;
        }

        public static double ElectricPower(double voltage = double.NaN, double current = double.NaN, 
            double resistance = double.NaN)
        {
            if (!double.IsNaN(voltage) && !double.IsNaN(current))
                return voltage * current;
            if (!double.IsNaN(voltage) && !double.IsNaN(resistance))
                return voltage * voltage / resistance;
            if (!double.IsNaN(current) && !double.IsNaN(resistance))
                return current * current * resistance;
            
            return double.NaN;
        }

        public static double MagneticField(double current, double distance, double permeability = 4e-7 * Math.PI)
        {
            return permeability * current / (2 * Math.PI * distance);
        }
    }

    // Quantum Mechanics (simplified)
    public class QuantumMechanics
    {
        private const double PlanckConstant = 6.62607015e-34;
        private const double SpeedOfLight = 299792458;

        public static double PhotonEnergy(double frequency)
        {
            return PlanckConstant * frequency;
        }

        public static double PhotonEnergyFromWavelength(double wavelength)
        {
            var frequency = SpeedOfLight / wavelength;
            return PhotonEnergy(frequency);
        }

        public static double DeBroglieWavelength(double momentum)
        {
            return PlanckConstant / momentum;
        }

        public static double BohrRadius(int n = 1, double reducedMass = 9.1094e-31)
        {
            var bohrRadius = 5.29177e-11; // meters
            return n * n * bohrRadius;
        }

        public static double HydrogenEnergyLevel(int n)
        {
            var rydbergEnergy = 13.6; // eV
            return -rydbergEnergy / (n * n);
        }

        public static double UncertaintyPrinciple(double positionUncertainty)
        {
            var reducedPlanck = PlanckConstant / (2 * Math.PI);
            return reducedPlanck / (2 * positionUncertainty);
        }
    }

    // Relativity
    public class Relativity
    {
        private const double SpeedOfLight = 299792458; // m/s

        public static double LorentzFactor(double velocity)
        {
            var beta = velocity / SpeedOfLight;
            return 1 / Math.Sqrt(1 - beta * beta);
        }

        public static double TimeDilation(double properTime, double velocity)
        {
            return properTime * LorentzFactor(velocity);
        }

        public static double LengthContraction(double properLength, double velocity)
        {
            return properLength / LorentzFactor(velocity);
        }

        public static double RelativisticMomentum(double restMass, double velocity)
        {
            return LorentzFactor(velocity) * restMass * velocity;
        }

        public static double MassEnergyEquivalence(double mass)
        {
            return mass * SpeedOfLight * SpeedOfLight;
        }
    }
}

// Result classes
public class ProjectileResult
{
    public double TimeOfFlight { get; set; }
    public double MaxHeight { get; set; }
    public double Range { get; set; }
    public (double x, double y)[] TrajectoryPoints { get; set; } = Array.Empty<(double, double)>();
}

public class PendulumResult
{
    public double Period { get; set; }
    public double Frequency { get; set; }
    public double AngularFrequency { get; set; }
    public double MaxVelocity { get; set; }
    public double MaxAcceleration { get; set; }
}

public enum WaveType
{
    Sine, Cosine, Square, Triangle, Sawtooth
}