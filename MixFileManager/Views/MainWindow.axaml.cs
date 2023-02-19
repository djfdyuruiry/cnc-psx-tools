using Avalonia.Controls;

using MixFileManager.ViewModels;

namespace MixFileManager.Views
{
    public partial class MainWindow : Window
    {
        private const string FILE_TREE_NAME = "FileTree";

        private readonly TreeView _treeView;

        public MainWindow()
        {
            InitializeComponent();

            _treeView = this.FindControl<TreeView>(FILE_TREE_NAME);

            _treeView.SelectionChanged += MainWindow_SelectionChanged;
        }

        private void MainWindow_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ITreeViewEventHandler eventHandler)
            {
                eventHandler.OnSelectedItemChangeAsync(
                    FILE_TREE_NAME,
                    e.AddedItems.Count > 0 ? e.AddedItems[0] : null
                );
            }
        }
    }
}
