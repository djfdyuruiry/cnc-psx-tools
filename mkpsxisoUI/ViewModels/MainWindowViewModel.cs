using System.IO;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Linq;

using Avalonia.Controls;
using ReactiveUI;

using mkpsxisoUI.Services;

namespace mkpsxisoUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string DEFAULT_BIN_PATH = "./mkpsxiso";

        private readonly ReleaseDownloader _releaseDownloader = new();
        private BinaryWrapper? _binaryWrapper;

        private string? _binaryPath;
        private string _version;

        private string? _discImagePath;
        private string? _outputPath;
        private string? _xmlOutputPath;

        private string? _xmlInputPath;

        public string? BinaryPath
        {
            get => _binaryPath;
            set => this.RaiseAndSetIfChanged(ref _binaryPath, value);
        }

        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }

        public string? DiscImagePath
        {
            get => _discImagePath;
            set => this.RaiseAndSetIfChanged(ref _discImagePath, value);
        }

        public string? OutputPath
        {
            get => _outputPath;
            set => this.RaiseAndSetIfChanged(ref _outputPath, value);
        }

        public string? XmlOutputPath
        {
            get => _xmlOutputPath;
            set => this.RaiseAndSetIfChanged(ref _xmlOutputPath, value);
        }

        public string? XmlInputPath
        {
            get => _xmlInputPath;
            set => this.RaiseAndSetIfChanged(ref _xmlInputPath, value);
        }

        public ReactiveCommand<Window, Unit> PickBinaryPath { get; }
        
        public ReactiveCommand<Unit, Unit> GetLatestRelease { get;  }
        
        public ReactiveCommand<Window, Unit> PickDiscImagePath { get; }
        
        public ReactiveCommand<Window, Unit> PickOutputPath { get; }
        
        public ReactiveCommand<Window, Unit> PickXmlOutputPath { get; }

        public ReactiveCommand<Unit, Unit> DumpIso { get; }

        public ReactiveCommand<Window, Unit> PickXmlInputPath { get; }

        public ReactiveCommand<Unit, Unit> MakeIso { get; }

        public MainWindowViewModel()
        {
            _version = "???";

            PickBinaryPath = ReactiveCommand.CreateFromTask<Window>(
                async w => await PickFolder(w, InitBinary)
            );

            GetLatestRelease = ReactiveCommand.CreateFromTask(DoGetLatestRelease);

            PickDiscImagePath = ReactiveCommand.CreateFromTask<Window>(
                async w => await PickFile(w, "*", async f => DiscImagePath = f)
            );

            PickOutputPath = ReactiveCommand.CreateFromTask<Window>(
                async w => await PickFolder(w, async f => OutputPath = f)
            );

            // TODO: use file save dialog instead of open (browse is broken)
            PickXmlOutputPath = ReactiveCommand.CreateFromTask<Window>(
                async w => await PickFile(w, "xml", async f => XmlOutputPath = f)
            );

            DumpIso = ReactiveCommand.CreateFromTask(async () => await _binaryWrapper?.DumpIso(DiscImagePath!, OutputPath!, XmlOutputPath!));

            // TODO: output path for image
            MakeIso = ReactiveCommand.CreateFromTask(async () => await _binaryWrapper?.BuildIso(XmlInputPath!));

            PickXmlInputPath = ReactiveCommand.CreateFromTask<Window>(
                async w => await PickFile(w, "xml", async f => XmlInputPath = f)
            );
        }

        private async Task DoGetLatestRelease()
        {
            var release = await _releaseDownloader.GetLatestRelease();

            await _releaseDownloader.DownloadAndInstallRelease(release, DEFAULT_BIN_PATH);

            await InitBinary(Path.GetFullPath(DEFAULT_BIN_PATH));
        }

        private async Task InitBinary(string binaryPath)
        {
            BinaryPath = binaryPath;

            _binaryWrapper = new BinaryWrapper(BinaryPath);

            Version = await _binaryWrapper.GetVersion();
        }

        private async Task PickFolder(Window activeWindow, Func<string, Task> setAction)
        {
            var fodlerDialog = new OpenFolderDialog();

            var folderResult = await fodlerDialog.ShowAsync(activeWindow);

            if (folderResult is null)
            {
                return;
            }

            await setAction(folderResult);
        }

        private async Task PickFile(Window activeWindow, string fileExtension, Func<string, Task> setAction)
        {
            var fileDialog = new OpenFileDialog()
            {
                Filters = new()
                {
                    new()
                    {
                        Extensions = new() { fileExtension }
                    }
                }
            };
            var dialogResult = await fileDialog.ShowAsync(activeWindow);

            if ((dialogResult?.Length ?? 0) < 1)
            {
                return;
            }

            await setAction(dialogResult!.First());
        }
    }
}
