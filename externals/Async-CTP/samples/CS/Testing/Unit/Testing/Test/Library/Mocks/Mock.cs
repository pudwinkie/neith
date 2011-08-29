using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

// This API is designed to mimic Moq, a popular mocking library for .NET
//
// However for this API, we only support mocking IStockSnapshotProvider
//
// This is not how Moq works at all - just the minimal amount of code that allows
// the surface area to appear like Moq.

class Mock<TInterface>
{
    readonly MockSetup mockImpl = new MockSetup();

    internal Mock()
    {
        if (!typeof(TInterface).Equals(typeof(IStockSnapshotProvider)))
        {
            throw new NotImplementedException("This sample only supports mocking IStockSnapshotProvider");
        }
    }

    // Our setup method only supports mocking IStockSnapshotProvider.GetLatestSnapshotAsync()
    internal MockSetup Setup(Expression<Func<IStockSnapshotProvider, Task<StockSnapshot>>> function)
    {
        if (function.Body.NodeType == ExpressionType.Call)
        {
            var innerCall = (MethodCallExpression)function.Body;
            var methodInfo = innerCall.Method;
            if (typeof(IStockSnapshotProvider).Equals(methodInfo.DeclaringType) &&
                methodInfo.IsStatic == false &&
                methodInfo.Name.Equals("GetLatestSnapshotAsync") &&
                methodInfo.GetGenericArguments().Length == 0 &&
                methodInfo.GetParameters().Length == 0)
            {
                // mocking IStockSnapshotProvider.GetLatestSnapshotAsync() (non-generic)
                //
                // This is the only method we support for mocking.
                return this.mockImpl;
            }
        }
        throw new NotImplementedException("This sample only supports mocking IStockSnapshotProvider.GetLatestSnapshotAsync()");
    }

    internal IStockSnapshotProvider Object
    {
        get { return this.mockImpl; }
    }
}

class MockSetup : IStockSnapshotProvider
{
    Action generalCallback = null;
    Func<Task<StockSnapshot>> valueCallback = null;
    Task<StockSnapshot> fixedValue = null;

    internal MockSetup Callback(Action action)
    {
        this.generalCallback = action;
        return this;
    }

    internal void Returns(Task<StockSnapshot> fixedValue)
    {
        this.fixedValue = fixedValue;
    }

    internal void Returns(Func<Task<StockSnapshot>> callback)
    {
        this.valueCallback = callback;
    }

    Task<StockSnapshot> IStockSnapshotProvider.GetLatestSnapshotAsync()
    {
        if (this.generalCallback != null)
        {
            this.generalCallback.Invoke();
        }

        if (this.valueCallback != null)
        {
            return valueCallback.Invoke();
        }

        if (this.fixedValue != null)
        {
            return fixedValue;
        }

        // we weren't set up with enough information to mock IStockSnapshotProvider
        throw new NotImplementedException("Not enough information to mock IStockSnapshotProvider");
    }
}