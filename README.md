TDMS Reader
=============

[![Nuget](http://img.shields.io/nuget/v/TDMSReader.svg)](http://www.nuget.org/packages/TDMSReader/) [![Nuget downloads](http://img.shields.io/nuget/dt/TDMSReader.svg)](http://www.nuget.org/packages/TDMSReader/) [![Build Status](https://travis-ci.org/mikeobrien/TDMSReader.png?branch=master)](https://travis-ci.org/mikeobrien/TDMSReader) 

A managed library for reading TDMS files.

## Install

TDMSReader can be found on nuget:

    PM> Install-Package TDMSReader

## Usage

The folowing demonstrates how to read data from a TDMS file.

```csharp
using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\export.txt")))
{
    var tdms = new NationalInstruments.Tdms.File("Sample.tdms");
    tdms.Open();

    foreach (var value in tdms.Groups["Noise data"].Channels["Noise_1"].GetData<double>())
        output.WriteLine(value);
}
```

The folowing demonstrates enumeration of properties, groups and channels.

```csharp
using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\tdms.overvw.txt")))
{
    var tdms = new NationalInstruments.Tdms.File("Sample.tdms");
    tdms.Open();

    output.WriteLine("Properties:");
    foreach (var property in tdms.Properties)
        output.WriteLine("  {0}: {1}", property.Key, property.Value);
    output.WriteLine();
    foreach (var group in tdms)
    {
        output.WriteLine("    Group: {0}", group.Name);
        foreach (var property in group.Properties)
            output.WriteLine("    {0}: {1}", property.Key, property.Value);
        output.WriteLine();
        foreach (var channel in group)
        {
            output.WriteLine("        Channel: {0}", channel.Name);
            foreach (var property in channel.Properties)
                output.WriteLine("        {0}: {1}", property.Key, property.Value);
            output.WriteLine();
        }
    }

    output.WriteLine("Data:");
    foreach (var group in tdms)
    {
        output.WriteLine("    Group: {0}", group.Name);
        foreach (var channel in group)
        {
            output.WriteLine("    Channel: {0} ({1} data points of type {2})", channel.Name,
                                channel.DataCount, channel.DataType);
            foreach (var value in channel.GetData<object>().Take(20))
                output.WriteLine("          {0}", value);
            if (channel.DataCount > 20) output.WriteLine("        ...");
            output.WriteLine();
        }
    }
}
```

## License
MIT License