using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weald.Service
{
    public interface IRepeatActions
    {
        void StartAction(Action action, TimeSpan interval, CancellationToken token);
    }

    public class ActionRepeater : IRepeatActions
    {
        public void StartAction(Action action, TimeSpan interval, CancellationToken token)
        {
            Task.Factory.StartNew(() =>
                {
                    for (;;)
                    {
                        if (token.WaitHandle.WaitOne(interval))
                        {
                            break;
                        }

                        action();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}