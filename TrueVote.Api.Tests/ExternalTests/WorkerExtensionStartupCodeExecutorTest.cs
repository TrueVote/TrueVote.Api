using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

// This test shouldn't be necessary. It's simply to "cover" Microsoft code that can't be excluded from coverage and can't be removed because of a known limitation in
// Azure Functions Isolated Process
// https://github.com/Azure/azure-functions-dotnet-worker/issues/1864
// As of now, this doesn't even work. It doesn't cover the right generated C# code. Leaving in for now.
namespace TrueVote.Api.Tests.ExternalTests
{
    public class WorkerExtensionStartupCodeExecutorTest : TestHelper
    {
        public WorkerExtensionStartupCodeExecutorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void RunsConfigure()
        {
            var executor = new WorkerExtensionStartupCodeExecutor();
            var builder = new MockFunctionsWorkerApplicationBuilder();

            // Act
            executor.Configure(builder);
        }
    }

    public class MockFunctionsWorkerApplicationBuilder : IFunctionsWorkerApplicationBuilder
    {
        public IServiceCollection Services { get; private set; }

        private readonly List<Func<FunctionContext, ValueTask>> _middleware;

        public MockFunctionsWorkerApplicationBuilder()
        {
            Services = new ServiceCollection();
            _middleware = new List<Func<FunctionContext, ValueTask>>();
        }

        public IFunctionsWorkerApplicationBuilder Use(Func<FunctionExecutionDelegate, FunctionExecutionDelegate> middleware)
        {
            // Convert the provided ASP.NET Core middleware to Azure Functions middleware.
            Func<FunctionContext, ValueTask> azureFunctionsMiddleware = async context =>
            {
                var executionDelegate = new FunctionExecutionDelegate(nextContext => Task.CompletedTask);
                await middleware(executionDelegate)(context);
            };

            _middleware.Add(azureFunctionsMiddleware);
            return this;
        }

        public IFunctionsWorkerApplicationBuilder UseMiddleware<TMiddleware>()
        {
            // Implement any necessary methods for testing.
            return this;
        }

        public async ValueTask ExecuteAsync(FunctionContext context)
        {
            // Execute the middleware in the order they were added.
            foreach (var middleware in _middleware)
            {
                await middleware(context);
            }
        }

        // Add other required methods as needed for your tests.
    }
}
