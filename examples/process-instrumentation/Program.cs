// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter.Geneva;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

internal class Program
{
    public static void Main()
    {
        var path = GetRandomFilePath();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new AlwaysOnSampler())
            .AddSource("*")
            .AddGenevaTraceExporter(options =>
            {
                options.ConnectionString = "Endpoint=unix:" + path;
            })
            .Build();

        using var activity = new Activity("hello");
        tracerProvider.ForceFlush();
        Thread.Sleep(10000);
    }

    private static string GetRandomFilePath()
    {
        while (true)
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!File.Exists(path))
            {
                Console.WriteLine(path);
                return path;
            }
        }
    }
}
