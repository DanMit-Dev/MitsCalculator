using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace MITSCalculator.Statistics;

public class StatisticalEngine
{
    private readonly Random _random = new();

    // Descriptive Statistics
    public StatisticalSummary CalculateDescriptiveStats(double[] data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty");

        return new StatisticalSummary
        {
            Count = data.Length,
            Mean = data.Mean(),
            Median = data.Median(),
            Mode = CalculateMode(data),
            StandardDeviation = data.StandardDeviation(),
            Variance = data.Variance(),
            Skewness = data.Skewness(),
            Kurtosis = data.Kurtosis(),
            Minimum = data.Minimum(),
            Maximum = data.Maximum(),
            Range = data.Maximum() - data.Minimum(),
            Q1 = data.LowerQuartile(),
            Q3 = data.UpperQuartile(),
            IQR = data.UpperQuartile() - data.LowerQuartile()
        };
    }

    private double[] CalculateMode(double[] data)
    {
        var frequency = data.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var maxFrequency = frequency.Values.Max();
        return frequency.Where(kv => kv.Value == maxFrequency).Select(kv => kv.Key).ToArray();
    }

    // Add CalculateDescriptiveStatistics method
    public DescriptiveStatistics CalculateDescriptiveStatistics(double[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Values array cannot be null or empty");

        var sorted = values.OrderBy(x => x).ToArray();

        return new DescriptiveStatistics
        {
            Mean = values.Average(),
            Median = CalculateMedian(sorted),
            StandardDeviation = CalculateStandardDeviation(values),
            Variance = CalculateVariance(values),
            Min = values.Min(),
            Max = values.Max(),
            Count = values.Length
        };
    }

    private double CalculateMedian(double[] sortedValues)
    {
        int n = sortedValues.Length;
        if (n % 2 == 0)
            return (sortedValues[n / 2 - 1] + sortedValues[n / 2]) / 2.0;
        else
            return sortedValues[n / 2];
    }

    private double CalculateStandardDeviation(double[] values)
    {
        return Math.Sqrt(CalculateVariance(values));
    }

    private double CalculateVariance(double[] values)
    {
        double mean = values.Average();
        return values.Select(x => Math.Pow(x - mean, 2)).Average();
    }

    // Probability Distributions
    public double NormalPdf(double x, double mean = 0, double stdDev = 1)
    {
        var normal = new Normal(mean, stdDev);
        return normal.Density(x);
    }

    public double NormalCdf(double x, double mean = 0, double stdDev = 1)
    {
        var normal = new Normal(mean, stdDev);
        return normal.CumulativeDistribution(x);
    }

    public double NormalInverse(double probability, double mean = 0, double stdDev = 1)
    {
        var normal = new Normal(mean, stdDev);
        return normal.InverseCumulativeDistribution(probability);
    }

    public double TDistributionPdf(double x, double degreesOfFreedom)
    {
        var t = new StudentT(0, 1, degreesOfFreedom);
        return t.Density(x);
    }

    public double TDistributionCdf(double x, double degreesOfFreedom)
    {
        var t = new StudentT(0, 1, degreesOfFreedom);
        return t.CumulativeDistribution(x);
    }

    public double ChiSquaredPdf(double x, double degreesOfFreedom)
    {
        var chi = new ChiSquared(degreesOfFreedom);
        return chi.Density(x);
    }

    public double ChiSquaredCdf(double x, double degreesOfFreedom)
    {
        var chi = new ChiSquared(degreesOfFreedom);
        return chi.CumulativeDistribution(x);
    }

    public double BinomialPmf(int k, int n, double p)
    {
        var binomial = new Binomial(p, n);
        return binomial.Probability(k);
    }

    public double PoissonPmf(int k, double lambda)
    {
        var poisson = new Poisson(lambda);
        return poisson.Probability(k);
    }

    // Hypothesis Testing
    public TTestResult OneSampleTTest(double[] sample, double populationMean, double alpha = 0.05)
    {
        var n = sample.Length;
        var sampleMean = sample.Mean();
        var sampleStdDev = sample.StandardDeviation();
        var standardError = sampleStdDev / Math.Sqrt(n);

        var tStatistic = (sampleMean - populationMean) / standardError;
        var degreesOfFreedom = n - 1;

        var tDist = new StudentT(0, 1, degreesOfFreedom);
        var pValue = 2 * (1 - tDist.CumulativeDistribution(Math.Abs(tStatistic)));

        var criticalValue = tDist.InverseCumulativeDistribution(1 - alpha / 2);
        var rejectNull = Math.Abs(tStatistic) > criticalValue;

        return new TTestResult
        {
            TStatistic = tStatistic,
            PValue = pValue,
            DegreesOfFreedom = degreesOfFreedom,
            CriticalValue = criticalValue,
            RejectNull = rejectNull,
            Alpha = alpha,
            SampleMean = sampleMean,
            PopulationMean = populationMean,
            StandardError = standardError
        };
    }

    public TTestResult TwoSampleTTest(double[] sample1, double[] sample2, bool equalVariances = true, double alpha = 0.05)
    {
        var n1 = sample1.Length;
        var n2 = sample2.Length;
        var mean1 = sample1.Mean();
        var mean2 = sample2.Mean();
        var var1 = sample1.Variance();
        var var2 = sample2.Variance();

        double tStatistic, degreesOfFreedom, standardError;

        if (equalVariances)
        {
            // Pooled variance t-test
            var pooledVariance = ((n1 - 1) * var1 + (n2 - 1) * var2) / (n1 + n2 - 2);
            standardError = Math.Sqrt(pooledVariance * (1.0 / n1 + 1.0 / n2));
            degreesOfFreedom = n1 + n2 - 2;
        }
        else
        {
            // Welch's t-test
            standardError = Math.Sqrt(var1 / n1 + var2 / n2);
            degreesOfFreedom = Math.Pow(var1 / n1 + var2 / n2, 2) /
                              (Math.Pow(var1 / n1, 2) / (n1 - 1) + Math.Pow(var2 / n2, 2) / (n2 - 1));
        }

        tStatistic = (mean1 - mean2) / standardError;

        var tDist = new StudentT(0, 1, degreesOfFreedom);
        var pValue = 2 * (1 - tDist.CumulativeDistribution(Math.Abs(tStatistic)));
        var criticalValue = tDist.InverseCumulativeDistribution(1 - alpha / 2);
        var rejectNull = Math.Abs(tStatistic) > criticalValue;

        return new TTestResult
        {
            TStatistic = tStatistic,
            PValue = pValue,
            DegreesOfFreedom = degreesOfFreedom,
            CriticalValue = criticalValue,
            RejectNull = rejectNull,
            Alpha = alpha,
            SampleMean = mean1,
            PopulationMean = mean2,
            StandardError = standardError
        };
    }

    public ANOVAResult OneWayANOVA(double[][] groups, double alpha = 0.05)
    {
        var allData = groups.SelectMany(g => g).ToArray();
        var grandMean = allData.Mean();
        var totalN = allData.Length;
        var k = groups.Length; // number of groups

        // Calculate group means and sizes
        var groupMeans = groups.Select(g => g.Mean()).ToArray();
        var groupSizes = groups.Select(g => g.Length).ToArray();

        // Sum of squares between groups (SSB)
        var ssb = groups.Select((g, i) => groupSizes[i] * Math.Pow(groupMeans[i] - grandMean, 2)).Sum();

        // Sum of squares within groups (SSW)
        var ssw = groups.Select(g => g.Select(x => Math.Pow(x - g.Mean(), 2)).Sum()).Sum();

        // Degrees of freedom
        var dfBetween = k - 1;
        var dfWithin = totalN - k;

        // Mean squares
        var msBetween = ssb / dfBetween;
        var msWithin = ssw / dfWithin;

        // F-statistic
        var fStatistic = msBetween / msWithin;

        // P-value
        var fDist = new FisherSnedecor(dfBetween, dfWithin);
        var pValue = 1 - fDist.CumulativeDistribution(fStatistic);

        var criticalValue = fDist.InverseCumulativeDistribution(1 - alpha);
        var rejectNull = fStatistic > criticalValue;

        return new ANOVAResult
        {
            FStatistic = fStatistic,
            PValue = pValue,
            DfBetween = dfBetween,
            DfWithin = dfWithin,
            SSBetween = ssb,
            SSWithin = ssw,
            MSBetween = msBetween,
            MSWithin = msWithin,
            CriticalValue = criticalValue,
            RejectNull = rejectNull,
            Alpha = alpha
        };
    }

    // Regression Analysis
    public LinearRegressionResult LinearRegression(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("X and Y arrays must have the same length");

        var n = x.Length;
        var xMean = x.Mean();
        var yMean = y.Mean();

        // Calculate slope (β₁)
        var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        var denominator = x.Select(xi => Math.Pow(xi - xMean, 2)).Sum();
        var slope = numerator / denominator;

        // Calculate intercept (β₀)
        var intercept = yMean - slope * xMean;

        // Calculate R-squared
        var yPredicted = x.Select(xi => intercept + slope * xi).ToArray();
        var ssTot = y.Select(yi => Math.Pow(yi - yMean, 2)).Sum();
        var ssRes = y.Zip(yPredicted, (yi, ypi) => Math.Pow(yi - ypi, 2)).Sum();
        var rSquared = 1 - (ssRes / ssTot);

        // Calculate correlation coefficient
        var correlation = Math.Sqrt(rSquared) * Math.Sign(slope);

        // Standard error of the estimate
        var standardError = Math.Sqrt(ssRes / (n - 2));

        return new LinearRegressionResult
        {
            Slope = slope,
            Intercept = intercept,
            RSquared = rSquared,
            Correlation = correlation,
            StandardError = standardError,
            ResidualSumOfSquares = ssRes,
            TotalSumOfSquares = ssTot
        };
    }

    // Monte Carlo Simulation
    public double[] MonteCarloSimulation(Func<double> randomVariableGenerator, int iterations)
    {
        var results = new double[iterations];
        for (int i = 0; i < iterations; i++)
        {
            results[i] = randomVariableGenerator();
        }
        return results;
    }

    public double MonteCarloIntegration(Func<double, double> function, double a, double b, int samples = 100000)
    {
        var sum = 0.0;
        for (int i = 0; i < samples; i++)
        {
            var x = a + _random.NextDouble() * (b - a);
            sum += function(x);
        }
        return (b - a) * sum / samples;
    }

    // Bootstrap Sampling
    public double[] Bootstrap(double[] data, Func<double[], double> statistic, int bootstrapSamples = 1000)
    {
        var results = new double[bootstrapSamples];
        var n = data.Length;

        for (int i = 0; i < bootstrapSamples; i++)
        {
            var sample = new double[n];
            for (int j = 0; j < n; j++)
            {
                sample[j] = data[_random.Next(n)];
            }
            results[i] = statistic(sample);
        }

        return results;
    }

    public ConfidenceInterval BootstrapConfidenceInterval(double[] data, Func<double[], double> statistic, 
        double confidenceLevel = 0.95, int bootstrapSamples = 1000)
    {
        var bootstrapStats = Bootstrap(data, statistic, bootstrapSamples);
        Array.Sort(bootstrapStats);

        var alpha = 1 - confidenceLevel;
        var lowerIndex = (int)(alpha / 2 * bootstrapSamples);
        var upperIndex = (int)((1 - alpha / 2) * bootstrapSamples);

        return new ConfidenceInterval
        {
            LowerBound = bootstrapStats[lowerIndex],
            UpperBound = bootstrapStats[upperIndex],
            ConfidenceLevel = confidenceLevel,
            Statistic = statistic(data)
        };
    }
}

// Result classes
public class StatisticalSummary
{
    public int Count { get; set; }
    public double Mean { get; set; }
    public double Median { get; set; }
    public double[] Mode { get; set; } = Array.Empty<double>();
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public double Skewness { get; set; }
    public double Kurtosis { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double Range { get; set; }
    public double Q1 { get; set; }
    public double Q3 { get; set; }
    public double IQR { get; set; }
}

// Define DescriptiveStatistics class
public class DescriptiveStatistics
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public int Count { get; set; }
}

public class TTestResult
{
    public double TStatistic { get; set; }
    public double PValue { get; set; }
    public double DegreesOfFreedom { get; set; }
    public double CriticalValue { get; set; }
    public bool RejectNull { get; set; }
    public double Alpha { get; set; }
    public double SampleMean { get; set; }
    public double PopulationMean { get; set; }
    public double StandardError { get; set; }
}

public class ANOVAResult
{
    public double FStatistic { get; set; }
    public double PValue { get; set; }
    public double DfBetween { get; set; }
    public double DfWithin { get; set; }
    public double SSBetween { get; set; }
    public double SSWithin { get; set; }
    public double MSBetween { get; set; }
    public double MSWithin { get; set; }
    public double CriticalValue { get; set; }
    public bool RejectNull { get; set; }
    public double Alpha { get; set; }
}

public class LinearRegressionResult
{
    public double Slope { get; set; }
    public double Intercept { get; set; }
    public double RSquared { get; set; }
    public double Correlation { get; set; }
    public double StandardError { get; set; }
    public double ResidualSumOfSquares { get; set; }
    public double TotalSumOfSquares { get; set; }
}

public class ConfidenceInterval
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double ConfidenceLevel { get; set; }
    public double Statistic { get; set; }
}