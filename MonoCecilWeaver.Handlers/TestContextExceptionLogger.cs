using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoCecilWeaver.Target;

namespace MonoCecilWeaver.Handlers
{
    /// <summary>
    /// Handles an <see cref="Exception"/> that has been thrown.
    /// </summary>
    public class TestContextExceptionLogger : ExceptionHandler
    {
        private const string TestContextProperty = "TestContext";
        private const string FailureInfoKey = "FailureInformation";

        /// <summary>
        /// Invoked when an <see cref="Exception"/> occurs. Handles the thrown <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that needs to be handled.</param>
        /// <param name="instance">The instance of the current method or <see langword="null"/> if static.</param>
        /// <param name="method">The current method.</param>
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
