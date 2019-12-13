﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System
{
    partial class MSharpExtensions
    {
        /// <summary>
        /// It's recommended to use Task.Factory.RunSync() instead.
        /// If you can't, at then call this while making it explicit that you know what you're doing.
        /// </summary>        
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult RiskDeadlockAndAwaitResult<TResult>(this Task<TResult> task)
              => Task.Run(async () => await task).Result;

        /// <summary>
        /// If the task is not completed already it throws an exception warning you to await the task.
        /// If the task wraps an exception, the wrapped exception will be thrown.
        /// Otherwise the result will be returned.
        /// Use this instead of calling the Result property when you know that the result is ready to avoid deadlocks.
        /// </summary>
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult GetAlreadyCompletedResult<TResult>(this Task<TResult> @this)
        {
            if (@this == null) return default(TResult);

            if (!@this.IsCompleted)
                throw new InvalidOperationException("This task is not completed yet. Do you need to await it?");

            if (@this.Exception != null)
                throw @this.Exception.InnerException;

            return @this.Result;
        }

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static void RunSync(this TaskFactory @this, Func<Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            using (var actualTask = new Task<Task>(task))
            {
                try
                {
                    actualTask.RunSynchronously();
                    actualTask.WaitAndThrow();
                    actualTask.Result.WaitAndThrow();
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException;
                }
            }
        }

        /// <summary>
        /// Waits for a task to complete, and then if it contains an exception, it will be thrown.
        /// </summary>
        public static void WaitAndThrow(this Task @this)
        {
            if (@this == null) return;
            try { @this.Wait(); }
            catch (AggregateException ex) { throw ex.InnerException; }

            if (@this.Exception?.InnerException != null)
                throw @this.Exception.InnerException;
        }

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult RunSync<TResult>(this TaskFactory @this, Func<Task<TResult>> task)
        {
            try
            {
                return @this.StartNew(task, TaskCreationOptions.LongRunning)
                     .ContinueWith(t =>
                     {
                         if (t.Exception == null) return t.Result;

                         System.Diagnostics.Debug.Fail("Error in calling TaskFactory.RunSync: " + t.Exception.InnerException.ToLogString());
                         throw t.Exception.InnerException;
                     })
                     .RiskDeadlockAndAwaitResult()
                     .RiskDeadlockAndAwaitResult();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public static async Task WithTimeout(this Task @this, TimeSpan timeout, Action success = null, Action timeoutAction = null)
        {
            if (await Task.WhenAny(@this, Task.Delay(timeout)) == @this) success?.Invoke();

            else
            {
                if (timeoutAction == null) throw new TimeoutException("The task didn't complete within " + timeout + "ms");
                else timeoutAction();
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> @this, TimeSpan timeout, Action success = null, Func<T> timeoutAction = null)
        {
            if (await Task.WhenAny(@this, Task.Delay(timeout)) == @this)
            {
                success?.Invoke();
                await @this;
                return @this.GetAwaiter().GetResult();
            }

            else
            {
                if (timeoutAction == null) throw new TimeoutException("The task didn't complete within " + timeout + "ms");
                else { return timeoutAction(); }
            }
        }

        public static async Task<List<T>> ToList<T>(this Task<IEnumerable<T>> @this) => (await @this).ToList();

        public static async Task<T[]> ToArray<T>(this Task<IEnumerable<T>> @this) => (await @this).ToArray();

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<Task<T>> @this)
            => await Task.WhenAll(@this);

        /// <summary>
        /// Casts the result type of the input task as if it were covariant.
        /// </summary>
        /// <typeparam name="TOriginal">The original result type of the task</typeparam>
        /// <typeparam name="TTarget">The covariant type to return</typeparam>
        /// <param name="this">The target task to cast</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TTarget> AsTask<TOriginal, TTarget>(this Task<TOriginal> @this)
            where TOriginal : TTarget => @this.ContinueWith(t => (TTarget)t.GetAlreadyCompletedResult());

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<T[]> @this)
            => @this.AsTask<T[], IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<List<T>> @this)
            => @this.AsTask<List<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IList<T>> @this)
            => @this.AsTask<IList<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IOrderedEnumerable<T>> @this)
            => @this.AsTask<IOrderedEnumerable<T>, IEnumerable<T>>();

        /// <summary>
        /// A shorter more readable alternative to ContinueWith().
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> @this, Func<TSource, TResult> expression)
            => expression(await @this.ConfigureAwait(continueOnCapturedContext: false));

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> @this, Func<TSource, Task<TResult>> expression)
        {
            var item = await @this.ConfigureAwait(continueOnCapturedContext: false);
            return await expression(item).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task Then<TSource, TResult>(this Task<TSource> @this, Action<TSource> action)
        {
            var item = await @this.ConfigureAwait(continueOnCapturedContext: false);
            action(item);
        }

        /// <summary>
        /// Awaits this task. If the result was an exception,
        /// it will return the default value of TResult rather than throwing the exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> ResultOrDefault<TResult>(this Task<TResult> @this)
        {
            try { return await @this.ConfigureAwait(continueOnCapturedContext: false); }
            catch
            {
                // No logging is needed
                return default(TResult);
            }
        }
    }
}