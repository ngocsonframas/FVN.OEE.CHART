﻿using System;

namespace MSharp.Framework.Data
{
    public interface IDataProviderFactory
    {
        IDataProvider GetProvider(Type type);

        /// <summary>
        /// Determines whether this data provider factory handles interface data queries.
        /// </summary>
        bool SupportsPolymorphism();
    }
}
