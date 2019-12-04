﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace WPF_StatusNotification.Base
{
    public class NotifierViewBase : Window
    {
        public ShowOptions Options { get; set; } = new ShowOptions();

        internal static void OverrideLoaded(object sender, RoutedEventArgs e)
        {
            var notifier = sender as NotifierViewBase;

            notifier.UpdateLayout();

            DoubleAnimation animation = new DoubleAnimation();

            var top = System.Windows.SystemParameters.WorkArea.Height;
            var helper= MemoryMapFileHelper<List<MyRectangular>>.GetHelper();
            var list = helper.Read();
            if (list!=null)
            {
                double topExisted = list.Select(t => t.Top).Sum()+10;
                top -= topExisted;
            }

            if (double.IsNaN(notifier.Options.NotifierTop))
            {
                notifier.Top = top - notifier.ActualHeight;
            }
            else
            {
                notifier.Top = notifier.Options.NotifierTop;
            }

            if (double.IsNaN(notifier.Options.RightTo))
                notifier.Options.RightTo = System.Windows.SystemParameters.WorkArea.Right - notifier.ActualWidth;

            if (double.IsNaN(notifier.Options.RightFrom))
                notifier.Options.RightFrom = System.Windows.SystemParameters.WorkArea.Right;

            animation.Duration = notifier.Options.AnamitionDurationTime;
            animation.From = notifier.Options.RightFrom;
            animation.To = notifier.Options.RightTo;

            notifier.BeginAnimation(Window.LeftProperty, animation);


            if (notifier.Options.IsAutoClose)
            {
                Task.Factory.StartNew(delegate
                {
                    int seconds = notifier.Options.ShowTimeMS;
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(seconds));
                    notifier.Dispatcher.Invoke(new Action(() =>
                        {
                            {
                                animation = new DoubleAnimation();
                                animation.Duration = notifier.Options.AnamitionDurationTime;
                                animation.Completed += (s, a) => { notifier.Close(); };
                                animation.From = notifier.Options.RightTo;
                                animation.To = notifier.Options.RightFrom;
                                notifier.BeginAnimation(Window.LeftProperty, animation);
                            }
                        }));
                });
            }
        }

        internal static void OverrideClosing(object sender, CancelEventArgs e)
        {
            var notifier = sender as NotifierViewBase;
            e.Cancel = true;
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = notifier.Options.AnamitionDurationTime;
            animation.Completed += (s, a) => { notifier.Closing -= OverrideClosing; notifier.Close(); };
            animation.From = notifier.Options.RightTo;
            animation.To = notifier.Options.RightFrom;
            notifier.BeginAnimation(Window.LeftProperty, animation);
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
