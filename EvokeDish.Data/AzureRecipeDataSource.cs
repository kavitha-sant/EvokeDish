using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EvokeDish.Abstractions;
using EvokeDish.Models;
using EvokeDish.Util;
using Microsoft.Practices.ServiceLocation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;

namespace EvokeDish.Data
{
    public class AzureRecipeSource : IDataSource<Recipe>
    {
        public AzureRecipeSource()
        {
            OnDataSyncError += (object sender, DataSyncErrorEventArgs<Recipe> e) => {
                // In and old version of Acquaint, we were presenting an error message instead of auto-handling. This is where you could wire up a message to show to the user.
                // ServiceLocator.Current.GetInstance<IDataSyncConflictMessagePresenter>().PresentConflictMessage();
            };
        }

        MobileServiceClient _MobileService { get; set; }

        SyncHandler<Recipe> _SyncHandler;

        IMobileServiceSyncTable<Recipe> _RecipeTable;

        MobileServiceSQLiteStore _MobileServiceSQLiteStore;

        bool _IsInitialized;

        private string _DataPartitionId => Settings.DataPartitionPhrase; //GuidUtility.Create(Settings.DataPartitionPhrase).ToString().ToUpper();

        const string _LocalDbName = "recipes.db";

        /// <summary>
        /// An event that is fired when a data sync error occurs.
        /// </summary>
        public event DataSyncErrorEventHandler<Recipe> OnDataSyncError;

        /// <summary>
        /// Raises the data sync error event.
        /// </summary>
        /// <param name="e">A DataSyncErrorEventArgs or type T.</param>
        protected virtual void RaiseDataSyncErrorEvent(DataSyncErrorEventArgs<Recipe> e)
        {
            DataSyncErrorEventHandler<Recipe> handler = OnDataSyncError;

            if (handler != null)
                handler(this, e);
        }

        #region Data Access

        public async Task<IEnumerable<Recipe>> GetItems()
        {
            return await Execute<IEnumerable<Recipe>>(async () =>
            {
                await SyncItemsAsync().ConfigureAwait(false);
                return await _RecipeTable.Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.Id).ToEnumerableAsync().ConfigureAwait(false);
            }, new List<Recipe>()).ConfigureAwait(false);
        }

        public async Task<Recipe> GetItem(string id)
        {
            return await Execute<Recipe>(async () =>
            {
                await SyncItemsAsync().ConfigureAwait(false);
                return await _RecipeTable.LookupAsync(id).ConfigureAwait(false);
            }, null).ConfigureAwait(false);
        }

        public async Task<bool> AddItem(Recipe item)
        {
            return await Execute<bool>(async () =>
            {
                item.DataPartitionId = _DataPartitionId;

                await Initialize().ConfigureAwait(false);
                await _RecipeTable.InsertAsync(item).ConfigureAwait(false);
                await SyncItemsAsync().ConfigureAwait(false);
                return true;
            }, false).ConfigureAwait(false);
        }

        public async Task<bool> UpdateItem(Recipe item)
        {
            return await Execute<bool>(async () =>
            {
                await Initialize().ConfigureAwait(false);
                await _RecipeTable.UpdateAsync(item).ConfigureAwait(false);
                await SyncItemsAsync().ConfigureAwait(false);
                return true;
            }, false).ConfigureAwait(false);
        }

        public async Task<bool> RemoveItem(Recipe item)
        {
            return await Execute<bool>(async () =>
            {
                await Initialize().ConfigureAwait(false);
                await _RecipeTable.DeleteAsync(item).ConfigureAwait(false);
                await SyncItemsAsync().ConfigureAwait(false);
                return true;
            }, false).ConfigureAwait(false);
        }

        #endregion


        #region helper methods for dealing with the state of the local store

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        async Task<bool> Initialize()
        {
            return await Execute<bool>(async () =>
            {
                if (_IsInitialized)
                    return true;

                // We're passing in a handler here for the sole purpose of inspecting outbound HTTP requests with Charles Web Debugging Proxy on OS X. Only in debug builds.
                _MobileService = new MobileServiceClient(Settings.AzureAppServiceUrl, GetHttpClientHandler());

                _MobileServiceSQLiteStore = new MobileServiceSQLiteStore(_LocalDbName);

                _MobileServiceSQLiteStore.DefineTable<Recipe>();

                _RecipeTable = _MobileService.GetSyncTable<Recipe>();

                _SyncHandler = new SyncHandler<Recipe>();

                _SyncHandler.OnDataSyncError += (object sender, DataSyncErrorEventArgs<Recipe> e) => {
                    RaiseDataSyncErrorEvent(e);
                };

                await _MobileService.SyncContext.InitializeAsync(_MobileServiceSQLiteStore, _SyncHandler).ConfigureAwait(false);

                _IsInitialized = true;

                return _IsInitialized;
            }, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Syncs the items.
        /// </summary>
        /// <returns>The items are synced.</returns>
        async Task<bool> SyncItemsAsync()
        {
            return await Execute(async () =>
            {
                if (Settings.LocalDataResetIsRequested)
                    await ResetLocalStoreAsync().ConfigureAwait(false);

                await Initialize().ConfigureAwait(false);
                await EnsureDataIsSeededAsync().ConfigureAwait(false);
                // PushAsync() has been omitted here because the _MobileService.SyncContext automatically calls PushAsync() before PullAsync() if it sees pending changes in the context. (Frequently misunderstood feature of the Azure App Service SDK)
                await _RecipeTable.PullAsync($"getAll{typeof(Recipe).Name}", _RecipeTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);
                return true;
            }, false);
        }

        /// <summary>
        /// Ensures the data is seeded.
        /// </summary>
        async Task EnsureDataIsSeededAsync()
        {
            if (Settings.DataIsSeeded)
                return;

            await _RecipeTable.PullAsync($"getAll{typeof(Recipe).Name}", _RecipeTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);

            var any = (await _RecipeTable.Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.Name).ToEnumerableAsync().ConfigureAwait(false)).Any();

            if (any)
                Settings.DataIsSeeded = true;

            await _RecipeTable.PurgeAsync();
            if (!Settings.DataIsSeeded)
            {
                var newItems = SeedData.Get("");

                foreach (var i in newItems)
                {
                    try
                    {
                        await _RecipeTable.InsertAsync(i);

                    }
                    catch (Exception e)
                    {
                        
                        throw e;
                    }
                }

                Settings.DataIsSeeded = true;
            }
        }

        /// <summary>
        /// Resets the local store.
        /// </summary>
        async Task ResetLocalStoreAsync()
        {
            _RecipeTable = null;

            // On UWP, it's necessary to Dispose() and nullify the MobileServiceSQLiteStore before 
            // trying to delete the database file, otherwise an access exception will occur
            // because of an open file handle. It's okay to do for iOS and Android as well, but not necessary.
            _MobileServiceSQLiteStore?.Dispose();
            _MobileServiceSQLiteStore = null;


            await DeleteOldLocalDatabase().ConfigureAwait(false);
            _IsInitialized = false;
            Settings.LocalDataResetIsRequested = false;
            Settings.DataIsSeeded = false;
        }

        /// <summary>
        /// Deletes the old local database.
        /// </summary>
        async Task DeleteOldLocalDatabase()
        {
            var datastoreFolderPathProvider = ServiceLocator.Current.GetInstance<IDatastoreFolderPathProvider>();
            var databaseFolderPath = datastoreFolderPathProvider.GetPath();
            var databaseFolder = await FileSystem.Current.GetFolderFromPathAsync(databaseFolderPath).ConfigureAwait(false);
            var dbFile = await databaseFolder.GetFileAsync(_LocalDbName, CancellationToken.None).ConfigureAwait(false);

            if (dbFile != null)
                await dbFile.DeleteAsync().ConfigureAwait(true);
        }

        #endregion


        #region some nifty exception helpers

        /// <summary>
        /// This method is intended for encapsulating the catching of exceptions related to the Azure MobileServiceClient.
        /// </summary>
        /// <param name="execute">A Func that contains the async work you'd like to do.</param>
        static async Task Execute(Func<Task> execute)
        {
            try
            {
                await execute().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
        }

        /// <summary>
        /// This method is intended for encapsulating the catching of exceptions related to the Azure MobileServiceClient.
        /// </summary>
        /// <param name="execute">A Func that contains the async work you'd like to do, and will return some value.</param>
        /// <param name="defaultReturnObject">A default return object, which will be returned in the event that an operation in the Func throws an exception.</param>
        /// <typeparam name="T">The type of the return value that the Func will returns, and also the type of the default return object. </typeparam>
        static async Task<T> Execute<T>(Func<Task<T>> execute, T defaultReturnObject)
        {
            try
            {
                return await execute().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex);
            }
            return defaultReturnObject;
        }

        /// <summary>
        /// Handles the exceptions.
        /// </summary>
        /// <returns>The exceptions.</returns>
        /// <param name="ex">Ex.</param>
        static void HandleExceptions(Exception ex)
        {
            if (ex is MobileServiceInvalidOperationException)
            {
                // TODO: report with HockeyApp
                System.Diagnostics.Debug.WriteLine($"MOBILE SERVICE ERROR {ex.Message}");
                return;
            }

            if (ex is MobileServicePushFailedException)
            {
                var pushResult = ((MobileServicePushFailedException)ex).PushResult;

                foreach (var e in pushResult.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR {pushResult.Status}: {e.RawResult}");
                }
            }

            else
            {
                // TODO: report with HockeyApp
                System.Diagnostics.Debug.WriteLine($"ERROR {ex.Message}");
            }
        }

        #endregion


        /// <summary>
        /// Gets an HttpClentHandler. The main purpose of which in this case is to 
        /// be able to inspect outbound HTTP traffic from the iOS simulator with
        /// Charles Web Debugging Proxy on OS X. Android and UWP will return a null handler.
        /// </summary>
        /// <returns>An HttpClentHandler</returns>
        HttpClientHandler GetHttpClientHandler()
        {
            return ServiceLocator.Current.GetInstance<IHttpClientHandlerFactory>().GetHttpClientHandler();
        }
    }
}
