// ⠀
// ObservableCollectionExtensions.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 12.01.2026.
// ⠀

using System;
using System.Collections.ObjectModel;

namespace TiAnomalyInstaller.UI.Avalonia.Extensions;

public static class ObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> ts)
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