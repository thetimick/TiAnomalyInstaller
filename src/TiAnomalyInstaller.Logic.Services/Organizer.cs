// ⠀
// Organizer.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 06.01.2026.
// ⠀

using TiAnomalyInstaller.AppConstants;

namespace TiAnomalyInstaller.Logic.Services;

public static class Organizer
{
    private const string Template = """
                                    [General]
                                    gameName=STALKER Anomaly
                                    gamePath=@ByteArray(<template-vanilla>)
                                    selected_profile=@ByteArray(<profile>)
                                    version=2.5.2
                                    first_start=false
                                    
                                    [customExecutables]
                                    size=10
                                    1\arguments=
                                    1\binary=<binary-template-vanilla>/AnomalyLauncher.exe
                                    1\hide=false
                                    1\ownicon=true
                                    1\steamAppID=
                                    1\title=Anomaly Launcher
                                    1\toolbar=false
                                    1\workingDirectory=<binary-template-vanilla>
                                    2\arguments=
                                    2\binary=<binary-template-vanilla>/bin/AnomalyDX11AVX.exe
                                    2\hide=false
                                    2\ownicon=true
                                    2\steamAppID=
                                    2\title=Anomaly (DX11-AVX)
                                    2\toolbar=false
                                    2\workingDirectory=<binary-template-vanilla>/bin
                                    3\arguments=
                                    3\binary=<binary-template-vanilla>/bin/AnomalyDX11.exe
                                    3\hide=false
                                    3\ownicon=true
                                    3\steamAppID=
                                    3\title=Anomaly (DX11)
                                    3\toolbar=false
                                    3\workingDirectory=<binary-template-vanilla>/bin
                                    4\arguments=
                                    4\binary=<binary-template-vanilla>/bin/AnomalyDX10AVX.exe
                                    4\hide=false
                                    4\ownicon=true
                                    4\steamAppID=
                                    4\title=Anomaly (DX10-AVX)
                                    4\toolbar=false
                                    4\workingDirectory=<binary-template-vanilla>/bin
                                    5\arguments=
                                    5\binary=<binary-template-vanilla>/bin/AnomalyDX10.exe
                                    5\hide=false
                                    5\ownicon=true
                                    5\steamAppID=
                                    5\title=Anomaly (DX10)
                                    5\toolbar=false
                                    5\workingDirectory=<binary-template-vanilla>/bin
                                    6\arguments=
                                    6\binary=<binary-template-vanilla>/bin/AnomalyDX9AVX.exe
                                    6\hide=false
                                    6\ownicon=true
                                    6\steamAppID=
                                    6\title=Anomaly (DX9-AVX)
                                    6\toolbar=false
                                    6\workingDirectory=<binary-template-vanilla>/bin
                                    7\arguments=
                                    7\binary=<binary-template-vanilla>/bin/AnomalyDX9.exe
                                    7\hide=false
                                    7\ownicon=true
                                    7\steamAppID=
                                    7\title=Anomaly (DX9)
                                    7\toolbar=false
                                    7\workingDirectory=<binary-template-vanilla>/bin
                                    8\arguments=
                                    8\binary=<binary-template-vanilla>/bin/AnomalyDX8AVX.exe
                                    8\hide=false
                                    8\ownicon=true
                                    8\steamAppID=
                                    8\title=Anomaly (DX8-AVX)
                                    8\toolbar=false
                                    8\workingDirectory=<binary-template-vanilla>/bin
                                    9\arguments=
                                    9\binary=<binary-template-vanilla>/bin/AnomalyDX8.exe
                                    9\hide=false
                                    9\ownicon=true
                                    9\steamAppID=
                                    9\title=Anomaly (DX8)
                                    9\toolbar=false
                                    9\workingDirectory=<binary-template-vanilla>/bin
                                    10\arguments=\"<template-vanilla>\"
                                    10\binary=<binary-template-organizer>/explorer++/Explorer++.exe
                                    10\hide=false
                                    10\ownicon=true
                                    10\steamAppID=
                                    10\title=Explore Virtual Folder
                                    10\toolbar=false
                                    10\workingDirectory=<binary-template-organizer>/explorer++
                                    """;
    
    public static void Setup(string profile)
    {
        if (!TryDeleteIfNeeded(Constants.MO2.ConfigFileName)) 
            return;
        
        var content = Template
            .Replace("<profile>", profile)
            .Replace("<template-vanilla>", Constants.VanillaFolderName.Replace(@"\", @"\\"))
            .Replace("<binary-template-vanilla>", Constants.VanillaFolderName.Replace(@"\", "/"))
            .Replace("<binary-template-organizer>", Constants.OrganizerFolderName.Replace(@"\", "/"));
        
        File.WriteAllText(Constants.MO2.ConfigFileName, content);
    }
    
    private static bool TryDeleteIfNeeded(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}