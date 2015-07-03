#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.Task))]
[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.Task<>))]

#else

using UniRx;

namespace System.Threading.Tasks
{
    public class Task
    {
        private readonly AsyncSubject<object> _t;

        protected Task() { }
        internal Task(AsyncSubject<object> t) { _t = t; }
        protected Task(IObservable<object> t)
        {
            var s = new AsyncSubject<object>();
            t.Subscribe(s);
            _t = s;
        }

        public IObservable<object> AsObservable() => _t;

        public virtual void Wait()
        {
            while (!_t.IsCompleted)
                Thread.Sleep(10);
        }

        public UniRx.Runtime.CompilerServices.AsyncSubjectAwaiter<object> GetAwaiter() => _t.GetAwaiter();
    }

    public class Task<T> : Task
    {
        private readonly AsyncSubject<T> _t;

        internal Task(AsyncSubject<T> t) : base(t.Cast<T, object>()) { _t = t; }

        public new IObservable<T> AsObservable() => _t;

        public override void Wait()
        {
            while (!_t.IsCompleted)
                Thread.Sleep(10);
        }

        public T Result => _t.Value;

        public new UniRx.Runtime.CompilerServices.AsyncSubjectAwaiter<T> GetAwaiter() => _t.GetAwaiter();
    }

    internal class TaskCompletionSource<TResult>
    {
        AsyncSubject<TResult> _tcs = new AsyncSubject<TResult>();
        public Task<TResult> Task => new Task<TResult>(_tcs);

        public bool TrySetResult(TResult result)
        {
            _tcs.OnNext(result);
            _tcs.OnCompleted();
            return true;
        }

        public bool TrySetCanceled()
        {
            _tcs.Dispose();
            _tcs.OnCompleted();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            _tcs.OnError(exception);
            _tcs.OnCompleted();
            return true;
        }
    }
}

#endif
