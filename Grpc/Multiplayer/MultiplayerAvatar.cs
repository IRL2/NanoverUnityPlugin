using System.Collections.Generic;
using System.Linq;
using Narupa.Core;
using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Grpc.Multiplayer
{
    /// <summary>
    /// A representation of a multiplayer avatar, which is shared with other users
    /// using the shared state dictionary under the 'avatar.{playerid}' key.
    /// </summary>
    public class MultiplayerAvatar
    {
        public const string HeadsetName = "headset";
        public const string LeftHandName = "hand.left";
        public const string RightHandName = "hand.right";

        private const string FieldID = "playerid";
        private const string FieldComponents = "components";

        /// <summary>
        /// Player ID associated with this avatar.
        /// </summary>
        public string ID;

        /// <summary>
        /// List of <see cref="AvatarComponent"/> such as headsets and controllers
        /// </summary>
        public List<Component> Components = new List<Component>();

        /// <summary>
        /// Set the three transformations used by this avatar.
        /// </summary>
        public void SetTransformations(Transformation? headsetTransformation,
                                       Transformation? leftHandTransformation,
                                       Transformation? rightHandTransformation)
        {
            Components.Clear();
            if (headsetTransformation.HasValue)
            {
                Components.Add(new Component(HeadsetName,
                                             headsetTransformation.Value.Position,
                                             headsetTransformation.Value.Rotation));
            }

            if (leftHandTransformation.HasValue)
            {
                Components.Add(new Component(LeftHandName,
                                             leftHandTransformation.Value.Position,
                                             leftHandTransformation.Value.Rotation));
            }

            if (rightHandTransformation.HasValue)
            {
                Components.Add(new Component(RightHandName,
                                             rightHandTransformation.Value.Position,
                                             rightHandTransformation.Value.Rotation));
            }
        }

        /// <summary>
        /// Convert to a dictionary structure for serialisation.
        /// </summary>
        public Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
            {
                [FieldID] = ID,
                [FieldComponents] = Components.Select(c => c.ToData()).ToList()
            };
        }

        /// <summary>
        /// Deserialise from a dictionary structure.
        /// </summary>
        public void FromData(Dictionary<string, object> data)
        {
            if (data == null)
                return;
            if (data.TryGetValue<IReadOnlyList<object>>(FieldComponents, out var componentList))
            {
                Components.Clear();
                foreach (var componentData in componentList)
                {
                    var comp = new Component();
                    comp.FromData(componentData as Dictionary<string, object>);
                    Components.Add(comp);
                }
            }
        }
        
        /// <summary>
        /// A part of an avatar, such as a headset or controller.
        /// </summary>
        public class Component
        {
            /// <summary>
            /// The name of the component, which defines its type.
            /// </summary>
            public string Name;

            /// <summary>
            /// The position of the component.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The rotation of the component.
            /// </summary>
            public Quaternion Rotation;
        
            private const string FieldName = "name";
            private const string FieldPosition = "position";
            private const string FieldRotation = "rotation";

            public Component()
            {
            
            }

            public Component(string name, Vector3 position, Quaternion rotation)
            {
                Name = name;
                Position = position;
                Rotation = rotation;
            }

            /// <summary>
            /// The component as a <see cref="UnitScaleTransformation"/>
            /// </summary>
            public UnitScaleTransformation Transformation =>
                new UnitScaleTransformation(Position, Rotation);
        
            /// <summary>
            /// Convert to a dictionary structure for serialisation.
            /// </summary>
            public Dictionary<string, object> ToData()
            {
                return new Dictionary<string, object>()
                {
                    [FieldName] = Name,
                    [FieldPosition] = new List<object> {Position.x, Position.y, Position.z},
                    [FieldRotation] = new List<object> {Rotation.x, Rotation.y, Rotation.z, Rotation.w},
                };
            }

            /// <summary>
            /// Deserialise from a dictionary structure.
            /// </summary>
            public void FromData(Dictionary<string, object> data)
            {
                if (data == null)
                    return;
                if (data.TryGetValue<string>(FieldName, out var name))
                    Name = name;
                if (data.TryGetValue<IReadOnlyList<object>>(FieldPosition, out var pos))
                    Position = pos.GetVector3();
                if (data.TryGetValue<IReadOnlyList<object>>(FieldRotation, out var rot))
                    Rotation = rot.GetQuaternion();
            }
        }
    }
}
