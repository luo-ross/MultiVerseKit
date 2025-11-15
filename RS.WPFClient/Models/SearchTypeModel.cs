using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using RS.WPFClient.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace RS.WPFClient.Models
{
    public class SearchTypeModel : ObservableObject
    {

        private SearchType searchType;
        /// <summary>
        /// 搜索类型
        /// </summary>
        public SearchType SearchType
        {
            get { return searchType; }
            set
            {
                this.SetProperty(ref searchType, value);
            }
        }


        private bool isSelect;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                this.SetProperty(ref isSelect, value);
            }
        }
    }
}
