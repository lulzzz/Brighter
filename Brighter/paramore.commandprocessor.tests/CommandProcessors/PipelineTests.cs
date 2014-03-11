using System.ComponentModel;
using System.Linq;
using Machine.Specifications;
using TinyIoC;
using paramore.commandprocessor.ioccontainers.IoCContainers;
using paramore.commandprocessor.tests.CommandProcessors.TestDoubles;

namespace paramore.commandprocessor.tests.CommandProcessors
{
    [Subject(typeof(PipelineBuilder<>))]
    public class When_Finding_A_Handler_For_A_Command
    {
        private static PipelineBuilder<MyCommand> Pipeline_Builder;
        private static IHandleRequests<MyCommand> Pipeline;
        private static IAdaptAnInversionOfControlContainer Container;

        Establish context = () =>
        {
            Container = new TinyIoCAdapter(new TinyIoCContainer());
            Container.Register<IHandleRequests<MyCommand>, MyCommandHandler>().AsMultiInstance();            

            Pipeline_Builder = new PipelineBuilder<MyCommand>(Container); 
        };

        Because of = () => Pipeline = Pipeline_Builder.Build(new RequestContext(Container)).First();

        It should_return_the_my_command_handler_as_the_implicit_handler = () => Pipeline.ShouldBeAssignableTo(typeof(MyCommandHandler));
        It should_be_the_only_element_in_the_chain = () => TracePipeline().ToString().ShouldEqual("MyCommandHandler|");

        private static PipelineTracer TracePipeline()
        {
            var pipelineTracer = new PipelineTracer();
            Pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }
    }

    [Subject(typeof(PipelineBuilder<>))]
    public class When_Finding_A_Hander_That_Has_Dependencies
    {
        private static PipelineBuilder<MyCommand> Pipeline_Builder;
        private static IHandleRequests<MyCommand> Pipeline;
        private static IAdaptAnInversionOfControlContainer Container;

        Establish context = () =>
        {
            Container = new TinyIoCAdapter(new TinyIoCContainer());

            Container.Register<IUnitOfWork, FakeSession>().AsMultiInstance();
            Container.Register<IRepository<MyAggregate>, FakeRepository<MyAggregate>>().AsMultiInstance();
            Container.Register<IHandleRequests<MyCommand>, MyDependentCommandHandler>().AsMultiInstance();

            Pipeline_Builder = new PipelineBuilder<MyCommand>(Container);
        };

        Because of = () => Pipeline = Pipeline_Builder.Build(new RequestContext(Container)).First();

        It should_return_the_command_handler_as_the_implicit_handler = () => Pipeline.ShouldBeAssignableTo(typeof(MyDependentCommandHandler));
        It should_be_the_only_element_in_the_chain = () => TracePipeline().ToString().ShouldEqual("MyDependentCommandHandler|");

        private static PipelineTracer TracePipeline()
        {
            var pipelineTracer = new PipelineTracer();
            Pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }      
    }

    [Subject(typeof(PipelineBuilder<>))]
    public class When_A_Handler_Is_Part_of_A_Pipeline
    {
        private static PipelineBuilder<MyCommand> Pipeline_Builder;
        private static IHandleRequests<MyCommand> Pipeline;
        private static IAdaptAnInversionOfControlContainer Container;

        Establish context = () =>
        {
            Container = new TinyIoCAdapter(new TinyIoCContainer());
            Container.Register<IHandleRequests<MyCommand>, MyImplicitHandler>().AsMultiInstance();

            Pipeline_Builder = new PipelineBuilder<MyCommand>(Container);
        };

        Because of = () => Pipeline = Pipeline_Builder.Build(new RequestContext(Container)).First();

        It should_include_my_command_handler_filter_in_the_chain = () => TracePipeline().ToString().Contains("MyImplicitHandler").ShouldBeTrue();
        It should_include_my_logging_handler_in_the_chain = () => TracePipeline().ToString().Contains("MyLoggingHandler").ShouldBeTrue();

        private static PipelineTracer TracePipeline()
        {
            var pipelineTracer = new PipelineTracer();
            Pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }
    }

    [Subject(typeof(PipelineBuilder<>))]
    public class When_Building_A_Pipeline_Preserve_The_Order
    {
        private static PipelineBuilder<MyCommand> Pipeline_Builder;
        private static IHandleRequests<MyCommand> Pipeline;
        private static IAdaptAnInversionOfControlContainer Container;

        Establish context = () =>
        {
            Container = new TinyIoCAdapter(new TinyIoCContainer());
            Container.Register<IHandleRequests<MyCommand>, MyDoubleDecoratedHandler>().AsMultiInstance();
            Pipeline_Builder = new PipelineBuilder<MyCommand>(Container);
        };

        Because of = () => Pipeline = Pipeline_Builder.Build(new RequestContext(Container)).First();

        It should_add_handlers_in_the_correct_sequence_into_the_chain = () => PipelineTracer().ToString().ShouldEqual("MyLoggingHandler`1|MyValidationHandler`1|MyDoubleDecoratedHandler|");

        private static PipelineTracer PipelineTracer()
        {
            var pipelineTracer = new PipelineTracer();
            Pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }
    }

    [Subject(typeof(PipelineBuilder<>))]
    public class When_Building_A_Pipeline_Allow_Pre_And_Post_Tasks
    {
        private static PipelineBuilder<MyCommand> Pipeline_Builder;
        private static IHandleRequests<MyCommand> Pipeline;
        private static IAdaptAnInversionOfControlContainer Container;

        Establish context = () =>
        {
            Container = new TinyIoCAdapter(new TinyIoCContainer());
            Container.Register<IHandleRequests<MyCommand>, MyPreAndPostDecoratedHandler>().AsMultiInstance();
            Pipeline_Builder = new PipelineBuilder<MyCommand>(Container);
        };

        Because of = () => Pipeline = Pipeline_Builder.Build(new RequestContext(Container)).First();

        It should_add_handlers_in_the_correct_sequence_into_the_chain = () => TraceFilters().ToString().ShouldEqual("MyValidationHandler`1|MyPreAndPostDecoratedHandler|MyLoggingHandler`1|");

        private static PipelineTracer TraceFilters()
        {
            var pipelineTracer = new PipelineTracer();
            Pipeline.DescribePath(pipelineTracer);
            return pipelineTracer;
        }
    }
    
}
