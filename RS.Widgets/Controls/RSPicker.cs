using RS.Commons.Extensions;
using RS.Widgets.Models;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    public class RSPicker : ContentControl
    {
        private Grid PART_ContentHost;
        private RepeatButton PART_BtnScrollUp;
        private RepeatButton PART_BtnScrollDown;
        private Canvas PART_Canvas;
        private bool IsCanRefreshItemsList = true;
        public List<object> SourceList;
        private List<FrameworkElement> ItemsList;
        static RSPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSPicker), new FrameworkPropertyMetadata(typeof(RSPicker)));
        }

        public RSPicker()
        {
            this.Loaded += RSPicker_Loaded;
            this.SizeChanged += RSPicker_SizeChanged;
        }

        private void RSPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateVisualItemsLayout();
        }

        private void RSPicker_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateVisualItemsLayout();
        }


        [Description("原始数据源")]
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(typeof(RSPicker),
                new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsPicker = d as RSPicker;
            rsPicker.SourceList = rsPicker.ItemsSource.Cast<object>().ToList();
            //if (rsPicker.SelectedItem == null)
            //{
            //    rsPicker.SelectedIndex = 0;
            //}
            //else
            //{
            //    rsPicker.SelectedIndex = rsPicker.SourceList.IndexOf(rsPicker.SelectedItem);
            //}
            rsPicker?.RefreshItemsList();
        }

        [Description("数据模版")]
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register("ItemTemplate",
            typeof(DataTemplate),
            typeof(RSPicker),
            new PropertyMetadata(null, OnItemTemplatePropertyChanged));

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsPicker = d as RSPicker;
            rsPicker?.RefreshItemsList();
        }



        [Description("选择项")]
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            Selector.SelectedItemProperty.AddOwner(typeof(RSPicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsPicker = d as RSPicker;
            //自己内部设置不用刷新
            if (rsPicker.IsCanRefreshItemsList)
            {
                if (rsPicker.SelectedItem != null && rsPicker.SourceList != null)
                {
                    rsPicker.SelectedIndex = rsPicker.SourceList.IndexOf(rsPicker.SelectedItem);
                    rsPicker.RefreshItemsList();
                }
            }
            rsPicker.IsCanRefreshItemsList = true;
        }

        

        [Description("控件描述")]
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RSPicker), new PropertyMetadata(null));


        [Description("搜索值")]
        public object SearchValue
        {
            get { return (object)GetValue(SearchValueProperty); }
            set { SetValue(SearchValueProperty, value); }
        }

        public static readonly DependencyProperty SearchValueProperty =
            DependencyProperty.Register("SearchValue", typeof(object), typeof(RSPicker), new PropertyMetadata(null, OnSearchValuePropertyChanged));


        private static void OnSearchValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsPicker = d as RSPicker;
            if (rsPicker?.SourceList == null || !rsPicker.SourceList.Any())
            {
                return;
            }

            var firstItem = rsPicker.SourceList.First();
            var elementType = firstItem.GetType();
            var searchPropertyName = rsPicker.SearchPropertyName;
            object? convertedValue = null;
            object? selectedItem = null;
            if (elementType.IsValueType || elementType == typeof(string))
            {
                if (e.NewValue == null)
                {
                    rsPicker.SelectedItem = null;
                    return;
                }

                try
                {
                    var targetType = elementType.IsNullableType()
                        ? Nullable.GetUnderlyingType(elementType)
                        : elementType;

                    convertedValue = ChangeToTargetType(e.NewValue, targetType);
                    var keyword = convertedValue?.ToString() ?? string.Empty;

                    selectedItem = rsPicker.SourceList.FirstOrDefault(item =>
                       item != null &&
                       item.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

                    rsPicker.SelectedItem = selectedItem;
                }
                catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
                {
                    rsPicker.SelectedItem = null;
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(searchPropertyName) || e.NewValue == null)
            {
                rsPicker.SelectedItem = null;
                return;
            }

            var propertyInfo = elementType.GetProperty(searchPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                return;
            }


            var propertyType = propertyInfo.PropertyType;
            var targetPropertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            convertedValue = ChangeToTargetType(e.NewValue, targetPropertyType);

            var parameter = System.Linq.Expressions.Expression.Parameter(elementType, "t");
            var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);

            System.Linq.Expressions.Expression predicateBody;
            System.Linq.Expressions.Expression constantExpression = null;

            if (propertyType == typeof(string))
            {
                var toStringMethod = propertyAccess.Type.GetMethod("ToString", Type.EmptyTypes)
                 ?? typeof(object).GetMethod("ToString", Type.EmptyTypes)!;

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                var keyword = convertedValue?.ToString() ?? string.Empty;
                var keywordExpression = System.Linq.Expressions.Expression.Constant(keyword, typeof(string));

                System.Linq.Expressions.Expression propertyString = propertyAccess;

                if (propertyString.Type != typeof(string))
                {
                    propertyString = System.Linq.Expressions.Expression.Call(propertyAccess, toStringMethod);
                }

                predicateBody = System.Linq.Expressions.Expression.Call(propertyString, containsMethod, keywordExpression);
            }
            else
            {
                if (convertedValue == null)
                {
                    if (!propertyType.IsValueType || propertyType.IsNullableType())
                    {
                        constantExpression = System.Linq.Expressions.Expression.Constant(null, propertyType);
                    }
                    else
                    {
                        rsPicker.SelectedItem = null;
                        return;
                    }
                }
                else
                {
                    constantExpression = System.Linq.Expressions.Expression.Constant(convertedValue, targetPropertyType);
                    if (propertyType != targetPropertyType)
                    {
                        constantExpression = System.Linq.Expressions.Expression.Convert(constantExpression, propertyType);
                    }
                }

                predicateBody = System.Linq.Expressions.Expression.Equal(propertyAccess, constantExpression);
            }

            var lambdaType = typeof(Func<,>).MakeGenericType(elementType, typeof(bool));
            var lambda = System.Linq.Expressions.Expression.Lambda(lambdaType, predicateBody, parameter);

            var queryable = rsPicker.SourceList.AsQueryable();
            var castMethod = typeof(Queryable).GetMethod("Cast")!.MakeGenericMethod(elementType);
            var castedQueryable = castMethod.Invoke(null, new object[] { queryable });

            var firstOrDefaultMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "FirstOrDefault" && m.IsGenericMethodDefinition)
                .Select(m => new { Method = m, Params = m.GetParameters() })
                .Where(x => x.Params.Length == 2)
                .Where(x =>
                {
                    var secondParamType = x.Params[1].ParameterType;
                    return secondParamType.IsGenericType
                           && secondParamType.GetGenericTypeDefinition() == typeof(Expression<>);
                })
                .Select(x => x.Method)
                .First()
                .MakeGenericMethod(elementType);

            selectedItem = firstOrDefaultMethod.Invoke(null, new object[] { castedQueryable, lambda });
            rsPicker.SelectedItem = selectedItem;
        }

        private static object ChangeToTargetType(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        [Description("搜索属性名称")]
        public string SearchPropertyName
        {
            get { return (string)GetValue(SearchPropertyNameProperty); }
            set { SetValue(SearchPropertyNameProperty, value); }
        }
        public static readonly DependencyProperty SearchPropertyNameProperty =
            DependencyProperty.Register("SearchPropertyName", typeof(string), typeof(RSPicker), new PropertyMetadata(null));




        [Description("选择项背景色")]
        public Brush SelectedBackground
        {
            get { return (Brush)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register("SelectedBackground", typeof(Brush), typeof(RSPicker), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E90FF"))));


        [Description("选择项背字体颜色")]
        public Brush SelectedForground
        {
            get { return (Brush)GetValue(SelectedForgroundProperty); }
            set { SetValue(SelectedForgroundProperty, value); }
        }

        public static readonly DependencyProperty SelectedForgroundProperty =
            DependencyProperty.Register("SelectedForground", typeof(Brush), typeof(RSPicker), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"))));



        [Description("选择项")]
        public ItemWrapper ItemWrapperSelect
        {
            get { return (ItemWrapper)GetValue(ItemWrapperSelectProperty); }
            set { SetValue(ItemWrapperSelectProperty, value); }
        }

        public static readonly DependencyProperty ItemWrapperSelectProperty =
            DependencyProperty.Register("ItemWrapperSelect", typeof(ItemWrapper), typeof(RSPicker), new PropertyMetadata(null, OnItemWrapperSelectPropertyChanged));

        private static void OnItemWrapperSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsPicker = d as RSPicker;
            if (rsPicker.ItemWrapperSelect != null)
            {
                rsPicker.IsCanRefreshItemsList = false;
                rsPicker.SelectedItem = rsPicker.ItemWrapperSelect.Item;
                rsPicker.IsCanRefreshItemsList = true;
            }
        }


        [Description("是否可以搜索")]
        public bool IsCanSearch
        {
            get { return (bool)GetValue(IsCanSearchProperty); }
            set { SetValue(IsCanSearchProperty, value); }
        }

        public static readonly DependencyProperty IsCanSearchProperty =
            DependencyProperty.Register("IsCanSearch", typeof(bool), typeof(RSPicker), new PropertyMetadata(true));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ContentHost = this.GetTemplateChild(nameof(this.PART_ContentHost)) as Grid;
            this.PART_BtnScrollUp = this.GetTemplateChild(nameof(this.PART_BtnScrollUp)) as RepeatButton;
            this.PART_BtnScrollDown = this.GetTemplateChild(nameof(this.PART_BtnScrollDown)) as RepeatButton;
            this.PART_Canvas = this.GetTemplateChild(nameof(this.PART_Canvas)) as Canvas;
            if (this.PART_BtnScrollUp != null)
            {
                this.PART_BtnScrollUp.Click += PART_BtnScrollUp_Click;
            }

            if (this.PART_BtnScrollDown != null)
            {
                this.PART_BtnScrollDown.Click += PART_BtnScrollDown_Click;
            }

            if (this.PART_ContentHost != null)
            {
                this.PART_ContentHost.PreviewMouseWheel += PART_ContentHost_MouseWheel;
            }

            this.RefreshItemsList();
        }

        public void SmallDecrement()
        {
            //double num = Math.Max(base.Value - base.SmallChange, base.Minimum);
            //if (base.Value != num)
            //{
            //    base.Value = num;
            //    //RaiseScrollEvent(ScrollEventType.SmallDecrement);
            //}


        }

        //public void SmallIncrement()
        //{
        //    double num = Math.Min(base.Value + base.SmallChange, base.Maximum);
        //    if (base.Value != num)
        //    {
        //        base.Value = num;
        //        //RaiseScrollEvent(ScrollEventType.SmallIncrement);
        //    }
        //}


        /// <summary>
        /// 更新实际渲染资源
        /// </summary>
        private void RefreshItemsList()
        {
            if (this.PART_Canvas == null)
            {
                return;
            }

            //if (this.SelectedItem == null
            //    && this.SourceList != null
            //    && this.SourceList.Count > 0)
            //{
            //    this.SelectedItem = this.SourceList[0];
            //}


            //获取窗口平移数据
            var dataList = this.CreateSlidingWindow(this.SourceList, this.SelectedIndex, 15);
            int count = dataList.Count;
            var sourceList = dataList.Select((item, idx) => new ItemWrapper()
            {
                Item = item,
                //每次都是中间项被选中
                IsSelected = (idx == count / 2)
            }).ToList();

            //设置当前选择项
            this.ItemWrapperSelect = sourceList.FirstOrDefault(t => t.IsSelected);

            //如果不为空 解除事件
            if (this.ItemsList != null)
            {
                foreach (var item in this.ItemsList)
                {
                    item.MouseLeftButtonUp -= ItemTemplate_MouseLeftButtonUp;
                }
            }

            this.ItemsList = new List<FrameworkElement>();
            foreach (var item in sourceList)
            {
                var itemTemplate = this.CreateItemContainer(item);
                if (itemTemplate != null)
                {
                    if (this.CalcuWidth < itemTemplate.ActualWidth)
                    {
                        this.CalcuWidth = itemTemplate.ActualWidth;
                    }
                    itemTemplate.MouseLeftButtonUp += ItemTemplate_MouseLeftButtonUp;
                    this.ItemsList.Add(itemTemplate);
                }
            }
            //this.Width = Math.Max(this.MinWidth, this.CalcuWidth);
            this.AddItemsIntoCanvas();
        }

        private void AddItemsIntoCanvas()
        {
            this.PART_Canvas.Children.Clear();
            //先全部添加进去
            foreach (var item in this.ItemsList)
            {
                this.PART_Canvas.Children.Add(item);
            }

            //更新位置信息
            this.UpdateVisualItemsLayout();
        }



        private double CalcuWidth = 60;
        private int SelectedIndex = -1;

        /// <summary>
        /// 这里只刷新尺寸  这样节省时间 
        /// </summary>
        private void UpdateVisualItemsLayout()
        {

            if (this.PART_Canvas == null
                || this.PART_Canvas.ActualHeight == 0
                || this.ItemsList == null
                || this.ItemsList.Count == 0)
            {
                return;
            }

            int middleIndex = this.ItemsList.Count / 2;
            var middleElement = this.ItemsList[middleIndex];
            var canvasHeight = this.PART_Canvas.ActualHeight;
            var middleHeight = middleElement.ActualHeight;
            double middleTop = canvasHeight / 2 - middleHeight / 2;
            //更新中部位置
            UpdateItemsPosition(middleElement, middleTop);
            //从中间向顶部更新位置
            double canvasTop = middleTop;
            for (var i = middleIndex - 1; i >= 0; i--)
            {
                var frameworkElement = this.ItemsList[i];
                canvasTop = canvasTop - frameworkElement.ActualHeight;
                UpdateItemsPosition(frameworkElement, canvasTop);
            }

            //从中间向底部更新位置
            canvasTop = middleTop + middleElement.ActualHeight;
            for (var i = middleIndex + 1; i < this.ItemsList.Count; i++)
            {
                var frameworkElement = this.ItemsList[i];
                UpdateItemsPosition(frameworkElement, canvasTop);
                canvasTop = canvasTop + frameworkElement.ActualHeight;
            }
        }


        private void UpdateItemsPosition(FrameworkElement frameworkElement, double canvasTop)
        {

            double canvasWidth = this.ActualWidth;
            double left = 0;
            if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
            {
                double elementWidth = frameworkElement.Width > 0 ? frameworkElement.Width : frameworkElement.ActualWidth;
                left = Math.Max(0, (canvasWidth - elementWidth) / 2);
            }

            Canvas.SetLeft(frameworkElement, left);
            Canvas.SetTop(frameworkElement, canvasTop);
        }

        public List<object> CreateSlidingWindow(List<object> source, int centerIndex, int radius)
        {
            if (source == null || source.Count == 0)
            {
                return new List<object>();
            }
            int sourceCount = source.Count;
            int windowSize = radius * 2 + 1;
            List<object> result = new List<object>(windowSize);

            for (int i = 0; i < windowSize; i++)
            {
                // 计算窗口内当前位置对应的源列表索引（环形映射）
                int sourceIndex = (centerIndex - radius + i) % sourceCount;
                if (sourceIndex < 0)
                {
                    sourceIndex += sourceCount;
                }
                result.Add(source[sourceIndex]);
            }

            return result;
        }

        private void ItemTemplate_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var itemTemplate = sender as FrameworkElement;
            var itemWrapper = itemTemplate.DataContext as ItemWrapper;
            var objectList = this.ItemsSource.Cast<object>().ToList();
            var index = objectList.IndexOf(itemWrapper.Item);
            this.SelectedItem = itemWrapper.Item;
        }


        private DateTime lastScrollTime = DateTime.MinValue;
        private double baseInterval = 100; // 基础间隔(毫秒)，用于计算速度

        private void PART_ContentHost_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double interval = (DateTime.Now - lastScrollTime).TotalMilliseconds;
            lastScrollTime = DateTime.Now;

            int increment = 1;
            if (interval > 0 && interval < baseInterval)
            {
                double speedFactor = baseInterval / interval;
                increment = Math.Max(1, (int)Math.Round(speedFactor));
                increment = Math.Min(increment, 5);
            }
            List<object> dataList = new List<object>();
            if (e.Delta < 0)
            {
                this.SelectedIndex = this.SelectedIndex - increment;
            }
            else
            {
                this.SelectedIndex = this.SelectedIndex + increment;
            }

            this.RefreshItemsList();
            e.Handled = true;
        }


        private void PART_BtnScrollDown_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedIndex++;
            this.RefreshItemsList();
        }

        public DispatcherTimer DispatcherTimer { get; set; }

        private void PART_BtnScrollUp_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedIndex--;
            this.RefreshItemsList();
        }



        private FrameworkElement CreateItemContainer(object item)
        {
            if (this.ItemTemplate == null)
            {
                return null;
            }
            ContentPresenter presenter = new ContentPresenter
            {
                Content = item,
                ContentTemplate = this.ItemTemplate,
            };
            presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            presenter.Arrange(new Rect(presenter.DesiredSize));
            return presenter;
        }



    }
}
