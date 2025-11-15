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
    public class DateFilterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           var dateFilterType = (DateFilterType)value;
            //这里后期通过语言服务接口动态获取语言解析 
            //待处理
            string dateFilterTypeDes = "不限";
            switch (dateFilterType)
            {
                case DateFilterType.Any:
                    dateFilterTypeDes = "不限";
                    break;
                case DateFilterType.InOneDay:
                    dateFilterTypeDes = "一天内";
                    break;
                case DateFilterType.InThreeDays:
                    dateFilterTypeDes = "三天内";
                    break;
                case DateFilterType.InOneWeek:
                    dateFilterTypeDes = "一周内";
                    break;
                case DateFilterType.InTwoWeeks:
                    dateFilterTypeDes = "两周内";
                    break;
                case DateFilterType.WithinOneMonth:
                    dateFilterTypeDes = "一个月内";
                    break;
                case DateFilterType.WithinTwoMonths:
                    dateFilterTypeDes = "两个月内";
                    break;
                case DateFilterType.WithinSixMonths:
                    dateFilterTypeDes = "六个月内";
                    break;
                case DateFilterType.InOneYear:
                    dateFilterTypeDes = "一年内";
                    break;
                case DateFilterType.Customise:
                    dateFilterTypeDes = "自定义";
                    break;
            }

            return dateFilterTypeDes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
