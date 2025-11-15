using CommunityToolkit.Mvvm.ComponentModel;
using RS.WPFClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Models
{
    public class DateFilterModel : ObservableObject
    {
        private DateFilterType dateFilterType;
        /// <summary>
        /// 日期筛选类型
        /// </summary>
        public DateFilterType DateFilterType
        {
            get { return dateFilterType; }
            set
            {
                this.SetProperty(ref dateFilterType, value);
            }
        }


        private bool isSelect;
        /// <summary>
        /// 选项是否选中
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
