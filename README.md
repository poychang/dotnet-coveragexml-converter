# dotnet-coveragexml-converter

[![Build Status](https://app.travis-ci.com/poychang/dotnet-coveragexml-converter.svg?branch=main)](https://app.travis-ci.com/github/poychang/dotnet-coveragexml-converter)
[![NuGet version](https://badge.fury.io/nu/dotnet-coveragexml-converter.svg)](https://badge.fury.io/nu/dotnet-coveragexml-converter)

This dotnet tool can be used to convert coverage files from `.coverage` (binary format) files to `.coveragexml` (xml format) files.

## Installation

Install with dotnet tool install command:

``` cmd
dotnet tool install --global dotnet-coveragexml-converter
```

or

Install the tool in yaml pipeline:

``` yaml
- task: DotNetCoreCLI@2
  displayName: "Install dotnet tool: dotnet-coveragexml-converter"
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'update --global dotnet-coveragexml-converter'
```

## Usage

You may need generate `.coverage` files first. For example, you can use [`dotnet test`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test) command to generate `.coverage` files.

On Windows, you can collect code coverage by using the `--collect "Code Coverage"` option in `dotnet test`. Examples are as follows:

```cmd
dotnet test ./projects/test.csproj --collect "Code Coverage"
```

On any platform that is supported by .NET Core, install [Coverlet](https://github.com/coverlet-coverage/coverlet/) and use the `--collect:"XPlat Code Coverage"` option.

Using `dotnet-coveragexml-converter` dotnet tool to convert `.coverage` to `.coveragexml`. It required `--CoverageFilesFolder` parameter to specify the folder that contains `.coverage` files. Examples are as follows:

```cmd
dotnet-coveragexml-converter --CoverageFilesFolder ".\TestResults"
```

### Easy Steps

1. `dotnet test .\projects\test.csproj --collect "Code Coverage"` generate `.coverage` files.
2. `dotnet tool install --global dotnet-coveragexml-converter'` install the dotnet tool.
3. `dotnet-coveragexml-converter --CoverageFilesFolder ".\TestResults"` convert `.coverage` to `.coveragexml`

### Example YAML pipeline tasks

``` yaml
- task: CmdLine@2
  displayName: 'Convert .coverage to .coveragexml'
  inputs:
    script: |
     dotnet tool install --global dotnet-coveragexml-converter
     dotnet-coveragexml-converter --CoverageFilesFolder "$(Agent.TempDirectory)\TestResults"
```

## Help

`dotnet-coveragexml-converter --help` show help and usage information for this tool.

```
dotnet-coveragexml-converter
  dotnet-coveragexml-converter can be used to convert coverage files from `.coverage` (binary format) files to `.coveragexml` (xml format) files.

Usage:
  dotnet-coveragexml-converter [options]

Options:
  -f, --coverage-files-folder <coverage-files-folder> (REQUIRED)  The folder contain the .coverage files.
  -a, --all-directories                                           Includes subfolders for search operation. [default: True]
  -p, --process-all-files                                         Convert all .coverage files. Default is false, only convert the folders which are a guid (that's the one VSTest creates). [default: False]
  -o, --overwrite                                                 Overwrite the existing .coveragexml files. [default: True]
  -r, --remove-original-files                                     Remove the original .coverage files.
  --version                                                       Show version information
  -?, -h, --help                                                  Show help and usage information
```

## CodeCoverage.exe Info

This dotnet tool use `CodeCoverage.exe` to convert `.coverage` to `.coveragexml` files.

`CodeCoverage.exe` is Visual Studio coverage tool that comes with Visual Studio 2012/2013 (Premium and Ultimate). You can find this tool under `C:\Program Files (x86)\Microsoft Visual Studio 11.0\Team Tools\Dynamic Code Coverage Tools\`.

The executable file can collect code coverage information into a `*.coverage` file. Also can analyze `*.coverage` file and convert to `*.xml` format.

To get the `*.xml` file you can use the following command:

```cmd
CodeCoverage.exe collect /output:DynamicCodeCoverage.coverage "PATH_OF_YOUR_EXECUTABLE_OR_DLL"
CodeCoverage.exe analyze /output:DynamicCodeCoverage.coveragexml DynamicCodeCoverage.coverage
```

`CodeCoverage.exe` is also part of this [Microsoft.CodeCoverage](https://www.nuget.org/packages/Microsoft.CodeCoverage/) NuGet package. This tool include `CodeCoverage.exe` from Microsoft.CodeCoverage and use that to convert code coverage binary report to xml format.
