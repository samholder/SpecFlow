namespace TechTalk.SpecFlow.RuntimeTests
{
    using NUnit.Framework;
    using Rhino.Mocks;
    using TestStatus = TechTalk.SpecFlow.Infrastructure.TestStatus;

    [Binding]
    public class StepExecutionTestsBindings
    {
        [Given("sample step without param")]
        public virtual void BindingWithoutParam()
        {
        }

        [Given("sample step with (single) param")]
        public virtual void BindingWithSingleParam(string param)
        {
        }

        [Given("sample step with (multi)(ple) param")]
        public virtual void BindingWithMultipleParam(string param1, string param2)
        {
        }

        [Given("sample step with table param")]
        public virtual void BindingWithTableParam(Table table)
        {
        }

        [Given("sample step with multi-line string param")]
        public virtual void BindingWithMlStringParam(string multiLineString)
        {
        }

        [Given("sample step with table and multi-line string param")]
        public virtual void BindingWithTableAndMlStringParam(string multiLineString, Table table)
        {
        }

        [Given("sample step with (mixed) params")]
        public virtual void BindingWithMixedParams(string param1, string multiLineString, Table table)
        {
        }

        [Given("sample step with simple convert param: (.*)")]
        public virtual void BindingWithSimpleConvertParam(double param)
        {
        }

        [Given("sample step with wrong param number")]
        public virtual void BindingWithWrongParamNumber(double param)
        {
        }

        [Given("Distinguish by table param")]
        public virtual void DistinguishByTableParam1()
        {
        }

        [Given("Distinguish by table param")]
        public virtual void DistinguishByTableParam2(Table table)
        {
        }
    }

    [Binding]
    public class StepExecutionTestsAmbiguousBindings
    {
        [Given("sample step without param")]
        public virtual void BindingWithoutParam1()
        {
        }

        [Given("sample step without param")]
        public virtual void BindingWithoutParam2()
        {
        }
    }

    [Binding]
    public class StepExecutionTestsStepArgumentTransformationWithMultipleParameters
    {
        [Given("sample step with user param firstname '(.*)' lastname '(.*)'")]
        public virtual void BindingWithMultipleStringParamStepTransformation(Employee employee)
        {
        }

        [Given("sample step with user param firstint (.*) lastint (.*)")]
        public virtual void BindingWithMultipleIntParamStepTransformation(IntThing employee)
        {
        }

        [Given("sample step with user param firstdouble (.*) lastdouble (.*)")]
        public virtual void BindingWithMultipleDoubleParamStepTransformation(DoubleThing doubleThing)
        {
        }

        [StepArgumentTransformation]
        public Employee TransformToEmployee(string firstName, string lastName)
        {
            return new Employee { FirstName = firstName, LastName = lastName };
        }

        [StepArgumentTransformation]
        public IntThing TransformToIntThing(int firstInt, int lastInt)
        {
            return new IntThing { FirstInt = firstInt, LastInt = lastInt };
        }

        [StepArgumentTransformation]
        public DoubleThing TransformToDoubleThing(double firstDouble, double lastDouble)
        {
            return new DoubleThing { FirstDouble = firstDouble, LastDouble = lastDouble };
        }
    }

    public class DoubleThing
    {
        public double FirstDouble { get; set; }
        public double LastDouble { get; set; }

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
            return Equals((DoubleThing)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FirstDouble.GetHashCode() * 397) ^ LastDouble.GetHashCode();
            }
        }

        protected bool Equals(DoubleThing other)
        {
            return FirstDouble.Equals(other.FirstDouble) && LastDouble.Equals(other.LastDouble);
        }
    }

    public class IntThing
    {
        public int FirstInt { get; set; }
        public int LastInt { get; set; }

        public static bool operator ==(IntThing left, IntThing right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IntThing left, IntThing right)
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
            return Equals((IntThing)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FirstInt * 397) ^ LastInt;
            }
        }

        protected bool Equals(IntThing other)
        {
            return FirstInt == other.FirstInt && LastInt == other.LastInt;
        }
    }

    [Binding]
    public class StepExecutionTestsStepArgumentTransformation
    {
        [Given("sample step with user param firstname '(.*)'")]
        public virtual void BindingWithoutParam1(User user)
        {
        }

        [StepArgumentTransformation]
        public User TransformToEmployee(string firstName)
        {
            return new User { Name = firstName };
        }
    }

    [TestFixture]
    public class StepExecutionTests : StepExecutionTestsBase
    {
        [Test]
        public void ShouldCallBindingMultipleParameter()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithMultipleParam("multi", "ple"));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with multiple param");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingSingleParameter()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithSingleParam("single"));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with single param");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWhenStepIsTransformed()
        {
            StepExecutionTestsStepArgumentTransformation bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithoutParam1(Arg<User>.Is.NotNull));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with user param firstname 'John'");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWhenStepWithMultipleDoubleParamsIsTransformed()
        {
            StepExecutionTestsStepArgumentTransformationWithMultipleParameters bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithMultipleDoubleParamStepTransformation(Arg<DoubleThing>.Is.Equal(new DoubleThing { FirstDouble = 10.1, LastDouble = 15.8 })));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with user param firstdouble 10.1 lastdouble 15.8");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWhenStepWithMultipleIntParamsIsTransformed()
        {
            StepExecutionTestsStepArgumentTransformationWithMultipleParameters bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithMultipleIntParamStepTransformation(Arg<IntThing>.Is.Equal(new IntThing { FirstInt = 10, LastInt = 15 })));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with user param firstint 10 lastint 15");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWhenStepWithMultipleParamsIsTransformed()
        {
            StepExecutionTestsStepArgumentTransformationWithMultipleParameters bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithMultipleStringParamStepTransformation(Arg<Employee>.Is.Equal(new Employee { FirstName = "John", LastName = "Smith" })));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with user param firstname 'John' lastname 'Smith'");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWithMixedParams()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            var table = new Table("h1");
            const string mlString = "ml-string";
            bindingInstance.Expect(b => b.BindingWithMixedParams("mixed", mlString, table));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with mixed params", mlString, table);

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWithMlStringParam()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            const string mlString = "ml-string";
            bindingInstance.Expect(b => b.BindingWithMlStringParam(mlString));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with multi-line string param", mlString, null);

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWithTableAndMlStringParam()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            var table = new Table("h1");
            const string mlString = "ml-string";
            bindingInstance.Expect(b => b.BindingWithTableAndMlStringParam(mlString, table));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with table and multi-line string param", mlString, table);

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWithTableParameter()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            var table = new Table("h1");
            bindingInstance.Expect(b => b.BindingWithTableParam(table));

            MockRepository.ReplayAll();

            testRunner.Given("sample step with table param", null, table);

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldCallBindingWithoutParameter()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.BindingWithoutParam());

            MockRepository.ReplayAll();

            testRunner.Given("sample step without param");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldDistinguishByTableParam_CallWithTable()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            var table = new Table("h1");
            bindingInstance.Expect(b => b.DistinguishByTableParam2(table));

            MockRepository.ReplayAll();

            testRunner.Given("Distinguish by table param", null, table);

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldDistinguishByTableParam_CallWithoutTable()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            bindingInstance.Expect(b => b.DistinguishByTableParam1());

            MockRepository.ReplayAll();

            testRunner.Given("Distinguish by table param");

            Assert.AreEqual(TestStatus.OK, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldRaiseAmbiguousIfMultipleMatch()
        {
            StepExecutionTestsAmbiguousBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            MockRepository.ReplayAll();

            testRunner.Given("sample step without param");

            Assert.AreEqual(TestStatus.BindingError, GetLastTestStatus());
            MockRepository.VerifyAll();
        }

        [Test]
        public void ShouldRaiseBindingErrorIfWrongParamNumber()
        {
            StepExecutionTestsBindings bindingInstance;
            TestRunner testRunner = GetTestRunnerFor(out bindingInstance);

            MockRepository.ReplayAll();

            testRunner.Given("sample step with wrong param number");

            Assert.AreEqual(TestStatus.BindingError, GetLastTestStatus());
            MockRepository.VerifyAll();
        }
    }
}