using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvokeDish.Models;

namespace EvokeDish.Data
{
    public static class SeedData
    {
        public static IEnumerable<Recipe> Get(string dataPartitionId)
        {
            dataPartitionId = dataPartitionId.ToUpper();

            return new List<Recipe>()
                {
                        new Recipe() { Id = "00004363-F79A-44E7-BC32-6128E2EC8401", DataPartitionId = dataPartitionId, Name = "Kozhakatai", Instructions = string.Join("'", new List<string> { "1.Heat a pan, add 1 tablespoon of water and add grated jaggery. When the jaggery dissolves completely in water, strain it to remove dust and sand particles ", "2.Add the strained jaggery juice again in the pan, and keep it in flame . When the the jaggery juice starts to boil, add grated coconut, cardamom powder and stir well continuously till it roll like a ball and does not stick the sides of the pan ", "3.This is the correct consistency to remove from flame. Take the coconut pooranam in a plate and allow it to cool off Make small balls out of the pooranam and keep it in a plate" }), ImageURL = "http://4.bp.blogspot.com/-L-ZK1ZUOdAY/U_XVMRhtEDI/AAAAAAAANnQ/hqRMgFwzbao/s1600/Thengai%2BPurana%2BKozhukattai_Final2.JPG" },
                        new Recipe() { Id = "c227bfd2-c6f6-49b5-93ec-afef9eb18d08", DataPartitionId = dataPartitionId, Name = "Curried Lentils and Rice", Instructions = string.Join("'", new List<string> {"Bring broth to a low boil.", "Add curry powder and salt.", "Cook lentils for 20 minutes.", "Add rice and simmer for 20 minutes.", "Enjoy!"}), ImageURL = "http://dagzhsfg97k4.cloudfront.net/wp-content/uploads/2012/05/lentils3.jpg" },
                        new Recipe() { Id = "31bf6fe5-18f1-4354-9571-2cdecb0c00af", DataPartitionId = dataPartitionId, Name = "Homemade Pizza", Instructions = string.Join("'", new List<string> {"Add hot water to yeast in a large bowl and let sit for 15 minutes.", "Mix in oil, sugar, salt, and flour and let sit for 1 hour.", "Knead the dough and spread onto a pan.", "Spread pizza sauce and sprinkle cheese.", "Add any optional toppings as you wish.", "Bake at 400 deg Fahrenheit for 15 minutes."}), ImageURL = "https://upload.wikimedia.org/wikipedia/commons/c/c7/Spinach_pizza.jpg" }
                };
        }
}
}
