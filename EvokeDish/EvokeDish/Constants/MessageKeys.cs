using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.Constants
{
    /// <summary>
    /// This class is simply a centralized place to keep strings that will be used with the MessagingService.
    /// </summary>
    public static class MessageKeys
    {
        public const string DisplayAlert = "DisplayAlert";
        public const string DisplayQuestion = "DisplayQuestion";
        public const string AddRecipe = "AddRecipe";
        public const string UpdateRecipe = "UpdateRecipe";
        public const string DeleteRecipe = "DeleteRecipe";
        public const string RecipeLocationUpdated = "RecipeLocationUpdated";
        public const string SetupMap = "SetupMap";
        public const string DataPartitionPhraseValidation = "DataPartitionPhraseValidation";
        public const string BackendUrlValidation = "BackendUrlValidation";
    }
}
