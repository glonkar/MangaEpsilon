using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace MangaEpsilon
{
    public static class AsyncExtensions
    {
        public static Task WaitForStoryboardCompletion(this Storyboard storyBoard)
        {
            TaskCompletionSource<object> task = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = new EventHandler((obj, args) =>
            {
                storyBoard.Completed -= handler;
                task.SetResult(0);
            });

            storyBoard.Completed += handler;

            return task.Task;
        }
    }
}
