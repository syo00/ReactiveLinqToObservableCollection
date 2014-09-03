using Kirinji.LinqToObservableCollection.SimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.SlimSimpleNotifyCollectionChangedEvents;
using Kirinji.LinqToObservableCollection.Support;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirinji.LinqToObservableCollection.Impl
{
    // 包むだけで整合性のチェックなどはしない。コンストラクタの引数として渡す IObservable は CheckAndClean されているか、もしくはその規則を守っていること。
    // 信頼できない IObservable は CollectionStatuses.Create を使う
    sealed class AnonymousCollectionStatuses<T> : CollectionStatuses<T>
    {
        readonly Func<IObservable<INotifyCollectionChangedEvent<T>>> initialStateAndChanged;
        readonly Func<IObservable<SlimNotifyCollectionChangedEvent<T>>> slimInitialStateAndChanged;
        readonly Func<IObservable<SimpleNotifyCollectionChangedEvent<T>>> simpleInitialStateAndChanged;
        readonly Func<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>> slimSimpleInitialStateAndChanged;

        public AnonymousCollectionStatuses(IObservable<INotifyCollectionChangedEvent<T>> initialStateAndChanged)
        {
            this.initialStateAndChanged = () => initialStateAndChanged;
        }

        public AnonymousCollectionStatuses(Func<IObservable<INotifyCollectionChangedEvent<T>>> initialStateAndChanged)
        {
            Contract.Requires<ArgumentNullException>(initialStateAndChanged != null);

            this.initialStateAndChanged = initialStateAndChanged;
        }

        public AnonymousCollectionStatuses(IObservable<SimpleNotifyCollectionChangedEvent<T>> simpleInitialStateAndChanged)
        {
            this.simpleInitialStateAndChanged = () => simpleInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(Func<IObservable<SimpleNotifyCollectionChangedEvent<T>>> simpleInitialStateAndChanged)
        {
            Contract.Requires<ArgumentNullException>(simpleInitialStateAndChanged != null);

            this.simpleInitialStateAndChanged = simpleInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(IObservable<SlimNotifyCollectionChangedEvent<T>> slimInitialStateAndChanged)
        {
            this.slimInitialStateAndChanged = () => slimInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(Func<IObservable<SlimNotifyCollectionChangedEvent<T>>> slimInitialStateAndChanged)
        {
            Contract.Requires<ArgumentNullException>(slimInitialStateAndChanged != null);

            this.slimInitialStateAndChanged = slimInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> slimSimpleInitialStateAndChanged)
        {
            this.slimSimpleInitialStateAndChanged = () => slimSimpleInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(Func<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>> slimSimpleInitialStateAndChanged)
        {
            Contract.Requires<ArgumentNullException>(slimSimpleInitialStateAndChanged != null);

            this.slimSimpleInitialStateAndChanged = slimSimpleInitialStateAndChanged;
        }

        public AnonymousCollectionStatuses(
            IObservable<INotifyCollectionChangedEvent<T>> initialStateAndChanged,
            IObservable<SlimNotifyCollectionChangedEvent<T>> slimInitialStateAndChanged,
            IObservable<SimpleNotifyCollectionChangedEvent<T>> simpleInitialStateAndChanged,
            IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> slimSimpleInitialStateAndChanged)
        {
            Contract.Requires<ArgumentException>(
                initialStateAndChanged != null
                || slimInitialStateAndChanged != null
                || simpleInitialStateAndChanged != null
                || slimSimpleInitialStateAndChanged != null);

            if (initialStateAndChanged != null)
            {
                this.initialStateAndChanged = () => initialStateAndChanged;
            }

            if (slimInitialStateAndChanged != null)
            {
                this.slimInitialStateAndChanged = () => slimInitialStateAndChanged;
            }

            if (simpleInitialStateAndChanged != null)
            {
                this.simpleInitialStateAndChanged = () => simpleInitialStateAndChanged;
            }

            if (slimSimpleInitialStateAndChanged != null)
            {
                this.slimSimpleInitialStateAndChanged = () => slimSimpleInitialStateAndChanged;
            }
        }

        public AnonymousCollectionStatuses(
            Func<IObservable<INotifyCollectionChangedEvent<T>>> initialStateAndChanged,
            Func<IObservable<SlimNotifyCollectionChangedEvent<T>>> slimInitialStateAndChanged,
            Func<IObservable<SimpleNotifyCollectionChangedEvent<T>>> simpleInitialStateAndChanged,
            Func<IObservable<SlimSimpleNotifyCollectionChangedEvent<T>>> slimSimpleInitialStateAndChanged)
        {
            Contract.Requires<ArgumentException>(
                initialStateAndChanged != null
                || slimInitialStateAndChanged != null
                || simpleInitialStateAndChanged != null
                || slimSimpleInitialStateAndChanged != null);

            this.initialStateAndChanged = initialStateAndChanged;
            this.slimInitialStateAndChanged = slimInitialStateAndChanged;
            this.simpleInitialStateAndChanged = simpleInitialStateAndChanged;
            this.slimSimpleInitialStateAndChanged = slimSimpleInitialStateAndChanged;
        }

        protected override IObservable<INotifyCollectionChangedEvent<T>> CreateInitialStateAndChanged()
        {
            if (initialStateAndChanged != null)
            {
                return initialStateAndChanged();
            }

            if (slimInitialStateAndChanged != null)
            {
                return slimInitialStateAndChanged().ExtractFromSlim();
            }

            if (simpleInitialStateAndChanged != null)
            {
                return simpleInitialStateAndChanged().Extract();
            }

            return slimSimpleInitialStateAndChanged().ExtractFromSlim().Extract();
        }

        protected override IObservable<SimpleNotifyCollectionChangedEvent<T>> CreateSimpleInitialStateAndChanged()
        {
            if (simpleInitialStateAndChanged != null)
            {
                return simpleInitialStateAndChanged();
            }

            if (slimSimpleInitialStateAndChanged != null)
            {
                return slimSimpleInitialStateAndChanged().ExtractFromSlim();
            }

            if (initialStateAndChanged != null)
            {
                return initialStateAndChanged().Simplify();
            }

            // ToDo: ExtractFromSlimを呼ばないようにして軽量化する
            return slimInitialStateAndChanged().ExtractFromSlim().Simplify();
        }

        protected override IObservable<SlimNotifyCollectionChangedEvent<T>> CreateSlimInitialStateAndChanged()
        {
            if (slimInitialStateAndChanged != null)
            {
                return slimInitialStateAndChanged();
            }

            if (initialStateAndChanged != null)
            {
                return initialStateAndChanged().ToSlim();
            }

            if (simpleInitialStateAndChanged != null)
            {
                return simpleInitialStateAndChanged().Extract().ToSlim();
            }

            return slimSimpleInitialStateAndChanged().ExtractFromSlim().Extract().ToSlim();
        }

        protected override IObservable<SlimSimpleNotifyCollectionChangedEvent<T>> CreateSlimSimpleInitialStateAndChanged()
        {
            if (slimSimpleInitialStateAndChanged != null)
            {
                return slimSimpleInitialStateAndChanged();
            }

            if (simpleInitialStateAndChanged != null)
            {
                return simpleInitialStateAndChanged().ToSlim();
            }

            if (initialStateAndChanged != null)
            {
                return initialStateAndChanged().Simplify().ToSlim();
            }

            return slimInitialStateAndChanged().ExtractFromSlim().Simplify().ToSlim();
        }

        public override RecommendedEvent RecommendedEvent
        {
            get
            {
                if (initialStateAndChanged != null)
                {
                    return new RecommendedEvent(true, false, false, false);
                }

                if (slimInitialStateAndChanged != null)
                {
                    return new RecommendedEvent(false, true, false, false);
                }

                if (simpleInitialStateAndChanged != null)
                {
                    return new RecommendedEvent(false, false, true, false);
                }

                if (slimSimpleInitialStateAndChanged != null)
                {
                    return new RecommendedEvent(false, false, false, true);
                }

                throw Exceptions.UnpredictableSwitchCasePattern;
            }
        }
    }
}
