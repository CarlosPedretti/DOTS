using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.UI
{
    /// <summary>
    /// Provides common extension methods for collections, including sorting, shuffling, and random selection.
    /// </summary>
    public static class CollectionExtensions
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Orders a collection by name in ascending order (A to Z).
        /// </summary>
        public static IEnumerable<T> OrderByName<T>(this IEnumerable<T> collection, Func<T, string> selector)
        {
            return collection.OrderBy(selector, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Orders a collection by name in descending order (Z to A).
        /// </summary>
        public static IEnumerable<T> OrderByNameDesc<T>(this IEnumerable<T> collection, Func<T, string> selector)
        {
            return collection.OrderByDescending(selector, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Orders a collection by ID in ascending order.
        /// </summary>
        public static IEnumerable<T> OrderById<T>(this IEnumerable<T> collection, Func<T, int> selector)
        {
            return collection.OrderBy(selector);
        }

        /// <summary>
        /// Orders a collection by date in descending order (newest first).
        /// </summary>
        public static IEnumerable<T> OrderByDateDesc<T>(this IEnumerable<T> collection, Func<T, DateTime> selector)
        {
            return collection.OrderByDescending(selector);
        }

        /// <summary>
        /// Shuffles the elements of a collection randomly (Fisher–Yates algorithm).
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">Collection to shuffle.</param>
        /// <returns>A new IEnumerable with elements in random order.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var list = collection.ToList();
            int n = list.Count;

            for (int i = n - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        /// <summary>
        /// Shuffles a List in place using the Fisher–Yates algorithm.
        /// </summary>
        /// <typeparam name="T">Type of the items in the list.</typeparam>
        /// <param name="list">The list to shuffle in place.</param>
        public static void ShuffleInPlace<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Takes a specified number of random elements from the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">Collection to take from.</param>
        /// <param name="count">Number of random elements to return.</param>
        /// <returns>A new IEnumerable with randomly selected elements.</returns>
        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> collection, int count)
        {
            return collection.Shuffle().Take(count);
        }

        /// <summary>
        /// Takes a single random element from the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">Collection to take from.</param>
        /// <returns>A random element from the collection.</returns>
        public static T TakeRandomOne<T>(this IEnumerable<T> collection)
        {
            var list = collection.ToList();
            if (!list.Any())
                throw new InvalidOperationException("Cannot take a random element from an empty collection.");

            int index = _random.Next(list.Count);
            return list[index];
        }
    }

}