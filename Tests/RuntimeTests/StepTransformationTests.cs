namespace TechTalk.SpecFlow.RuntimeTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using TechTalk.SpecFlow.Bindings;
    using TechTalk.SpecFlow.Bindings.Reflection;
    using TechTalk.SpecFlow.Configuration;
    using TechTalk.SpecFlow.ErrorHandling;
    using TechTalk.SpecFlow.Infrastructure;
    using TechTalk.SpecFlow.Tracing;

    public class User
    {
        public string Name { get; set; }
    }

    [Binding]
    public class UserCreator
    {
        [StepArgumentTransformation("user (w+)")]
        public User Create(string name)
        {
            return new User { Name = name };
        }

        [StepArgumentTransformation]
        public IEnumerable<User> CreateUsers(Table table)
        {
            return table.Rows.Select(tableRow =>
                                     new User { Name = tableRow["Name"] });
        }
    }

    [Binding]
    public class EmployeeCreator
    {
        [StepArgumentTransformation]
        public Employee CreateEmployee(string firstName, string lastName)
        {
            return new Employee { FirstName = firstName, LastName = lastName };
        }
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static bool operator ==(Employee left, Employee right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Employee left, Employee right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Employee)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FirstName != null ? FirstName.GetHashCode() : 0) * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
            }
        }

        protected bool Equals(Employee other)
        {
            return string.Equals(FirstName, other.FirstName) && string.Equals(LastName, other.LastName);
        }
    }

    [TestFixture]
    public class StepTransformationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            // ScenarioContext is needed, because the [Binding]-instances live there
            var scenarioContext = new ScenarioContext(null, null, null);
            contextManagerStub.Setup(cm => cm.ScenarioContext).Returns(scenarioContext);

            bindingRegistryStub.Setup(br => br.GetStepTransformations()).Returns(stepTransformations);
        }

        #endregion

        private readonly Mock<IBindingRegistry> bindingRegistryStub = new Mock<IBindingRegistry>();
        private readonly Mock<IContextManager> contextManagerStub = new Mock<IContextManager>();
        private readonly Mock<IBindingInvoker> methodBindingInvokerStub = new Mock<IBindingInvoker>();
        private readonly List<IStepArgumentTransformationBinding> stepTransformations = new List<IStepArgumentTransformationBinding>();

        private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, IBindingMethod transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, transformMethod);
        }

        private IStepArgumentTransformationBinding CreateStepTransformationBinding(string regexString, MethodInfo transformMethod)
        {
            return new StepArgumentTransformationBinding(regexString, new RuntimeBindingMethod(transformMethod));
        }

        private StepArgumentTypeConverter CreateStepArgumentTypeConverter()
        {
            return new StepArgumentTypeConverter(new Mock<ITestTracer>().Object, bindingRegistryStub.Object, contextManagerStub.Object, methodBindingInvokerStub.Object);
        }

        [Test]
        public void EmployeeConverterShouldConvertMultipleStringsToEmployee()
        {
            var stepTransformationInstance = new EmployeeCreator();
            MethodInfo transformMethod = stepTransformationInstance.GetType().GetMethod("CreateEmployee");
            IStepArgumentTransformationBinding stepTransformationBinding = CreateStepTransformationBinding(@"", transformMethod);

            var invoker = new BindingInvoker(new RuntimeConfiguration(), new Mock<IErrorProvider>().Object);
            TimeSpan duration;
            object result = invoker.InvokeBinding(stepTransformationBinding, contextManagerStub.Object, new object[] { "John", "Smith" }, new Mock<ITestTracer>().Object, out duration);
            Assert.NotNull(result);
            Assert.That(result.GetType(), Is.EqualTo(typeof(Employee)));
            Assert.That(((Employee)result).FirstName, Is.EqualTo("John"));
            Assert.That(((Employee)result).LastName, Is.EqualTo("Smith"));
        }

        [Test]
        public void ShouldUseStepArgumentTransformationToConvertTable()
        {
            var table = new Table("Name");

            var stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod("CreateUsers"));
            IStepArgumentTransformationBinding stepTransformationBinding = CreateStepTransformationBinding(@"", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            TimeSpan duration;
            var resultUsers = new User[3];
            methodBindingInvokerStub.Setup(i => i.InvokeBinding(stepTransformationBinding, It.IsAny<IContextManager>(), new object[] { table }, It.IsAny<ITestTracer>(), out duration))
                .Returns(resultUsers);

            StepArgumentTypeConverter stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            object result = stepArgumentTypeConverter.Convert(table, typeof(IEnumerable<User>), new CultureInfo("en-US"));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(resultUsers));
        }

        [Test]
        public void StepArgumentTypeConverterShouldUseUserConverterForConversion()
        {
            var stepTransformationInstance = new UserCreator();
            var transformMethod = new RuntimeBindingMethod(stepTransformationInstance.GetType().GetMethod("Create"));
            IStepArgumentTransformationBinding stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);
            stepTransformations.Add(stepTransformationBinding);
            TimeSpan duration;
            var resultUser = new User();
            methodBindingInvokerStub.Setup(i => i.InvokeBinding(stepTransformationBinding, It.IsAny<IContextManager>(), It.IsAny<object[]>(), It.IsAny<ITestTracer>(), out duration))
                .Returns(resultUser);

            StepArgumentTypeConverter stepArgumentTypeConverter = CreateStepArgumentTypeConverter();

            object result = stepArgumentTypeConverter.Convert("user xyz", typeof(User), new CultureInfo("en-US"));
            Assert.That(result, Is.EqualTo(resultUser));
        }

        [Test]
        public void UserConverterShouldConvertStringToUser()
        {
            var stepTransformationInstance = new UserCreator();
            MethodInfo transformMethod = stepTransformationInstance.GetType().GetMethod("Create");
            IStepArgumentTransformationBinding stepTransformationBinding = CreateStepTransformationBinding(@"user (\w+)", transformMethod);

            Assert.True(stepTransformationBinding.Regex.IsMatch("user xyz"));

            var invoker = new BindingInvoker(new RuntimeConfiguration(), new Mock<IErrorProvider>().Object);
            TimeSpan duration;
            object result = invoker.InvokeBinding(stepTransformationBinding, contextManagerStub.Object, new object[] { "xyz" }, new Mock<ITestTracer>().Object, out duration);
            Assert.NotNull(result);
            Assert.That(result.GetType(), Is.EqualTo(typeof(User)));
            Assert.That(((User)result).Name, Is.EqualTo("xyz"));
        }
    }
}