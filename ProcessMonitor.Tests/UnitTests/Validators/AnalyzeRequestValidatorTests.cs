using FluentValidation.TestHelper;
using ProcessMonitor.API.DTOs;
using ProcessMonitor.API.Validators;

namespace ProcessMonitor.Tests.UnitTests.Validators
{
    [TestClass]
    public class AnalyzeRequestValidatorTests
    {
        private AnalyzeRequestValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new AnalyzeRequestValidator();
        }

        [TestMethod]
        public void Should_Have_Error_When_Action_Is_Empty()
        {
            var model = new AnalyzeRequest("", "some guideline");

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Action);
        }

        [TestMethod]
        public void Should_Have_Error_When_Guideline_Is_Empty()
        {
            var model = new AnalyzeRequest ("do something", "");

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Guideline);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new AnalyzeRequest("some action", "some guideline");

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
