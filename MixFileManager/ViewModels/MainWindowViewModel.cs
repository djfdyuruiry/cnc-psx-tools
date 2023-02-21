using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using YamlDotNet.Serialization;

using CncPsxLib;

using static CncPsxLib.FileConstants;

namespace MixFileManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string DEFAULT_WINDOW_TITLE = "Mix File Manager";

        private readonly static FileDialogFilter FAT_FILTER;
        private readonly static FileDialogFilter MIX_FILTER;
        private readonly static FileDialogFilter XA_FILTER;

        static MainWindowViewModel()
        {
            FAT_FILTER = new FileDialogFilter
            {
                Name = $"File Allocation Table (.{FAT_EXTENSION.ToLower()})",
                Extensions = new List<string> { FAT_EXTENSION }
            };

            MIX_FILTER = new FileDialogFilter
            {
                Name = $"MIX Archive ({MIX_EXTENSION.ToLower()})",
                Extensions = new List<string> { MIX_EXTENSION }
            };

            XA_FILTER = new FileDialogFilter
            {
                Name = $"XA Archive (.{XA_EXTENSION.ToLower()})",
                Extensions = new List<string> { XA_EXTENSION }
            };
        }

        private readonly OpenFileDialog _fileOpenDialog;
        private readonly FatFileReader _fatFileReader;
        private readonly ISerializer _yamlSerialiser;

        private string _windowTitle;
        private FatFile? _fatFile;
        private string? _mixFilePath;
        private bool _isXaMixFile;
        private bool _entryIsSelected;
        private FatFileEntry? _currentEntry;
        private bool _currentEntryIsTextFile;
        private bool _viewingDetails;
        private bool _viewingText;
        private bool _editingText;
        private string? _currentEntryYaml;
        private string? _currentEntryText;
        private string? _currentEntryEditableText;

        public string WindowTitle 
        {
            get => _windowTitle;
            set => this.RaiseAndSetIfChanged(ref _windowTitle, value);
        }

        public ObservableCollection<FatFileEntry> FileEntries { get; } 
            = new ObservableCollection<FatFileEntry>();

        public bool EntryIsSelected
        {
            get => _entryIsSelected;
            set => this.RaiseAndSetIfChanged(ref _entryIsSelected, value);
        }

        public FatFileEntry? CurrentEntry
        {
            get => _currentEntry;
            set => this.RaiseAndSetIfChanged(ref _currentEntry, value);
        }

        public bool CurrentEntryIsTextFile
        {
            get => _currentEntryIsTextFile;
            set => this.RaiseAndSetIfChanged(ref _currentEntryIsTextFile, value);
        }

        public bool ViewingDetails
        {
            get => _viewingDetails;
            set => this.RaiseAndSetIfChanged(ref _viewingDetails, value);
        }

        public bool ViewingText
        {
            get => _viewingText;
            set => this.RaiseAndSetIfChanged(ref _viewingText, value);
        }
        public bool EditingText
        {
            get => _editingText;
            set => this.RaiseAndSetIfChanged(ref _editingText, value);
        }

        public string? CurrentEntryYaml
        {
            get => _currentEntryYaml;
            set => this.RaiseAndSetIfChanged(ref _currentEntryYaml, value);
        }

        public string? CurrentEntryText
        {
            get => _currentEntryText;
            set => this.RaiseAndSetIfChanged(ref _currentEntryText, value);
        }

        public string? CurrentEntryEditableText
        {
            get => _currentEntryEditableText;
            set => this.RaiseAndSetIfChanged(ref _currentEntryEditableText, value);
        }

        public ReactiveCommand<Window, Unit> LoadMixFile { get; }

        public ReactiveCommand<Window, Unit> LoadXaFile { get; }

        public ReactiveCommand<Unit, Unit> ViewDetails { get; }

        public ReactiveCommand<Unit, Unit> ViewText { get; }

        public ReactiveCommand<Unit, Unit> EditText { get; }

        public ReactiveCommand<Window, Unit> ExtractFile { get; }

        public ReactiveCommand<Window, Unit> ReplaceFile { get; }

        public ReactiveCommand<Unit, Unit> SaveTextEdits { get; }

        public MainWindowViewModel()
        {
            _fileOpenDialog = new OpenFileDialog()
            {
                Directory = "../../../../reference-files/tiberian-dawn/nod",
                InitialFileName = "DATA.FAT",
                Filters = new List<FileDialogFilter>()
            };

            Reset();

            LoadMixFile = ReactiveCommand.CreateFromTask<Window>(w => DoLoadFile(w, true));
            LoadXaFile = ReactiveCommand.CreateFromTask<Window>(w => DoLoadFile(w));
            ViewDetails = ReactiveCommand.Create(() => { ViewingDetails = true; ViewingText = false; EditingText = false; });
            ViewText = ReactiveCommand.Create(() => { ViewingDetails = false; ViewingText = true; EditingText = false; });
            EditText = ReactiveCommand.Create(() => { ViewingDetails = false; ViewingText = false; EditingText = true; });
            SaveTextEdits = ReactiveCommand.CreateFromTask(DoSaveTextEdits);
            ExtractFile = ReactiveCommand.CreateFromTask<Window>(DoExtractFile);
            ReplaceFile = ReactiveCommand.CreateFromTask<Window>(DoReplaceFile);
        }

        public MainWindowViewModel(FatFileReader fatFileReader, ISerializer yamlSerializer) : this()
        {
            _fatFileReader = fatFileReader;
            _yamlSerialiser = yamlSerializer;
        }

        private void ResetActivity()
        {
            ViewingDetails = false;
            ViewingText = false;
            EditingText = false;
        }

        private void ResetCurrentEntry()
        {
            EntryIsSelected = false;
            CurrentEntry = null;
            CurrentEntryIsTextFile = false;
            CurrentEntryYaml = null;
            CurrentEntryText = null;
            CurrentEntryEditableText = null;
        }

        private void Reset()
        {
            WindowTitle = DEFAULT_WINDOW_TITLE;
            _fileOpenDialog.Filters = new List<FileDialogFilter>();

            _fatFile = null;
            _mixFilePath = null;
            _isXaMixFile = false;

            FileEntries.Clear();
            
            ResetCurrentEntry();
            ResetActivity();
        }

        private async Task SetCurrentEntry(FatFileEntry entry)
        {
            ResetCurrentEntry();

            EntryIsSelected = true;
            CurrentEntry = entry;
            CurrentEntryYaml = _yamlSerialiser.Serialize(CurrentEntry!);
            CurrentEntryIsTextFile = CurrentEntry.IsTextFile;

            if (!CurrentEntry.IsTextFile || _mixFilePath is null)
            {
                return;
            }

            using var reader = MixFileReader.Open(_mixFilePath);
      
            CurrentEntryText = Encoding.ASCII.GetString(
                await reader.ReadFile(CurrentEntry)
            );
            CurrentEntryEditableText = new string(CurrentEntryText);
        }

        public async Task SelectFile(object? _, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1
               || e.AddedItems[0] is null
               || e.AddedItems[0] is not FatFileEntry)
            {
                return;
            }

            var currentEntry = e.AddedItems[0] as FatFileEntry;

            ResetActivity();
            ViewingDetails = true;

            await SetCurrentEntry(currentEntry!);
        }

        private async Task LoadFile(string filePath)
        {
            var fatFilePath = Path.GetExtension(filePath).ToUpper() == $".{FAT_EXTENSION}"
                ? filePath
                : Path.ChangeExtension(filePath, FAT_EXTENSION);

            if (!File.Exists(fatFilePath))
            {
                // TODO: raise error
                return;
            }

            _fatFile = await _fatFileReader.Read(fatFilePath);
            _mixFilePath = Path.ChangeExtension(
                fatFilePath,
                _isXaMixFile ? XA_EXTENSION : MIX_EXTENSION
            );

            if (!File.Exists(_mixFilePath))
            {
                // TODO: raise error
                return;
            }


            FileEntries.Clear();
            FileEntries.AddRange(_isXaMixFile ? _fatFile.XaFileEntries : _fatFile.MixFileEntries);

            WindowTitle = $"{DEFAULT_WINDOW_TITLE} - {filePath}";
        }

        public async Task DoLoadFile(Window activeWindow, bool loadingMixFile = false)
        {
            _fileOpenDialog.Filters!.Clear();

            _fileOpenDialog.Filters!.Add(FAT_FILTER);
            _fileOpenDialog.Filters!.Add(loadingMixFile ? MIX_FILTER : XA_FILTER);

            var filePathResult = await _fileOpenDialog.ShowAsync(activeWindow);

            if (filePathResult is null || filePathResult.Length < 1)
            {
                return;
            }

            Reset();

            _isXaMixFile = !loadingMixFile;

            await LoadFile(filePathResult.First());
        }

        public async Task DoExtractFile(Window activeWindow)
        {
            if (_mixFilePath is null || CurrentEntry is null)
            {
                return;
            }

            var saveDialog = new SaveFileDialog()
            {
                Directory = _fileOpenDialog.Directory,
                InitialFileName = CurrentEntry.FileName,
                Filters = new List<FileDialogFilter> { 
                    new FileDialogFilter
                    {
                        Extensions = new List<string> { CurrentEntry.FileExtension }
                    }
                }
            };

            var extractPath = await saveDialog.ShowAsync(activeWindow);

            if (extractPath is null || extractPath.Length < 1)
            {
                return;
            }

            using var writer = File.OpenWrite(extractPath);
            using var reader = MixFileReader.Open(_mixFilePath);

            writer.Write(await reader.ReadFile(CurrentEntry));
        }

        private async Task ReplaceFileContents(Stream readStream)
        {
            if (_fatFile is null || _mixFilePath is null || CurrentEntry is null)
            {
                return;
            }

            var mixFileManager = new MixFileEditor(_fatFile, _mixFilePath);

            await mixFileManager.ReplaceFile(CurrentEntry, readStream);

            await LoadFile(_fatFile.Path);
            await SetCurrentEntry(FileEntries.First(e => e.Index == CurrentEntry.Index));
        }

        public async Task DoSaveTextEdits()
        {
            if (CurrentEntryEditableText is null)
            {
                return;
            }

            using var textStream = new MemoryStream(
                Encoding.ASCII.GetBytes(CurrentEntryEditableText)
            );

            await ReplaceFileContents(textStream);
        }

        public async Task DoReplaceFile(Window activeWindow)
        {
            if(CurrentEntry is null)
            {
                return;
            }

            _fileOpenDialog.Filters!.Clear();
            _fileOpenDialog.Filters!.Add(
                new FileDialogFilter
                {
                    Extensions = new List<string> { CurrentEntry.FileExtension }
                }
            );

            var filePathResult = await _fileOpenDialog.ShowAsync(activeWindow);

            if (filePathResult is null || filePathResult.Length < 1)
            {
                return;
            }

            using var readStream = File.OpenRead(filePathResult.First());

            await ReplaceFileContents(readStream);
        }
    }
}
