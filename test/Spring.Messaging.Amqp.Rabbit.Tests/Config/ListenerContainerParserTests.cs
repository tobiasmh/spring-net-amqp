﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using AopAlliance.Aop;

using NUnit.Framework;

using Spring.Core.IO;
using Spring.Messaging.Amqp.Core;
using Spring.Messaging.Amqp.Rabbit.Config;
using Spring.Messaging.Amqp.Rabbit.Connection;
using Spring.Messaging.Amqp.Rabbit.Core;
using Spring.Messaging.Amqp.Rabbit.Listener;
using Spring.Messaging.Amqp.Rabbit.Listener.Adapter;
using Spring.Messaging.Amqp.Rabbit.Tests.Test;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Messaging.Amqp.Rabbit.Tests.Config
{
    /// <summary>
    /// ListenerContainerParser Tests
    /// </summary>
    [TestFixture]
    [Category(TestCategory.Unit)]
    [Ignore]
    public class ListenerContainerParserTests
    {
        private XmlObjectFactory beanFactory;

        [TestFixtureSetUp]
        public void Setup()
        {
            NamespaceParserRegistry.RegisterParser(typeof(RabbitNamespaceHandler));
            var resourceName = @"assembly://Spring.Messaging.Amqp.Rabbit.Tests/Spring.Messaging.Amqp.Rabbit.Tests.Config/" + typeof(ListenerContainerParserTests).Name + "-context.xml";
            var resource = new AssemblyResource(resourceName);
            beanFactory = new XmlObjectFactory(resource);
            // ((IConfigurableObjectFactory)beanFactory).setBeanExpressionResolver(new StandardObjectExpressionResolver());
        }

        [Test]
        public void testParseWithQueueNames()
        {
		    var container = beanFactory.GetObject<SimpleMessageListenerContainer>("container1");
		    Assert.AreEqual(AcknowledgeModeUtils.AcknowledgeMode.MANUAL, container.AcknowledgeMode);
		    Assert.AreEqual(beanFactory.GetObject<IConnectionFactory>(), container.ConnectionFactory);
		    Assert.AreEqual(typeof(MessageListenerAdapter), container.MessageListener.GetType());
		    var listenerAccessor = container.MessageListener;
            Assert.AreEqual(beanFactory.GetObject<TestObject>(), ((MessageListenerAdapter)listenerAccessor).HandlerObject);
            
            Assert.AreEqual("Handle", ((MessageListenerAdapter)listenerAccessor).DefaultListenerMethod); 
		    var queue = beanFactory.GetObject<Queue>("bar");
            var queueNamesForVerification = "[";
            foreach(var queueName in container.QueueNames)
            {
                queueNamesForVerification += queueNamesForVerification == "[" ? queueName : ", " + queueName;
            }
            queueNamesForVerification += "]";
		    Assert.AreEqual("[foo, "+ queue.Name + "]", queueNamesForVerification);
	    }

        [Test]
        [Ignore("Need to determine how to allow injection of IAdvice[] via config...")]
        public void testParseWithAdviceChain()
        {
		    var container = beanFactory.GetObject<SimpleMessageListenerContainer>("container3");
            var fields = typeof(SimpleMessageListenerContainer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var adviceChainField = typeof(SimpleMessageListenerContainer).GetField("adviceChain", BindingFlags.NonPublic | BindingFlags.Instance);

            var adviceChain = adviceChainField.GetValue(container);
		    Assert.IsNotNull(adviceChain);
		    Assert.AreEqual(3, ((IAdvice[]) adviceChain).Count());
	    }

        [Test]
        public void testParseWithDefaults()
        {
		    SimpleMessageListenerContainer container = beanFactory.GetObject<SimpleMessageListenerContainer>("container4");
            var concurrentConsumersField = typeof(SimpleMessageListenerContainer).GetField("concurrentConsumers", BindingFlags.NonPublic | BindingFlags.Instance);

            var concurrentConsumers = concurrentConsumersField.GetValue(container);
		    Assert.AreEqual(1, concurrentConsumers);
	    }
    }
}