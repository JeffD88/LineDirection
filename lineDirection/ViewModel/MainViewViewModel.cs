using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using lineDirection.Resources;
using LineDirection.Commands;
using LineDirection.Services;

using MessageBox = System.Windows.Forms.MessageBox;

namespace LineVector.ViewModel
{
    class MainViewViewModel : INotifyPropertyChanged
    {

        #region Fields

        private readonly ILineDirectionService lineDirectionService;

        private readonly Window mainView;

        private string selectedFile;

        private int outputTypeIndex;

        private Brush validationBrush;

        private SolidColorBrush errorBrush = new SolidColorBrush(Color.FromRgb(153, 0, 0));

        private SolidColorBrush okBrush = new SolidColorBrush(Color.FromRgb(0, 153, 0));

        private bool okEnabled;

        #endregion

        #region Constructor

        public MainViewViewModel(ILineDirectionService lineDirectionService, Window mainView)
        {
            this.lineDirectionService = lineDirectionService;
            this.mainView = mainView;

            this.SelectFileCommand = new DelegateCommand(this.OnSelectFileCommand);
            this.OkCommand = new DelegateCommand(this.OnOkCommand);

            this.selectedFile = Resource.SelectedFileText;
            this.outputTypeIndex = 0;
            this.ValidationBrush = this.errorBrush;
            this.okEnabled = false;
        }

        #endregion

        #region Commands

        public ICommand SelectFileCommand { get; }

        public ICommand OkCommand { get; }

        #endregion

        #region Properties

        public string SelectedFile
        {
            get => this.selectedFile;

            set
            {
                this.selectedFile = value;
                this.OnPropertyChanged(nameof(this.SelectedFile));
            }
        }

        public int OutputTypeIndex
        {
            get => this.outputTypeIndex;

            set
            {
                this.outputTypeIndex = value;
                this.OnPropertyChanged(nameof(this.OutputTypeIndex));
            }
        }

        public Brush ValidationBrush
        {
            get => this.validationBrush;

            set
            {
                this.validationBrush = value;
                this.OnPropertyChanged(nameof(this.ValidationBrush));
            }
        }

        public bool OkEnabled
        {
            get => this.okEnabled;

            set
            {
                this.okEnabled = value;
                this.OnPropertyChanged(nameof(this.OkEnabled));
            }
        }

        #endregion

        #region Methods

        private void OnSelectFileCommand(object parameter)
        {
            var fileBrowser = new OpenFileDialog
            {
                Title = $"{Resource.FileBrowserTitle}",
                Filter = $"{Resource.FileBrowserFilter}"
            };
            DialogResult result = fileBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                SelectedFile = fileBrowser.FileName;
                this.ValidationBrush = this.okBrush;
                this.OkEnabled = true;
                
            }
        }

        private void OnOkCommand(object parameter)
        {
            bool linesFound = this.lineDirectionService.ProcessLinesInFile(selectedFile, (OutputType)this.OutputTypeIndex);
            if (linesFound)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = $"{Resource.SaveFileTitle}",
                    DefaultExt = $"{Resource.SaveFileDefaultExt}",
                    AddExtension = true,
                    Filter = $"{Resource.SaveFileFilter}"
                };

                DialogResult result = saveFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string writtenFile = this.lineDirectionService.WriteCSV(saveFileDialog.FileName);
                    MessageBox.Show($"{Resource.FileWrittenMessage}{Environment.NewLine}{writtenFile}", $"{Resource.FileWrittenTitle}",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mainView?.Close();
                }
            }
            else
            {
                MessageBox.Show($"{Resource.NoLinesFoundMessage}", $"{Resource.NoLinesfoundTitle}",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Notify Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        #endregion
    }
}
