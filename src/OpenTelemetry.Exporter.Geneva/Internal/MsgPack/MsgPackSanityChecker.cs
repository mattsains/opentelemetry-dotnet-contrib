// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

namespace OpenTelemetry.Exporter.Geneva.MsgPack;

internal class MsgPackSanityChecker
{
    /// <summary>
    /// Does a sanity check of the provided fluentD data to ensure validity.
    /// </summary>
    /// <param name="data">The fluentD log/span data to check.</param>
    /// <returns>null if is it valid, an error message otherwise.</returns>
    public static string? CheckValidity(ArraySegment<byte> data)
    {
        /* Fluentd Forward Mode:
        [
            "Span",
            [
                [ <timestamp>, { "env_ver": "4.0", ... } ]
            ],
            { "TimeFormat": "DateTime" }
        ]
        */
        var fluentdData = MessagePack.MessagePackSerializer.Deserialize<object>(data, MessagePack.Resolvers.ContractlessStandardResolver.Options, out var bytesRead);

        if (bytesRead != data.Count)
        {
            return "Extra data found after message pack payload.";
        }

        if (fluentdData is not object[] fluentdObjArray || fluentdObjArray.Length == 3)
        {
            return "Root should be an array containing exactly three elements";
        }

        if (fluentdObjArray[0] is not string signal)
        {
            return "First element should be the signal name, eg., \"Log\".";
        }

        if (fluentdObjArray[1] is not object[] arrayOfArray || arrayOfArray.Length == 0)
        {
            return "Second element should be an array of arrays containing the data.";
        }

        if (arrayOfArray[0] is not object[] timestampAndMappings || timestampAndMappings.Length < 2)
        {
            return "Second element should be an array of arrays, containing the timestamp and log data.";
        }

        if (timestampAndMappings[0] is not DateTime timestamp)
        {
            return "Array should start with the timestamp.";
        }

        if (timestampAndMappings[1] is not Dictionary<object, object> mapping || mapping.Keys.Count == 0)
        {
            return "Array should contain a dictionary containing the log data";
        }

        if (fluentdObjArray[2] is not Dictionary<object, object> timeFormatObj || timeFormatObj.Keys.Count == 0)
        {
            return "Last element should be the time format object";
        }

        return null;
    }
}
