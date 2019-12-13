﻿namespace MSharp.Framework.Services
{
    /// <summary>
    /// Represents a sortable entity type.
    /// </summary>
    public interface ISortable : IEntity
    {
        int Order { get; set; }
    }
}