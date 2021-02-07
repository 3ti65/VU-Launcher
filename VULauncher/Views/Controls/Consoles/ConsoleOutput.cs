using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;


namespace VULauncher.Views.Controls.Consoles
{
    public sealed class ConsoleOutput : RichTextBox
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(ObservableCollection<Inline>),
            typeof(ConsoleOutput),
            new PropertyMetadata(default(ObservableCollection<Inline>), OnItemsSourceChanged));

        private readonly Paragraph _paragraph;

        private INotifyCollectionChanged _notifyChanged;

        public ConsoleOutput()
        {
            _paragraph = new Paragraph();

            IsUndoEnabled = false;

            Document = new FlowDocument(_paragraph);

            DataObject.AddCopyingHandler(this, CopyCommand);
        }

        public ObservableCollection<Inline> ItemsSource
        {
            get => (ObservableCollection<Inline>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs args)
        {
            base.OnPreviewKeyDown(args);

            switch (args.Key)
            {
                case Key.A:
                    args.Handled = HandleSelectAllKeys();
                    break;
                case Key.C:
                    args.Handled = HandleCopyKeys();
                    break;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue) return;

            ConsoleOutput consoleOutput = (ConsoleOutput)d;
            consoleOutput.HandleItemsSourceChanged((ObservableCollection<Inline>)args.NewValue);
            consoleOutput.ScrollToEnd();
        }

        private void CopyCommand(object sender, DataObjectCopyingEventArgs args)
        {
            if (!string.IsNullOrEmpty(Selection.Text))
            {
                args.DataObject.SetData(typeof(string), Selection.Text);
            }

            args.Handled = true;
        }

        private void HandleItemsSourceChanged(ObservableCollection<Inline> items)
        {
            using (DeclareChangeBlock())
            {
                if (items is INotifyCollectionChanged changed)
                {
                    var notifyChanged = changed;
                    if (_notifyChanged != null) _notifyChanged.CollectionChanged -= HandleItemsChanged;

                    _notifyChanged = notifyChanged;
                    _notifyChanged.CollectionChanged += HandleItemsChanged;

                    var existingItems = items.Cast<Inline>().ToArray();
                    if (existingItems.Any())
                        ReplaceItems(existingItems);
                    else
                        ClearItems();
                }
                else
                {
                    ReplaceItems(ItemsSource.Cast<Inline>().ToArray());
                }
            }
        }

        private void HandleItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(args.NewItems.Cast<Inline>().ToArray());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(args.OldItems.Cast<Inline>().ToArray());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReplaceItems(((IEnumerable)sender).Cast<Inline>().ToArray());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(args.OldItems.Cast<Inline>().ToArray());
                    AddItems(args.NewItems.Cast<Inline>().ToArray());
                    break;
            }
        }

        private void ClearItems()
        {
            if (CheckAccess())
            {
                _paragraph.Inlines.Clear();
            }
        }

        private void ReplaceItems(Inline[] items)
        {
            ClearItems();

            AddItems(items);
        }

        private void AddItems(Inline[] items)
        {
            if (items.Length > 0 && CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    CaretPosition = CaretPosition.DocumentEnd;

                    _paragraph.Inlines.AddRange(items);

                    CaretPosition = Document.ContentEnd;
                });
            }
        }

        private void RemoveItems(Inline[] items)
        {
            if (CheckAccess())
            {
                foreach (var item in items)
                {
                    var inline = _paragraph.Inlines
                        .First(x => x == item);

                    if (inline != null)
                    {
                        _paragraph.Inlines.Remove(inline);
                    }
                }
            }
        }

        private bool HandleCopyKeys()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) return false;

            return CaretPosition.CompareTo(Document.ContentEnd) < 0 || Selection.Start.CompareTo(CaretPosition) < 0;
        }

        private bool HandleSelectAllKeys()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Selection.Select(Document.ContentStart, Document.ContentEnd);

                return true;
            }

            return HandleAnyOtherKey();
        }

        private bool HandleAnyOtherKey()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) return false;
            return CaretPosition.CompareTo(Document.ContentEnd) < 0;
        }
    }
}
