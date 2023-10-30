﻿// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Random;
using UnityEngine;
using UnityEngine.Assertions;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Reference implementation of the monkey testing.
    ///
    /// - Can specific lifetime and delay time
    /// - Can specific random number generator
    /// - Can lottery and operation only clickable objects
    /// - Can throw assert exception if Interactive component not found in 5 sec
    /// </summary>
    public static class Monkey
    {
        /// <summary>
        /// Run monkey testing by repeating to call <c cref="RunStep" /> and wait.
        /// </summary>
        /// <param name="config">Run configuration for monkey testing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async UniTask Run(MonkeyConfig config, CancellationToken cancellationToken = default)
        {
            var endTime = config.Lifetime == TimeSpan.MaxValue
                ? TimeSpan.MaxValue.TotalSeconds
                : config.Lifetime.Add(TimeSpan.FromSeconds(Time.time)).TotalSeconds;
            var lastOperationTime = Time.time;

            config.Logger.Log($"Using {config.Random}");

            while (Time.time < endTime)
            {
                var didAct = await RunStep(config, cancellationToken);
                if (didAct)
                {
                    lastOperationTime = Time.time;
                }
                else if (config.SecondsToErrorForNoInteractiveComponent > 0)
                {
                    Assert.IsTrue((Time.time - lastOperationTime) < config.SecondsToErrorForNoInteractiveComponent,
                        $"Interactive component not found in {config.SecondsToErrorForNoInteractiveComponent} seconds");
                }

                await UniTask.Delay(config.DelayMillis, DelayType.DeltaTime, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Run a step of monkey testing.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async UniTask<bool> RunStep(MonkeyConfig config, CancellationToken cancellationToken = default)
        {
            var components = InteractiveComponentCollector
                .FindInteractiveComponents()
                .ToList();
            var component = Lottery(ref components, config.Random, config.ScreenPointStrategy);
            if (component == null)
            {
                return false;
            }

            await DoOperation(component, config, cancellationToken);
            return true;
        }

        internal static InteractiveComponent Lottery(ref List<InteractiveComponent> components, IRandom random, Func<GameObject, Vector2> screenPointStrategy)
        {
            if (components == null || components.Count == 0)
            {
                return null;
            }

            while (true)
            {
                if (components.Count == 0)
                {
                    return null;
                }

                var next = components[random.Next(components.Count)];
                if (next.IsReallyInteractiveFromUser(screenPointStrategy) && GetCanOperations(next).Any())
                {
                    return next;
                }

                components.Remove(next);
            }
        }

        private enum SupportOperation
        {
            Click,
            TouchAndHold,
        }

        private static IEnumerable<SupportOperation> GetCanOperations(InteractiveComponent component)
        {
            if (component.CanClick()) yield return SupportOperation.Click;
            if (component.CanTouchAndHold()) yield return SupportOperation.TouchAndHold;
        }

        internal static async UniTask DoOperation(InteractiveComponent component, MonkeyConfig config,
            CancellationToken cancellationToken = default)
        {
            var operations = GetCanOperations(component).ToArray();
            var operation = operations[config.Random.Next(operations.Length)];
            config.Logger.Log($"Do operation {component.gameObject.name} {operation.ToString()}");
            switch (operation)
            {
                case SupportOperation.Click:
                    component.Click(config.ScreenPointStrategy);
                    break;
                case SupportOperation.TouchAndHold:
                    await component.TouchAndHold(config.ScreenPointStrategy, config.TouchAndHoldDelayMillis, cancellationToken);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
