namespace UniRx.Runtime.CompilerServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using UniRx;

    /// <summary>Provides an awaiter for awaiting a <see cref="Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct AsyncSubjectAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        /// <summary>The task being awaited.</summary>
        private readonly AsyncSubject<TResult> _task;

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted
        {
            get
            {
                return _task.IsCompleted;
            }
        }

        /// <summary>Initializes the <see cref="TaskAwaiter{TResult}"/>.</summary>
        /// <param name="task">The <see cref="Task{TResult}"/> to be awaited.</param>
        internal AsyncSubjectAwaiter(AsyncSubject<TResult> task)
        {
            Debug.Assert(task != null, null);
            _task = task;
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="AsyncSubject{TResult}"/> associated with this
        /// <see cref="AsyncSubjectAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, true);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="AsyncSubject{TResult}"/> associated with this
        /// <see cref="AsyncSubjectAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, true);
        }

        /// <summary>Ends the await on the completed <see cref="Task{TResult}"/>.</summary>
        /// <returns>The result of the completed <see cref="Task{TResult}"/>.</returns>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="InvalidOperationException">The task was not yet completed.</exception>
        /// <exception cref="TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="Exception">The task completed in a <see cref="TaskStatus.Faulted"/> state.</exception>
        public TResult GetResult()
        {
            return _task.Value;
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        private static void OnCompletedInternal(AsyncSubject<TResult> task, Action continuation, bool continueOnCapturedContext)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            SynchronizationContext sc = continueOnCapturedContext ? SynchronizationContext.Current : null;
            if (sc != null && sc.GetType() != typeof(SynchronizationContext))
            {
                task.Subscribe(param0 =>
                {
                    try
                    {
                        sc.Post(state => ((Action)state).Invoke(), continuation);
                    }
                    catch (Exception exception)
                    {
                        AsyncServices.ThrowAsync(exception, null);
                    }
                });//, CancellationToken.None);, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return;
            }

            IScheduler taskScheduler = Scheduler.ThreadPool;
            if (task.IsCompleted)
            {
                //Task.Factory.StartNew(s => ((Action)s).Invoke(), continuation, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                continuation();
                return;
            }

            if (taskScheduler != Scheduler.ThreadPool)
            {
                //task.ContinueWith(_ => RunNoException(continuation), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, taskScheduler);
                task.SubscribeOn(taskScheduler).Subscribe(_ => RunNoException(continuation));
                return;
            }

            task.Subscribe(param0 =>
            {
                if (IsValidLocationForInlining)
                {
                    RunNoException(continuation);
                    return;
                }

                //Task.Factory.StartNew(s => RunNoException((Action)s), continuation, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                continuation();
            });//, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>Whether the current thread is appropriate for inlining the await continuation.</summary>
        private static bool IsValidLocationForInlining
        {
            get
            {
                SynchronizationContext current = SynchronizationContext.Current;
                return (current == null || current.GetType() == typeof(SynchronizationContext));
            }
        }

        /// <summary>
        /// Invokes the delegate in a try/catch that will propagate the exception asynchronously on the thread pool.
        /// </summary>
        /// <param name="continuation"></param>
        private static void RunNoException(Action continuation)
        {
            try
            {
                continuation.Invoke();
            }
            catch (Exception exception)
            {
                AsyncServices.ThrowAsync(exception, null);
            }
        }
    }
}
