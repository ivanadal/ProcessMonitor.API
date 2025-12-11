using FluentValidation.TestHelper;
using ProcessMonitor.API.DTOs;
using ProcessMonitor.API.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Tests.UnitTests.Validators
{
    [TestClass]
    public class HistoryQueryValidatorTests
    {
        private HistoryQueryValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new HistoryQueryValidator();
        }

        [TestMethod]
        public void Should_Have_Error_When_Page_Is_Zero_Or_Less()
        {
            var model = new HistoryQuery(0, 10);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Page)
                  .WithErrorMessage("Page must be greater than 0.");
        }

        [TestMethod]
        public void Should_Have_Error_When_PageSize_Is_Zero_Or_Less()
        {
            var model = new HistoryQuery(1, 0);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("PageSize must be greater than 0.");
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new HistoryQuery(1, 10);

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
