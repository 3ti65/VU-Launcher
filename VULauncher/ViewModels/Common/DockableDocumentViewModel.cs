using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using VULauncher.Commands;

namespace VULauncher.ViewModels.Common
{
    public class DockableDocumentViewModel : ViewModel
    {
        private bool _isClosed;
        private bool _canClose = true;
        private string _title;
        private bool _isSelected;

        public ICommand CloseCommand { set; get; }

        public DockableDocumentViewModel()
        {
            //CloseCommand = new RelayCommand(x =>
            //{
            //    IsClosed = true;
            //});


            this.CanClose = true;
        }

        public bool IsClosed
        {
            get => _isClosed;
            set => SetField(ref _isClosed, value);
        }

        public bool CanClose
        {
            get => _canClose;
            set => SetField(ref _canClose, value);
        }

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }
    }
}
