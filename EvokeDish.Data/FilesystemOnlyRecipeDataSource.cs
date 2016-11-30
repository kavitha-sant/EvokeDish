using EvokeDish.Abstractions;
using EvokeDish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCLStorage;
using Xamarin.Forms;

namespace EvokeDish.Data
{
    /// <summary>
    /// This class exists mainly for isolating data during our Xamarin Test Cloud test runs, 
    /// but it can also serve as an example of how to do local storage.
    /// </summary>
    public class FilesystemOnlyRecipeDataSource : IDataSource<Recipe>
    {
        const string _FileName = "recipes.json";

        readonly IFolder _RootFolder;

        bool _IsInitialized;

        List<Recipe> _Recipes;

        public FilesystemOnlyRecipeDataSource()
        {
            _RootFolder = FileSystem.Current.LocalStorage;

            OnDataSyncError += (object sender, DataSyncErrorEventArgs<Recipe> e) => {
                // Do nothing, because we won't have data sync issues with local storage
            };
        }

        #region IDataSource implementation
        /// <summary>
        /// Not used in this implementation of IDataSoure<T>.
        /// </summary>
        public event DataSyncErrorEventHandler<Recipe> OnDataSyncError;

        public async Task<IEnumerable<Recipe>> GetItems()
        {
            await EnsureInitialized().ConfigureAwait(false);

            return await Task.FromResult(_Recipes.OrderBy(x => Guid.Parse(x.Id))).ConfigureAwait(false);
        }

        public async Task<Recipe> GetItem(string id)
        {
            await EnsureInitialized().ConfigureAwait(false);

            return await Task.FromResult(_Recipes.SingleOrDefault(x => x.Id == id)).ConfigureAwait(false);
        }

        public async Task<bool> AddItem(Recipe item)
        {
            _Recipes.Add(item);

            await WriteFile(_RootFolder, _FileName, JsonConvert.SerializeObject(_Recipes)).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> UpdateItem(Recipe item)
        {
            await EnsureInitialized().ConfigureAwait(false);

            int i = _Recipes.FindIndex(a => a.Id == item.Id);

            if (i < 0)
                return false;

            _Recipes[i] = item;

            await WriteFile(_RootFolder, _FileName, JsonConvert.SerializeObject(_Recipes)).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> RemoveItem(Recipe item)
        {
            await EnsureInitialized().ConfigureAwait(false);

            _Recipes.RemoveAll(c => c.Id == item.Id);

            await WriteFile(_RootFolder, _FileName, JsonConvert.SerializeObject(_Recipes)).ConfigureAwait(false);

            return true;
        }

        #endregion

        #region supporting methods

        async Task Initialize()
        {
            try
            {
                if (!await FileExists(_RootFolder, _FileName).ConfigureAwait(false))
                {
                    await CreateFile(_RootFolder, _FileName).ConfigureAwait(false);
                }

                if (string.IsNullOrWhiteSpace(await GetFileContents(await GetFile(_RootFolder, _FileName).ConfigureAwait(false)).ConfigureAwait(false)))
                {
                    _Recipes = GenerateRecipes();

                    await WriteFile(_RootFolder, _FileName, JsonConvert.SerializeObject(_Recipes)).ConfigureAwait(false);
                }
                else
                {
                    _Recipes = JsonConvert.DeserializeObject<List<Recipe>>(await GetFileContents(await GetFile(_RootFolder, _FileName).ConfigureAwait(false)).ConfigureAwait(false));
                }

                _IsInitialized = true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        async Task EnsureInitialized()
        {
            if (!_IsInitialized)
                await Initialize().ConfigureAwait(false);
        }

        static async Task<bool> FileExists(IFolder folder, string fileName)
        {
            return await Task.FromResult<bool>(await folder.CheckExistsAsync(fileName) == ExistenceCheckResult.FileExists).ConfigureAwait(false);
        }

        static async Task<IFile> CreateFile(IFolder folder, string fileName)
        {
            return await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
        }

        static async Task<IFile> GetFile(IFolder folder, string fileName)
        {
            return await folder.GetFileAsync(fileName).ConfigureAwait(false);
        }

        static async Task WriteFile(IFolder folder, string fileName, string fileContents)
        {
            var file = await GetFile(folder, fileName).ConfigureAwait(false);

            await file.WriteAllTextAsync(fileContents).ConfigureAwait(false);
        }

        static async Task<string> GetFileContents(IFile file)
        {
            return await file.ReadAllTextAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Generates the Recipes.
        /// </summary>
        /// <returns>The recipes.</returns>
        static List<Recipe> GenerateRecipes()
        {
            return new List<Recipe>()
            {
                //new Recipe() { Id = "00004363-F79A-44E7-BC32-6128E2EC8401", Name = "Kozhakatai", Instructions = new List<string> { "1.Heat a pan, add 1 tablespoon of water and add grated jaggery. When the jaggery dissolves completely in water, strain it to remove dust and sand particles ", "2.Add the strained jaggery juice again in the pan, and keep it in flame . When the the jaggery juice starts to boil, add grated coconut, cardamom powder and stir well continuously till it roll like a ball and does not stick the sides of the pan ", "3.This is the correct consistency to remove from flame. Take the coconut pooranam in a plate and allow it to cool off Make small balls out of the pooranam and keep it in a plate" }, ImageURL = "http://4.bp.blogspot.com/-L-ZK1ZUOdAY/U_XVMRhtEDI/AAAAAAAANnQ/hqRMgFwzbao/s1600/Thengai%2BPurana%2BKozhukattai_Final2.JPG" },
                //new Recipe() { Id = "c227bfd2-c6f6-49b5-93ec-afef9eb18d08", Name = "Curried Lentils and Rice", Instructions = new List<string> {"Bring broth to a low boil.", "Add curry powder and salt.", "Cook lentils for 20 minutes.", "Add rice and simmer for 20 minutes.", "Enjoy!"}, ImageURL = "http://dagzhsfg97k4.cloudfront.net/wp-content/uploads/2012/05/lentils3.jpg" },
                //new Recipe() { Id = "31bf6fe5-18f1-4354-9571-2cdecb0c00af", Name = "Homemade Pizza", Instructions = new List<string> {"Add hot water to yeast in a large bowl and let sit for 15 minutes.", "Mix in oil, sugar, salt, and flour and let sit for 1 hour.", "Knead the dough and spread onto a pan.", "Spread pizza sauce and sprinkle cheese.", "Add any optional toppings as you wish.", "Bake at 400 deg Fahrenheit for 15 minutes."}, ImageURL = "https://upload.wikimedia.org/wikipedia/commons/c/c7/Spinach_pizza.jpg" }
            };
        }

        #endregion
    }
}
