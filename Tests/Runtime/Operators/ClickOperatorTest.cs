﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class ClickOperatorTest
    {
        [SetUp]
        public async Task SetUp()
        {
            await EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
        }

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        public void Click(string targetName, string expectedMessage)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanClick(), Is.True);
            target.Click();
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerClickHandler", "OnPointerClick")]
        [TestCase("UsingPointerClickEventTrigger", "ReceivePointerClick")]
        public void Tap(string targetName, string expectedMessage) // Same as Click
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanTap(), Is.True);
            target.Tap();
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
        }

        [TestCase("UsingOnPointerDownUpHandler")]
        [TestCase("UsingPointerDownUpEventTrigger")]
        public void CanNotClick(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanClick(), Is.False);
        }
    }
}
