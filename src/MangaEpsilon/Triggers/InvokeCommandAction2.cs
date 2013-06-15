using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace MangaEpsilon.Triggers
{
    // Members of this namespace were created for debugging purposes only. They DO work but I recommend the standard versions instead of mine.
    public class InvokeCommandAction2 : System.Windows.Interactivity.TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeCommandAction2));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeCommandAction2));
    }
}
