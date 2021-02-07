using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VULauncher.ViewModels.Common
{
    public class IsDirtyChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; }

        public IsDirtyChangedEventArgs(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }

    public delegate void IsDirtyChangedEventHandler(object sender, IsDirtyChangedEventArgs e);

    public abstract class ViewModel : NotifyPropertyChangedBase, 
                                      IDisposable, 
                                      IObservableObject
    {
        private bool _isDirty;

        public event IsDirtyChangedEventHandler IsDirtyChanged;

        public void NotifyIsDirtyChanged(string propertyName)
        {
            IsDirtyChanged?.Invoke(this, new IsDirtyChangedEventArgs(propertyName));
        }

        public virtual bool IsDirty {
            //[DebuggerStepThrough]
            get => _isDirty;

            //[DebuggerStepThrough]
            set => SetField(ref _isDirty, value, notifyPropertyChanged: true, setDirty: false);
        }

        protected bool SetField<T>(ref T field, 
                                   T value, 
                                   bool notifyPropertyChanged = true, 
                                   bool setDirty = false, 
                                   [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                if (setDirty)
                {
                    bool wasClean = _isDirty == false;

                    IsDirty = true;

                    if (wasClean)
                    {
                        NotifyIsDirtyChanged(propertyName);
                    }
                }

                if (notifyPropertyChanged)
                {
                    NotifyPropertyChanged(propertyName);
                }

                return true;
            }

            return false;
        }

        public virtual void Dispose()
        {
        }
    }
}
