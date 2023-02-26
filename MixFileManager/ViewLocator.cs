using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using MixFileManager.ViewModels;

namespace MixFileManager
{
    internal class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type is  null)
            {
                return new TextBlock { Text = $"View not found: {name}" };
            }

            return (IControl)AppServices.Provider.GetService(type)!;
        }

        public bool Match(object data) => data is ViewModelBase;
    }
}
