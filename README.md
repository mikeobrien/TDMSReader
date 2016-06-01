TDMS Reader
=============

[![Nuget](http://img.shields.io/nuget/v/TDMSReader.svg?style=flat)](http://www.nuget.org/packages/TDMSReader/) [![Travis CI Build Status](http://img.shields.io/travis/mikeobrien/TDMSReader.svg?style=flat&label=Travis%20CI)](https://travis-ci.org/mikeobrien/TDMSReader) [![TeamCity Build Status](https://img.shields.io/teamcity/http/build.mikeobrien.net/s/tdmsreader.svg?style=flat&label=TeamCity)](http://build.mikeobrien.net/viewType.html?buildTypeId=tdmsreader&guest=1)

A managed library for reading TDMS files.

## Install

TDMSReader can be found on nuget:

    PM> Install-Package TDMSReader

## Usage

The folowing demonstrates how to read data from a TDMS file.

```csharp
using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\export.txt")))
using (var tdms = new NationalInstruments.Tdms.File("Sample.tdms"))
{
    tdms.Open();

    foreach (var value in tdms.Groups["Noise data"].Channels["Noise_1"].GetData<double>())
        output.WriteLine(value);
}
```

The folowing demonstrates enumeration of properties, groups and channels.

```csharp
using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\tdms.overvw.txt")))
using (var tdms = new NationalInstruments.Tdms.File("Sample.tdms"))
{
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
Contributors
------------

Huge thanks to Tommy Jakobsen for adding interleaved data support!

| [![Tommy Jakobsen](http://www.gravatar.com/avatar/1.jpg?s=144)](https://github.com/tommyja) |
|:---:|
| [Tommy Jakobsen](https://github.com/tommyja) |
	
Props
------------

Thanks to [JetBrains](http://www.jetbrains.com/) for providing OSS licenses!

## License
MIT License