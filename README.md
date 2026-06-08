# At.Matus.IO.PerkinElmerLib

A .NET library for reading and parsing PerkinElmer SP (Spectrum) binary file format, extracting spectral data, metadata, and history records into structured objects. This library is based on (https://github.com/kutukvpavel/PerkinElmer2CSV)](https://github.com/kutukvpavel/PerkinElmer2CSV). The main difference ist the exhaustive handling of measurement metadata contained in the SP files.

## Overview

This library provides tools to work with PerkinElmer SP spectroscopy files, converting proprietary binary format data into accessible .NET objects. The solution includes both a reusable library component and a command-line tool for converting SP files to CSV and JSON formats.

## Projects

### At.Matus.IO.PerkinElmerSP.Reader

The core library that handles parsing of PerkinElmer SP binary files. It provides:

- **Binary file parsing** with block-based structure analysis
- **Spectral data extraction** into `Spectrum2d` objects
- **Metadata and history record** parsing
- **Export functionality** to CSV and JSON formats

### Sp2Csv

A command-line tool that converts PerkinElmer SP files to multiple output formats:
- CSV files with spectral data and metadata comments
- JSON files with metadata
- Text files with formatted metadata

## The Spectrum2d Class

The `Spectrum2d` class is the central data structure representing a two-dimensional spectrum. It encapsulates all spectral information extracted from PerkinElmer SP files.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `MetaData` | `MeasurementMetaData` | Stores instrument settings, acquisition parameters, and file information as key-value pairs |
| `StartX` | `double` | Starting value of the X-axis (e.g., wavelength, wavenumber) |
| `EndX` | `double` | Ending value of the X-axis |
| `ResolutionX` | `double` | Resolution/step size along the X-axis |
| `LabelX` | `string` | Label for the X-axis (e.g., "Wavenumber", "Wavelength") |
| `LabelY` | `string` | Label for the Y-axis (e.g., "Absorbance", "Transmittance") |
| `Points` | `Point2d[]` | Array of spectral data points containing X and Y coordinate pairs |

### Extension Methods

The library provides extension methods for `Spectrum2d` objects:

- **`WriteMetaDataAsComments(TextWriter)`** - Exports metadata as commented lines (# key = value)
- **`WriteCsv(TextWriter)`** - Exports spectral data points in CSV format
- **`WriteMetaDataAsJson(TextWriter)`** - Exports metadata in JSON format
- **`WriteMetaDataAsText(TextWriter)`** - Exports metadata in plain text format

## Dependencies

### Target Framework
- **.NET 8.0** (net8.0)

### External Dependencies
- **At.Matus.MetaData** - Custom library for handling measurement metadata storage and manipulation

### .NET Standard Libraries
The library relies on standard .NET namespaces:
- `System.Text` - String and encoding operations
- `System.Text.Json` - JSON serialization for metadata export
- `System.IO` - File and stream operations
- `System.Globalization` - Culture-invariant number formatting

## Usage

### Basic Usage - Reading SP Files

### Command-Line Tool Usage

## File Format

PerkinElmer SP files use a binary block-based structure:

1. **Header**: "PEPE" magic signature + 40-byte description string
2. **Blocks**: Array of data blocks with ID, size, and binary content
3. **History Records**: Metadata entries with specific byte patterns for different data types

The parser supports:
- Text records (prefix: `0x23 0x75`)
- Short integer records (prefix: `0x2D 0x75`)
- Double records (prefix: `0x1C 0x75`)

## Acknowledgments

Based on original MATLAB code by:
- **Stephen Westlake** (Perkin Elmer, 2007)
- **Seer Green** (Perkin Elmer, 2007)

Adapted for C#/.NET by:
- **Kutukov Pavel** (2022)
- **Michael Matus** (2026)

## Architecture Requirements

- Currently supports **Little-Endian** architectures only
- BigEndian support is not implemented

## Copyright

Copyright (c) 2026 Michael Matus  
All rights reserved.

## Repository

[https://github.com/matusm/At.Matus.IO.PerkinElmerLib](https://github.com/matusm/At.Matus.IO.PerkinElmerLib)
