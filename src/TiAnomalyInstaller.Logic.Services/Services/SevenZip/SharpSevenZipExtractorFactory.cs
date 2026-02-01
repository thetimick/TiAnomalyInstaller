// ⠀
// SharpSevenZipExtractorFactory.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 01.02.2026.
// ⠀

using SharpSevenZip;

namespace TiAnomalyInstaller.Logic.Services.Services.SevenZip;

public interface ISharpSevenZipExtractorFactory
{
    public SharpSevenZipExtractor MakeExtractor(string fileName);
}

public class SharpSevenZipExtractorFactory: ISharpSevenZipExtractorFactory
{
    public SharpSevenZipExtractor MakeExtractor(string fileName)
    {
        return new SharpSevenZipExtractor(fileName);
    }
}