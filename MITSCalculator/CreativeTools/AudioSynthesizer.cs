using System;
using System.Collections.Generic;
using System.Linq;

namespace MITSCalculator.CreativeTools;

public class AudioSynthesizer
{
    private readonly int _sampleRate;
    private readonly Random _random = new();

    public AudioSynthesizer(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;
    }

    // Basic Waveforms
    public double[] GenerateSineWave(double frequency, double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            var time = (double)i / _sampleRate;
            wave[i] = amplitude * Math.Sin(2 * Math.PI * frequency * time);
        }

        return wave;
    }

    public double[] GenerateSquareWave(double frequency, double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            var time = (double)i / _sampleRate;
            var sineValue = Math.Sin(2 * Math.PI * frequency * time);
            wave[i] = amplitude * Math.Sign(sineValue);
        }

        return wave;
    }

    public double[] GenerateSawtoothWave(double frequency, double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];
        var period = _sampleRate / frequency;

        for (int i = 0; i < samples; i++)
        {
            var position = i % period;
            wave[i] = amplitude * (2.0 * position / period - 1.0);
        }

        return wave;
    }

    public double[] GenerateTriangleWave(double frequency, double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];
        var period = _sampleRate / frequency;

        for (int i = 0; i < samples; i++)
        {
            var position = i % period;
            var normalizedPos = position / period;

            if (normalizedPos <= 0.5)
                wave[i] = amplitude * (4.0 * normalizedPos - 1.0);
            else
                wave[i] = amplitude * (3.0 - 4.0 * normalizedPos);
        }

        return wave;
    }

    public double[] GenerateNoiseWave(double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            wave[i] = amplitude * (2.0 * _random.NextDouble() - 1.0);
        }

        return wave;
    }

    // Mathematical Function-Based Synthesis
    public double[] GenerateFromFunction(Func<double, double> function, double duration, 
        double timeScale = 1.0, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            var time = (double)i / _sampleRate * timeScale;
            try
            {
                var value = function(time);
                wave[i] = amplitude * Math.Max(-1.0, Math.Min(1.0, value)); // Clamp to [-1, 1]
            }
            catch
            {
                wave[i] = 0; // Silence on mathematical errors
            }
        }

        return wave;
    }

    // Harmonic Series
    public double[] GenerateHarmonicSeries(double fundamental, double duration, 
        double[] harmonicAmplitudes, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int harmonic = 0; harmonic < harmonicAmplitudes.Length; harmonic++)
        {
            var frequency = fundamental * (harmonic + 1);
            var harmonicAmplitude = harmonicAmplitudes[harmonic];

            for (int i = 0; i < samples; i++)
            {
                var time = (double)i / _sampleRate;
                wave[i] += amplitude * harmonicAmplitude * Math.Sin(2 * Math.PI * frequency * time);
            }
        }

        // Normalize
        var maxValue = wave.Max(Math.Abs);
        if (maxValue > 0)
        {
            for (int i = 0; i < samples; i++)
            {
                wave[i] /= maxValue;
            }
        }

        return wave;
    }

    // AM Synthesis (Amplitude Modulation)
    public double[] GenerateAMSynthesis(double carrierFreq, double modulatorFreq, 
        double duration, double modulationDepth = 0.5, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            var time = (double)i / _sampleRate;
            var carrier = Math.Sin(2 * Math.PI * carrierFreq * time);
            var modulator = Math.Sin(2 * Math.PI * modulatorFreq * time);
            wave[i] = amplitude * carrier * (1 + modulationDepth * modulator);
        }

        return wave;
    }

    // FM Synthesis (Frequency Modulation)
    public double[] GenerateFMSynthesis(double carrierFreq, double modulatorFreq, 
        double duration, double modulationIndex, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        for (int i = 0; i < samples; i++)
        {
            var time = (double)i / _sampleRate;
            var modulator = Math.Sin(2 * Math.PI * modulatorFreq * time);
            var instantaneousFreq = carrierFreq + modulationIndex * modulatorFreq * modulator;
            
            // Integrate to get phase
            var phase = 2 * Math.PI * instantaneousFreq * time;
            wave[i] = amplitude * Math.Sin(phase);
        }

        return wave;
    }

    // Chord Generation
    public double[] GenerateChord(double[] frequencies, double duration, double amplitude = 1.0)
    {
        var samples = (int)(duration * _sampleRate);
        var wave = new double[samples];

        foreach (var freq in frequencies)
        {
            for (int i = 0; i < samples; i++)
            {
                var time = (double)i / _sampleRate;
                wave[i] += Math.Sin(2 * Math.PI * freq * time);
            }
        }

        // Normalize
        var maxValue = wave.Max(Math.Abs);
        if (maxValue > 0)
        {
            for (int i = 0; i < samples; i++)
            {
                wave[i] = amplitude * wave[i] / maxValue;
            }
        }

        return wave;
    }

    // ADSR Envelope
    public double[] ApplyADSREnvelope(double[] audio, double attackTime, double decayTime, 
        double sustainLevel, double releaseTime)
    {
        var result = new double[audio.Length];
        var attackSamples = (int)(attackTime * _sampleRate);
        var decaySamples = (int)(decayTime * _sampleRate);
        var releaseSamples = (int)(releaseTime * _sampleRate);
        var sustainSamples = audio.Length - attackSamples - decaySamples - releaseSamples;

        for (int i = 0; i < audio.Length; i++)
        {
            double envelope;

            if (i < attackSamples)
            {
                // Attack phase
                envelope = (double)i / attackSamples;
            }
            else if (i < attackSamples + decaySamples)
            {
                // Decay phase
                var decayProgress = (double)(i - attackSamples) / decaySamples;
                envelope = 1.0 - decayProgress * (1.0 - sustainLevel);
            }
            else if (i < attackSamples + decaySamples + sustainSamples)
            {
                // Sustain phase
                envelope = sustainLevel;
            }
            else
            {
                // Release phase
                var releaseProgress = (double)(i - attackSamples - decaySamples - sustainSamples) / releaseSamples;
                envelope = sustainLevel * (1.0 - releaseProgress);
            }

            result[i] = audio[i] * envelope;
        }

        return result;
    }

    // Effects
    public double[] AddReverb(double[] audio, double delayTime, double decay, int numEchoes = 5)
    {
        var delaySamples = (int)(delayTime * _sampleRate);
        var result = new double[audio.Length + delaySamples * numEchoes];
        
        // Copy original audio
        Array.Copy(audio, result, audio.Length);

        // Add echoes
        for (int echo = 1; echo <= numEchoes; echo++)
        {
            var echoAmplitude = Math.Pow(decay, echo);
            var echoOffset = delaySamples * echo;

            for (int i = 0; i < audio.Length; i++)
            {
                if (i + echoOffset < result.Length)
                {
                    result[i + echoOffset] += audio[i] * echoAmplitude;
                }
            }
        }

        return result;
    }

    public double[] AddDistortion(double[] audio, double gain, double threshold = 0.7)
    {
        var result = new double[audio.Length];

        for (int i = 0; i < audio.Length; i++)
        {
            var amplified = audio[i] * gain;
            
            if (Math.Abs(amplified) > threshold)
            {
                result[i] = Math.Sign(amplified) * threshold + 
                           (amplified - Math.Sign(amplified) * threshold) * 0.1;
            }
            else
            {
                result[i] = amplified;
            }
        }

        return result;
    }

    // Utility methods
    public double[] CombineAudio(params double[][] audioStreams)
    {
        if (audioStreams.Length == 0) return Array.Empty<double>();

        var maxLength = audioStreams.Max(s => s.Length);
        var result = new double[maxLength];

        foreach (var stream in audioStreams)
        {
            for (int i = 0; i < stream.Length; i++)
            {
                result[i] += stream[i];
            }
        }

        // Normalize to prevent clipping
        var maxValue = result.Max(Math.Abs);
        if (maxValue > 1.0)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] /= maxValue;
            }
        }

        return result;
    }

    public double[] ChangeSpeed(double[] audio, double speedMultiplier)
    {
        var newLength = (int)(audio.Length / speedMultiplier);
        var result = new double[newLength];

        for (int i = 0; i < newLength; i++)
        {
            var sourceIndex = i * speedMultiplier;
            var lowerIndex = (int)Math.Floor(sourceIndex);
            var upperIndex = (int)Math.Ceiling(sourceIndex);
            
            if (upperIndex >= audio.Length) upperIndex = audio.Length - 1;
            
            var fraction = sourceIndex - lowerIndex;
            result[i] = audio[lowerIndex] * (1 - fraction) + audio[upperIndex] * fraction;
        }

        return result;
    }

    // Musical note frequencies (equal temperament)
    public static double GetNoteFrequency(string note, int octave)
    {
        var noteToSemitone = new Dictionary<string, int>
        {
            ["C"] = 0, ["C#"] = 1, ["Db"] = 1, ["D"] = 2, ["D#"] = 3, ["Eb"] = 3,
            ["E"] = 4, ["F"] = 5, ["F#"] = 6, ["Gb"] = 6, ["G"] = 7, ["G#"] = 8,
            ["Ab"] = 8, ["A"] = 9, ["A#"] = 10, ["Bb"] = 10, ["B"] = 11
        };

        if (!noteToSemitone.ContainsKey(note))
            throw new ArgumentException($"Invalid note: {note}");

        var semitoneOffset = noteToSemitone[note];
        var totalSemitones = (octave - 4) * 12 + semitoneOffset;
        
        // A4 = 440 Hz
        return 440.0 * Math.Pow(2.0, totalSemitones / 12.0);
    }
}