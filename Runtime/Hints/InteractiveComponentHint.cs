// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey
{
    public class InteractiveComponentHint : MonoBehaviour
    {
        private readonly int refreshPerFrame = 100;
        private static readonly Color notReallyInteractiveColor = Color.red;
        private static readonly Color reallyInteractiveColor = Color.blue;
        private readonly List<Vector3> notReallyInteractives = new List<Vector3>();
        private readonly List<Vector3> reallyInteractives = new List<Vector3>();
        
        
        private void OnDrawGizmos()
        {
            if (Time.frameCount % refreshPerFrame == 0)
            {
                UpdateInteractives();
            }

            foreach (var pos in notReallyInteractives)
            {
                Gizmos.color = notReallyInteractiveColor;
                Gizmos.DrawWireSphere(pos, 0.1f);
            }

            foreach (var pos in reallyInteractives)
            {
                Gizmos.color = reallyInteractiveColor;
                Gizmos.DrawWireSphere(pos, 0.1f);
            }
        }


        private void UpdateInteractives()
        {
            notReallyInteractives.Clear();
            reallyInteractives.Clear();
            
            var components = InteractiveComponentCollector.FindInteractiveComponents(false);
            foreach (var component in components)
            {
                var dst = component.IsReallyInteractiveFromUser() ? reallyInteractives : notReallyInteractives;
                dst.Add(component.transform.position);
            }
        }
    }
}
