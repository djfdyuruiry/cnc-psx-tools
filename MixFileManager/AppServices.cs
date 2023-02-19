using Microsoft.Extensions.DependencyInjection;
using System;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using CncPsxLib;

using MixFileManager.ViewModels;
using MixFileManager.Views;

namespace MixFileManager
{
    internal class AppServices
    {
        public static readonly IServiceProvider Provider;

        static AppServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<FatFileReader>();
            services.AddSingleton(_ =>
                new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build()
            );

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton(s =>
                new MainWindow
                {
                    DataContext = s.GetService<MainWindowViewModel>(),
                }
            );

            Provider = services.BuildServiceProvider();
        }
    }
}
