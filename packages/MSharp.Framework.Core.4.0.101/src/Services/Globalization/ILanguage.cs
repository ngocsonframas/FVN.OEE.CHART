﻿namespace MSharp.Framework.Services.Globalization
{
    public interface ILanguage : IEntity
    {
        string Name { get; }
        string IsoCode { get; }
        bool IsDefault { get; }
    }
}
