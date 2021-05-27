using System;
using System.Collections.Generic;

namespace Arkitektum.RuleValidator.Core.Extensions
{
    public static class DisposableExtensions
    {
        public static void Dispose(this IEnumerable<IDisposable> collection)
        {
            foreach (IDisposable item in collection)
            {
                if (item != null)
                {
                    try
                    {
                        item.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
