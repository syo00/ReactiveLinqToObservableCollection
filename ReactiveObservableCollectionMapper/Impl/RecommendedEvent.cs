using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    internal struct RecommendedEvent
    {
        public RecommendedEvent(bool? initialStateAndChanged, bool? slimInitialStateAndChanged, bool? simpleInitialStateAndChanged, bool? slimSimpleInitialStateAndChanged)
            : this()
        {
            this.InitialStateAndChanged = initialStateAndChanged;
            this.SlimInitialStateAndChanged = slimInitialStateAndChanged;
            this.SimpleInitialStateAndChanged = simpleInitialStateAndChanged;
            this.SlimSimpleInitialStateAndChanged = slimSimpleInitialStateAndChanged;
        }

        public bool? InitialStateAndChanged { get; private set; }
        public bool? SlimInitialStateAndChanged { get; private set; }
        public bool? SimpleInitialStateAndChanged { get; private set; }
        public bool? SlimSimpleInitialStateAndChanged { get; private set; }

        public RecommendedEventType RecommendedEventType
        {
            get
            {
                bool? maxValue = false;
                var maxValueType = RecommendedEventType.None;
                
                if(IsRightValueGreater(maxValue, InitialStateAndChanged) == true)
                {
                    maxValue = InitialStateAndChanged;
                    maxValueType = RecommendedEventType.DefaultOne;
                    if(maxValue == true)
                    {
                        return maxValueType;
                    }
                }

                if (IsRightValueGreater(maxValue, SlimInitialStateAndChanged) == true)
                {
                    maxValue = SlimInitialStateAndChanged;
                    maxValueType = RecommendedEventType.SlimOne;
                    if (maxValue == true)
                    {
                        return maxValueType;
                    }
                }

                if (IsRightValueGreater(maxValue, SimpleInitialStateAndChanged) == true)
                {
                    maxValue = SlimInitialStateAndChanged;
                    maxValueType = RecommendedEventType.SimpleOne;
                    if (maxValue == true)
                    {
                        return maxValueType;
                    }
                }

                if (IsRightValueGreater(maxValue, SlimSimpleInitialStateAndChanged) == true)
                {
                    maxValue = SlimInitialStateAndChanged;
                    maxValueType = RecommendedEventType.SlimSimpleOne;
                }

                return maxValueType;
            }
        }

        static bool? IsRightValueGreater(bool? x, bool? y)
        {
            var remainder = ToInt32(y) - ToInt32(x);

            if(remainder > 0)
            {
                return true;
            }
            if(remainder == 0)
            {
                return null;
            }
            return false;
        }

        static int ToInt32(bool? value)
        {
            if(value == true)
            {
                return 1;
            }
            
            if(value == null)
            {
                return 0;
            }

            return -1;
        }
    }
}
