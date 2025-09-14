using AdvShields;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo
{
    public class DomeShieldInstanceManager : MonoBehaviour
    {
        // A list of all active shield blocks on this specific craft.
        private readonly List<AdvShieldProjector> _shields = new List<AdvShieldProjector>();

        /// <summary>
        /// Safe way to retreive the number of shields present on craft
        /// </summary>
        public int ShieldCount => _shields.Count;

        /// <summary>
        /// Registers given shield to the the count
        /// </summary>
        /// <param name="projector"> The shield to add</param>
        public void RegisterShield(AdvShieldProjector projector)
        {
            if (!_shields.Contains(projector) && projector != null)
            {
                _shields.Add(projector);
            }
        }

        /// <summary>
        /// Removes given shield from count
        /// </summary>
        /// <param name="projector"> The shield to remove</param>
        public void UnregisterShield(AdvShieldProjector projector)
        {
            //This list won't contain a null value, so we don't need to specifically check for it.
            _shields.Remove(projector);

        }
    }
}
