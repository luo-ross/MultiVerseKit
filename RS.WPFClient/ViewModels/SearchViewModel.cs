using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.WPFClient.Enums;
using RS.WPFClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.WPFClient.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        #region 依赖服务
        private readonly IWindowService WindowService;
        #endregion

        #region 命令

        public ICommand HideSearchCommand { get; }

        public ICommand SearchViewPreviewMouseLeftButtonUpCommand { get; }

        #endregion
        public SearchViewModel()
        {
            this.HideSearchCommand = new RelayCommand(HideSearch);
            this.SearchViewPreviewMouseLeftButtonUpCommand = new RelayCommand(SearchViewPreviewMouseLeftButtonUp);
            this.InitSearchTypeModelList();
            this.InitDateFilterList();
        }
       

        private void SearchViewPreviewMouseLeftButtonUp()
        {
            this.IsShowSearch = true;
        }

        private void HideSearch()
        {
            //用户点击搜索类型或者高级搜索时不关闭
            if (this.IsSearchTypeChecked || this.IsAdvancedSearchChecked)
            {
                return;
            }

            //搜索内容不为空 则不关闭
            if (!string.IsNullOrWhiteSpace(this.SearchContent))
            {
                return;
            }

            IsShowSearch = false;
        }



        private bool isShowSearch;
        /// <summary>
        /// 是否选择搜索
        /// </summary>
        public bool IsShowSearch
        {
            get { return isShowSearch; }
            set
            {
                this.SetProperty(ref isShowSearch, value);
            }
        }


        private bool isSearchTypeChecked;
        /// <summary>
        /// 搜索类型是否选中
        /// </summary>
        public bool IsSearchTypeChecked
        {
            get { return isSearchTypeChecked; }
            set
            {
                this.SetProperty(ref isSearchTypeChecked, value);
            }
        }


        private bool isAdvancedSearchChecked;
        /// <summary>
        /// 高级搜索是否选中
        /// </summary>
        public bool IsAdvancedSearchChecked
        {
            get { return isAdvancedSearchChecked; }
            set
            {
                this.SetProperty(ref isAdvancedSearchChecked, value);
            }
        }


        private string searchContent;
        /// <summary>
        /// 搜索内容
        /// </summary>
        public string SearchContent
        {
            get { return searchContent; }
            set
            {
                this.SetProperty(ref searchContent, value);
            }
        }

        #region 搜索类型

        private void InitSearchTypeModelList()
        {
            var searchTypeList = Enum.GetValues(typeof(SearchType))
             .Cast<SearchType>()
             .Select(t =>
             {
                 var searchTypeModel = new SearchTypeModel()
                 {
                     SearchType = t
                 };
                 if (searchTypeModel.SearchType == SearchType.Email)
                 {
                     searchTypeModel.IsSelect = true;
                     this.SearchTypeModelSelect = searchTypeModel;
                 }
                 return searchTypeModel;
             });
            this.SearchTypeModelList = new ObservableCollection<SearchTypeModel>(searchTypeList);
        }


        private ObservableCollection<SearchTypeModel> searchTypeModelList;
        /// <summary>
        /// 搜索类型
        /// </summary>
        public ObservableCollection<SearchTypeModel> SearchTypeModelList
        {
            get { return searchTypeModelList; }
            set
            {
                this.SetProperty(ref searchTypeModelList, value);
            }
        }

        private SearchTypeModel searchTypeModelSelect;
        /// <summary>
        /// 搜索类型
        /// </summary>
        public SearchTypeModel SearchTypeModelSelect
        {
            get { return searchTypeModelSelect; }
            set
            {
                this.SetProperty(ref searchTypeModelSelect, value);
            }
        }
        #endregion

        #region 日期

        private void InitDateFilterList()
        {
            var dateFilterTypeList = Enum.GetValues(typeof(DateFilterType))
              .Cast<DateFilterType>()
              .Select(t =>
              {
                  var dateFilterModel = new DateFilterModel()
                  {
                      DateFilterType = t
                  };
                  if (dateFilterModel.DateFilterType == DateFilterType.Any)
                  {
                      dateFilterModel.IsSelect = true;
                      this.DateFilterModelSelect = dateFilterModel;
                  }
                  return dateFilterModel;
              });
            this.DateFilterList = new ObservableCollection<DateFilterModel>(dateFilterTypeList);
        }

        private ObservableCollection<DateFilterModel> dateFilterList;
        /// <summary>
        /// 日期
        /// </summary>
        public ObservableCollection<DateFilterModel> DateFilterList
        {
            get
            {
                return dateFilterList;
            }
            set
            {
                this.SetProperty(ref dateFilterList, value);
            }
        }


        private DateFilterModel dateFilterModelSelect;
        /// <summary>
        /// 日期搜索类型
        /// </summary>
        public DateFilterModel DateFilterModelSelect
        {
            get { return dateFilterModelSelect; }
            set
            {
                this.SetProperty(ref dateFilterModelSelect, value);
            }
        }
        #endregion

    }
}
