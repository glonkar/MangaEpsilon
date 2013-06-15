using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace MangaEpsilon.Triggers
{
    // Members of this namespace were created for debugging purposes only. They DO work but I recommend the standard versions instead of mine.
    public class EventTrigger2 : System.Windows.Interactivity.TriggerBase<DependencyObject>
    {
        public EventTrigger2()
            : base()
        {
        }

        private EventInfo eventToBind = null;
        private dynamic handler = null;

        protected override void OnAttached()
        {
            base.OnAttached();

            eventToBind = AssociatedObject.GetType().GetEvent(EventName);

            handler = (dynamic)Delegate.CreateDelegate(eventToBind.EventHandlerType, this, this.GetType().GetMethod("AssociatedObject_EventHandled", BindingFlags.NonPublic | BindingFlags.Instance));

            eventToBind.AddEventHandler(this.AssociatedObject, handler);
        }

        private delegate void EmptyDelegate();
        private delegate void EmptyDelegate2(object p1);
        private delegate void EmptyDelegate3(object p1, object p2);

        protected override void OnDetaching()
        {
            base.OnDetaching();

            eventToBind.RemoveEventHandler(this.AssociatedObject, handler);

        }

        private void AssociatedObject_EventHandled(object sender, EventArgs e)
        {
            this.InvokeActions(sender);
        }

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(EventTrigger2));
    }
}
