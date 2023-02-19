using System.Threading.Tasks;

namespace MixFileManager.ViewModels
{
    internal interface ITreeViewEventHandler
    {
        public Task OnSelectedItemChangeAsync(string treeViewName, object? item);
    }
}
