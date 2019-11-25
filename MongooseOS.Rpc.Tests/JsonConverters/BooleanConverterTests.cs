using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongooseOS.Rpc.JsonConverters;
using System.Text.Json;

namespace MongooseOS.Rpc.Tests
{
    [TestClass]
    public class BooleanConverterTests
    {
        private BooleanConverter _converter;
        private JsonSerializerOptions _serializationOptions;

        [TestInitialize]
        public void Setup()
        {
            _converter = new BooleanConverter();
            _serializationOptions = new JsonSerializerOptions();
            _serializationOptions.Converters.Add(_converter);
        }

        class TestClass
        {
            public bool Bool { get; set; }
        }

        [TestMethod]
        public void Boolean_True_ReadsAsBoolean()
        {
            var target = JsonSerializer.Deserialize<TestClass>("{\"Bool\": true}", _serializationOptions);

            target.Bool.Should().BeTrue();
        }

        [TestMethod]
        public void StringBoolean_True_ReadsAsBoolean()
        {
            var target = JsonSerializer.Deserialize<TestClass>("{\"Bool\": \"true\"}", _serializationOptions);

            target.Bool.Should().BeTrue();
        }

        [TestMethod]
        public void Boolean_False_ReadsAsBoolean()
        {
            var target = JsonSerializer.Deserialize<TestClass>("{\"Bool\": false}", _serializationOptions);

            target.Bool.Should().BeFalse();
        }

        [TestMethod]
        public void StringBoolean_False_ReadsAsBoolean()
        {
            var target = JsonSerializer.Deserialize<TestClass>("{\"Bool\": \"false\"}", _serializationOptions);

            target.Bool.Should().BeFalse();
        }
    }
}
