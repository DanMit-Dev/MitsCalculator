using System;
using System.Collections.Generic;
using System.Linq;

namespace MITSCalculator.STEM;

public class ChemistrySimulator
{
    private static readonly Dictionary<string, AtomicData> PeriodicTable = InitializePeriodicTable();

    // Chemical Equations and Stoichiometry
    public class Stoichiometry
    {
        public static double CalculateMoles(double mass, double molarMass)
        {
            return mass / molarMass;
        }

        public static double CalculateMass(double moles, double molarMass)
        {
            return moles * molarMass;
        }

        public static double CalculateMolarity(double moles, double volumeLiters)
        {
            return moles / volumeLiters;
        }

        public static double CalculateConcentration(double moles, double volumeLiters)
        {
            return moles / volumeLiters;
        }

        public static StoichiometryResult BalanceEquation(string[] reactants, string[] products, 
            double[] reactantCoefficients)
        {
            // Simplified stoichiometry calculation
            var result = new StoichiometryResult
            {
                ReactantMoles = new double[reactants.Length],
                ProductMoles = new double[products.Length],
                LimitingReagent = "",
                TheoreticalYield = 0
            };

            for (int i = 0; i < reactants.Length; i++)
            {
                result.ReactantMoles[i] = reactantCoefficients[i];
            }

            // Find limiting reagent (simplified)
            var minRatio = result.ReactantMoles.Min();
            var limitingIndex = Array.IndexOf(result.ReactantMoles, minRatio);
            result.LimitingReagent = reactants[limitingIndex];

            return result;
        }
    }

    // Gas Laws
    public class GasLaws
    {
        private const double GasConstant = 0.08206; // L·atm/(mol·K)

        public static double IdealGasLaw(double pressure = double.NaN, double volume = double.NaN,
            double moles = double.NaN, double temperature = double.NaN)
        {
            if (double.IsNaN(pressure))
                return moles * GasConstant * temperature / volume;
            if (double.IsNaN(volume))
                return moles * GasConstant * temperature / pressure;
            if (double.IsNaN(moles))
                return pressure * volume / (GasConstant * temperature);
            if (double.IsNaN(temperature))
                return pressure * volume / (moles * GasConstant);

            return double.NaN;
        }

        public static double CombinedGasLaw(double p1, double v1, double t1, 
            double p2 = double.NaN, double v2 = double.NaN, double t2 = double.NaN)
        {
            // P1V1/T1 = P2V2/T2
            if (double.IsNaN(p2))
                return p1 * v1 * t2 / (v2 * t1);
            if (double.IsNaN(v2))
                return p1 * v1 * t2 / (p2 * t1);
            if (double.IsNaN(t2))
                return p1 * v1 * t1 / (p2 * v2);

            return double.NaN;
        }

        public static double VanDerWaalsEquation(double moles, double temperature, double volume,
            double a, double b)
        {
            // (P + a(n/V)²)(V - nb) = nRT
            var correction1 = a * moles * moles / (volume * volume);
            var correction2 = moles * b;
            return (moles * GasConstant * temperature / (volume - correction2)) - correction1;
        }
    }

    // Thermochemistry
    public class Thermochemistry
    {
        public static double CalculateEnthalpy(double[] heatsOfFormation, double[] stoichiometricCoefficients)
        {
            double deltaH = 0;
            for (int i = 0; i < heatsOfFormation.Length; i++)
            {
                deltaH += stoichiometricCoefficients[i] * heatsOfFormation[i];
            }
            return deltaH;
        }

        public static double HessLaw(double[] stepEnthalpies)
        {
            return stepEnthalpies.Sum();
        }

        public static double CalorimeterHeat(double mass, double specificHeat, double temperatureChange)
        {
            return mass * specificHeat * temperatureChange;
        }

        public static double BondEnergy(double[] bondEnergiesBroken, double[] bondEnergiesFormed)
        {
            return bondEnergiesBroken.Sum() - bondEnergiesFormed.Sum();
        }
    }

    // Chemical Kinetics
    public class Kinetics
    {
        public static double FirstOrderRate(double initialConcentration, double rateConstant, double time)
        {
            return initialConcentration * Math.Exp(-rateConstant * time);
        }

        public static double SecondOrderRate(double initialConcentration, double rateConstant, double time)
        {
            return initialConcentration / (1 + rateConstant * initialConcentration * time);
        }

        public static double HalfLife(double rateConstant, int order, double initialConcentration = 1.0)
        {
            return order switch
            {
                1 => Math.Log(2) / rateConstant,
                2 => 1 / (rateConstant * initialConcentration),
                0 => initialConcentration / (2 * rateConstant),
                _ => double.NaN
            };
        }

        public static double ArrheniusEquation(double preExponentialFactor, double activationEnergy,
            double temperature, double gasConstant = 8.314)
        {
            return preExponentialFactor * Math.Exp(-activationEnergy / (gasConstant * temperature));
        }
    }

    // Chemical Equilibrium
    public class Equilibrium
    {
        public static double EquilibriumConstant(double[] productConcentrations, double[] reactantConcentrations,
            double[] productCoefficients, double[] reactantCoefficients)
        {
            double numerator = 1.0;
            double denominator = 1.0;

            for (int i = 0; i < productConcentrations.Length; i++)
            {
                numerator *= Math.Pow(productConcentrations[i], productCoefficients[i]);
            }

            for (int i = 0; i < reactantConcentrations.Length; i++)
            {
                denominator *= Math.Pow(reactantConcentrations[i], reactantCoefficients[i]);
            }

            return numerator / denominator;
        }

        public static double IceTableCalculation(double initialConcentration, double changeAmount)
        {
            return initialConcentration + changeAmount;
        }

        public static double LeChatelier(double equilibriumConstant, double temperature1, double temperature2,
            double deltaH, double gasConstant = 8.314)
        {
            // Van't Hoff equation
            var factor = deltaH / gasConstant * (1 / temperature2 - 1 / temperature1);
            return equilibriumConstant * Math.Exp(factor);
        }
    }

    // Acids and Bases
    public class AcidsAndBases
    {
        public static double CalculatePH(double hydrogenConcentration)
        {
            return -Math.Log10(hydrogenConcentration);
        }

        public static double CalculatePOH(double hydroxideConcentration)
        {
            return -Math.Log10(hydroxideConcentration);
        }

        public static double HendersonHasselbalch(double pKa, double baseConcentration, double acidConcentration)
        {
            return pKa + Math.Log10(baseConcentration / acidConcentration);
        }

        public static BufferResult CalculateBuffer(double weakAcidConcentration, double conjugateBaseConcentration,
            double pKa, double addedAcid = 0, double addedBase = 0)
        {
            var newAcidConc = weakAcidConcentration + addedAcid - addedBase;
            var newBaseConc = conjugateBaseConcentration - addedAcid + addedBase;

            var pH = pKa + Math.Log10(newBaseConc / newAcidConc);

            return new BufferResult
            {
                pH = pH,
                NewAcidConcentration = newAcidConc,
                NewBaseConcentration = newBaseConc,
                BufferCapacity = Math.Min(newAcidConc, newBaseConc)
            };
        }
    }

    // Electrochemistry
    public class Electrochemistry
    {
        private const double FaradayConstant = 96485; // C/mol

        public static double NernstEquation(double standardPotential, double temperature, int electronsTransferred,
            double reactionQuotient, double gasConstant = 8.314)
        {
            return standardPotential - (gasConstant * temperature / (electronsTransferred * FaradayConstant)) 
                   * Math.Log(reactionQuotient);
        }

        public static double CellPotential(double cathodeReduction, double anodeReduction)
        {
            return cathodeReduction - anodeReduction;
        }

        public static ElectrolysisResult Electrolysis(double current, double time, int electronsPerMole,
            double molarMass)
        {
            var totalCharge = current * time;
            var molesElectrons = totalCharge / FaradayConstant;
            var molesProduct = molesElectrons / electronsPerMole;
            var massProduct = molesProduct * molarMass;

            return new ElectrolysisResult
            {
                MolesProducted = molesProduct,
                MassProduced = massProduct,
                ChargeTransferred = totalCharge
            };
        }
    }

    // Atomic Structure
    public class AtomicStructure
    {
        public static int GetAtomicNumber(string element)
        {
            return PeriodicTable.GetValueOrDefault(element)?.AtomicNumber ?? 0;
        }

        public static double GetAtomicMass(string element)
        {
            return PeriodicTable.GetValueOrDefault(element)?.AtomicMass ?? 0;
        }

        public static string GetElectronConfiguration(int atomicNumber)
        {
            var shells = new[] { "1s", "2s", "2p", "3s", "3p", "4s", "3d", "4p", "5s", "4d", "5p", "6s", "4f", "5d", "6p" };
            var maxElectrons = new[] { 2, 2, 6, 2, 6, 2, 10, 6, 2, 10, 6, 2, 14, 10, 6 };

            var config = "";
            var remainingElectrons = atomicNumber;

            for (int i = 0; i < shells.Length && remainingElectrons > 0; i++)
            {
                var electronsInShell = Math.Min(remainingElectrons, maxElectrons[i]);
                config += $"{shells[i]}{electronsInShell} ";
                remainingElectrons -= electronsInShell;
            }

            return config.Trim();
        }
    }

    private static Dictionary<string, AtomicData> InitializePeriodicTable()
    {
        return new Dictionary<string, AtomicData>
        {
            ["H"] = new AtomicData { AtomicNumber = 1, AtomicMass = 1.008, Symbol = "H", Name = "Hydrogen" },
            ["He"] = new AtomicData { AtomicNumber = 2, AtomicMass = 4.003, Symbol = "He", Name = "Helium" },
            ["Li"] = new AtomicData { AtomicNumber = 3, AtomicMass = 6.941, Symbol = "Li", Name = "Lithium" },
            ["Be"] = new AtomicData { AtomicNumber = 4, AtomicMass = 9.012, Symbol = "Be", Name = "Beryllium" },
            ["B"] = new AtomicData { AtomicNumber = 5, AtomicMass = 10.811, Symbol = "B", Name = "Boron" },
            ["C"] = new AtomicData { AtomicNumber = 6, AtomicMass = 12.011, Symbol = "C", Name = "Carbon" },
            ["N"] = new AtomicData { AtomicNumber = 7, AtomicMass = 14.007, Symbol = "N", Name = "Nitrogen" },
            ["O"] = new AtomicData { AtomicNumber = 8, AtomicMass = 15.999, Symbol = "O", Name = "Oxygen" },
            ["F"] = new AtomicData { AtomicNumber = 9, AtomicMass = 18.998, Symbol = "F", Name = "Fluorine" },
            ["Ne"] = new AtomicData { AtomicNumber = 10, AtomicMass = 20.180, Symbol = "Ne", Name = "Neon" }
            // Add more elements as needed
        };
    }
}

// Data structures
public class AtomicData
{
    public int AtomicNumber { get; set; }
    public double AtomicMass { get; set; }
    public string Symbol { get; set; } = "";
    public string Name { get; set; } = "";
}

public class StoichiometryResult
{
    public double[] ReactantMoles { get; set; } = Array.Empty<double>();
    public double[] ProductMoles { get; set; } = Array.Empty<double>();
    public string LimitingReagent { get; set; } = "";
    public double TheoreticalYield { get; set; }
}

public class BufferResult
{
    public double pH { get; set; }
    public double NewAcidConcentration { get; set; }
    public double NewBaseConcentration { get; set; }
    public double BufferCapacity { get; set; }
}

public class ElectrolysisResult
{
    public double MolesProducted { get; set; }
    public double MassProduced { get; set; }
    public double ChargeTransferred { get; set; }
}