﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaEpsilon
{
    /// <summary>
    /// This class represents a single Page collection, but have the entire items available inside
    /// </summary>
    /// from http://paginatedcollection.codeplex.com/ - http://jobijoy.blogspot.com/2008/12/paginated-observablecollection.html
    /// modified by Amrykid

    public class PaginatedObservableCollection<T> : ObservableCollection<T>
    {
        #region Properties
        public int PageSize
        {
            get { return _itemCountPerPage; }
            set
            {
                if (value >= 0)
                {
                    _itemCountPerPage = value;
                    RecalculateThePageItems();
                    OnPropertyChanged(new PropertyChangedEventArgs("PageSize"));
                }
            }
        }

        public int CurrentPage
        {
            get { return _currentPageIndex; }
            set
            {
                if (value >= 0 && (CanPageUp && value > _currentPageIndex || CanPageDown && value < _currentPageIndex))
                {
                    _currentPageIndex = value;
                    RecalculateThePageItems();
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
                }
            }
        }

        #endregion

        #region Constructor
        public PaginatedObservableCollection(IEnumerable<T> collecton)
        {
            _currentPageIndex = 0;
            _itemCountPerPage = 1;
            originalCollection = new List<T>(collecton);
            RecalculateThePageItems();
        }

        public PaginatedObservableCollection(int itemsPerPage)
        {
            _currentPageIndex = 0;
            _itemCountPerPage = itemsPerPage;
            originalCollection = new List<T>();
        }
        public PaginatedObservableCollection()
        {
            _currentPageIndex = 0;
            _itemCountPerPage = 1;
            originalCollection = new List<T>();
        }

        public PaginatedObservableCollection(IEnumerable<T> collecton, int itemsPerPage)
        {
            _currentPageIndex = 0;
            _itemCountPerPage = itemsPerPage;
            originalCollection = new List<T>(collecton);
            RecalculateThePageItems();
        }
        #endregion

        #region private
        private void RecalculateThePageItems()
        {
            int startIndex = _currentPageIndex * _itemCountPerPage;

            CanPageUp = originalCollection.Count >= (_currentPageIndex + 1) * _itemCountPerPage;
            CanPageDown = startIndex >= _itemCountPerPage;

            if (_itemCountPerPage > 0)
                MaxPageIndex = originalCollection.Count / _itemCountPerPage;

            if (originalCollection.Count <= startIndex)
                return; //prevents it from navigating to a page with no items.

            Clear();

            for (int i = startIndex; i < startIndex + _itemCountPerPage; i++)
            {
                if (originalCollection.Count > i)
                    base.InsertItem(i - startIndex, originalCollection[i]);
            }
        }
        #endregion

        #region Overrides

        protected override void InsertItem(int index, T item)
        {
            int startIndex = _currentPageIndex * _itemCountPerPage;
            int endIndex = startIndex + _itemCountPerPage;

            //Check if the Index is with in the current Page then add to the collection as bellow. And add to the originalCollection also
            if ((index >= startIndex) && (index < endIndex))
            {
                base.InsertItem(index - startIndex, item);

                if (Count > _itemCountPerPage)
                    base.RemoveItem(endIndex);
            }

            if (index >= Count)
                originalCollection.Add(item);
            else
                originalCollection.Insert(index, item);
        }

        protected override void RemoveItem(int index)
        {
            int startIndex = _currentPageIndex * _itemCountPerPage;
            int endIndex = startIndex + _itemCountPerPage;
            //Check if the Index is with in the current Page range then remove from the collection as bellow. And remove from the originalCollection also
            if ((index >= startIndex) && (index < endIndex))
            {
                base.RemoveAt(index - startIndex);

                if (Count <= _itemCountPerPage)
                    base.InsertItem(endIndex - 1, originalCollection[index + 1]);
            }

            originalCollection.RemoveAt(index);
        }

        #endregion

        private List<T> originalCollection;
        private int _currentPageIndex;
        private int _itemCountPerPage;

        public bool CanPageUp { get; private set; }
        public bool CanPageDown { get; private set; }

        public int MaxPageIndex { get; private set; }

        internal void Refresh()
        {
            RecalculateThePageItems();
        }
    }
}
