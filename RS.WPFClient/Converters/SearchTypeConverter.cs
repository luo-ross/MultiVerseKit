using RS.WPFClient.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RS.WPFClient.Converters
{
    public class SearchTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           var searchType = (SearchType)value;
            //这里后期通过语言服务接口动态获取语言解析 
            //待处理
            string searchTypeDes = "邮箱";
            switch (searchType)
            {
                case SearchType.Email:
                    searchTypeDes = "邮箱";
                    break;
                case SearchType.File:
                    searchTypeDes = "文件";
                    break;
                case SearchType.Invoice:
                    searchTypeDes = "发票";
                    break;
                case SearchType.Contacts:
                    searchTypeDes = "联系人";
                    break;
                case SearchType.Notes:
                    searchTypeDes = "记事本";
                    break;
            }
            return searchTypeDes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
