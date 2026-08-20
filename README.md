# MoreFodyHelpers

[![Build](https://github.com/huoshan12345/MoreFodyHelpers/actions/workflows/build.yml/badge.svg)](https://github.com/huoshan12345/MoreFodyHelpers/actions/workflows/build.yml)
[![NuGet package](https://img.shields.io/nuget/v/MoreFodyHelpers.svg?logo=NuGet)](https://www.nuget.org/packages/MoreFodyHelpers)
[![.net](https://img.shields.io/badge/.net%20standard-2.0-ff69b4.svg?)](https://www.microsoft.com/net/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/huoshan12345/MoreFodyHelpers/blob/main/LICENSE)  
![Icon](https://raw.githubusercontent.com/huoshan12345/MoreFodyHelpers/main/icon.png)

---
 - [Installation](#installation)
 - [Usage](#usage)
 - [Example](#examples) 
---

## Installation
- Include the [`MoreFodyHelpers`](https://www.nuget.org/packages/MoreFodyHelpers) NuGet package to the weaver project.

  ```XML
  <PackageReference Include="MoreFodyHelpers" GeneratePathProperty="true" Version="..." />
  ```
- Add this code snippet to the csproj file of the NuGet package, **NOT** the weaver project.
```XML
<PropertyGroup>
  <TargetsForTfmSpecificContentInPackage>$(TargetsForTfmSpecificContentInPackage);IncludeReferences</TargetsForTfmSpecificContentInPackage>
</PropertyGroup>
<Target Name="IncludeReferences">
  <ItemGroup>
    <TfmSpecificPackageFile Include="$(OutputPath)\MoreFodyHelpers.dll" PackagePath="weaver" />
  </ItemGroup>
</Target>
```

## Usage

## Examples

