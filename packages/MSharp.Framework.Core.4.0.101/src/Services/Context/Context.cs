using MSharp.Framework.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace MSharp.Framework
{
    public class Context
    {
        public static Context Current { get; private set; }

        readonly IServiceProvider ServiceProvider;

        public IPrincipal User => GetRequiredService<IUserAccessor>().User;

        public IHttpContextItemsAccessor HttpContextItemsAccessor => GetRequiredService<IHttpContextItemsAccessor>();

        public IDictionary HttpContextItems => GetRequiredService<IHttpContextItemsAccessor>().Items;

        Context(IServiceProvider provider) => ServiceProvider = provider;

        public static void Initialize(IServiceProvider provider) => Current = new Context(provider);

        public T GetRequiredService<T>() => ServiceProvider.GetRequiredService<T>();

        public T GetService<T>() => ServiceProvider.GetService<T>();

        public string Param(string key) => GetRequiredService<IContextParameterValueProvider>().Param(key);
    }
}
