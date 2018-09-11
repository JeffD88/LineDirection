using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
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

        private string selectedFile = "";

        private int outputTypeIndex;

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
                this.OnPropertyChanged(nameof(this.selectedFile));
            }
        }

        public int OutputTypeIndex
        {
            get => this.outputTypeIndex;

            set
            {
                this.outputTypeIndex = value;
                this.OnPropertyChanged(nameof(this.outputTypeIndex));
            }
        }

        #endregion

        #region Methods

        private void OnSelectFileCommand(object parameter)
        {
            var fileBrowser = new OpenFileDialog
            {
                Title = "Select CSV file",
                Filter = "Comma-separated values file|*.csv"
            };
            DialogResult result = fileBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                SelectedFile = fileBrowser.FileName;
            }
        }

        private void OnOkCommand(object parameter)
        {
            if (selectedFile != Resource.SelectedFileText)
            {
                bool linesFound = this.lineDirectionService.ProcessLinesInFile(selectedFile, (OutputType)this.OutputTypeIndex);
                if (linesFound)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Title = "Save File As",
                        DefaultExt = ".csv",
                        AddExtension = true,
                        Filter = "Comma-separated values file|*.csv|Text file|*.txt"
                    };

                    DialogResult result = saveFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string writtenFile = this.lineDirectionService.WriteCSV(saveFileDialog.FileName);
                        MessageBox.Show($"File written{Environment.NewLine}{writtenFile}", "File Written",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.mainView?.Close();
                    }
                }
                else
                {
                    MessageBox.Show($"No lines found in file.", "No Lines Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"Please select a file.", "No File Selected",
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
