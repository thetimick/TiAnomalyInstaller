// ⠀
// ExtendedListExtensions.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 12.01.2026.
// ⠀

using System;
using DynamicData;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.Extensions;

public static class ExtendedListExtensions
{
    extension<T>(IExtendedList<T> ts) where T : notnull
    {
        public int? FindIndex(Predicate<T> match)
        {
            return ts.FindIndex(0, ts.Count, match);
        }

        public int? FindIndex(int startIndex, Predicate<T> match)
        {
            return ts.FindIndex(startIndex, ts.Count, match);
        }

        private int? FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if (startIndex < 0) 
                startIndex = 0;
            if (count > ts.Count) 
                count = ts.Count;
            for (var i = startIndex; i < count; i++)
                if (match(ts[i])) 
                    return i;
            return null;
        }
    }
}