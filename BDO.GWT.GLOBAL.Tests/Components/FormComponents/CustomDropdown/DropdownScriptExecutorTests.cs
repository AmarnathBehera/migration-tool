using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class DropdownScriptExecutorTests
    {
        private DropdownScriptExecutor _executor;

        [SetUp]
        public void Setup()
        {
            _executor = new DropdownScriptExecutor();
        }

        [Test]
        public async Task ExecuteAsync_WhenSqlIsNull_ReturnsEmpty()
        {
            var result = await _executor.ExecuteAsync(null!);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_WhenSqlIsEmpty_ReturnsEmpty()
        {
            var result = await _executor.ExecuteAsync(string.Empty);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_WhenSqlIsWhitespace_ReturnsEmpty()
        {
            var result = await _executor.ExecuteAsync("   ");

            Assert.That(result, Is.Empty);
        }
    }
}