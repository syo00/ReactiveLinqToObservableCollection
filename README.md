Important
====================================
*This source has significant problems and I am writing a new code. I will publish it when it becomes practically.*

*このソースは構造に根本的な欠陥があるので新しく作り直します。新しいコードとファイルはある程度形になったら公開します。*


Reactive LINQ to ObservableCollection 
====================================
This library provides quite easy way to **convert a ObservableCollection/ReadOnlyObservableCollection into new ReadOnlyObservableCollection by using LINQ to Objects and Reactive Extensions style methods**.

This library is portable. Targets are:
* .NET Framework 4.5
* Windows 8
* Windows Phone 8.1
* Windows Phone Silverlight 8

This project is still in beta and may change name or usage of methods.

Download
--------
See [release page](https://github.com/syo00/Reactive-ObservableCollection-Mapper/releases)

Requirements
------------
### To use binary(DLL file)
You need Reactive extensions - Query library (Rx-Linq) and its dependent libraries.

### To open and build project files
You need these software and its extensions:

* Visual Studio 2013 professional or higher
* [Code contracts binary rewriter](http://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970)
* [Nuget](http://nuget.org/)

QuickStart
----------
1. write **using Kirinji.LinqToObservableCollection**
1. call **ToStatuses()** to (ReadOnly)ObservableCollection
1. use LINQ methods (e.g. **Select, Where, SelectMany, Reverse, OrderBy, GroupBy, Count, Any, Aggregate and etc.**)
1. call **ToObservableCollection()** to obtain ReadOnlyObservableCollection when result is not IObservable

When items in source collection are changed, the changes will be applied to the result collection.

```csharp
ObservableCollection<string> observableCollection = ...; // create or get ObservableCollection. you can also use ReadOnlyObservableCollection

// (ReadOnly)ObservableCollection -> ReadOnlyObservableCollection
ReadOnlyObservableCollection<string> observableCollection =
    .ToStatuses() // ToStatuses() makes you can use LINQ methods
    .Where(str => str != null && str.Length >= 5)
    .Select(str => str.ToUpper())
    .OrderBy(str => str, Comparer<string>.Default)
    .ToObservableCollection(); // ToObservableCollection() creates ReadOnlyObservableCollection

// (ReadOnly)ObservableCollection -> IObservable
IObservable<int> count = observableCollection
    .ToStatuses()
    .Where(str => str != null && str.Length >= 5)
    .Count();  // Count() returns IObservable<int>
```

In additional, you can use some of useful Reactive Extensions methods such as **ObserveOn**, **SubscribeOn**, and **Synchronize**. They make very easy to change ObservableCollection handling thread and synchronize.

```csharp
ObservableCollection<string> observableCollection = ...;
SynchronizationContext synchronizationContext = ...; // create or get SynchronizationContext
ReadOnlyObservableCollection<string> observableCollection =
    .ToStatuses()
    .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
    .Select(str => SomeSlowMethod(str)) // call SomeSlowMethod on another thread (prevent it using UI thread)
    .ObserveOn(synchronizationContext)
    .ToObservableCollection();
```

License
-------
Released under MIT License.

For details, please see License.txt
