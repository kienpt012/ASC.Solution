using ASC.Tests.TestUtilities;
using ASC.Web.Configuration;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ASC.Utilities;

namespace ASC.Tests
{
    public class MathOperationsTests
    {
        private readonly MathOperations mathOperations;

        public MathOperationsTests()
        {
            mathOperations = new MathOperations();
        }

        [Fact]
        public void Add_TwoNumbers_ReturnsCorrectSum()
        {
            // Arrange
            int a = 10, b = 5;

            // Act
            int result = mathOperations.Add(a, b);

            // Assert
            Assert.Equal(15, result);
        }

        [Fact]
        public void Subtract_TwoNumbers_ReturnsCorrectDifference()
        {
            // Arrange
            int a = 10, b = 5;

            // Act
            int result = mathOperations.Subtract(a, b);

            // Assert
            Assert.Equal(5, result);
        }
    }
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<HttpContext> mockHttpContext;
        private readonly Mock<ILogger<HomeController>> loggerMock;

        public HomeControllerTests()
        {
            // Create an instance of Mock IOptions
            optionsMock = new Mock<IOptions<ApplicationSettings>>();
            mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());

            // Create an instance of Mock ILogger
            loggerMock = new Mock<ILogger<HomeController>>();

            // Set IOptions<> Values property to return ApplicationSettings object
            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings
            {
                ApplicationTitle = "Automobile Service Center Application"
            });
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            // Home controller instantiated with Mock IOptions<> and ILogger objects
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert return ViewResult
            Assert.IsType(typeof(ViewResult), controller.Index());
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert Model for Null
            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Assert ModelState Error Count to 0
            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        [Fact]
        public void HomeController_Index_Session_Test()
        {
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            controller.Index();

            // Session value with key "Test" should not be null.
            Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));
        }
    }
}