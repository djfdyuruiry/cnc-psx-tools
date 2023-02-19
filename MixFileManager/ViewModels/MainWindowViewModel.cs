using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using YamlDotNet.Serialization;

using CncPsxLib;
using System.IO;
using System.Collections.Generic;
using System.Text;

using static CncPsxLib.FileConstants;

namespace MixFileManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ITreeViewEventHandler
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
        private FatFileEntry? _currentEntry;
        private bool _shouldShowDetails;
        private bool _shouldShowText;
        private string? _currentEntryYaml;
        private string? _currentEntryText;

        public string WindowTitle 
        {
            get => _windowTitle;
            set => this.RaiseAndSetIfChanged(ref _windowTitle, value);
        }

        public ObservableCollection<FatFileEntry> FileEntries { get; } 
            = new ObservableCollection<FatFileEntry>();

        public FatFileEntry? CurrentEntry
        {
            get => _currentEntry;
            set => this.RaiseAndSetIfChanged(ref _currentEntry, value);
        }
        public bool ShouldShowDetails
        {
            get => _shouldShowDetails;
            set => this.RaiseAndSetIfChanged(ref _shouldShowDetails, value);
        }
        public bool ShouldShowText
        {
            get => _shouldShowText;
            set => this.RaiseAndSetIfChanged(ref _shouldShowText, value);
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

        public ReactiveCommand<Window, Unit> LoadMixFile { get; }

        public ReactiveCommand<Window, Unit> LoadXaFile { get; }

        public ReactiveCommand<Unit, Unit> ShowDetails { get; }

        public ReactiveCommand<Unit, Unit> ShowText { get; }

        public ReactiveCommand<Window, Unit> ExtractFile { get; }

        public MainWindowViewModel()
        {
            _windowTitle = DEFAULT_WINDOW_TITLE;

            _fileOpenDialog = new OpenFileDialog()
            {
                Directory = "../../../../reference-files/tiberian-dawn/nod",
                InitialFileName = "DATA.FAT",
                Filters = new List<FileDialogFilter>()
            };

            _shouldShowDetails = true;

            LoadMixFile = ReactiveCommand.CreateFromTask<Window>(w => DoLoadFile(w, true));
            LoadXaFile = ReactiveCommand.CreateFromTask<Window>(w => DoLoadFile(w));
            ShowDetails = ReactiveCommand.Create(() => { ShouldShowDetails = true; ShouldShowText = false; });
            ShowText = ReactiveCommand.Create(() => { ShouldShowDetails = false; ShouldShowText = true; });
            ExtractFile = ReactiveCommand.CreateFromTask<Window>(DoExtractFile);
        }

        public MainWindowViewModel(FatFileReader fatFileReader, ISerializer yamlSerializer) : this()
        {
            _fatFileReader = fatFileReader;
            _yamlSerialiser = yamlSerializer;
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

            var filePath = filePathResult.First();
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
                loadingMixFile ? MIX_EXTENSION : XA_EXTENSION
            );

            if (!File.Exists(_mixFilePath))
            {
                // TODO: raise error
                return;
            }

            FileEntries.Clear();
            FileEntries.AddRange(loadingMixFile ? _fatFile.MixFileEntries : _fatFile.XaFileEntries);

            WindowTitle = $"{DEFAULT_WINDOW_TITLE} - {filePath}";
        }

        public async Task OnSelectedItemChangeAsync(string _, object? item)
        {
            if(item is null || item is not FatFileEntry)
            {
                return;
            }

            CurrentEntry = item as FatFileEntry;
            CurrentEntryYaml = _yamlSerialiser.Serialize(CurrentEntry!);
            CurrentEntryText = "<Entry is not a text file>";

            if (TEXT_EXTENSIONS.Contains(CurrentEntry!.FileExtension.ToUpper()))
            {
                if (_mixFilePath is null)
                {
                    return;
                }

                using (var reader = MixFileReader.Open(_mixFilePath))
                {
                    CurrentEntryText = Encoding.ASCII.GetString(await reader.ReadFile(CurrentEntry));
                }
            }
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

            using (var writer = File.OpenWrite(extractPath))
            {
                using (var reader = MixFileReader.Open(_mixFilePath))
                {
                    writer.Write(await reader.ReadFile(CurrentEntry));
                }
            }
        }
    }
}
