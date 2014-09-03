using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Kirinji.LinqToObservableCollection.Test
{
    [TestClass]
    public class NotifyCollectionChangedEventTest
    {
        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Reset の変更を説明する
        //     System.Collections.Specialized.NotifyCollectionChangedEventArgs クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 これは、System.Collections.Specialized.NotifyCollectionChangedAction.Reset
        //     に設定する必要があります。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action);
         */
        [TestMethod]
        public void Constructor1Test()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            var i = NotifyCollectionChangedEvent.Convert<int>(e, () => new int[] { -1, -2 });
            i.Action.Is(NotifyCollectionChangedEventAction.Reset);
            i.Reset.Items.Is(-1, -2);
        }

        /*
        // 概要:
        //     複数項目の変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Reset、System.Collections.Specialized.NotifyCollectionChangedAction.Add、または
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Remove に設定できます。
        //
        //   changedItems:
        //     変更の影響を受ける項目。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems);
         */
        [TestMethod]
        public void Constructor2Test()
        {
            var addEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { 765, 961 });
            var removeEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { 765, 961 });
            var resetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, (IList<int>)null);

            var add = NotifyCollectionChangedEvent.Convert<int>(addEventArgs, () => new int[] { -1, -2 });
            var remove = NotifyCollectionChangedEvent.Convert<int>(removeEventArgs, () => new int[] { -1, -2 });
            var reset = NotifyCollectionChangedEvent.Convert<int>(resetEventArgs, () => new int[] { -1, -2 });

            add.Action.Is(NotifyCollectionChangedEventAction.Add);
            add.Added.Items.Is(765, 961);
            add.Added.StartingIndex.Is(-1);

            remove.Action.Is(NotifyCollectionChangedEventAction.Remove);
            remove.Removed.Items.Is(765, 961);
            remove.Removed.StartingIndex.Is(-1);

            reset.Action.Is(NotifyCollectionChangedEventAction.Reset);
            reset.Reset.Items.Is(-1, -2);
        }

        /*
        // 概要:
        //     1 項目の変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Reset、System.Collections.Specialized.NotifyCollectionChangedAction.Add、または
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Remove に設定できます。
        //
        //   changedItem:
        //     変更の影響を受ける項目。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Reset、Add、Remove ではない場合、または action が Reset で、かつ changedItem が null
        //     ではない場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem);
         */
        [TestMethod]
        public void Constructor3Test()
        {
            var addEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 765);
            var removeEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 765);
            var resetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, (object)null);

            var add = NotifyCollectionChangedEvent.Convert<int>(addEventArgs, () => new int[] { -1, -2 });
            var remove = NotifyCollectionChangedEvent.Convert<int>(removeEventArgs, () => new int[] { -1, -2 });
            var reset = NotifyCollectionChangedEvent.Convert<int>(resetEventArgs, () => new int[] { -1, -2 });

            add.Action.Is(NotifyCollectionChangedEventAction.Add);
            add.Added.Items.Is(765);
            add.Added.StartingIndex.Is(-1);

            remove.Action.Is(NotifyCollectionChangedEventAction.Remove);
            remove.Removed.Items.Is(765);
            remove.Removed.StartingIndex.Is(-1);

            reset.Action.Is(NotifyCollectionChangedEventAction.Reset);
            reset.Reset.Items.Is(-1, -2);
        }

        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Replace による複数項目の変更を表す
        //     System.Collections.Specialized.NotifyCollectionChangedEventArgs クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Replace
        //     のみに設定できます。
        //
        //   newItems:
        //     元の項目を置き換える新しい項目。
        //
        //   oldItems:
        //     置き換えられる元の項目。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Replace ではない場合。
        //
        //   System.ArgumentNullException:
        //     oldItems または newItems が null の場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems);
         */
        [TestMethod]
        public void Constructor4Test()
        {
            var replaceEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { 765, 961 }, new[] { 573, 876 });

            var replace = NotifyCollectionChangedEvent.Convert<int>(replaceEventArgs, () => new int[] { -1, -2 });

            replace.Action.Is(NotifyCollectionChangedEventAction.Replace);
            replace.Replaced.OldItems.Is(573, 876);
            replace.Replaced.NewItems.Is(765, 961);
            replace.Replaced.StartingIndex.Is(-1);
        }

        /*
        // 概要:
        //     複数項目の変更または System.Collections.Specialized.NotifyCollectionChangedAction.Reset
        //     による変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Reset、System.Collections.Specialized.NotifyCollectionChangedAction.Add、または
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Remove に設定できます。
        //
        //   changedItems:
        //     変更の影響を受ける項目。
        //
        //   startingIndex:
        //     変更が発生したインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Reset、Add、Remove ではない場合、action が Reset で、かつchangedItems が null ではないか、
        //     startingIndex が -1 ではない場合、または action が Add または Remove で、かつstartingIndex が
        //     -1 より小さい場合。
        //
        //   System.ArgumentNullException:
        //     action が Add または Remove で、changedItems が null の場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex);
         */
        [TestMethod]
        public void Constructor5Test()
        {
            var addEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { 765, 961 }, 1);
            var removeEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { 765, 961 }, 1);
            var resetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, (IList<int>)null, -1);

            var add = NotifyCollectionChangedEvent.Convert<int>(addEventArgs, () => new int[] { -1, -2 });
            var remove = NotifyCollectionChangedEvent.Convert<int>(removeEventArgs, () => new int[] { -1, -2 });
            var reset = NotifyCollectionChangedEvent.Convert<int>(resetEventArgs, () => new int[] { -1, -2 });

            add.Action.Is(NotifyCollectionChangedEventAction.Add);
            add.Added.Items.Is(765, 961);
            add.Added.StartingIndex.Is(1);

            remove.Action.Is(NotifyCollectionChangedEventAction.Remove);
            remove.Removed.Items.Is(765, 961);
            remove.Removed.StartingIndex.Is(1);

            reset.Action.Is(NotifyCollectionChangedEventAction.Reset);
            reset.Reset.Items.Is(-1, -2);
        }

        /*
        // 概要:
        //     1 項目の変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Reset、System.Collections.Specialized.NotifyCollectionChangedAction.Add、または
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Remove に設定できます。
        //
        //   changedItem:
        //     変更の影響を受ける項目。
        //
        //   index:
        //     変更が発生したインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Reset、Add、Remove ではない場合、または action が Reset で、かつ changedItems が null
        //     ではないか、index が -1 ではない場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index);
         */
        [TestMethod]
        public void Constructor6Test()
        {
            var addEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 765, 1);
            var removeEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 765, 1);
            var resetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, (object)null, -1);

            var add = NotifyCollectionChangedEvent.Convert<int>(addEventArgs, () => new int[] { -1, -2 });
            var remove = NotifyCollectionChangedEvent.Convert<int>(removeEventArgs, () => new int[] { -1, -2 });
            var reset = NotifyCollectionChangedEvent.Convert<int>(resetEventArgs, () => new int[] { -1, -2 });

            add.Action.Is(NotifyCollectionChangedEventAction.Add);
            add.Added.Items.Is(765);
            add.Added.StartingIndex.Is(1);

            remove.Action.Is(NotifyCollectionChangedEventAction.Remove);
            remove.Removed.Items.Is(765);
            remove.Removed.StartingIndex.Is(1);

            reset.Action.Is(NotifyCollectionChangedEventAction.Reset);
            reset.Reset.Items.Is(-1, -2);
        }

        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Replace による
        //     1 項目の変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Replace
        //     のみに設定できます。
        //
        //   newItem:
        //     元の項目を置き換える新しい項目。
        //
        //   oldItem:
        //     置き換えられる元の項目。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Replace ではない場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem);
         */
        [TestMethod]
        public void Constructor7Test()
        {
            var replaceEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object)765, (object)961);

            var replace = NotifyCollectionChangedEvent.Convert<int>(replaceEventArgs, () => new int[] { -1, -2 });

            replace.Action.Is(NotifyCollectionChangedEventAction.Replace);
            replace.Replaced.OldItems.Is(961);
            replace.Replaced.NewItems.Is(765);
            replace.Replaced.StartingIndex.Is(-1);
        }

        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Replace による複数項目の変更を表す
        //     System.Collections.Specialized.NotifyCollectionChangedEventArgs クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Replace
        //     のみに設定できます。
        //
        //   newItems:
        //     元の項目を置き換える新しい項目。
        //
        //   oldItems:
        //     置き換えられる元の項目。
        //
        //   startingIndex:
        //     置き換えられる項目の最初の項目のインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Replace ではない場合。
        //
        //   System.ArgumentNullException:
        //     oldItems または newItems が null の場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex);
         */
        [TestMethod]
        public void Constructor8Test()
        {
            var replaceEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { 765, 961 }, new[] { 573, 876 }, 1);

            var replace = NotifyCollectionChangedEvent.Convert<int>(replaceEventArgs, () => new int[] { -1, -2 });

            replace.Action.Is(NotifyCollectionChangedEventAction.Replace);
            replace.Replaced.OldItems.Is(573, 876);
            replace.Replaced.NewItems.Is(765, 961);
            replace.Replaced.StartingIndex.Is(1);
        }

        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Move による複数項目の変更を表す
        //     System.Collections.Specialized.NotifyCollectionChangedEventArgs クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Move
        //     のみに設定できます。
        //
        //   changedItems:
        //     変更の影響を受ける項目。
        //
        //   index:
        //     変更された項目の新しいインデックス。
        //
        //   oldIndex:
        //     変更された項目の古いインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Move ではない場合、または index が 0 未満の場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex);
         */
        [TestMethod]
        public void Constructor9Test()
        {
            var moveEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new[] { 765, 961 }, 5, 10);

            var move = NotifyCollectionChangedEvent.Convert<int>(moveEventArgs, () => new int[] { -1, -2 });

            move.Action.Is(NotifyCollectionChangedEventAction.Move);
            move.Moved.Items.Is(765, 961);
            move.Moved.OldStartingIndex.Is(10);
            move.Moved.NewStartingIndex.Is(5);
        }

        /*        
        // 概要:
        //     1 項目の System.Collections.Specialized.NotifyCollectionChangedAction.Move の変更を表す
        //     System.Collections.Specialized.NotifyCollectionChangedEventArgs クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 System.Collections.Specialized.NotifyCollectionChangedAction.Move
        //     のみに設定できます。
        //
        //   changedItem:
        //     変更の影響を受ける項目。
        //
        //   index:
        //     変更された項目の新しいインデックス。
        //
        //   oldIndex:
        //     変更された項目の古いインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Move ではない場合、または index が 0 未満の場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex);
         */
        [TestMethod]
        public void Constructor10Test()
        {
            var moveEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, 765, 5, 10);

            var move = NotifyCollectionChangedEvent.Convert<int>(moveEventArgs, () => new int[] { -1, -2 });

            move.Action.Is(NotifyCollectionChangedEventAction.Move);
            move.Moved.Items.Is(765);
            move.Moved.OldStartingIndex.Is(10);
            move.Moved.NewStartingIndex.Is(5);
        }

        /*
        // 概要:
        //     System.Collections.Specialized.NotifyCollectionChangedAction.Replace による
        //     1 項目の変更を表す System.Collections.Specialized.NotifyCollectionChangedEventArgs
        //     クラスの新しいインスタンスを初期化します。
        //
        // パラメーター:
        //   action:
        //     イベントの原因となったアクション。 これは、System.Collections.Specialized.NotifyCollectionChangedAction.Replace
        //     に設定できます。
        //
        //   newItem:
        //     元の項目を置き換える新しい項目。
        //
        //   oldItem:
        //     置き換えられる元の項目。
        //
        //   index:
        //     置き換えられる項目のインデックス。
        //
        // 例外:
        //   System.ArgumentException:
        //     action が Replace ではない場合。
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index);
         */
        [TestMethod]
        public void Constructor11Test()
        {
            var replaceEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object)765, (object)961, 10);

            var replace = NotifyCollectionChangedEvent.Convert<int>(replaceEventArgs, () => new int[] { -1, -2 });

            replace.Action.Is(NotifyCollectionChangedEventAction.Replace);
            replace.Replaced.OldItems.Is(961);
            replace.Replaced.NewItems.Is(765);
            replace.Replaced.StartingIndex.Is(10);
        }
    }
}
