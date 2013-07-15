using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangaEpsilon
{
    public static class Throttle
    {
        private static ConcurrentQueue<Tuple<Action, Func<bool>>> queue = new ConcurrentQueue<Tuple<Action, Func<bool>>>();
        private static volatile bool Running = false;
        private static async void RunQueue()
        {
            Running = true;

            await Task.Run(() =>
                {
                    while (queue.Count > 0)
                    {
                        Tuple<Action, Func<bool>> action = null;
                        if (queue.TryPeek(out action))
                        {
                            if (action.Item2())
                                action.Item1();
                            queue.TryDequeue(out action);
                        }

                        if (queue.Count > 0)
                        {
                            Tuple<Action, Func<bool>> nextAction = null;
                            if (queue.TryDequeue(out nextAction))
                            {
                                if (nextAction.Item2())
                                    Thread.Sleep(1000);
                            }
                        }
                    }
                });

            Running = false;
        }

        public static void QueueWork(Tuple<Action, Func<bool>> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            queue.Enqueue(action);

            if (!Running)
                RunQueue();
        }
    }
}
