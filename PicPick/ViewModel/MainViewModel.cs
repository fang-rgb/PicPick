using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace PicPick.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                loadData(folder);
            }
            catch (Exception ex)
            {

            }
        }

        #region Private Methods
        private void loadData(string folder)
        {
            string[] fileArray = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

            List<string> picFileList = new List<string>();
            foreach (string s in fileArray)
            {
                if (RGB.General.PictureAssistor.IsPicture(s))
                {
                    picFileList.Add(s);
                }
            }

            SourceItems = new ObservableCollection<string>(picFileList);
            PickedItems = new ObservableCollection<string>();
        }
        #endregion
        #region Properties
        /// <summary>
        /// The <see cref="sourceList" /> property's name.
        /// </summary>
        public const string SourceItemsPropertyName = "SourceItems";

        private ObservableCollection<string> _sourceItems = null;

        /// <summary>
        /// Sets and gets the sourceItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> SourceItems
        {
            get
            {
                return _sourceItems;
            }

            set
            {
                if (_sourceItems == value)
                {
                    return;
                }

                _sourceItems = value;
                RaisePropertyChanged(SourceItemsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedItem" /> property's name.
        /// </summary>
        public const string SelectedItemPropertyName = "SelectedItem";

        private string _selectedItem = null;

        /// <summary>
        /// Sets and gets the SelectedItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                if (_selectedItem == value)
                {
                    return;
                }

                _selectedItem = value;
                RaisePropertyChanged(SelectedItemPropertyName);

                PickCommand.RaiseCanExecuteChanged();
                CancelPickCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="PickedItem" /> property's name.
        /// </summary>
        public const string PickedItemsPropertyName = "PickedItems";

        private ObservableCollection<string> _pickedItems = null;

        /// <summary>
        /// Sets and gets the PickedItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> PickedItems
        {
            get
            {
                return _pickedItems;
            }

            set
            {
                if (_pickedItems == value)
                {
                    return;
                }

                _pickedItems = value;
                RaisePropertyChanged(PickedItemsPropertyName);

                ExportCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #region Command
        private RelayCommand _pickCommand;

        /// <summary>
        /// Gets the PickCommand.
        /// </summary>
        public RelayCommand PickCommand
        {
            get
            {
                return _pickCommand ?? (_pickCommand = new RelayCommand(
                    ExecutePickCommand,
                    CanExecutePickCommand));
            }
        }

        private void ExecutePickCommand()
        {
            if (CanExecutePickCommand())
            {
                string temp = SelectedItem;
                int idx = SourceItems.IndexOf(temp);
                SourceItems.Remove(temp);
                PickedItems.Add(temp);
                SelectedItem = SourceItems.Count > 0 ? (idx >= 0 && idx < SourceItems.Count ? SourceItems[idx] : SourceItems[idx - 1]) : null;
            }
        }

        private bool CanExecutePickCommand()
        {
            bool isSelectedFromSource = SourceItems.Contains(SelectedItem) == true;
            return SelectedItem != null && isSelectedFromSource;
        }

        private RelayCommand _cancelPickCommand;

        /// <summary>
        /// Gets the CancelPickCommand.
        /// </summary>
        public RelayCommand CancelPickCommand
        {
            get
            {
                return _cancelPickCommand ?? (_cancelPickCommand = new RelayCommand(
                    ExecuteCancelPickCommand,
                    CanExecuteCancelPickCommand));
            }
        }

        private void ExecuteCancelPickCommand()
        {
            if (CanExecuteCancelPickCommand())
            {
                string temp = SelectedItem;
                int idx = PickedItems.IndexOf(temp);
                PickedItems.Remove(temp);
                SourceItems.Add(temp);
                SelectedItem = PickedItems.Count > 0 ? (idx >= 0 && idx < PickedItems.Count ? PickedItems[idx] : PickedItems[idx - 1]) : null;
            }
        }

        private bool CanExecuteCancelPickCommand()
        {
            return SelectedItem != null && PickedItems.Contains(SelectedItem) == true;
        }


        private RelayCommand _exportCommand;

        /// <summary>
        /// Gets the ExportCommand.
        /// </summary>
        public RelayCommand ExportCommand
        {
            get
            {
                return _exportCommand ?? (_exportCommand = new RelayCommand(
                    ExecuteExportCommand,
                    CanExecuteExportCommand));
            }
        }

        private void ExecuteExportCommand()
        {
            if (CanExecuteExportCommand())
            {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.FileName = DateTime.Today.ToString("yyyyMMdd")+ "_export.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fileName = ofd.FileName;
                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        foreach (string s in PickedItems)
                        {
                            sw.WriteLine(Path.GetFileName(s));
                        }
                    }
                }

            }
        }

        private bool CanExecuteExportCommand()
        {
            return PickedItems.Count > 0;
        }

        private RelayCommand _openFolderCommand;

        /// <summary>
        /// Gets the OpenFolderCommand.
        /// </summary>
        public RelayCommand OpenFolderCommand
        {
            get
            {
                return _openFolderCommand
                    ?? (_openFolderCommand = new RelayCommand(ExecuteOpenFolderCommand));
            }
        }

        private void ExecuteOpenFolderCommand()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                loadData(fbd.SelectedPath);
            }
        }
        #endregion

    }
}