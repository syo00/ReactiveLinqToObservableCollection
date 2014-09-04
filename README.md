Reactive ObservableCollection Mapper
====================================
With Reactive ObservableCollection Mapper, you can convert a ObservableCollection/ReadOnlyObservableCollection into new ReadOnlyObservableCollection like LINQ to objects.

This project is still in beta and may change name usage of methods.

QuickStart
----------
write **using Kirinji.LinqToObservableCollection**.
 
```csharp
ObservableCollection<string> observableCollection = ...; // create or get ObservableCollection. you can also use ReadOnlyObservableCollection
ReadOnlyObservableCollection<string> observableCollection =
    .ToStatuses() // ToStatuses() makes you can use LINQ methods
    .Where(str => str != null && str.Length >= 5)
    .Select(str => str.ToUpper())
    .OrderBy(str => str, EqualityComparer<string>.Default)
    .ToObservableCollection(); // ToObservableCollection() creates ReadOnlyObservableCollection
```

When items in source collection are changed, the changes will be applied to the converted collection.

In additional, you can use some of useful reactive extensions methods such as ObserveOn and Synchronize. They make very easy to handle  ObservableCollection thread and synchronization.

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
MIT License.

for details, please see License.txt