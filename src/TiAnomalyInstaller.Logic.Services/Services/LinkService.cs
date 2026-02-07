// ⠀
// LinkService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.02.2026.
// ⠀

using Securify.ShellLink;

namespace TiAnomalyInstaller.Logic.Services.Services;

public interface ILinkService
{
    void Make(string input, string linkFileName);
}

public class LinkService: ILinkService
{
    public void Make(string inputFileName, string linkFileName)
    {
        Shortcut.CreateShortcut(inputFileName, "", Path.GetDirectoryName(inputFileName))
            .WriteToFile(linkFileName);
    }
}