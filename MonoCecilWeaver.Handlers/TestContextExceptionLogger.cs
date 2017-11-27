using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoCecilWeaver.Target;

namespace MonoCecilWeaver.Handlers
{
    public class TestContextExceptionLogger : ExceptionHandler
    {
        private const string TestContextProperty = "TestContext";
        private const string FailureInfoKey = "FailureInformation";

        public override void Handle(Exception ex, object instance, MethodBase method)
        {
            var failureMessage = $"Message: {ex.Message}";
            var stackTrace = $"StackTrace: {Environment.NewLine}{ex.StackTrace}";
            var failureInfo = $"{method.Name} method FAILED. {Environment.NewLine}{failureMessage}{Environment.NewLine}{stackTrace}";

            var testContextProperty = instance
                ?.GetType()
                .GetProperty(TestContextProperty)
                ?.GetValue(instance);

            if (testContextProperty is TestContext testContext)
            {
                if (testContext.Properties.Contains(FailureInfoKey))
                {
                    var currentFailureInfo = testContext.Properties[FailureInfoKey].ToString();
                    testContext.Properties[FailureInfoKey] = $"{currentFailureInfo}{Environment.NewLine}{Environment.NewLine}{failureInfo}";
                }
                else
                {
                    testContext.Properties[FailureInfoKey] = failureInfo;
                }
            }

            throw new Exception(failureInfo);
        }
    }
}
