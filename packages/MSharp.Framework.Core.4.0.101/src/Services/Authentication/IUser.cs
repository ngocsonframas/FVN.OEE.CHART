﻿using System.Collections.Generic;
using System.Linq;
using MSharp.Framework.Services;

namespace MSharp.Framework.Services
{
    public interface IUser : IEntity
    {
        IEnumerable<string> GetRoles();
    }
}

namespace System
{
    public static class IUserExtensions
    {
        /// <summary>
        /// Determines whether this user has a specified role.
        /// </summary>
        public static bool IsInRole(this IUser user, string role)
        {
            if (user == null) return false;
            else return user.GetRoles().Contains(role);
        }

        /// <summary>
        /// Determines if this user Is Authenticated.
        /// </summary>
        public static bool IsAuthenticated(this IUser user)
        {
            if (user == null) return false;
            else return !user.IsNew;
        }
    }
}