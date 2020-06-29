using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public readonly struct ValueStopwatch
{
    static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    readonly long startTimestamp;

    public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

    ValueStopwatch(long startTimestamp)
    {
        this.startTimestamp = startTimestamp;
    }

    public TimeSpan Elapsed => TimeSpan.FromTicks(this.ElapsedTicks);

    public long ElapsedTicks
    {
        get
        {
            if (startTimestamp == 0)
            {
                throw new InvalidOperationException("Detected invalid initialization(use 'default'), only to create from StartNew().");
            }

            var delta = Stopwatch.GetTimestamp() - startTimestamp;
            return (long)(delta * TimestampToTicks);
        }
    }
}

public static class ValueStopwatchExtentions
{
    public static void Restart(this ref ValueStopwatch sp)
    {
        sp = ValueStopwatch.StartNew();
    }
}

public class Measurement : IDisposable
{
    private string key;
    private ValueStopwatch sw = ValueStopwatch.StartNew();
    public Measurement(string key)
    {
        this.key = key;
    }
    public void Dispose()
    {
        UnityEngine.Debug.Log($"{key}: {sw.Elapsed.TotalSeconds}s");
    }
}