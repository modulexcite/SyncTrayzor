﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SyncTrayzor.Utils;

namespace SyncTrayzor.Xaml
{
    public class ActivateBehaviour : DetachingBehaviour<Window>
    {
        private IDisposable registration;


        public IObservable<bool> ActivateObservable
        {
            get { return (IObservable<bool>)GetValue(ActivateObservableProperty); }
            set { SetValue(ActivateObservableProperty, value); }
        }

        public static readonly DependencyProperty ActivateObservableProperty =
            DependencyProperty.Register("ActivateObservable", typeof(IObservable<bool>), typeof(ActivateBehaviour), new PropertyMetadata(null, (d, e) =>
            {
                ((ActivateBehaviour)d).ObservableChanged(e.NewValue as IObservable<bool>);
            }));

        private void ObservableChanged(IObservable<bool> newValue)
        {
            this.registration?.Dispose();
            this.registration = newValue?.Subscribe(_ => this.Activate());
        }

        private void Activate()
        {
            if (this.AssociatedObject.WindowState == WindowState.Minimized)
                this.AssociatedObject.WindowState = WindowState.Normal;
            this.AssociatedObject.Activate();
        }
    }
}
