using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Droid;
using Microsoft.Practices.ServiceLocation;
using EvokeDish.Util;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using EvokeDish.Abstractions;
using EvokeDish.Common.Droid;
using EvokeDish.Models;
using EvokeDish.Data;

namespace EvokeDish.Droid
{
    [Activity(Label = "EvokeDish", Icon = "@drawable/icon", Theme = "@style/AcquaintTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // an IoC Container
        IContainer _IoCContainer;

        protected override void OnCreate(Bundle bundle)
        {
            RegisterDependencies();

            Settings.OnDataPartitionPhraseChanged += (sender, e) => {
                UpdateDataSourceIfNecessary();
            };

            CachedImageRenderer.Init();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        /// <summary>
        /// Registers dependencies with an IoC container.
        /// </summary>
        /// <remarks>
        /// Since some of our libraries are shared between the Forms and Native versions 
        /// of this app, we're using an IoC/DI framework to provide access across implementations.
        /// </remarks>
        void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new EnvironmentService()).As<IEnvironmentService>();

            builder.RegisterInstance(new HttpClientHandlerFactory()).As<IHttpClientHandlerFactory>();

            builder.RegisterInstance(new DatastoreFolderPathProvider()).As<IDatastoreFolderPathProvider>();

            // Set the data source dependent on whether or not the data parition phrase is "UseLocalDataSource".
            // The local data source is mainly for use in TestCloud test runs, but the app can be used in local-only data mode if desired.
            if (Settings.IsUsingLocalDataSource)
                builder.RegisterInstance(_LazyFilesystemOnlyRecipeDataSource.Value).As<IDataSource<Recipe>>();
            else
                builder.RegisterInstance(_LazyAzureRecipeSource.Value).As<IDataSource<Recipe>>();

            _IoCContainer = builder.Build();

            var csl = new AutofacServiceLocator(_IoCContainer);
            ServiceLocator.SetLocatorProvider(() => csl);
        }

        /// <summary>
		/// Updates the data source if necessary.
		/// </summary>
		void UpdateDataSourceIfNecessary()
        {
            var dataSource = ServiceLocator.Current.GetInstance<IDataSource<Recipe>>();

            // Set the data source dependent on whether or not the data parition phrase is "UseLocalDataSource".
            // The local data source is mainly for use in TestCloud test runs, but the app can be used in local-only data mode if desired.

            // if the settings dictate that a local data source should be used, then register the local data provider and update the IoC container
            if (Settings.IsUsingLocalDataSource && !(dataSource is FilesystemOnlyRecipeDataSource))
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(_LazyFilesystemOnlyRecipeDataSource.Value).As<IDataSource<Recipe>>();
                builder.Update(_IoCContainer);
                return;
            }

            // if the settings dictate that a local data souce should not be used, then register the remote data source and update the IoC container
            if (!Settings.IsUsingLocalDataSource && !(dataSource is AzureRecipeSource))
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(_LazyAzureRecipeSource.Value).As<IDataSource<Recipe>>();
                builder.Update(_IoCContainer);
            }
        }

        // we need lazy loaded instances of these two types hanging around because if the registration on IoC container changes at runtime, we want the same instances
        static Lazy<FilesystemOnlyRecipeDataSource> _LazyFilesystemOnlyRecipeDataSource = new Lazy<FilesystemOnlyRecipeDataSource>(() => new FilesystemOnlyRecipeDataSource());
        static Lazy<AzureRecipeSource> _LazyAzureRecipeSource = new Lazy<AzureRecipeSource>(() => new AzureRecipeSource());

    }
}

