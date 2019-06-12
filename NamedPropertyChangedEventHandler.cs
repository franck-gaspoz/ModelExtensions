using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelExtensions
{
    public class NamedPropertyChangedEventHandler
    {
        readonly string _PropertyName;
        readonly BindableModel _NotifiableModel;
        readonly Action<object, PropertyChangedEventArgs> _HandlerAction;

        public NamedPropertyChangedEventHandler(
            string propertyName,
            BindableModel notifiableModel,
            Action<object,PropertyChangedEventArgs> handlerAction
            )
        {
            _PropertyName = propertyName;
            _NotifiableModel = notifiableModel;
            _HandlerAction = handlerAction;
            notifiableModel.PropertyChanged += NamedPropertyChanged;
        }

        public void NamedPropertyChanged
            (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _PropertyName)
                _HandlerAction?.Invoke(sender, e);
        }
    }
}
