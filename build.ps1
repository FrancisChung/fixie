﻿param([int]$buildNumber)

. .\build-helpers

$versionPrefix = "2.2.0"
$prerelease = $false

$authors = "Patrick Lioi"
$copyright = copyright 2013 $authors
$configuration = 'Release'
$versionSuffix = if ($prerelease) { "beta-{0:D4}" -f $buildNumber } else { "" }

function Build {
    mit-license $copyright

    generate "src\Directory.build.props" @"
<Project>
    <PropertyGroup>
        <Product>Fixie</Product>
        <VersionPrefix>$versionPrefix</VersionPrefix>
        <VersionSuffix>$versionSuffix</VersionSuffix>
        <Authors>$authors</Authors>
        <Copyright>$copyright</Copyright>
        <PackageLicenseExpression>MIT</PackageLicenseExpression>
        <PackageProjectUrl>https://fixie.github.io</PackageProjectUrl>
        <PackageIcon>icon.png</PackageIcon>
        <RepositoryUrl>https://github.com/fixie/fixie</RepositoryUrl>
        <PackageOutputPath>..\..\packages</PackageOutputPath>
        <IncludeSymbols>true</IncludeSymbols>
        <LangVersion>latest</LangVersion>
    </PropertyGroup>
</Project>
"@

    exec { dotnet clean src -c $configuration --nologo -v minimal }
    exec { dotnet build src -c $configuration --nologo }
}

function Test {
    $fixie = resolve-path .\src\Fixie.Console\bin\$configuration\netcoreapp2.0\dotnet-fixie.dll

    exec { dotnet $fixie --configuration $configuration --no-build } src/Fixie.Tests
}

function Pack {
    remove-folder packages
    exec { dotnet pack -c $configuration --no-restore --no-build --nologo } src\Fixie
    exec { dotnet pack -c $configuration --no-restore --no-build --nologo } src\Fixie.Console
}

main {
    step { dotnet --version }
    step { Build }
    step { Test }
    step { Pack }
}